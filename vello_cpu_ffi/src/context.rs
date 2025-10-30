// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

//! RenderContext FFI bindings

use std::os::raw::c_int;

use vello_cpu::RenderContext;

use crate::error::set_last_error;
use crate::types::*;
use crate::{ffi_catch, ffi_catch_ptr};

/// Create new render context with default settings
#[no_mangle]
pub extern "C" fn vello_render_context_new(width: u16, height: u16) -> *mut VelloRenderContext {
    ffi_catch_ptr!({
        let ctx = RenderContext::new(width, height);
        Box::into_raw(Box::new(ctx)) as *mut VelloRenderContext
    })
}

/// Create new render context with custom settings
#[no_mangle]
pub extern "C" fn vello_render_context_new_with(
    width: u16,
    height: u16,
    settings: *const VelloRenderSettings,
) -> *mut VelloRenderContext {
    if settings.is_null() {
        set_last_error("Null settings pointer");
        return std::ptr::null_mut();
    }

    ffi_catch_ptr!({
        let settings = unsafe { &*settings };
        let render_settings = vello_cpu::RenderSettings {
            level: settings.level.to_vello_level(),
            num_threads: settings.num_threads,
            render_mode: settings.render_mode.into(),
        };
        let ctx = RenderContext::new_with(width, height, render_settings);
        Box::into_raw(Box::new(ctx)) as *mut VelloRenderContext
    })
}

/// Free render context
#[no_mangle]
pub extern "C" fn vello_render_context_free(ctx: *mut VelloRenderContext) {
    if !ctx.is_null() {
        unsafe {
            drop(Box::from_raw(ctx as *mut RenderContext));
        }
    }
}

/// Get width
#[no_mangle]
pub extern "C" fn vello_render_context_width(ctx: *const VelloRenderContext) -> u16 {
    if ctx.is_null() {
        return 0;
    }
    unsafe {
        let ctx = &*(ctx as *const RenderContext);
        ctx.width()
    }
}

/// Get height
#[no_mangle]
pub extern "C" fn vello_render_context_height(ctx: *const VelloRenderContext) -> u16 {
    if ctx.is_null() {
        return 0;
    }
    unsafe {
        let ctx = &*(ctx as *const RenderContext);
        ctx.height()
    }
}

/// Reset to initial state
#[no_mangle]
pub extern "C" fn vello_render_context_reset(ctx: *mut VelloRenderContext) -> c_int {
    if ctx.is_null() {
        set_last_error("Null context pointer");
        return VELLO_ERROR_NULL_POINTER;
    }

    ffi_catch!({
        let ctx = unsafe { &mut *(ctx as *mut RenderContext) };
        ctx.reset();
        VELLO_OK
    })
}

/// Set solid color paint (non-premultiplied RGBA)
#[no_mangle]
pub extern "C" fn vello_render_context_set_paint_solid(
    ctx: *mut VelloRenderContext,
    r: u8,
    g: u8,
    b: u8,
    a: u8,
) -> c_int {
    if ctx.is_null() {
        set_last_error("Null context pointer");
        return VELLO_ERROR_NULL_POINTER;
    }

    ffi_catch!({
        let ctx = unsafe { &mut *(ctx as *mut RenderContext) };

        // Convert u8 RGBA values to AlphaColor<Srgb>
        use vello_cpu::peniko::color::{AlphaColor, Srgb};

        // Create color from RGBA u8 values
        let color = AlphaColor::<Srgb>::from_rgba8(r, g, b, a);

        ctx.set_paint(color);
        VELLO_OK
    })
}

