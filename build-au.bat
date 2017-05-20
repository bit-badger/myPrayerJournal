@echo off
cd .\src\app
au build
cd ..\..
exit %errorlevel%