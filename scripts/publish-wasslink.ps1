$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

$desktopProject = Join-Path $root 'src\Apps\WASSLink.Desktop\WASSLink.Desktop.csproj'
$profile = Join-Path $root 'src\Apps\WASSLink.Desktop\Properties\PublishProfiles\Win64-SingleFile.pubxml'
$ytDlpSource = Join-Path $root 'tools\yt-dlp\yt-dlp.exe'
$ffmpegSource = Join-Path $root 'tools\ffmpeg\ffmpeg.exe'
$instructionsSource = Join-Path $root 'release\WASSLink-Desktop-README.txt'
$noticesSource = Join-Path $root 'release\THIRD-PARTY-NOTICES.txt'
$betaRoot = Join-Path $root 'artifacts\beta'
$packageName = 'GowLink-Desktop-win'
$packageDir = Join-Path $betaRoot $packageName
$stagingDir = Join-Path $betaRoot "$packageName-staging"
$archivePath = Join-Path $betaRoot "$packageName.zip"

foreach ($requiredFile in @($desktopProject, $profile, $ytDlpSource, $ffmpegSource, $instructionsSource, $noticesSource)) {
	if (-not (Test-Path $requiredFile -PathType Leaf)) {
		throw "Required release file not found: $requiredFile"
	}
}

New-Item -ItemType Directory -Path $betaRoot -Force | Out-Null
if (Test-Path $stagingDir) {
	Remove-Item $stagingDir -Recurse -Force
}

$publishOutput = $stagingDir + [IO.Path]::DirectorySeparatorChar
Write-Host 'Publishing GowLink Desktop for Windows x64...'
& dotnet publish $desktopProject -c Release -p:PublishProfile=Win64-SingleFile "-p:PublishDir=$publishOutput"
if ($LASTEXITCODE -ne 0) {
	throw "Desktop publish failed with exit code $LASTEXITCODE."
}

$appExecutable = Join-Path $stagingDir 'WASSLink.Desktop.exe'
if (-not (Test-Path $appExecutable -PathType Leaf)) {
	throw "Published application executable not found: $appExecutable"
}

Get-ChildItem $stagingDir -Filter '*.pdb' -Recurse -File | Remove-Item -Force

$ytDlpTarget = Join-Path $stagingDir 'tools\yt-dlp'
$ffmpegTarget = Join-Path $stagingDir 'tools\ffmpeg'
New-Item -ItemType Directory -Path $ytDlpTarget, $ffmpegTarget -Force | Out-Null
Copy-Item $ytDlpSource (Join-Path $ytDlpTarget 'yt-dlp.exe')
Copy-Item $ffmpegSource (Join-Path $ffmpegTarget 'ffmpeg.exe')
Copy-Item $instructionsSource (Join-Path $stagingDir 'LEEME.txt')
Copy-Item $noticesSource (Join-Path $stagingDir 'THIRD-PARTY-NOTICES.txt')

if (Test-Path $packageDir) {
	Remove-Item $packageDir -Recurse -Force
}
Move-Item $stagingDir $packageDir

if (Test-Path $archivePath) {
	Remove-Item $archivePath -Force
}
Compress-Archive -Path $packageDir -DestinationPath $archivePath -CompressionLevel Optimal

Write-Host "Portable folder: $packageDir"
Write-Host "Shareable ZIP:   $archivePath"