/// Set paint to linear gradient
#[no_mangle]
pub extern "C" fn vello_render_context_set_paint_linear_gradient(
    ctx: *mut VelloRenderContext,
    x0: f64,
    y0: f64,
    x1: f64,
    y1: f64,
    stops: *const VelloColorStop,
    stop_count: usize,
    extend: VelloExtend,
) -> c_int {
    if ctx.is_null() || (stop_count > 0 && stops.is_null()) {
        set_last_error("Null pointer");
        return VELLO_ERROR_NULL_POINTER;
    }

    if stop_count < 2 {
        set_last_error("Gradient requires at least 2 color stops");
        return VELLO_ERROR_INVALID_PARAMETER;
    }

    ffi_catch!({
        let ctx = unsafe { &mut *(ctx as *mut RenderContext) };
        let stops_slice = unsafe { std::slice::from_raw_parts(stops, stop_count) };

        // Convert color stops to peniko format
        use vello_cpu::peniko::{ColorStop, Extend, Gradient};
        use vello_cpu::peniko::color::{AlphaColor, Srgb};
        use vello_cpu::kurbo::Point;

        let mut color_stops = Vec::with_capacity(stop_count);
        for stop in stops_slice {
            let color = AlphaColor::<Srgb>::from_rgba8(stop.r, stop.g, stop.b, stop.a);
            color_stops.push(ColorStop {
                offset: stop.offset,
                color: color.into(),
            });
        }

        let gradient = Gradient::new_linear(Point::new(x0, y0), Point::new(x1, y1))
            .with_stops(&color_stops[..])
            .with_extend(match extend {
                VelloExtend::Pad => Extend::Pad,
                VelloExtend::Repeat => Extend::Repeat,
                VelloExtend::Reflect => Extend::Reflect,
            });

        ctx.set_paint(gradient);
        VELLO_OK
    })
}

/// Set paint to radial gradient
#[no_mangle]
pub extern "C" fn vello_render_context_set_paint_radial_gradient(
    ctx: *mut VelloRenderContext,
    cx: f64,
    cy: f64,
    radius: f64,
    stops: *const VelloColorStop,
    stop_count: usize,
    extend: VelloExtend,
) -> c_int {
    if ctx.is_null() || (stop_count > 0 && stops.is_null()) {
        set_last_error("Null pointer");
        return VELLO_ERROR_NULL_POINTER;
    }

    if stop_count < 2 {
        set_last_error("Gradient requires at least 2 color stops");
        return VELLO_ERROR_INVALID_PARAMETER;
    }

    ffi_catch!({
        let ctx = unsafe { &mut *(ctx as *mut RenderContext) };
        let stops_slice = unsafe { std::slice::from_raw_parts(stops, stop_count) };

        // Convert color stops to peniko format
        use vello_cpu::peniko::{ColorStop, Extend, Gradient};
        use vello_cpu::peniko::color::{AlphaColor, Srgb};
        use vello_cpu::kurbo::Point;

        let mut color_stops = Vec::with_capacity(stop_count);
        for stop in stops_slice {
            let color = AlphaColor::<Srgb>::from_rgba8(stop.r, stop.g, stop.b, stop.a);
            color_stops.push(ColorStop {
                offset: stop.offset,
                color: color.into(),
            });
        }

        let gradient = Gradient::new_radial(Point::new(cx, cy), radius as f32)
            .with_stops(&color_stops[..])
            .with_extend(match extend {
                VelloExtend::Pad => Extend::Pad,
                VelloExtend::Repeat => Extend::Repeat,
                VelloExtend::Reflect => Extend::Reflect,
            });

        ctx.set_paint(gradient);
        VELLO_OK
    })
}

/// Set paint to sweep gradient
#[no_mangle]
pub extern "C" fn vello_render_context_set_paint_sweep_gradient(
    ctx: *mut VelloRenderContext,
    cx: f64,
    cy: f64,
    start_angle: f32,
    end_angle: f32,
    stops: *const VelloColorStop,
    stop_count: usize,
    extend: VelloExtend,
) -> c_int {
    if ctx.is_null() || (stop_count > 0 && stops.is_null()) {
        set_last_error("Null pointer");
        return VELLO_ERROR_NULL_POINTER;
    }

    if stop_count < 2 {
        set_last_error("Gradient requires at least 2 color stops");
        return VELLO_ERROR_INVALID_PARAMETER;
    }

    ffi_catch!({
        let ctx = unsafe { &mut *(ctx as *mut RenderContext) };
        let stops_slice = unsafe { std::slice::from_raw_parts(stops, stop_count) };

        // Convert color stops to peniko format
        use vello_cpu::peniko::{ColorStop, Extend, Gradient};
        use vello_cpu::peniko::color::{AlphaColor, Srgb};
        use vello_cpu::kurbo::Point;

        let mut color_stops = Vec::with_capacity(stop_count);
        for stop in stops_slice {
            let color = AlphaColor::<Srgb>::from_rgba8(stop.r, stop.g, stop.b, stop.a);
            color_stops.push(ColorStop {
                offset: stop.offset,
                color: color.into(),
            });
        }

        let gradient = Gradient::new_sweep(Point::new(cx, cy), start_angle, end_angle)
            .with_stops(&color_stops[..])
            .with_extend(match extend {
                VelloExtend::Pad => Extend::Pad,
                VelloExtend::Repeat => Extend::Repeat,
                VelloExtend::Reflect => Extend::Reflect,
            });

        ctx.set_paint(gradient);
        VELLO_OK
    })
}

