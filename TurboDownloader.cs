using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using System.Drawing;
using System.Net.Sockets;
using System.Linq;
using System.Runtime.InteropServices;
using System.IO.Compression;

[assembly: AssemblyTitle("180Hz Downloader")]
[assembly: AssemblyDescription("180Hz Studio High-Frame-Rate Desktop Media Utility")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyCompany("180Hz Studio")]
[assembly: AssemblyProduct("180Hz Downloader")]
[assembly: AssemblyCopyright("Copyright (c) 2026 180Hz Studio. All rights reserved.")]
[assembly: AssemblyTrademark("180Hz Studio")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: Guid("8f4e2a10-5c7b-4d93-9b1e-3a2c6e8f1d42")]
[assembly: AssemblyVersion("2.3.0.0")]
[assembly: AssemblyFileVersion("2.3.0.0")]

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
        public string formatId { get; set; }
        public bool audioOnly { get; set; }
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
        private static JavaScriptSerializer json = new JavaScriptSerializer();
        private static ConcurrentDictionary<string, DownloadJob> jobs = new ConcurrentDictionary<string, DownloadJob>();

        private static Mutex appMutex;
        private static string portFilePath;
        private static NotifyIcon trayIcon;

        private static string ytdlpPath;
        private static string ffmpegPath;
        private static bool isDownloadingDeps = false;
        private static string depStatusMessage = "Ready";

        private const string APP_VERSION = "2.3.0";
        private const int APP_VERSION_CODE = 230;
        private const string GITHUB_RAW_BASE_URL = "https://raw.githubusercontent.com/tanjeem180hz/180hz_video-downloader/main";
        private const string GITHUB_VERSION_URL = "https://raw.githubusercontent.com/tanjeem180hz/180hz_video-downloader/main/web/version.json";
        private const string GITHUB_ROOT_VERSION_URL = "https://raw.githubusercontent.com/tanjeem180hz/180hz_video-downloader/main/version.json";
        private const string GITHUB_PAGES_VERSION_URL = "https://tanjeem180hz.github.io/180hz_video-downloader/version.json";
        private const string GITHUB_SETUP_DOWNLOAD_URL = "https://raw.githubusercontent.com/tanjeem180hz/180hz_video-downloader/main/180Hz%20Downloader%20Setup.exe";
        private const string GITHUB_REPO_URL = "https://github.com/tanjeem180hz/180hz_video-downloader";

        private static bool isApplyingAppUpdate = false;
        private static int appUpdateProgress = 0;
        private static string appUpdateStage = "Up to date";
        private static string appUpdateError = "";
        private static bool hasRecentBrowserHealthPing = false;

        private static string FindSystemFFmpeg()
        {
            try
            {
                string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string wingetDir = Path.Combine(localAppData, @"Microsoft\WinGet");
                if (Directory.Exists(wingetDir))
                {
                    string[] files = Directory.GetFiles(wingetDir, "ffmpeg.exe", SearchOption.AllDirectories);
                    if (files.Length > 0 && File.Exists(files[0])) return files[0];
                }
                string envPath = Environment.GetEnvironmentVariable("PATH");
                if (!string.IsNullOrEmpty(envPath))
                {
                    foreach (string p in envPath.Split(';'))
                    {
                        if (string.IsNullOrEmpty(p)) continue;
                        string fp = Path.Combine(p.Trim(), "ffmpeg.exe");
                        if (File.Exists(fp)) return fp;
                    }
                }
            }
            catch { }
            return null;
        }

        private static bool DownloadFileWithProgress(string url, string targetPath, Action<int, long, long> progressCallback = null)
        {
            string tempFile = targetPath + ".tmp";
            try
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);

                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072 | (SecurityProtocolType)768 | SecurityProtocolType.Tls;
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) 180HzDownloader/2.3");
                    if (progressCallback != null)
                    {
                        wc.DownloadProgressChanged += (s, e) =>
                        {
                            try { progressCallback(e.ProgressPercentage, e.BytesReceived, e.TotalBytesToReceive); } catch { }
                        };
                    }
                    var syncEvent = new ManualResetEvent(false);
                    Exception downloadEx = null;
                    wc.DownloadFileCompleted += (s, e) =>
                    {
                        if (e.Error != null) downloadEx = e.Error;
                        syncEvent.Set();
                    };
                    wc.DownloadFileAsync(new Uri(url), tempFile);
                    syncEvent.WaitOne();
                    if (downloadEx != null) throw downloadEx;

                    if (File.Exists(tempFile) && new FileInfo(tempFile).Length > 100000)
                    {
                        if (File.Exists(targetPath)) File.Delete(targetPath);
                        File.Move(tempFile, targetPath);
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }

        private static bool DownloadFileWithFallback(string url, string targetPath)
        {
            return DownloadFileWithProgress(url, targetPath, null);
        }

        private static bool ExtractFFmpegFromZip(string zipPath, string targetExePath)
        {
            try
            {
                using (var archive = System.IO.Compression.ZipFile.OpenRead(zipPath))
                {
                    foreach (var entry in archive.Entries)
                    {
                        if (entry.Name.Equals("ffmpeg.exe", StringComparison.OrdinalIgnoreCase))
                        {
                            if (File.Exists(targetExePath)) File.Delete(targetExePath);
                            entry.ExtractToFile(targetExePath, true);
                            return true;
                        }
                    }
                }
            }
            catch { }

            // Secondary fallback extraction using Windows built-in tar.exe
            try
            {
                string tempDir = Path.Combine(Path.GetTempPath(), "180hz_ffmpeg_ext_" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(tempDir);
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "tar.exe",
                    Arguments = string.Format("-xf \"{0}\" -C \"{1}\"", zipPath, tempDir),
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                using (Process p = Process.Start(psi))
                {
                    if (p.WaitForExit(35000))
                    {
                        string[] matches = Directory.GetFiles(tempDir, "ffmpeg.exe", SearchOption.AllDirectories);
                        if (matches.Length > 0 && File.Exists(matches[0]))
                        {
                            File.Copy(matches[0], targetExePath, true);
                            try { Directory.Delete(tempDir, true); } catch { }
                            return true;
                        }
                    }
                }
                try { Directory.Delete(tempDir, true); } catch { }
            }
            catch { }

            return false;
        }

        private static void SetupYtDlpEngine(string sourceDir, string targetYtDlp, Action<int, string, string> progress = null)
        {
            string localSrc = Path.Combine(sourceDir ?? "", "yt-dlp.exe");
            if (File.Exists(localSrc) && new FileInfo(localSrc).Length > 10000000)
            {
                if (progress != null) progress(50, "Configuring local yt-dlp engine...", "Instant copy from source folder");
                try { File.Copy(localSrc, targetYtDlp, true); if (File.Exists(targetYtDlp)) return; } catch { }
            }

            if (File.Exists(targetYtDlp) && new FileInfo(targetYtDlp).Length > 10000000)
            {
                if (progress != null) progress(52, "yt-dlp engine is ready and verified.", targetYtDlp);
                return;
            }

            if (progress != null) progress(42, "Downloading yt-dlp engine...", "Fetching latest nightly release from GitHub...");
            string nightlyUrl = "https://github.com/yt-dlp/yt-dlp-nightly-builds/releases/latest/download/yt-dlp.exe";
            bool ok = DownloadFileWithProgress(nightlyUrl, targetYtDlp, (pct, recv, total) =>
            {
                if (progress != null)
                {
                    int p = 42 + (int)(pct * 0.12);
                    string mb = (recv / (1024.0 * 1024.0)).ToString("0.0") + " MB";
                    string tot = total > 0 ? " / " + (total / (1024.0 * 1024.0)).ToString("0.0") + " MB" : "";
                    progress(p, "Downloading yt-dlp (" + mb + tot + ")...", nightlyUrl);
                }
            });

            if (!ok || !File.Exists(targetYtDlp))
            {
                string stableUrl = "https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe";
                DownloadFileWithProgress(stableUrl, targetYtDlp, (pct, recv, total) =>
                {
                    if (progress != null)
                    {
                        int p = 42 + (int)(pct * 0.12);
                        progress(p, "Downloading yt-dlp (stable build)...", stableUrl);
                    }
                });
            }
        }

        private static void SetupFFmpegEngine(string sourceDir, string targetFFmpeg, Action<int, string, string> progress = null)
        {
            string localSrc = Path.Combine(sourceDir ?? "", "ffmpeg.exe");
            if (File.Exists(localSrc) && new FileInfo(localSrc).Length > 10000000)
            {
                if (progress != null) progress(75, "Configuring local FFmpeg media engine...", "Instant copy (" + (new FileInfo(localSrc).Length / (1024 * 1024)) + " MB)");
                try { File.Copy(localSrc, targetFFmpeg, true); if (File.Exists(targetFFmpeg)) return; } catch { }
            }

            if (File.Exists(targetFFmpeg) && new FileInfo(targetFFmpeg).Length > 10000000)
            {
                if (progress != null) progress(80, "FFmpeg media engine is ready and verified.", targetFFmpeg);
                return;
            }

            string sysF = FindSystemFFmpeg();
            if (!string.IsNullOrEmpty(sysF) && File.Exists(sysF))
            {
                if (progress != null) progress(75, "Linking system FFmpeg media engine...", sysF);
                try { File.Copy(sysF, targetFFmpeg, true); if (File.Exists(targetFFmpeg)) return; } catch { }
            }

            // Winget attempt
            try
            {
                if (progress != null) progress(65, "Checking Windows Package Manager (winget)...", "Querying yt-dlp.FFmpeg package...");
                ProcessStartInfo wpsi = new ProcessStartInfo
                {
                    FileName = "winget.exe",
                    Arguments = "install --id yt-dlp.FFmpeg --accept-package-agreements --accept-source-agreements --silent",
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                using (Process wp = Process.Start(wpsi))
                {
                    if (wp.WaitForExit(45000))
                    {
                        string wf = FindSystemFFmpeg();
                        if (!string.IsNullOrEmpty(wf) && File.Exists(wf))
                        {
                            File.Copy(wf, targetFFmpeg, true);
                            if (File.Exists(targetFFmpeg))
                            {
                                if (progress != null) progress(82, "FFmpeg configured via Windows Package Manager.", targetFFmpeg);
                                return;
                            }
                        }
                    }
                }
            }
            catch { }

            // Direct Zip Download from GitHub / gyan.dev
            if (progress != null) progress(68, "Downloading FFmpeg media engine package...", "Downloading zip archive from GitHub...");
            string tempZip = Path.Combine(Path.GetTempPath(), "180hz_ffmpeg_setup_" + Guid.NewGuid().ToString("N") + ".zip");
            try
            {
                string ffmpegZipUrl = "https://github.com/yt-dlp/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-win64-gpl.zip";
                bool downloaded = DownloadFileWithProgress(ffmpegZipUrl, tempZip, (pct, recv, total) =>
                {
                    if (progress != null)
                    {
                        int p = 68 + (int)(pct * 0.18);
                        string mb = (recv / (1024.0 * 1024.0)).ToString("0.0") + " MB";
                        string tot = total > 0 ? " / " + (total / (1024.0 * 1024.0)).ToString("0.0") + " MB" : "";
                        progress(p, "Downloading FFmpeg engine (" + mb + tot + ")...", "Package: yt-dlp FFmpeg Builds");
                    }
                });

                if (!downloaded || !File.Exists(tempZip))
                {
                    string fallbackZipUrl = "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip";
                    downloaded = DownloadFileWithProgress(fallbackZipUrl, tempZip, (pct, recv, total) =>
                    {
                        if (progress != null)
                        {
                            int p = 68 + (int)(pct * 0.18);
                            progress(p, "Downloading FFmpeg fallback package...", "gyan.dev essentials");
                        }
                    });
                }

                if (File.Exists(tempZip))
                {
                    if (progress != null) progress(86, "Extracting FFmpeg executable...", "Unpacking ffmpeg.exe from archive...");
                    ExtractFFmpegFromZip(tempZip, targetFFmpeg);
                }
            }
            finally
            {
                try { if (File.Exists(tempZip)) File.Delete(tempZip); } catch { }
            }

            if (File.Exists(targetFFmpeg))
            {
                if (progress != null) progress(88, "FFmpeg media engine extracted and ready.", targetFFmpeg);
            }
        }

        private static bool isUpdatingYtDlp = false;
        private static void CheckUpdateYtDlp()
        {
            if (isUpdatingYtDlp || !File.Exists(ytdlpPath)) return;
            isUpdatingYtDlp = true;
            Task.Factory.StartNew(() =>
            {
                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = ytdlpPath;
                    psi.Arguments = "--update-to nightly";
                    psi.CreateNoWindow = true;
                    psi.UseShellExecute = false;
                    using (Process p = Process.Start(psi))
                    {
                        p.WaitForExit(90000);
                    }
                }
                catch { }
                finally
                {
                    isUpdatingYtDlp = false;
                }
            });
        }

        private static void EnsureDependencies()
        {
            if (File.Exists(ytdlpPath) && File.Exists(ffmpegPath))
            {
                depStatusMessage = "Ready";
                CheckUpdateYtDlp();
                return;
            }

            if (isDownloadingDeps) return;
            isDownloadingDeps = true;

            Task.Factory.StartNew(() =>
            {
                try
                {
                    if (!File.Exists(ytdlpPath))
                    {
                        depStatusMessage = "Downloading extraction engine (yt-dlp)...";
                        SetupYtDlpEngine(appDir, ytdlpPath, (p, s, d) => { depStatusMessage = s; });
                    }
                    else
                    {
                        CheckUpdateYtDlp();
                    }

                    if (!File.Exists(ffmpegPath))
                    {
                        depStatusMessage = "Configuring FFmpeg media engine...";
                        SetupFFmpegEngine(appDir, ffmpegPath, (p, s, d) => { depStatusMessage = s; });
                    }

                    depStatusMessage = (File.Exists(ytdlpPath) && File.Exists(ffmpegPath)) ? "Ready" : "Partial";
                }
                catch (Exception ex)
                {
                    depStatusMessage = "Dependency check error";
                    try { File.AppendAllText(Path.Combine(appDir, "dependencies.log"), ex.ToString() + "\r\n"); } catch { }
                }
                finally
                {
                    isDownloadingDeps = false;
                }
            });
        }

        private static string GetNodePath()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string[] candidates = new string[]
            {
                @"C:\Program Files\nodejs\node.exe",
                @"C:\Program Files (x86)\nodejs\node.exe",
                Path.Combine(localAppData, @"Programs\node\node.exe"),
                Path.Combine(localAppData, @"Microsoft\WinGet\Links\node.exe"),
                Path.Combine(appDir ?? "", "node.exe")
            };
            foreach (string c in candidates)
            {
                if (!string.IsNullOrEmpty(c) && File.Exists(c)) return c;
            }
            try
            {
                string envPath = Environment.GetEnvironmentVariable("PATH");
                if (!string.IsNullOrEmpty(envPath))
                {
                    foreach (string p in envPath.Split(';'))
                    {
                        if (string.IsNullOrEmpty(p)) continue;
                        string np = Path.Combine(p.Trim(), "node.exe");
                        if (File.Exists(np)) return np;
                    }
                }
            }
            catch { }
            return null;
        }

        private static readonly object cookieLock = new object();
        private static DateTime lastCookieFetch = DateTime.MinValue;

        private static string EnsureYouTubeVisitorCookies(bool forceRefresh = false)
        {
            try
            {
                // Determine target path for youtube_visitor_cookies.txt
                string cookiesPath = Path.Combine(appDir ?? "", "youtube_visitor_cookies.txt");
                if (string.IsNullOrEmpty(appDir) || !Directory.Exists(appDir))
                {
                    cookiesPath = Path.Combine(Path.GetTempPath(), "180hz_yt_visitor_cookies.txt");
                }

                lock (cookieLock)
                {
                    if (!forceRefresh && File.Exists(cookiesPath))
                    {
                        FileInfo fi = new FileInfo(cookiesPath);
                        if (fi.Length > 50 && (DateTime.UtcNow - fi.LastWriteTimeUtc).TotalHours < 12)
                        {
                            return cookiesPath;
                        }
                    }

                    // Fetch fresh visitor session cookies from YouTube guest endpoints
                    CookieContainer container = new CookieContainer();
                    string[] endpoints = new string[] { "https://www.youtube.com/sw.js", "https://www.youtube.com/" };
                    CookieCollection cc = null;

                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072 | (SecurityProtocolType)768 | SecurityProtocolType.Tls;

                    foreach (string ep in endpoints)
                    {
                        try
                        {
                            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(ep);
                            req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36";
                            req.Timeout = 8000;
                            req.CookieContainer = container;
                            using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                            {
                                cc = container.GetCookies(new Uri("https://www.youtube.com"));
                                if (cc != null && cc.Count >= 2) break;
                            }
                        }
                        catch { }
                    }

                    if (cc != null && cc.Count > 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine("# Netscape HTTP Cookie File");
                        sb.AppendLine("# https://curl.se/docs/http-cookies.html");
                        sb.AppendLine("# Automatically generated by 180Hz Downloader");
                        long expiry = (long)(DateTime.UtcNow.AddYears(1) - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
                        foreach (Cookie c in cc)
                        {
                            string isSecure = c.Name.StartsWith("__Secure-") ? "TRUE" : "FALSE";
                            sb.AppendLine(string.Format(".youtube.com\tTRUE\t/\t{0}\t{1}\t{2}\t{3}", isSecure, expiry, c.Name, c.Value));
                        }

                        try
                        {
                            File.WriteAllText(cookiesPath, sb.ToString(), Encoding.ASCII);
                            lastCookieFetch = DateTime.UtcNow;
                            return cookiesPath;
                        }
                        catch
                        {
                            string fallback = Path.Combine(Path.GetTempPath(), "180hz_yt_visitor_cookies.txt");
                            File.WriteAllText(fallback, sb.ToString(), Encoding.ASCII);
                            lastCookieFetch = DateTime.UtcNow;
                            return fallback;
                        }
                    }

                    if (File.Exists(cookiesPath) && new FileInfo(cookiesPath).Length > 0)
                        return cookiesPath;
                }
            }
            catch (Exception ex)
            {
                try { File.AppendAllText(Path.Combine(appDir ?? "", "cookies.log"), DateTime.UtcNow.ToString("o") + " [EnsureYouTubeVisitorCookies] " + ex.Message + "\r\n"); } catch { }
            }

            return null;
        }

        private static string ResolveUserPlatformCookies(string targetUrl = "", string customSaveDir = null)
        {
            try
            {
                string effectiveSave = GetEffectiveWritableSaveDir();
                string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                List<string> checkDirs = new List<string>();

                Action<string> AddDir = delegate(string d)
                {
                    if (!string.IsNullOrEmpty(d) && Directory.Exists(d) && !checkDirs.Contains(d))
                        checkDirs.Add(d);
                };

                AddDir(customSaveDir);
                AddDir(effectiveSave);
                AddDir(saveDir);
                AddDir(appDir);
                AddDir(Path.Combine(userProfile, "Downloads"));
                AddDir(Path.Combine(userProfile, "Desktop"));
                AddDir(Path.Combine(userProfile, "Documents"));
                AddDir(userProfile);
                AddDir(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "180Hz Downloader"));

                // 1. Direct priority candidates
                foreach (string dir in checkDirs)
                {
                    string[] directCandidates = new string[]
                    {
                        Path.Combine(dir, "cookies.txt"),
                        Path.Combine(dir, "instagram_cookies.txt"),
                        Path.Combine(dir, "cookie.txt"),
                        Path.Combine(dir, "instagram.cookies")
                    };

                    foreach (string cand in directCandidates)
                    {
                        if (File.Exists(cand))
                        {
                            FileInfo fi = new FileInfo(cand);
                            if (fi.Length > 20) return cand;
                        }
                    }
                }

                // 2. Scan each directory for any *cookie*.txt or *instagram*.txt
                foreach (string dir in checkDirs)
                {
                    try
                    {
                        string[] txtFiles = Directory.GetFiles(dir, "*cookie*.txt", SearchOption.TopDirectoryOnly);
                        foreach (string tf in txtFiles)
                        {
                            if (File.Exists(tf))
                            {
                                FileInfo fi = new FileInfo(tf);
                                if (fi.Length > 20) return tf;
                            }
                        }
                    }
                    catch { }
                }
            }
            catch { }
            return null;
        }

        private static bool SaveCookiesContent(string input)
        {
            try
            {
                if (string.IsNullOrEmpty(input)) return false;
                string trimmed = input.Trim();
                StringBuilder sb = new StringBuilder();

                if (trimmed.IndexOf("# Netscape", StringComparison.OrdinalIgnoreCase) >= 0 || trimmed.Contains("\t"))
                {
                    sb.Append(trimmed);
                }
                else
                {
                    sb.AppendLine("# Netscape HTTP Cookie File");
                    sb.AppendLine("# Generated by 180Hz Downloader Cookie Assistant");
                    long expiry = (long)(DateTime.UtcNow.AddYears(1) - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
                    if (trimmed.Contains("=") || trimmed.Contains(";"))
                    {
                        string[] parts = trimmed.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string part in parts)
                        {
                            int eqIdx = part.IndexOf('=');
                            if (eqIdx > 0)
                            {
                                string k = part.Substring(0, eqIdx).Trim();
                                string v = part.Substring(eqIdx + 1).Trim();
                                if (!string.IsNullOrEmpty(k) && !string.IsNullOrEmpty(v))
                                {
                                    sb.AppendLine(string.Format(".instagram.com\tTRUE\t/\tTRUE\t{0}\t{1}\t{2}", expiry, k, v));
                                }
                            }
                        }
                    }
                    else
                    {
                        sb.AppendLine(string.Format(".instagram.com\tTRUE\t/\tTRUE\t{0}\tsessionid\t{1}", expiry, trimmed));
                    }
                }

                string textToWrite = sb.ToString();
                string effectiveSave = GetEffectiveWritableSaveDir();
                try
                {
                    if (!Directory.Exists(effectiveSave)) Directory.CreateDirectory(effectiveSave);
                    File.WriteAllText(Path.Combine(effectiveSave, "cookies.txt"), textToWrite, Encoding.UTF8);
                }
                catch { }

                if (!string.IsNullOrEmpty(appDir) && Directory.Exists(appDir))
                {
                    try
                    {
                        File.WriteAllText(Path.Combine(appDir, "cookies.txt"), textToWrite, Encoding.UTF8);
                    }
                    catch { }
                }

                string localApp = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "180Hz Downloader");
                if (Directory.Exists(localApp))
                {
                    try
                    {
                        File.WriteAllText(Path.Combine(localApp, "cookies.txt"), textToWrite, Encoding.UTF8);
                    }
                    catch { }
                }

                return true;
            }
            catch { }
            return false;
        }

        private static string PromptUserForCookiesFile()
        {
            string selectedFile = null;
            Thread staThread = new Thread(() =>
            {
                try
                {
                    using (Form topForm = new Form())
                    {
                        topForm.TopMost = true;
                        topForm.ShowInTaskbar = false;
                        topForm.FormBorderStyle = FormBorderStyle.None;
                        topForm.StartPosition = FormStartPosition.CenterScreen;
                        topForm.Size = new Size(1, 1);
                        topForm.Opacity = 0;
                        topForm.Show();
                        topForm.Focus();
                        topForm.BringToFront();
                        topForm.Activate();

                        using (OpenFileDialog ofd = new OpenFileDialog())
                        {
                            ofd.Title = "Select cookies.txt file for 180Hz Downloader";
                            ofd.Filter = "Cookie Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
                            ofd.CheckFileExists = true;
                            ofd.Multiselect = false;
                            DialogResult result = ofd.ShowDialog(topForm);
                            if (result == DialogResult.OK && !string.IsNullOrEmpty(ofd.FileName))
                            {
                                selectedFile = ofd.FileName;
                            }
                        }
                        topForm.Close();
                    }
                }
                catch { }
            });
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join(60000);

            if (!string.IsNullOrEmpty(selectedFile) && File.Exists(selectedFile))
            {
                try
                {
                    string content = File.ReadAllText(selectedFile);
                    SaveCookiesContent(content);
                    return selectedFile;
                }
                catch { }
            }
            return null;
        }

        private static void ExtractEmbeddedResourceIfPresent(string resourceName, string targetPath)
        {
            try
            {
                Assembly asm = Assembly.GetExecutingAssembly();
                using (Stream stream = asm.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        string dir = Path.GetDirectoryName(targetPath);
                        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                            Directory.CreateDirectory(dir);
                        using (FileStream fs = new FileStream(targetPath, FileMode.Create, FileAccess.Write))
                        {
                            stream.CopyTo(fs);
                        }
                    }
                }
            }
            catch { }
        }

        private static void EnsureEmbeddedWebAssets(string baseDir, bool forceOverwrite = false)
        {
            try
            {
                string webDir = Path.Combine(baseDir, "web");
                if (!Directory.Exists(webDir)) Directory.CreateDirectory(webDir);
                string[] assets = new string[] { "index.html", "styles.css", "app.js", "version.json", "click.mp3" };
                foreach (string asset in assets)
                {
                    string target = Path.Combine(webDir, asset);
                    if (forceOverwrite || !File.Exists(target))
                    {
                        ExtractEmbeddedResourceIfPresent(asset, target);
                    }
                }
                string icoTarget = Path.Combine(baseDir, "app.ico");
                if (forceOverwrite || !File.Exists(icoTarget))
                {
                    ExtractEmbeddedResourceIfPresent("app.ico", icoTarget);
                }
            }
            catch { }
        }

        private static bool isCustomSaveDirConfigured = false;
        private static readonly object folderPickerLock = new object();

        private static string GetSaveFolderConfigPath()
        {
            try
            {
                string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string dir = Path.Combine(localAppData, "Programs", "180Hz Downloader");
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                return Path.Combine(dir, "save_folder.cfg");
            }
            catch
            {
                return Path.Combine(appDir ?? AppDomain.CurrentDomain.BaseDirectory, "save_folder.cfg");
            }
        }

        private static bool IsDirectoryWritable(string dirPath)
        {
            if (string.IsNullOrEmpty(dirPath)) return false;
            try
            {
                if (!Directory.Exists(dirPath))
                {
                    try { Directory.CreateDirectory(dirPath); } catch { return false; }
                }
                string testFile = Path.Combine(dirPath, ".__180hz_test_" + Guid.NewGuid().ToString("N").Substring(0, 8) + ".tmp");
                File.WriteAllText(testFile, "180hz_write_test", Encoding.UTF8);
                if (File.Exists(testFile))
                {
                    try { File.Delete(testFile); } catch { }
                    return true;
                }
            }
            catch { }
            return false;
        }

        private static string GetDefaultDownloadsFolder()
        {
            try
            {
                string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                if (!string.IsNullOrEmpty(userProfile))
                {
                    string dl = Path.Combine(userProfile, "Downloads");
                    if (!Directory.Exists(dl)) try { Directory.CreateDirectory(dl); } catch { }
                    if (Directory.Exists(dl) && IsDirectoryWritable(dl)) return dl;
                }
            }
            catch { }

            try
            {
                string docDl = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Downloads");
                if (!Directory.Exists(docDl)) try { Directory.CreateDirectory(docDl); } catch { }
                if (Directory.Exists(docDl) && IsDirectoryWritable(docDl)) return docDl;
            }
            catch { }

            return !string.IsNullOrEmpty(appDir) && IsDirectoryWritable(appDir) ? appDir : Path.GetTempPath();
        }

        private static string GetEffectiveWritableSaveDir(string targetDir = null)
        {
            if (string.IsNullOrEmpty(targetDir)) targetDir = saveDir;
            if (string.IsNullOrEmpty(targetDir)) return GetDefaultDownloadsFolder();
            try
            {
                targetDir = targetDir.Trim();
                string root = Path.GetPathRoot(targetDir);
                bool isDriveRoot = !string.IsNullOrEmpty(root) && 
                    string.Equals(root.TrimEnd('\\', '/'), targetDir.TrimEnd('\\', '/'), StringComparison.OrdinalIgnoreCase);

                // If user selected a drive root like "C:\" or a directory where direct file creation is denied:
                if (isDriveRoot || !IsDirectoryWritable(targetDir))
                {
                    // 1. Try a "Downloads" subfolder under this drive (e.g. C:\Downloads, D:\Downloads)
                    string subDownloads = Path.Combine(targetDir, "Downloads");
                    try
                    {
                        if (!Directory.Exists(subDownloads)) Directory.CreateDirectory(subDownloads);
                        if (IsDirectoryWritable(subDownloads)) return subDownloads;
                    }
                    catch { }

                    // 2. Try a "180Hz Downloads" subfolder under this drive
                    string sub180 = Path.Combine(targetDir, "180Hz Downloads");
                    try
                    {
                        if (!Directory.Exists(sub180)) Directory.CreateDirectory(sub180);
                        if (IsDirectoryWritable(sub180)) return sub180;
                    }
                    catch { }

                    // 3. Fallback to the user's primary Windows Downloads folder
                    return GetDefaultDownloadsFolder();
                }

                return targetDir;
            }
            catch
            {
                return GetDefaultDownloadsFolder();
            }
        }

        private static void LoadSaveFolderConfig()
        {
            string defaultDownloads = GetDefaultDownloadsFolder();

            try
            {
                string cfgPath = GetSaveFolderConfigPath();
                string localCfgPath = Path.Combine(appDir ?? AppDomain.CurrentDomain.BaseDirectory, "save_folder.cfg");
                string savedPath = null;

                if (File.Exists(cfgPath))
                {
                    savedPath = File.ReadAllText(cfgPath, Encoding.UTF8).Trim();
                }
                else if (File.Exists(localCfgPath))
                {
                    savedPath = File.ReadAllText(localCfgPath, Encoding.UTF8).Trim();
                }

                if (!string.IsNullOrEmpty(savedPath))
                {
                    string writablePath = GetEffectiveWritableSaveDir(savedPath);
                    if (Directory.Exists(writablePath))
                    {
                        saveDir = writablePath;
                        isCustomSaveDirConfigured = true;
                        if (!string.Equals(savedPath, writablePath, StringComparison.OrdinalIgnoreCase))
                        {
                            try { File.WriteAllText(cfgPath, writablePath, Encoding.UTF8); } catch { }
                            try { File.WriteAllText(localCfgPath, writablePath, Encoding.UTF8); } catch { }
                        }
                        return;
                    }
                }
            }
            catch { }

            saveDir = defaultDownloads;
            isCustomSaveDirConfigured = false;
        }

        private static bool SaveCustomFolderConfig(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath)) return false;
            try
            {
                string writablePath = GetEffectiveWritableSaveDir(folderPath);
                if (!Directory.Exists(writablePath))
                {
                    Directory.CreateDirectory(writablePath);
                }
                saveDir = writablePath;
                isCustomSaveDirConfigured = true;

                string cfgPath = GetSaveFolderConfigPath();
                File.WriteAllText(cfgPath, writablePath, Encoding.UTF8);

                try
                {
                    string localCfgPath = Path.Combine(appDir ?? AppDomain.CurrentDomain.BaseDirectory, "save_folder.cfg");
                    File.WriteAllText(localCfgPath, writablePath, Encoding.UTF8);
                }
                catch { }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string PromptUserForSaveFolder()
        {
            lock (folderPickerLock)
            {
                string selectedPath = null;
                Thread staThread = new Thread(() =>
                {
                    try
                    {
                        using (Form topForm = new Form())
                        {
                            topForm.TopMost = true;
                            topForm.ShowInTaskbar = false;
                            topForm.FormBorderStyle = FormBorderStyle.None;
                            topForm.StartPosition = FormStartPosition.CenterScreen;
                            topForm.Size = new Size(1, 1);
                            topForm.Opacity = 0;
                            topForm.Show();
                            topForm.Focus();
                            topForm.BringToFront();
                            topForm.Activate();

                            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
                            {
                                fbd.Description = "Select a folder on This PC to save downloaded videos & audio (180Hz Downloader):";
                                fbd.ShowNewFolderButton = true;
                                fbd.RootFolder = Environment.SpecialFolder.MyComputer;
                                if (isCustomSaveDirConfigured && !string.IsNullOrEmpty(saveDir) && Directory.Exists(saveDir))
                                {
                                    fbd.SelectedPath = saveDir;
                                }
                                DialogResult result = fbd.ShowDialog(topForm);
                                if (result == DialogResult.OK && !string.IsNullOrEmpty(fbd.SelectedPath))
                                {
                                    selectedPath = fbd.SelectedPath;
                                }
                            }
                            topForm.Close();
                        }
                    }
                    catch { }
                });
                staThread.SetApartmentState(ApartmentState.STA);
                staThread.Start();
                staThread.Join();

                if (!string.IsNullOrEmpty(selectedPath))
                {
                    SaveCustomFolderConfig(selectedPath);
                }
                return selectedPath;
            }
        }

        private static void LogTrace(string msg)
        {
            try
            {
                string traceFile = Path.Combine(Path.GetTempPath(), "180hz_boot_trace.log");
                File.AppendAllText(traceFile, DateTime.Now.ToString("HH:mm:ss.fff") + " " + msg + "\r\n");
            }
            catch { }
        }

        private static void CreateAppShortcuts(string targetExe, string workingDir)
        {
            try
            {
                Type shellType = Type.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8"));
                if (shellType == null) return;
                object shell = Activator.CreateInstance(shellType);
                if (shell == null) return;

                string iconPath = Path.Combine(workingDir, "app.ico");
                string iconSpec = File.Exists(iconPath) ? iconPath + ",0" : targetExe + ",0";

                Action<string> makeLnk = (lnkPath) =>
                {
                    try
                    {
                        object sc = shellType.InvokeMember("CreateShortcut", BindingFlags.InvokeMethod, null, shell, new object[] { lnkPath });
                        if (sc != null)
                        {
                            Type scType = sc.GetType();
                            scType.InvokeMember("TargetPath", BindingFlags.SetProperty, null, sc, new object[] { targetExe });
                            scType.InvokeMember("WorkingDirectory", BindingFlags.SetProperty, null, sc, new object[] { workingDir });
                            scType.InvokeMember("Description", BindingFlags.SetProperty, null, sc, new object[] { "180Hz Downloader - Lossless 8K Media Engine" });
                            scType.InvokeMember("IconLocation", BindingFlags.SetProperty, null, sc, new object[] { iconSpec });
                            scType.InvokeMember("Save", BindingFlags.InvokeMethod, null, sc, null);
                        }
                    }
                    catch { }
                };

                // Desktop shortcut
                try
                {
                    string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    if (!string.IsNullOrEmpty(desktop) && Directory.Exists(desktop))
                    {
                        makeLnk(Path.Combine(desktop, "180Hz Downloader.lnk"));
                    }
                }
                catch { }

                // Start Menu shortcut
                try
                {
                    string startMenu = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
                    if (!string.IsNullOrEmpty(startMenu) && Directory.Exists(startMenu))
                    {
                        makeLnk(Path.Combine(startMenu, "180Hz Downloader.lnk"));
                    }
                }
                catch { }
            }
            catch { }
        }

        private static void KillRunningAppProcesses()
        {
            try
            {
                int myPid = Process.GetCurrentProcess().Id;
                string[] procNames = new string[] { "180Hz Downloader", "180HzDownloader", "TurboDownloader" };
                foreach (string pName in procNames)
                {
                    try
                    {
                        foreach (Process p in Process.GetProcessesByName(pName))
                        {
                            if (p.Id != myPid)
                            {
                                try { p.Kill(); p.WaitForExit(2500); } catch { }
                            }
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        private static bool SafeCopyFileWithRetry(string source, string dest)
        {
            for (int retry = 0; retry < 15; retry++)
            {
                try
                {
                    if (File.Exists(dest))
                    {
                        string bak = dest + ".old";
                        try { if (File.Exists(bak)) File.Delete(bak); } catch { }
                        try { File.Move(dest, bak); } catch { }
                    }
                    File.Copy(source, dest, true);
                    return true;
                }
                catch
                {
                    Thread.Sleep(250);
                }
            }
            return false;
        }

        public class SetupProgressForm : Form
        {
            private Label lblTitle;
            private Label lblSubtitle;
            private Label lblStatus;
            private Label lblDetail;
            private ProgressBar progressBar;
            private Label lblFooter;
            private Panel pnlHeader;
            private Panel pnlDivider;

            public SetupProgressForm(Icon icon = null)
            {
                this.Text = "180Hz Downloader Setup";
                this.Size = new Size(540, 275);
                this.StartPosition = FormStartPosition.CenterScreen;
                this.FormBorderStyle = FormBorderStyle.FixedDialog;
                this.MaximizeBox = false;
                this.MinimizeBox = false;
                this.BackColor = Color.FromArgb(17, 18, 22);
                this.ForeColor = Color.FromArgb(240, 240, 240);
                this.ShowInTaskbar = true;
                this.TopMost = false;
                if (icon != null) this.Icon = icon;

                pnlHeader = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 65,
                    BackColor = Color.FromArgb(24, 25, 30)
                };

                lblTitle = new Label
                {
                    Text = "⚡ 180Hz DOWNLOADER",
                    Font = new Font("Segoe UI", 14.5f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(216, 255, 62),
                    Location = new Point(20, 12),
                    AutoSize = true
                };

                lblSubtitle = new Label
                {
                    Text = "All-In-One Setup: yt-dlp, FFmpeg & Full Studio Engine",
                    Font = new Font("Segoe UI", 9.0f, FontStyle.Regular),
                    ForeColor = Color.FromArgb(160, 165, 180),
                    Location = new Point(22, 38),
                    AutoSize = true
                };

                pnlDivider = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 1,
                    BackColor = Color.FromArgb(40, 42, 50)
                };

                lblStatus = new Label
                {
                    Text = "Initializing setup...",
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(245, 245, 245),
                    Location = new Point(22, 85),
                    Size = new Size(480, 20)
                };

                lblDetail = new Label
                {
                    Text = "Preparing application components...",
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                    ForeColor = Color.FromArgb(130, 135, 150),
                    Location = new Point(22, 108),
                    Size = new Size(480, 20)
                };

                progressBar = new ProgressBar
                {
                    Location = new Point(24, 138),
                    Size = new Size(475, 22),
                    Minimum = 0,
                    Maximum = 100,
                    Value = 0,
                    Style = ProgressBarStyle.Continuous
                };

                lblFooter = new Label
                {
                    Text = "Lossless PEAK/8K Video · 320 kbps Studio Audio · GitHub Live Sync",
                    Font = new Font("Segoe UI", 7.8f, FontStyle.Regular),
                    ForeColor = Color.FromArgb(90, 95, 110),
                    Location = new Point(24, 178),
                    Size = new Size(475, 18),
                    TextAlign = ContentAlignment.MiddleCenter
                };

                pnlHeader.Controls.Add(lblTitle);
                pnlHeader.Controls.Add(lblSubtitle);
                this.Controls.Add(lblFooter);
                this.Controls.Add(progressBar);
                this.Controls.Add(lblDetail);
                this.Controls.Add(lblStatus);
                this.Controls.Add(pnlDivider);
                this.Controls.Add(pnlHeader);
            }

            public void UpdateProgress(int percent, string status, string detail = null)
            {
                if (this.IsDisposed) return;
                if (this.InvokeRequired)
                {
                    try { this.BeginInvoke(new Action(() => UpdateProgress(percent, status, detail))); } catch { }
                    return;
                }
                if (percent < 0) percent = 0;
                if (percent > 100) percent = 100;
                progressBar.Value = percent;
                if (!string.IsNullOrEmpty(status)) lblStatus.Text = status;
                if (detail != null) lblDetail.Text = detail;
            }
        }

        private static void RunCompleteSetup(bool isSilent)
        {
            Action<int, string, string> progressAction = null;
            SetupProgressForm form = null;

            if (!isSilent)
            {
                Icon icon = null;
                try
                {
                    string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.ico");
                    if (File.Exists(iconPath)) icon = new Icon(iconPath);
                }
                catch { }

                form = new SetupProgressForm(icon);
                progressAction = (pct, stat, det) =>
                {
                    if (form != null) form.UpdateProgress(pct, stat, det);
                };

                form.Shown += (s, ev) =>
                {
                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            ExecuteCompleteSetup(progressAction);
                            progressAction(100, "Setup Complete!", "Starting 180Hz Downloader...");
                            Thread.Sleep(800);
                            try { form.Invoke(new Action(() => form.Close())); } catch { }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Setup encountered an issue: " + ex.Message, "180Hz Downloader Setup", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            try { form.Invoke(new Action(() => form.Close())); } catch { }
                        }
                    });
                };

                Application.Run(form);
            }
            else
            {
                progressAction = (pct, stat, det) => { };
                ExecuteCompleteSetup(progressAction);
            }
        }

        private static void ExecuteCompleteSetup(Action<int, string, string> progress)
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string installDir = Path.Combine(localAppData, "Programs", "180Hz Downloader");
            string sourceExe = Application.ExecutablePath;
            string sourceDir = AppDomain.CurrentDomain.BaseDirectory;
            string installedExe = Path.Combine(installDir, "180Hz Downloader.exe");

            progress(5, "Closing running instances...", "Releasing file locks...");
            KillRunningAppProcesses();

            progress(15, "Configuring program folder...", installDir);
            if (!Directory.Exists(installDir)) Directory.CreateDirectory(installDir);

            progress(25, "Installing core application...", "Copying 180Hz Downloader.exe...");
            if (!string.Equals(Path.GetFullPath(sourceExe), Path.GetFullPath(installedExe), StringComparison.OrdinalIgnoreCase))
            {
                SafeCopyFileWithRetry(sourceExe, installedExe);
            }

            progress(35, "Extracting interface assets...", "Extracting studio web interface...");
            EnsureEmbeddedWebAssets(installDir, true);

            // Copy app.ico
            try
            {
                string iconPath = Path.Combine(installDir, "app.ico");
                ExtractEmbeddedResourceIfPresent("app.ico", iconPath);
                string localIco = Path.Combine(sourceDir, "app.ico");
                if (!File.Exists(iconPath) && File.Exists(localIco)) File.Copy(localIco, iconPath, true);
            }
            catch { }

            // Copy cookies if present
            try
            {
                string srcCookies = Path.Combine(sourceDir, "youtube_visitor_cookies.txt");
                string dstCookies = Path.Combine(installDir, "youtube_visitor_cookies.txt");
                if (File.Exists(srcCookies) && !File.Exists(dstCookies))
                {
                    File.Copy(srcCookies, dstCookies, true);
                }
            }
            catch { }

            // Media Engine 1: yt-dlp
            string targetYtDlp = Path.Combine(installDir, "yt-dlp.exe");
            progress(45, "Configuring media extraction engine (yt-dlp)...", "Checking yt-dlp executable...");
            SetupYtDlpEngine(sourceDir, targetYtDlp, progress);

            // Media Engine 2: FFmpeg
            string targetFFmpeg = Path.Combine(installDir, "ffmpeg.exe");
            progress(60, "Configuring FFmpeg media engine...", "Checking FFmpeg 8K/320k merger pipeline...");
            SetupFFmpegEngine(sourceDir, targetFFmpeg, progress);

            // Windows Shortcuts
            progress(92, "Creating shortcuts...", "Configuring Desktop & Start Menu shortcuts...");
            CreateAppShortcuts(installedExe, installDir);

            progress(98, "Finalizing installation...", "Setting up environment...");
            try
            {
                string installedCookies = Path.Combine(installDir, "youtube_visitor_cookies.txt");
                if (!File.Exists(installedCookies))
                {
                    Task.Factory.StartNew(() => { EnsureYouTubeVisitorCookies(); });
                }
            }
            catch { }

            progress(100, "Setup completed successfully!", "Launching 180Hz Downloader...");

            // Launch installed application
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = installedExe,
                    WorkingDirectory = installDir,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch { }
        }

        [STAThread]
        static void Main(string[] args)
        {
            LogTrace("Main start. Args: " + (args != null ? string.Join(" ", args) : "none"));
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                {
                    try
                    {
                        LogTrace("UnhandledException: " + (e.ExceptionObject != null ? e.ExceptionObject.ToString() : "null"));
                        File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash.log"), (e.ExceptionObject != null ? e.ExceptionObject.ToString() : "Unknown error"));
                    }
                    catch { }
                };

                bool isRestartedFromUpdate = false;
                bool isApplyUpdateArg = false;
                string applyTargetExe = null;
                bool isSilent = false;
                bool isExplicitSetup = false;
                if (args != null)
                {
                    for (int i = 0; i < args.Length; i++)
                    {
                        string a = args[i] ?? "";
                        if (string.Equals(a, "--restarted-from-update", StringComparison.OrdinalIgnoreCase))
                        {
                            isRestartedFromUpdate = true;
                        }
                        else if (string.Equals(a, "--apply-update", StringComparison.OrdinalIgnoreCase))
                        {
                            isApplyUpdateArg = true;
                            if (i + 1 < args.Length && !string.IsNullOrEmpty(args[i + 1]))
                            {
                                applyTargetExe = args[i + 1];
                            }
                        }
                        else if (string.Equals(a, "--silent", StringComparison.OrdinalIgnoreCase) ||
                                 string.Equals(a, "--quiet", StringComparison.OrdinalIgnoreCase) ||
                                 string.Equals(a, "-s", StringComparison.OrdinalIgnoreCase))
                        {
                            isSilent = true;
                        }
                        else if (string.Equals(a, "--setup", StringComparison.OrdinalIgnoreCase) ||
                                 string.Equals(a, "--install", StringComparison.OrdinalIgnoreCase))
                        {
                            isExplicitSetup = true;
                        }
                    }
                }

                string currentExe = Application.ExecutablePath;
                string currentExeName = Path.GetFileName(currentExe) ?? "";
                string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string installDir = Path.Combine(localAppData, "Programs", "180Hz Downloader");

                // Clean up any leftover .old files from atomic self-updates
                try
                {
                    string localOld = currentExe + ".old";
                    if (File.Exists(localOld)) File.Delete(localOld);
                }
                catch { }
                try
                {
                    string instOld = Path.Combine(installDir, "180Hz Downloader.exe.old");
                    if (File.Exists(instOld)) File.Delete(instOld);
                }
                catch { }

                // Check for Setup / Installation execution
                bool isSetupMode = !isApplyUpdateArg && (isExplicitSetup ||
                                   currentExeName.IndexOf("Setup", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                   currentExeName.IndexOf("Install", StringComparison.OrdinalIgnoreCase) >= 0);

                if (isSetupMode)
                {
                    RunCompleteSetup(isSilent);
                    return;
                }

                // Internal updater replacement routine (invoked by --apply-update)
                if (isApplyUpdateArg || currentExeName.IndexOf("Update", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    try
                    {
                        if (!Directory.Exists(installDir)) Directory.CreateDirectory(installDir);
                        string installedExe = Path.Combine(installDir, "180Hz Downloader.exe");

                        // Terminate any running 180Hz Downloader instances so file locks are released
                        KillRunningAppProcesses();

                        if (!string.Equals(Path.GetFullPath(currentExe), Path.GetFullPath(installedExe), StringComparison.OrdinalIgnoreCase))
                        {
                            SafeCopyFileWithRetry(currentExe, installedExe);
                            EnsureEmbeddedWebAssets(installDir, false);
                        }

                        string launchExe = installedExe;
                        if (!string.IsNullOrEmpty(applyTargetExe))
                        {
                            try
                            {
                                if (!string.Equals(Path.GetFullPath(currentExe), Path.GetFullPath(applyTargetExe), StringComparison.OrdinalIgnoreCase) &&
                                    !string.Equals(Path.GetFullPath(installedExe), Path.GetFullPath(applyTargetExe), StringComparison.OrdinalIgnoreCase))
                                {
                                    SafeCopyFileWithRetry(currentExe, applyTargetExe);
                                }
                                if (File.Exists(applyTargetExe)) launchExe = applyTargetExe;
                            }
                            catch { }
                        }

                        if (!string.Equals(Path.GetFullPath(currentExe), Path.GetFullPath(launchExe), StringComparison.OrdinalIgnoreCase))
                        {
                            ProcessStartInfo psi = new ProcessStartInfo();
                            psi.FileName = launchExe;
                            psi.WorkingDirectory = Path.GetDirectoryName(launchExe) ?? installDir;
                            psi.Arguments = "--restarted-from-update";
                            psi.UseShellExecute = true;
                            Process.Start(psi);
                            return;
                        }
                    }
                    catch { }
                }

                appDir = AppDomain.CurrentDomain.BaseDirectory;
                EnsureEmbeddedWebAssets(appDir, false);
                LoadSaveFolderConfig();

                ytdlpPath = Path.Combine(appDir, "yt-dlp.exe");
                ffmpegPath = Path.Combine(appDir, "ffmpeg.exe");
                if (!File.Exists(ffmpegPath))
                {
                    string sysF = FindSystemFFmpeg();
                    if (!string.IsNullOrEmpty(sysF) && File.Exists(sysF))
                    {
                        try { File.Copy(sysF, ffmpegPath, true); } catch { }
                    }
                }
                EnsureDependencies();
                Task.Factory.StartNew(() => { EnsureYouTubeVisitorCookies(); });

                portFilePath = Path.Combine(Path.GetTempPath(), "180hz_downloader.port");

                // Check single-instance mutex
                bool createdNew = false;
                try
                {
                    appMutex = new Mutex(true, "Global\\180HzDownloader_SingleInstance", out createdNew);
                }
                catch (AbandonedMutexException)
                {
                    createdNew = true;
                }
                catch
                {
                    try { appMutex = new Mutex(true, "Local\\180HzDownloader_SingleInstance", out createdNew); } catch { createdNew = true; }
                }

                LogTrace("Mutex check: createdNew=" + createdNew + ", isRestartedFromUpdate=" + isRestartedFromUpdate);
                if (!createdNew && !isRestartedFromUpdate)
                {
                    Process currentProc = Process.GetCurrentProcess();
                    bool anotherIsRunning = false;
                    try
                    {
                        string[] checkNames = new string[] { "180Hz Downloader", "180HzDownloader", "TurboDownloader" };
                        foreach (string cName in checkNames)
                        {
                            foreach (Process p in Process.GetProcessesByName(cName))
                            {
                                if (p.Id != currentProc.Id)
                                {
                                    anotherIsRunning = true;
                                    break;
                                }
                            }
                            if (anotherIsRunning) break;
                        }
                    }
                    catch { }

                    if (anotherIsRunning)
                    {
                        LogTrace("Another instance is running. Exiting after LaunchNativeWindow.");
                        int existingPort = 4000;
                        if (File.Exists(portFilePath))
                        {
                            int p;
                            if (int.TryParse(File.ReadAllText(portFilePath).Trim(), out p) && p > 0)
                                existingPort = p;
                        }
                        LaunchNativeWindow(existingPort);
                        return;
                    }
                }

                try
                {
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072 | (SecurityProtocolType)768 | SecurityProtocolType.Tls;
                    ServicePointManager.DefaultConnectionLimit = 512;
                    ServicePointManager.Expect100Continue = false;
                    ServicePointManager.UseNagleAlgorithm = false;
                }
                catch { }

                // Start HTTP Server
                LogTrace("Calling StartHttpServer()...");
                if (!StartHttpServer())
                {
                    LogTrace("Failed to bind HTTP server to any candidate port.");
                    File.WriteAllText(Path.Combine(appDir, "crash.log"), "Failed to bind HTTP server to any candidate port.");
                    return;
                }
                LogTrace("HTTP Server started on port " + port);

                // Save active port
                try { File.WriteAllText(portFilePath, port.ToString()); } catch { }

                // Launch Desktop Application Window (if restarted from update, let open browser reconnect first)
                if (isRestartedFromUpdate)
                {
                    Task.Factory.StartNew(() =>
                    {
                        Thread.Sleep(4500);
                        if (!hasRecentBrowserHealthPing)
                        {
                            LaunchNativeWindow(port);
                        }
                    });
                }
                else
                {
                    LaunchNativeWindow(port);
                }

                // Run Tray App & Message Loop
                RunTrayApp();
            }
            catch (Exception ex)
            {
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash.log"), ex.ToString());
            }
            finally
            {
                CleanUp();
            }
        }

        private static bool StartHttpServer()
        {
            List<int> candidatePorts = new List<int>();
            for (int p = 4000; p <= 4030; p++) candidatePorts.Add(p);
            candidatePorts.Add(48291);
            candidatePorts.Add(48292);
            candidatePorts.Add(48293);
            candidatePorts.Add(52000);
            candidatePorts.Add(52001);

            for (int i = 0; i < 5; i++)
            {
                int freeP = GetFreePort();
                if (freeP > 0 && !candidatePorts.Contains(freeP))
                    candidatePorts.Add(freeP);
            }

            foreach (int candidate in candidatePorts)
            {
                HttpListener l = new HttpListener();
                try
                {
                    l.Prefixes.Add(string.Format("http://127.0.0.1:{0}/", candidate));
                    l.Start();
                    listener = l;
                    port = candidate;
                    break;
                }
                catch
                {
                    try { l.Close(); } catch { }
                }
            }

            if (listener == null || !listener.IsListening)
                return false;

            Task.Factory.StartNew(() =>
            {
                while (listener != null && listener.IsListening)
                {
                    try
                    {
                        HttpListenerContext ctx = listener.GetContext();
                        Task.Factory.StartNew(() =>
                        {
                            try
                            {
                                HandleRequest(ctx);
                            }
                            catch (Exception)
                            {
                                try
                                {
                                    ctx.Response.StatusCode = 500;
                                    ctx.Response.Close();
                                }
                                catch { }
                            }
                        });
                    }
                    catch (HttpListenerException)
                    {
                        break;
                    }
                    catch (ObjectDisposedException)
                    {
                        break;
                    }
                    catch { }
                }
            });

            return true;
        }

        private static int GetFreePort()
        {
            try
            {
                TcpListener tcp = new TcpListener(IPAddress.Loopback, 0);
                tcp.Start();
                int p = ((IPEndPoint)tcp.LocalEndpoint).Port;
                tcp.Stop();
                return p;
            }
            catch
            {
                return 0;
            }
        }

        private static void RunTrayApp()
        {
            try
            {
                trayIcon = new NotifyIcon();
                trayIcon.Text = string.Format("180Hz Downloader (Port {0})", port);
                try
                {
                    string icoPath = Path.Combine(appDir, "app.ico");
                    if (File.Exists(icoPath)) trayIcon.Icon = new Icon(icoPath);
                    else trayIcon.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
                }
                catch
                {
                    trayIcon.Icon = SystemIcons.Application;
                }
                trayIcon.Visible = true;

                ContextMenu menu = new ContextMenu();
                menu.MenuItems.Add(new MenuItem("🌐 Open 180Hz Downloader", (s, e) => LaunchNativeWindow(port)));
                menu.MenuItems.Add(new MenuItem("📂 Open Downloads Folder", (s, e) => {
                    try { Process.Start("explorer.exe", saveDir); } catch { }
                }));
                menu.MenuItems.Add("-");
                menu.MenuItems.Add(new MenuItem("❌ Exit 180Hz Downloader", (s, e) => {
                    CleanUp();
                    Application.Exit();
                }));

                trayIcon.ContextMenu = menu;
                trayIcon.DoubleClick += (s, e) => LaunchNativeWindow(port);

                LogTrace("RunTrayApp: Calling Application.Run()...");
                Application.Run();
                LogTrace("RunTrayApp: Application.Run() returned.");
            }
            catch (Exception ex)
            {
                LogTrace("RunTrayApp catch: " + ex);
                try { File.AppendAllText(Path.Combine(appDir, "tray.log"), ex.ToString() + "\r\n"); } catch { }
            }

            LogTrace("RunTrayApp: entering while loop. listener=" + (listener != null) + ", isListening=" + (listener != null && listener.IsListening));
            while (listener != null && listener.IsListening)
            {
                Thread.Sleep(2000);
            }
            LogTrace("RunTrayApp: exited while loop.");
        }

        private static void CleanUp()
        {
            LogTrace("CleanUp called. Stack: " + Environment.StackTrace);
            try
            {
                if (trayIcon != null)
                {
                    trayIcon.Visible = false;
                    trayIcon.Dispose();
                    trayIcon = null;
                }
            }
            catch { }

            try
            {
                if (listener != null)
                {
                    listener.Stop();
                    listener.Close();
                    listener = null;
                }
            }
            catch { }

            try
            {
                if (File.Exists(portFilePath))
                {
                    File.Delete(portFilePath);
                }
            }
            catch { }

            try
            {
                if (appMutex != null)
                {
                    appMutex.ReleaseMutex();
                    appMutex.Dispose();
                    appMutex = null;
                }
            }
            catch { }
        }

        private static void LaunchNativeWindow(int targetPort)
        {
            string edgePath1 = @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe";
            string edgePath2 = @"C:\Program Files\Microsoft\Edge\Application\msedge.exe";
            string chromePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe";

            string targetBrowser = null;
            if (File.Exists(edgePath1)) targetBrowser = edgePath1;
            else if (File.Exists(edgePath2)) targetBrowser = edgePath2;
            else if (File.Exists(chromePath)) targetBrowser = chromePath;

            string appUrl = string.Format("http://127.0.0.1:{0}/", targetPort);

            try
            {
                if (targetBrowser != null)
                {
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = targetBrowser;
                    psi.Arguments = string.Format("--app=\"{0}\" --window-size=1120,820", appUrl);
                    psi.UseShellExecute = true;
                    Process.Start(psi);
                }
                else
                {
                    Process.Start(appUrl);
                }
            }
            catch
            {
                try { Process.Start(appUrl); } catch { }
            }
        }

        private static void HandleRequest(HttpListenerContext ctx)
        {
            HttpListenerRequest req = ctx.Request;
            HttpListenerResponse res = ctx.Response;

            // Security Check 1: Reject any non-loopback connections (LAN/External protection)
            if (!IPAddress.IsLoopback(req.RemoteEndPoint.Address))
            {
                res.StatusCode = 403;
                res.StatusDescription = "Forbidden: Localhost Only";
                res.Close();
                return;
            }

            // Security Check 2: DNS Rebinding Protection (validate Host header)
            string hostHeader = req.Headers["Host"];
            if (!string.IsNullOrEmpty(hostHeader))
            {
                string hostName = hostHeader.Split(':')[0].Trim().ToLowerInvariant();
                if (hostName != "localhost" && hostName != "127.0.0.1" && hostName != "::1" && hostName != "[::1]")
                {
                    res.StatusCode = 403;
                    res.StatusDescription = "Forbidden: Invalid Host Header";
                    res.Close();
                    return;
                }
            }

            // Security Check 3: Browser Security Headers
            res.Headers.Add("X-Content-Type-Options", "nosniff");
            res.Headers.Add("X-Frame-Options", "SAMEORIGIN");
            res.Headers.Add("X-XSS-Protection", "1; mode=block");
            res.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");

            // Security Check 4: Full CORS & Chrome Private Network Access (PNA) Support
            string origin = req.Headers["Origin"];
            if (!string.IsNullOrEmpty(origin))
            {
                res.Headers.Add("Access-Control-Allow-Origin", origin);
                res.Headers.Add("Access-Control-Allow-Credentials", "true");
            }
            else
            {
                res.Headers.Add("Access-Control-Allow-Origin", "*");
            }

            res.Headers.Add("Access-Control-Allow-Private-Network", "true");
            res.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS, HEAD");
            res.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization, X-Requested-With, Range, Accept, Cache-Control, Pragma, Access-Control-Request-Private-Network");
            res.Headers.Add("Access-Control-Max-Age", "86400");

            if (req.HttpMethod == "OPTIONS")
            {
                res.StatusCode = 200;
                res.Close();
                return;
            }

            string rawUrl = req.Url.AbsolutePath;
            bool isHead = (req.HttpMethod == "HEAD");

            try
            {
                // Engine Health & Status Check
                if (rawUrl == "/api/v1/health" && req.HttpMethod == "GET")
                {
                    hasRecentBrowserHealthPing = true;
                    SendJson(res, new {
                        success = true,
                        engine = "180Hz Downloader " + GetLocalAppVersion(),
                        version = GetLocalAppVersion(),
                        status = depStatusMessage,
                        isReady = File.Exists(ytdlpPath) && File.Exists(ffmpegPath),
                        isUpdating = isUpdatingYtDlp,
                        isApplyingAppUpdate = isApplyingAppUpdate,
                        appUpdateProgress = appUpdateProgress,
                        appUpdateStage = appUpdateStage,
                        port = port,
                        saveDir = saveDir,
                        isSaveDirConfigured = isCustomSaveDirConfigured
                    });
                    return;
                }
                else if (rawUrl == "/api/v1/engine/update" && req.HttpMethod == "POST")
                {
                    CheckUpdateYtDlp();
                    SendJson(res, new {
                        success = true,
                        message = "Engine update check initiated in background."
                    });
                    return;
                }
                // Serve Frontend
                if (rawUrl == "/" || rawUrl == "/index.html")
                {
                    ServeFile(res, GetWebFilePath("index.html"), "text/html; charset=utf-8", isHead);
                }
                else if (rawUrl == "/styles.css" || rawUrl == "/style.css")
                {
                    ServeFile(res, GetWebFilePath("styles.css"), "text/css; charset=utf-8", isHead);
                }
                else if (rawUrl == "/app.js")
                {
                    ServeFile(res, GetWebFilePath("app.js"), "application/javascript; charset=utf-8", isHead);
                }
                else if (rawUrl == "/version.json")
                {
                    ServeFile(res, GetWebFilePath("version.json"), "application/json; charset=utf-8", isHead);
                }
                else if (rawUrl == "/click.mp3")
                {
                    ServeFile(res, GetWebFilePath("click.mp3"), "audio/mpeg", isHead);
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
                else if (rawUrl.StartsWith("/api/v1/downloads/") && (req.HttpMethod == "GET" || req.HttpMethod == "HEAD"))
                {
                    // Check if requesting file or job status
                    if (rawUrl.EndsWith("/file"))
                    {
                        string id = ExtractIdFromUrl(rawUrl, "/api/v1/downloads/", "/file");
                        HandleDownloadFile(res, id, isHead);
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
                else if (rawUrl == "/api/v1/system/save-folder" && req.HttpMethod == "GET")
                {
                    SendJson(res, new {
                        success = true,
                        path = saveDir,
                        isConfigured = isCustomSaveDirConfigured
                    });
                    return;
                }
                else if (rawUrl == "/api/v1/system/save-folder" && req.HttpMethod == "POST")
                {
                    string body = ReadBody(req);
                    try
                    {
                        var dict = json.Deserialize<Dictionary<string, object>>(body);
                        if (dict != null && dict.ContainsKey("path"))
                        {
                            string customPath = Convert.ToString(dict["path"]).Trim();
                            if (SaveCustomFolderConfig(customPath))
                            {
                                SendJson(res, new {
                                    success = true,
                                    path = saveDir,
                                    isConfigured = isCustomSaveDirConfigured
                                });
                                return;
                            }
                        }
                    }
                    catch { }
                    res.StatusCode = 400;
                    SendJson(res, new { success = false, message = "Invalid or inaccessible folder path." });
                    return;
                }
                else if (rawUrl == "/api/v1/system/select-folder" && req.HttpMethod == "POST")
                {
                    string chosen = PromptUserForSaveFolder();
                    SendJson(res, new {
                        success = true,
                        selected = !string.IsNullOrEmpty(chosen),
                        path = saveDir,
                        isConfigured = isCustomSaveDirConfigured
                    });
                    return;
                }
                else if (rawUrl == "/api/v1/system/open-save-folder" && req.HttpMethod == "POST")
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(saveDir) && Directory.Exists(saveDir))
                        {
                            Process.Start("explorer.exe", saveDir);
                        }
                    }
                    catch { }
                    SendJson(res, new {
                        success = true,
                        path = saveDir,
                        isConfigured = isCustomSaveDirConfigured
                    });
                    return;
                }
                else if (rawUrl == "/api/v1/system/open-url" && req.HttpMethod == "POST")
                {
                    string body = ReadBody(req);
                    try
                    {
                        var ser = new JavaScriptSerializer();
                        var dict = ser.Deserialize<Dictionary<string, object>>(body);
                        if (dict != null && dict.ContainsKey("url"))
                        {
                            string targetUrl = dict["url"].ToString();
                            if (targetUrl.StartsWith("http://") || targetUrl.StartsWith("https://"))
                            {
                                ProcessStartInfo psi = new ProcessStartInfo();
                                psi.FileName = targetUrl;
                                psi.UseShellExecute = true;
                                Process.Start(psi);
                            }
                        }
                    }
                    catch { }
                    SendJson(res, new { success = true });
                    return;
                }
                else if (rawUrl == "/api/v1/system/select-cookies-file" && req.HttpMethod == "POST")
                {
                    string chosen = PromptUserForCookiesFile();
                    SendJson(res, new {
                        success = !string.IsNullOrEmpty(chosen),
                        filePath = chosen ?? "",
                        message = !string.IsNullOrEmpty(chosen) ? "Cookies imported successfully." : "Cookie selection cancelled."
                    });
                    return;
                }
                else if (rawUrl == "/api/v1/system/save-cookies-text" && req.HttpMethod == "POST")
                {
                    string body = ReadBody(req);
                    bool ok = false;
                    try
                    {
                        var ser = new JavaScriptSerializer();
                        var dict = ser.Deserialize<Dictionary<string, object>>(body);
                        if (dict != null && dict.ContainsKey("content"))
                        {
                            ok = SaveCookiesContent(dict["content"].ToString());
                        }
                    }
                    catch { }
                    SendJson(res, new {
                        success = ok,
                        message = ok ? "Session cookies saved successfully." : "Failed to save cookies."
                    });
                    return;
                }
                else if (rawUrl == "/api/v1/system/cookies-status" && req.HttpMethod == "GET")
                {
                    string cookiesFile = ResolveUserPlatformCookies("", saveDir);
                    bool exists = !string.IsNullOrEmpty(cookiesFile) && File.Exists(cookiesFile);
                    bool hasInstagram = false;
                    bool hasYouTube = false;
                    bool hasFacebook = false;
                    long sizeBytes = 0;
                    if (exists)
                    {
                        try
                        {
                            FileInfo fi = new FileInfo(cookiesFile);
                            sizeBytes = fi.Length;
                            string txt = File.ReadAllText(cookiesFile);
                            hasInstagram = txt.IndexOf("sessionid", StringComparison.OrdinalIgnoreCase) >= 0 || txt.IndexOf(".instagram.com", StringComparison.OrdinalIgnoreCase) >= 0;
                            hasYouTube = txt.IndexOf("SAPISID", StringComparison.OrdinalIgnoreCase) >= 0 || txt.IndexOf("LOGIN_INFO", StringComparison.OrdinalIgnoreCase) >= 0;
                            hasFacebook = txt.IndexOf("c_user", StringComparison.OrdinalIgnoreCase) >= 0;
                        }
                        catch { }
                    }
                    SendJson(res, new {
                        success = true,
                        hasCookies = exists,
                        path = cookiesFile ?? "",
                        sizeBytes = sizeBytes,
                        hasInstagram = hasInstagram,
                        hasYouTube = hasYouTube,
                        hasFacebook = hasFacebook
                    });
                    return;
                }
                else if ((rawUrl == "/api/v1/system/check-update" || rawUrl.StartsWith("/api/v1/system/check-update?")) && (req.HttpMethod == "GET" || req.HttpMethod == "POST"))
                {
                    HandleCheckUpdate(res);
                }
                else if (rawUrl == "/api/v1/system/update-status" && req.HttpMethod == "GET")
                {
                    SendJson(res, new {
                        success = true,
                        isUpdating = isApplyingAppUpdate,
                        progress = appUpdateProgress,
                        stage = appUpdateStage,
                        error = appUpdateError,
                        currentVersion = GetLocalAppVersion()
                    });
                    return;
                }
                else if (rawUrl == "/api/v1/system/apply-update" && req.HttpMethod == "POST")
                {
                    HandleApplyUpdate(res);
                }
                else if (rawUrl == "/api/v1/system/exit" && req.HttpMethod == "POST")
                {
                    SendJson(res, new { success = true, message = "Shutting down 180Hz Downloader" });
                    Task.Factory.StartNew(() =>
                    {
                        Thread.Sleep(400);
                        CleanUp();
                        Environment.Exit(0);
                    });
                }
                else
                {
                    // Check if file exists in web/docs folder
                    string localPath = GetWebFilePath(rawUrl.TrimStart('/'));
                    if (File.Exists(localPath))
                    {
                        ServeFile(res, localPath, GetMimeType(localPath), isHead);
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

        private static string GetWebFilePath(string relativePath)
        {
            string p1 = Path.Combine(appDir, "web", relativePath);
            if (File.Exists(p1)) return p1;
            string p2 = Path.Combine(Environment.CurrentDirectory, "web", relativePath);
            if (File.Exists(p2)) return p2;
            string p3 = Path.Combine(appDir, "docs", relativePath);
            if (File.Exists(p3)) return p3;
            string p4 = Path.Combine(appDir, relativePath);
            if (File.Exists(p4)) return p4;
            return p1;
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

        private static void ServeFile(HttpListenerResponse res, string path, string contentType, bool isHead = false)
        {
            if (File.Exists(path))
            {
                byte[] data = File.ReadAllBytes(path);
                res.ContentType = contentType;
                res.ContentLength64 = data.Length;
                if (!isHead)
                {
                    res.OutputStream.Write(data, 0, data.Length);
                }
                res.Close();
                return;
            }

            try
            {
                string fileName = Path.GetFileName(path);
                if (!string.IsNullOrEmpty(fileName))
                {
                    using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fileName))
                    {
                        if (stream != null)
                        {
                            using (MemoryStream ms = new MemoryStream())
                            {
                                stream.CopyTo(ms);
                                byte[] data = ms.ToArray();
                                res.ContentType = contentType;
                                res.ContentLength64 = data.Length;
                                if (!isHead)
                                {
                                    res.OutputStream.Write(data, 0, data.Length);
                                }
                                res.Close();
                                return;
                            }
                        }
                    }
                }
            }
            catch { }

            res.StatusCode = 404;
            res.Close();
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
                case ".mp4": return "video/mp4";
                case ".webm": return "video/webm";
                case ".mkv": return "video/x-matroska";
                case ".mp3": return "audio/mpeg";
                case ".m4a": return "audio/mp4";
                case ".opus": return "audio/opus";
                case ".wav": return "audio/wav";
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
            string url = reqObj.ContainsKey("url") ? Convert.ToString(reqObj["url"]).Trim() : "";

            if (string.IsNullOrEmpty(url))
            {
                res.StatusCode = 400;
                SendJson(res, new { success = false, message = "URL is required" });
                return;
            }

            Uri parsedUri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out parsedUri) || 
                (parsedUri.Scheme != Uri.UriSchemeHttp && parsedUri.Scheme != Uri.UriSchemeHttps))
            {
                res.StatusCode = 400;
                SendJson(res, new { success = false, message = "Invalid URL protocol. Only HTTP and HTTPS URLs are permitted." });
                return;
            }

            // Command injection defense: escape quotes and strip control characters
            string safeUrl = url.Replace("\"", "%22").Replace("\r", "").Replace("\n", "");

            string ytdlp = File.Exists(ytdlpPath) ? ytdlpPath : Path.Combine(appDir, "yt-dlp.exe");
            if (!File.Exists(ytdlp))
            {
                EnsureDependencies();
                res.StatusCode = 503;
                SendJson(res, new { success = false, message = "Engine components are initializing (" + depStatusMessage + "). Please retry in 5 seconds." });
                return;
            }

            bool isYouTube = safeUrl.IndexOf("youtube.com", StringComparison.OrdinalIgnoreCase) >= 0 || safeUrl.IndexOf("youtu.be", StringComparison.OrdinalIgnoreCase) >= 0;
            string np = GetNodePath();
            string jsRuntimeArg = !string.IsNullOrEmpty(np) ? string.Format("--js-runtimes node:\"{0}\"", np) : "";
            string userAgentArg = "--user-agent \"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36\"";
            string extArgs = userAgentArg;
            if (safeUrl.IndexOf("facebook.com", StringComparison.OrdinalIgnoreCase) >= 0 || safeUrl.IndexOf("fb.watch", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                extArgs += " --add-header \"Accept-Language:en-US,en;q=0.9\"";
            }
            if (safeUrl.IndexOf("instagram.com", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                extArgs += " --add-header \"Accept:text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8\"";
            }
            string cookiesPath = ResolveUserPlatformCookies(safeUrl, saveDir);
            if (string.IsNullOrEmpty(cookiesPath) && isYouTube)
            {
                cookiesPath = EnsureYouTubeVisitorCookies();
            }
            string cookiesArg = (!string.IsNullOrEmpty(cookiesPath) && File.Exists(cookiesPath)) ? string.Format("--cookies \"{0}\"", cookiesPath) : "";

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = ytdlp;
            psi.Arguments = string.Format("{0} {1} {2} --dump-json --no-playlist --no-warnings --socket-timeout 20 \"{3}\"", jsRuntimeArg, extArgs, cookiesArg, safeUrl);
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.StandardOutputEncoding = Encoding.UTF8;

            try
            {
                using (Process p = Process.Start(psi))
                {
                    Task<string> outTask = Task.Factory.StartNew(() => p.StandardOutput.ReadToEnd());
                    Task<string> errTask = Task.Factory.StartNew(() => p.StandardError.ReadToEnd());
                    if (!p.WaitForExit(25000))
                    {
                        try { p.Kill(); } catch { }
                    }
                    string output = "";
                    try { output = outTask.Wait(4000) ? outTask.Result : ""; } catch { }
                    string err = "";
                    try { err = errTask.Wait(1000) ? errTask.Result : ""; } catch { }

                    // If YouTube analysis returned empty (e.g. rate limit or stale session), retry with fresh visitor cookies and tv/web fallback
                    if (string.IsNullOrEmpty(output) && isYouTube)
                    {
                        try
                        {
                            string freshCookies = EnsureYouTubeVisitorCookies(true);
                            string freshCookiesArg = (freshCookies != null && File.Exists(freshCookies)) ? string.Format("--cookies \"{0}\"", freshCookies) : "";
                            psi.Arguments = string.Format("{0} {1} --extractor-args \"youtube:player_client=tv,tv_embedded,web\" {2} --dump-json --no-playlist --no-warnings --socket-timeout 20 \"{3}\"", jsRuntimeArg, userAgentArg, freshCookiesArg, safeUrl);
                            using (Process rp = Process.Start(psi))
                            {
                                Task<string> rOut = Task.Factory.StartNew(() => rp.StandardOutput.ReadToEnd());
                                if (rp.WaitForExit(20000))
                                {
                                    output = rOut.Wait(3000) ? rOut.Result : "";
                                }
                            }
                        }
                        catch { }
                    }

                    if (string.IsNullOrEmpty(output))
                    {
                        string inferredTitle = "Media Stream";
                        try
                        {
                            Match m = Regex.Match(safeUrl, @"(?:reel|reels|p|tv|shorts)/([A-Za-z0-9_\-]+)");
                            if (!m.Success)
                            {
                                m = Regex.Match(safeUrl, @"(?:watch\?v=)([A-Za-z0-9_\-]+)");
                            }
                            if (m.Success && !string.IsNullOrEmpty(m.Groups[1].Value))
                            {
                                string code = m.Groups[1].Value;
                                inferredTitle = safeUrl.IndexOf("instagram.com", StringComparison.OrdinalIgnoreCase) >= 0
                                    ? "Instagram Reel (" + code + ")"
                                    : "Media Stream (" + code + ")";
                            }
                        }
                        catch { }

                        SendJson(res, new {
                            success = true,
                            data = new {
                                title = inferredTitle,
                                creator = "",
                                thumbnailUrl = "",
                                durationSeconds = 0,
                                platformMediaId = "",
                                videoFormats = GetDefaultVideoFormats(),
                                audioFormats = GetDefaultAudioFormats()
                            }
                        });
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

                            if (ext == "mhtml" || formatId.StartsWith("sb") || formatId.IndexOf("storyboard", StringComparison.OrdinalIgnoreCase) >= 0) continue;
                            if ((string.IsNullOrEmpty(vcodec) || vcodec == "none") && (string.IsNullOrEmpty(acodec) || acodec == "none")) continue;

                            long filesize = 0;
                            if (f.ContainsKey("filesize") && f["filesize"] != null) long.TryParse(Convert.ToString(f["filesize"]), out filesize);
                            else if (f.ContainsKey("filesize_approx") && f["filesize_approx"] != null) long.TryParse(Convert.ToString(f["filesize_approx"]), out filesize);

                            int width = 0;
                            if (f.ContainsKey("width") && f["width"] != null) int.TryParse(Convert.ToString(f["width"]), out width);

                            int height = 0;
                            if (f.ContainsKey("height") && f["height"] != null) int.TryParse(Convert.ToString(f["height"]), out height);

                            double fps = 0;
                            if (f.ContainsKey("fps") && f["fps"] != null) double.TryParse(Convert.ToString(f["fps"]), out fps);

                            // Video Streams (support YouTube, Instagram, Facebook hd/sd, TikTok, etc.)
                            bool isVideo = (vcodec != "none" && !string.IsNullOrEmpty(vcodec)) ||
                                           height > 0 || width > 0 ||
                                           formatId.IndexOf("hd", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                           formatId.IndexOf("sd", StringComparison.OrdinalIgnoreCase) >= 0;

                            if (isVideo)
                            {
                                // Infer height if 0 (e.g. Facebook "hd" / "sd")
                                if (height == 0)
                                {
                                    if (formatId.IndexOf("hd", StringComparison.OrdinalIgnoreCase) >= 0 || width >= 1920) { height = 1080; if (width == 0) width = 1920; }
                                    else if (formatId.IndexOf("sd", StringComparison.OrdinalIgnoreCase) >= 0 || width >= 854) { height = 480; if (width == 0) width = 854; }
                                    else if (width >= 3840) height = 2160;
                                    else if (width >= 2560) height = 1440;
                                    else if (width >= 1280) height = 720;
                                    else if (width >= 640) height = 360;
                                }

                                int shortSide = (width > 0 && height > 0) ? Math.Min(width, height) : (height > 0 ? height : width);
                                int longSide = (width > 0 && height > 0) ? Math.Max(width, height) : (height > 0 ? height : width);

                                string resLabel = shortSide > 0 ? (shortSide + "p") : (height > 0 ? (height + "p") : formatId);
                                if (shortSide >= 4320 || longSide >= 7680) resLabel = "4320p / 8K";
                                else if (shortSide >= 2160 || longSide >= 3840) resLabel = "2160p / 4K";
                                else if (shortSide >= 1440 || longSide >= 2560) resLabel = "1440p / 2K";
                                else if (shortSide >= 1080 || longSide >= 1920) resLabel = "1080p / Full HD";
                                else if (shortSide >= 720 || longSide >= 1280) resLabel = "720p / HD";
                                else if (shortSide >= 480 || longSide >= 854) resLabel = "480p";
                                else if (shortSide >= 360 || longSide >= 640) resLabel = "360p";
                                else if (shortSide >= 240) resLabel = "240p";
                                else if (shortSide >= 144) resLabel = "144p";

                                bool isHdr = false;
                                if (f.ContainsKey("dynamic_range") && f["dynamic_range"] != null)
                                    isHdr = Convert.ToString(f["dynamic_range"]).IndexOf("HDR", StringComparison.OrdinalIgnoreCase) >= 0;

                                VideoFormatDto vdto = new VideoFormatDto();
                                vdto.formatId = formatId;
                                vdto.resolution = resLabel + (isHdr ? " HDR" : "");
                                vdto.width = width;
                                vdto.height = height;
                                vdto.fps = fps;
                                vdto.vcodec = vcodec;
                                vdto.container = ext;
                                vdto.filesizeFormatted = filesize > 0 ? FormatBytes(filesize) : null;
                                vdto.estimatedSizeBytes = filesize;
                                vdto.hdr = isHdr;

                                // Group by resolution tier and container (MP4 / WebM), keeping best FPS, HDR, or bitrate
                                string groupKey = resLabel + "_" + ext.ToLower();
                                if (!resolutionMap.ContainsKey(groupKey))
                                {
                                    resolutionMap[groupKey] = vdto;
                                }
                                else
                                {
                                    VideoFormatDto existing = resolutionMap[groupKey];
                                    if ((vdto.fps > existing.fps) || (vdto.hdr && !existing.hdr) || (vdto.estimatedSizeBytes > existing.estimatedSizeBytes))
                                    {
                                        resolutionMap[groupKey] = vdto;
                                    }
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

                        // Sort video formats descending by pixel count (8K -> 4K -> 2K -> 1080p -> 720p...)
                        videoFormats = new List<VideoFormatDto>(resolutionMap.Values);
                        videoFormats.Sort((a, b) =>
                        {
                            long aPixels = (long)a.width * (long)a.height;
                            long bPixels = (long)b.width * (long)b.height;
                            int cmp = bPixels.CompareTo(aPixels);
                            if (cmp != 0) return cmp;
                            cmp = b.height.CompareTo(a.height);
                            if (cmp != 0) return cmp;
                            cmp = b.fps.CompareTo(a.fps);
                            if (cmp != 0) return cmp;
                            return b.estimatedSizeBytes.CompareTo(a.estimatedSizeBytes);
                        });

                        // Sort audio formats descending
                        audioFormats.Sort((a, b) => b.bitrateKbps.CompareTo(a.bitrateKbps));
                    }

                    // Always ensure Maximum Quality format is prepended at index 0
                    VideoFormatDto maxDto = new VideoFormatDto();
                    maxDto.formatId = "best";
                    maxDto.resolution = videoFormats.Count > 0 ? string.Format("Maximum Quality ({0})", videoFormats[0].resolution) : "Maximum Quality (Best Available)";
                    maxDto.container = "mp4";
                    maxDto.fps = videoFormats.Count > 0 ? videoFormats[0].fps : 60;
                    maxDto.width = videoFormats.Count > 0 ? videoFormats[0].width : 3840;
                    maxDto.height = videoFormats.Count > 0 ? videoFormats[0].height : 2160;
                    videoFormats.Insert(0, maxDto);

                    // Always inject High-Quality MP3 formats for ALL platforms at the top of audioFormats
                    List<AudioFormatDto> mp3Tiers = new List<AudioFormatDto>();
                    mp3Tiers.Add(new AudioFormatDto {
                        formatId = "mp3-320k",
                        label = "320 kbps (Lossless MP3)",
                        bitrateKbps = 320,
                        codec = "MP3",
                        container = "mp3",
                        filesizeFormatted = durationSeconds > 0 ? FormatBytes((long)(durationSeconds * (320L * 1024L / 8L))) : "320k Lossless"
                    });
                    mp3Tiers.Add(new AudioFormatDto {
                        formatId = "mp3-192k",
                        label = "192 kbps (High Quality MP3)",
                        bitrateKbps = 192,
                        codec = "MP3",
                        container = "mp3",
                        filesizeFormatted = durationSeconds > 0 ? FormatBytes((long)(durationSeconds * (192L * 1024L / 8L))) : "192k High"
                    });
                    mp3Tiers.Add(new AudioFormatDto {
                        formatId = "mp3-128k",
                        label = "128 kbps (Standard MP3)",
                        bitrateKbps = 128,
                        codec = "MP3",
                        container = "mp3",
                        filesizeFormatted = durationSeconds > 0 ? FormatBytes((long)(durationSeconds * (128L * 1024L / 8L))) : "128k Standard"
                    });
                    mp3Tiers.Add(new AudioFormatDto {
                        formatId = "mp3-64k",
                        label = "64 kbps (Voice / Compact MP3)",
                        bitrateKbps = 64,
                        codec = "MP3",
                        container = "mp3",
                        filesizeFormatted = durationSeconds > 0 ? FormatBytes((long)(durationSeconds * (64L * 1024L / 8L))) : "64k Compact"
                    });

                    List<AudioFormatDto> finalAudio = new List<AudioFormatDto>(mp3Tiers);
                    foreach (var af in audioFormats)
                    {
                        if (af.container != "mp3")
                        {
                            finalAudio.Add(af);
                        }
                    }
                    audioFormats = finalAudio;

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

        private static List<VideoFormatDto> GetDefaultVideoFormats()
        {
            List<VideoFormatDto> list = new List<VideoFormatDto>();
            list.Add(new VideoFormatDto { formatId = "best", resolution = "Auto Max (4K/8K)", container = "mp4", fps = 60, vcodec = "H.264 / AV1", filesizeFormatted = "Source" });
            list.Add(new VideoFormatDto { formatId = "2160p", resolution = "2160p / 4K", container = "mp4", fps = 60, vcodec = "AV1 / VP9", width = 3840, height = 2160, filesizeFormatted = "High Bitrate" });
            list.Add(new VideoFormatDto { formatId = "2160p", resolution = "2160p / 4K", container = "webm", fps = 60, vcodec = "VP9", width = 3840, height = 2160, filesizeFormatted = "High Bitrate" });
            list.Add(new VideoFormatDto { formatId = "1440p", resolution = "1440p", container = "mp4", fps = 60, vcodec = "VP9 / AVC", width = 2560, height = 1440 });
            list.Add(new VideoFormatDto { formatId = "1440p", resolution = "1440p", container = "webm", fps = 60, vcodec = "VP9", width = 2560, height = 1440 });
            list.Add(new VideoFormatDto { formatId = "1080p", resolution = "1080p", container = "mp4", fps = 60, vcodec = "H.264", width = 1920, height = 1080 });
            list.Add(new VideoFormatDto { formatId = "1080p", resolution = "1080p", container = "webm", fps = 60, vcodec = "VP9", width = 1920, height = 1080 });
            list.Add(new VideoFormatDto { formatId = "720p", resolution = "720p", container = "mp4", fps = 30, vcodec = "H.264", width = 1280, height = 720 });
            list.Add(new VideoFormatDto { formatId = "720p", resolution = "720p", container = "webm", fps = 30, vcodec = "VP9", width = 1280, height = 720 });
            list.Add(new VideoFormatDto { formatId = "480p", resolution = "480p", container = "mp4", fps = 30, vcodec = "H.264", width = 854, height = 480 });
            list.Add(new VideoFormatDto { formatId = "360p", resolution = "360p", container = "mp4", fps = 30, vcodec = "H.264", width = 640, height = 360 });
            return list;
        }

        private static List<AudioFormatDto> GetDefaultAudioFormats()
        {
            List<AudioFormatDto> list = new List<AudioFormatDto>();
            list.Add(new AudioFormatDto { formatId = "mp3-320k", label = "320 kbps (Lossless MP3)", container = "mp3", bitrateKbps = 320, codec = "MP3", filesizeFormatted = "320k Lossless" });
            list.Add(new AudioFormatDto { formatId = "mp3-192k", label = "192 kbps (High Quality MP3)", container = "mp3", bitrateKbps = 192, codec = "MP3", filesizeFormatted = "192k High" });
            list.Add(new AudioFormatDto { formatId = "mp3-128k", label = "128 kbps (Standard MP3)", container = "mp3", bitrateKbps = 128, codec = "MP3", filesizeFormatted = "128k Standard" });
            list.Add(new AudioFormatDto { formatId = "mp3-64k", label = "64 kbps (Compact MP3)", container = "mp3", bitrateKbps = 64, codec = "MP3", filesizeFormatted = "64k Compact" });
            list.Add(new AudioFormatDto { formatId = "128k", label = "128 kbps (Source AAC)", container = "m4a", bitrateKbps = 128, codec = "AAC", filesizeFormatted = "128k" });
            list.Add(new AudioFormatDto { formatId = "opus", label = "160 kbps (Source Opus)", container = "webm", bitrateKbps = 160, codec = "Opus", filesizeFormatted = "160k" });
            return list;
        }

        // ==========================================
        //  API: /api/v1/downloads
        // ==========================================
        private static void HandleCreateDownload(HttpListenerResponse res, string body)
        {
            Dictionary<string, object> reqObj = json.Deserialize<Dictionary<string, object>>(body);
            string url = reqObj.ContainsKey("url") ? Convert.ToString(reqObj["url"]).Trim() : "";
            string formatId = reqObj.ContainsKey("formatId") ? Convert.ToString(reqObj["formatId"]) : "best";
            string container = reqObj.ContainsKey("container") ? Convert.ToString(reqObj["container"]) : "mp4";
            string quality = reqObj.ContainsKey("quality") ? Convert.ToString(reqObj["quality"]) : "best";
            bool audioOnly = reqObj.ContainsKey("audioOnly") && Convert.ToBoolean(reqObj["audioOnly"]);

            if (string.IsNullOrEmpty(url))
            {
                res.StatusCode = 400;
                SendJson(res, new { success = false, message = "URL is required" });
                return;
            }

            Uri parsedUri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out parsedUri) || 
                (parsedUri.Scheme != Uri.UriSchemeHttp && parsedUri.Scheme != Uri.UriSchemeHttps))
            {
                res.StatusCode = 400;
                SendJson(res, new { success = false, message = "Invalid URL protocol. Only HTTP and HTTPS URLs are permitted." });
                return;
            }

            if (!isCustomSaveDirConfigured)
            {
                string chosen = PromptUserForSaveFolder();
                if (!string.IsNullOrEmpty(chosen))
                {
                    saveDir = chosen;
                    isCustomSaveDirConfigured = true;
                    SaveCustomFolderConfig(chosen);
                }
                else
                {
                    if (string.IsNullOrEmpty(saveDir) || !Directory.Exists(saveDir))
                    {
                        saveDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "180Hz Downloads");
                        try { Directory.CreateDirectory(saveDir); } catch { }
                    }
                }
            }

            bool isAudioReq = audioOnly || container == "mp3" || (formatId ?? "").StartsWith("mp3") || (formatId ?? "") == "bestaudio";
            string effectiveContainer = isAudioReq ? "mp3" : container;

            string jobId = "job_" + Guid.NewGuid().ToString("N").Substring(0, 8);

            DownloadJob job = new DownloadJob();
            job.id = jobId;
            job.url = url;
            job.status = "STARTING";
            job.progressPercent = 0;
            job.formatId = formatId;
            job.audioOnly = isAudioReq;
            job.requestedFormat = effectiveContainer;
            job.downloadUrl = string.Format("/api/v1/downloads/{0}/file", jobId);
            jobs[jobId] = job;

            Task.Factory.StartNew(() => RunDownloadProcess(job, formatId, container, quality, audioOnly));

            SendJson(res, new {
                success = true,
                saveDir = saveDir,
                isConfigured = isCustomSaveDirConfigured,
                data = new { id = jobId, saveDir = saveDir }
            });
        }

        private static void RunDownloadProcess(DownloadJob job, string formatId, string container, string quality, bool audioOnly)
        {
            string ytdlp = File.Exists(ytdlpPath) ? ytdlpPath : Path.Combine(appDir, "yt-dlp.exe");
            string ffmpeg = File.Exists(ffmpegPath) ? ffmpegPath : Path.Combine(appDir, "ffmpeg.exe");

            if (!File.Exists(ytdlp) || !File.Exists(ffmpeg))
            {
                EnsureDependencies();
            }

            string safeFormatId = (formatId ?? "").Trim();
            if (safeFormatId.StartsWith("-") || !Regex.IsMatch(safeFormatId, @"^[a-zA-Z0-9_\-\.\:\/]+$"))
                safeFormatId = "best";

            string safeContainer = (container ?? "mp4").Trim().ToLowerInvariant();
            if (!Regex.IsMatch(safeContainer, @"^[a-zA-Z0-9]+$")) safeContainer = "mp4";

            string safeUrl = job.url.Replace("\"", "%22").Replace("\r", "").Replace("\n", "");
            bool isFacebook = safeUrl.IndexOf("facebook.com", StringComparison.OrdinalIgnoreCase) >= 0 || safeUrl.IndexOf("fb.watch", StringComparison.OrdinalIgnoreCase) >= 0;
            bool isInstagram = safeUrl.IndexOf("instagram.com", StringComparison.OrdinalIgnoreCase) >= 0;
            bool isSocial = isInstagram || isFacebook || safeUrl.IndexOf("tiktok.com", StringComparison.OrdinalIgnoreCase) >= 0 || safeUrl.IndexOf("twitter.com", StringComparison.OrdinalIgnoreCase) >= 0 || safeUrl.IndexOf("x.com", StringComparison.OrdinalIgnoreCase) >= 0;

            string formatArg;
            string mergeArg;

            bool isWebM = string.Equals(safeContainer, "webm", StringComparison.OrdinalIgnoreCase) || safeFormatId.EndsWith("-webm", StringComparison.OrdinalIgnoreCase);

            bool isAudio = audioOnly || safeContainer == "mp3" || safeContainer == "m4a" || safeContainer == "opus" ||
                           safeFormatId.StartsWith("mp3") || safeFormatId == "bestaudio" || safeFormatId == "192k" || 
                           safeFormatId == "128k" || safeFormatId == "64k" || safeFormatId == "opus";

            if (isAudio)
            {
                if (string.Equals(safeContainer, "m4a", StringComparison.OrdinalIgnoreCase))
                {
                    formatArg = "-f \"bestaudio/best\" -x --audio-format m4a --audio-quality 0";
                }
                else if (string.Equals(safeContainer, "webm", StringComparison.OrdinalIgnoreCase) || string.Equals(safeContainer, "opus", StringComparison.OrdinalIgnoreCase))
                {
                    formatArg = "-f \"bestaudio/best\" -x --audio-format opus";
                }
                else
                {
                    safeContainer = "mp3";
                    job.requestedFormat = "mp3";
                    if (safeFormatId == "mp3-320k" || safeFormatId == "320k" || safeFormatId == "bestaudio")
                    {
                        formatArg = "-f \"bestaudio/best\" -x --audio-format mp3 --audio-quality 320K";
                    }
                    else if (safeFormatId == "mp3-192k" || safeFormatId == "192k")
                    {
                        formatArg = "-f \"bestaudio/best\" -x --audio-format mp3 --audio-quality 192K";
                    }
                    else if (safeFormatId == "mp3-128k" || safeFormatId == "128k")
                    {
                        formatArg = "-f \"bestaudio/best\" -x --audio-format mp3 --audio-quality 128K";
                    }
                    else if (safeFormatId == "mp3-64k" || safeFormatId == "64k")
                    {
                        formatArg = "-f \"bestaudio/best\" -x --audio-format mp3 --audio-quality 64K";
                    }
                    else
                    {
                        formatArg = "-f \"bestaudio/best\" -x --audio-format mp3 --audio-quality 320K";
                    }
                }
                mergeArg = "";
            }
            else
            {
                int targetHeight = 0;
                Match hm1 = Regex.Match(safeFormatId, @"^(\d+)p");
                if (hm1.Success) targetHeight = int.Parse(hm1.Groups[1].Value);
                if (targetHeight == 0 && !string.IsNullOrEmpty(quality))
                {
                    Match hm2 = Regex.Match(quality, @"(\d+)p");
                    if (hm2.Success) targetHeight = int.Parse(hm2.Groups[1].Value);
                }

                if (isInstagram || isFacebook)
                {
                    // Instagram & Facebook: Pure Raw Max Quality Pipeline (4K / 8K DASH + Best Audio merged into MP4)
                    if (string.IsNullOrEmpty(safeFormatId) || safeFormatId == "best" || safeFormatId == "max" || safeFormatId.IndexOf("hd", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        formatArg = "-f \"bv*+ba/b\" -S \"res,fps,vcodec:hevc:av01:h264,br,acodec:m4a\"";
                        mergeArg = "--merge-output-format mp4";
                    }
                    else if (safeFormatId.IndexOf("sd", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        formatArg = "-f \"bv*[height<=480]+ba/bv*[width<=480]+ba/best[format_id*=sd][height<=480]/best[height<=480]/best\" -S \"res:480,acodec:m4a\"";
                        mergeArg = "--merge-output-format mp4";
                    }
                    else if (targetHeight >= 2160 || safeFormatId.IndexOf("2160", StringComparison.OrdinalIgnoreCase) >= 0 || safeFormatId.IndexOf("4k", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        // 4K tier (supports both horizontal 3840x2160 and vertical 2160x3840 Reels)
                        formatArg = "-f \"bv*[height>=2160]+ba/bv*[width>=2160]+ba/bv*[height<=2160]+ba/bv*[width<=2160]+ba/bv*+ba/b\" -S \"res:2160,fps,vcodec:hevc:av01:h264,br,acodec:m4a\"";
                        mergeArg = "--merge-output-format mp4";
                    }
                    else if (targetHeight >= 1440 || safeFormatId.IndexOf("1440", StringComparison.OrdinalIgnoreCase) >= 0 || safeFormatId.IndexOf("2k", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        // 2K tier (supports both horizontal 2560x1440 and vertical 1440x2560 Reels)
                        formatArg = "-f \"bv*[height>=1440]+ba/bv*[width>=1440]+ba/bv*[height<=1440]+ba/bv*[width<=1440]+ba/bv*+ba/b\" -S \"res:1440,fps,vcodec:hevc:av01:h264,br,acodec:m4a\"";
                        mergeArg = "--merge-output-format mp4";
                    }
                    else if (targetHeight >= 1080 || safeFormatId.IndexOf("1080", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        // 1080p Full HD tier
                        formatArg = "-f \"bv*[height<=1080][height>=720]+ba/bv*[width<=1080][width>=720]+ba/bv*[height<=1080]+ba/bv*[width<=1080]+ba/bv*+ba/b\" -S \"res:1080,fps,br,acodec:m4a\"";
                        mergeArg = "--merge-output-format mp4";
                    }
                    else if (targetHeight >= 720 || safeFormatId.IndexOf("720", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        // 720p HD tier
                        formatArg = "-f \"bv*[height<=720][height>=480]+ba/bv*[width<=720][width>=480]+ba/bv*[height<=720]+ba/bv*[width<=720]+ba/bv*+ba/b\" -S \"res:720,fps,br,acodec:m4a\"";
                        mergeArg = "--merge-output-format mp4";
                    }
                    else if (targetHeight > 0)
                    {
                        formatArg = string.Format("-f \"bv*[height<={0}]+ba/bv*[width<={0}]+ba/b[height<={0}]/b[width<={0}]/bv*+ba/b\" -S \"res:{0},acodec:m4a\"", targetHeight);
                        mergeArg = "--merge-output-format mp4";
                    }
                    else
                    {
                        // Specific DASH format ID requested from analyze list
                        formatArg = string.Format("-f \"{0}[acodec!=none]/{0}+bestaudio[ext=m4a]/{0}+ba/bv*+ba/b\" -S \"res,acodec:m4a\"", safeFormatId);
                        mergeArg = "--merge-output-format mp4";
                    }
                }
                else if (isSocial && (string.IsNullOrEmpty(safeFormatId) || safeFormatId == "best"))
                {
                    formatArg = "-f \"best[ext=mp4]/bestvideo[ext=mp4]+bestaudio[ext=m4a]/bestvideo+bestaudio/best\" -S \"res,acodec:m4a\"";
                    mergeArg = "--merge-output-format mp4";
                }
                else if (string.IsNullOrEmpty(safeFormatId) || safeFormatId == "best")
                {
                    if (isWebM)
                    {
                        formatArg = targetHeight > 0
                            ? string.Format("-f \"bv*[height<={0}][ext=webm]+ba[ext=webm]/bv*[width<={0}][ext=webm]+ba[ext=webm]/bv*[height<={0}]+ba/bv*[width<={0}]+ba/b[height<={0}]/bv*+ba/b\" -S \"res:{0},acodec:opus\"", targetHeight)
                            : "-f \"bv*[ext=webm]+ba[ext=webm]/bv*+ba/b\" -S \"res,acodec:opus\"";
                        mergeArg = "--merge-output-format webm";
                    }
                    else
                    {
                        formatArg = targetHeight > 0
                            ? string.Format("-f \"bv*[height<={0}]+ba/bv*[width<={0}]+ba/b[height<={0}]/bv*+ba/b\" -S \"res:{0},acodec:m4a\"", targetHeight)
                            : "-f \"bv*+ba/b\" -S \"res,acodec:m4a\"";
                        mergeArg = "--merge-output-format mp4";
                    }
                }
                else if (safeFormatId.EndsWith("p") && targetHeight > 0)
                {
                    if (isWebM)
                    {
                        formatArg = string.Format("-f \"bv*[height<={0}][ext=webm]+ba[ext=webm]/bv*[width<={0}][ext=webm]+ba[ext=webm]/bv*[height<={0}]+ba/bv*[width<={0}]+ba/b[height<={0}]/bv*+ba/b\" -S \"res:{0},acodec:opus\"", targetHeight);
                        mergeArg = "--merge-output-format webm";
                    }
                    else
                    {
                        formatArg = string.Format("-f \"bv*[height<={0}]+ba/bv*[width<={0}]+ba/b[height<={0}]/bv*+ba/b\" -S \"res:{0},acodec:m4a\"", targetHeight);
                        mergeArg = "--merge-output-format mp4";
                    }
                }
                else
                {
                    // Specific Format ID selected by user (e.g. 701, 401, 315, 299, 137, 18, 0, etc.)
                    if (isWebM)
                    {
                        formatArg = targetHeight > 0
                            ? string.Format("-f \"{0}[acodec!=none]/{0}+bestaudio[ext=webm]/{0}+bestaudio[acodec=opus]/{0}+ba/bv*[height<={1}]+ba/bv*[width<={1}]+ba/bv*+ba/b\" -S \"res:{1},acodec:opus\"", safeFormatId, targetHeight)
                            : string.Format("-f \"{0}[acodec!=none]/{0}+bestaudio[ext=webm]/{0}+bestaudio[acodec=opus]/{0}+ba/bv*+ba/b\" -S \"res,acodec:opus\"", safeFormatId);
                        mergeArg = "--merge-output-format webm";
                    }
                    else
                    {
                        formatArg = targetHeight > 0
                            ? string.Format("-f \"{0}[acodec!=none]/{0}+bestaudio[ext=m4a]/{0}+ba/bv*[height<={1}]+ba/bv*[width<={1}]+ba/bv*+ba/b\" -S \"res:{1},acodec:m4a\"", safeFormatId, targetHeight)
                            : string.Format("-f \"{0}[acodec!=none]/{0}+bestaudio[ext=m4a]/{0}+ba/bv*+ba/b\" -S \"res,acodec:m4a\"", safeFormatId);
                        mergeArg = "--merge-output-format mp4";
                    }
                }
            }

            bool isYouTube = safeUrl.IndexOf("youtube.com", StringComparison.OrdinalIgnoreCase) >= 0 || safeUrl.IndexOf("youtu.be", StringComparison.OrdinalIgnoreCase) >= 0;
            string effectiveSaveDir = GetEffectiveWritableSaveDir(saveDir);
            saveDir = effectiveSaveDir;
            string outTemplate = Path.Combine(effectiveSaveDir, "%(title)s.%(ext)s");
            string np = GetNodePath();
            string jsRuntimeArg = !string.IsNullOrEmpty(np) ? string.Format("--js-runtimes node:\"{0}\"", np) : "";
            string userAgentArg = "--user-agent \"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36\"";
            string extArgs = userAgentArg;
            if (isFacebook)
            {
                extArgs += " --add-header \"Accept-Language:en-US,en;q=0.9\"";
            }
            if (safeUrl.IndexOf("instagram.com", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                extArgs += " --add-header \"Accept:text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8\"";
            }
            string cookiesPath = ResolveUserPlatformCookies(safeUrl, effectiveSaveDir);
            if (string.IsNullOrEmpty(cookiesPath) && isYouTube)
            {
                cookiesPath = EnsureYouTubeVisitorCookies();
            }
            string cookiesArg = (!string.IsNullOrEmpty(cookiesPath) && File.Exists(cookiesPath)) ? string.Format("--cookies \"{0}\"", cookiesPath) : "";

            string faststartArg = !isAudio ? "--postprocessor-args \"ffmpeg:-movflags faststart\"" : "";

            string args = string.Format("{0} {1} {2} {3} {4} {5} --ffmpeg-location \"{6}\" --force-overwrites --newline --no-playlist --no-mtime --windows-filenames -o \"{7}\" \"{8}\"",
                jsRuntimeArg, extArgs, cookiesArg, formatArg, mergeArg, faststartArg, ffmpeg, outTemplate, safeUrl);

            job.status = "DOWNLOADING";

            try
            {
                StringBuilder errSb = new StringBuilder();
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = ytdlp;
                psi.Arguments = args;
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.StandardOutputEncoding = Encoding.UTF8;

                job.process = new Process();
                job.process.StartInfo = psi;
                job.process.ErrorDataReceived += (s, e) =>
                {
                    if (e.Data != null) errSb.AppendLine(e.Data);
                };

                job.process.Start();
                job.process.BeginErrorReadLine();

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

                    if (string.IsNullOrEmpty(job.filePath) || !File.Exists(job.filePath))
                    {
                        try
                        {
                            DirectoryInfo di = new DirectoryInfo(effectiveSaveDir);
                            FileInfo latest = di.GetFiles()
                                .Where(f => !f.Name.EndsWith(".part") && !f.Name.EndsWith(".ytdl") && !f.Name.EndsWith(".aria2"))
                                .OrderByDescending(f => f.LastWriteTimeUtc)
                                .FirstOrDefault();
                            if (latest != null && (DateTime.UtcNow - latest.LastWriteTimeUtc).TotalMinutes < 5)
                            {
                                job.filePath = latest.FullName;
                                job.title = Path.GetFileNameWithoutExtension(latest.FullName);
                            }
                        }
                        catch { }
                    }

                    // Guarantee universal MP4 output: if output file is .mkv while container requested is mp4, remux cleanly
                    if (!isAudio && safeContainer == "mp4" && !string.IsNullOrEmpty(job.filePath) && File.Exists(job.filePath) && job.filePath.EndsWith(".mkv", StringComparison.OrdinalIgnoreCase))
                    {
                        string targetMp4 = Path.ChangeExtension(job.filePath, ".mp4");
                        try
                        {
                            ProcessStartInfo rpsi = new ProcessStartInfo();
                            rpsi.FileName = ffmpeg;
                            rpsi.Arguments = string.Format("-i \"{0}\" -c copy -movflags faststart \"{1}\" -y", job.filePath, targetMp4);
                            rpsi.CreateNoWindow = true;
                            rpsi.UseShellExecute = false;
                            using (Process rp = Process.Start(rpsi))
                            {
                                rp.WaitForExit(30000);
                                if (rp.ExitCode == 0 && File.Exists(targetMp4))
                                {
                                    try { File.Delete(job.filePath); } catch { }
                                    job.filePath = targetMp4;
                                    job.title = Path.GetFileNameWithoutExtension(targetMp4);
                                }
                            }
                        }
                        catch { }
                    }

                    if (!string.IsNullOrEmpty(job.filePath) && File.Exists(job.filePath))
                    {
                        job.fileSizeBytes = new FileInfo(job.filePath).Length;
                    }
                }
                else
                {
                    string err = errSb.ToString();

                    // If file is locked by Windows (e.g. playing in media player) or permission denied (e.g. drive root), auto-retry with safe writable directory and timestamped suffix
                    if (err.IndexOf("Permission denied", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        err.IndexOf("used by another process", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        err.IndexOf("Errno 13", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        try
                        {
                            string retryDir = IsDirectoryWritable(effectiveSaveDir) ? effectiveSaveDir : GetDefaultDownloadsFolder();
                            saveDir = retryDir;
                            SaveCustomFolderConfig(retryDir);
                            string fallbackTemplate = Path.Combine(retryDir, "%(title)s (" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ").%(ext)s");
                            string retryArgs = string.Format("{0} {1} {2} {3} {4} {5} --ffmpeg-location \"{6}\" --force-overwrites --newline --no-playlist --no-mtime --windows-filenames -o \"{7}\" \"{8}\"",
                                jsRuntimeArg, extArgs, cookiesArg, formatArg, mergeArg, faststartArg, ffmpeg, fallbackTemplate, safeUrl);

                            errSb.Clear();
                            psi.Arguments = retryArgs;
                            job.process = new Process();
                            job.process.StartInfo = psi;
                            job.process.ErrorDataReceived += (s, e) => { if (e.Data != null) errSb.AppendLine(e.Data); };
                            job.process.Start();
                            job.process.BeginErrorReadLine();
                            string retryLine;
                            while ((retryLine = job.process.StandardOutput.ReadLine()) != null)
                            {
                                ParseDownloadOutput(job, retryLine);
                            }
                            job.process.WaitForExit();

                            if (job.process.ExitCode == 0)
                            {
                                job.status = "COMPLETED";
                                job.progressPercent = 100;
                                if (!string.IsNullOrEmpty(job.filePath) && File.Exists(job.filePath))
                                {
                                    job.fileSizeBytes = new FileInfo(job.filePath).Length;
                                }
                                return;
                            }
                        }
                        catch { }
                    }

                    // If YouTube anti-bot / HTTP 429 challenge triggered, retry with fresh visitor cookies and tv/web fallback
                    if (err.IndexOf("Sign in to confirm", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        err.IndexOf("bot", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        err.IndexOf("HTTP Error 429", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        err.IndexOf("PO Token", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        try
                        {
                            string freshCookies = EnsureYouTubeVisitorCookies(true);
                            string freshCookiesArg = (freshCookies != null && File.Exists(freshCookies)) ? string.Format("--cookies \"{0}\"", freshCookies) : cookiesArg;
                            string fallbackClientArgs = userAgentArg + " --extractor-args \"youtube:player_client=tv,tv_embedded,web\"";
                            string retryArgs = string.Format("{0} {1} {2} {3} {4} {5} --ffmpeg-location \"{6}\" --force-overwrites --newline --no-playlist --no-mtime --windows-filenames -o \"{7}\" \"{8}\"",
                                jsRuntimeArg, fallbackClientArgs, freshCookiesArg, formatArg, mergeArg, faststartArg, ffmpeg, outTemplate, safeUrl);

                            errSb.Clear();
                            psi.Arguments = retryArgs;
                            job.process = new Process();
                            job.process.StartInfo = psi;
                            job.process.ErrorDataReceived += (s, e) => { if (e.Data != null) errSb.AppendLine(e.Data); };
                            job.process.Start();
                            job.process.BeginErrorReadLine();
                            string retryLine;
                            while ((retryLine = job.process.StandardOutput.ReadLine()) != null)
                            {
                                ParseDownloadOutput(job, retryLine);
                            }
                            job.process.WaitForExit();

                            if (job.process.ExitCode == 0)
                            {
                                job.status = "COMPLETED";
                                job.progressPercent = 100;
                                if (!string.IsNullOrEmpty(job.filePath) && File.Exists(job.filePath))
                                {
                                    job.fileSizeBytes = new FileInfo(job.filePath).Length;
                                }
                                return;
                            }
                        }
                        catch { }
                    }

                    // If Instagram 18+ / audience restricted post, attempt auto-scan retry with any newly placed cookies.txt
                    if (isInstagram && (err.IndexOf("available to everyone", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                        err.IndexOf("certain audiences", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                        err.IndexOf("empty media response", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                        err.IndexOf("API is not granting access", StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        try
                        {
                            string freshCookies = ResolveUserPlatformCookies(safeUrl, effectiveSaveDir);
                            if (!string.IsNullOrEmpty(freshCookies) && File.Exists(freshCookies) && freshCookies != cookiesPath)
                            {
                                string freshCookiesArg = string.Format("--cookies \"{0}\"", freshCookies);
                                string retryArgs = string.Format("{0} {1} {2} {3} {4} {5} --ffmpeg-location \"{6}\" --force-overwrites --newline --no-playlist --no-mtime --windows-filenames -o \"{7}\" \"{8}\"",
                                    jsRuntimeArg, extArgs, freshCookiesArg, formatArg, mergeArg, faststartArg, ffmpeg, outTemplate, safeUrl);

                                errSb.Clear();
                                psi.Arguments = retryArgs;
                                job.process = new Process();
                                job.process.StartInfo = psi;
                                job.process.ErrorDataReceived += (s, e) => { if (e.Data != null) errSb.AppendLine(e.Data); };
                                job.process.Start();
                                job.process.BeginErrorReadLine();
                                string retryLine;
                                while ((retryLine = job.process.StandardOutput.ReadLine()) != null)
                                {
                                    ParseDownloadOutput(job, retryLine);
                                }
                                job.process.WaitForExit();

                                if (job.process.ExitCode == 0)
                                {
                                    job.status = "COMPLETED";
                                    job.progressPercent = 100;
                                    if (!string.IsNullOrEmpty(job.filePath) && File.Exists(job.filePath))
                                    {
                                        job.fileSizeBytes = new FileInfo(job.filePath).Length;
                                    }
                                    return;
                                }
                            }
                        }
                        catch { }

                        job.status = "FAILED";
                        job.errorCode = "AGE_RESTRICTED";
                        job.errorMessage = "This Instagram reel has 18+ audience restrictions set by the author. To download age-restricted reels directly in the app, please save your cookies.txt in your Downloads folder.";
                        return;
                    }

                    job.status = "FAILED";
                    job.errorMessage = "Download error: " + (string.IsNullOrEmpty(err) ? "Process exited with code " + job.process.ExitCode : err);
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
                int startIdx = line.IndexOf("into \"");
                if (startIdx >= 0)
                {
                    startIdx += 6;
                    int endIdx = line.LastIndexOf("\"");
                    if (endIdx > startIdx)
                    {
                        string fn = line.Substring(startIdx, endIdx - startIdx).Trim();
                        job.filePath = fn;
                        job.title = Path.GetFileNameWithoutExtension(fn);
                    }
                }
            }
            else if (line.Contains("[ExtractAudio] Destination:"))
            {
                string fn = line.Substring(line.IndexOf("Destination:") + 12).Trim();
                job.filePath = fn;
                job.title = Path.GetFileNameWithoutExtension(fn);
            }
            else if (line.Contains("[download] Destination:"))
            {
                string fn = line.Substring(line.IndexOf("Destination:") + 12).Trim();
                if (!fn.Contains(".f") || fn.EndsWith(".mp4") || fn.EndsWith(".webm") || fn.EndsWith(".mkv") || fn.EndsWith(".mp3"))
                {
                    job.filePath = fn;
                    job.title = Path.GetFileNameWithoutExtension(fn);
                }
            }
            else if (line.Contains("has already been downloaded"))
            {
                job.progressPercent = 100;
                job.status = "COMPLETED";
                string prefix = "[download] ";
                int pIdx = line.IndexOf(prefix);
                if (pIdx >= 0)
                {
                    int endIdx = line.IndexOf(" has already been downloaded");
                    if (endIdx > pIdx + prefix.Length)
                    {
                        string fn = line.Substring(pIdx + prefix.Length, endIdx - (pIdx + prefix.Length)).Trim();
                        job.filePath = fn;
                        job.title = Path.GetFileNameWithoutExtension(fn);
                    }
                }
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
        private static void HandleDownloadFile(HttpListenerResponse res, string id, bool isHead)
        {
            if (jobs.ContainsKey(id))
            {
                DownloadJob job = jobs[id];
                if (!string.IsNullOrEmpty(job.filePath) && File.Exists(job.filePath))
                {
                    string fullPath = Path.GetFullPath(job.filePath);
                    string fullSaveDir = Path.GetFullPath(saveDir);
                    string fullAppDir = Path.GetFullPath(appDir);
                    if (!fullPath.StartsWith(fullSaveDir, StringComparison.OrdinalIgnoreCase) &&
                        !fullPath.StartsWith(fullAppDir, StringComparison.OrdinalIgnoreCase))
                    {
                        res.StatusCode = 403;
                        res.StatusDescription = "Forbidden: Access Denied";
                        res.Close();
                        return;
                    }

                    string fn = Path.GetFileName(job.filePath);
                    res.Headers.Add("Content-Disposition", string.Format("attachment; filename=\"{0}\"", fn));
                    res.ContentType = GetMimeType(job.filePath);
                    FileInfo fi = new FileInfo(job.filePath);
                    res.ContentLength64 = fi.Length;
                    if (!isHead)
                    {
                        using (FileStream fs = new FileStream(job.filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            fs.CopyTo(res.OutputStream);
                        }
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
            bool launched = false;
            if (jobs.ContainsKey(id))
            {
                DownloadJob job = jobs[id];
                if (!string.IsNullOrEmpty(job.filePath) && File.Exists(job.filePath))
                {
                    if (target == "file")
                    {
                        // Open directly using Windows Settings Default Player (Movies & TV, Windows Media Player, VLC, PotPlayer, etc.)
                        try
                        {
                            ProcessStartInfo psi = new ProcessStartInfo();
                            psi.FileName = job.filePath;
                            psi.WorkingDirectory = Path.GetDirectoryName(job.filePath);
                            psi.UseShellExecute = true;
                            psi.Verb = "open";
                            psi.WindowStyle = ProcessWindowStyle.Normal;
                            Process.Start(psi);
                            launched = true;
                        }
                        catch
                        {
                            try
                            {
                                ProcessStartInfo psi = new ProcessStartInfo();
                                psi.FileName = "explorer.exe";
                                psi.Arguments = string.Format("\"{0}\"", job.filePath);
                                psi.UseShellExecute = true;
                                Process.Start(psi);
                                launched = true;
                            }
                            catch { }
                        }
                    }
                    else
                    {
                        // "View File": open folder in Windows Explorer and highlight the file
                        try
                        {
                            ProcessStartInfo psi = new ProcessStartInfo();
                            psi.FileName = "explorer.exe";
                            psi.Arguments = string.Format("/select,\"{0}\"", job.filePath);
                            psi.UseShellExecute = true;
                            Process.Start(psi);
                            launched = true;
                        }
                        catch
                        {
                            try
                            {
                                string dir = Path.GetDirectoryName(job.filePath);
                                if (Directory.Exists(dir))
                                {
                                    Process.Start("explorer.exe", dir);
                                    launched = true;
                                }
                            }
                            catch { }
                        }
                    }
                }
                else if (Directory.Exists(saveDir))
                {
                    try { Process.Start("explorer.exe", saveDir); launched = true; } catch { }
                }
            }
            else if (Directory.Exists(saveDir))
            {
                try { Process.Start("explorer.exe", saveDir); launched = true; } catch { }
            }

            SendJson(res, new { success = true, launched = launched });
        }

        private static string GetLocalAppVersion()
        {
            try
            {
                string vPath = GetWebFilePath("version.json");
                if (File.Exists(vPath))
                {
                    string vText = File.ReadAllText(vPath, Encoding.UTF8);
                    Dictionary<string, object> d = json.Deserialize<Dictionary<string, object>>(vText);
                    if (d != null && d.ContainsKey("version"))
                    {
                        string diskVer = Convert.ToString(d["version"]).Trim();
                        if (!string.IsNullOrEmpty(diskVer) && IsNewerVersion(diskVer, APP_VERSION))
                        {
                            return diskVer;
                        }
                    }
                }
            }
            catch { }
            return APP_VERSION;
        }

        private static string ComputeNormalizedTextHash(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            try
            {
                string norm = text.Replace("\r\n", "\n").Replace("\r", "\n").Trim();
                using (var sha = System.Security.Cryptography.SHA256.Create())
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(norm);
                    byte[] hash = sha.ComputeHash(bytes);
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < 6; i++) sb.Append(hash[i].ToString("x2"));
                    return sb.ToString();
                }
            }
            catch
            {
                return "";
            }
        }

        private static string ComputeNormalizedFileHash(string path)
        {
            try
            {
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    string content = File.ReadAllText(path, Encoding.UTF8);
                    return ComputeNormalizedTextHash(content);
                }
            }
            catch { }
            return "";
        }

        private static bool IsNewerVersion(string remoteVer, string localVer)
        {
            if (string.IsNullOrEmpty(remoteVer) || string.IsNullOrEmpty(localVer)) return false;
            try
            {
                string cleanR = remoteVer.Trim().TrimStart('v', 'V');
                string cleanL = localVer.Trim().TrimStart('v', 'V');
                string[] rParts = cleanR.Split('.');
                string[] lParts = cleanL.Split('.');
                int max = Math.Max(rParts.Length, lParts.Length);
                for (int i = 0; i < max; i++)
                {
                    int rVal = i < rParts.Length ? int.Parse(rParts[i]) : 0;
                    int lVal = i < lParts.Length ? int.Parse(lParts[i]) : 0;
                    if (rVal > lVal) return true;
                    if (rVal < lVal) return false;
                }
            }
            catch { }
            return false;
        }

        private static void HandleCheckUpdate(HttpListenerResponse res)
        {
            try
            {
                string localVer = GetLocalAppVersion();
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072 | (SecurityProtocolType)768 | SecurityProtocolType.Tls;

                string remoteJsonStr = null;
                long ts = DateTime.UtcNow.Ticks;
                string[] candidateUrls = new string[] {
                    GITHUB_VERSION_URL + "?_ts=" + ts,
                    GITHUB_ROOT_VERSION_URL + "?_ts=" + ts,
                    GITHUB_PAGES_VERSION_URL + "?_ts=" + ts
                };

                foreach (string url in candidateUrls)
                {
                    try
                    {
                        using (WebClient wc = new WebClient())
                        {
                            wc.Encoding = Encoding.UTF8;
                            wc.Headers.Add("User-Agent", "180HzDownloader/" + localVer);
                            wc.Headers.Add("Cache-Control", "no-cache");
                            remoteJsonStr = wc.DownloadString(url);
                            if (!string.IsNullOrEmpty(remoteJsonStr)) break;
                        }
                    }
                    catch { }
                }

                if (string.IsNullOrEmpty(remoteJsonStr))
                {
                    SendJson(res, new {
                        success = true,
                        hasUpdate = false,
                        currentVersion = localVer,
                        latestVersion = localVer,
                        message = "Unable to reach GitHub update server."
                    });
                    return;
                }

                Dictionary<string, object> dict = json.Deserialize<Dictionary<string, object>>(remoteJsonStr);
                string latestVer = dict.ContainsKey("version") ? dict["version"].ToString() : localVer;
                string downloadUrl = dict.ContainsKey("downloadUrl") ? dict["downloadUrl"].ToString() : GITHUB_SETUP_DOWNLOAD_URL;
                string changelog = dict.ContainsKey("changelog") ? dict["changelog"].ToString() : "";
                bool versionNewer = IsNewerVersion(latestVer, localVer);

                // Also compare live normalized hash of web/app.js and web/index.html on GitHub so any code push is detected 100%
                string localAppHash = ComputeNormalizedFileHash(GetWebFilePath("app.js"));
                string localHtmlHash = ComputeNormalizedFileHash(GetWebFilePath("index.html"));
                string localCombinedHash = (!string.IsNullOrEmpty(localAppHash) && !string.IsNullOrEmpty(localHtmlHash))
                    ? (localAppHash.Substring(0, 6) + localHtmlHash.Substring(0, 6))
                    : localAppHash;

                string remoteCombinedHash = "";
                try
                {
                    using (WebClient wc = new WebClient())
                    {
                        wc.Encoding = Encoding.UTF8;
                        wc.Headers.Add("User-Agent", "180HzDownloader/" + localVer);
                        wc.Headers.Add("Cache-Control", "no-cache");
                        string remoteAppJs = wc.DownloadString(GITHUB_RAW_BASE_URL + "/web/app.js?_ts=" + ts);
                        string remoteHtml = wc.DownloadString(GITHUB_RAW_BASE_URL + "/web/index.html?_ts=" + ts);
                        string rAppH = ComputeNormalizedTextHash(remoteAppJs);
                        string rHtmlH = ComputeNormalizedTextHash(remoteHtml);
                        if (!string.IsNullOrEmpty(rAppH) && !string.IsNullOrEmpty(rHtmlH))
                        {
                            remoteCombinedHash = rAppH.Substring(0, 6) + rHtmlH.Substring(0, 6);
                        }
                    }
                }
                catch { }

                bool buildChanged = (!string.IsNullOrEmpty(localCombinedHash) &&
                                     !string.IsNullOrEmpty(remoteCombinedHash) &&
                                     !string.Equals(localCombinedHash, remoteCombinedHash, StringComparison.OrdinalIgnoreCase));

                bool hasUpdate = versionNewer || buildChanged;

                SendJson(res, new {
                    success = true,
                    hasUpdate = hasUpdate,
                    versionNewer = versionNewer,
                    buildChanged = buildChanged,
                    currentVersion = localVer,
                    latestVersion = latestVer,
                    localHash = localCombinedHash,
                    remoteHash = remoteCombinedHash,
                    downloadUrl = downloadUrl,
                    githubUrl = GITHUB_REPO_URL,
                    changelog = changelog
                });
            }
            catch (Exception ex)
            {
                SendJson(res, new {
                    success = false,
                    hasUpdate = false,
                    currentVersion = GetLocalAppVersion(),
                    error = ex.Message
                });
            }
        }

        private static bool ReplaceExecutableAtomically(string sourceExe, string targetExe)
        {
            if (string.IsNullOrEmpty(sourceExe) || !File.Exists(sourceExe) || string.IsNullOrEmpty(targetExe)) return false;
            try
            {
                string dir = Path.GetDirectoryName(targetExe);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

                if (File.Exists(targetExe))
                {
                    string oldBackup = targetExe + ".old";
                    try { if (File.Exists(oldBackup)) File.Delete(oldBackup); } catch { }
                    try { File.Move(targetExe, oldBackup); } catch { }
                }

                File.Copy(sourceExe, targetExe, true);
                return File.Exists(targetExe);
            }
            catch
            {
                return false;
            }
        }

        private static void HandleApplyUpdate(HttpListenerResponse res)
        {
            try
            {
                if (isApplyingAppUpdate)
                {
                    SendJson(res, new {
                        success = true,
                        started = true,
                        progress = appUpdateProgress,
                        stage = appUpdateStage,
                        message = "Update is already in progress."
                    });
                    return;
                }

                isApplyingAppUpdate = true;
                appUpdateProgress = 12;
                appUpdateError = "";
                appUpdateStage = "Connecting to GitHub Live Release Server...";

                SendJson(res, new {
                    success = true,
                    started = true,
                    progress = appUpdateProgress,
                    stage = appUpdateStage,
                    message = "Automatic update started."
                });

                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072 | (SecurityProtocolType)768 | SecurityProtocolType.Tls;
                        long ts = DateTime.UtcNow.Ticks;

                        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                        string installDir = Path.Combine(localAppData, "Programs", "180Hz Downloader");
                        try { if (!Directory.Exists(installDir)) Directory.CreateDirectory(installDir); } catch { }

                        string[] webTextFiles = new string[] { "index.html", "styles.css", "app.js", "version.json" };
                        appUpdateProgress = 35;
                        appUpdateStage = "Downloading latest Web UI & Configuration from GitHub...";

                        foreach (string wf in webTextFiles)
                        {
                            try
                            {
                                using (WebClient wc = new WebClient())
                                {
                                    wc.Encoding = Encoding.UTF8;
                                    wc.Headers.Add("User-Agent", "180HzDownloader/" + APP_VERSION);
                                    wc.Headers.Add("Cache-Control", "no-cache");
                                    byte[] rawBytes = wc.DownloadData(GITHUB_RAW_BASE_URL + "/web/" + wf + "?_ts=" + ts);
                                    if (rawBytes != null && rawBytes.Length > 20)
                                    {
                                        string[] targetDirs = new string[] {
                                            Path.Combine(appDir, "web"),
                                            Path.Combine(installDir, "web"),
                                            Path.Combine(appDir, "docs")
                                        };
                                        foreach (string td in targetDirs)
                                        {
                                            try
                                            {
                                                if (td.EndsWith("docs") && !Directory.Exists(td)) continue;
                                                if (!Directory.Exists(td)) Directory.CreateDirectory(td);
                                                File.WriteAllBytes(Path.Combine(td, wf), rawBytes);
                                            }
                                            catch { }
                                        }
                                        if (wf == "version.json")
                                        {
                                            try { File.WriteAllBytes(Path.Combine(appDir, "version.json"), rawBytes); } catch { }
                                            try { File.WriteAllBytes(Path.Combine(installDir, "version.json"), rawBytes); } catch { }
                                        }
                                    }
                                }
                            }
                            catch { }
                        }

                        // Also sync click.mp3 if available
                        try
                        {
                            using (WebClient wc = new WebClient())
                            {
                                wc.Headers.Add("User-Agent", "180HzDownloader/" + APP_VERSION);
                                byte[] mp3Bytes = wc.DownloadData(GITHUB_RAW_BASE_URL + "/web/click.mp3?_ts=" + ts);
                                if (mp3Bytes != null && mp3Bytes.Length > 500)
                                {
                                    try { File.WriteAllBytes(Path.Combine(appDir, "web", "click.mp3"), mp3Bytes); } catch { }
                                    try { File.WriteAllBytes(Path.Combine(installDir, "web", "click.mp3"), mp3Bytes); } catch { }
                                }
                            }
                        }
                        catch { }

                        appUpdateProgress = 65;
                        appUpdateStage = "Downloading latest 180Hz Engine binary from GitHub...";

                        string tempInstaller = Path.Combine(Path.GetTempPath(), "180Hz_Downloader_Update.exe");
                        if (File.Exists(tempInstaller)) { try { File.Delete(tempInstaller); } catch { } }

                        string[] downloadCandidates = new string[] {
                            GITHUB_SETUP_DOWNLOAD_URL + "?_ts=" + ts,
                            GITHUB_RAW_BASE_URL + "/180Hz%20Downloader.exe?_ts=" + ts,
                            "https://github.com/tanjeem180hz/180hz_video-downloader/raw/main/180Hz%20Downloader%20Setup.exe"
                        };

                        bool downloadedExe = false;
                        foreach (string dUrl in downloadCandidates)
                        {
                            try
                            {
                                using (WebClient wc = new WebClient())
                                {
                                    wc.Headers.Add("User-Agent", "180HzDownloader/" + APP_VERSION);
                                    wc.DownloadFile(dUrl, tempInstaller);
                                    if (File.Exists(tempInstaller) && new FileInfo(tempInstaller).Length > 40000)
                                    {
                                        downloadedExe = true;
                                        break;
                                    }
                                }
                            }
                            catch { }
                        }

                        appUpdateProgress = 85;
                        appUpdateStage = "Installing update & syncing core dependencies...";

                        CheckUpdateYtDlp();

                        string currentExe = Application.ExecutablePath;
                        string installedExe = Path.Combine(installDir, "180Hz Downloader.exe");
                        bool replacedInPlace = false;

                        if (downloadedExe && File.Exists(tempInstaller))
                        {
                            bool r1 = ReplaceExecutableAtomically(tempInstaller, installedExe);
                            bool r2 = ReplaceExecutableAtomically(tempInstaller, currentExe);
                            replacedInPlace = r1 || r2;
                        }

                        appUpdateProgress = 96;
                        appUpdateStage = "Restarting 180Hz Engine...";
                        Thread.Sleep(700);

                        try
                        {
                            if (replacedInPlace || !downloadedExe)
                            {
                                string targetToStart = File.Exists(currentExe) ? currentExe : installedExe;
                                ProcessStartInfo psi = new ProcessStartInfo();
                                psi.FileName = targetToStart;
                                psi.WorkingDirectory = Path.GetDirectoryName(targetToStart) ?? appDir;
                                psi.Arguments = "--restarted-from-update";
                                psi.UseShellExecute = true;
                                CleanUp();
                                Process.Start(psi);
                            }
                            else if (File.Exists(tempInstaller))
                            {
                                ProcessStartInfo psi = new ProcessStartInfo();
                                psi.FileName = tempInstaller;
                                psi.Arguments = "--apply-update \"" + currentExe + "\"";
                                psi.UseShellExecute = true;
                                CleanUp();
                                Process.Start(psi);
                            }
                        }
                        catch { }

                        Thread.Sleep(250);
                        CleanUp();
                        Environment.Exit(0);
                    }
                    catch (Exception ex)
                    {
                        isApplyingAppUpdate = false;
                        appUpdateError = ex.Message;
                        appUpdateStage = "Update failed: " + ex.Message;
                    }
                });
            }
            catch (Exception ex)
            {
                isApplyingAppUpdate = false;
                SendJson(res, new {
                    success = false,
                    message = ex.Message
                });
            }
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
