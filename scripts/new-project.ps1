# ============================================
# WASSLink Studio - New Project Generator
# Version: 1.0
# ============================================

param(
    [Parameter(Mandatory=$true)]
    [string]$ProjectName,

    [Parameter(Mandatory=$true)]
    [ValidateSet("Core","Apps","Tests")]
    [string]$ProjectType
)


$solution = "WASSLink-Studio.slnx"


switch ($ProjectType) {

    "Core" {
        $path = "src/Core/$ProjectName"
        $template = "classlib"
    }

    "Apps" {
        $path = "src/Apps/$ProjectName"
        $template = "console"
    }

    "Tests" {
        $path = "tests/$ProjectName"
        $template = "xunit"
    }
}


Write-Host ""
Write-Host "===================================="
Write-Host " Creating $ProjectName"
Write-Host " Type: $ProjectType"
Write-Host " Path: $path"
Write-Host "===================================="
Write-Host ""


if (Test-Path $path) {

    $existingProject = Get-ChildItem $path -Filter *.csproj

    if ($existingProject) {

        Write-Host "[ERROR] Project already exists:"
        Write-Host $existingProject.Name
        exit 1

    }
    else {

        Write-Host "[INFO] Folder exists but no project found"
        Write-Host "[INFO] Creating project inside existing folder"

    }

}


dotnet new $template `
    -n $ProjectName `
    -f net10.0 `
    -o $path


dotnet sln $solution add "$path/$ProjectName.csproj"


Write-Host ""
Write-Host "[OK] Project created successfully"
Write-Host ""
