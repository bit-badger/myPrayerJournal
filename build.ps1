#!/snap/bin/pwsh
Set-Location ./src/my-prayer-journal
npm run build
if ($?) {
  Copy-Item ./dist/my-prayer-journal/* ../MyPrayerJournal.Api/wwwroot
  Set-Location ../MyPrayerJournal.Api
  dotnet run
}