/// Set transform
#[no_mangle]
pub extern "C" fn vello_render_context_set_transform(
    ctx: *mut VelloRenderContext,
    transform: *const VelloAffine,
) -> c_int {
    if ctx.is_null() || transform.is_null() {
        set_last_error("Null pointer");
        return VELLO_ERROR_NULL_POINTER;
    }

    ffi_catch!({
        let ctx = unsafe { &mut *(ctx as *mut RenderContext) };
        let t = unsafe { &*transform };
        let affine = vello_cpu::kurbo::Affine::new([t.m11, t.m12, t.m21, t.m22, t.m13, t.m23]);
        ctx.set_transform(affine);
        VELLO_OK
    })
}

/// Reset transform to identity
#[no_mangle]
pub extern "C" fn vello_render_context_reset_transform(ctx: *mut VelloRenderContext) -> c_int {
    if ctx.is_null() {
        set_last_error("Null context pointer");
        return VELLO_ERROR_NULL_POINTER;
    }

    ffi_catch!({
        let ctx = unsafe { &mut *(ctx as *mut RenderContext) };
        ctx.reset_transform();
        VELLO_OK
    })
}

/// Get current transform
#[no_mangle]
pub extern "C" fn vello_render_context_get_transform(
    ctx: *const VelloRenderContext,
    out_transform: *mut VelloAffine,
) -> c_int {
    if ctx.is_null() || out_transform.is_null() {
        set_last_error("Null pointer");
        return VELLO_ERROR_NULL_POINTER;
    }

    ffi_catch!({
        let ctx = unsafe { &*(ctx as *const RenderContext) };
        let transform = ctx.transform();
        let coeffs = transform.as_coeffs();
        let out = unsafe { &mut *out_transform };
        out.m11 = coeffs[0];
        out.m12 = coeffs[1];
        out.m21 = coeffs[2];
        out.m22 = coeffs[3];
        out.m13 = coeffs[4];
        out.m23 = coeffs[5];
        VELLO_OK
    })
}

/// Set stroke parameters
#[no_mangle]
pub extern "C" fn vello_render_context_set_stroke(
    ctx: *mut VelloRenderContext,
    stroke: *const VelloStroke,
) -> c_int {
    if ctx.is_null() || stroke.is_null() {
        set_last_error("Null pointer");
        return VELLO_ERROR_NULL_POINTER;
    }

    ffi_catch!({
        let ctx = unsafe { &mut *(ctx as *mut RenderContext) };
        let s = unsafe { &*stroke };

        let join = match s.join {
            VelloJoin::Bevel => vello_cpu::kurbo::Join::Bevel,
            VelloJoin::Miter => vello_cpu::kurbo::Join::Miter,
            VelloJoin::Round => vello_cpu::kurbo::Join::Round,
        };

        let start_cap = match s.start_cap {
            VelloCap::Butt => vello_cpu::kurbo::Cap::Butt,
            VelloCap::Square => vello_cpu::kurbo::Cap::Square,
            VelloCap::Round => vello_cpu::kurbo::Cap::Round,
        };

        let end_cap = match s.end_cap {
            VelloCap::Butt => vello_cpu::kurbo::Cap::Butt,
            VelloCap::Square => vello_cpu::kurbo::Cap::Square,
            VelloCap::Round => vello_cpu::kurbo::Cap::Round,
        };

        let stroke = vello_cpu::kurbo::Stroke {
            width: s.width as f64,
            join,
            start_cap,
            end_cap,
            miter_limit: s.miter_limit as f64,
            ..Default::default()
        };

        ctx.set_stroke(stroke);
        VELLO_OK
    })
}

