@echo off
cls
"packages\FAKE\tools\Fake.exe" build.fsx %1
pause