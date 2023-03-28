$ErrorActionPreference = 'Stop'

$applicationName = "LogExpert"

# remove start menu shortcut
Remove-Item "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\$applicationName.lnk"

# remote context menu entry
Remove-Item -Path "Registry::HKEY_CURRENT_USER\SOFTWARE\Classes\*\shell\$applicationName" -Recurse
