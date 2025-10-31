// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

using Vello.Native;

namespace Vello;

/// <summary>
/// Represents a recorded sequence of drawing operations that can be replayed efficiently.
/// </summary>
/// <remarks>
/// Recording allows you to pre-record complex drawing operations and replay them multiple times
/// without re-processing paths and paint setup.
/// </remarks>
public sealed class Recording : IDisposable
{
    private nint _handle;
    private bool _disposed;

    /// <summary>
    /// Creates a new empty recording.
    /// </summary>
    public Recording()
    {
        _handle = NativeMethods.Recording_New();
        if (_handle == nint.Zero)
        {
            throw new InvalidOperationException("Failed to create recording");
        }
    }

    /// <summary>
    /// Gets the number of recorded commands.
    /// </summary>
    public int Count
    {
        get
        {
            ThrowIfDisposed();
            return (int)NativeMethods.Recording_Len(_handle);
        }
    }

    /// <summary>
    /// Clears all recorded commands.
    /// </summary>
    public void Clear()
    {
        ThrowIfDisposed();
        NativeMethods.Recording_Clear(_handle);
    }

    internal nint Handle
    {
        get
        {
            ThrowIfDisposed();
            return _handle;
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(Recording));
        }
    }

    /// <summary>
    /// Disposes the recording and releases unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            if (_handle != nint.Zero)
            {
                NativeMethods.Recording_Free(_handle);
                _handle = nint.Zero;
            }
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Finalizer that ensures native resources are freed.
    /// </summary>
    ~Recording()
    {
        Dispose();
    }
}
