# 180hz_video-downloader (TurboDownloader 2.0 / PEAK 8K)
> *First use then talk about this.*

A high-performance, bit-exact video and media download engine built with a futuristic cyberpunk interface (PEAK/8K) and a high-speed C# .NET multi-threaded socket backend.

---

## ⚡ Features

- **4K, 8K, 16K Ultra HD Video:** Downloads YouTube videos in any resolution up to 8K 4320p and merges with untouched high-fidelity audio streams (Opus/AAC/MP3) via FFmpeg.
- **Social Media Support:**
  - YouTube (Videos, Shorts, Playlists, 4K/8K HDR)
  - Instagram (Reels, Posts, Stories, Audio)
  - Facebook (HD Videos, Watch, Reels)
  - TikTok, Twitter/X, Vimeo, Reddit
- **BDIX & Direct Link Acceleration:** Multi-segmented parallel socket downloader (up to 64 connections) for saturating full 300+ Mbps bandwidth on BDIX FTP servers and direct links.
- **Futuristic PEAK/8K UI:** Hardware-accelerated animations, ambient glow mesh, particle physics, and live stream telemetry.
- **Zero External Runtimes Needed:** Standalone, lightweight Windows application (`TurboDownloader.exe`).

---

## 🚀 Quick Start

1. Clone the repository:
   ```bash
   git clone https://github.com/tanjeem180hz/180hz_video-downloader.git
   cd 180hz_video-downloader
   ```

2. If `yt-dlp.exe` and `ffmpeg.exe` are not present, run:
   ```cmd
   setup.bat
   ```

3. Run **`TurboDownloader.exe`**:
   Double click `TurboDownloader.exe` or run:
   ```cmd
   .\TurboDownloader.exe
   ```

4. Paste your video or direct download link into the console, select your desired resolution (e.g. 4K Ultra HD or Best Available), and click **Download**!

---

## 🛠️ Building from Source

To compile the C# backend without installing Visual Studio or heavy SDKs:
```cmd
build.bat
```
*(Uses the built-in Microsoft Visual C# compiler `csc.exe` in Windows).*

---

## 📜 License
MIT License. Created by [tanjeem180hz](https://github.com/tanjeem180hz).
