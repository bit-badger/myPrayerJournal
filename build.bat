@echo off
cls
.paket\paket.exe restore
if errorlevel 1 (
  exit /b %errorlevel%
)
"packages\FAKE\tools\Fake.exe" build.fsx %1
pause