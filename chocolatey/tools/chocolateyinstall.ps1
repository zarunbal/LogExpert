$ErrorActionPreference = 'Stop'

$toolsDir = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$applicationName = "LogExpert"
$applicationPath = "$toolsDir\$applicationName.exe"

# create start menu shortcut only for current user
Install-ChocolateyShortcut -shortcutFilePath "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\$applicationName.lnk" -targetpath $applicationPath
