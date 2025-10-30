// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

//! Text rendering FFI functions

use crate::{ffi_catch, ffi_catch_ptr};
use crate::error::set_last_error;
use crate::types::*;
use std::os::raw::c_int;
use vello_cpu::peniko::{FontData, Blob};

/// Opaque handle to FontData
pub type VelloFontData = std::ffi::c_void;

/// Glyph structure for FFI
#[repr(C)]
#[derive(Copy, Clone, Debug)]
pub struct VelloGlyph {
    /// Glyph ID (font-specific, not Unicode)
    pub id: u32,
    /// X offset in pixels
    pub x: f32,
    /// Y offset in pixels
    pub y: f32,
}

/// Create FontData from font file bytes
#[no_mangle]
pub extern "C" fn vello_font_data_new(
    data: *const u8,
    len: usize,
    index: u32,
) -> *mut VelloFontData {
    if data.is_null() || len == 0 {
        set_last_error("Null or empty font data");
        return std::ptr::null_mut();
    }

    ffi_catch_ptr!({
        let slice = unsafe { std::slice::from_raw_parts(data, len) };
        let vec = slice.to_vec();
        let blob = Blob::from(vec);
        let font_data = FontData::new(blob, index);
        Box::into_raw(Box::new(font_data)) as *mut VelloFontData
    })
}

/// Free FontData
#[no_mangle]
pub extern "C" fn vello_font_data_free(font: *mut VelloFontData) {
    if !font.is_null() {
        unsafe {
            drop(Box::from_raw(font as *mut FontData));
        }
    }
}

/// Fill glyphs with current paint
#[no_mangle]
pub extern "C" fn vello_render_context_fill_glyphs(
    ctx: *mut VelloRenderContext,
    font: *const VelloFontData,
    font_size: f32,
    glyphs: *const VelloGlyph,
    glyph_count: usize,
) -> c_int {
    if ctx.is_null() || font.is_null() || (glyph_count > 0 && glyphs.is_null()) {
        set_last_error("Null pointer");
        return VELLO_ERROR_NULL_POINTER;
    }

    ffi_catch!({
        let ctx = unsafe { &mut *(ctx as *mut vello_cpu::RenderContext) };
        let font_data = unsafe { &*(font as *const FontData) };
        let glyph_slice = if glyph_count > 0 {
            unsafe { std::slice::from_raw_parts(glyphs, glyph_count) }
        } else {
            &[]
        };

        use vello_cpu::Glyph;

        // Convert FFI glyphs to vello glyphs
        let vello_glyphs: Vec<Glyph> = glyph_slice
            .iter()
            .map(|g| Glyph {
                id: g.id,
                x: g.x,
                y: g.y,
            })
            .collect();

        // Create glyph run and fill
        ctx.glyph_run(font_data)
            .font_size(font_size)
            .fill_glyphs(vello_glyphs.into_iter());

        VELLO_OK
    })
}

/// Stroke glyphs with current paint and stroke settings
#[no_mangle]
pub extern "C" fn vello_render_context_stroke_glyphs(
    ctx: *mut VelloRenderContext,
    font: *const VelloFontData,
    font_size: f32,
    glyphs: *const VelloGlyph,
    glyph_count: usize,
) -> c_int {
    if ctx.is_null() || font.is_null() || (glyph_count > 0 && glyphs.is_null()) {
        set_last_error("Null pointer");
        return VELLO_ERROR_NULL_POINTER;
    }

    ffi_catch!({
        let ctx = unsafe { &mut *(ctx as *mut vello_cpu::RenderContext) };
        let font_data = unsafe { &*(font as *const FontData) };
        let glyph_slice = if glyph_count > 0 {
            unsafe { std::slice::from_raw_parts(glyphs, glyph_count) }
        } else {
            &[]
        };

        use vello_cpu::Glyph;

        // Convert FFI glyphs to vello glyphs
        let vello_glyphs: Vec<Glyph> = glyph_slice
            .iter()
            .map(|g| Glyph {
                id: g.id,
                x: g.x,
                y: g.y,
            })
            .collect();

        // Create glyph run and stroke
        ctx.glyph_run(font_data)
            .font_size(font_size)
            .stroke_glyphs(vello_glyphs.into_iter());

        VELLO_OK
    })
}

/// Helper function to convert UTF-8 text to glyph IDs
/// This is a simplified version - full text shaping would require harfbuzz or similar
#[no_mangle]
pub extern "C" fn vello_font_data_text_to_glyphs(
    font: *const VelloFontData,
    text: *const std::os::raw::c_char,
    out_glyphs: *mut VelloGlyph,
    max_glyphs: usize,
    out_count: *mut usize,
) -> c_int {
    if font.is_null() || text.is_null() || out_glyphs.is_null() || out_count.is_null() {
        set_last_error("Null pointer");
        return VELLO_ERROR_NULL_POINTER;
    }

    let font_data = unsafe { &*(font as *const FontData) };
    let c_str = unsafe { std::ffi::CStr::from_ptr(text) };

    let text_str = match c_str.to_str() {
        Ok(s) => s,
        Err(_) => {
            set_last_error("Invalid UTF-8");
            return VELLO_ERROR_INVALID_PARAMETER;
        }
    };

    use skrifa::{FontRef, MetadataProvider};
    use skrifa::instance::{Size, LocationRef};

    let font_ref = match FontRef::from_index(font_data.data.as_ref(), font_data.index) {
        Ok(f) => f,
        Err(_) => {
            set_last_error("Invalid font data");
            return VELLO_ERROR_INVALID_PARAMETER;
        }
    };

    let charmap = font_ref.charmap();
    let mut count = 0;
    let mut x_offset = 0.0f32;

    let glyphs_slice = unsafe { std::slice::from_raw_parts_mut(out_glyphs, max_glyphs) };

    for ch in text_str.chars() {
        if count >= max_glyphs {
            break;
        }

        if let Some(glyph_id) = charmap.map(ch) {
            glyphs_slice[count] = VelloGlyph {
                id: glyph_id.to_u32(),
                x: x_offset,
                y: 0.0,
            };
            count += 1;

            // Simple advance calculation (not perfect, but works for basic text)
            // In production, use proper text shaping with harfbuzz
            let metrics = font_ref.glyph_metrics(Size::unscaled(), LocationRef::default());
            if let Some(advance) = metrics.advance_width(glyph_id) {
                x_offset += advance;
            }
        }
    }

    unsafe { *out_count = count };
    VELLO_OK
}
