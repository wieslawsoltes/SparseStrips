// Copyright 2025
// SPDX-License-Identifier: Apache-2.0 OR MIT

using System;

namespace Vello.Native.FastPath;

/// <summary>
/// Managed wrapper for constructing bezier paths via the native API.
/// </summary>
public ref struct NativeBezPath : IDisposable
{
   private nint _handle;

    /// <summary>
    /// Initializes a new empty path.
    /// </summary>
    public NativeBezPath()
    {
        _handle = NativeResult.EnsureHandle(
            NativeMethods.BezPath_New(),
            nameof(NativeMethods.BezPath_New));
    }

    /// <summary>
    /// Gets the native path handle.
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
    /// Gets a value indicating whether the path owns a native handle.
    /// </summary>
    public bool IsAllocated => _handle != nint.Zero;

    /// <summary>
    /// Moves the pen to the specified position.
    /// </summary>
    /// <param name="x">X coordinate.</param>
    /// <param name="y">Y coordinate.</param>
    public void MoveTo(double x, double y)
    {
        EnsureNotDisposed();
        NativeResult.ThrowIfFailed(
            NativeMethods.BezPath_MoveTo(_handle, x, y),
            nameof(NativeMethods.BezPath_MoveTo));
    }

    /// <summary>
    /// Adds a line segment to the path.
    /// </summary>
    /// <param name="x">Destination X coordinate.</param>
    /// <param name="y">Destination Y coordinate.</param>
    public void LineTo(double x, double y)
    {
        EnsureNotDisposed();
        NativeResult.ThrowIfFailed(
            NativeMethods.BezPath_LineTo(_handle, x, y),
            nameof(NativeMethods.BezPath_LineTo));
    }

    /// <summary>
    /// Adds a quadratic bezier segment to the path.
    /// </summary>
    /// <param name="x1">Control point X.</param>
    /// <param name="y1">Control point Y.</param>
    /// <param name="x2">Endpoint X.</param>
    /// <param name="y2">Endpoint Y.</param>
    public void QuadTo(double x1, double y1, double x2, double y2)
    {
        EnsureNotDisposed();
        NativeResult.ThrowIfFailed(
            NativeMethods.BezPath_QuadTo(_handle, x1, y1, x2, y2),
            nameof(NativeMethods.BezPath_QuadTo));
    }

    /// <summary>
    /// Adds a cubic bezier segment to the path.
    /// </summary>
    /// <param name="x1">First control point X.</param>
    /// <param name="y1">First control point Y.</param>
    /// <param name="x2">Second control point X.</param>
    /// <param name="y2">Second control point Y.</param>
    /// <param name="x3">Endpoint X.</param>
    /// <param name="y3">Endpoint Y.</param>
    public void CurveTo(double x1, double y1, double x2, double y2, double x3, double y3)
    {
        EnsureNotDisposed();
        NativeResult.ThrowIfFailed(
            NativeMethods.BezPath_CurveTo(_handle, x1, y1, x2, y2, x3, y3),
            nameof(NativeMethods.BezPath_CurveTo));
    }

    /// <summary>
    /// Closes the current contour.
    /// </summary>
    public void Close()
    {
        EnsureNotDisposed();
        NativeResult.ThrowIfFailed(
            NativeMethods.BezPath_Close(_handle),
            nameof(NativeMethods.BezPath_Close));
    }

    /// <summary>
    /// Clears all path commands.
    /// </summary>
    public void Clear()
    {
        EnsureNotDisposed();
        NativeResult.ThrowIfFailed(
            NativeMethods.BezPath_Clear(_handle),
            nameof(NativeMethods.BezPath_Clear));
    }

    /// <summary>
    /// Releases the native path.
    /// </summary>
    public void Dispose()
    {
        if (_handle == nint.Zero)
        {
            return;
        }

        NativeMethods.BezPath_Free(_handle);
        _handle = nint.Zero;
    }

    private void EnsureNotDisposed()
    {
        if (_handle == nint.Zero)
        {
            throw new ObjectDisposedException(nameof(NativeBezPath));
        }
    }
}
