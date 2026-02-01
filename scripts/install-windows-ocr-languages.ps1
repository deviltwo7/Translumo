#Requires -RunAsAdministrator
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$ocrLanguages = @(
    "en-US",
    "de-DE",
    "ru-RU",
    "ja-JP",
    "zh-CN",
    "ko-KR"
)

$capabilities = $ocrLanguages | ForEach-Object { "Language.OCR~~~$($_)~0.0.1.0" }

Write-Host "Checking Windows OCR capabilities..."
$installed = Get-WindowsCapability -Online | Where-Object { $_.Name -in $capabilities }
$missing = $installed | Where-Object { $_.State -ne "Installed" }

if (-not $missing) {
    Write-Host "All requested OCR language packs are already installed." -ForegroundColor Green
    return
}

foreach ($capability in $missing) {
    Write-Host "Installing $($capability.Name)..."
    Add-WindowsCapability -Online -Name $capability.Name | Out-Null
}

Write-Host "Done. OCR language packs installed." -ForegroundColor Green
