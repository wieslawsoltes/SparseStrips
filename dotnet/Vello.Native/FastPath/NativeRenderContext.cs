// Copyright 2025
// SPDX-License-Identifier: Apache-2.0 OR MIT

using System;
using System.Runtime.InteropServices;

namespace Vello.Native.FastPath;

/// <summary>
/// Managed wrapper around a native Vello render context for the fast-path API.
/// </summary>
public ref struct NativeRenderContext : IDisposable
{
    private nint _handle;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void RecordCallbackThunk(nint userData, nint recorderHandle);

    /// <summary>
    /// Delegate invoked for each recording callback invocation.
    /// </summary>
    public delegate void NativeRecorderCallback(ref NativeRecorder recorder);

    private static readonly RecordCallbackThunk s_recordCallbackThunk = RecordCallback;
    private static readonly nint s_recordCallbackPtr = Marshal.GetFunctionPointerForDelegate(s_recordCallbackThunk);

    /// <summary>
    /// Initializes a new render context for the given output dimensions.
    /// </summary>
    /// <param name="width">Target width in pixels.</param>
    /// <param name="height">Target height in pixels.</param>
    public NativeRenderContext(ushort width, ushort height)
    {
        _handle = NativeResult.EnsureHandle(
            NativeMethods.RenderContext_New(width, height),
            nameof(NativeMethods.RenderContext_New));
    }

    private NativeRenderContext(nint handle)
    {
        _handle = handle;
    }

    /// <summary>
    /// Creates a render context with explicit render settings.
    /// </summary>
    /// <param name="width">Target width in pixels.</param>
    /// <param name="height">Target height in pixels.</param>
    /// <param name="settings">Render configuration to apply at creation time.</param>
    /// <returns>A managed wrapper owning the native render context.</returns>
    public static unsafe NativeRenderContext CreateWith(
        ushort width,
        ushort height,
        ref VelloRenderSettings settings)
    {
        fixed (VelloRenderSettings* settingsPtr = &settings)
        {
            var handle = NativeResult.EnsureHandle(
                NativeMethods.RenderContext_NewWith(width, height, settingsPtr),
                nameof(NativeMethods.RenderContext_NewWith));
            return new NativeRenderContext(handle);
        }
    }

    /// <summary>
    /// Gets the native context handle.
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
    /// Gets a value indicating whether the handle is currently allocated.
    /// </summary>
    public bool IsAllocated => _handle != nint.Zero;

    /// <summary>
    /// Gets the context width reported by the native renderer.
    /// </summary>
    public ushort Width
    {
        get
        {
            EnsureNotDisposed();
            return NativeMethods.RenderContext_Width(_handle);
        }
    }

    /// <summary>
    /// Gets the context height reported by the native renderer.
    /// </summary>
    public ushort Height
    {
        get
        {
            EnsureNotDisposed();
            return NativeMethods.RenderContext_Height(_handle);
        }
    }

    /// <summary>
    /// Resets the context state to its defaults.
    /// </summary>
    public void Reset()
    {
        EnsureNotDisposed();
        NativeResult.ThrowIfFailed(
            NativeMethods.RenderContext_Reset(_handle),
            nameof(NativeMethods.RenderContext_Reset));
    }

    /// <summary>
    /// Flushes pending drawing commands to the backend.
    /// </summary>
    public void Flush()
    {
        EnsureNotDisposed();
        NativeResult.ThrowIfFailed(
            NativeMethods.RenderContext_Flush(_handle),
            nameof(NativeMethods.RenderContext_Flush));
    }

    /// <summary>
    /// Reads the current render settings.
    /// </summary>
    /// <returns>The render settings snapshot.</returns>
    public unsafe VelloRenderSettings GetRenderSettings()
    {
        EnsureNotDisposed();
        VelloRenderSettings settings = default;
        NativeResult.ThrowIfFailed(
            NativeMethods.RenderContext_GetRenderSettings(_handle, &settings),
            nameof(NativeMethods.RenderContext_GetRenderSettings));
        return settings;
    }

    /// <summary>
    /// Renders the current scene into the supplied pixmap.
    /// </summary>
    /// <param name="pixmap">Target pixmap that receives pixel output.</param>
    public void RenderToPixmap(NativePixmap pixmap)
    {
        EnsureNotDisposed();
        if (!pixmap.IsAllocated)
        {
            throw new ArgumentException("Pixmap is not initialized.", nameof(pixmap));
        }
        NativeResult.ThrowIfFailed(
            NativeMethods.RenderContext_RenderToPixmap(_handle, pixmap.Handle),
            nameof(NativeMethods.RenderContext_RenderToPixmap));
    }

    /// <summary>
    /// Renders the current scene into a caller-provided buffer.
    /// </summary>
    /// <param name="buffer">Destination buffer containing premultiplied RGBA pixels.</param>
    /// <param name="width">Width of the render target in pixels.</param>
    /// <param name="height">Height of the render target in pixels.</param>
    /// <param name="renderMode">Backend render mode to use.</param>
    public unsafe void RenderToBuffer(
        Span<byte> buffer,
        ushort width,
        ushort height,
        VelloRenderMode renderMode)
    {
        EnsureNotDisposed();
        if (buffer.IsEmpty)
        {
            throw new ArgumentException("Render buffer span must not be empty.", nameof(buffer));
        }

        fixed (byte* ptr = buffer)
        {
            NativeResult.ThrowIfFailed(
                NativeMethods.RenderContext_RenderToBuffer(
                    _handle,
                    ptr,
                    (nuint)buffer.Length,
                    width,
                    height,
                    renderMode),
                nameof(NativeMethods.RenderContext_RenderToBuffer));
        }
    }

    /// <summary>
    /// Records drawing commands into a native recording.
    /// </summary>
    public void Record(NativeRecording recording, NativeRecorderCallback callback)
    {
        EnsureNotDisposed();
        ArgumentNullException.ThrowIfNull(callback);

        nint recordingHandle = recording.Handle;
        var handle = GCHandle.Alloc(callback);
        nint userData = GCHandle.ToIntPtr(handle);
        try
        {
            NativeResult.ThrowIfFailed(
                NativeMethods.RenderContext_Record(
                    _handle,
                    recordingHandle,
                    s_recordCallbackPtr,
                    userData),
                nameof(NativeMethods.RenderContext_Record));
        }
        finally
        {
            handle.Free();
        }
    }

    /// <summary>
    /// Optimizes a recording for later playback.
    /// </summary>
    public void PrepareRecording(NativeRecording recording)
    {
        EnsureNotDisposed();
        nint recordingHandle = recording.Handle;
        NativeResult.ThrowIfFailed(
            NativeMethods.RenderContext_PrepareRecording(_handle, recordingHandle),
            nameof(NativeMethods.RenderContext_PrepareRecording));
    }

    /// <summary>
    /// Executes a previously recorded command stream.
    /// </summary>
    public void ExecuteRecording(NativeRecording recording)
    {
        EnsureNotDisposed();
        nint recordingHandle = recording.Handle;
        NativeResult.ThrowIfFailed(
            NativeMethods.RenderContext_ExecuteRecording(_handle, recordingHandle),
            nameof(NativeMethods.RenderContext_ExecuteRecording));
    }

    /// <summary>
    /// Sets a solid paint color.
    /// </summary>
    public void SetPaintSolid(byte r, byte g, byte b, byte a)
    {
        EnsureNotDisposed();
        NativeResult.ThrowIfFailed(
            NativeMethods.RenderContext_SetPaintSolid(_handle, r, g, b, a),
            nameof(NativeMethods.RenderContext_SetPaintSolid));
    }

    /// <summary>
    /// Sets a linear gradient paint.
    /// </summary>
    public unsafe void SetPaintLinearGradient(
        double x0,
        double y0,
        double x1,
        double y1,
        ReadOnlySpan<VelloColorStop> stops,
        VelloExtend extend)
    {
        EnsureNotDisposed();
        if (stops.IsEmpty)
        {
            throw new ArgumentException("Gradient stops must not be empty.", nameof(stops));
        }

        fixed (VelloColorStop* stopsPtr = stops)
        {
            NativeResult.ThrowIfFailed(
                NativeMethods.RenderContext_SetPaintLinearGradient(
                    _handle,
                    x0,
                    y0,
                    x1,
                    y1,
                    stopsPtr,
                    (nuint)stops.Length,
                    extend),
                nameof(NativeMethods.RenderContext_SetPaintLinearGradient));
        }
    }

    /// <summary>
    /// Applies stroke settings to the context.
    /// </summary>
    public unsafe void SetStroke(in VelloStroke stroke)
    {
        EnsureNotDisposed();
        fixed (VelloStroke* strokePtr = &stroke)
        {
            NativeResult.ThrowIfFailed(
                NativeMethods.RenderContext_SetStroke(_handle, strokePtr),
                nameof(NativeMethods.RenderContext_SetStroke));
        }
    }

    /// <summary>
    /// Sets the current fill rule.
    /// </summary>
    public void SetFillRule(VelloFillRule fillRule)
    {
        EnsureNotDisposed();
        NativeResult.ThrowIfFailed(
            NativeMethods.RenderContext_SetFillRule(_handle, fillRule),
            nameof(NativeMethods.RenderContext_SetFillRule));
    }

    /// <summary>
    /// Fills the specified rectangle.
    /// </summary>
    public unsafe void FillRect(in VelloRect rect)
    {
        EnsureNotDisposed();
        fixed (VelloRect* rectPtr = &rect)
        {
            NativeResult.ThrowIfFailed(
                NativeMethods.RenderContext_FillRect(_handle, rectPtr),
                nameof(NativeMethods.RenderContext_FillRect));
        }
    }

    /// <summary>
    /// Strokes the specified rectangle.
    /// </summary>
    public unsafe void StrokeRect(in VelloRect rect)
    {
        EnsureNotDisposed();
        fixed (VelloRect* rectPtr = &rect)
        {
            NativeResult.ThrowIfFailed(
                NativeMethods.RenderContext_StrokeRect(_handle, rectPtr),
                nameof(NativeMethods.RenderContext_StrokeRect));
        }
    }

    /// <summary>
    /// Fills the provided path.
    /// </summary>
    public void FillPath(NativeBezPath path)
    {
        EnsureNotDisposed();
        if (!path.IsAllocated)
        {
            throw new ArgumentException("Path is not initialized.", nameof(path));
        }

        NativeResult.ThrowIfFailed(
            NativeMethods.RenderContext_FillPath(_handle, path.Handle),
            nameof(NativeMethods.RenderContext_FillPath));
    }

    /// <summary>
    /// Strokes the provided path.
    /// </summary>
    public void StrokePath(NativeBezPath path)
    {
        EnsureNotDisposed();
        if (!path.IsAllocated)
        {
            throw new ArgumentException("Path is not initialized.", nameof(path));
        }

        NativeResult.ThrowIfFailed(
            NativeMethods.RenderContext_StrokePath(_handle, path.Handle),
            nameof(NativeMethods.RenderContext_StrokePath));
    }

    /// <summary>
    /// Releases the native render context.
    /// </summary>
    public void Dispose()
    {
        if (_handle == nint.Zero)
        {
            return;
        }

        NativeMethods.RenderContext_Free(_handle);
        _handle = nint.Zero;
    }

    private void EnsureNotDisposed()
    {
        if (_handle == nint.Zero)
        {
            throw new ObjectDisposedException(nameof(NativeRenderContext));
        }
    }

    private static void RecordCallback(nint userData, nint recorderHandle)
    {
        var handle = GCHandle.FromIntPtr(userData);
        if (handle.Target is NativeRecorderCallback callback)
        {
            var recorder = new NativeRecorder(recorderHandle);
            callback(ref recorder);
            recorder.Invalidate();
        }
    }
}
