@echo off
setlocal
set CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe
if not exist "%CSC%" set CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe

"%CSC%" /nologo /target:winexe /platform:anycpu /out:UnitConverter.exe ^
    /reference:System.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll ^
    Program.cs

if %ERRORLEVEL% neq 0 (
    echo Build failed.
    exit /b 1
)
echo Build succeeded: UnitConverter.exe
