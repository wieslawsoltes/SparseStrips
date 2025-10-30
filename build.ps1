# PowerShell build script for Windows
$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Building Vello CPU FFI and .NET Bindings" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$PLATFORM = "Windows"
$LIB_EXT = "dll"
$LIB_PREFIX = ""

Write-Host "Platform: $PLATFORM" -ForegroundColor White
Write-Host ""

# Build Rust FFI library
Write-Host "Step 1/3: Building Rust FFI library..." -ForegroundColor Green
Push-Location vello_cpu_ffi
cargo build --release

if (-not (Test-Path "target/release/${LIB_PREFIX}vello_cpu_ffi.${LIB_EXT}")) {
    Write-Host "Error: Native library was not built successfully" -ForegroundColor Red
    Pop-Location
    exit 1
}

Write-Host "✓ Native library built: target/release/${LIB_PREFIX}vello_cpu_ffi.${LIB_EXT}" -ForegroundColor Green
Pop-Location
Write-Host ""

# Build .NET projects
Write-Host "Step 2/3: Building .NET projects..." -ForegroundColor Green
Push-Location dotnet
dotnet build -c Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: .NET build failed" -ForegroundColor Red
    Pop-Location
    exit 1
}

Write-Host "✓ .NET projects built successfully" -ForegroundColor Green
Pop-Location
Write-Host ""

# Verify native library was copied
Write-Host "Step 3/3: Verifying native library deployment..." -ForegroundColor Green

$ARCH = [System.Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture
if ($ARCH -eq "X64") {
    $RID = "win-x64"
} elseif ($ARCH -eq "Arm64") {
    $RID = "win-arm64"
} else {
    $RID = "win-x86"
}

$NATIVE_LIB_PATH = "dotnet/Vello.Native/bin/Release/net8.0/runtimes/${RID}/native/${LIB_PREFIX}vello_cpu_ffi.${LIB_EXT}"

if (Test-Path $NATIVE_LIB_PATH) {
    Write-Host "✓ Native library deployed to: $NATIVE_LIB_PATH" -ForegroundColor Green
} else {
    Write-Host "⚠ Warning: Native library not found at expected location: $NATIVE_LIB_PATH" -ForegroundColor Yellow
    Write-Host "  Build will still work if library is in search path" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Build Complete!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Native library: vello_cpu_ffi/target/release/${LIB_PREFIX}vello_cpu_ffi.${LIB_EXT}"
Write-Host ".NET binaries:  dotnet/Vello/bin/Release/net8.0/"
Write-Host ""
Write-Host "To run samples:"
Write-Host "  cd dotnet\Vello.Samples"
Write-Host "  dotnet run -c Release"
