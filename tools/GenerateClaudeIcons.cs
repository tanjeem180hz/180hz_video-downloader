using System;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Collections.Generic;

public class GenerateClaudeIcons
{
    private class IconImageEntry
    {
        public int Width;
        public int Height;
        public byte[] PngData;
    }

    public static void Main(string[] args)
    {
        string rootDir = Environment.CurrentDirectory;
        if (File.Exists(Path.Combine(rootDir, "TurboDownloader.cs")))
        {
            // Already in root
        }
        else
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\', '/');
            rootDir = Directory.GetParent(baseDir).FullName;
        }

        Console.WriteLine("Generating Claude-style icons for 180Hz Downloader...");
        Console.WriteLine("Root directory: " + rootDir);

        string claudePathD = "m4.7144 15.9555 4.7174-2.6471.079-.2307-.079-.1275h-.2307l-.7893-.0486-2.6956-.0729-2.3375-.0971-2.2646-.1214-.5707-.1215-.5343-.7042.0546-.3522.4797-.3218.686.0608 1.5179.1032 2.2767.1578 1.6514.0972 2.4468.255h.3886l.0546-.1579-.1336-.0971-.1032-.0972L6.973 9.8356l-2.55-1.6879-1.3356-.9714-.7225-.4918-.3643-.4614-.1578-1.0078.6557-.7225.8803.0607.2246.0607.8925.686 1.9064 1.4754 2.4893 1.8336.3643.3035.1457-.1032.0182-.0728-.164-.2733-1.3539-2.4467-1.445-2.4893-.6435-1.032-.17-.6194c-.0607-.255-.1032-.4674-.1032-.7285L6.287.1335 6.6997 0l.9957.1336.419.3642.6192 1.4147 1.0018 2.2282 1.5543 3.0296.4553.8985.2429.8318.091.255h.1579v-.1457l.1275-1.706.2368-2.0947.2307-2.6957.0789-.7589.3764-.9107.7468-.4918.5828.2793.4797.686-.0668.4433-.2853 1.8517-.5586 2.9021-.3643 1.9429h.2125l.2429-.2429.9835-1.3053 1.6514-2.0643.7286-.8196.85-.9046.5464-.4311h1.0321l.759 1.1293-.34 1.1657-1.0625 1.3478-.8804 1.1414-1.2628 1.7-.7893 1.36.0729.1093.1882-.0183 2.8535-.607 1.5421-.2794 1.8396-.3157.8318.3886.091.3946-.3278.8075-1.967.4857-2.3072.4614-3.4364.8136-.0425.0304.0486.0607 1.5482.1457.6618.0364h1.621l3.0175.2247.7892.522.4736.6376-.079.4857-1.2142.6193-1.6393-.3886-3.825-.9107-1.3113-.3279h-.1822v.1093l1.0929 1.0686 2.0035 1.8092 2.5075 2.3314.1275.5768-.3218.4554-.34-.0486-2.2039-1.6575-.85-.7468-1.9246-1.621h-.1275v.17l.4432.6496 2.3436 3.5214.1214 1.0807-.17.3521-.6071.2125-.6679-.1214-1.3721-1.9246L14.38 17.959l-1.1414-1.9428-.1397.079-.674 7.2552-.3156.3703-.7286.2793-.6071-.4614-.3218-.7468.3218-1.4753.3886-1.9246.3157-1.53.2853-1.9004.17-.6314-.0121-.0425-.1397.0182-1.4328 1.9672-2.1796 2.9446-1.7243 1.8456-.4128.164-.7164-.3704.0667-.6618.4008-.5889 2.386-3.0357 1.4389-1.882.929-1.0868-.0062-.1579h-.0546l-6.3385 4.1164-1.1293.1457-.4857-.4554.0608-.7467.2307-.2429 1.9064-1.3114Z";