/// Set fill rule
#[no_mangle]
pub extern "C" fn vello_render_context_set_fill_rule(
    ctx: *mut VelloRenderContext,
    fill_rule: VelloFillRule,
) -> c_int {
    if ctx.is_null() {
        set_last_error("Null context pointer");
        return VELLO_ERROR_NULL_POINTER;
    }

    ffi_catch!({
        let ctx = unsafe { &mut *(ctx as *mut RenderContext) };
        let rule = match fill_rule {
            VelloFillRule::NonZero => vello_cpu::peniko::Fill::NonZero,
            VelloFillRule::EvenOdd => vello_cpu::peniko::Fill::EvenOdd,
        };
        ctx.set_fill_rule(rule);
        VELLO_OK
    })
}

/// Fill rectangle
#[no_mangle]
pub extern "C" fn vello_render_context_fill_rect(
    ctx: *mut VelloRenderContext,
    rect: *const VelloRect,
) -> c_int {
    if ctx.is_null() || rect.is_null() {
        set_last_error("Null pointer");
        return VELLO_ERROR_NULL_POINTER;
    }

    ffi_catch!({
        let ctx = unsafe { &mut *(ctx as *mut RenderContext) };
        let r = unsafe { &*rect };
        let rect = vello_cpu::kurbo::Rect::new(r.x0, r.y0, r.x1, r.y1);
        ctx.fill_rect(&rect);
        VELLO_OK
    })
}

/// Stroke rectangle
#[no_mangle]
pub extern "C" fn vello_render_context_stroke_rect(
    ctx: *mut VelloRenderContext,
    rect: *const VelloRect,
) -> c_int {
    if ctx.is_null() || rect.is_null() {
        set_last_error("Null pointer");
        return VELLO_ERROR_NULL_POINTER;
    }

    ffi_catch!({
        let ctx = unsafe { &mut *(ctx as *mut RenderContext) };
        let r = unsafe { &*rect };
        let rect = vello_cpu::kurbo::Rect::new(r.x0, r.y0, r.x1, r.y1);
        ctx.stroke_rect(&rect);
        VELLO_OK
    })
}

/// Fill a blurred rounded rectangle
#[no_mangle]
pub extern "C" fn vello_render_context_fill_blurred_rounded_rect(
    ctx: *mut VelloRenderContext,
    rect: *const VelloRect,
    radius: f32,
    std_dev: f32,
) -> c_int {
    if ctx.is_null() || rect.is_null() {
        set_last_error("Null pointer");
        return VELLO_ERROR_NULL_POINTER;
    }

    ffi_catch!({
        let ctx = unsafe { &mut *(ctx as *mut RenderContext) };
        let r = unsafe { &*rect };
        let rect = vello_cpu::kurbo::Rect::new(r.x0, r.y0, r.x1, r.y1);
        ctx.fill_blurred_rounded_rect(&rect, radius, std_dev);
        VELLO_OK
    })
}

/// Push a blend layer with specified blend mode
#[no_mangle]
pub extern "C" fn vello_render_context_push_blend_layer(
    ctx: *mut VelloRenderContext,
    blend_mode: *const VelloBlendMode,
) -> c_int {
    if ctx.is_null() || blend_mode.is_null() {
        set_last_error("Null pointer");
        return VELLO_ERROR_NULL_POINTER;
    }

    ffi_catch!({
        let ctx = unsafe { &mut *(ctx as *mut RenderContext) };
        let bm = unsafe { &*blend_mode };

        use vello_cpu::peniko::{BlendMode, Compose, Mix};

        let mix = match bm.mix {
            VelloMix::Normal => Mix::Normal,
            VelloMix::Multiply => Mix::Multiply,
            VelloMix::Screen => Mix::Screen,
            VelloMix::Overlay => Mix::Overlay,
            VelloMix::Darken => Mix::Darken,
            VelloMix::Lighten => Mix::Lighten,
            VelloMix::ColorDodge => Mix::ColorDodge,
            VelloMix::ColorBurn => Mix::ColorBurn,
            VelloMix::HardLight => Mix::HardLight,
            VelloMix::SoftLight => Mix::SoftLight,
            VelloMix::Difference => Mix::Difference,
            VelloMix::Exclusion => Mix::Exclusion,
            VelloMix::Hue => Mix::Hue,
            VelloMix::Saturation => Mix::Saturation,
            VelloMix::Color => Mix::Color,
            VelloMix::Luminosity => Mix::Luminosity,
        };

        let compose = match bm.compose {
            VelloCompose::Clear => Compose::Clear,
            VelloCompose::Copy => Compose::Copy,
            VelloCompose::Dest => Compose::Dest,
            VelloCompose::SrcOver => Compose::SrcOver,
            VelloCompose::DestOver => Compose::DestOver,
            VelloCompose::SrcIn => Compose::SrcIn,
            VelloCompose::DestIn => Compose::DestIn,
            VelloCompose::SrcOut => Compose::SrcOut,
            VelloCompose::DestOut => Compose::DestOut,
            VelloCompose::SrcAtop => Compose::SrcAtop,
            VelloCompose::DestAtop => Compose::DestAtop,
            VelloCompose::Xor => Compose::Xor,
            VelloCompose::Plus => Compose::Plus,
            VelloCompose::PlusLighter => Compose::PlusLighter,
        };

        let blend_mode = BlendMode::new(mix, compose);
        ctx.push_blend_layer(blend_mode);
        VELLO_OK
    })
}

