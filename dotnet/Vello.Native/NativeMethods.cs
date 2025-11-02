// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Vello.Native;

public static unsafe class NativeMethods
{
    /// <summary>
    /// Module initializer to ensure native library loader is registered before any native symbol lookups.
    /// Ensures export resolution succeeds even when callers access function pointers during type initialization.
    /// </summary>
    [ModuleInitializer]
    internal static void Initialize()
    {
        NativeLibraryLoader.EnsureLoaded();
    }

    // Error codes
    public const int VELLO_OK = 0;
    public const int VELLO_ERROR_NULL_POINTER = -1;
    public const int VELLO_ERROR_INVALID_HANDLE = -2;
    public const int VELLO_ERROR_RENDER_FAILED = -3;
    public const int VELLO_ERROR_OUT_OF_MEMORY = -4;
    public const int VELLO_ERROR_INVALID_PARAMETER = -5;
    public const int VELLO_ERROR_PNG_DECODE = -6;
    public const int VELLO_ERROR_PNG_ENCODE = -7;

    // Version and capabilities
    private static readonly delegate* unmanaged[Cdecl]<nint> s_vello_version = (delegate* unmanaged[Cdecl]<nint>)NativeLibraryLoader.GetExport("vello_version");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe nint Version()
    {
        return s_vello_version();
    }


    private static readonly delegate* unmanaged[Cdecl]<VelloSimdLevel> s_vello_simd_detect = (delegate* unmanaged[Cdecl]<VelloSimdLevel>)NativeLibraryLoader.GetExport("vello_simd_detect");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe VelloSimdLevel SimdDetect()
    {
        return s_vello_simd_detect();
    }


    // Error handling
    private static readonly delegate* unmanaged[Cdecl]<nint> s_vello_get_last_error = (delegate* unmanaged[Cdecl]<nint>)NativeLibraryLoader.GetExport("vello_get_last_error");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe nint GetLastError()
    {
        return s_vello_get_last_error();
    }


    private static readonly delegate* unmanaged[Cdecl]<void> s_vello_clear_last_error = (delegate* unmanaged[Cdecl]<void>)NativeLibraryLoader.GetExport("vello_clear_last_error");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void ClearLastError()
    {
        s_vello_clear_last_error();
    }


    // RenderContext
    private static readonly delegate* unmanaged[Cdecl]<ushort, ushort, nint> s_vello_render_context_new = (delegate* unmanaged[Cdecl]<ushort, ushort, nint>)NativeLibraryLoader.GetExport("vello_render_context_new");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe nint RenderContext_New(
        ushort width,
        ushort height
    )
    {
        return s_vello_render_context_new(width, height);
    }


