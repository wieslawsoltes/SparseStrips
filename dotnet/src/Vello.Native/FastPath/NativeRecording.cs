// Copyright 2025
// SPDX-License-Identifier: Apache-2.0 OR MIT

using System;

namespace Vello.Native.FastPath;

/// <summary>
/// Managed wrapper for recording reusable render commands.
/// </summary>
public ref struct NativeRecording : IDisposable
{
    private nint _handle;

    /// <summary>
    /// Initializes an empty recording.
    /// </summary>
    public NativeRecording()
    {
        _handle = NativeResult.EnsureHandle(
            NativeMethods.Recording_New(),
            nameof(NativeMethods.Recording_New));
    }

    /// <summary>
    /// Gets the native recording handle.
    /// </summary>
    public nint Handle
    {
        get
        {
            EnsureNotDisposed();
            return _handle;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the recording owns a native handle.
    /// </summary>
    public bool IsAllocated => _handle != nint.Zero;

    /// <summary>
    /// Gets the number of recorded commands.
    /// </summary>
    public nuint Length
    {
        get
        {
            EnsureNotDisposed();
            return NativeMethods.Recording_Len(_handle);
        }
    }

    /// <summary>
    /// Gets a value indicating whether the recording owns cached strips.
    /// </summary>
    public bool HasCachedStrips
    {
        get
        {
            EnsureNotDisposed();
            return NativeMethods.Recording_HasCachedStrips(_handle) != 0;
        }
    }

    /// <summary>
    /// Gets the number of cached strips stored on the native recording.
    /// </summary>
    public nuint StripCount
    {
        get
        {
            EnsureNotDisposed();
            return NativeMethods.Recording_StripCount(_handle);
        }
    }

    /// <summary>
    /// Gets the number of cached alpha bytes stored on the native recording.
    /// </summary>
    public nuint AlphaByteCount
    {
        get
        {
            EnsureNotDisposed();
            return NativeMethods.Recording_AlphaCount(_handle);
        }
    }

    /// <summary>
    /// Clears the recorded commands.
    /// </summary>
    public void Clear()
    {
        EnsureNotDisposed();
        NativeResult.ThrowIfFailed(
            NativeMethods.Recording_Clear(_handle),
            nameof(NativeMethods.Recording_Clear));
    }

    /// <summary>
    /// Releases the native recording handle.
    /// </summary>
    public void Dispose()
    {
        if (_handle == nint.Zero)
        {
            return;
        }

        NativeMethods.Recording_Free(_handle);
        _handle = nint.Zero;
    }

    private void EnsureNotDisposed()
    {
        if (_handle == nint.Zero)
        {
            throw new ObjectDisposedException(nameof(NativeRecording));
        }
    }
}
