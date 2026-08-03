# ============================================
# WASSLink Studio - Setup
# Structure Validator & Creator
# Version: 1.0
# ============================================

Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host " WASSLink Studio - Project Setup" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

$folders = @(
    ".github",

    "assets",

    "developer",
    "developer/adr",

    "docs",

    "examples",

    "plugins",

    "resources",
    "resources/languages",
    "resources/themes",
    "resources/templates",

    "scripts",

    "src",
    "src/Apps",
    "src/Core",

    "tests",

    "tools",
    "tools/ffmpeg",
    "tools/yt-dlp"
)


$files = @(
    "README.md",
    "LICENSE",
    ".gitignore",
    ".editorconfig",
    ".gitattributes",

    "developer/PROJECT_MANIFEST.md",
    "developer/AI_GUIDE.md",
    "developer/ROADMAP.md",
    "developer/WORKSPACE.md",
    "developer/CHANGELOG.md",
    "developer/GLOSSARY.md"
)


Write-Host "Verificando carpetas..." -ForegroundColor Yellow
Write-Host ""

foreach ($folder in $folders) {

    if (Test-Path $folder) {

        Write-Host "[OK]      $folder" -ForegroundColor DarkGray

    }
    else {

        New-Item -ItemType Directory -Path $folder | Out-Null
        Write-Host "[CREADA]  $folder" -ForegroundColor Green

    }
}


Write-Host ""
Write-Host "Verificando archivos..." -ForegroundColor Yellow
Write-Host ""

foreach ($file in $files) {

    if (Test-Path $file) {

        Write-Host "[OK]      $file" -ForegroundColor DarkGray

    }
    else {

        New-Item -ItemType File -Path $file | Out-Null
        Write-Host "[CREADO]  $file" -ForegroundColor Green

    }
}


Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host " Verificacion completada" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""
