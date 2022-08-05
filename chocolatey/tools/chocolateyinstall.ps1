$ErrorActionPreference = 'Stop'

$toolsDir = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$applicationName = "LogExpert"
$applicationPath = "$toolsDir\$applicationName.exe"

# create start menu shortcut only for current user
Install-ChocolateyShortcut -shortcutFilePath "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\$applicationName.lnk" -targetpath $applicationPath

# create context menu entry only for current user
$registryKey = "Registry::HKEY_CURRENT_USER\SOFTWARE\Classes\*\shell\$applicationName"

if (Test-Path $registryKey) {
    Remove-Item -Path $registryKey -Recurse
}

New-Item -Path $registryKey -Force | Out-Null
New-ItemProperty -Path $registryKey -Name "(Default)" -Value "Open with $applicationName" | Out-Null
New-ItemProperty -Path $registryKey -Name "Icon" -Value $applicationPath | Out-Null

$registryCommand = "$registryKey\command"
New-Item -Path $registryCommand | Out-Null
New-ItemProperty -Path $registryCommand -Name "(Default)" -Value "`"$applicationPath`" `"%1`"" | Out-Null
