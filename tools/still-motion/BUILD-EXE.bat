@echo off
cd /d "%~dp0"
set "CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist "%CSC%" set "CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe"
if not exist "%CSC%" (echo C# compiler not found.&pause&exit /b 1)
"%CSC%" /nologo /target:winexe /optimize+ /out:"TASY-Still-Motion.exe" /reference:System.dll /reference:System.Core.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll "TASY-Still-Motion.cs"
if errorlevel 1 (echo BUILD FAILED&pause&exit /b 1)
echo READY: TASY-Still-Motion.exe
pause
