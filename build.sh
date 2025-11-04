#!/bin/bash
set -e

echo "========================================"
echo "Building Vello CPU FFI and .NET Bindings"
echo "========================================"
echo ""

# Determine platform
if [[ "$OSTYPE" == "darwin"* ]]; then
    PLATFORM="macOS"
    LIB_EXT="dylib"
    LIB_PREFIX="lib"
elif [[ "$OSTYPE" == "linux-gnu"* ]]; then
    PLATFORM="Linux"
    LIB_EXT="so"
    LIB_PREFIX="lib"
else
    echo "Unsupported platform: $OSTYPE"
    exit 1
fi

echo "Platform: $PLATFORM"
echo ""

# Build Rust FFI library
echo "Step 1/3: Building Rust FFI library..."
cd vello_cpu_ffi
cargo build --release

if [ ! -f "target/release/${LIB_PREFIX}vello_cpu_ffi.${LIB_EXT}" ]; then
    echo "Error: Native library was not built successfully"
    exit 1
fi

echo "✓ Native library built: target/release/${LIB_PREFIX}vello_cpu_ffi.${LIB_EXT}"
cd ..
echo ""

# Build .NET projects
echo "Step 2/3: Building .NET projects..."
cd dotnet
dotnet build -c Release

if [ $? -ne 0 ]; then
    echo "Error: .NET build failed"
    exit 1
fi

echo "✓ .NET projects built successfully"
cd ..
echo ""

# Verify native library was copied
echo "Step 3/3: Verifying native library deployment..."

if [[ "$PLATFORM" == "macOS" ]]; then
    ARCH=$(uname -m)
    if [[ "$ARCH" == "arm64" ]]; then
        RID="osx-arm64"
    else
        RID="osx-x64"
    fi
elif [[ "$PLATFORM" == "Linux" ]]; then
    ARCH=$(uname -m)
    if [[ "$ARCH" == "aarch64" ]]; then
        RID="linux-arm64"
    else
        RID="linux-x64"
    fi
fi

NATIVE_LIB_PATH="dotnet/src/Vello.Native/bin/Release/net8.0/runtimes/${RID}/native/${LIB_PREFIX}vello_cpu_ffi.${LIB_EXT}"

if [ -f "$NATIVE_LIB_PATH" ]; then
    echo "✓ Native library deployed to: $NATIVE_LIB_PATH"
else
    echo "⚠ Warning: Native library not found at expected location: $NATIVE_LIB_PATH"
    echo "  Build will still work if library is in search path"
fi

echo ""
echo "========================================"
echo "Build Complete!"
echo "========================================"
echo ""
echo "Native library: vello_cpu_ffi/target/release/${LIB_PREFIX}vello_cpu_ffi.${LIB_EXT}"
echo ".NET binaries:  dotnet/src/Vello/bin/Release/net8.0/"
echo ""
echo "To run samples:"
echo "  cd dotnet/samples/Vello.Samples"
echo "  dotnet run -c Release"
