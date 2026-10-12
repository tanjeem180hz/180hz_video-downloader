@echo off
echo ========================================================
echo   Compiling 180Hz Downloader 2.0 (Modern CSS UI + 4K/8K Engine)...
echo ========================================================
set CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe

if not exist "%CSC%" (
    echo Error: C# compiler csc.exe not found at %CSC%
    exit /b 1
)

"%CSC%" /target:winexe /optimize+ /win32icon:app.ico /r:System.dll,System.Core.dll,System.Drawing.dll,System.Windows.Forms.dll,System.Web.Extensions.dll /out:"180Hz Downloader.exe" "TurboDownloader.cs"
if %ERRORLEVEL% NEQ 0 (
    echo BUILD FAILED for 180Hz Downloader.exe!
    exit /b 1
)

copy /Y "180Hz Downloader.exe" "TurboDownloader.exe" >nul
copy /Y "180Hz Downloader.exe" "180HzDownloader.exe" >nul

echo Compiling 180Hz Downloader Setup.exe installer...
"%CSC%" /target:winexe /optimize+ /win32icon:setup.ico /r:System.dll,System.Core.dll,System.Drawing.dll,System.Windows.Forms.dll /resource:"180Hz Downloader.exe","180Hz Downloader.exe" /resource:"180Hz Downloader.exe",TurboDownloader.exe /resource:app.ico,app.ico /resource:web\index.html,index.html /resource:web\styles.css,styles.css /resource:web\app.js,app.js /resource:web\version.json,version.json /resource:web\click.mp3,click.mp3 /out:"180Hz Downloader Setup.exe" "TurboDownloaderInstaller.cs"
if %ERRORLEVEL% NEQ 0 (
    echo BUILD FAILED for 180Hz Downloader Setup.exe!
    exit /b 1
)

copy /Y "180Hz Downloader Setup.exe" "TurboDownloaderSetup.exe" >nul
copy /Y "180Hz Downloader Setup.exe" "180HzDownloaderSetup.exe" >nul

copy /Y "180Hz Downloader Setup.exe" "web\180Hz Downloader Setup.exe" >nul
copy /Y "180Hz Downloader.exe" "web\180Hz Downloader.exe" >nul
copy /Y "180HzDownloaderSetup.exe" "web\180HzDownloaderSetup.exe" >nul
copy /Y "180HzDownloader.exe" "web\180HzDownloader.exe" >nul
copy /Y "TurboDownloaderSetup.exe" "web\TurboDownloaderSetup.exe" >nul
copy /Y "TurboDownloader.exe" "web\TurboDownloader.exe" >nul

copy /Y "180Hz Downloader Setup.exe" "docs\180Hz Downloader Setup.exe" >nul
copy /Y "180Hz Downloader.exe" "docs\180Hz Downloader.exe" >nul
copy /Y "180HzDownloaderSetup.exe" "docs\180HzDownloaderSetup.exe" >nul
copy /Y "180HzDownloader.exe" "docs\180HzDownloader.exe" >nul
copy /Y "TurboDownloaderSetup.exe" "docs\TurboDownloaderSetup.exe" >nul
copy /Y "TurboDownloader.exe" "docs\TurboDownloader.exe" >nul
copy /Y "web\index.html" "docs\index.html" >nul
copy /Y "web\styles.css" "docs\styles.css" >nul
copy /Y "web\app.js" "docs\app.js" >nul
copy /Y "web\version.json" "docs\version.json" >nul
copy /Y "web\click.mp3" "docs\click.mp3" >nul

echo.
echo ========================================================
echo   BUILD SUCCESSFUL!
echo   Generated: 180Hz Downloader.exe and 180Hz Downloader Setup.exe
echo ========================================================