        // HTML for main desktop squircle app icon
        string squircleHtml = @"<!DOCTYPE html>
<html>
<head>
<meta charset='utf-8'>
<style>
  * { margin: 0; padding: 0; box-sizing: border-box; }
  body {
    width: 512px; height: 512px;
    background: transparent;
    display: flex; align-items: center; justify-content: center;
    overflow: hidden;
  }
  .app-icon {
    width: 472px; height: 472px;
    border-radius: 106px;
    background: radial-gradient(circle at 50% 30%, #292723 0%, #181715 64%, #0d0d0c 100%);
    box-shadow: 0 16px 44px rgba(0,0,0,0.48), inset 0 1px 1px rgba(255,255,255,0.15), inset 0 -2px 4px rgba(0,0,0,0.5);
    display: flex; align-items: center; justify-content: center;
    position: relative;
    border: 1px solid rgba(255, 255, 255, 0.08);
  }
  .spark-glow {
    position: absolute;
    width: 290px; height: 290px;
    border-radius: 50%;
    background: radial-gradient(circle, rgba(217, 119, 87, 0.22) 0%, rgba(217, 119, 87, 0) 70%);
    pointer-events: none;
  }
  .spark-svg {
    width: 312px; height: 312px;
    filter: drop-shadow(0 4px 14px rgba(217, 119, 87, 0.38));
  }
</style>
</head>
<body>
  <div class='app-icon'>
    <div class='spark-glow'></div>
    <svg class='spark-svg' viewBox='0 0 24 24'>
      <defs>
        <linearGradient id='claudeGrad' x1='0%' y1='0%' x2='100%' y2='100%'>
          <stop offset='0%' stop-color='#EA8E70' />
          <stop offset='50%' stop-color='#D97757' />
          <stop offset='100%' stop-color='#C66545' />
        </linearGradient>
      </defs>
      <path fill='url(#claudeGrad)' d='" + claudePathD + @"'/>
    </svg>
  </div>
</body>
</html>";

        // HTML for pure spark (favicon)
        string sparkHtml = @"<!DOCTYPE html>
<html>
<head>
<meta charset='utf-8'>
<style>
  * { margin: 0; padding: 0; box-sizing: border-box; }
  body {
    width: 512px; height: 512px;
    background: transparent;
    display: flex; align-items: center; justify-content: center;
    overflow: hidden;
  }
  .spark-svg {
    width: 480px; height: 480px;
  }
</style>
</head>
<body>
  <svg class='spark-svg' viewBox='-0.6 -0.6 25.2 25.2'>
    <defs>
      <linearGradient id='sparkGrad' x1='0%' y1='0%' x2='100%' y2='100%'>
        <stop offset='0%' stop-color='#EA8E70' />
        <stop offset='50%' stop-color='#D97757' />
        <stop offset='100%' stop-color='#C66545' />
      </linearGradient>
    </defs>
    <path fill='url(#sparkGrad)' d='" + claudePathD + @"'/>
  </svg>
</body>
</html>";

        string tempSquircleHtml = Path.Combine(rootDir, "temp_squircle.html");
        string tempSparkHtml = Path.Combine(rootDir, "temp_spark.html");
        string tempSquirclePng = Path.Combine(rootDir, "temp_squircle_512.png");
        string tempSparkPng = Path.Combine(rootDir, "temp_spark_512.png");

        File.WriteAllText(tempSquircleHtml, squircleHtml, System.Text.Encoding.UTF8);
        File.WriteAllText(tempSparkHtml, sparkHtml, System.Text.Encoding.UTF8);

        // Render via Edge
        RenderHtmlToPng(tempSquircleHtml, tempSquirclePng, 512, 512);
        RenderHtmlToPng(tempSparkHtml, tempSparkPng, 512, 512);

        if (!File.Exists(tempSquirclePng) || !File.Exists(tempSparkPng))
        {
            Console.WriteLine("Error: Failed to render HTML to PNG.");
            return;
        }

        Console.WriteLine("Rendered 512x512 source assets.");

        // 1. Generate PNG sizes
        byte[] png512 = File.ReadAllBytes(tempSquirclePng);
        byte[] png256 = ResizeToPngBytes(tempSquirclePng, 256, 256);
        byte[] png192 = ResizeToPngBytes(tempSquirclePng, 192, 192);
        byte[] png128 = ResizeToPngBytes(tempSquirclePng, 128, 128);
        byte[] png64  = ResizeToPngBytes(tempSquirclePng, 64, 64);
        byte[] png48  = ResizeToPngBytes(tempSquirclePng, 48, 48);
        byte[] png32  = ResizeToPngBytes(tempSquirclePng, 32, 32);
        byte[] png16  = ResizeToPngBytes(tempSquirclePng, 16, 16);

        // Favicon pure spark sizes
        byte[] spark48 = ResizeToPngBytes(tempSparkPng, 48, 48);
        byte[] spark32 = ResizeToPngBytes(tempSparkPng, 32, 32);
        byte[] spark16 = ResizeToPngBytes(tempSparkPng, 16, 16);

        // 2. Build multi-resolution ICO files
        // app.ico & setup.ico: 256, 128, 64, 48, 32, 16
        List<IconImageEntry> appIcoImages = new List<IconImageEntry>
        {
            new IconImageEntry { Width = 256, Height = 256, PngData = png256 },
            new IconImageEntry { Width = 128, Height = 128, PngData = png128 },
            new IconImageEntry { Width = 64,  Height = 64,  PngData = png64 },
            new IconImageEntry { Width = 48,  Height = 48,  PngData = png48 },
            new IconImageEntry { Width = 32,  Height = 32,  PngData = png32 },
            new IconImageEntry { Width = 16,  Height = 16,  PngData = png16 }
        };
        byte[] appIcoBytes = BuildIco(appIcoImages);

        // favicon.ico: pure spark 48, 32, 16
        List<IconImageEntry> favIcoImages = new List<IconImageEntry>
        {
            new IconImageEntry { Width = 48, Height = 48, PngData = spark48 },
            new IconImageEntry { Width = 32, Height = 32, PngData = spark32 },
            new IconImageEntry { Width = 16, Height = 16, PngData = spark16 }
        };
        byte[] favIcoBytes = BuildIco(favIcoImages);

        // 3. Save files to destinations
        string[] targetDirs = new string[]
        {
            rootDir,
            Path.Combine(rootDir, "web"),
            Path.Combine(rootDir, "docs")
        };

        foreach (string d in targetDirs)
        {
            if (!Directory.Exists(d)) Directory.CreateDirectory(d);

            File.WriteAllBytes(Path.Combine(d, "app.ico"), appIcoBytes);
            File.WriteAllBytes(Path.Combine(d, "favicon.ico"), favIcoBytes);
            File.WriteAllBytes(Path.Combine(d, "app.png"), png256);
            File.WriteAllBytes(Path.Combine(d, "favicon-32x32.png"), spark32);
            File.WriteAllBytes(Path.Combine(d, "icon-192.png"), png192);
            File.WriteAllBytes(Path.Combine(d, "icon-512.png"), png512);

            if (d == rootDir)
            {
                File.WriteAllBytes(Path.Combine(d, "setup.ico"), appIcoBytes);
            }
            Console.WriteLine("Updated icon assets in: " + d);
        }

        // Also update local installed programs folder if present
        try
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string installDir = Path.Combine(localAppData, "Programs", "180Hz Downloader");
            if (Directory.Exists(installDir))
            {
                File.WriteAllBytes(Path.Combine(installDir, "app.ico"), appIcoBytes);
                File.WriteAllBytes(Path.Combine(installDir, "favicon.ico"), favIcoBytes);
                File.WriteAllBytes(Path.Combine(installDir, "app.png"), png256);
                File.WriteAllBytes(Path.Combine(installDir, "favicon-32x32.png"), spark32);
                File.WriteAllBytes(Path.Combine(installDir, "icon-192.png"), png192);
                File.WriteAllBytes(Path.Combine(installDir, "icon-512.png"), png512);

                string installWeb = Path.Combine(installDir, "web");
                if (Directory.Exists(installWeb))
                {
                    File.WriteAllBytes(Path.Combine(installWeb, "app.ico"), appIcoBytes);
                    File.WriteAllBytes(Path.Combine(installWeb, "favicon.ico"), favIcoBytes);
                    File.WriteAllBytes(Path.Combine(installWeb, "app.png"), png256);
                    File.WriteAllBytes(Path.Combine(installWeb, "favicon-32x32.png"), spark32);
                    File.WriteAllBytes(Path.Combine(installWeb, "icon-192.png"), png192);
                    File.WriteAllBytes(Path.Combine(installWeb, "icon-512.png"), png512);
                }
                Console.WriteLine("Updated icons in local install directory: " + installDir);
            }
        }
        catch { }

