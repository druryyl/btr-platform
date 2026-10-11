<#
.SYNOPSIS
    Builds and packages BTR Retur Gudang into a self-contained portable distribution folder and ZIP.
.DESCRIPTION
    Compiles the solution in Release configuration, publishes all dependent assemblies (Dapper, RestSharp,
    Microsoft.Extensions.DependencyInjection, etc.) to a standalone xcopy-ready folder without installer wizards.
#>

param(
    [string]$Configuration = "Release",
    [string]$OutputDir = "$PSScriptRoot\dist\BtrReturGudang-Portable"
)

$ErrorActionPreference = "Stop"

Write-Host "============================================================" -ForegroundColor Cyan
Write-Host " BTR Retur Gudang - Portable Release Distribution Packager" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan

$projectPath = Join-Path $PSScriptRoot "BtrReturGudang.Winform\BtrReturGudang.Winform.csproj"
$publishDir = Join-Path $PSScriptRoot "BtrReturGudang.Winform\bin\$Configuration\net471\publish"

Write-Host "`n1. Running dotnet publish in $Configuration mode..." -ForegroundColor Yellow
dotnet publish $projectPath -c $Configuration --no-self-contained

if ($LASTEXITCODE -ne 0) {
    Write-Error "dotnet publish failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}

Write-Host "`n2. Preparing distribution directory at: $OutputDir" -ForegroundColor Yellow
if (Test-Path $OutputDir) {
    Remove-Item -Path $OutputDir -Recurse -Force
}
New-Item -Path $OutputDir -ItemType Directory -Force | Out-Null

Write-Host "3. Copying published binaries to distribution output..." -ForegroundColor Yellow
Copy-Item -Path "$publishDir\*" -Destination $OutputDir -Recurse -Force

# Create portable launch batch helper
$launcherBatch = Join-Path $OutputDir "Run-BtrReturGudang.bat"
@"
@echo off
start "" "%~dp0BtrReturGudang.Winform.exe"
"@ | Set-Content -Path $launcherBatch -Encoding ASCII

# Create ZIP archive
$zipPath = "$PSScriptRoot\dist\BtrReturGudang-Portable.zip"
if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}
Write-Host "4. Creating distribution ZIP archive at: $zipPath" -ForegroundColor Yellow
Compress-Archive -Path "$OutputDir\*" -DestinationPath $zipPath -Force

Write-Host "`nDistribution package created successfully!" -ForegroundColor Green
Write-Host "Folder: $OutputDir" -ForegroundColor Green
Write-Host "Archive: $zipPath" -ForegroundColor Green
Get-ChildItem -Path $OutputDir | Select-Object Name, Length | Format-Table -AutoSize
