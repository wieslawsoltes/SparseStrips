// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Vello.Native;

public static partial class NativeMethods
{
    private const string LibraryName = "vello_cpu_ffi";

    /// <summary>
    /// Module initializer to ensure native library loader is registered before any P/Invoke calls.
    /// Note: With LibraryImport, the module initializer may not be called before P/Invoke resolution,
    /// so we rely on the native library being copied to the output directory root.
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
    [LibraryImport(LibraryName, EntryPoint = "vello_version")]
    public static partial nint Version();

    [LibraryImport(LibraryName, EntryPoint = "vello_simd_detect")]
    public static partial VelloSimdLevel SimdDetect();

    // Error handling
    [LibraryImport(LibraryName, EntryPoint = "vello_get_last_error")]
    public static partial nint GetLastError();

    [LibraryImport(LibraryName, EntryPoint = "vello_clear_last_error")]
    public static partial void ClearLastError();

    // RenderContext
    [LibraryImport(LibraryName, EntryPoint = "vello_render_context_new")]
    public static partial nint RenderContext_New(ushort width, ushort height);

    [LibraryImport(LibraryName, EntryPoint = "vello_render_context_new_with")]
    public static unsafe partial nint RenderContext_NewWith(
        ushort width,
        ushort height,
        VelloRenderSettings* settings);

    [LibraryImport(LibraryName, EntryPoint = "vello_render_context_free")]
    public static partial void RenderContext_Free(nint ctx);

    [LibraryImport(LibraryName, EntryPoint = "vello_render_context_width")]
    public static partial ushort RenderContext_Width(nint ctx);

    [LibraryImport(LibraryName, EntryPoint = "vello_render_context_height")]
    public static partial ushort RenderContext_Height(nint ctx);

    [LibraryImport(LibraryName, EntryPoint = "vello_render_context_reset")]
    public static partial int RenderContext_Reset(nint ctx);

    [LibraryImport(LibraryName, EntryPoint = "vello_render_context_set_paint_solid")]
    public static partial int RenderContext_SetPaintSolid(
        nint ctx,
        byte r,
        byte g,
        byte b,
        byte a);

    [LibraryImport(LibraryName, EntryPoint = "vello_render_context_set_paint_linear_gradient")]
    public static unsafe partial int RenderContext_SetPaintLinearGradient(
        nint ctx,
        double x0,
        double y0,
        double x1,
        double y1,
        VelloColorStop* stops,
        nuint stopCount,
        VelloExtend extend);

    [LibraryImport(LibraryName, EntryPoint = "vello_render_context_set_paint_radial_gradient")]
    public static unsafe partial int RenderContext_SetPaintRadialGradient(
        nint ctx,
        double cx,
        double cy,
        double radius,
        VelloColorStop* stops,
        nuint stopCount,
        VelloExtend extend);

    [LibraryImport(LibraryName, EntryPoint = "vello_render_context_set_paint_sweep_gradient")]
    public static unsafe partial int RenderContext_SetPaintSweepGradient(
        nint ctx,
        double cx,
        double cy,
        float startAngle,
        float endAngle,
        VelloColorStop* stops,
        nuint stopCount,
        VelloExtend extend);

    [LibraryImport(LibraryName, EntryPoint = "vello_render_context_set_transform")]
    public static unsafe partial int RenderContext_SetTransform(
        nint ctx,
        VelloAffine* transform);

    [LibraryImport(LibraryName, EntryPoint = "vello_render_context_reset_transform")]
    public static partial int RenderContext_ResetTransform(nint ctx);

    [LibraryImport(LibraryName, EntryPoint = "vello_render_context_get_transform")]
    public static unsafe partial int RenderContext_GetTransform(
        nint ctx,
        VelloAffine* outTransform);

    [LibraryImport(LibraryName, EntryPoint = "vello_render_context_set_stroke")]
    public static unsafe partial int RenderContext_SetStroke(
        nint ctx,
        VelloStroke* stroke);

    [LibraryImport(LibraryName, EntryPoint = "vello_render_context_set_fill_rule")]
    public static partial int RenderContext_SetFillRule(
        nint ctx,
        VelloFillRule fillRule);