/// Push a clip layer with specified path
#[no_mangle]
pub extern "C" fn vello_render_context_push_clip_layer(
    ctx: *mut VelloRenderContext,
    path: *const VelloBezPath,
) -> c_int {
    if ctx.is_null() || path.is_null() {
        set_last_error("Null pointer");
        return VELLO_ERROR_NULL_POINTER;
    }

    ffi_catch!({
        let ctx = unsafe { &mut *(ctx as *mut RenderContext) };
        let path = unsafe { &*(path as *const vello_cpu::kurbo::BezPath) };
        ctx.push_clip_layer(path);
        VELLO_OK
    })
}

/// Push an opacity layer with specified opacity
#[no_mangle]
pub extern "C" fn vello_render_context_push_opacity_layer(
    ctx: *mut VelloRenderContext,
    opacity: f32,
) -> c_int {
    if ctx.is_null() {
        set_last_error("Null context pointer");
        return VELLO_ERROR_NULL_POINTER;
    }

    ffi_catch!({
        let ctx = unsafe { &mut *(ctx as *mut RenderContext) };
        ctx.push_opacity_layer(opacity);
        VELLO_OK
    })
}

/// Pop current layer (blend/clip/mask)
#[no_mangle]
pub extern "C" fn vello_render_context_pop_layer(ctx: *mut VelloRenderContext) -> c_int {
    if ctx.is_null() {
        set_last_error("Null context pointer");
        return VELLO_ERROR_NULL_POINTER;
    }

    ffi_catch!({
        let ctx = unsafe { &mut *(ctx as *mut RenderContext) };
        ctx.pop_layer();
        VELLO_OK
    })
}

/// Flush rendering (required for multithreading)
#[no_mangle]
pub extern "C" fn vello_render_context_flush(ctx: *mut VelloRenderContext) -> c_int {
    if ctx.is_null() {
        set_last_error("Null context pointer");
        return VELLO_ERROR_NULL_POINTER;
    }

    ffi_catch!({
        let ctx = unsafe { &mut *(ctx as *mut RenderContext) };
        ctx.flush();
        VELLO_OK
    })
}

/// Get current stroke
#[no_mangle]
pub extern "C" fn vello_render_context_get_stroke(
    ctx: *const VelloRenderContext,
    out_stroke: *mut VelloStroke,
) -> c_int {
    if ctx.is_null() || out_stroke.is_null() {
        set_last_error("Null pointer");
        return VELLO_ERROR_NULL_POINTER;
    }

    ffi_catch!({
        let ctx = unsafe { &*(ctx as *const RenderContext) };
        let stroke = ctx.stroke();
        let out = unsafe { &mut *out_stroke };

        out.width = stroke.width as f32;
        out.miter_limit = stroke.miter_limit as f32;

        out.join = match stroke.join {
            vello_cpu::kurbo::Join::Bevel => VelloJoin::Bevel,
            vello_cpu::kurbo::Join::Miter => VelloJoin::Miter,
            vello_cpu::kurbo::Join::Round => VelloJoin::Round,
        };

        out.start_cap = match stroke.start_cap {
            vello_cpu::kurbo::Cap::Butt => VelloCap::Butt,
            vello_cpu::kurbo::Cap::Square => VelloCap::Square,
            vello_cpu::kurbo::Cap::Round => VelloCap::Round,
        };

        out.end_cap = match stroke.end_cap {
            vello_cpu::kurbo::Cap::Butt => VelloCap::Butt,
            vello_cpu::kurbo::Cap::Square => VelloCap::Square,
            vello_cpu::kurbo::Cap::Round => VelloCap::Round,
        };

        VELLO_OK
    })
}