        // Clean up temporary files
        try
        {
            if (File.Exists(tempSquircleHtml)) File.Delete(tempSquircleHtml);
            if (File.Exists(tempSparkHtml)) File.Delete(tempSparkHtml);
            if (File.Exists(tempSquirclePng)) File.Delete(tempSquirclePng);
            if (File.Exists(tempSparkPng)) File.Delete(tempSparkPng);
        }
        catch { }

        Console.WriteLine("All Claude-style logo and icon assets successfully generated!");
    }

    private static void RenderHtmlToPng(string htmlPath, string pngPath, int width, int height)
    {
        string edgePath = @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe";
        if (!File.Exists(edgePath)) edgePath = @"C:\Program Files\Microsoft\Edge\Application\msedge.exe";
        if (!File.Exists(edgePath)) edgePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe";

        ProcessStartInfo psi = new ProcessStartInfo();
        psi.FileName = edgePath;
        psi.Arguments = string.Format("--headless=new --disable-gpu --default-background-color=00000000 --screenshot=\"{0}\" --window-size={1},{2} \"{3}\"",
            pngPath, width, height, htmlPath);
        psi.CreateNoWindow = true;
        psi.UseShellExecute = false;

        using (Process p = Process.Start(psi))
        {
            p.WaitForExit(15000);
        }
    }

    private static byte[] ResizeToPngBytes(string srcPath, int w, int h)
    {
        using (Bitmap src = new Bitmap(srcPath))
        using (Bitmap dst = new Bitmap(w, h, PixelFormat.Format32bppArgb))
        {
            using (Graphics g = Graphics.FromImage(dst))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.Clear(Color.Transparent);
                g.DrawImage(src, new Rectangle(0, 0, w, h));
            }
            using (MemoryStream ms = new MemoryStream())
            {
                dst.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
        }
    }

    private static byte[] BuildIco(List<IconImageEntry> entries)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            BinaryWriter w = new BinaryWriter(ms);
            // ICONDIR
            w.Write((ushort)0); // reserved
            w.Write((ushort)1); // type 1 = icon
            w.Write((ushort)entries.Count); // image count

            int offset = 6 + (16 * entries.Count);
            for (int i = 0; i < entries.Count; i++)
            {
                IconImageEntry e = entries[i];
                w.Write((byte)(e.Width >= 256 ? 0 : e.Width));
                w.Write((byte)(e.Height >= 256 ? 0 : e.Height));
                w.Write((byte)0); // colors
                w.Write((byte)0); // reserved
                w.Write((ushort)1); // planes
                w.Write((ushort)32); // bpp
                w.Write((int)e.PngData.Length);
                w.Write((int)offset);
                offset += e.PngData.Length;
            }

            for (int i = 0; i < entries.Count; i++)
            {
                w.Write(entries[i].PngData);
            }

            w.Flush();
            return ms.ToArray();
        }
    }
}
