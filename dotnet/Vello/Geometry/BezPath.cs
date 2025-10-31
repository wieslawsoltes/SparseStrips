// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

using Vello.Geometry;
using Vello.Native;

namespace Vello;

/// <summary>
/// A Bézier path for drawing complex shapes.
/// </summary>
public sealed class BezPath : IDisposable
{
    private nint _handle;
    private bool _disposed;

    public BezPath()
    {
        _handle = NativeMethods.BezPath_New();
        if (_handle == 0)
            throw new VelloException("Failed to create BezPath");
    }

    internal nint Handle
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _handle;
        }
    }

    public BezPath MoveTo(double x, double y)
    {
        VelloException.ThrowIfError(
            NativeMethods.BezPath_MoveTo(Handle, x, y));
        return this;
    }

    public BezPath MoveTo(Point point) => MoveTo(point.X, point.Y);

    public BezPath LineTo(double x, double y)
    {
        VelloException.ThrowIfError(
            NativeMethods.BezPath_LineTo(Handle, x, y));
        return this;
    }

    public BezPath LineTo(Point point) => LineTo(point.X, point.Y);

    public BezPath QuadTo(double x1, double y1, double x2, double y2)
    {
        VelloException.ThrowIfError(
            NativeMethods.BezPath_QuadTo(Handle, x1, y1, x2, y2));
        return this;
    }

    public BezPath QuadTo(Point p1, Point p2) => QuadTo(p1.X, p1.Y, p2.X, p2.Y);

    public BezPath CurveTo(double x1, double y1, double x2, double y2, double x3, double y3)
    {
        VelloException.ThrowIfError(
            NativeMethods.BezPath_CurveTo(Handle, x1, y1, x2, y2, x3, y3));
        return this;
    }

    public BezPath CurveTo(Point p1, Point p2, Point p3) =>
        CurveTo(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y);

    public BezPath Close()
    {
        VelloException.ThrowIfError(
            NativeMethods.BezPath_Close(Handle));
        return this;
    }

    public void Clear()
    {
        VelloException.ThrowIfError(
            NativeMethods.BezPath_Clear(Handle));
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_handle != 0)
            {
                NativeMethods.BezPath_Free(_handle);
                _handle = 0;
            }
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    ~BezPath() => Dispose();
}
