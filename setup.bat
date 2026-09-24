@echo off
echo ========================================================
echo   TurboDownloader 2.0 (PEAK/8K Engine) Setup
echo ========================================================
echo Checking dependencies...

if not exist "yt-dlp.exe" (
    echo Downloading yt-dlp.exe...
    curl.exe -L "https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe" -o "yt-dlp.exe"
) else (
    echo yt-dlp.exe is already present.
)

if not exist "ffmpeg.exe" (
    echo Installing ffmpeg via winget...
    winget install --id yt-dlp.FFmpeg --accept-package-agreements --accept-source-agreements
    for /f "delims=" %%i in ('powershell -Command "(Get-ChildItem -Path \"$env:LOCALAPPDATA\Microsoft\WinGet\" -Recurse -Filter \"ffmpeg.exe\" -ErrorAction SilentlyContinue | Select-Object -First 1).FullName"') do (
        copy "%%i" "ffmpeg.exe"
    )
) else (
    echo ffmpeg.exe is already present.
)

echo.
echo Compiling TurboDownloader.exe...
call build.bat

echo.
echo Setup completed successfully! You can now run TurboDownloader.exe
pause