/// Get current fill rule
#[no_mangle]
pub extern "C" fn vello_render_context_get_fill_rule(
    ctx: *const VelloRenderContext,
) -> VelloFillRule {
    if ctx.is_null() {
        return VelloFillRule::NonZero; // Default
    }

    let ctx = unsafe { &*(ctx as *const RenderContext) };
    let fill_rule = ctx.fill_rule();
    match fill_rule {
        vello_cpu::peniko::Fill::NonZero => VelloFillRule::NonZero,
        vello_cpu::peniko::Fill::EvenOdd => VelloFillRule::EvenOdd,
    }
}

/// Set paint transform
#[no_mangle]
pub extern "C" fn vello_render_context_set_paint_transform(
    ctx: *mut VelloRenderContext,
    transform: *const VelloAffine,
) -> c_int {
    if ctx.is_null() || transform.is_null() {
        set_last_error("Null pointer");
        return VELLO_ERROR_NULL_POINTER;
    }

    ffi_catch!({
        let ctx = unsafe { &mut *(ctx as *mut RenderContext) };
        let t = unsafe { &*transform };
        let affine = vello_cpu::kurbo::Affine::new([t.m11, t.m12, t.m21, t.m22, t.m13, t.m23]);
        ctx.set_paint_transform(affine);
        VELLO_OK
    })
}

/// Get current paint transform
#[no_mangle]
pub extern "C" fn vello_render_context_get_paint_transform(
    ctx: *const VelloRenderContext,
    out_transform: *mut VelloAffine,
) -> c_int {
    if ctx.is_null() || out_transform.is_null() {
        set_last_error("Null pointer");
        return VELLO_ERROR_NULL_POINTER;
    }

    ffi_catch!({
        let ctx = unsafe { &*(ctx as *const RenderContext) };
        let transform = ctx.paint_transform();
        let coeffs = transform.as_coeffs();
        let out = unsafe { &mut *out_transform };
        out.m11 = coeffs[0];
        out.m12 = coeffs[1];
        out.m21 = coeffs[2];
        out.m22 = coeffs[3];
        out.m13 = coeffs[4];
        out.m23 = coeffs[5];
        VELLO_OK
    })
}

/// Reset paint transform to identity
#[no_mangle]
pub extern "C" fn vello_render_context_reset_paint_transform(ctx: *mut VelloRenderContext) -> c_int {
    if ctx.is_null() {
        set_last_error("Null context pointer");
        return VELLO_ERROR_NULL_POINTER;
    }

    ffi_catch!({
        let ctx = unsafe { &mut *(ctx as *mut RenderContext) };
        ctx.reset_paint_transform();
        VELLO_OK
    })
}

/// Set anti-aliasing threshold (0-255, or negative to use default)
#[no_mangle]
pub extern "C" fn vello_render_context_set_aliasing_threshold(
    ctx: *mut VelloRenderContext,
    threshold: i16,
) -> c_int {
    if ctx.is_null() {
        set_last_error("Null context pointer");
        return VELLO_ERROR_NULL_POINTER;
    }

    ffi_catch!({
        let ctx = unsafe { &mut *(ctx as *mut RenderContext) };
        let threshold_opt = if threshold < 0 {
            None
        } else {
            Some(threshold.clamp(0, 255) as u8)
        };
        ctx.set_aliasing_threshold(threshold_opt);
        VELLO_OK
    })
}

