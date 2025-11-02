// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

using System.Reflection;
using System.Runtime.InteropServices;

namespace Vello.Native;

/// <summary>
/// Handles loading of the native vello_cpu_ffi library with cross-platform support.
/// </summary>
public static class NativeLibraryLoader
{
    private static nint _libraryHandle;
    private static readonly object _lock = new();
    private static bool _resolverRegistered;

    /// <summary>
    /// Ensures the native library is loaded and resolver is registered.
    /// </summary>
    public static nint EnsureLoaded()
    {
        lock (_lock)
        {
            if (!_resolverRegistered)
            {
                // Register DllImportResolver for this assembly
                NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), DllImportResolver);
                _resolverRegistered = true;
            }

            if (_libraryHandle == 0)
            {
                _libraryHandle = LoadNativeLibrary();
                if (_libraryHandle == 0)
                {
                    throw new DllNotFoundException("Failed to load native library 'vello_cpu_ffi'.");
                }
            }

            return _libraryHandle;
        }
    }

    /// <summary>
    /// Custom DllImportResolver for LibraryImport attributes.
    /// </summary>
    private static nint DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        // Only handle vello_cpu_ffi library
        if (libraryName != "vello_cpu_ffi")
            return IntPtr.Zero;

        return EnsureLoaded();
    }

    /// <summary>
    /// Retrieves a function pointer for the specified export.
    /// </summary>
    /// <param name="entryPoint">Name of the native symbol.</param>
    /// <returns>Pointer to the native function.</returns>
    /// <exception cref="ArgumentException">Thrown when the entry point name is null or empty.</exception>
    /// <exception cref="EntryPointNotFoundException">Thrown when the symbol cannot be resolved.</exception>
    public static nint GetExport(string entryPoint)
    {
        if (string.IsNullOrEmpty(entryPoint))
        {
            throw new ArgumentException("Entry point name must be provided.", nameof(entryPoint));
        }

        nint handle = EnsureLoaded();
        if (NativeLibrary.TryGetExport(handle, entryPoint, out nint address))
        {
            return address;
        }

        throw new EntryPointNotFoundException($"Unable to resolve native symbol '{entryPoint}'.");
    }
    
    private static nint LoadNativeLibrary()
    {
        string libraryName = GetLibraryName();
        nint handle;

        // Try multiple search strategies

        // Strategy 1: Try runtime-specific paths first (where csproj copies libraries)
        string[] searchPaths = GetSearchPaths(libraryName);

        foreach (var path in searchPaths)
        {
            if (File.Exists(path))
            {
                if (NativeLibrary.TryLoad(path, out handle))
                    return handle;
            }
        }

        // Strategy 2: Let NativeLibrary.TryLoad use default search paths
        if (NativeLibrary.TryLoad(libraryName, Assembly.GetExecutingAssembly(), null, out handle))
            return handle;

        // Strategy 3: Try loading from assembly directory
        string? assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (assemblyDir != null)
        {
            string assemblyPath = Path.Combine(assemblyDir, libraryName);
            if (File.Exists(assemblyPath))
            {
                if (NativeLibrary.TryLoad(assemblyPath, out handle))
                    return handle;
            }
        }

        return 0;
    }
    
    private static string GetLibraryName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return "vello_cpu_ffi.dll";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return "libvello_cpu_ffi.dylib";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return "libvello_cpu_ffi.so";
        else
            throw new PlatformNotSupportedException($"Unsupported platform: {RuntimeInformation.OSDescription}");
    }
    
    private static string[] GetSearchPaths(string libraryName)
    {
        string runtimeIdentifier = GetRuntimeIdentifier();
        string? assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        
        if (assemblyDir == null)
            return Array.Empty<string>();
        
        List<string> paths = new();
        
        // Standard .NET runtime paths
        paths.Add(Path.Combine(assemblyDir, "runtimes", runtimeIdentifier, "native", libraryName));
        paths.Add(Path.Combine(assemblyDir, "runtimes", GetGenericRuntimeIdentifier(), "native", libraryName));
        
        // Legacy paths
        paths.Add(Path.Combine(assemblyDir, runtimeIdentifier, libraryName));
        paths.Add(Path.Combine(assemblyDir, "native", libraryName));
        
        // Development paths (relative to assembly)
        string? projectRoot = FindProjectRoot(assemblyDir);
        if (projectRoot != null)
        {
            paths.Add(Path.Combine(projectRoot, "vello_cpu_ffi", "target", "release", libraryName));
            paths.Add(Path.Combine(projectRoot, "vello_cpu_ffi", "target", "debug", libraryName));
        }
        
        return paths.ToArray();
    }
    
    private static string GetRuntimeIdentifier()
    {
        string architecture = RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant();
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return $"win-{architecture}";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return $"osx-{architecture}";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return $"linux-{architecture}";
        else
            throw new PlatformNotSupportedException();
    }
    
    private static string GetGenericRuntimeIdentifier()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return "win";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return "osx";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return "linux";
        else
            throw new PlatformNotSupportedException();
    }
    
    private static string? FindProjectRoot(string startPath)
    {
        DirectoryInfo? dir = new DirectoryInfo(startPath);
        
        while (dir != null)
        {
            // Look for the vello_cpu_ffi directory
            string velloFfiPath = Path.Combine(dir.FullName, "vello_cpu_ffi");
            if (Directory.Exists(velloFfiPath))
                return dir.FullName;
            
            // Look for .git directory (repository root)
            if (Directory.Exists(Path.Combine(dir.FullName, ".git")))
                return dir.FullName;
            
            dir = dir.Parent;
        }
        
        return null;
    }
}
