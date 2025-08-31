# PowerShell script to merge ServerSync.dll into BetterSapCollector.dll
# Run this script after building if automatic ILMerge doesn't work

$outputDir = "bin\Debug"
$mainDll = "$outputDir\BetterSapCollector.dll"
$serverSyncDll = "$outputDir\ServerSync.dll"
$mergedDll = "$outputDir\BetterSapCollector.Merged.dll"

# Check if both files exist
if (!(Test-Path $mainDll)) {
    Write-Error "Main DLL not found: $mainDll"
    exit 1
}

if (!(Test-Path $serverSyncDll)) {
    Write-Host "ServerSync.dll not found. Skipping merge."
    exit 0
}

# Try to find ILMerge
$ilmergePaths = @(
    "packages\ILMerge.3.0.41\tools\net452\ILMerge.exe",
    "C:\Program Files (x86)\Microsoft\ILMerge\ILMerge.exe",
    "C:\Program Files\Microsoft\ILMerge\ILMerge.exe",
    "ILMerge.exe"
)

$ilmerge = $null
foreach ($path in $ilmergePaths) {
    if (Test-Path $path -ErrorAction SilentlyContinue) {
        $ilmerge = $path
        break
    }
}

if (!$ilmerge) {
    # Try to find it in PATH
    try {
        $ilmerge = (Get-Command ILMerge.exe -ErrorAction Stop).Source
    } catch {
        Write-Warning "ILMerge.exe not found. Please install ILMerge or download it manually."
        Write-Host "Download from: https://www.nuget.org/packages/ILMerge/"
        exit 1
    }
}

Write-Host "Using ILMerge: $ilmerge"

# Get Valheim paths
$valheimPath = "D:\SteamLibrary\steamapps\common\Valheim"
$managedPath = "$valheimPath\valheim_Data\Managed"
$publicizedPath = "$valheimPath\valheim_Data\Managed\publicized_assemblies"
$bepinexPath = "$valheimPath\BepInEx\core"

# Run ILMerge with library paths
Write-Host "Merging $mainDll with $serverSyncDll..."
& "$ilmerge" /target:library /out:"$mergedDll" /lib:"$managedPath" /lib:"$publicizedPath" /lib:"$bepinexPath" /lib:"$outputDir" /closed "$mainDll" "$serverSyncDll"

if ($LASTEXITCODE -eq 0) {
    # Replace original with merged
    Remove-Item $mainDll
    Rename-Item $mergedDll (Split-Path $mainDll -Leaf)
    Remove-Item $serverSyncDll
    Write-Host "ILMerge completed successfully!" -ForegroundColor Green
} else {
    Write-Error "ILMerge failed with exit code $LASTEXITCODE"
    if (Test-Path $mergedDll) {
        Remove-Item $mergedDll
    }
    exit 1
}
