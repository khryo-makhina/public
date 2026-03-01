param(
    [Parameter(Mandatory=$true)]
    [string]$destinationPath
)

$sourcePath = ".\bin\Debug\net8.0"
$requiredFiles = @(
    "FilenameSanitizerCli.exe",
    "FilenameSanitizerCli.dll",
    "FilenameSanitizer.dll",
    "FilenameSanitizerCli.runtimeconfig.json",
    "System.Text.Json.dll",
    "System.Text.Encodings.Web.dll",
    "System.IO.Pipelines.dll"
)

# Create destination directory if it doesn't exist
New-Item -ItemType Directory -Force -Path $destinationPath

# Copy required files
foreach ($file in $requiredFiles) {
    Copy-Item "$sourcePath\$file" -Destination $destinationPath
}

# Copy runtime components
Copy-Item "$sourcePath\runtimes" -Destination $destinationPath -Recurse
