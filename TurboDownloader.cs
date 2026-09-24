using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace TurboDownloader
{
    public class DownloadJob
    {
        public string id { get; set; }
        public string url { get; set; }
        public string status { get; set; } // STARTING, DOWNLOADING, MERGING, COMPLETED, FAILED, CANCELLED
        public double progressPercent { get; set; }
        public double speedMBps { get; set; }
        public long fileSizeBytes { get; set; }
        public long downloadedBytes { get; set; }
        public string title { get; set; }
        public string requestedFormat { get; set; }
        public string downloadUrl { get; set; }
        public string filePath { get; set; }
        public string errorMessage { get; set; }
        public string errorCode { get; set; }

        [ScriptIgnore]
        public Process process { get; set; }
    }

    public class VideoFormatDto
    {
        public string formatId { get; set; }
        public string resolution { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public double fps { get; set; }
        public string vcodec { get; set; }
        public string container { get; set; }
        public string filesizeFormatted { get; set; }
        public long estimatedSizeBytes { get; set; }
        public bool hdr { get; set; }
    }

    public class AudioFormatDto
    {
        public string formatId { get; set; }
        public string label { get; set; }
        public int bitrateKbps { get; set; }
        public string codec { get; set; }
        public string container { get; set; }
        public string filesizeFormatted { get; set; }
        public long estimatedSizeBytes { get; set; }
    }

    static class Program
    {
        private static int port = 4000;
        private static HttpListener listener;
        private static string appDir;
        private static string saveDir;
        private static string nodePath = @"C:\Program Files\nodejs\node.exe";
        private static JavaScriptSerializer json = new JavaScriptSerializer();
        private static ConcurrentDictionary<string, DownloadJob> jobs = new ConcurrentDictionary<string, DownloadJob>();

        [STAThread]
        static void Main()
        {
            try
            {
                try
                {
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072 | (SecurityProtocolType)768 | SecurityProtocolType.Tls;
                    ServicePointManager.DefaultConnectionLimit = 512;
                    ServicePointManager.Expect100Continue = false;
                    ServicePointManager.UseNagleAlgorithm = false;
                }
                catch { }

                appDir = AppDomain.CurrentDomain.BaseDirectory;
                saveDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                if (!Directory.Exists(saveDir)) saveDir = appDir;

                // Start HTTP Server
                StartHttpServer();

                // Launch Desktop Application Window
                LaunchNativeWindow();

                // Keep Server Running
                while (true)
                {
                    Thread.Sleep(2000);
                }
            }
            catch (Exception ex)
            {
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash.log"), ex.ToString());
            }
        }

        private static void StartHttpServer()
        {
            listener = new HttpListener();
            string prefix = string.Format("http://127.0.0.1:{0}/", port);
            listener.Prefixes.Add(prefix);

            try
            {
                listener.Start();
            }
            catch (Exception)
            {
                port = 48291;
                listener = new HttpListener();
                listener.Prefixes.Add(string.Format("http://127.0.0.1:{0}/", port));
                listener.Start();
            }

            Task.Factory.StartNew(() =>
            {
                while (listener.IsListening)
                {
                    try
                    {
                        HttpListenerContext ctx = listener.GetContext();
                        Task.Factory.StartNew(() => HandleRequest(ctx));
                    }
                    catch { }
                }
            });
        }

        private static void LaunchNativeWindow()
        {
            string edgePath1 = @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe";
            string edgePath2 = @"C:\Program Files\Microsoft\Edge\Application\msedge.exe";
            string chromePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe";

            string targetBrowser = null;
            if (File.Exists(edgePath1)) targetBrowser = edgePath1;
            else if (File.Exists(edgePath2)) targetBrowser = edgePath2;
            else if (File.Exists(chromePath)) targetBrowser = chromePath;

            string appUrl = string.Format("http://127.0.0.1:{0}/", port);

            try
            {
                if (targetBrowser != null)
                {
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = targetBrowser;
                    psi.Arguments = string.Format("--app=\"{0}\" --window-size=1120,820", appUrl);
                    Process.Start(psi);
                }
                else
                {
                    Process.Start(appUrl);
                }
            }
            catch
            {
                Process.Start(appUrl);
            }
        }

        private static void HandleRequest(HttpListenerContext ctx)
        {
            HttpListenerRequest req = ctx.Request;
            HttpListenerResponse res = ctx.Response;

            res.Headers.Add("Access-Control-Allow-Origin", "*");
            res.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            res.Headers.Add("Access-Control-Allow-Headers", "Content-Type");

            if (req.HttpMethod == "OPTIONS")
            {
                res.StatusCode = 200;
                res.Close();
                return;
            }

            string rawUrl = req.Url.AbsolutePath;

            try
            {
                // Serve Frontend
                if (rawUrl == "/" || rawUrl == "/index.html")
                {
                    ServeFile(res, Path.Combine(appDir, "web", "index.html"), "text/html; charset=utf-8");
                }
                else if (rawUrl == "/styles.css" || rawUrl == "/style.css")
                {
                    ServeFile(res, Path.Combine(appDir, "web", "styles.css"), "text/css; charset=utf-8");
                }
                else if (rawUrl == "/app.js")
                {
                    ServeFile(res, Path.Combine(appDir, "web", "app.js"), "application/javascript; charset=utf-8");
                }
                // FlowDown / PEAK/8K API endpoints
                else if (rawUrl.StartsWith("/api/v1/media/analyze") && req.HttpMethod == "POST")
                {
                    string body = ReadBody(req);
                    HandleMediaAnalyze(res, body);
                }
                else if (rawUrl.StartsWith("/api/v1/downloads") && req.HttpMethod == "POST" && !rawUrl.Contains("/cancel") && !rawUrl.Contains("/open"))
                {
                    string body = ReadBody(req);
                    HandleCreateDownload(res, body);
                }
                else if (rawUrl.StartsWith("/api/v1/downloads/") && req.HttpMethod == "GET")
                {
                    // Check if requesting file or job status
                    if (rawUrl.EndsWith("/file"))
                    {
                        string id = ExtractIdFromUrl(rawUrl, "/api/v1/downloads/", "/file");
                        HandleDownloadFile(res, id);
                    }
                    else
                    {
                        string id = rawUrl.Substring("/api/v1/downloads/".Length).Trim('/');
                        HandleGetDownloadStatus(res, id);
                    }
                }
                else if (rawUrl.StartsWith("/api/v1/downloads/") && rawUrl.EndsWith("/cancel") && req.HttpMethod == "POST")
                {
                    string id = ExtractIdFromUrl(rawUrl, "/api/v1/downloads/", "/cancel");
                    HandleCancelDownload(res, id);
                }
                else if (rawUrl.StartsWith("/api/v1/downloads/") && rawUrl.EndsWith("/open") && req.HttpMethod == "POST")
                {
                    string id = ExtractIdFromUrl(rawUrl, "/api/v1/downloads/", "/open");
                    HandleOpenCompleted(res, id, req.QueryString["target"]);
                }
                else
                {
                    // Check if file exists in web folder
                    string localPath = Path.Combine(appDir, "web", rawUrl.TrimStart('/'));
                    if (File.Exists(localPath))
                    {
                        ServeFile(res, localPath, GetMimeType(localPath));
                    }
                    else
                    {
                        res.StatusCode = 404;
                        res.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                try
                {
                    res.StatusCode = 500;
                    byte[] b = Encoding.UTF8.GetBytes(json.Serialize(new Dictionary<string, object> {
                        { "success", false },
                        { "message", ex.Message }
                    }));
                    res.ContentType = "application/json";
                    res.OutputStream.Write(b, 0, b.Length);
                    res.Close();
                }
                catch { }
            }
        }

        private static string ReadBody(HttpListenerRequest req)
        {
            using (StreamReader sr = new StreamReader(req.InputStream, req.ContentEncoding))
            {
                return sr.ReadToEnd();
            }
        }

        private static string ExtractIdFromUrl(string url, string prefix, string suffix)
        {
            string s = url.Substring(prefix.Length);
            if (s.EndsWith(suffix)) s = s.Substring(0, s.Length - suffix.Length);
            return s.Trim('/');
        }

        private static void ServeFile(HttpListenerResponse res, string path, string contentType)
        {
            if (File.Exists(path))
            {
                byte[] data = File.ReadAllBytes(path);
                res.ContentType = contentType;
                res.ContentLength64 = data.Length;
                res.OutputStream.Write(data, 0, data.Length);
                res.Close();
            }
            else
            {
                res.StatusCode = 404;
                res.Close();
            }
        }

        private static string GetMimeType(string path)
        {
            string ext = Path.GetExtension(path).ToLower();
            switch (ext)
            {
                case ".html": return "text/html; charset=utf-8";
                case ".css": return "text/css; charset=utf-8";
                case ".js": return "application/javascript; charset=utf-8";
                case ".json": return "application/json; charset=utf-8";
                case ".svg": return "image/svg+xml";
                case ".png": return "image/png";
                case ".jpg": case ".jpeg": return "image/jpeg";
                default: return "application/octet-stream";
            }
        }

        private static void SendJson(HttpListenerResponse res, object data)
        {
            string s = json.Serialize(data);
            byte[] bytes = Encoding.UTF8.GetBytes(s);
            res.ContentType = "application/json; charset=utf-8";
            res.ContentLength64 = bytes.Length;
            res.OutputStream.Write(bytes, 0, bytes.Length);
            res.Close();
        }

        // ==========================================
        //  API: /api/v1/media/analyze
        // ==========================================
        private static void HandleMediaAnalyze(HttpListenerResponse res, string body)
        {
            Dictionary<string, object> reqObj = json.Deserialize<Dictionary<string, object>>(body);
            string url = reqObj.ContainsKey("url") ? Convert.ToString(reqObj["url"]) : "";

            if (string.IsNullOrEmpty(url))
            {
                res.StatusCode = 400;
                SendJson(res, new { success = false, message = "URL is required" });
                return;
            }

            string ytdlp = Path.Combine(appDir, "yt-dlp.exe");
            if (!File.Exists(ytdlp)) ytdlp = "yt-dlp.exe";

            string jsRuntimeArg = File.Exists(nodePath) ? string.Format("--js-runtimes node:\"{0}\"", nodePath) : "";

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = ytdlp;
            psi.Arguments = string.Format("{0} --dump-json --no-playlist \"{1}\"", jsRuntimeArg, url);
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.StandardOutputEncoding = Encoding.UTF8;

            try
            {
                using (Process p = Process.Start(psi))
                {
                    string output = p.StandardOutput.ReadToEnd();
                    string err = p.StandardError.ReadToEnd();
                    p.WaitForExit(20000);

                    if (string.IsNullOrEmpty(output))
                    {
                        res.StatusCode = 400;
                        SendJson(res, new { success = false, message = "Could not analyze link: " + err });
                        return;
                    }

                    Dictionary<string, object> info = json.Deserialize<Dictionary<string, object>>(output);

                    string title = info.ContainsKey("title") ? Convert.ToString(info["title"]) : "Video";
                    string creator = info.ContainsKey("uploader") ? Convert.ToString(info["uploader"]) : (info.ContainsKey("channel") ? Convert.ToString(info["channel"]) : "");
                    string thumbnailUrl = info.ContainsKey("thumbnail") ? Convert.ToString(info["thumbnail"]) : "";
                    double durationSeconds = 0;
                    if (info.ContainsKey("duration") && info["duration"] != null)
                        double.TryParse(Convert.ToString(info["duration"]), out durationSeconds);

                    string platformMediaId = info.ContainsKey("id") ? Convert.ToString(info["id"]) : "";

                    // Parse Formats
                    List<VideoFormatDto> videoFormats = new List<VideoFormatDto>();
                    List<AudioFormatDto> audioFormats = new List<AudioFormatDto>();

                    if (info.ContainsKey("formats") && info["formats"] is System.Collections.ArrayList)
                    {
                        System.Collections.ArrayList rawFormats = (System.Collections.ArrayList)info["formats"];
                        Dictionary<string, VideoFormatDto> resolutionMap = new Dictionary<string, VideoFormatDto>();

                        foreach (object item in rawFormats)
                        {
                            if (!(item is Dictionary<string, object>)) continue;
                            Dictionary<string, object> f = (Dictionary<string, object>)item;

                            string vcodec = f.ContainsKey("vcodec") ? Convert.ToString(f["vcodec"]) : "none";
                            string acodec = f.ContainsKey("acodec") ? Convert.ToString(f["acodec"]) : "none";
                            string formatId = f.ContainsKey("format_id") ? Convert.ToString(f["format_id"]) : "";
                            string ext = f.ContainsKey("ext") ? Convert.ToString(f["ext"]) : "mp4";

                            long filesize = 0;
                            if (f.ContainsKey("filesize") && f["filesize"] != null) long.TryParse(Convert.ToString(f["filesize"]), out filesize);
                            else if (f.ContainsKey("filesize_approx") && f["filesize_approx"] != null) long.TryParse(Convert.ToString(f["filesize_approx"]), out filesize);

                            int width = 0;
                            if (f.ContainsKey("width") && f["width"] != null) int.TryParse(Convert.ToString(f["width"]), out width);

                            int height = 0;
                            if (f.ContainsKey("height") && f["height"] != null) int.TryParse(Convert.ToString(f["height"]), out height);

                            double fps = 0;
                            if (f.ContainsKey("fps") && f["fps"] != null) double.TryParse(Convert.ToString(f["fps"]), out fps);

                            // Video Streams
                            if (vcodec != "none" && !string.IsNullOrEmpty(vcodec))
                            {
                                // Infer height if 0 (e.g. Facebook "hd" / "sd")
                                if (height == 0)
                                {
                                    if (formatId == "hd") height = 1080;
                                    else if (formatId == "sd") height = 480;
                                }

                                string resKey = height > 0 ? (height + "p") : formatId;
                                string resLabel = resKey;
                                if (height == 4320) resLabel = "4320p / 8K";
                                else if (height == 2160) resLabel = "2160p / 4K";
                                else if (height == 1440) resLabel = "1440p / 2K";
                                else if (height == 1080) resLabel = "1080p / Full HD";
                                else if (height == 720) resLabel = "720p / HD";

                                bool isHdr = false;
                                if (f.ContainsKey("dynamic_range")) isHdr = Convert.ToString(f["dynamic_range"]).IndexOf("HDR", StringComparison.OrdinalIgnoreCase) >= 0;

                                VideoFormatDto vdto = new VideoFormatDto();
                                vdto.formatId = formatId;
                                vdto.resolution = resLabel;
                                vdto.width = width;
                                vdto.height = height;
                                vdto.fps = fps;
                                vdto.vcodec = vcodec;
                                vdto.container = ext;
                                vdto.filesizeFormatted = filesize > 0 ? FormatBytes(filesize) : null;
                                vdto.estimatedSizeBytes = filesize;
                                vdto.hdr = isHdr;

                                // Keep best (prefer MP4 or higher filesize for each resolution tier)
                                if (!resolutionMap.ContainsKey(resKey) || (filesize > resolutionMap[resKey].estimatedSizeBytes))
                                {
                                    resolutionMap[resKey] = vdto;
                                }
                            }
                            // Audio Streams
                            else if (acodec != "none" && !string.IsNullOrEmpty(acodec) && (vcodec == "none" || string.IsNullOrEmpty(vcodec)))
                            {
                                double abr = 0;
                                if (f.ContainsKey("abr") && f["abr"] != null) double.TryParse(Convert.ToString(f["abr"]), out abr);

                                AudioFormatDto adto = new AudioFormatDto();
                                adto.formatId = formatId;
                                adto.bitrateKbps = (int)Math.Round(abr > 0 ? abr : 128);
                                adto.label = string.Format("{0} kbps ({1})", adto.bitrateKbps, ext.ToUpper());
                                adto.codec = acodec;
                                adto.container = ext;
                                adto.filesizeFormatted = filesize > 0 ? FormatBytes(filesize) : null;
                                adto.estimatedSizeBytes = filesize;

                                audioFormats.Add(adto);
                            }
                        }

                        // Sort video formats descending (8K -> 4K -> 2K -> 1080p -> 720p...)
                        videoFormats = new List<VideoFormatDto>(resolutionMap.Values);
                        videoFormats.Sort((a, b) => b.height.CompareTo(a.height));

                        // Sort audio formats descending
                        audioFormats.Sort((a, b) => b.bitrateKbps.CompareTo(a.bitrateKbps));
                    }

                    // Always ensure Best format exists
                    if (videoFormats.Count == 0)
                    {
                        VideoFormatDto bestDto = new VideoFormatDto();
                        bestDto.formatId = "best";
                        bestDto.resolution = "Best Quality";
                        bestDto.container = "mp4";
                        videoFormats.Add(bestDto);
                    }

                    if (audioFormats.Count == 0)
                    {
                        AudioFormatDto bestAud = new AudioFormatDto();
                        bestAud.formatId = "bestaudio";
                        bestAud.label = "Best Audio (320 kbps)";
                        bestAud.bitrateKbps = 320;
                        bestAud.container = "mp3";
                        audioFormats.Add(bestAud);
                    }

                    SendJson(res, new {
                        success = true,
                        data = new {
                            title = title,
                            creator = creator,
                            thumbnailUrl = thumbnailUrl,
                            durationSeconds = durationSeconds,
                            platformMediaId = platformMediaId,
                            videoFormats = videoFormats,
                            audioFormats = audioFormats
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                res.StatusCode = 500;
                SendJson(res, new { success = false, message = ex.Message });
            }
        }

        // ==========================================
        //  API: /api/v1/downloads
        // ==========================================
        private static void HandleCreateDownload(HttpListenerResponse res, string body)
        {
            Dictionary<string, object> reqObj = json.Deserialize<Dictionary<string, object>>(body);
            string url = reqObj.ContainsKey("url") ? Convert.ToString(reqObj["url"]) : "";
            string formatId = reqObj.ContainsKey("formatId") ? Convert.ToString(reqObj["formatId"]) : "best";
            string container = reqObj.ContainsKey("container") ? Convert.ToString(reqObj["container"]) : "mp4";
            string quality = reqObj.ContainsKey("quality") ? Convert.ToString(reqObj["quality"]) : "best";
            bool audioOnly = reqObj.ContainsKey("audioOnly") && Convert.ToBoolean(reqObj["audioOnly"]);

            string jobId = "job_" + Guid.NewGuid().ToString("N").Substring(0, 8);

            DownloadJob job = new DownloadJob();
            job.id = jobId;
            job.url = url;
            job.status = "STARTING";
            job.progressPercent = 0;
            job.requestedFormat = container;
            job.downloadUrl = string.Format("/api/v1/downloads/{0}/file", jobId);
            jobs[jobId] = job;

            Task.Factory.StartNew(() => RunDownloadProcess(job, formatId, container, quality, audioOnly));

            SendJson(res, new {
                success = true,
                data = new { id = jobId }
            });
        }

        private static void RunDownloadProcess(DownloadJob job, string formatId, string container, string quality, bool audioOnly)
        {
            string ytdlp = Path.Combine(appDir, "yt-dlp.exe");
            if (!File.Exists(ytdlp)) ytdlp = "yt-dlp.exe";

            string ffmpeg = Path.Combine(appDir, "ffmpeg.exe");

            string formatArg;
            string mergeArg = string.Format("--merge-output-format {0}", container);

            if (audioOnly)
            {
                formatArg = "-x --audio-format mp3 --audio-quality 0";
                mergeArg = "";
            }
            else if (!string.IsNullOrEmpty(formatId) && formatId != "best")
            {
                formatArg = string.Format("-f \"{0}+bestaudio/best\"", formatId);
            }
            else
            {
                formatArg = "-f \"bestvideo+bestaudio/best\"";
            }

            string outTemplate = Path.Combine(saveDir, "%(title)s.%(ext)s");
            string jsRuntimeArg = File.Exists(nodePath) ? string.Format("--js-runtimes node:\"{0}\"", nodePath) : "";

            string args = string.Format("{0} {1} {2} --ffmpeg-location \"{3}\" --newline --no-playlist --no-mtime --windows-filenames -o \"{4}\" \"{5}\"",
                jsRuntimeArg, formatArg, mergeArg, ffmpeg, outTemplate, job.url);

            job.status = "DOWNLOADING";

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = ytdlp;
                psi.Arguments = args;
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.StandardOutputEncoding = Encoding.UTF8;

                job.process = Process.Start(psi);

                string line;
                while ((line = job.process.StandardOutput.ReadLine()) != null)
                {
                    ParseDownloadOutput(job, line);
                }

                job.process.WaitForExit();

                if (job.status == "CANCELLED") return;

                if (job.process.ExitCode == 0)
                {
                    job.status = "COMPLETED";
                    job.progressPercent = 100;

                    if (!string.IsNullOrEmpty(job.filePath) && File.Exists(job.filePath))
                    {
                        job.fileSizeBytes = new FileInfo(job.filePath).Length;
                    }
                }
                else
                {
                    string err = job.process.StandardError.ReadToEnd();
                    job.status = "FAILED";
                    job.errorMessage = "Download error: " + err;
                    job.errorCode = "EXIT_" + job.process.ExitCode;
                }
            }
            catch (Exception ex)
            {
                job.status = "FAILED";
                job.errorMessage = ex.Message;
            }
            finally
            {
                job.process = null;
            }
        }

        private static void ParseDownloadOutput(DownloadJob job, string line)
        {
            if (string.IsNullOrEmpty(line)) return;

            // [download]  45.2% of ~ 150.00MiB at  12.50MiB/s ETA 00:06
            Match m = Regex.Match(line, @"\[download\]\s+(\d+\.?\d*)%\s+of\s+~?\s*(\d+\.?\d*\w+)(?:\s+at\s+(\d+\.?\d*\w+/s))?");
            if (m.Success)
            {
                double p;
                if (double.TryParse(m.Groups[1].Value, out p))
                {
                    job.progressPercent = p;
                }

                if (!string.IsNullOrEmpty(m.Groups[3].Value))
                {
                    job.speedMBps = ParseSpeedToMB(m.Groups[3].Value);
                }
            }
            else if (line.Contains("[Merger]") || line.Contains("Merging formats"))
            {
                job.status = "MERGING";
                job.progressPercent = 99.0;
            }
            else if (line.Contains("[download] Destination:"))
            {
                string fn = line.Substring(line.IndexOf("Destination:") + 12).Trim();
                job.filePath = fn;
                job.title = Path.GetFileNameWithoutExtension(fn);
            }
            else if (line.Contains("has already been downloaded"))
            {
                job.progressPercent = 100;
                job.status = "COMPLETED";
            }
        }

        private static double ParseSpeedToMB(string s)
        {
            try
            {
                Match m = Regex.Match(s, @"(\d+\.?\d*)\s*(\w+)/s");
                if (m.Success)
                {
                    double v = double.Parse(m.Groups[1].Value);
                    string u = m.Groups[2].Value.ToUpper();
                    if (u.Contains("KIB") || u.Contains("KB")) return v / 1024.0;
                    if (u.Contains("MIB") || u.Contains("MB")) return v;
                    if (u.Contains("GIB") || u.Contains("GB")) return v * 1024.0;
                }
            }
            catch { }
            return 0;
        }

        // ==========================================
        //  API: /api/v1/downloads/{id}
        // ==========================================
        private static void HandleGetDownloadStatus(HttpListenerResponse res, string id)
        {
            if (jobs.ContainsKey(id))
            {
                SendJson(res, new { success = true, data = jobs[id] });
            }
            else
            {
                res.StatusCode = 404;
                SendJson(res, new { success = false, message = "Job not found" });
            }
        }

        // ==========================================
        //  API: /api/v1/downloads/{id}/cancel
        // ==========================================
        private static void HandleCancelDownload(HttpListenerResponse res, string id)
        {
            if (jobs.ContainsKey(id))
            {
                DownloadJob job = jobs[id];
                job.status = "CANCELLED";
                if (job.process != null && !job.process.HasExited)
                {
                    try { job.process.Kill(); } catch { }
                }
                SendJson(res, new { success = true });
            }
            else
            {
                res.StatusCode = 404;
                SendJson(res, new { success = false, message = "Job not found" });
            }
        }

        // ==========================================
        //  API: /api/v1/downloads/{id}/file
        // ==========================================
        private static void HandleDownloadFile(HttpListenerResponse res, string id)
        {
            if (jobs.ContainsKey(id))
            {
                DownloadJob job = jobs[id];
                if (!string.IsNullOrEmpty(job.filePath) && File.Exists(job.filePath))
                {
                    string fn = Path.GetFileName(job.filePath);
                    res.Headers.Add("Content-Disposition", string.Format("attachment; filename=\"{0}\"", fn));
                    res.ContentType = GetMimeType(job.filePath);
                    using (FileStream fs = new FileStream(job.filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        res.ContentLength64 = fs.Length;
                        fs.CopyTo(res.OutputStream);
                    }
                    res.Close();
                    return;
                }
            }
            res.StatusCode = 404;
            res.Close();
        }

        // ==========================================
        //  API: /api/v1/downloads/{id}/open
        // ==========================================
        private static void HandleOpenCompleted(HttpListenerResponse res, string id, string target)
        {
            if (jobs.ContainsKey(id))
            {
                DownloadJob job = jobs[id];
                if (target == "file" && !string.IsNullOrEmpty(job.filePath) && File.Exists(job.filePath))
                {
                    try { Process.Start(job.filePath); } catch { }
                }
                else if (!string.IsNullOrEmpty(job.filePath) && File.Exists(job.filePath))
                {
                    try { Process.Start("explorer.exe", string.Format("/select,\"{0}\"", job.filePath)); } catch { }
                }
                else if (Directory.Exists(saveDir))
                {
                    try { Process.Start("explorer.exe", saveDir); } catch { }
                }
            }
            else if (Directory.Exists(saveDir))
            {
                try { Process.Start("explorer.exe", saveDir); } catch { }
            }

            SendJson(res, new { success = true });
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes <= 0) return "0 B";
            string[] suf = { "B", "KB", "MB", "GB", "TB" };
            int i = 0;
            double d = bytes;
            while (d >= 1024 && i < suf.Length - 1)
            {
                d /= 1024.0;
                i++;
            }
            return string.Format("{0:0.#} {1}", d, suf[i]);
        }
    }
}
