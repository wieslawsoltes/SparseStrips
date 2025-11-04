#!/usr/bin/env bash
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
CONFIGS=("Debug" "Release")

echo "========================================"
echo "SparseStrips Linux native build"
echo "========================================"
echo ""

cd "$REPO_ROOT"

echo "Building Vello CPU FFI (Rust)..."
pushd vello_cpu_ffi > /dev/null
for config in "${CONFIGS[@]}"; do
    if [[ "$config" == "Release" ]]; then
        echo "  -> cargo build --release"
        cargo build --release
    else
        echo "  -> cargo build"
        cargo build
    fi
done
popd > /dev/null
echo "✓ Native builds completed."
echo ""

echo "Artifacts:"
for config in "${CONFIGS[@]}"; do
    profile=$(echo "$config" | tr '[:upper:]' '[:lower:]')
    native_lib="vello_cpu_ffi/target/${profile}/libvello_cpu_ffi.so"
    if [[ -f "$native_lib" ]]; then
        echo "  ${config}: $REPO_ROOT/$native_lib"
    else
        echo "  ${config}: (not found – ensure cargo produced libvello_cpu_ffi.so)"
    fi
done
echo ""
echo "Linux native build finished successfully."
