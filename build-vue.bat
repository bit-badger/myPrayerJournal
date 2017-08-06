@echo off
cd .\src\app
npm run build prod
cd ..\..
exit %errorlevel%