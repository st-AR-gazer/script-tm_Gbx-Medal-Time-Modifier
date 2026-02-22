param(
    [string]$Runtime = "win-x64",
    [string]$Configuration = "Release",
    [string]$OutputDir = "dist"
)

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectPath = Join-Path $scriptDir "GbxMedalTimeModifier.csproj"
$publishDir = Join-Path $scriptDir (Join-Path $OutputDir $Runtime)
$exeName = "GbxMedalTimeModifier.exe"

if (!(Test-Path $projectPath)) {
    Write-Error "Project file not found: $projectPath"
    exit 1
}

Write-Host "Publishing standalone single-file executable..."
dotnet publish $projectPath `
    -c $Configuration `
    -r $Runtime `
    --self-contained true `
    /p:PublishSingleFile=true `
    /p:IncludeNativeLibrariesForSelfExtract=true `
    /p:PublishTrimmed=false `
    -o $publishDir

if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$publishedExePath = Join-Path $publishDir $exeName
if (!(Test-Path $publishedExePath)) {
    Write-Error "Published exe not found: $publishedExePath"
    exit 1
}

$rootExePath = Join-Path $scriptDir $exeName
Copy-Item $publishedExePath $rootExePath -Force

$licensePath = Join-Path $scriptDir "LICENSE"
if (Test-Path $licensePath) {
    Copy-Item $licensePath $publishDir -Force
}

Write-Host "Published to: $publishDir"
Write-Host "Copied exe to repo root: $rootExePath"