/// Push a general layer with full control over clip path, blend mode, opacity, and mask
/// Pass null pointers for optional parameters
#[no_mangle]
pub extern "C" fn vello_render_context_push_layer(
    ctx: *mut VelloRenderContext,
    clip_path: *const VelloBezPath,
    blend_mode: *const VelloBlendMode,
    opacity: f32,
    mask: *const VelloMask,
) -> c_int {
    if ctx.is_null() {
        set_last_error("Null context pointer");
        return VELLO_ERROR_NULL_POINTER;
    }

    ffi_catch!({
        let ctx = unsafe { &mut *(ctx as *mut RenderContext) };

        let clip_path_opt = if clip_path.is_null() {
            None
        } else {
            let path = unsafe { &*(clip_path as *const vello_cpu::kurbo::BezPath) };
            Some(path)
        };

        let blend_mode_opt = if blend_mode.is_null() {
            None
        } else {
            let bm = unsafe { &*blend_mode };

            use vello_cpu::peniko::{BlendMode, Mix, Compose};

            let mix = match bm.mix {
                VelloMix::Normal => Mix::Normal,
                VelloMix::Multiply => Mix::Multiply,
                VelloMix::Screen => Mix::Screen,
                VelloMix::Overlay => Mix::Overlay,
                VelloMix::Darken => Mix::Darken,
                VelloMix::Lighten => Mix::Lighten,
                VelloMix::ColorDodge => Mix::ColorDodge,
                VelloMix::ColorBurn => Mix::ColorBurn,
                VelloMix::HardLight => Mix::HardLight,
                VelloMix::SoftLight => Mix::SoftLight,
                VelloMix::Difference => Mix::Difference,
                VelloMix::Exclusion => Mix::Exclusion,
                VelloMix::Hue => Mix::Hue,
                VelloMix::Saturation => Mix::Saturation,
                VelloMix::Color => Mix::Color,
                VelloMix::Luminosity => Mix::Luminosity,
            };

            let compose = match bm.compose {
                VelloCompose::Clear => Compose::Clear,
                VelloCompose::Copy => Compose::Copy,
                VelloCompose::Dest => Compose::Dest,
                VelloCompose::SrcOver => Compose::SrcOver,
                VelloCompose::DestOver => Compose::DestOver,
                VelloCompose::SrcIn => Compose::SrcIn,
                VelloCompose::DestIn => Compose::DestIn,
                VelloCompose::SrcOut => Compose::SrcOut,
                VelloCompose::DestOut => Compose::DestOut,
                VelloCompose::SrcAtop => Compose::SrcAtop,
                VelloCompose::DestAtop => Compose::DestAtop,
                VelloCompose::Xor => Compose::Xor,
                VelloCompose::Plus => Compose::Plus,
                VelloCompose::PlusLighter => Compose::PlusLighter,
            };

            Some(BlendMode::new(mix, compose))
        };

        let opacity_opt = if opacity < 0.0 {
            None
        } else {
            Some(opacity)
        };

        let mask_opt = if mask.is_null() {
            None
        } else {
            let m = unsafe { &*(mask as *const vello_cpu::Mask) };
            Some(m.clone())
        };

        ctx.push_layer(clip_path_opt, blend_mode_opt, opacity_opt, mask_opt);
        VELLO_OK
    })
}

/// Get render settings
#[no_mangle]
pub extern "C" fn vello_render_context_get_render_settings(
    ctx: *const VelloRenderContext,
    out_settings: *mut VelloRenderSettings,
) -> c_int {
    if ctx.is_null() || out_settings.is_null() {
        set_last_error("Null pointer");
        return VELLO_ERROR_NULL_POINTER;
    }

    ffi_catch!({
        let ctx = unsafe { &*(ctx as *const RenderContext) };
        let settings = ctx.render_settings();
        let out = unsafe { &mut *out_settings };

        out.level = VelloSimdLevel::from_vello_level(settings.level);
        out.num_threads = settings.num_threads;
        out.render_mode = settings.render_mode.into();

        VELLO_OK
    })
}

/// Render to raw RGBA buffer (u8 bytes, premultiplied)
/// Buffer must be at least width * height * 4 bytes
#[no_mangle]
pub extern "C" fn vello_render_context_render_to_buffer(
    ctx: *mut VelloRenderContext,
    buffer: *mut u8,
    buffer_len: usize,
    width: u16,
    height: u16,
    render_mode: VelloRenderMode,
) -> c_int {
    if ctx.is_null() || buffer.is_null() {
        set_last_error("Null pointer");
        return VELLO_ERROR_NULL_POINTER;
    }

    ffi_catch!({
        let ctx = unsafe { &*(ctx as *const RenderContext) };
        let required_len = (width as usize) * (height as usize) * 4;

        if buffer_len < required_len {
            set_last_error("Buffer too small");
            return VELLO_ERROR_INVALID_PARAMETER;
        }

        let buffer_slice = unsafe {
            std::slice::from_raw_parts_mut(buffer, required_len)
        };

        ctx.render_to_buffer(buffer_slice, width, height, render_mode.into());
        VELLO_OK
    })
}
