@echo off
setlocal
set CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe
if not exist "%CSC%" set CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe
if not exist "%CSC%" (
  echo C# compiler not found.
  pause
  exit /b 1
)
"%CSC%" /nologo /unsafe /target:winexe /optimize+ /out:TASY-Water-Motion.exe /reference:System.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll TASY-Water-Motion.cs
if errorlevel 1 pause
