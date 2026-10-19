using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Forms;

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

            string sourceDir = AppDomain.CurrentDomain.BaseDirectory;
            string targetExe = Path.Combine(installDir, "180Hz Downloader.exe");
            string currentExe = Application.ExecutablePath;

            // If a sibling 180Hz Downloader.exe or 180Hz Downloader Setup.exe exists
            string localMainExe = Path.Combine(sourceDir, "180Hz Downloader.exe");
            if (File.Exists(localMainExe))
            {
                File.Copy(localMainExe, targetExe, true);
            }
            else if (!string.Equals(Path.GetFullPath(currentExe), Path.GetFullPath(targetExe), StringComparison.OrdinalIgnoreCase))
            {
                File.Copy(currentExe, targetExe, true);
            }

            // Copy dependencies if present in installer folder
            try
            {
                string[] depFiles = new string[] { "yt-dlp.exe", "ffmpeg.exe", "app.ico", "youtube_visitor_cookies.txt" };
                foreach (string dep in depFiles)
                {
                    string src = Path.Combine(sourceDir, dep);
                    string dst = Path.Combine(installDir, dep);
                    if (File.Exists(src)) File.Copy(src, dst, true);
                }
            }
            catch { }

            // Create Desktop and Start Menu Shortcuts
            try
            {
                Type shellType = Type.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8"));
                if (shellType != null)
                {
                    object shell = Activator.CreateInstance(shellType);
                    string iconPath = Path.Combine(installDir, "app.ico");
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
                                scType.InvokeMember("WorkingDirectory", BindingFlags.SetProperty, null, sc, new object[] { installDir });
                                scType.InvokeMember("Description", BindingFlags.SetProperty, null, sc, new object[] { "180Hz Downloader - Lossless 8K Media Engine" });
                                scType.InvokeMember("IconLocation", BindingFlags.SetProperty, null, sc, new object[] { iconSpec });
                                scType.InvokeMember("Save", BindingFlags.InvokeMethod, null, sc, null);
                            }
                        }
                        catch { }
                    };

                    string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    if (!string.IsNullOrEmpty(desktop) && Directory.Exists(desktop)) makeLnk(Path.Combine(desktop, "180Hz Downloader.lnk"));

                    string startMenu = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
                    if (!string.IsNullOrEmpty(startMenu) && Directory.Exists(startMenu)) makeLnk(Path.Combine(startMenu, "180Hz Downloader.lnk"));
                }
            }
            catch { }

            if (File.Exists(targetExe))
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = targetExe;
                psi.WorkingDirectory = installDir;
                psi.UseShellExecute = true;
                Process.Start(psi);
            }
        }
    }
}
