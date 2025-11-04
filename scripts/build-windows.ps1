$ErrorActionPreference = "Stop"

$Configurations = @("Debug", "Release")
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
Set-Location $repoRoot

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "SparseStrips Windows native build" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

try {
    Write-Host "Building Vello CPU FFI (Rust)..." -ForegroundColor Green
    Push-Location vello_cpu_ffi
    foreach ($config in $Configurations) {
        $cargoArgs = @("build")
        if ($config -eq "Release") {
            $cargoArgs += "--release"
        }
        Write-Host ("  -> cargo {0}" -f ($cargoArgs -join " ")) -ForegroundColor DarkCyan
        cargo @cargoArgs
    }
    Pop-Location
    Write-Host "✓ Native builds completed." -ForegroundColor Green
    Write-Host ""

    Write-Host "Artifacts:" -ForegroundColor Green
    foreach ($config in $Configurations) {
        $nativeLib = Join-Path vello_cpu_ffi "target\$($config.ToLower())\vello_cpu_ffi.dll"
        if (Test-Path $nativeLib) {
            Write-Host ("  {0}: {1}" -f $config, (Resolve-Path $nativeLib)) -ForegroundColor White
        } else {
            Write-Host ("  {0}: (not found – ensure cargo produced the DLL)" -f $config) -ForegroundColor Yellow
        }
    }
    Write-Host ""
    Write-Host "Windows native build finished successfully." -ForegroundColor Cyan
}
finally {
    Set-Location $repoRoot
}
