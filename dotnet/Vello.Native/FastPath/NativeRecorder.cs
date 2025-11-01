// Copyright 2025
// SPDX-License-Identifier: Apache-2.0 OR MIT

using System;

namespace Vello.Native.FastPath;

/// <summary>
/// Stack-only wrapper around the native recorder handle exposed during recording callbacks.
/// </summary>
public ref struct NativeRecorder
{
    private nint _handle;

    internal NativeRecorder(nint handle)
    {
        _handle = handle;
    }

    /// <summary>
    /// Gets a value indicating whether the recorder handle is valid.
    /// </summary>
    public bool IsAllocated => _handle != nint.Zero;

    /// <summary>
    /// Records a fill rectangle command.
    /// </summary>
    public unsafe void FillRect(in VelloRect rect)
    {
        EnsureActive();
        fixed (VelloRect* rectPtr = &rect)
        {
            NativeResult.ThrowIfFailed(
                NativeMethods.Recorder_FillRect(_handle, rectPtr),
                nameof(NativeMethods.Recorder_FillRect));
        }
    }

    /// <summary>
    /// Records a stroke rectangle command.
    /// </summary>
    public unsafe void StrokeRect(in VelloRect rect)
    {
        EnsureActive();
        fixed (VelloRect* rectPtr = &rect)
        {
            NativeResult.ThrowIfFailed(
                NativeMethods.Recorder_StrokeRect(_handle, rectPtr),
                nameof(NativeMethods.Recorder_StrokeRect));
        }
    }

    /// <summary>
    /// Records a fill path command.
    /// </summary>
    public void FillPath(NativeBezPath path)
    {
        EnsureActive();
        if (!path.IsAllocated)
        {
            throw new ArgumentException("Path is not initialized.", nameof(path));
        }

        NativeResult.ThrowIfFailed(
            NativeMethods.Recorder_FillPath(_handle, path.Handle),
            nameof(NativeMethods.Recorder_FillPath));
    }

    /// <summary>
    /// Records a stroke path command.
    /// </summary>
    public void StrokePath(NativeBezPath path)
    {
        EnsureActive();
        if (!path.IsAllocated)
        {
            throw new ArgumentException("Path is not initialized.", nameof(path));
        }

        NativeResult.ThrowIfFailed(
            NativeMethods.Recorder_StrokePath(_handle, path.Handle),
            nameof(NativeMethods.Recorder_StrokePath));
    }

    /// <summary>
    /// Sets a solid paint color for subsequent commands.
    /// </summary>
    public void SetPaintSolid(byte r, byte g, byte b, byte a)
    {
        EnsureActive();
        NativeResult.ThrowIfFailed(
            NativeMethods.Recorder_SetPaintSolid(_handle, r, g, b, a),
            nameof(NativeMethods.Recorder_SetPaintSolid));
    }

    /// <summary>
    /// Applies stroke parameters for subsequent stroke commands.
    /// </summary>
    public unsafe void SetStroke(in VelloStroke stroke)
    {
        EnsureActive();
        fixed (VelloStroke* strokePtr = &stroke)
        {
            NativeResult.ThrowIfFailed(
                NativeMethods.Recorder_SetStroke(_handle, strokePtr),
                nameof(NativeMethods.Recorder_SetStroke));
        }
    }

    /// <summary>
    /// Sets the current transform.
    /// </summary>
    public unsafe void SetTransform(in VelloAffine transform)
    {
        EnsureActive();
        fixed (VelloAffine* transformPtr = &transform)
        {
            NativeResult.ThrowIfFailed(
                NativeMethods.Recorder_SetTransform(_handle, transformPtr),
                nameof(NativeMethods.Recorder_SetTransform));
        }
    }

    /// <summary>
    /// Sets the fill rule.
    /// </summary>
    public void SetFillRule(VelloFillRule fillRule)
    {
        EnsureActive();
        NativeResult.ThrowIfFailed(
            NativeMethods.Recorder_SetFillRule(_handle, fillRule),
            nameof(NativeMethods.Recorder_SetFillRule));
    }

    /// <summary>
    /// Applies a paint-space transform for gradients or images.
    /// </summary>
    public unsafe void SetPaintTransform(in VelloAffine transform)
    {
        EnsureActive();
        fixed (VelloAffine* transformPtr = &transform)
        {
            NativeResult.ThrowIfFailed(
                NativeMethods.Recorder_SetPaintTransform(_handle, transformPtr),
                nameof(NativeMethods.Recorder_SetPaintTransform));
        }
    }

    /// <summary>
    /// Resets the paint transform to identity.
    /// </summary>
    public void ResetPaintTransform()
    {
        EnsureActive();
        NativeResult.ThrowIfFailed(
            NativeMethods.Recorder_ResetPaintTransform(_handle),
            nameof(NativeMethods.Recorder_ResetPaintTransform));
    }

    /// <summary>
    /// Pushes a clip layer with the supplied path.
    /// </summary>
    public void PushClipLayer(NativeBezPath clipPath)
    {
        EnsureActive();
        if (!clipPath.IsAllocated)
        {
            throw new ArgumentException("Clip path is not initialized.", nameof(clipPath));
        }

        NativeResult.ThrowIfFailed(
            NativeMethods.Recorder_PushClipLayer(_handle, clipPath.Handle),
            nameof(NativeMethods.Recorder_PushClipLayer));
    }

    /// <summary>
    /// Pops the most recently pushed layer.
    /// </summary>
    public void PopLayer()
    {
        EnsureActive();
        NativeResult.ThrowIfFailed(
            NativeMethods.Recorder_PopLayer(_handle),
            nameof(NativeMethods.Recorder_PopLayer));
    }

    internal void Invalidate()
    {
        _handle = nint.Zero;
    }

    private void EnsureActive()
    {
        if (_handle == nint.Zero)
        {
            throw new InvalidOperationException("Recorder handle is no longer valid.");
        }
    }
}
