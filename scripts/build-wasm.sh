#!/usr/bin/env bash
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
WASM_TARGET="wasm32-unknown-emscripten"
CONFIGS=("Debug" "Release")

usage() {
    cat <<EOF
Usage: $(basename "$0") [--threads|--single|--both]

Options:
  --threads        Build with wasm threads enabled (nightly toolchain, default)
  --single         Build a single-threaded variant (stable toolchain, no atomics)
  --both           Build both single-threaded and threaded variants sequentially
  -h, --help       Show this help message
EOF
}

VARIANT="threads"
while [[ $# -gt 0 ]]; do
    case "$1" in
        --threads)
            VARIANT="threads"
            ;;
        --single|--no-threads)
            VARIANT="single"
            ;;
        --both)
            VARIANT="both"
            ;;
        -h|--help)
            usage
            exit 0
            ;;
        *)
            echo "Unknown option: $1" >&2
            usage
            exit 1
            ;;
    esac
    shift
done

echo "========================================"
echo "SparseStrips WebAssembly native build"
echo "========================================"
echo ""

cd "$REPO_ROOT"

ensure_toolchain() {
    local toolchain="$1"
    if ! rustup toolchain list | grep -q "^${toolchain}"; then
        echo "Installing ${toolchain} toolchain..."
        rustup toolchain install "${toolchain}"
        echo ""
    fi
}

ensure_target() {
    local toolchain="$1"
    echo "Ensuring Rust target ${WASM_TARGET} (${toolchain}) is installed..."
    if ! rustup target list --toolchain "${toolchain}" | grep -q "^${WASM_TARGET} (installed)"; then
        rustup target add "${WASM_TARGET}" --toolchain "${toolchain}"
    fi
    echo "✓ Target available."
    echo ""
}

ensure_rust_src() {
    local toolchain="$1"
    if ! rustup component list --toolchain "${toolchain}" | grep -q "^rust-src.*(installed)"; then
        echo "Installing rust-src component for ${toolchain}..."
        rustup component add rust-src --toolchain "${toolchain}"
        echo ""
    fi
}

print_artifacts() {
    local label="$1"
    echo "Artifacts (${label}):"
    for config in "${CONFIGS[@]}"; do
        local profile archive
        profile=$(echo "$config" | tr '[:upper:]' '[:lower:]')
        archive="vello_cpu_ffi/target/${WASM_TARGET}/${profile}/libvello_cpu_ffi.a"
        if [[ -f "$archive" ]]; then
            echo "  ${config}: $REPO_ROOT/$archive"
        else
            echo "  ${config}: (not found – ensure cargo produced libvello_cpu_ffi.a)"
        fi
    done
    echo ""
}

build_threaded() {
    echo ">>> Building threaded wasm variant (pthreads enabled)"
    ensure_toolchain "nightly"
    ensure_target "nightly"
    ensure_rust_src "nightly"

    local extra_flags="-Z unstable-options -C panic=abort -C target-feature=+atomics,+bulk-memory,+mutable-globals -C link-arg=-sSTANDALONE_WASM=0 -C link-arg=--no-entry -C link-arg=-pthread -C link-arg=-sUSE_PTHREADS=1 -C link-arg=-g"
    local effective_flags
    if [[ -n "${RUSTFLAGS:-}" ]]; then
        effective_flags="${RUSTFLAGS} ${extra_flags}"
    else
        effective_flags="${extra_flags}"
    fi

    echo "Using toolchain: nightly"
    echo "Using RUSTFLAGS='${effective_flags}'"
    echo ""

    pushd vello_cpu_ffi > /dev/null
    for config in "${CONFIGS[@]}"; do
        if [[ "$config" == "Release" ]]; then
            echo "  -> cargo +nightly rustc --release --target ${WASM_TARGET} -Z build-std=std,panic_abort -- --crate-type staticlib"
            RUSTFLAGS="${effective_flags}" cargo +nightly rustc --release --target "${WASM_TARGET}" -Z build-std=std,panic_abort -- --crate-type staticlib
        else
            echo "  -> cargo +nightly rustc --target ${WASM_TARGET} -Z build-std=std,panic_abort -- --crate-type staticlib"
            RUSTFLAGS="${effective_flags}" cargo +nightly rustc --target "${WASM_TARGET}" -Z build-std=std,panic_abort -- --crate-type staticlib
        fi
    done
    popd > /dev/null

    echo "✓ Threaded wasm build completed."
    echo ""
    print_artifacts "threads enabled"
}

build_single() {
    echo ">>> Building single-threaded wasm variant (stable toolchain)"
    ensure_toolchain "stable"
    ensure_target "stable"

    local extra_flags="-C link-arg=-sSTANDALONE_WASM=0 -C link-arg=--no-entry -C link-arg=-g"
    local effective_flags
    if [[ -n "${RUSTFLAGS:-}" ]]; then
        effective_flags="${RUSTFLAGS} ${extra_flags}"
    else
        effective_flags="${extra_flags}"
    fi

    echo "Using toolchain: stable"
    echo "Using RUSTFLAGS='${effective_flags}'"
    echo ""

    pushd vello_cpu_ffi > /dev/null
    for config in "${CONFIGS[@]}"; do
        if [[ "$config" == "Release" ]]; then
            echo "  -> cargo +stable rustc --release --target ${WASM_TARGET} -- --crate-type staticlib"
            CARGO_PROFILE_RELEASE_PANIC=unwind CARGO_PROFILE_DEV_PANIC=unwind RUSTFLAGS="${effective_flags}" \
                cargo +stable rustc --release --target "${WASM_TARGET}" -- --crate-type staticlib
        else
            echo "  -> cargo +stable rustc --target ${WASM_TARGET} -- --crate-type staticlib"
            CARGO_PROFILE_RELEASE_PANIC=unwind CARGO_PROFILE_DEV_PANIC=unwind RUSTFLAGS="${effective_flags}" \
                cargo +stable rustc --target "${WASM_TARGET}" -- --crate-type staticlib
        fi
    done
    popd > /dev/null

    echo "✓ Single-threaded wasm build completed."
    echo ""
    print_artifacts "single-threaded"
}

case "$VARIANT" in
    threads)
        build_threaded
        ;;
    single)
        build_single
        ;;
    both)
        build_single
        build_threaded
        ;;
esac

echo "WebAssembly native build finished successfully."
