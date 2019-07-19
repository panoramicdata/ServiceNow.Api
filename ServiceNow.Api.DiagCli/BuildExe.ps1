$ErrorActionPreference = "Stop"
$assemblyName = "ServiceNow.Api.DiagCli"

# https://github.com/Hubert-Rybak/dotnet-warp
# Uncomment the line below if you do not have dotnet-warp installed. If you do this will not run if this is uncommented.
#dotnet tool install --global dotnet-warp

dotnet clean -c release
dotnet build -c release

# Create the release exe
dotnet-warp --no-crossgen --verbose

# Get the version
$dllPath = "bin\release\netcoreapp2.2\win-x64\${assemblyName}.dll"
$version = (Get-Command $dllPath).FileVersionInfo
$major = $version.FileMajorPart
$minor = $version.FileMinorPart
$build = $version.FileBuildPart
$versionString = @($major, $minor, $build) -join "."

# Rename the exe
move "${assemblyName}.exe" "${assemblyName}.${versionString}.exe" -Force