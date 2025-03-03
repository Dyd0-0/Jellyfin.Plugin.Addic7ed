#!/bin/bash

# Build script for Jellyfin.Plugin.Addic7ed

# Ensure we have the required .NET SDK
required_dotnet_version="6.0"
dotnet_version=$(dotnet --version)

if [[ ! $dotnet_version == $required_dotnet_version* ]]; then
    echo "This plugin requires .NET SDK $required_dotnet_version. Please install it from https://dotnet.microsoft.com/download/dotnet/$required_dotnet_version"
    exit 1
fi

# Clean previous builds
if [ -d "bin" ]; then
    echo "Cleaning previous build..."
    rm -rf bin
fi

# Build the plugin
echo "Building Jellyfin.Plugin.Addic7ed..."
dotnet build -c Release

# Check if build was successful
if [ $? -ne 0 ]; then
    echo "Build failed with exit code $?"
    exit $?
fi

# Create output directory
output_dir="dist"
if [ ! -d "$output_dir" ]; then
    mkdir -p "$output_dir"
fi

# Copy the built DLL to the output directory
echo "Copying files to $output_dir..."
cp bin/Release/net6.0/Jellyfin.Plugin.Addic7ed.dll "$output_dir/"

# Create a zip file for distribution
version=$(date +"%Y%m%d")
zip_file="$output_dir/Jellyfin.Plugin.Addic7ed_$version.zip"
echo "Creating zip file: $zip_file..."
(cd "$output_dir" && zip -j "$zip_file" Jellyfin.Plugin.Addic7ed.dll)

echo "Build completed successfully!"
echo "Plugin file: $output_dir/Jellyfin.Plugin.Addic7ed.dll"
echo "Zip file: $zip_file"
echo ""
echo "To install the plugin, copy the DLL file to your Jellyfin plugins directory and restart Jellyfin."