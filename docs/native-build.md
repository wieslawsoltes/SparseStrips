# Native Build Guide

This document describes how to compile the native parts of SparseStrips on each supported platform. The build consists of two layers:

1. The Rust C-ABI crate `vello_cpu_ffi`
2. The managed `.NET` projects that P/Invoke into the library

> Tip: All steps assume you cloned the repository and are working from its root directory.

---

## Common Prerequisites

- **Rust toolchain** (1.76 or newer) with `cargo`
- **.NET SDK 9.0** (installs `dotnet`)
- Internet access to download dependencies

Update Rust and cargo before running the builds:

```bash
rustup update
```

---

## Windows

### Required Tools

- Visual Studio 2022 or the Build Tools workload (for MSVC linker)
- PowerShell 7+ (recommended)

### Build Steps

```powershell
# From the repo root
.\scripts\build-windows.ps1
```

The script builds `vello_cpu_ffi` in both `Debug` and `Release` modes and leaves the DLLs under `vello_cpu_ffi/target/<profile>/`. Run `dotnet build` separately when you need the managed assemblies.

---

## Linux

### Required Tools

- GCC or Clang toolchain (install via your package manager)
- pkg-config
- Make sure `libstdc++` and `libgcc` are present for linking

### Build Steps

```bash
# From the repo root
./scripts/build-linux.sh
```

The script compiles the Rust FFI in `Debug` and `Release`. Use `dotnet build` afterwards to produce managed binaries if required.

---

## macOS

### Required Tools

- Xcode Command Line Tools (`xcode-select --install`)
- Homebrew (optional) for additional packages

### Build Steps

```bash
# From the repo root
./scripts/build-macos.sh
```

The script exports a sensible `MACOSX_DEPLOYMENT_TARGET` and builds `vello_cpu_ffi` in both configurations. Invoke `dotnet build` separately when needed.

---

## WebAssembly

### Required Tools

1. **Rust toolchains & wasm target**

   - Single-threaded builds (`./scripts/build-wasm.sh --single`) only require the stable toolchain:

     ```bash
     rustup toolchain install stable   # no-op if already installed
     rustup target add wasm32-unknown-emscripten --toolchain stable
     ```

   - Threaded builds (`./scripts/build-wasm.sh`, `--threads`, or `--both`) require nightly and `rust-src`:

     ```bash
     rustup toolchain install nightly
     rustup target add wasm32-unknown-emscripten --toolchain nightly
     rustup component add rust-src --toolchain nightly
     ```

2. **Emscripten SDK** — install and activate via the [official instructions](https://emscripten.org/docs/getting_started/downloads.html), or install via Homebrew on macOS:

   ```bash
   brew install emscripten
   ```

   Add the Emscripten `bin` directory to your shell profile (e.g., `~/.zshrc`):

   ```bash
   export PATH="/opt/homebrew/opt/emscripten/bin:$PATH"
   ```

   Verify with `emcc --version`. If you are using the official SDK (`emsdk`), run `source ./emsdk_env.sh` before building so `emcc` is on the PATH.

3. **.NET wasm workload**

   ```bash
   dotnet workload install wasm-tools
   ```

4. **Disable standalone wasm main check** (Emscripten ≥ 4.0 defaults to standalone mode and expects a `main` symbol). The provided build script sets `-sSTANDALONE_WASM=0` automatically. If you invoke `cargo` manually, add the flag yourself (see below).

### Build Steps

```bash
# From the repo root
./scripts/build-wasm.sh            # threaded build (default)
./scripts/build-wasm.sh --single   # single-threaded build on stable
./scripts/build-wasm.sh --both     # build both variants sequentially
```

Each run produces `libvello_cpu_ffi.a` archives for `Debug` and `Release` ready for static linking. The threaded variant passes `--crate-type staticlib`, enables atomics (`-C target-feature=+atomics,+bulk-memory,+mutable-globals`), and adds Emscripten link arguments `-sSTANDALONE_WASM=0`, `--no-entry`, `-sUSE_PTHREADS=1`, and `-pthread`. The single-threaded variant keeps the same static linking setup but omits atomics and pthread flags, uses the stable toolchain end-to-end, and builds with the default `panic=unwind` strategy (required because the standard library is not rebuilt on stable).

> **Important:** P/Invoke on WebAssembly requires the native library to be linked statically. Ensure `libvello_cpu_ffi.a` exists before running `dotnet publish`.

### Manual Command

If you prefer to run the build by hand, mirror the script.

#### Threaded build (nightly)

```bash
cd vello_cpu_ffi
RUSTFLAGS="-Z unstable-options -C panic=abort -C target-feature=+atomics,+bulk-memory,+mutable-globals -C link-arg=-sSTANDALONE_WASM=0 -C link-arg=--no-entry -C link-arg=-pthread -C link-arg=-sUSE_PTHREADS=1" \
  cargo +nightly rustc --release --target wasm32-unknown-emscripten \
  -Z build-std=std,panic_abort \
  -- --crate-type staticlib
```

#### Single-threaded build (stable)

```bash
cd vello_cpu_ffi
CARGO_PROFILE_DEV_PANIC=unwind \
CARGO_PROFILE_RELEASE_PANIC=unwind \
RUSTFLAGS="-C link-arg=-sSTANDALONE_WASM=0 -C link-arg=--no-entry" \
  cargo +stable rustc --release --target wasm32-unknown-emscripten \
  -- --crate-type staticlib
```

## Verifying Outputs

After any build, inspect the following directories:

- `vello_cpu_ffi/target/<profile>/` — contains the platform-specific native artifacts
- `dotnet/src/Vello.Native/bin/<Configuration>/net9.0/runtimes/` — contains platform-specific native libraries copied during the build
- `dotnet/samples/Vello.Samples/*/bin/<Configuration>/` — contains the managed binaries and (for WASM) the `AppBundle`

These locations confirm both native and managed layers built successfully. Refer back to the `README.md` for instructions on running the sample applications.
