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
using System.Windows.Forms;
using System.Drawing;
using System.Net.Sockets;
using System.Linq;
using System.Runtime.InteropServices;

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

        private const string APP_VERSION = "2.1.0";
        private const int APP_VERSION_CODE = 210;
        private const string GITHUB_VERSION_URL = "https://raw.githubusercontent.com/tanjeem180hz/180hz_video-downloader/main/version.json";
        private const string GITHUB_PAGES_VERSION_URL = "https://tanjeem180hz.github.io/180hz_video-downloader/version.json";
        private const string GITHUB_SETUP_DOWNLOAD_URL = "https://raw.githubusercontent.com/tanjeem180hz/180hz_video-downloader/main/180Hz%20Downloader%20Setup.exe";
        private const string GITHUB_REPO_URL = "https://github.com/tanjeem180hz/180hz_video-downloader";

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

        private static bool DownloadFileWithFallback(string url, string targetPath)
        {
            string tempFile = targetPath + ".tmp";
            try
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);

                string curlPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "curl.exe");
                if (File.Exists(curlPath))
                {
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = curlPath;
                    psi.Arguments = string.Format("-L \"{0}\" -o \"{1}\" --retry 3 --max-time 180 -s", url, tempFile);
                    psi.UseShellExecute = false;
                    psi.CreateNoWindow = true;
                    using (Process p = Process.Start(psi))
                    {
                        p.WaitForExit();
                        if (p.ExitCode == 0 && File.Exists(tempFile) && new FileInfo(tempFile).Length > 100000)
                        {
                            if (File.Exists(targetPath)) File.Delete(targetPath);
                            File.Move(tempFile, targetPath);
                            return true;
                        }
                    }
                }

                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072 | (SecurityProtocolType)768 | SecurityProtocolType.Tls;
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) 180HzDownloader/2.0");
                    wc.DownloadFile(url, tempFile);
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
                    // 1. Download yt-dlp.exe (nightly with latest YouTube player/challenge fixes) if missing
                    if (!File.Exists(ytdlpPath))
                    {
                        depStatusMessage = "Downloading extraction engine (yt-dlp)...";
                        string ytdlpUrl = "https://github.com/yt-dlp/yt-dlp-nightly-builds/releases/latest/download/yt-dlp.exe";
                        DownloadFileWithFallback(ytdlpUrl, ytdlpPath);
                    }
                    else
                    {
                        CheckUpdateYtDlp();
                    }

                    // 2. Locate or download ffmpeg.exe if missing
                    if (!File.Exists(ffmpegPath))
                    {
                        string sysFfmpeg = FindSystemFFmpeg();
                        if (!string.IsNullOrEmpty(sysFfmpeg) && File.Exists(sysFfmpeg))
                        {
                            try { File.Copy(sysFfmpeg, ffmpegPath, true); } catch { }
                        }
                    }

                    if (!File.Exists(ffmpegPath))
                    {
                        depStatusMessage = "Setting up ffmpeg...";
                        try
                        {
                            ProcessStartInfo psi = new ProcessStartInfo();
                            psi.FileName = "winget";
                            psi.Arguments = "install --id yt-dlp.FFmpeg --accept-package-agreements --accept-source-agreements --silent";
                            psi.UseShellExecute = false;
                            psi.CreateNoWindow = true;
                            using (Process p = Process.Start(psi))
                            {
                                p.WaitForExit(60000);
                            }
                            string sysFfmpeg = FindSystemFFmpeg();
                            if (!string.IsNullOrEmpty(sysFfmpeg) && File.Exists(sysFfmpeg))
                            {
                                File.Copy(sysFfmpeg, ffmpegPath, true);
                            }
                        }
                        catch { }
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

        [STAThread]
        static void Main()
        {
            try
            {
                AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                {
                    try
                    {
                        File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash.log"), (e.ExceptionObject != null ? e.ExceptionObject.ToString() : "Unknown error"));
                    }
                    catch { }
                };

                appDir = AppDomain.CurrentDomain.BaseDirectory;
                saveDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                if (!Directory.Exists(saveDir)) saveDir = appDir;

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

                if (!createdNew)
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
                if (!StartHttpServer())
                {
                    File.WriteAllText(Path.Combine(appDir, "crash.log"), "Failed to bind HTTP server to any candidate port.");
                    return;
                }

                // Save active port
                try { File.WriteAllText(portFilePath, port.ToString()); } catch { }

                // Launch Desktop Application Window
                LaunchNativeWindow(port);

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
                    try { l.Prefixes.Add(string.Format("http://localhost:{0}/", candidate)); } catch { }
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
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

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

                Application.Run(new ApplicationContext());
            }
            catch (Exception ex)
            {
                try { File.AppendAllText(Path.Combine(appDir, "tray.log"), ex.ToString() + "\r\n"); } catch { }
            }

            while (listener != null && listener.IsListening)
            {
                Thread.Sleep(2000);
            }
        }

        private static void CleanUp()
        {
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

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_SHOWWINDOW = 0x0040;

        public static void ForceWindowToTop(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero) return;
            try
            {
                ShowWindow(hWnd, 9); // SW_RESTORE
                SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
                SetWindowPos(hWnd, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
                SetForegroundWindow(hWnd);
                BringWindowToTop(hWnd);
                SwitchToThisWindow(hWnd, true);
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
                Process p = null;
                if (targetBrowser != null)
                {
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = targetBrowser;
                    psi.Arguments = string.Format("--app=\"{0}\" --window-size=1120,820", appUrl);
                    p = Process.Start(psi);
                }
                else
                {
                    p = Process.Start(appUrl);
                }

                // Force 180Hz Downloader window to the topmost foreground above all running exes
                Task.Factory.StartNew(() =>
                {
                    for (int i = 0; i < 20; i++)
                    {
                        Thread.Sleep(250);
                        bool elevated = false;

                        if (p != null)
                        {
                            try
                            {
                                p.Refresh();
                                if (p.MainWindowHandle != IntPtr.Zero)
                                {
                                    ForceWindowToTop(p.MainWindowHandle);
                                    elevated = true;
                                }
                            }
                            catch { }
                        }

                        EnumWindows((hWnd, lParam) =>
                        {
                            try
                            {
                                StringBuilder sb = new StringBuilder(256);
                                if (GetWindowText(hWnd, sb, 256) > 0)
                                {
                                    string title = sb.ToString();
                                    if (title.IndexOf("180Hz", StringComparison.OrdinalIgnoreCase) >= 0)
                                    {
                                        ForceWindowToTop(hWnd);
                                        elevated = true;
                                        return false;
                                    }
                                }
                            }
                            catch { }
                            return true;
                        }, IntPtr.Zero);

                        if (elevated && i >= 4) break;
                    }
                });
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

            // Security Check 4: Strict CORS Origin Validation
            string origin = req.Headers["Origin"];
            bool isTrustedOrigin = false;
            if (string.IsNullOrEmpty(origin))
            {
                isTrustedOrigin = true; // Local direct browser or native window request
            }
            else
            {
                Uri originUri;
                if (Uri.TryCreate(origin, UriKind.Absolute, out originUri))
                {
                    string h = originUri.Host.ToLowerInvariant();
                    if (h == "127.0.0.1" || h == "localhost" || h == "tanjeem180hz.github.io")
                    {
                        isTrustedOrigin = true;
                        res.Headers.Add("Access-Control-Allow-Origin", origin);
                        res.Headers.Add("Access-Control-Allow-Credentials", "true");
                    }
                }
            }

            res.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            res.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization, X-Requested-With");

            if (req.HttpMethod == "OPTIONS")
            {
                res.StatusCode = 200;
                res.Close();
                return;
            }

            if (!isTrustedOrigin)
            {
                res.StatusCode = 403;
                res.StatusDescription = "Forbidden: Untrusted Origin";
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
                    SendJson(res, new {
                        success = true,
                        engine = "180Hz Downloader 2.0",
                        status = depStatusMessage,
                        isReady = File.Exists(ytdlpPath) && File.Exists(ffmpegPath),
                        isUpdating = isUpdatingYtDlp,
                        port = port
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
                else if ((rawUrl == "/api/v1/system/check-update" || rawUrl.StartsWith("/api/v1/system/check-update?")) && (req.HttpMethod == "GET" || req.HttpMethod == "POST"))
                {
                    HandleCheckUpdate(res);
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

            string np = GetNodePath();
            string jsRuntimeArg = !string.IsNullOrEmpty(np) ? string.Format("--js-runtimes node:\"{0}\"", np) : "";
            string userAgentArg = "--user-agent \"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36\"";
            string extArgs = userAgentArg;
            if (safeUrl.IndexOf("facebook.com", StringComparison.OrdinalIgnoreCase) >= 0 || safeUrl.IndexOf("fb.watch", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                extArgs += " --add-header \"Accept-Language:en-US,en;q=0.9\"";
            }
            string cookiesPath = Path.Combine(appDir, "cookies.txt");
            if (!File.Exists(cookiesPath)) cookiesPath = Path.Combine(saveDir, "cookies.txt");
            string cookiesArg = File.Exists(cookiesPath) ? string.Format("--cookies \"{0}\"", cookiesPath) : "";

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = ytdlp;
            psi.Arguments = string.Format("{0} {1} {2} --dump-json --no-playlist --no-warnings --socket-timeout 15 \"{3}\"", jsRuntimeArg, extArgs, cookiesArg, safeUrl);
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

                    if (string.IsNullOrEmpty(output))
                    {
                        string inferredTitle = "Media Stream";
                        try
                        {
                            Match m = Regex.Match(safeUrl, @"(?:reel|p|tv|shorts|watch\?v=|\/)([A-Za-z0-9_\-]+)");
                            if (m.Success && !string.IsNullOrEmpty(m.Groups[1].Value))
                                inferredTitle = "Media Stream (" + m.Groups[1].Value + ")";
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

                                string resLabel = height > 0 ? (height + "p") : formatId;
                                if (height >= 4320 || width >= 7680) resLabel = "4320p / 8K";
                                else if (height >= 2160 || width >= 3840) resLabel = "2160p / 4K";
                                else if (height >= 1440 || width >= 2560) resLabel = "1440p / 2K";
                                else if (height >= 1080 || width >= 1920) resLabel = "1080p / Full HD";
                                else if (height >= 720 || width >= 1280) resLabel = "720p / HD";
                                else if (height >= 480 || width >= 854) resLabel = "480p";
                                else if (height >= 360 || width >= 640) resLabel = "360p";
                                else if (height >= 240) resLabel = "240p";
                                else if (height >= 144) resLabel = "144p";

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
                data = new { id = jobId }
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

                if (isFacebook && (safeFormatId.IndexOf("hd", StringComparison.OrdinalIgnoreCase) >= 0 || safeFormatId == "best"))
                {
                    formatArg = "-f \"best[format_id*=hd]/best[ext=mp4]/bestvideo+bestaudio/best\" -S \"res,acodec:m4a\"";
                    mergeArg = "--merge-output-format mp4";
                }
                else if (isFacebook && safeFormatId.IndexOf("sd", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    formatArg = "-f \"best[format_id*=sd][height<=480]/bestvideo[height<=480]+bestaudio/best[height<=480]/best\" -S \"res:480,acodec:m4a\"";
                    mergeArg = "--merge-output-format mp4";
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
                            ? string.Format("-f \"bv*[height<={0}][ext=webm]+ba[ext=webm]/bv*[height<={0}]+ba/b[height<={0}]/bv*+ba/b\" -S \"res:{0},acodec:opus\"", targetHeight)
                            : "-f \"bv*[ext=webm]+ba[ext=webm]/bv*+ba/b\" -S \"res,acodec:opus\"";
                        mergeArg = "--merge-output-format webm";
                    }
                    else
                    {
                        formatArg = targetHeight > 0
                            ? string.Format("-f \"bv*[height<={0}]+ba/b[height<={0}]/bv*+ba/b\" -S \"res:{0},acodec:m4a\"", targetHeight)
                            : "-f \"bv*+ba/b\" -S \"res,acodec:m4a\"";
                        mergeArg = "--merge-output-format mp4";
                    }
                }
                else if (safeFormatId.EndsWith("p") && targetHeight > 0)
                {
                    if (isWebM)
                    {
                        formatArg = string.Format("-f \"bv*[height<={0}][ext=webm]+ba[ext=webm]/bv*[height<={0}]+ba/b[height<={0}]/bv*+ba/b\" -S \"res:{0},acodec:opus\"", targetHeight);
                        mergeArg = "--merge-output-format webm";
                    }
                    else
                    {
                        formatArg = string.Format("-f \"bv*[height<={0}]+ba/b[height<={0}]/bv*+ba/b\" -S \"res:{0},acodec:m4a\"", targetHeight);
                        mergeArg = "--merge-output-format mp4";
                    }
                }
                else
                {
                    // Specific Format ID selected by user (e.g. 701, 401, 315, 299, 137, 18, 0, etc.)
                    if (isWebM)
                    {
                        formatArg = targetHeight > 0
                            ? string.Format("-f \"{0}[acodec!=none]/{0}+bestaudio[ext=webm]/{0}+bestaudio[acodec=opus]/{0}+ba/bv*[height<={1}]+ba/bv*+ba/b\" -S \"res:{1},acodec:opus\"", safeFormatId, targetHeight)
                            : string.Format("-f \"{0}[acodec!=none]/{0}+bestaudio[ext=webm]/{0}+bestaudio[acodec=opus]/{0}+ba/bv*+ba/b\" -S \"res,acodec:opus\"", safeFormatId);
                        mergeArg = "--merge-output-format webm";
                    }
                    else
                    {
                        formatArg = targetHeight > 0
                            ? string.Format("-f \"{0}[acodec!=none]/{0}+bestaudio[ext=m4a]/{0}+ba/bv*[height<={1}]+ba/bv*+ba/b\" -S \"res:{1},acodec:m4a\"", safeFormatId, targetHeight)
                            : string.Format("-f \"{0}[acodec!=none]/{0}+bestaudio[ext=m4a]/{0}+ba/bv*+ba/b\" -S \"res,acodec:m4a\"", safeFormatId);
                        mergeArg = "--merge-output-format mp4";
                    }
                }
            }

            string outTemplate = Path.Combine(saveDir, "%(title)s.%(ext)s");
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
            string cookiesPath = Path.Combine(appDir, "cookies.txt");
            if (!File.Exists(cookiesPath)) cookiesPath = Path.Combine(saveDir, "cookies.txt");
            string cookiesArg = File.Exists(cookiesPath) ? string.Format("--cookies \"{0}\"", cookiesPath) : "";

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
                            DirectoryInfo di = new DirectoryInfo(saveDir);
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

                    // If file is locked by Windows (e.g. playing in media player), auto-retry with timestamped suffix
                    if (err.IndexOf("Permission denied", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        err.IndexOf("used by another process", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        err.IndexOf("Errno 13", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        try
                        {
                            string fallbackTemplate = Path.Combine(saveDir, "%(title)s (" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ").%(ext)s");
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

        private static string FindSystemVideoPlayer()
        {
            string[] candidatePaths = new string[]
            {
                // 1. VLC Media Player (Universal player, decodes AV1, VP9, HEVC, H.264, 4K/8K)
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "VideoLAN", "VLC", "vlc.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "VideoLAN", "VLC", "vlc.exe"),
                // 2. PotPlayer
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "DAUM", "PotPlayer", "PotPlayer64.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "DAUM", "PotPlayer", "PotPlayer.exe"),
                // 3. MPC-HC / MPC-BE
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "MPC-HC", "mpc-hc64.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "K-Lite Codec Pack", "MPC-HC64", "mpc-hc64.exe"),
                // 4. Windows Media Player (Classic)
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Windows Media Player", "wmplayer.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Windows Media Player", "wmplayer.exe")
            };

            foreach (string p in candidatePaths)
            {
                try
                {
                    if (!string.IsNullOrEmpty(p) && File.Exists(p)) return p;
                }
                catch { }
            }
            return null;
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
                    string fullPath = Path.GetFullPath(job.filePath);
                    string fullSaveDir = Path.GetFullPath(saveDir);
                    string fullAppDir = Path.GetFullPath(appDir);
                    if (fullPath.StartsWith(fullSaveDir, StringComparison.OrdinalIgnoreCase) ||
                        fullPath.StartsWith(fullAppDir, StringComparison.OrdinalIgnoreCase))
                    {
                        if (target == "file")
                        {
                            string ext = Path.GetExtension(job.filePath).ToLowerInvariant();
                            bool isVideo = (ext == ".mp4" || ext == ".webm" || ext == ".mkv" || ext == ".avi" || ext == ".mov");

                            if (isVideo)
                            {
                                string dedicatedPlayer = FindSystemVideoPlayer();
                                if (!string.IsNullOrEmpty(dedicatedPlayer))
                                {
                                    try
                                    {
                                        ProcessStartInfo psi = new ProcessStartInfo();
                                        psi.FileName = dedicatedPlayer;
                                        psi.Arguments = string.Format("\"{0}\"", job.filePath);
                                        psi.UseShellExecute = false;
                                        Process.Start(psi);
                                        launched = true;
                                    }
                                    catch { }
                                }

                                if (!launched)
                                {
                                    try
                                    {
                                        ProcessStartInfo psi = new ProcessStartInfo();
                                        psi.FileName = job.filePath;
                                        psi.UseShellExecute = true;
                                        Process.Start(psi);
                                        launched = true;
                                    }
                                    catch
                                    {
                                        try
                                        {
                                            ProcessStartInfo psi = new ProcessStartInfo();
                                            psi.FileName = "explorer.exe";
                                            psi.Arguments = string.Format("/select,\"{0}\"", job.filePath);
                                            psi.UseShellExecute = true;
                                            Process.Start(psi);
                                            launched = true;
                                        }
                                        catch { }
                                    }
                                }
                            }
                            else
                            {
                                try
                                {
                                    ProcessStartInfo psi = new ProcessStartInfo();
                                    psi.FileName = job.filePath;
                                    psi.UseShellExecute = true;
                                    Process.Start(psi);
                                    launched = true;
                                }
                                catch
                                {
                                    try
                                    {
                                        ProcessStartInfo psi = new ProcessStartInfo();
                                        psi.FileName = "explorer.exe";
                                        psi.Arguments = string.Format("/select,\"{0}\"", job.filePath);
                                        psi.UseShellExecute = true;
                                        Process.Start(psi);
                                        launched = true;
                                    }
                                    catch { }
                                }
                            }
                        }
                        else
                        {
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
                                    else if (Directory.Exists(saveDir))
                                    {
                                        Process.Start("explorer.exe", saveDir);
                                        launched = true;
                                    }
                                }
                                catch { }
                            }
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
                string remoteJsonStr = null;
                string[] candidateUrls = new string[] {
                    GITHUB_VERSION_URL + "?_ts=" + DateTime.UtcNow.Ticks,
                    GITHUB_PAGES_VERSION_URL + "?_ts=" + DateTime.UtcNow.Ticks
                };

                foreach (string url in candidateUrls)
                {
                    try
                    {
                        using (WebClient wc = new WebClient())
                        {
                            wc.Headers.Add("User-Agent", "180HzDownloader/" + APP_VERSION);
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
                        currentVersion = APP_VERSION,
                        latestVersion = APP_VERSION,
                        message = "Unable to reach GitHub update server."
                    });
                    return;
                }

                Dictionary<string, object> dict = json.Deserialize<Dictionary<string, object>>(remoteJsonStr);
                string latestVer = dict.ContainsKey("version") ? dict["version"].ToString() : APP_VERSION;
                string downloadUrl = dict.ContainsKey("downloadUrl") ? dict["downloadUrl"].ToString() : GITHUB_SETUP_DOWNLOAD_URL;
                string changelog = dict.ContainsKey("changelog") ? dict["changelog"].ToString() : "";
                bool hasUpdate = IsNewerVersion(latestVer, APP_VERSION);

                SendJson(res, new {
                    success = true,
                    hasUpdate = hasUpdate,
                    currentVersion = APP_VERSION,
                    latestVersion = latestVer,
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
                    currentVersion = APP_VERSION,
                    error = ex.Message
                });
            }
        }

        private static void HandleApplyUpdate(HttpListenerResponse res)
        {
            try
            {
                string tempInstaller = Path.Combine(Path.GetTempPath(), "180Hz_Downloader_Update.exe");
                if (File.Exists(tempInstaller)) { try { File.Delete(tempInstaller); } catch { } }

                string[] downloadCandidates = new string[] {
                    GITHUB_SETUP_DOWNLOAD_URL,
                    "https://github.com/tanjeem180hz/180hz_video-downloader/raw/main/180Hz%20Downloader%20Setup.exe"
                };

                bool downloaded = false;
                foreach (string dUrl in downloadCandidates)
                {
                    try
                    {
                        ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072 | (SecurityProtocolType)768 | SecurityProtocolType.Tls;
                        using (WebClient wc = new WebClient())
                        {
                            wc.Headers.Add("User-Agent", "180HzDownloader/" + APP_VERSION);
                            wc.DownloadFile(dUrl, tempInstaller);
                            if (File.Exists(tempInstaller) && new FileInfo(tempInstaller).Length > 50000)
                            {
                                downloaded = true;
                                break;
                            }
                        }
                    }
                    catch { }
                }

                if (!downloaded)
                {
                    SendJson(res, new {
                        success = false,
                        message = "Failed to download update installer from GitHub."
                    });
                    return;
                }

                SendJson(res, new {
                    success = true,
                    message = "Update installer downloaded. Launching update and restarting..."
                });

                Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(700);
                    try
                    {
                        ProcessStartInfo psi = new ProcessStartInfo();
                        psi.FileName = tempInstaller;
                        psi.UseShellExecute = true;
                        Process.Start(psi);
                    }
                    catch { }

                    Thread.Sleep(500);
                    CleanUp();
                    Environment.Exit(0);
                });
            }
            catch (Exception ex)
            {
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
