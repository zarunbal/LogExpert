$ErrorActionPreference = 'Stop'

$applicationName = "LogExpert"

# remove start menu shortcut
Remove-Item "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\$applicationName.lnk"
