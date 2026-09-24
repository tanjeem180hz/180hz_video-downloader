# 180hz_video-downloader (TurboDownloader 2.0 / PEAK 8K)
> *First use then talk about this.*

🌐 **Live Website / Web App:** [https://tanjeem180hz.github.io/180hz_video-downloader/](https://tanjeem180hz.github.io/180hz_video-downloader/)

A high-performance, bit-exact video and media download engine built with a futuristic cyberpunk interface (PEAK/8K) and a high-speed C# .NET multi-threaded socket backend.

---

## ⚡ Features

- **1-Click Windows Installer (`TurboDownloaderSetup.exe`):** Automatically installs to your system, creates Desktop & Start Menu shortcuts, and automatically downloads & sets up `yt-dlp.exe` and `ffmpeg.exe` with zero manual configuration.
- **4K, 8K, 16K Ultra HD Video:** Downloads YouTube videos in any resolution up to 8K 4320p and merges with untouched high-fidelity audio streams (Opus/AAC/MP3) via FFmpeg.
- **Social Media Support:**
  - YouTube (Videos, Shorts, Playlists, 4K/8K HDR)
  - Instagram (Reels, Posts, Stories, Audio)
  - Facebook (HD Videos, Watch, Reels)
  - TikTok, Twitter/X, Vimeo, Reddit
- **BDIX & Direct Link Acceleration:** Multi-segmented parallel socket downloader (up to 64 connections) for saturating full 300+ Mbps bandwidth on BDIX FTP servers and direct links.
- **Futuristic PEAK/8K UI:** Hardware-accelerated animations, ambient glow mesh, particle physics, and live stream telemetry.
- **Enterprise-Grade Security:**
  - **Loopback IP Lockdown:** Rejects all non-loopback connections (`403 Forbidden`).
  - **DNS Rebinding Protection:** Enforces strict `Host` validation (`localhost`, `127.0.0.1`).
  - **CORS Origin Filtering:** Whitelisted to local origins and GitHub Pages.
  - **Command Injection & Path Traversal Shields:** Full input regex validation, quote escaping, and canonical path checks.
  - **Console Data Shield:** DevTools console data leakage restriction.

---

## 🚀 Quick Start (1-Click Install)

### For End-Users:
1. Visit the live site: [https://tanjeem180hz.github.io/180hz_video-downloader/](https://tanjeem180hz.github.io/180hz_video-downloader/)
2. Click **⚡ Download App (.exe)** to download `TurboDownloaderSetup.exe`.
3. Run the installer. It will automatically configure everything and launch ready to download!

---

### For Developers (Manual / Portable):
1. Clone the repository:
   ```bash
   git clone https://github.com/tanjeem180hz/180hz_video-downloader.git
   cd 180hz_video-downloader
   ```

2. Run **`TurboDownloader.exe`**:
   Double click `TurboDownloader.exe` or run:
   ```cmd
   .\TurboDownloader.exe
   ```
   *(Missing dependencies like yt-dlp and ffmpeg are automatically self-healed and fetched).*

3. Paste your video or direct download link into the console, select your desired resolution (e.g. 4K Ultra HD or Best Available), and click **Download**!

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