    [LibraryImport(LibraryName, EntryPoint = "vello_render_context_fill_rect")]
    public static unsafe partial int RenderContext_FillRect(
        nint ctx,
        VelloRect* rect);

    [LibraryImport(LibraryName, EntryPoint = "vello_render_context_stroke_rect")]
    public static unsafe partial int RenderContext_StrokeRect(
        nint ctx,
        VelloRect* rect);

    [LibraryImport(LibraryName, EntryPoint = "vello_render_context_fill_blurred_rounded_rect")]
    public static unsafe partial int RenderContext_FillBlurredRoundedRect(
        nint ctx,
        VelloRect* rect,
        float radius,
        float stdDev);

    [LibraryImport(LibraryName, EntryPoint = "vello_render_context_fill_path")]
    public static partial int RenderContext_FillPath(nint ctx, nint path);

    [LibraryImport(LibraryName, EntryPoint = "vello_render_context_stroke_path")]
    public static partial int RenderContext_StrokePath(nint ctx, nint path);

    [LibraryImport(LibraryName, EntryPoint = "vello_render_context_push_blend_layer")]
    public static unsafe partial int RenderContext_PushBlendLayer(
        nint ctx,
        VelloBlendMode* blendMode);

    [LibraryImport(LibraryName, EntryPoint = "vello_render_context_push_clip_layer")]
    public static partial int RenderContext_PushClipLayer(nint ctx, nint path);

    [LibraryImport(LibraryName, EntryPoint = "vello_render_context_push_opacity_layer")]
    public static partial int RenderContext_PushOpacityLayer(nint ctx, float opacity);

    [LibraryImport(LibraryName, EntryPoint = "vello_render_context_pop_layer")]
    public static partial int RenderContext_PopLayer(nint ctx);

    [LibraryImport(LibraryName, EntryPoint = "vello_render_context_flush")]
    public static partial int RenderContext_Flush(nint ctx);

    // Advanced/Optional methods

    [LibraryImport(LibraryName, EntryPoint = "vello_render_context_get_stroke")]
    public static unsafe partial int RenderContext_GetStroke(
        nint ctx,
        VelloStroke* outStroke);

    [LibraryImport(LibraryName, EntryPoint = "vello_render_context_get_fill_rule")]
    public static partial VelloFillRule RenderContext_GetFillRule(nint ctx);

    [LibraryImport(LibraryName, EntryPoint = "vello_render_context_set_paint_transform")]
    public static unsafe partial int RenderContext_SetPaintTransform(
        nint ctx,
        VelloAffine* transform);

    [LibraryImport(LibraryName, EntryPoint = "vello_render_context_get_paint_transform")]
    public static unsafe partial int RenderContext_GetPaintTransform(
        nint ctx,
        VelloAffine* outTransform);

    [LibraryImport(LibraryName, EntryPoint = "vello_render_context_reset_paint_transform")]
    public static partial int RenderContext_ResetPaintTransform(nint ctx);

    [LibraryImport(LibraryName, EntryPoint = "vello_render_context_get_paint_kind")]
    public static partial VelloPaintKind RenderContext_GetPaintKind(nint ctx);

    [LibraryImport(LibraryName, EntryPoint = "vello_render_context_set_aliasing_threshold")]
    public static partial int RenderContext_SetAliasingThreshold(nint ctx, short threshold);

    [LibraryImport(LibraryName, EntryPoint = "vello_render_context_push_layer")]
    public static unsafe partial int RenderContext_PushLayer(
        nint ctx,
        nint clipPath,
        VelloBlendMode* blendMode,
        float opacity,
        nint mask);

    [LibraryImport(LibraryName, EntryPoint = "vello_render_context_get_render_settings")]
    public static unsafe partial int RenderContext_GetRenderSettings(
        nint ctx,
        VelloRenderSettings* outSettings);

    [LibraryImport(LibraryName, EntryPoint = "vello_render_context_render_to_buffer")]
    public static unsafe partial int RenderContext_RenderToBuffer(
        nint ctx,
        byte* buffer,
        nuint bufferLen,
        ushort width,
        ushort height,
        VelloRenderMode renderMode);

