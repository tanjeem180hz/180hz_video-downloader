@echo off
echo ========================================================
echo   180Hz Downloader 2.3 (All-in-One Engine) Setup
echo ========================================================
echo Checking dependencies...

if not exist "yt-dlp.exe" (
    echo Downloading yt-dlp.exe (Latest Nightly Engine)...
    curl.exe -L "https://github.com/yt-dlp/yt-dlp-nightly-builds/releases/latest/download/yt-dlp.exe" -o "yt-dlp.exe"
) else (
    echo Updating yt-dlp.exe to latest engine...
    yt-dlp.exe --update-to nightly
)

if not exist "ffmpeg.exe" (
    echo Installing ffmpeg via winget...
    winget install --id yt-dlp.FFmpeg --accept-package-agreements --accept-source-agreements
    for /f "delims=" %%i in ('powershell -Command "(Get-ChildItem -Path \"$env:LOCALAPPDATA\Microsoft\WinGet\" -Recurse -Filter \"ffmpeg.exe\" -ErrorAction SilentlyContinue | Select-Object -First 1).FullName"') do (
        copy "%%i" "ffmpeg.exe"
    )
)

if not exist "ffmpeg.exe" (
    echo Downloading ffmpeg package directly...
    curl.exe -L "https://github.com/yt-dlp/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-win64-gpl.zip" -o "ffmpeg_temp.zip"
    powershell -Command "Add-Type -AssemblyName System.IO.Compression.FileSystem; `$zip = [System.IO.Compression.ZipFile]::OpenRead('ffmpeg_temp.zip'); foreach (`$entry in `$zip.Entries) { if (`$entry.Name -eq 'ffmpeg.exe') { [System.IO.Compression.ZipFileExtensions]::ExtractToFile(`$entry, 'ffmpeg.exe', `$true); break; } }; `$zip.Dispose(); Remove-Item ffmpeg_temp.zip -Force -ErrorAction SilentlyContinue"
)

if exist "ffmpeg.exe" (
    echo ffmpeg.exe is present and ready.
) else (
    echo Warning: ffmpeg.exe was not acquired automatically.
)

echo.
echo Compiling 180Hz Downloader.exe...
call build.bat

echo.
echo Setup completed successfully! You can now run 180Hz Downloader.exe
pause
