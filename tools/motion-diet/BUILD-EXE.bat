@echo off
setlocal
cd /d "%~dp0"

set "CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist "%CSC%" set "CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe"

if not exist "%CSC%" (
  echo .NET Framework C# compiler not found.
  pause
  exit /b 1
)

"%CSC%" /nologo /target:winexe /optimize+ /out:"TASY-Motion-Diet.exe" /reference:System.dll /reference:System.Core.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll "TASY-Motion-Diet.cs"

if errorlevel 1 (
  echo.
  echo BUILD FAILED
  pause
  exit /b 1
)

echo.
echo READY: TASY-Motion-Diet.exe
pause
