<#
.SYNOPSIS
    Builds the mod in Release and packs it into a zip for GitHub Releases / Nexus.

.EXAMPLE
    .\build-release.ps1
    Builds the version from the .csproj -> dist\CUAchievements-<version>.zip

.EXAMPLE
    .\build-release.ps1 -Suffix beta.1
    Same, but the zip is named CUAchievements-<version>-beta.1.zip
#>
[CmdletBinding()]
param(
    # Mod version. If omitted, <Version> from the .csproj is used.
    [string]$Version,

    # Optional label added ONLY to the zip name (e.g. "beta.1").
    # The version inside the DLL stays as is.
    [string]$Suffix,

    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'

# ---- Settings: adjust once ---------------------------------------------------
$AssemblyName = 'CUAchievements'
$ProjectPath  = Join-Path $PSScriptRoot 'CUAchievements\CUAchievements.csproj'

# Extra files/folders (relative to the repo root) that are shipped inside the
# mod folder next to the DLL. Add locale files etc. here if they are not
# embedded into the DLL, e.g. 'CUAchievements\Locales'.
$ExtraItems   = @('README.md', 'LICENSE.txt', "CUAchievements\Locales")
# -----------------------------------------------------------------------------

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw "dotnet SDK not found. Install it or run the script from a Developer PowerShell."
}
if (-not (Test-Path $ProjectPath)) {
    throw "Project not found: $ProjectPath"
}

# Version: parameter > <Version> in the .csproj
if (-not $Version) {
    [xml]$csproj = Get-Content $ProjectPath
    $Version = @($csproj.Project.PropertyGroup.Version) | Where-Object { $_ } | Select-Object -First 1
}
if (-not $Version) {
    throw "Version not found. Pass -Version 1.2.3 or add <Version> to the .csproj."
}
$label = if ($Suffix) { "$Version-$Suffix" } else { $Version }

$dist     = Join-Path $PSScriptRoot 'dist'
$buildDir = Join-Path $dist 'build'
$stage    = Join-Path $dist 'stage'
$modDir   = Join-Path $stage $AssemblyName
$zipPath  = Join-Path $dist "$AssemblyName-$label.zip"

# Clean start: nothing left over from a previous build can end up in the zip
if (Test-Path $dist) { Remove-Item $dist -Recurse -Force }
New-Item -ItemType Directory -Path $buildDir, $modDir | Out-Null

Write-Host "Building $AssemblyName $label ($Configuration)..." -ForegroundColor Cyan
dotnet build $ProjectPath -c $Configuration -o $buildDir --nologo -v:minimal
if ($LASTEXITCODE -ne 0) { throw "Build failed." }

# Whitelist what goes into the mod folder (never copy the whole build output:
# it may contain game or Unity DLLs that must not be redistributed)
$dll = Join-Path $buildDir "$AssemblyName.dll"
if (-not (Test-Path $dll)) { throw "$AssemblyName.dll not found in $buildDir" }
Copy-Item $dll $modDir

$xmlDoc = Join-Path $buildDir "$AssemblyName.xml"
if (Test-Path $xmlDoc) {
    Copy-Item $xmlDoc $modDir
} else {
    Write-Warning "$AssemblyName.xml not found. Enable <GenerateDocumentationFile> in the .csproj to ship the XML docs."
}

foreach ($item in $ExtraItems) {
    $src = Join-Path $PSScriptRoot $item
    if (-not (Test-Path $src)) {
        Write-Warning "Extra item not found, skipped: $item"
        continue
    }
    Copy-Item $src $modDir -Recurse
}

# Zip with the mod folder at the root: extract into BepInEx\plugins and it just works
Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory(
    $stage, $zipPath, [System.IO.Compression.CompressionLevel]::Optimal, $false)

Write-Host "`nZip contents:" -ForegroundColor Cyan
$zip = [System.IO.Compression.ZipFile]::OpenRead($zipPath)
try {
    $zip.Entries | ForEach-Object { '  {0}  ({1:N0} KB)' -f $_.FullName, ($_.Length / 1KB) }
}
finally {
    $zip.Dispose()
}

$hash = (Get-FileHash $zipPath -Algorithm SHA256).Hash
Write-Host "`nDone: $zipPath" -ForegroundColor Green
Write-Host "SHA256: $hash"
