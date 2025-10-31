// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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

    /// <summary>
    /// Sets the paint to a linear gradient with zero additional allocations.
    /// </summary>
    /// <param name="x0">X coordinate of gradient start point</param>
    /// <param name="y0">Y coordinate of gradient start point</param>
    /// <param name="x1">X coordinate of gradient end point</param>
    /// <param name="y1">Y coordinate of gradient end point</param>
    /// <param name="stops">Color stops defining the gradient (minimum 2 required)</param>
    /// <param name="extend">How to extend the gradient beyond its bounds</param>
    public unsafe void SetPaintLinearGradient(
        double x0, double y0,
        double x1, double y1,
        ReadOnlySpan<ColorStop> stops,
        GradientExtend extend = GradientExtend.Pad)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (stops.Length < 2)
            throw new ArgumentException("Gradient must have at least 2 color stops", nameof(stops));

        ReadOnlySpan<VelloColorStop> nativeStops = MemoryMarshal.Cast<ColorStop, VelloColorStop>(stops);

        fixed (VelloColorStop* pStops = nativeStops)
        {
            VelloException.ThrowIfError(
                NativeMethods.RenderContext_SetPaintLinearGradient(
                    Handle, x0, y0, x1, y1, pStops, (nuint)stops.Length, (VelloExtend)extend));
        }
    }

    /// <summary>
    /// Sets the paint to a linear gradient (array overload).
    /// For zero-allocation, use the ReadOnlySpan&lt;ColorStop&gt; overload.
    /// </summary>
    public void SetPaintLinearGradient(
        double x0, double y0,
        double x1, double y1,
        ColorStop[] stops,
        GradientExtend extend = GradientExtend.Pad)
        => SetPaintLinearGradient(x0, y0, x1, y1, stops.AsSpan(), extend);

    /// <summary>
        /// Sets the paint to a radial gradient with zero additional allocations.
    /// </summary>
    /// <param name="cx">X coordinate of gradient center</param>
    /// <param name="cy">Y coordinate of gradient center</param>
    /// <param name="radius">Radius of the gradient</param>
    /// <param name="stops">Color stops defining the gradient (minimum 2 required)</param>
    /// <param name="extend">How to extend the gradient beyond its bounds</param>
    public unsafe void SetPaintRadialGradient(
        double cx, double cy,
        double radius,
        ReadOnlySpan<ColorStop> stops,
        GradientExtend extend = GradientExtend.Pad)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (stops.Length < 2)
            throw new ArgumentException("Gradient must have at least 2 color stops", nameof(stops));

        ReadOnlySpan<VelloColorStop> nativeStops = MemoryMarshal.Cast<ColorStop, VelloColorStop>(stops);

        fixed (VelloColorStop* pStops = nativeStops)
        {
            VelloException.ThrowIfError(
                NativeMethods.RenderContext_SetPaintRadialGradient(
                    Handle, cx, cy, radius, pStops, (nuint)stops.Length, (VelloExtend)extend));
        }
    }

    /// <summary>
    /// Sets the paint to a radial gradient (array overload).
    /// For zero-allocation, use the ReadOnlySpan&lt;ColorStop&gt; overload.
    /// </summary>
    public void SetPaintRadialGradient(
        double cx, double cy,
        double radius,
        ColorStop[] stops,
        GradientExtend extend = GradientExtend.Pad)
        => SetPaintRadialGradient(cx, cy, radius, stops.AsSpan(), extend);

    /// <summary>
    /// Sets the paint to a sweep (angular) gradient with zero additional allocations.
    /// </summary>
    /// <param name="cx">X coordinate of gradient center</param>
    /// <param name="cy">Y coordinate of gradient center</param>
    /// <param name="startAngle">Starting angle in radians</param>
    /// <param name="endAngle">Ending angle in radians</param>
    /// <param name="stops">Color stops defining the gradient (minimum 2 required)</param>
    /// <param name="extend">How to extend the gradient beyond its bounds</param>
    public unsafe void SetPaintSweepGradient(
        double cx, double cy,
        float startAngle, float endAngle,
        ReadOnlySpan<ColorStop> stops,
        GradientExtend extend = GradientExtend.Pad)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (stops.Length < 2)
            throw new ArgumentException("Gradient must have at least 2 color stops", nameof(stops));

        ReadOnlySpan<VelloColorStop> nativeStops = MemoryMarshal.Cast<ColorStop, VelloColorStop>(stops);

        fixed (VelloColorStop* pStops = nativeStops)
        {
            VelloException.ThrowIfError(
                NativeMethods.RenderContext_SetPaintSweepGradient(
                    Handle, cx, cy, startAngle, endAngle, pStops, (nuint)stops.Length, (VelloExtend)extend));
        }
    }

    /// <summary>
    /// Sets the paint to a sweep (angular) gradient (array overload).
    /// For zero-allocation, use the ReadOnlySpan&lt;ColorStop&gt; overload.
    /// </summary>
    public void SetPaintSweepGradient(
        double cx, double cy,
        float startAngle, float endAngle,
        ColorStop[] stops,
        GradientExtend extend = GradientExtend.Pad)
        => SetPaintSweepGradient(cx, cy, startAngle, endAngle, stops.AsSpan(), extend);

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

    public unsafe void SetTransform(in Affine transform)
    {
        ref readonly VelloAffine native = ref Unsafe.As<Affine, VelloAffine>(ref Unsafe.AsRef(in transform));
        fixed (VelloAffine* ptr = &native)
        {
            VelloException.ThrowIfError(
                NativeMethods.RenderContext_SetTransform(Handle, ptr));
        }
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

    public unsafe void FillRect(in Rect rect)
    {
        ref Rect rectRef = ref Unsafe.AsRef(in rect);
        ref VelloRect native = ref Unsafe.As<Rect, VelloRect>(ref rectRef);
        fixed (VelloRect* ptr = &native)
        {
            VelloException.ThrowIfError(
                NativeMethods.RenderContext_FillRect(Handle, ptr));
        }
    }

    public unsafe void StrokeRect(in Rect rect)
    {
        ref Rect rectRef = ref Unsafe.AsRef(in rect);
        ref VelloRect native = ref Unsafe.As<Rect, VelloRect>(ref rectRef);
        fixed (VelloRect* ptr = &native)
        {
            VelloException.ThrowIfError(
                NativeMethods.RenderContext_StrokeRect(Handle, ptr));
        }
    }

    public unsafe void FillBlurredRoundedRect(in Rect rect, float radius, float stdDev)
    {
        ref Rect rectRef = ref Unsafe.AsRef(in rect);
        ref VelloRect native = ref Unsafe.As<Rect, VelloRect>(ref rectRef);
        fixed (VelloRect* ptr = &native)
        {
            VelloException.ThrowIfError(
                NativeMethods.RenderContext_FillBlurredRoundedRect(Handle, ptr, radius, stdDev));
        }
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

    /// <summary>
    /// Fills glyphs at specified positions. Zero-allocation for glyph runs with ≤256 glyphs.
    /// </summary>
    /// <param name="font">The font data containing the glyph definitions</param>
    /// <param name="fontSize">The size of the font in points</param>
    /// <param name="glyphs">Span of glyphs with their positions</param>
    public unsafe void FillGlyphs(FontData font, float fontSize, ReadOnlySpan<Glyph> glyphs)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(font);

        if (glyphs.IsEmpty)
            return;

        ReadOnlySpan<VelloGlyph> nativeGlyphs = MemoryMarshal.Cast<Glyph, VelloGlyph>(glyphs);

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

    // Array overload for backward compatibility
    public void FillGlyphs(FontData font, float fontSize, Glyph[] glyphs)
        => FillGlyphs(font, fontSize, glyphs.AsSpan());

    /// <summary>
    /// Strokes glyphs at specified positions. Zero-allocation for glyph runs with ≤256 glyphs.
    /// </summary>
    /// <param name="font">The font data containing the glyph definitions</param>
    /// <param name="fontSize">The size of the font in points</param>
    /// <param name="glyphs">Span of glyphs with their positions</param>
    public unsafe void StrokeGlyphs(FontData font, float fontSize, ReadOnlySpan<Glyph> glyphs)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(font);

        if (glyphs.IsEmpty)
            return;

        ReadOnlySpan<VelloGlyph> nativeGlyphs = MemoryMarshal.Cast<Glyph, VelloGlyph>(glyphs);

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

    // Array overload for backward compatibility
    public void StrokeGlyphs(FontData font, float fontSize, Glyph[] glyphs)
        => StrokeGlyphs(font, fontSize, glyphs.AsSpan());

    /// <summary>
    /// Fills text at specified position. Zero-allocation for text with ≤256 characters.
    /// </summary>
    /// <param name="font">The font data to use</param>
    /// <param name="fontSize">The size of the font in points</param>
    /// <param name="text">The text to render</param>
    /// <param name="x">X coordinate of the text position</param>
    /// <param name="y">Y coordinate of the text position</param>
    public void FillText(FontData font, float fontSize, string text, double x, double y)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(font);
        ArgumentNullException.ThrowIfNull(text);

        if (text.Length == 0)
            return;

        const int StackAllocThreshold = 256;
        Glyph[]? rentedGlyphs = null;
        Span<Glyph> glyphs = text.Length <= StackAllocThreshold
            ? stackalloc Glyph[text.Length]
            : default;

        if (glyphs == default)
        {
            rentedGlyphs = ArrayPool<Glyph>.Shared.Rent(text.Length);
            glyphs = rentedGlyphs.AsSpan(0, text.Length);
        }

        try
        {
            int glyphCount = font.TextToGlyphs(text, glyphs);

            for (int i = 0; i < glyphCount; i++)
            {
                glyphs[i] = new Glyph(glyphs[i].Id, glyphs[i].X + (float)x, glyphs[i].Y + (float)y);
            }

            FillGlyphs(font, fontSize, glyphs[..glyphCount]);
        }
        finally
        {
            if (rentedGlyphs is not null)
                ArrayPool<Glyph>.Shared.Return(rentedGlyphs, clearArray: false);
        }
    }

    /// <summary>
    /// Strokes text at specified position. Zero-allocation for text with ≤256 characters.
    /// </summary>
    /// <param name="font">The font data to use</param>
    /// <param name="fontSize">The size of the font in points</param>
    /// <param name="text">The text to render</param>
    /// <param name="x">X coordinate of the text position</param>
    /// <param name="y">Y coordinate of the text position</param>
    public void StrokeText(FontData font, float fontSize, string text, double x, double y)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(font);
        ArgumentNullException.ThrowIfNull(text);

        if (text.Length == 0)
            return;

        const int StackAllocThreshold = 256;
        Glyph[]? rentedGlyphs = null;
        Span<Glyph> glyphs = text.Length <= StackAllocThreshold
            ? stackalloc Glyph[text.Length]
            : default;

        if (glyphs == default)
        {
            rentedGlyphs = ArrayPool<Glyph>.Shared.Rent(text.Length);
            glyphs = rentedGlyphs.AsSpan(0, text.Length);
        }

        try
        {
            int glyphCount = font.TextToGlyphs(text, glyphs);

            for (int i = 0; i < glyphCount; i++)
            {
                glyphs[i] = new Glyph(glyphs[i].Id, glyphs[i].X + (float)x, glyphs[i].Y + (float)y);
            }

            StrokeGlyphs(font, fontSize, glyphs[..glyphCount]);
        }
        finally
        {
            if (rentedGlyphs is not null)
                ArrayPool<Glyph>.Shared.Return(rentedGlyphs, clearArray: false);
        }
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

    public unsafe void SetPaintTransform(in Affine transform)
    {
        ref Affine transformRef = ref Unsafe.AsRef(in transform);
        ref VelloAffine native = ref Unsafe.As<Affine, VelloAffine>(ref transformRef);
        fixed (VelloAffine* ptr = &native)
        {
            VelloException.ThrowIfError(
                NativeMethods.RenderContext_SetPaintTransform(Handle, ptr));
        }
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

    /// <summary>
    /// Gets the current paint kind (for querying paint type).
    /// </summary>
    /// <returns>The kind of paint currently set.</returns>
    public PaintKind GetPaintKind()
    {
        return (PaintKind)NativeMethods.RenderContext_GetPaintKind(Handle);
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

    /// <summary>
    /// Records drawing operations for later replay.
    /// </summary>
    /// <param name="recording">The recording to store operations in.</param>
    /// <param name="recordAction">The callback that performs drawing operations using the provided recorder.</param>
    /// <remarks>
    /// The callback receives a <see cref="Recorder"/> instance that supports the same drawing operations
    /// as <see cref="RenderContext"/>. All operations performed in the callback are recorded into the
    /// <paramref name="recording"/> for later playback using <see cref="ExecuteRecording"/>.
    /// </remarks>
    public unsafe void Record(Recording recording, Action<Recorder> recordAction)
    {
        if (recording == null) throw new ArgumentNullException(nameof(recording));
        if (recordAction == null) throw new ArgumentNullException(nameof(recordAction));

        nint userData = recording.PrepareCallback(recordAction);
        try
        {
            delegate* unmanaged[Cdecl]<nint, nint, void> callback = &RecordCallback;
            VelloException.ThrowIfError(
                NativeMethods.RenderContext_Record(
                    Handle,
                    recording.Handle,
                    callback,
                    userData));
        }
        finally
        {
            recording.ReleaseCallback();
        }
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void RecordCallback(nint userData, nint recorderHandle)
    {
        var state = (Recording.RecordingCallbackState)GCHandle.FromIntPtr(userData).Target!;
        state.Invoke(recorderHandle);
    }

    /// <summary>
    /// Prepares a recording for optimized playback.
    /// </summary>
    /// <param name="recording">The recording to prepare.</param>
    /// <remarks>
    /// This method optimizes the recorded operations for efficient playback.
    /// Call this once after recording is complete and before calling <see cref="ExecuteRecording"/>.
    /// </remarks>
    public void PrepareRecording(Recording recording)
    {
        if (recording == null) throw new ArgumentNullException(nameof(recording));

        VelloException.ThrowIfError(
            NativeMethods.RenderContext_PrepareRecording(Handle, recording.Handle));
    }

    /// <summary>
    /// Executes a previously recorded set of drawing operations.
    /// </summary>
    /// <param name="recording">The recording to execute.</param>
    /// <remarks>
    /// This replays all drawing operations that were recorded into <paramref name="recording"/>
    /// via <see cref="Record"/>. The recording should be prepared using <see cref="PrepareRecording"/>
    /// before execution for optimal performance.
    /// </remarks>
    public void ExecuteRecording(Recording recording)
    {
        if (recording == null) throw new ArgumentNullException(nameof(recording));

        VelloException.ThrowIfError(
            NativeMethods.RenderContext_ExecuteRecording(Handle, recording.Handle));
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
