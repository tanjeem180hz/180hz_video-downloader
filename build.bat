@echo off
echo ========================================================
echo   Compiling TurboDownloader 2.0 (Modern CSS UI + 4K/8K Engine)...
echo ========================================================
set CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe

if not exist "%CSC%" (
    echo Error: C# compiler csc.exe not found at %CSC%
    exit /b 1
)

"%CSC%" /target:winexe /optimize+ /r:System.dll,System.Core.dll,System.Drawing.dll,System.Windows.Forms.dll,System.Web.Extensions.dll /out:"TurboDownloader.exe" "TurboDownloader.cs"
if %ERRORLEVEL% NEQ 0 (
    echo BUILD FAILED for TurboDownloader.exe!
    exit /b 1
)

echo Compiling TurboDownloaderSetup.exe installer...
"%CSC%" /target:winexe /optimize+ /r:System.dll,System.Core.dll,System.Drawing.dll,System.Windows.Forms.dll /resource:TurboDownloader.exe /resource:web\index.html,index.html /resource:web\styles.css,styles.css /resource:web\app.js,app.js /out:"TurboDownloaderSetup.exe" "TurboDownloaderInstaller.cs"
if %ERRORLEVEL% NEQ 0 (
    echo BUILD FAILED for TurboDownloaderSetup.exe!
    exit /b 1
)

copy /Y "TurboDownloaderSetup.exe" "web\TurboDownloaderSetup.exe" >nul
copy /Y "TurboDownloader.exe" "web\TurboDownloader.exe" >nul

echo.
echo ========================================================
echo   BUILD SUCCESSFUL!
echo   Generated: TurboDownloader.exe and TurboDownloaderSetup.exe
echo ========================================================
