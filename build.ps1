# Build script for Jellyfin.Plugin.Addic7ed

# Ensure we have the required .NET SDK
$requiredDotNetVersion = "6.0"
$dotnetInfo = dotnet --version
if (-not $dotnetInfo.StartsWith($requiredDotNetVersion)) {
    Write-Warning "This plugin requires .NET SDK $requiredDotNetVersion. Please install it from https://dotnet.microsoft.com/download/dotnet/$requiredDotNetVersion"
    exit 1
}

# Clean previous builds
if (Test-Path -Path "bin") {
    Write-Host "Cleaning previous build..."
    Remove-Item -Recurse -Force "bin"
}

# Build the plugin
Write-Host "Building Jellyfin.Plugin.Addic7ed..."
dotnet build -c Release

# Check if build was successful
if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}

# Create output directory
$outputDir = "dist"
if (-not (Test-Path -Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir | Out-Null
}

# Copy the built DLL to the output directory
Write-Host "Copying files to $outputDir..."
Copy-Item -Path "bin\Release\net6.0\Jellyfin.Plugin.Addic7ed.dll" -Destination $outputDir

# Create a zip file for distribution
$version = (Get-Date -Format "yyyyMMdd")
$zipFile = "$outputDir\Jellyfin.Plugin.Addic7ed_$version.zip"
Write-Host "Creating zip file: $zipFile..."
Compress-Archive -Path "$outputDir\Jellyfin.Plugin.Addic7ed.dll" -DestinationPath $zipFile -Force

Write-Host "Build completed successfully!"
Write-Host "Plugin file: $outputDir\Jellyfin.Plugin.Addic7ed.dll"
Write-Host "Zip file: $zipFile"
Write-Host ""
Write-Host "To install the plugin, copy the DLL file to your Jellyfin plugins directory and restart Jellyfin."