    [LibraryImport(LibraryName, EntryPoint = "vello_render_context_render_to_pixmap")]
    public static partial int RenderContext_RenderToPixmap(nint ctx, nint pixmap);

    // Pixmap
    [LibraryImport(LibraryName, EntryPoint = "vello_pixmap_new")]
    public static partial nint Pixmap_New(ushort width, ushort height);

    [LibraryImport(LibraryName, EntryPoint = "vello_pixmap_free")]
    public static partial void Pixmap_Free(nint pixmap);

    [LibraryImport(LibraryName, EntryPoint = "vello_pixmap_width")]
    public static partial ushort Pixmap_Width(nint pixmap);

    [LibraryImport(LibraryName, EntryPoint = "vello_pixmap_height")]
    public static partial ushort Pixmap_Height(nint pixmap);

    [LibraryImport(LibraryName, EntryPoint = "vello_pixmap_data")]
    public static unsafe partial int Pixmap_Data(
        nint pixmap,
        nint* outPtr,
        nuint* outLen);

    [LibraryImport(LibraryName, EntryPoint = "vello_pixmap_data_mut")]
    public static unsafe partial int Pixmap_DataMut(
        nint pixmap,
        nint* outPtr,
        nuint* outLen);

    [LibraryImport(LibraryName, EntryPoint = "vello_pixmap_resize")]
    public static partial int Pixmap_Resize(
        nint pixmap,
        ushort width,
        ushort height);

    [LibraryImport(LibraryName, EntryPoint = "vello_pixmap_from_png")]
    public static unsafe partial nint Pixmap_FromPng(byte* data, nuint len);

    [LibraryImport(LibraryName, EntryPoint = "vello_pixmap_to_png")]
    public static unsafe partial int Pixmap_ToPng(
        nint pixmap,
        byte** outData,
        nuint* outLen);

    [LibraryImport(LibraryName, EntryPoint = "vello_png_data_free")]
    public static unsafe partial void PngDataFree(byte* data, nuint len);

    [LibraryImport(LibraryName, EntryPoint = "vello_pixmap_sample")]
    public static unsafe partial int Pixmap_Sample(
        nint pixmap,
        ushort x,
        ushort y,
        VelloPremulRgba8* outPixel);

    // BezPath
    [LibraryImport(LibraryName, EntryPoint = "vello_bezpath_new")]
    public static partial nint BezPath_New();

    [LibraryImport(LibraryName, EntryPoint = "vello_bezpath_free")]
    public static partial void BezPath_Free(nint path);

    [LibraryImport(LibraryName, EntryPoint = "vello_bezpath_move_to")]
    public static partial int BezPath_MoveTo(nint path, double x, double y);

    [LibraryImport(LibraryName, EntryPoint = "vello_bezpath_line_to")]
    public static partial int BezPath_LineTo(nint path, double x, double y);

    [LibraryImport(LibraryName, EntryPoint = "vello_bezpath_quad_to")]
    public static partial int BezPath_QuadTo(
        nint path,
        double x1,
        double y1,
        double x2,
        double y2);

    [LibraryImport(LibraryName, EntryPoint = "vello_bezpath_curve_to")]
    public static partial int BezPath_CurveTo(
        nint path,
        double x1,
        double y1,
        double x2,
        double y2,
        double x3,
        double y3);

    [LibraryImport(LibraryName, EntryPoint = "vello_bezpath_close")]
    public static partial int BezPath_Close(nint path);

    [LibraryImport(LibraryName, EntryPoint = "vello_bezpath_clear")]
    public static partial int BezPath_Clear(nint path);

    // Text Rendering
    [LibraryImport(LibraryName, EntryPoint = "vello_font_data_new")]
    public static unsafe partial nint FontData_New(byte* data, nuint len, uint index);

    [LibraryImport(LibraryName, EntryPoint = "vello_font_data_free")]
    public static partial void FontData_Free(nint font);

    [LibraryImport(LibraryName, EntryPoint = "vello_render_context_fill_glyphs")]
    public static unsafe partial int RenderContext_FillGlyphs(
        nint ctx,
        nint font,
        float fontSize,
        VelloGlyph* glyphs,
        nuint glyphCount);