    private static readonly delegate* unmanaged[Cdecl]<ushort, ushort, VelloRenderSettings*, nint> s_vello_render_context_new_with = (delegate* unmanaged[Cdecl]<ushort, ushort, VelloRenderSettings*, nint>)NativeLibraryLoader.GetExport("vello_render_context_new_with");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe nint RenderContext_NewWith(
        ushort width,
        ushort height,
        VelloRenderSettings* settings
    )
    {
        return s_vello_render_context_new_with(width, height, settings);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, void> s_vello_render_context_free = (delegate* unmanaged[Cdecl]<nint, void>)NativeLibraryLoader.GetExport("vello_render_context_free");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void RenderContext_Free(
        nint ctx
    )
    {
        s_vello_render_context_free(ctx);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, ushort> s_vello_render_context_width = (delegate* unmanaged[Cdecl]<nint, ushort>)NativeLibraryLoader.GetExport("vello_render_context_width");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe ushort RenderContext_Width(
        nint ctx
    )
    {
        return s_vello_render_context_width(ctx);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, ushort> s_vello_render_context_height = (delegate* unmanaged[Cdecl]<nint, ushort>)NativeLibraryLoader.GetExport("vello_render_context_height");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe ushort RenderContext_Height(
        nint ctx
    )
    {
        return s_vello_render_context_height(ctx);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, int> s_vello_render_context_reset = (delegate* unmanaged[Cdecl]<nint, int>)NativeLibraryLoader.GetExport("vello_render_context_reset");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int RenderContext_Reset(
        nint ctx
    )
    {
        return s_vello_render_context_reset(ctx);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, byte, byte, byte, byte, int> s_vello_render_context_set_paint_solid = (delegate* unmanaged[Cdecl]<nint, byte, byte, byte, byte, int>)NativeLibraryLoader.GetExport("vello_render_context_set_paint_solid");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int RenderContext_SetPaintSolid(
        nint ctx,
        byte r,
        byte g,
        byte b,
        byte a
    )
    {
        return s_vello_render_context_set_paint_solid(ctx, r, g, b, a);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, double, double, double, double, VelloColorStop*, nuint, VelloExtend, int> s_vello_render_context_set_paint_linear_gradient = (delegate* unmanaged[Cdecl]<nint, double, double, double, double, VelloColorStop*, nuint, VelloExtend, int>)NativeLibraryLoader.GetExport("vello_render_context_set_paint_linear_gradient");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int RenderContext_SetPaintLinearGradient(
        nint ctx,
        double x0,
        double y0,
        double x1,
        double y1,
        VelloColorStop* stops,
        nuint stopCount,
        VelloExtend extend
    )
    {
        return s_vello_render_context_set_paint_linear_gradient(ctx, x0, y0, x1, y1, stops, stopCount, extend);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, double, double, double, VelloColorStop*, nuint, VelloExtend, int> s_vello_render_context_set_paint_radial_gradient = (delegate* unmanaged[Cdecl]<nint, double, double, double, VelloColorStop*, nuint, VelloExtend, int>)NativeLibraryLoader.GetExport("vello_render_context_set_paint_radial_gradient");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int RenderContext_SetPaintRadialGradient(
        nint ctx,
        double cx,
        double cy,
        double radius,
        VelloColorStop* stops,
        nuint stopCount,
        VelloExtend extend
    )
    {
        return s_vello_render_context_set_paint_radial_gradient(ctx, cx, cy, radius, stops, stopCount, extend);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, double, double, float, float, VelloColorStop*, nuint, VelloExtend, int> s_vello_render_context_set_paint_sweep_gradient = (delegate* unmanaged[Cdecl]<nint, double, double, float, float, VelloColorStop*, nuint, VelloExtend, int>)NativeLibraryLoader.GetExport("vello_render_context_set_paint_sweep_gradient");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int RenderContext_SetPaintSweepGradient(
        nint ctx,
        double cx,
        double cy,
        float startAngle,
        float endAngle,
        VelloColorStop* stops,
        nuint stopCount,
        VelloExtend extend
    )
    {
        return s_vello_render_context_set_paint_sweep_gradient(ctx, cx, cy, startAngle, endAngle, stops, stopCount, extend);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, VelloAffine*, int> s_vello_render_context_set_transform = (delegate* unmanaged[Cdecl]<nint, VelloAffine*, int>)NativeLibraryLoader.GetExport("vello_render_context_set_transform");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int RenderContext_SetTransform(
        nint ctx,
        VelloAffine* transform
    )
    {
        return s_vello_render_context_set_transform(ctx, transform);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, int> s_vello_render_context_reset_transform = (delegate* unmanaged[Cdecl]<nint, int>)NativeLibraryLoader.GetExport("vello_render_context_reset_transform");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int RenderContext_ResetTransform(
        nint ctx
    )
    {
        return s_vello_render_context_reset_transform(ctx);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, VelloAffine*, int> s_vello_render_context_get_transform = (delegate* unmanaged[Cdecl]<nint, VelloAffine*, int>)NativeLibraryLoader.GetExport("vello_render_context_get_transform");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int RenderContext_GetTransform(
        nint ctx,
        VelloAffine* outTransform
    )
    {
        return s_vello_render_context_get_transform(ctx, outTransform);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, VelloStroke*, int> s_vello_render_context_set_stroke = (delegate* unmanaged[Cdecl]<nint, VelloStroke*, int>)NativeLibraryLoader.GetExport("vello_render_context_set_stroke");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int RenderContext_SetStroke(
        nint ctx,
        VelloStroke* stroke
    )
    {
        return s_vello_render_context_set_stroke(ctx, stroke);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, VelloFillRule, int> s_vello_render_context_set_fill_rule = (delegate* unmanaged[Cdecl]<nint, VelloFillRule, int>)NativeLibraryLoader.GetExport("vello_render_context_set_fill_rule");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int RenderContext_SetFillRule(
        nint ctx,
        VelloFillRule fillRule
    )
    {
        return s_vello_render_context_set_fill_rule(ctx, fillRule);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, VelloRect*, int> s_vello_render_context_fill_rect = (delegate* unmanaged[Cdecl]<nint, VelloRect*, int>)NativeLibraryLoader.GetExport("vello_render_context_fill_rect");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int RenderContext_FillRect(
        nint ctx,
        VelloRect* rect
    )
    {
        return s_vello_render_context_fill_rect(ctx, rect);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, VelloRect*, int> s_vello_render_context_stroke_rect = (delegate* unmanaged[Cdecl]<nint, VelloRect*, int>)NativeLibraryLoader.GetExport("vello_render_context_stroke_rect");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int RenderContext_StrokeRect(
        nint ctx,
        VelloRect* rect
    )
    {
        return s_vello_render_context_stroke_rect(ctx, rect);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, VelloRect*, float, float, int> s_vello_render_context_fill_blurred_rounded_rect = (delegate* unmanaged[Cdecl]<nint, VelloRect*, float, float, int>)NativeLibraryLoader.GetExport("vello_render_context_fill_blurred_rounded_rect");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int RenderContext_FillBlurredRoundedRect(
        nint ctx,
        VelloRect* rect,
        float radius,
        float stdDev
    )
    {
        return s_vello_render_context_fill_blurred_rounded_rect(ctx, rect, radius, stdDev);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, nint, int> s_vello_render_context_fill_path = (delegate* unmanaged[Cdecl]<nint, nint, int>)NativeLibraryLoader.GetExport("vello_render_context_fill_path");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int RenderContext_FillPath(
        nint ctx,
        nint path
    )
    {
        return s_vello_render_context_fill_path(ctx, path);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, nint, int> s_vello_render_context_stroke_path = (delegate* unmanaged[Cdecl]<nint, nint, int>)NativeLibraryLoader.GetExport("vello_render_context_stroke_path");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int RenderContext_StrokePath(
        nint ctx,
        nint path
    )
    {
        return s_vello_render_context_stroke_path(ctx, path);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, VelloBlendMode*, int> s_vello_render_context_push_blend_layer = (delegate* unmanaged[Cdecl]<nint, VelloBlendMode*, int>)NativeLibraryLoader.GetExport("vello_render_context_push_blend_layer");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int RenderContext_PushBlendLayer(
        nint ctx,
        VelloBlendMode* blendMode
    )
    {
        return s_vello_render_context_push_blend_layer(ctx, blendMode);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, nint, int> s_vello_render_context_push_clip_layer = (delegate* unmanaged[Cdecl]<nint, nint, int>)NativeLibraryLoader.GetExport("vello_render_context_push_clip_layer");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int RenderContext_PushClipLayer(
        nint ctx,
        nint path
    )
    {
        return s_vello_render_context_push_clip_layer(ctx, path);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, float, int> s_vello_render_context_push_opacity_layer = (delegate* unmanaged[Cdecl]<nint, float, int>)NativeLibraryLoader.GetExport("vello_render_context_push_opacity_layer");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int RenderContext_PushOpacityLayer(
        nint ctx,
        float opacity
    )
    {
        return s_vello_render_context_push_opacity_layer(ctx, opacity);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, int> s_vello_render_context_pop_layer = (delegate* unmanaged[Cdecl]<nint, int>)NativeLibraryLoader.GetExport("vello_render_context_pop_layer");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int RenderContext_PopLayer(
        nint ctx
    )
    {
        return s_vello_render_context_pop_layer(ctx);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, int> s_vello_render_context_flush = (delegate* unmanaged[Cdecl]<nint, int>)NativeLibraryLoader.GetExport("vello_render_context_flush");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int RenderContext_Flush(
        nint ctx
    )
    {
        return s_vello_render_context_flush(ctx);
    }


    // Advanced/Optional methods

    private static readonly delegate* unmanaged[Cdecl]<nint, VelloStroke*, int> s_vello_render_context_get_stroke = (delegate* unmanaged[Cdecl]<nint, VelloStroke*, int>)NativeLibraryLoader.GetExport("vello_render_context_get_stroke");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int RenderContext_GetStroke(
        nint ctx,
        VelloStroke* outStroke
    )
    {
        return s_vello_render_context_get_stroke(ctx, outStroke);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, VelloFillRule> s_vello_render_context_get_fill_rule = (delegate* unmanaged[Cdecl]<nint, VelloFillRule>)NativeLibraryLoader.GetExport("vello_render_context_get_fill_rule");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe VelloFillRule RenderContext_GetFillRule(
        nint ctx
    )
    {
        return s_vello_render_context_get_fill_rule(ctx);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, VelloAffine*, int> s_vello_render_context_set_paint_transform = (delegate* unmanaged[Cdecl]<nint, VelloAffine*, int>)NativeLibraryLoader.GetExport("vello_render_context_set_paint_transform");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int RenderContext_SetPaintTransform(
        nint ctx,
        VelloAffine* transform
    )
    {
        return s_vello_render_context_set_paint_transform(ctx, transform);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, VelloAffine*, int> s_vello_render_context_get_paint_transform = (delegate* unmanaged[Cdecl]<nint, VelloAffine*, int>)NativeLibraryLoader.GetExport("vello_render_context_get_paint_transform");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int RenderContext_GetPaintTransform(
        nint ctx,
        VelloAffine* outTransform
    )
    {
        return s_vello_render_context_get_paint_transform(ctx, outTransform);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, int> s_vello_render_context_reset_paint_transform = (delegate* unmanaged[Cdecl]<nint, int>)NativeLibraryLoader.GetExport("vello_render_context_reset_paint_transform");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int RenderContext_ResetPaintTransform(
        nint ctx
    )
    {
        return s_vello_render_context_reset_paint_transform(ctx);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, VelloPaintKind> s_vello_render_context_get_paint_kind = (delegate* unmanaged[Cdecl]<nint, VelloPaintKind>)NativeLibraryLoader.GetExport("vello_render_context_get_paint_kind");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe VelloPaintKind RenderContext_GetPaintKind(
        nint ctx
    )
    {
        return s_vello_render_context_get_paint_kind(ctx);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, short, int> s_vello_render_context_set_aliasing_threshold = (delegate* unmanaged[Cdecl]<nint, short, int>)NativeLibraryLoader.GetExport("vello_render_context_set_aliasing_threshold");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int RenderContext_SetAliasingThreshold(
        nint ctx,
        short threshold
    )
    {
        return s_vello_render_context_set_aliasing_threshold(ctx, threshold);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, nint, VelloBlendMode*, float, nint, int> s_vello_render_context_push_layer = (delegate* unmanaged[Cdecl]<nint, nint, VelloBlendMode*, float, nint, int>)NativeLibraryLoader.GetExport("vello_render_context_push_layer");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int RenderContext_PushLayer(
        nint ctx,
        nint clipPath,
        VelloBlendMode* blendMode,
        float opacity,
        nint mask
    )
    {
        return s_vello_render_context_push_layer(ctx, clipPath, blendMode, opacity, mask);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, VelloRenderSettings*, int> s_vello_render_context_get_render_settings = (delegate* unmanaged[Cdecl]<nint, VelloRenderSettings*, int>)NativeLibraryLoader.GetExport("vello_render_context_get_render_settings");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int RenderContext_GetRenderSettings(
        nint ctx,
        VelloRenderSettings* outSettings
    )
    {
        return s_vello_render_context_get_render_settings(ctx, outSettings);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, byte*, nuint, ushort, ushort, VelloRenderMode, int> s_vello_render_context_render_to_buffer = (delegate* unmanaged[Cdecl]<nint, byte*, nuint, ushort, ushort, VelloRenderMode, int>)NativeLibraryLoader.GetExport("vello_render_context_render_to_buffer");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int RenderContext_RenderToBuffer(
        nint ctx,
        byte* buffer,
        nuint bufferLen,
        ushort width,
        ushort height,
        VelloRenderMode renderMode
    )
    {
        return s_vello_render_context_render_to_buffer(ctx, buffer, bufferLen, width, height, renderMode);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, nint, int> s_vello_render_context_render_to_pixmap = (delegate* unmanaged[Cdecl]<nint, nint, int>)NativeLibraryLoader.GetExport("vello_render_context_render_to_pixmap");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int RenderContext_RenderToPixmap(
        nint ctx,
        nint pixmap
    )
    {
        return s_vello_render_context_render_to_pixmap(ctx, pixmap);
    }


    // Pixmap
    private static readonly delegate* unmanaged[Cdecl]<ushort, ushort, nint> s_vello_pixmap_new = (delegate* unmanaged[Cdecl]<ushort, ushort, nint>)NativeLibraryLoader.GetExport("vello_pixmap_new");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe nint Pixmap_New(
        ushort width,
        ushort height
    )
    {
        return s_vello_pixmap_new(width, height);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, void> s_vello_pixmap_free = (delegate* unmanaged[Cdecl]<nint, void>)NativeLibraryLoader.GetExport("vello_pixmap_free");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void Pixmap_Free(
        nint pixmap
    )
    {
        s_vello_pixmap_free(pixmap);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, ushort> s_vello_pixmap_width = (delegate* unmanaged[Cdecl]<nint, ushort>)NativeLibraryLoader.GetExport("vello_pixmap_width");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe ushort Pixmap_Width(
        nint pixmap
    )
    {
        return s_vello_pixmap_width(pixmap);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, ushort> s_vello_pixmap_height = (delegate* unmanaged[Cdecl]<nint, ushort>)NativeLibraryLoader.GetExport("vello_pixmap_height");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe ushort Pixmap_Height(
        nint pixmap
    )
    {
        return s_vello_pixmap_height(pixmap);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, nint*, nuint*, int> s_vello_pixmap_data = (delegate* unmanaged[Cdecl]<nint, nint*, nuint*, int>)NativeLibraryLoader.GetExport("vello_pixmap_data");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int Pixmap_Data(
        nint pixmap,
        nint* outPtr,
        nuint* outLen
    )
    {
        return s_vello_pixmap_data(pixmap, outPtr, outLen);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, nint*, nuint*, int> s_vello_pixmap_data_mut = (delegate* unmanaged[Cdecl]<nint, nint*, nuint*, int>)NativeLibraryLoader.GetExport("vello_pixmap_data_mut");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int Pixmap_DataMut(
        nint pixmap,
        nint* outPtr,
        nuint* outLen
    )
    {
        return s_vello_pixmap_data_mut(pixmap, outPtr, outLen);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, ushort, ushort, int> s_vello_pixmap_resize = (delegate* unmanaged[Cdecl]<nint, ushort, ushort, int>)NativeLibraryLoader.GetExport("vello_pixmap_resize");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int Pixmap_Resize(
        nint pixmap,
        ushort width,
        ushort height
    )
    {
        return s_vello_pixmap_resize(pixmap, width, height);
    }


    private static readonly delegate* unmanaged[Cdecl]<byte*, nuint, nint> s_vello_pixmap_from_png = (delegate* unmanaged[Cdecl]<byte*, nuint, nint>)NativeLibraryLoader.GetExport("vello_pixmap_from_png");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe nint Pixmap_FromPng(
        byte* data,
        nuint len
    )
    {
        return s_vello_pixmap_from_png(data, len);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, byte**, nuint*, int> s_vello_pixmap_to_png = (delegate* unmanaged[Cdecl]<nint, byte**, nuint*, int>)NativeLibraryLoader.GetExport("vello_pixmap_to_png");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int Pixmap_ToPng(
        nint pixmap,
        byte** outData,
        nuint* outLen
    )
    {
        return s_vello_pixmap_to_png(pixmap, outData, outLen);
    }


    private static readonly delegate* unmanaged[Cdecl]<byte*, nuint, void> s_vello_png_data_free = (delegate* unmanaged[Cdecl]<byte*, nuint, void>)NativeLibraryLoader.GetExport("vello_png_data_free");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void PngDataFree(
        byte* data,
        nuint len
    )
    {
        s_vello_png_data_free(data, len);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, ushort, ushort, VelloPremulRgba8*, int> s_vello_pixmap_sample = (delegate* unmanaged[Cdecl]<nint, ushort, ushort, VelloPremulRgba8*, int>)NativeLibraryLoader.GetExport("vello_pixmap_sample");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int Pixmap_Sample(
        nint pixmap,
        ushort x,
        ushort y,
        VelloPremulRgba8* outPixel
    )
    {
        return s_vello_pixmap_sample(pixmap, x, y, outPixel);
    }


    // BezPath
    private static readonly delegate* unmanaged[Cdecl]<nint> s_vello_bezpath_new = (delegate* unmanaged[Cdecl]<nint>)NativeLibraryLoader.GetExport("vello_bezpath_new");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe nint BezPath_New()
    {
        return s_vello_bezpath_new();
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, void> s_vello_bezpath_free = (delegate* unmanaged[Cdecl]<nint, void>)NativeLibraryLoader.GetExport("vello_bezpath_free");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void BezPath_Free(
        nint path
    )
    {
        s_vello_bezpath_free(path);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, double, double, int> s_vello_bezpath_move_to = (delegate* unmanaged[Cdecl]<nint, double, double, int>)NativeLibraryLoader.GetExport("vello_bezpath_move_to");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int BezPath_MoveTo(
        nint path,
        double x,
        double y
    )
    {
        return s_vello_bezpath_move_to(path, x, y);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, double, double, int> s_vello_bezpath_line_to = (delegate* unmanaged[Cdecl]<nint, double, double, int>)NativeLibraryLoader.GetExport("vello_bezpath_line_to");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int BezPath_LineTo(
        nint path,
        double x,
        double y
    )
    {
        return s_vello_bezpath_line_to(path, x, y);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, double, double, double, double, int> s_vello_bezpath_quad_to = (delegate* unmanaged[Cdecl]<nint, double, double, double, double, int>)NativeLibraryLoader.GetExport("vello_bezpath_quad_to");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int BezPath_QuadTo(
        nint path,
        double x1,
        double y1,
        double x2,
        double y2
    )
    {
        return s_vello_bezpath_quad_to(path, x1, y1, x2, y2);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, double, double, double, double, double, double, int> s_vello_bezpath_curve_to = (delegate* unmanaged[Cdecl]<nint, double, double, double, double, double, double, int>)NativeLibraryLoader.GetExport("vello_bezpath_curve_to");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int BezPath_CurveTo(
        nint path,
        double x1,
        double y1,
        double x2,
        double y2,
        double x3,
        double y3
    )
    {
        return s_vello_bezpath_curve_to(path, x1, y1, x2, y2, x3, y3);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, int> s_vello_bezpath_close = (delegate* unmanaged[Cdecl]<nint, int>)NativeLibraryLoader.GetExport("vello_bezpath_close");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int BezPath_Close(
        nint path
    )
    {
        return s_vello_bezpath_close(path);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, int> s_vello_bezpath_clear = (delegate* unmanaged[Cdecl]<nint, int>)NativeLibraryLoader.GetExport("vello_bezpath_clear");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int BezPath_Clear(
        nint path
    )
    {
        return s_vello_bezpath_clear(path);
    }


    // Text Rendering
    private static readonly delegate* unmanaged[Cdecl]<byte*, nuint, uint, nint> s_vello_font_data_new = (delegate* unmanaged[Cdecl]<byte*, nuint, uint, nint>)NativeLibraryLoader.GetExport("vello_font_data_new");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe nint FontData_New(
        byte* data,
        nuint len,
        uint index
    )
    {
        return s_vello_font_data_new(data, len, index);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, void> s_vello_font_data_free = (delegate* unmanaged[Cdecl]<nint, void>)NativeLibraryLoader.GetExport("vello_font_data_free");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void FontData_Free(
        nint font
    )
    {
        s_vello_font_data_free(font);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, nint, float, VelloGlyph*, nuint, int> s_vello_render_context_fill_glyphs = (delegate* unmanaged[Cdecl]<nint, nint, float, VelloGlyph*, nuint, int>)NativeLibraryLoader.GetExport("vello_render_context_fill_glyphs");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int RenderContext_FillGlyphs(
        nint ctx,
        nint font,
        float fontSize,
        VelloGlyph* glyphs,
        nuint glyphCount
    )
    {
        return s_vello_render_context_fill_glyphs(ctx, font, fontSize, glyphs, glyphCount);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, nint, float, VelloGlyph*, nuint, int> s_vello_render_context_stroke_glyphs = (delegate* unmanaged[Cdecl]<nint, nint, float, VelloGlyph*, nuint, int>)NativeLibraryLoader.GetExport("vello_render_context_stroke_glyphs");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int RenderContext_StrokeGlyphs(
        nint ctx,
        nint font,
        float fontSize,
        VelloGlyph* glyphs,
        nuint glyphCount
    )
    {
        return s_vello_render_context_stroke_glyphs(ctx, font, fontSize, glyphs, glyphCount);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, byte*, VelloGlyph*, nuint, nuint*, int> s_vello_font_data_text_to_glyphs = (delegate* unmanaged[Cdecl]<nint, byte*, VelloGlyph*, nuint, nuint*, int>)NativeLibraryLoader.GetExport("vello_font_data_text_to_glyphs");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int FontData_TextToGlyphs(
        nint font,
        byte* text,
        VelloGlyph* outGlyphs,
        nuint maxGlyphs,
        nuint* outCount
    )
    {
        return s_vello_font_data_text_to_glyphs(font, text, outGlyphs, maxGlyphs, outCount);
    }


    // Mask
    private static readonly delegate* unmanaged[Cdecl]<nint, nint> s_vello_mask_new_alpha = (delegate* unmanaged[Cdecl]<nint, nint>)NativeLibraryLoader.GetExport("vello_mask_new_alpha");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe nint Mask_NewAlpha(
        nint pixmap
    )
    {
        return s_vello_mask_new_alpha(pixmap);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, nint> s_vello_mask_new_luminance = (delegate* unmanaged[Cdecl]<nint, nint>)NativeLibraryLoader.GetExport("vello_mask_new_luminance");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe nint Mask_NewLuminance(
        nint pixmap
    )
    {
        return s_vello_mask_new_luminance(pixmap);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, void> s_vello_mask_free = (delegate* unmanaged[Cdecl]<nint, void>)NativeLibraryLoader.GetExport("vello_mask_free");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void Mask_Free(
        nint mask
    )
    {
        s_vello_mask_free(mask);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, ushort> s_vello_mask_get_width = (delegate* unmanaged[Cdecl]<nint, ushort>)NativeLibraryLoader.GetExport("vello_mask_get_width");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe ushort Mask_GetWidth(
        nint mask
    )
    {
        return s_vello_mask_get_width(mask);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, ushort> s_vello_mask_get_height = (delegate* unmanaged[Cdecl]<nint, ushort>)NativeLibraryLoader.GetExport("vello_mask_get_height");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe ushort Mask_GetHeight(
        nint mask
    )
    {
        return s_vello_mask_get_height(mask);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, nint, int> s_vello_render_context_push_mask_layer = (delegate* unmanaged[Cdecl]<nint, nint, int>)NativeLibraryLoader.GetExport("vello_render_context_push_mask_layer");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int RenderContext_PushMaskLayer(
        nint ctx,
        nint mask
    )
    {
        return s_vello_render_context_push_mask_layer(ctx, mask);
    }


    // Image
    private static readonly delegate* unmanaged[Cdecl]<nint, VelloExtend, VelloExtend, VelloImageQuality, float, nint> s_vello_image_new_from_pixmap = (delegate* unmanaged[Cdecl]<nint, VelloExtend, VelloExtend, VelloImageQuality, float, nint>)NativeLibraryLoader.GetExport("vello_image_new_from_pixmap");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe nint Image_NewFromPixmap(
        nint pixmap,
        VelloExtend xExtend,
        VelloExtend yExtend,
        VelloImageQuality quality,
        float alpha
    )
    {
        return s_vello_image_new_from_pixmap(pixmap, xExtend, yExtend, quality, alpha);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, void> s_vello_image_free = (delegate* unmanaged[Cdecl]<nint, void>)NativeLibraryLoader.GetExport("vello_image_free");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void Image_Free(
        nint image
    )
    {
        s_vello_image_free(image);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, nint, int> s_vello_render_context_set_paint_image = (delegate* unmanaged[Cdecl]<nint, nint, int>)NativeLibraryLoader.GetExport("vello_render_context_set_paint_image");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int RenderContext_SetPaintImage(
        nint ctx,
        nint image
    )
    {
        return s_vello_render_context_set_paint_image(ctx, image);
    }


    // Recording
    private static readonly delegate* unmanaged[Cdecl]<nint> s_vello_recording_new = (delegate* unmanaged[Cdecl]<nint>)NativeLibraryLoader.GetExport("vello_recording_new");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe nint Recording_New()
    {
        return s_vello_recording_new();
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, void> s_vello_recording_free = (delegate* unmanaged[Cdecl]<nint, void>)NativeLibraryLoader.GetExport("vello_recording_free");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void Recording_Free(
        nint recording
    )
    {
        s_vello_recording_free(recording);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, int> s_vello_recording_clear = (delegate* unmanaged[Cdecl]<nint, int>)NativeLibraryLoader.GetExport("vello_recording_clear");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int Recording_Clear(
        nint recording
    )
    {
        return s_vello_recording_clear(recording);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, nuint> s_vello_recording_len = (delegate* unmanaged[Cdecl]<nint, nuint>)NativeLibraryLoader.GetExport("vello_recording_len");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe nuint Recording_Len(
        nint recording
    )
    {
        return s_vello_recording_len(recording);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, int> s_vello_recording_has_cached_strips = (delegate* unmanaged[Cdecl]<nint, int>)NativeLibraryLoader.GetExport("vello_recording_has_cached_strips");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int Recording_HasCachedStrips(
        nint recording
    )
    {
        return s_vello_recording_has_cached_strips(recording);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, nuint> s_vello_recording_strip_count = (delegate* unmanaged[Cdecl]<nint, nuint>)NativeLibraryLoader.GetExport("vello_recording_strip_count");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe nuint Recording_StripCount(
        nint recording
    )
    {
        return s_vello_recording_strip_count(recording);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, nuint> s_vello_recording_alpha_count = (delegate* unmanaged[Cdecl]<nint, nuint>)NativeLibraryLoader.GetExport("vello_recording_alpha_count");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe nuint Recording_AlphaCount(
        nint recording
    )
    {
        return s_vello_recording_alpha_count(recording);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, nint, nint, nint, int> s_vello_render_context_record = (delegate* unmanaged[Cdecl]<nint, nint, nint, nint, int>)NativeLibraryLoader.GetExport("vello_render_context_record");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int RenderContext_Record(
        nint ctx,
        nint recording,
        nint callback,
        nint userData
    )
    {
        return s_vello_render_context_record(ctx, recording, callback, userData);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, nint, int> s_vello_render_context_prepare_recording = (delegate* unmanaged[Cdecl]<nint, nint, int>)NativeLibraryLoader.GetExport("vello_render_context_prepare_recording");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int RenderContext_PrepareRecording(
        nint ctx,
        nint recording
    )
    {
        return s_vello_render_context_prepare_recording(ctx, recording);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, nint, int> s_vello_render_context_execute_recording = (delegate* unmanaged[Cdecl]<nint, nint, int>)NativeLibraryLoader.GetExport("vello_render_context_execute_recording");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int RenderContext_ExecuteRecording(
        nint ctx,
        nint recording
    )
    {
        return s_vello_render_context_execute_recording(ctx, recording);
    }


    // Recorder methods
    private static readonly delegate* unmanaged[Cdecl]<nint, VelloRect*, int> s_vello_recorder_fill_rect = (delegate* unmanaged[Cdecl]<nint, VelloRect*, int>)NativeLibraryLoader.GetExport("vello_recorder_fill_rect");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int Recorder_FillRect(
        nint recorder,
        VelloRect* rect
    )
    {
        return s_vello_recorder_fill_rect(recorder, rect);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, VelloRect*, int> s_vello_recorder_stroke_rect = (delegate* unmanaged[Cdecl]<nint, VelloRect*, int>)NativeLibraryLoader.GetExport("vello_recorder_stroke_rect");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int Recorder_StrokeRect(
        nint recorder,
        VelloRect* rect
    )
    {
        return s_vello_recorder_stroke_rect(recorder, rect);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, nint, int> s_vello_recorder_fill_path = (delegate* unmanaged[Cdecl]<nint, nint, int>)NativeLibraryLoader.GetExport("vello_recorder_fill_path");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int Recorder_FillPath(
        nint recorder,
        nint path
    )
    {
        return s_vello_recorder_fill_path(recorder, path);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, nint, int> s_vello_recorder_stroke_path = (delegate* unmanaged[Cdecl]<nint, nint, int>)NativeLibraryLoader.GetExport("vello_recorder_stroke_path");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int Recorder_StrokePath(
        nint recorder,
        nint path
    )
    {
        return s_vello_recorder_stroke_path(recorder, path);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, byte, byte, byte, byte, int> s_vello_recorder_set_paint_solid = (delegate* unmanaged[Cdecl]<nint, byte, byte, byte, byte, int>)NativeLibraryLoader.GetExport("vello_recorder_set_paint_solid");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int Recorder_SetPaintSolid(
        nint recorder,
        byte r,
        byte g,
        byte b,
        byte a
    )
    {
        return s_vello_recorder_set_paint_solid(recorder, r, g, b, a);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, VelloStroke*, int> s_vello_recorder_set_stroke = (delegate* unmanaged[Cdecl]<nint, VelloStroke*, int>)NativeLibraryLoader.GetExport("vello_recorder_set_stroke");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int Recorder_SetStroke(
        nint recorder,
        VelloStroke* stroke
    )
    {
        return s_vello_recorder_set_stroke(recorder, stroke);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, VelloAffine*, int> s_vello_recorder_set_transform = (delegate* unmanaged[Cdecl]<nint, VelloAffine*, int>)NativeLibraryLoader.GetExport("vello_recorder_set_transform");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int Recorder_SetTransform(
        nint recorder,
        VelloAffine* transform
    )
    {
        return s_vello_recorder_set_transform(recorder, transform);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, VelloFillRule, int> s_vello_recorder_set_fill_rule = (delegate* unmanaged[Cdecl]<nint, VelloFillRule, int>)NativeLibraryLoader.GetExport("vello_recorder_set_fill_rule");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int Recorder_SetFillRule(
        nint recorder,
        VelloFillRule fillRule
    )
    {
        return s_vello_recorder_set_fill_rule(recorder, fillRule);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, VelloAffine*, int> s_vello_recorder_set_paint_transform = (delegate* unmanaged[Cdecl]<nint, VelloAffine*, int>)NativeLibraryLoader.GetExport("vello_recorder_set_paint_transform");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int Recorder_SetPaintTransform(
        nint recorder,
        VelloAffine* transform
    )
    {
        return s_vello_recorder_set_paint_transform(recorder, transform);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, int> s_vello_recorder_reset_paint_transform = (delegate* unmanaged[Cdecl]<nint, int>)NativeLibraryLoader.GetExport("vello_recorder_reset_paint_transform");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int Recorder_ResetPaintTransform(
        nint recorder
    )
    {
        return s_vello_recorder_reset_paint_transform(recorder);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, nint, int> s_vello_recorder_push_clip_layer = (delegate* unmanaged[Cdecl]<nint, nint, int>)NativeLibraryLoader.GetExport("vello_recorder_push_clip_layer");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int Recorder_PushClipLayer(
        nint recorder,
        nint clipPath
    )
    {
        return s_vello_recorder_push_clip_layer(recorder, clipPath);
    }


    private static readonly delegate* unmanaged[Cdecl]<nint, int> s_vello_recorder_pop_layer = (delegate* unmanaged[Cdecl]<nint, int>)NativeLibraryLoader.GetExport("vello_recorder_pop_layer");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int Recorder_PopLayer(
        nint recorder
    )
    {
        return s_vello_recorder_pop_layer(recorder);
    }

}
