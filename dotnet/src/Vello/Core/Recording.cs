// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

using System.Runtime.InteropServices;
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
    private readonly RecordingCallbackState _callbackState;
    private GCHandle _callbackHandle;

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

        _callbackState = new RecordingCallbackState();
        _callbackHandle = GCHandle.Alloc(_callbackState);
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
    /// Gets a value indicating whether the recording currently holds cached strips generated during preparation.
    /// </summary>
    public bool HasCachedStrips
    {
        get
        {
            ThrowIfDisposed();
            return NativeMethods.Recording_HasCachedStrips(_handle) != 0;
        }
    }

    /// <summary>
    /// Gets the number of cached strips stored with this recording.
    /// </summary>
    public ulong StripCount
    {
        get
        {
            ThrowIfDisposed();
            return (ulong)NativeMethods.Recording_StripCount(_handle);
        }
    }

    /// <summary>
    /// Gets the number of cached alpha bytes stored with this recording.
    /// </summary>
    public ulong AlphaByteCount
    {
        get
        {
            ThrowIfDisposed();
            return (ulong)NativeMethods.Recording_AlphaCount(_handle);
        }
    }

    /// <summary>
    /// Clears all recorded commands.
    /// </summary>
    public void Clear()
    {
        ThrowIfDisposed();
        VelloException.ThrowIfError(
            NativeMethods.Recording_Clear(_handle));
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
            if (_callbackHandle.IsAllocated)
            {
                _callbackState.Clear();
                _callbackHandle.Free();
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

    internal nint PrepareCallback(Action<Recorder> callback)
    {
        ThrowIfDisposed();
        _callbackState.Set(callback);
        return GCHandle.ToIntPtr(_callbackHandle);
    }

    internal void ReleaseCallback()
    {
        _callbackState.Clear();
    }

    internal sealed class RecordingCallbackState
    {
        private Action<Recorder>? _callback;

        public void Set(Action<Recorder> callback) => _callback = callback;

        public void Clear() => _callback = null;

        public void Invoke(nint recorderHandle)
        {
            var callback = _callback;
            if (callback is null)
                return;

            var recorder = new Recorder(recorderHandle);
            callback(recorder);
        }
    }
}