    [LibraryImport(LibraryName, EntryPoint = "vello_render_context_stroke_glyphs")]
    public static unsafe partial int RenderContext_StrokeGlyphs(
        nint ctx,
        nint font,
        float fontSize,
        VelloGlyph* glyphs,
        nuint glyphCount);

    [LibraryImport(LibraryName, EntryPoint = "vello_font_data_text_to_glyphs")]
    public static unsafe partial int FontData_TextToGlyphs(
        nint font,
        byte* text,
        VelloGlyph* outGlyphs,
        nuint maxGlyphs,
        nuint* outCount);

    // Mask
    [LibraryImport(LibraryName, EntryPoint = "vello_mask_new_alpha")]
    public static partial nint Mask_NewAlpha(nint pixmap);

    [LibraryImport(LibraryName, EntryPoint = "vello_mask_new_luminance")]
    public static partial nint Mask_NewLuminance(nint pixmap);

    [LibraryImport(LibraryName, EntryPoint = "vello_mask_free")]
    public static partial void Mask_Free(nint mask);

    [LibraryImport(LibraryName, EntryPoint = "vello_mask_get_width")]
    public static partial ushort Mask_GetWidth(nint mask);

    [LibraryImport(LibraryName, EntryPoint = "vello_mask_get_height")]
    public static partial ushort Mask_GetHeight(nint mask);

    [LibraryImport(LibraryName, EntryPoint = "vello_render_context_push_mask_layer")]
    public static partial int RenderContext_PushMaskLayer(nint ctx, nint mask);

    // Image
    [LibraryImport(LibraryName, EntryPoint = "vello_image_new_from_pixmap")]
    public static partial nint Image_NewFromPixmap(
        nint pixmap,
        VelloExtend xExtend,
        VelloExtend yExtend,
        VelloImageQuality quality,
        float alpha);

    [LibraryImport(LibraryName, EntryPoint = "vello_image_free")]
    public static partial void Image_Free(nint image);

    [LibraryImport(LibraryName, EntryPoint = "vello_render_context_set_paint_image")]
    public static partial int RenderContext_SetPaintImage(nint ctx, nint image);

    // Recording
    [LibraryImport(LibraryName, EntryPoint = "vello_recording_new")]
    public static partial nint Recording_New();

    [LibraryImport(LibraryName, EntryPoint = "vello_recording_free")]
    public static partial void Recording_Free(nint recording);

    [LibraryImport(LibraryName, EntryPoint = "vello_recording_clear")]
    public static partial int Recording_Clear(nint recording);

    [LibraryImport(LibraryName, EntryPoint = "vello_recording_len")]
    public static partial nuint Recording_Len(nint recording);

    [LibraryImport(LibraryName, EntryPoint = "vello_render_context_record")]
    public static unsafe partial int RenderContext_Record(
        nint ctx,
        nint recording,
        delegate* unmanaged[Cdecl]<nint, nint, void> callback,
        nint userData);

    [LibraryImport(LibraryName, EntryPoint = "vello_render_context_prepare_recording")]
    public static partial int RenderContext_PrepareRecording(nint ctx, nint recording);

    [LibraryImport(LibraryName, EntryPoint = "vello_render_context_execute_recording")]
    public static partial int RenderContext_ExecuteRecording(nint ctx, nint recording);

    // Recorder methods
    [LibraryImport(LibraryName, EntryPoint = "vello_recorder_fill_rect")]
    public static unsafe partial int Recorder_FillRect(nint recorder, VelloRect* rect);

    [LibraryImport(LibraryName, EntryPoint = "vello_recorder_stroke_rect")]
    public static unsafe partial int Recorder_StrokeRect(nint recorder, VelloRect* rect);

    [LibraryImport(LibraryName, EntryPoint = "vello_recorder_fill_path")]
    public static partial int Recorder_FillPath(nint recorder, nint path);

    [LibraryImport(LibraryName, EntryPoint = "vello_recorder_stroke_path")]
    public static partial int Recorder_StrokePath(nint recorder, nint path);

    [LibraryImport(LibraryName, EntryPoint = "vello_recorder_set_paint_solid")]
    public static partial int Recorder_SetPaintSolid(nint recorder, byte r, byte g, byte b, byte a);
}
