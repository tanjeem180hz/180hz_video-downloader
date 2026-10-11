using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;

namespace TurboDownloaderInstaller
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                RunInstaller();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Installation encountered an issue: " + ex.Message, "180Hz Downloader Setup", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static void RunInstaller()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string installDir = Path.Combine(localAppData, "Programs", "180Hz Downloader");
            if (!Directory.Exists(installDir))
            {
                Directory.CreateDirectory(installDir);
            }

            // Close any existing instances before updating files
            try
            {
                string[] procNames = new string[] { "180Hz Downloader", "180HzDownloader", "TurboDownloader" };
                foreach (string name in procNames)
                {
                    foreach (Process p in Process.GetProcessesByName(name))
                    {
                        try { p.Kill(); p.WaitForExit(3000); } catch { }
                    }
                }
            }
            catch { }

            // Extract or copy application files (support both names for 100% compatibility)
            ExtractResourceOrCopy("180Hz Downloader.exe", Path.Combine(installDir, "180Hz Downloader.exe"));
            ExtractResourceOrCopy("TurboDownloader.exe", Path.Combine(installDir, "TurboDownloader.exe"));
            try { File.Copy(Path.Combine(installDir, "180Hz Downloader.exe"), Path.Combine(installDir, "180HzDownloader.exe"), true); } catch { }

            string webDir = Path.Combine(installDir, "web");
            if (!Directory.Exists(webDir)) Directory.CreateDirectory(webDir);
            ExtractResourceOrCopy("index.html", Path.Combine(webDir, "index.html"));
            ExtractResourceOrCopy("styles.css", Path.Combine(webDir, "styles.css"));
            ExtractResourceOrCopy("app.js", Path.Combine(webDir, "app.js"));

            ExtractResourceOrCopy("index.html", Path.Combine(installDir, "index.html"));
            ExtractResourceOrCopy("styles.css", Path.Combine(installDir, "styles.css"));
            ExtractResourceOrCopy("app.js", Path.Combine(installDir, "app.js"));
            ExtractResourceOrCopy("app.ico", Path.Combine(installDir, "app.ico"));

            // Create Desktop and Start Menu Shortcuts
            CreateShortcuts(Path.Combine(installDir, "180Hz Downloader.exe"));

            // Check & copy local dependencies if present
            string ytdlpTarget = Path.Combine(installDir, "yt-dlp.exe");
            string ffmpegTarget = Path.Combine(installDir, "ffmpeg.exe");

            string currentYtdlp = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "yt-dlp.exe");
            if (File.Exists(currentYtdlp) && !File.Exists(ytdlpTarget))
            {
                try { File.Copy(currentYtdlp, ytdlpTarget, true); } catch { }
            }

            string currentFfmpeg = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.exe");
            if (File.Exists(currentFfmpeg) && !File.Exists(ffmpegTarget))
            {
                try { File.Copy(currentFfmpeg, ffmpegTarget, true); } catch { }
            }

            // If yt-dlp still missing, download it
            if (!File.Exists(ytdlpTarget))
            {
                DownloadDependency("https://github.com/yt-dlp/yt-dlp-nightly-builds/releases/latest/download/yt-dlp.exe", ytdlpTarget);
            }
            else
            {
                // Run background update check to ensure latest signatures
                try
                {
                    ProcessStartInfo upPsi = new ProcessStartInfo();
                    upPsi.FileName = ytdlpTarget;
                    upPsi.Arguments = "--update-to nightly";
                    upPsi.CreateNoWindow = true;
                    upPsi.UseShellExecute = false;
                    Process.Start(upPsi);
                }
                catch { }
            }

            // If ffmpeg still missing, try system WinGet or PATH
            if (!File.Exists(ffmpegTarget))
            {
                string sysFfmpeg = FindSystemFFmpeg();
                if (!string.IsNullOrEmpty(sysFfmpeg) && File.Exists(sysFfmpeg))
                {
                    try { File.Copy(sysFfmpeg, ffmpegTarget, true); } catch { }
                }
            }

            // Launch the application
            string targetExe = Path.Combine(installDir, "180Hz Downloader.exe");
            if (!File.Exists(targetExe)) targetExe = Path.Combine(installDir, "TurboDownloader.exe");
            if (File.Exists(targetExe))
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = targetExe;
                psi.WorkingDirectory = installDir;
                Process.Start(psi);
            }

            MessageBox.Show("180Hz Downloader 2.0 (PEAK/8K Engine) installed successfully!\n\n• Desktop shortcut has been created.\n• The application is now running and ready to download.", "180Hz Downloader Setup", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        static void ExtractResourceOrCopy(string resourceName, string targetPath)
        {
            try
            {
                Assembly asm = Assembly.GetExecutingAssembly();
                using (Stream stream = asm.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        using (FileStream fs = new FileStream(targetPath, FileMode.Create, FileAccess.Write))
                        {
                            stream.CopyTo(fs);
                        }
                        return;
                    }
                }
            }
            catch { }

            try
            {
                string localFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, resourceName);
                if (!File.Exists(localFile))
                {
                    localFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "web", resourceName);
                }
                if (!File.Exists(localFile))
                {
                    localFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "docs", resourceName);
                }
                if (File.Exists(localFile))
                {
                    File.Copy(localFile, targetPath, true);
                }
            }
            catch { }
        }

        static void CreateShortcuts(string exePath)
        {
            try
            {
                Type t = Type.GetTypeFromProgID("WScript.Shell");
                if (t != null)
                {
                    dynamic shell = Activator.CreateInstance(t);

                    string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    string desktopLnk = Path.Combine(desktop, "180Hz Downloader.lnk");
                    string icoPath = Path.Combine(Path.GetDirectoryName(exePath), "app.ico");

                    var sc = shell.CreateShortcut(desktopLnk);
                    sc.TargetPath = exePath;
                    sc.WorkingDirectory = Path.GetDirectoryName(exePath);
                    sc.Description = "180Hz Downloader 2.0 - Bit-Exact 4K/8K Media Downloader";
                    if (File.Exists(icoPath)) sc.IconLocation = icoPath + ",0";
                    else sc.IconLocation = exePath + ",0";
                    sc.Save();

                    string startMenu = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs");
                    string startLnk = Path.Combine(startMenu, "180Hz Downloader.lnk");
                    var sc2 = shell.CreateShortcut(startLnk);
                    sc2.TargetPath = exePath;
                    sc2.WorkingDirectory = Path.GetDirectoryName(exePath);
                    sc2.Description = "180Hz Downloader 2.0 - Bit-Exact 4K/8K Media Downloader";
                    if (File.Exists(icoPath)) sc2.IconLocation = icoPath + ",0";
                    else sc2.IconLocation = exePath + ",0";
                    sc2.Save();
                }
            }
            catch { }
        }

        static void DownloadDependency(string url, string targetPath)
        {
            try
            {
                string temp = targetPath + ".tmp";
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072 | (SecurityProtocolType)768 | SecurityProtocolType.Tls;
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add("User-Agent", "Mozilla/5.0 180HzDownloaderSetup/2.0");
                    wc.DownloadFile(url, temp);
                    if (File.Exists(temp) && new FileInfo(temp).Length > 10000)
                    {
                        if (File.Exists(targetPath)) File.Delete(targetPath);
                        File.Move(temp, targetPath);
                    }
                }
            }
            catch { }
        }

        static string FindSystemFFmpeg()
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
    }
}
