<#
.SYNOPSIS
    Genclik Merkezi - Modular Monolith solution iskeletini olusturur.

.DESCRIPTION
    ARCHITECTURE.md #6 (Solution Structure) ve #7 (Module Architecture) dokumanlarinda
    tanimlanan klasor/proje yapisini ADGM klasoru altinda olusturur:

    ADGM/
    ├── GenclikMerkezi.sln
    ├── src/
    │   ├── Host/GenclikMerkezi.Api                (ASP.NET Core Web API)
    │   ├── BuildingBlocks/GenclikMerkezi.SharedKernel     (classlib)
    │   ├── BuildingBlocks/GenclikMerkezi.BuildingBlocks.Infrastructure (classlib)
    │   ├── BuildingBlocks/GenclikMerkezi.Contracts        (classlib)
    │   └── Modules/<ModuleName>/GenclikMerkezi.Modules.<ModuleName> (classlib)
    │       her modulun icinde: Domain/ Features/ Infrastructure/ Contracts/
    └── tests/
        ├── UnitTests/GenclikMerkezi.UnitTests
        ├── IntegrationTests/GenclikMerkezi.IntegrationTests
        └── ArchitectureTests/GenclikMerkezi.ArchitectureTests

    Script idempotent degildir; ayni klasor uzerinde tekrar calistirmadan once
    ADGM klasorunu silin veya farkli bir -RootPath verin.

.PARAMETER RootPath
    Solution'in olusturulacagi kok klasor. Varsayilan: ".\ADGM"

.PARAMETER SolutionName
    Solution ve proje isim on eki. Varsayilan: "GenclikMerkezi"

.EXAMPLE
    .\New-GenclikMerkeziSolution.ps1
    .\New-GenclikMerkeziSolution.ps1 -RootPath "C:\Dev\ADGM"
#>

[CmdletBinding()]
param(
    [string]$RootPath = ".\ADGM",
    [string]$SolutionName = "GenclikMerkezi"
)

$ErrorActionPreference = "Stop"

# ---------------------------------------------------------------------------
# 0. On kontrol
# ---------------------------------------------------------------------------
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw ".NET SDK bulunamadi. Lutfen once .NET SDK kurun (dotnet --version ile kontrol edin)."
}

Write-Host "== Genclik Merkezi Solution Olusturucu ==" -ForegroundColor Cyan
Write-Host "Root Path : $RootPath"
Write-Host "Solution  : $SolutionName"
Write-Host ""

# ---------------------------------------------------------------------------
# 1. Modul listesi (ARCHITECTURE.md #8 - Module List)
# ---------------------------------------------------------------------------
$modules = @(
    "Identity",
    "Candidate",
    "Employer",
    "CareerAdvisor",
    "Job",
    "Matching",
    "Interview",
    "Employment",
    "Website",
    "Notification",
    "ReferenceData",
    "Support"
)

# Her modulun icinde acilacak alt klasorler (ARCHITECTURE.md #7 - Module Architecture)
$moduleSubFolders = @("Domain", "Features", "Infrastructure", "Contracts")

# ---------------------------------------------------------------------------
# 2. Kok klasor ve solution
# ---------------------------------------------------------------------------
New-Item -ItemType Directory -Force -Path $RootPath | Out-Null
Push-Location $RootPath

Write-Host "[1/6] Solution dosyasi olusturuluyor..." -ForegroundColor Yellow
dotnet new sln -n $SolutionName | Out-Null

# ---------------------------------------------------------------------------
# 3. Host (API) projesi
# ---------------------------------------------------------------------------
Write-Host "[2/6] Host (API) projesi olusturuluyor..." -ForegroundColor Yellow

$hostProjectDir = "src/Host/$SolutionName.Api"
dotnet new webapi -n "$SolutionName.Api" -o $hostProjectDir --use-controllers | Out-Null
dotnet sln add "$hostProjectDir/$SolutionName.Api.csproj" | Out-Null

# ---------------------------------------------------------------------------
# 4. BuildingBlocks projeleri
# ---------------------------------------------------------------------------
Write-Host "[3/6] BuildingBlocks projeleri olusturuluyor..." -ForegroundColor Yellow

$buildingBlocks = @(
    @{ Name = "$SolutionName.SharedKernel";               Folder = "src/BuildingBlocks/SharedKernel" },
    @{ Name = "$SolutionName.BuildingBlocks.Infrastructure"; Folder = "src/BuildingBlocks/Infrastructure" },
    @{ Name = "$SolutionName.Contracts";                  Folder = "src/BuildingBlocks/Contracts" }
)

foreach ($bb in $buildingBlocks) {
    dotnet new classlib -n $bb.Name -o $bb.Folder | Out-Null
    # Ornek dosya olarak gelen Class1.cs'i temizleyelim
    $class1 = Join-Path $bb.Folder "Class1.cs"
    if (Test-Path $class1) { Remove-Item $class1 }
    dotnet sln add "$($bb.Folder)/$($bb.Name).csproj" | Out-Null
}

# SharedKernel altinda dokumandaki (ARCHITECTURE.md #9) klasorleri hazirla
$sharedKernelFolders = @("Domain", "Results", "Abstractions")
foreach ($f in $sharedKernelFolders) {
    New-Item -ItemType Directory -Force -Path "src/BuildingBlocks/SharedKernel/$f" | Out-Null
    New-Item -ItemType File -Force -Path "src/BuildingBlocks/SharedKernel/$f/.gitkeep" | Out-Null
}

