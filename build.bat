@echo off
echo ========================================================
echo   Compiling TurboDownloader 2.0 (Modern CSS UI + 4K/8K Engine)...
echo ========================================================
set CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe

if not exist "%CSC%" (
    echo Error: C# compiler csc.exe not found at %CSC%
    pause
    exit /b 1
)

"%CSC%" /target:winexe /optimize+ /r:System.dll,System.Core.dll,System.Drawing.dll,System.Windows.Forms.dll,System.Web.Extensions.dll /out:"TurboDownloader.exe" "TurboDownloader.cs"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================================
    echo   BUILD SUCCESSFUL!
    echo   File generated: TurboDownloader.exe
    echo ========================================================
) else (
    echo.
    echo BUILD FAILED!
)
