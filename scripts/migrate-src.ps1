# ============================================
# WASSLink Studio - Source Migration
# Version: 1.0
# ============================================

Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host " WASSLink Studio - SRC Migration" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

$apps = @(
    "WASSLink.Desktop",
    "WASSLink.CLI",
    "WASSLink.Mobile"
)

$core = @(
    "WASSLink.Abstractions",
    "WASSLink.Shared",
    "WASSLink.Configuration",
    "WASSLink.Download",
    "WASSLink.Library",
    "WASSLink.Player",
    "WASSLink.Search",
    "WASSLink.Plugins"
)

foreach ($folder in @("src/Apps","src/Core")) {
    if (!(Test-Path $folder)) {
        New-Item -ItemType Directory -Path $folder | Out-Null
        Write-Host "[CREADA] $folder" -ForegroundColor Green
    }
}


foreach ($project in $apps) {

    $source = "src/$project"
    $target = "src/Apps/$project"

    if (Test-Path $source) {

        Move-Item $source $target
        Write-Host "[MOVIDO] $project -> Apps" -ForegroundColor Yellow

    }
}


foreach ($project in $core) {

    $source = "src/$project"
    $target = "src/Core/$project"

    if (Test-Path $source) {

        Move-Item $source $target
        Write-Host "[MOVIDO] $project -> Core" -ForegroundColor Yellow

    }
}


Write-Host ""
Write-Host "Migracion completada." -ForegroundColor Cyan
Write-Host ""