$sharedKernelName = "$SolutionName.SharedKernel"
$sharedKernelPath = "src/BuildingBlocks/SharedKernel/$sharedKernelName.csproj"

# ---------------------------------------------------------------------------
# 5. Modul projeleri
# ---------------------------------------------------------------------------
Write-Host "[4/6] Modul projeleri olusturuluyor ($($modules.Count) modul)..." -ForegroundColor Yellow

foreach ($module in $modules) {
    $moduleDir = "src/Modules/$module"
    $projectName = "$SolutionName.Modules.$module"

    Write-Host "  -> $module" -ForegroundColor DarkGray

    dotnet new classlib -n $projectName -o $moduleDir | Out-Null

    $class1 = Join-Path $moduleDir "Class1.cs"
    if (Test-Path $class1) { Remove-Item $class1 }

    # Modul, kendi SharedKernel'ine referans verir (SharedKernel'in kendisi disinda
    # baska hicbir modulun projesine referans VERMEZ - ARCHITECTURE.md #12/#13)
    Push-Location $moduleDir
    dotnet add reference "..\..\BuildingBlocks\SharedKernel\$sharedKernelName.csproj" | Out-Null
    Pop-Location

    dotnet sln add "$moduleDir/$projectName.csproj" | Out-Null

    # Modul ic klasor yapisi (Domain / Features / Infrastructure / Contracts)
    foreach ($sub in $moduleSubFolders) {
        $subPath = "$moduleDir/$sub"
        New-Item -ItemType Directory -Force -Path $subPath | Out-Null
        New-Item -ItemType File -Force -Path "$subPath/.gitkeep" | Out-Null
    }
}

# ---------------------------------------------------------------------------
# 6. Host projesinin tum modullere referans vermesi
#    (Composition Root - ARCHITECTURE.md #7: API/Host = composition root)
# ---------------------------------------------------------------------------
Write-Host "[5/6] Host projesine modul referanslari ekleniyor..." -ForegroundColor Yellow

Push-Location $hostProjectDir
foreach ($module in $modules) {
    $projectName = "$SolutionName.Modules.$module"
    dotnet add reference "..\..\Modules\$module\$projectName.csproj" | Out-Null
}
dotnet add reference "..\..\BuildingBlocks\SharedKernel\$sharedKernelName.csproj" | Out-Null
Pop-Location

# ---------------------------------------------------------------------------
# 7. Test projeleri (ARCHITECTURE.md #6 - tests/)
# ---------------------------------------------------------------------------
Write-Host "[6/6] Test projeleri olusturuluyor..." -ForegroundColor Yellow

$testProjects = @(
    @{ Name = "$SolutionName.UnitTests";        Folder = "tests/UnitTests" },
    @{ Name = "$SolutionName.IntegrationTests";  Folder = "tests/IntegrationTests" },
    @{ Name = "$SolutionName.ArchitectureTests"; Folder = "tests/ArchitectureTests" }
)

foreach ($tp in $testProjects) {
    dotnet new xunit -n $tp.Name -o $tp.Folder | Out-Null

    $unitTest1 = Join-Path $tp.Folder "UnitTest1.cs"
    if (Test-Path $unitTest1) { Remove-Item $unitTest1 }

    dotnet sln add "$($tp.Folder)/$($tp.Name).csproj" | Out-Null
}

# ArchitectureTests, kural gereği TUM modullere ve host'a referans verir
# (mimari sinirlarin test edilebilmesi icin - ARCHITECTURE.md #47)
Push-Location "tests/ArchitectureTests"
dotnet add reference "..\..\src\Host\$SolutionName.Api\$SolutionName.Api.csproj" | Out-Null
foreach ($module in $modules) {
    $projectName = "$SolutionName.Modules.$module"
    dotnet add reference "..\..\src\Modules\$module\$projectName.csproj" | Out-Null
}
Pop-Location

# ---------------------------------------------------------------------------
# 8. .gitignore ve README iskeleti
# ---------------------------------------------------------------------------
$gitignoreContent = @"
## .NET
bin/
obj/
*.user

## IDE
.vs/
.vscode/
.idea/

## OS
.DS_Store
Thumbs.db
"@
Set-Content -Path ".gitignore" -Value $gitignoreContent -Encoding UTF8

New-Item -ItemType Directory -Force -Path "docs" | Out-Null
New-Item -ItemType Directory -Force -Path "docs/adr" | Out-Null

Write-Host ""
Write-Host "Tamamlandi. Olusturulan yapi:" -ForegroundColor Green
Pop-Location

# ---------------------------------------------------------------------------
# 9. Sonuc agacini goster
# ---------------------------------------------------------------------------
Write-Host ""
Write-Host "Sonraki adimlar:" -ForegroundColor Cyan
Write-Host "  1) cd $RootPath"
Write-Host "  2) AGENTS.md, PROJECT.md, ARCHITECTURE.md, DOMAIN.md, DEVELOPMENT.md, SECURITY.md, PERFORMANCE.md dosyalarini kok klasore kopyalayin"
Write-Host "  3) docs/adr altina ilk ADR'lerinizi ekleyin (orn. ADR-001-Modular-Monolith.md)"
Write-Host "  4) dotnet build ile solution'in derlendigini dogrulayin"
Write-Host "  5) .github/workflows/ci.yml ekleyerek GitHub Actions pipeline'ini kurun"
