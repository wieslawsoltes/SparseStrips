// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

using Vello.Geometry;
using Vello.Native;

namespace Vello;

/// <summary>
/// A render context for 2D drawing.
/// </summary>
public sealed class RenderContext : IDisposable
{
    private nint _handle;
    private bool _disposed;

    public RenderContext(ushort width, ushort height)
    {
        _handle = NativeMethods.RenderContext_New(width, height);
        if (_handle == 0)
            throw new VelloException("Failed to create RenderContext");
    }

    public unsafe RenderContext(ushort width, ushort height, RenderSettings settings)
    {
        var nativeSettings = settings.ToNative();
        _handle = NativeMethods.RenderContext_NewWith(width, height, &nativeSettings);
        if (_handle == 0)
            throw new VelloException("Failed to create RenderContext");
    }

    internal nint Handle
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _handle;
        }
    }

    public ushort Width
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return NativeMethods.RenderContext_Width(_handle);
        }
    }

    public ushort Height
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return NativeMethods.RenderContext_Height(_handle);
        }
    }

    public void SetPaint(Color color)
    {
        VelloException.ThrowIfError(
            NativeMethods.RenderContext_SetPaintSolid(
                Handle,
                color.R,
                color.G,
                color.B,
                color.A));
    }

    public unsafe void SetPaintLinearGradient(
        double x0, double y0,
        double x1, double y1,
        ColorStop[] stops,
        GradientExtend extend = GradientExtend.Pad)
    {
        ArgumentNullException.ThrowIfNull(stops);
        if (stops.Length < 2)
            throw new ArgumentException("Gradient must have at least 2 color stops", nameof(stops));

        var nativeStops = new VelloColorStop[stops.Length];
        for (int i = 0; i < stops.Length; i++)
        {
            nativeStops[i] = new VelloColorStop
            {
                Offset = stops[i].Offset,
                R = stops[i].Color.R,
                G = stops[i].Color.G,
                B = stops[i].Color.B,
                A = stops[i].Color.A
            };
        }

        fixed (VelloColorStop* pStops = nativeStops)
        {
            VelloException.ThrowIfError(
                NativeMethods.RenderContext_SetPaintLinearGradient(
                    Handle, x0, y0, x1, y1, pStops, (nuint)stops.Length, (VelloExtend)extend));
        }
    }

    public unsafe void SetPaintRadialGradient(
        double cx, double cy,
        double radius,
        ColorStop[] stops,
        GradientExtend extend = GradientExtend.Pad)
    {
        ArgumentNullException.ThrowIfNull(stops);
        if (stops.Length < 2)
            throw new ArgumentException("Gradient must have at least 2 color stops", nameof(stops));

        var nativeStops = new VelloColorStop[stops.Length];
        for (int i = 0; i < stops.Length; i++)
        {
            nativeStops[i] = new VelloColorStop
            {
                Offset = stops[i].Offset,
                R = stops[i].Color.R,
                G = stops[i].Color.G,
                B = stops[i].Color.B,
                A = stops[i].Color.A
            };
        }

        fixed (VelloColorStop* pStops = nativeStops)
        {
            VelloException.ThrowIfError(
                NativeMethods.RenderContext_SetPaintRadialGradient(
                    Handle, cx, cy, radius, pStops, (nuint)stops.Length, (VelloExtend)extend));
        }
    }

    public unsafe void SetPaintSweepGradient(
        double cx, double cy,
        float startAngle, float endAngle,
        ColorStop[] stops,
        GradientExtend extend = GradientExtend.Pad)
    {
        ArgumentNullException.ThrowIfNull(stops);
        if (stops.Length < 2)
            throw new ArgumentException("Gradient must have at least 2 color stops", nameof(stops));

        var nativeStops = new VelloColorStop[stops.Length];
        for (int i = 0; i < stops.Length; i++)
        {
            nativeStops[i] = new VelloColorStop
            {
                Offset = stops[i].Offset,
                R = stops[i].Color.R,
                G = stops[i].Color.G,
                B = stops[i].Color.B,
                A = stops[i].Color.A
            };
        }

        fixed (VelloColorStop* pStops = nativeStops)
        {
            VelloException.ThrowIfError(
                NativeMethods.RenderContext_SetPaintSweepGradient(
                    Handle, cx, cy, startAngle, endAngle, pStops, (nuint)stops.Length, (VelloExtend)extend));
        }
    }

    public unsafe void PushBlendLayer(BlendMode blendMode)
    {
        var native = new VelloBlendMode
        {
            Mix = (VelloMix)blendMode.Mix,
            Compose = (VelloCompose)blendMode.Compose
        };

        VelloException.ThrowIfError(
            NativeMethods.RenderContext_PushBlendLayer(Handle, &native));
    }

    public void PushClipLayer(BezPath path)
    {
        ArgumentNullException.ThrowIfNull(path);
        VelloException.ThrowIfError(
            NativeMethods.RenderContext_PushClipLayer(Handle, path.Handle));
    }

    public void PushOpacityLayer(float opacity)
    {
        VelloException.ThrowIfError(
            NativeMethods.RenderContext_PushOpacityLayer(Handle, opacity));
    }

    public void PushMaskLayer(Mask mask)
    {
        ArgumentNullException.ThrowIfNull(mask);
        VelloException.ThrowIfError(
            NativeMethods.RenderContext_PushMaskLayer(Handle, mask.Handle));
    }

    public void SetPaintImage(Image image)
    {
        ArgumentNullException.ThrowIfNull(image);
        VelloException.ThrowIfError(
            NativeMethods.RenderContext_SetPaintImage(Handle, image.Handle));
    }

    public void PopLayer()
    {
        VelloException.ThrowIfError(
            NativeMethods.RenderContext_PopLayer(Handle));
    }

    public unsafe void SetTransform(Affine transform)
    {
        var native = new VelloAffine
        {
            M11 = transform.M11,
            M12 = transform.M12,
            M13 = transform.M13,
            M21 = transform.M21,
            M22 = transform.M22,
            M23 = transform.M23
        };

        VelloException.ThrowIfError(
            NativeMethods.RenderContext_SetTransform(Handle, &native));
    }

    public void ResetTransform()
    {
        VelloException.ThrowIfError(
            NativeMethods.RenderContext_ResetTransform(Handle));
    }

    public unsafe void SetStroke(Stroke stroke)
    {
        var native = stroke.ToNative();
        VelloException.ThrowIfError(
            NativeMethods.RenderContext_SetStroke(Handle, &native));
    }

    public unsafe void FillRect(Rect rect)
    {
        var native = new VelloRect
        {
            X0 = rect.X0,
            Y0 = rect.Y0,
            X1 = rect.X1,
            Y1 = rect.Y1
        };

        VelloException.ThrowIfError(
            NativeMethods.RenderContext_FillRect(Handle, &native));
    }

    public unsafe void StrokeRect(Rect rect)
    {
        var native = new VelloRect
        {
            X0 = rect.X0,
            Y0 = rect.Y0,
            X1 = rect.X1,
            Y1 = rect.Y1
        };

        VelloException.ThrowIfError(
            NativeMethods.RenderContext_StrokeRect(Handle, &native));
    }

    public unsafe void FillBlurredRoundedRect(Rect rect, float radius, float stdDev)
    {
        var native = new VelloRect
        {
            X0 = rect.X0,
            Y0 = rect.Y0,
            X1 = rect.X1,
            Y1 = rect.Y1
        };

        VelloException.ThrowIfError(
            NativeMethods.RenderContext_FillBlurredRoundedRect(Handle, &native, radius, stdDev));
    }

    public void FillPath(BezPath path)
    {
        ArgumentNullException.ThrowIfNull(path);
        VelloException.ThrowIfError(
            NativeMethods.RenderContext_FillPath(Handle, path.Handle));
    }

    public void StrokePath(BezPath path)
    {
        ArgumentNullException.ThrowIfNull(path);
        VelloException.ThrowIfError(
            NativeMethods.RenderContext_StrokePath(Handle, path.Handle));
    }

    public unsafe void FillGlyphs(FontData font, float fontSize, Glyph[] glyphs)
    {
        ArgumentNullException.ThrowIfNull(font);
        ArgumentNullException.ThrowIfNull(glyphs);

        // Convert to native glyphs
        var nativeGlyphs = new VelloGlyph[glyphs.Length];
        for (int i = 0; i < glyphs.Length; i++)
        {
            nativeGlyphs[i] = new VelloGlyph
            {
                Id = glyphs[i].Id,
                X = glyphs[i].X,
                Y = glyphs[i].Y
            };
        }

        fixed (VelloGlyph* glyphsPtr = nativeGlyphs)
        {
            VelloException.ThrowIfError(
                NativeMethods.RenderContext_FillGlyphs(
                    Handle,
                    font.Handle,
                    fontSize,
                    glyphsPtr,
                    (nuint)glyphs.Length));
        }
    }

    public unsafe void StrokeGlyphs(FontData font, float fontSize, Glyph[] glyphs)
    {
        ArgumentNullException.ThrowIfNull(font);
        ArgumentNullException.ThrowIfNull(glyphs);

        // Convert to native glyphs
        var nativeGlyphs = new VelloGlyph[glyphs.Length];
        for (int i = 0; i < glyphs.Length; i++)
        {
            nativeGlyphs[i] = new VelloGlyph
            {
                Id = glyphs[i].Id,
                X = glyphs[i].X,
                Y = glyphs[i].Y
            };
        }

        fixed (VelloGlyph* glyphsPtr = nativeGlyphs)
        {
            VelloException.ThrowIfError(
                NativeMethods.RenderContext_StrokeGlyphs(
                    Handle,
                    font.Handle,
                    fontSize,
                    glyphsPtr,
                    (nuint)glyphs.Length));
        }
    }

    public void FillText(FontData font, float fontSize, string text, double x, double y)
    {
        ArgumentNullException.ThrowIfNull(font);
        ArgumentNullException.ThrowIfNull(text);

        var glyphs = font.TextToGlyphs(text);

        // Offset all glyphs by the provided position
        for (int i = 0; i < glyphs.Length; i++)
        {
            glyphs[i] = new Glyph(glyphs[i].Id, glyphs[i].X + (float)x, glyphs[i].Y + (float)y);
        }

        FillGlyphs(font, fontSize, glyphs);
    }

    public void StrokeText(FontData font, float fontSize, string text, double x, double y)
    {
        ArgumentNullException.ThrowIfNull(font);
        ArgumentNullException.ThrowIfNull(text);

        var glyphs = font.TextToGlyphs(text);

        // Offset all glyphs by the provided position
        for (int i = 0; i < glyphs.Length; i++)
        {
            glyphs[i] = new Glyph(glyphs[i].Id, glyphs[i].X + (float)x, glyphs[i].Y + (float)y);
        }

        StrokeGlyphs(font, fontSize, glyphs);
    }

    public void Flush()
    {
        VelloException.ThrowIfError(
            NativeMethods.RenderContext_Flush(Handle));
    }

    public void RenderToPixmap(Pixmap pixmap)
    {
        ArgumentNullException.ThrowIfNull(pixmap);
        VelloException.ThrowIfError(
            NativeMethods.RenderContext_RenderToPixmap(Handle, pixmap.Handle));
    }

    public void Reset()
    {
        VelloException.ThrowIfError(
            NativeMethods.RenderContext_Reset(Handle));
    }

    // Advanced/Optional methods

    public unsafe Stroke GetStroke()
    {
        var native = new VelloStroke();
        VelloException.ThrowIfError(
            NativeMethods.RenderContext_GetStroke(Handle, &native));

        // Construct the Stroke with correct parameter order: width, join, start_cap, end_cap, miter_limit
        return new Stroke(
            width: native.Width,
            join: (Join)native.Join,
            startCap: (Cap)native.StartCap,
            endCap: (Cap)native.EndCap,
            miterLimit: native.MiterLimit);
    }

    public void SetFillRule(FillRule fillRule)
    {
        VelloException.ThrowIfError(
            NativeMethods.RenderContext_SetFillRule(Handle, (VelloFillRule)fillRule));
    }

    public FillRule GetFillRule()
    {
        return (FillRule)NativeMethods.RenderContext_GetFillRule(Handle);
    }

    public unsafe Affine GetTransform()
    {
        var native = new VelloAffine();
        VelloException.ThrowIfError(
            NativeMethods.RenderContext_GetTransform(Handle, &native));

        return new Affine(
            native.M11, native.M12,
            native.M21, native.M22,
            native.M13, native.M23);
    }

    public unsafe void SetPaintTransform(Affine transform)
    {
        var native = new VelloAffine
        {
            M11 = transform.M11,
            M12 = transform.M12,
            M13 = transform.M13,
            M21 = transform.M21,
            M22 = transform.M22,
            M23 = transform.M23
        };

        VelloException.ThrowIfError(
            NativeMethods.RenderContext_SetPaintTransform(Handle, &native));
    }

    public unsafe Affine GetPaintTransform()
    {
        var native = new VelloAffine();
        VelloException.ThrowIfError(
            NativeMethods.RenderContext_GetPaintTransform(Handle, &native));

        return new Affine(
            native.M11, native.M12,
            native.M21, native.M22,
            native.M13, native.M23);
    }

    public void ResetPaintTransform()
    {
        VelloException.ThrowIfError(
            NativeMethods.RenderContext_ResetPaintTransform(Handle));
    }

    public void SetAliasingThreshold(byte? threshold)
    {
        short value = threshold.HasValue ? (short)threshold.Value : (short)-1;
        VelloException.ThrowIfError(
            NativeMethods.RenderContext_SetAliasingThreshold(Handle, value));
    }

    public unsafe void PushLayer(
        BezPath? clipPath = null,
        BlendMode? blendMode = null,
        float? opacity = null,
        Mask? mask = null)
    {
        var clipPathHandle = clipPath?.Handle ?? IntPtr.Zero;
        var maskHandle = mask?.Handle ?? IntPtr.Zero;
        var opacityValue = opacity ?? -1.0f;

        if (blendMode.HasValue)
        {
            var native = blendMode.Value.ToNative();
            VelloException.ThrowIfError(
                NativeMethods.RenderContext_PushLayer(
                    Handle,
                    clipPathHandle,
                    &native,
                    opacityValue,
                    maskHandle));
        }
        else
        {
            VelloException.ThrowIfError(
                NativeMethods.RenderContext_PushLayer(
                    Handle,
                    clipPathHandle,
                    null,
                    opacityValue,
                    maskHandle));
        }
    }

    public unsafe RenderSettings GetRenderSettings()
    {
        var native = new VelloRenderSettings();
        VelloException.ThrowIfError(
            NativeMethods.RenderContext_GetRenderSettings(Handle, &native));

        return new RenderSettings(
            (SimdLevel)native.Level,
            native.NumThreads,
            (RenderMode)native.RenderMode);
    }

    public unsafe void RenderToBuffer(byte[] buffer, ushort width, ushort height, RenderMode renderMode = RenderMode.OptimizeQuality)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        fixed (byte* bufferPtr = buffer)
        {
            VelloException.ThrowIfError(
                NativeMethods.RenderContext_RenderToBuffer(
                    Handle,
                    bufferPtr,
                    (nuint)buffer.Length,
                    width,
                    height,
                    (VelloRenderMode)renderMode));
        }
    }

    public unsafe void RenderToBuffer(Span<byte> buffer, ushort width, ushort height, RenderMode renderMode = RenderMode.OptimizeQuality)
    {
        fixed (byte* bufferPtr = buffer)
        {
            VelloException.ThrowIfError(
                NativeMethods.RenderContext_RenderToBuffer(
                    Handle,
                    bufferPtr,
                    (nuint)buffer.Length,
                    width,
                    height,
                    (VelloRenderMode)renderMode));
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_handle != 0)
            {
                NativeMethods.RenderContext_Free(_handle);
                _handle = 0;
            }
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    ~RenderContext() => Dispose();
}
