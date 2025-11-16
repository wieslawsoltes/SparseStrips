#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
NUGET_DIR="$ROOT_DIR/dotnet/NuGet"
TEST_PROJECT="$ROOT_DIR/dotnet/tests/Vello.IntegrationTest/Vello.IntegrationTest.csproj"

echo "Packing local Vello.Native..."
dotnet pack "$ROOT_DIR/dotnet/src/Vello.Native/Vello.Native.csproj" \
  --configuration Release \
  --output "$NUGET_DIR"

echo "Packing local Vello..."
dotnet pack "$ROOT_DIR/dotnet/src/Vello/Vello.csproj" \
  --configuration Release \
  --output "$NUGET_DIR"

echo "Packing local Vello.Avalonia..."
dotnet pack "$ROOT_DIR/dotnet/src/Vello.Avalonia/Vello.Avalonia.csproj" \
  --configuration Release \
  --output "$NUGET_DIR"

echo "Restoring integration test project..."
dotnet restore "$TEST_PROJECT" \
  --source "$NUGET_DIR"

echo "Running integration test..."
dotnet run --project "$TEST_PROJECT" --configuration Release
