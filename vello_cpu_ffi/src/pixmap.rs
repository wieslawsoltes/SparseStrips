// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

//! Pixmap FFI bindings

use std::os::raw::c_int;

use vello_cpu::Pixmap;

use crate::error::set_last_error;
use crate::types::*;
use crate::{ffi_catch, ffi_catch_ptr};

/// Create new pixmap
#[no_mangle]
pub extern "C" fn vello_pixmap_new(width: u16, height: u16) -> *mut VelloPixmap {
    ffi_catch_ptr!({
        let pixmap = Pixmap::new(width, height);
        Box::into_raw(Box::new(pixmap)) as *mut VelloPixmap
    })
}

/// Free pixmap
#[no_mangle]
pub extern "C" fn vello_pixmap_free(pixmap: *mut VelloPixmap) {
    if !pixmap.is_null() {
        unsafe {
            drop(Box::from_raw(pixmap as *mut Pixmap));
        }
    }
}

/// Get pixmap width
#[no_mangle]
pub extern "C" fn vello_pixmap_width(pixmap: *const VelloPixmap) -> u16 {
    if pixmap.is_null() {
        return 0;
    }
    unsafe {
        let pixmap = &*(pixmap as *const Pixmap);
        pixmap.width()
    }
}

/// Get pixmap height
#[no_mangle]
pub extern "C" fn vello_pixmap_height(pixmap: *const VelloPixmap) -> u16 {
    if pixmap.is_null() {
        return 0;
    }
    unsafe {
        let pixmap = &*(pixmap as *const Pixmap);
        pixmap.height()
    }
}

/// Get pixmap data pointer and length (zero-copy access)
#[no_mangle]
pub extern "C" fn vello_pixmap_data(
    pixmap: *const VelloPixmap,
    out_ptr: *mut *const VelloPremulRgba8,
    out_len: *mut usize,
) -> c_int {
    if pixmap.is_null() || out_ptr.is_null() || out_len.is_null() {
        set_last_error("Null pointer");
        return VELLO_ERROR_NULL_POINTER;
    }

    ffi_catch!({
        let pixmap = unsafe { &*(pixmap as *const Pixmap) };
        let data = pixmap.data();
        unsafe {
            *out_ptr = data.as_ptr() as *const VelloPremulRgba8;
            *out_len = data.len();
        }
        VELLO_OK
    })
}

/// Get mutable pixmap data pointer and length
#[no_mangle]
pub extern "C" fn vello_pixmap_data_mut(
    pixmap: *mut VelloPixmap,
    out_ptr: *mut *mut VelloPremulRgba8,
    out_len: *mut usize,
) -> c_int {
    if pixmap.is_null() || out_ptr.is_null() || out_len.is_null() {
        set_last_error("Null pointer");
        return VELLO_ERROR_NULL_POINTER;
    }

    ffi_catch!({
        let pixmap = unsafe { &mut *(pixmap as *mut Pixmap) };
        let data = pixmap.data_mut();
        unsafe {
            *out_ptr = data.as_mut_ptr() as *mut VelloPremulRgba8;
            *out_len = data.len();
        }
        VELLO_OK
    })
}

/// Resize pixmap
#[no_mangle]
pub extern "C" fn vello_pixmap_resize(
    pixmap: *mut VelloPixmap,
    width: u16,
    height: u16,
) -> c_int {
    if pixmap.is_null() {
        set_last_error("Null pixmap pointer");
        return VELLO_ERROR_NULL_POINTER;
    }

    ffi_catch!({
        let pixmap = unsafe { &mut *(pixmap as *mut Pixmap) };
        pixmap.resize(width, height);
        VELLO_OK
    })
}

/// Sample pixel at coordinates
#[no_mangle]
pub extern "C" fn vello_pixmap_sample(
    pixmap: *const VelloPixmap,
    x: u16,
    y: u16,
    out_pixel: *mut VelloPremulRgba8,
) -> c_int {
    if pixmap.is_null() || out_pixel.is_null() {
        set_last_error("Null pointer");
        return VELLO_ERROR_NULL_POINTER;
    }

    ffi_catch!({
        let pixmap = unsafe { &*(pixmap as *const Pixmap) };
        if x >= pixmap.width() || y >= pixmap.height() {
            set_last_error("Coordinates out of bounds");
            return VELLO_ERROR_INVALID_PARAMETER;
        }
        let pixel = pixmap.sample(x, y);
        unsafe {
            *out_pixel = pixel.into();
        }
        VELLO_OK
    })
}

/// Render to pixmap
#[no_mangle]
pub extern "C" fn vello_render_context_render_to_pixmap(
    ctx: *const VelloRenderContext,
    pixmap: *mut VelloPixmap,
) -> c_int {
    if ctx.is_null() || pixmap.is_null() {
        set_last_error("Null pointer");
        return VELLO_ERROR_NULL_POINTER;
    }

    ffi_catch!({
        let ctx = unsafe { &*(ctx as *const vello_cpu::RenderContext) };
        let pixmap = unsafe { &mut *(pixmap as *mut Pixmap) };
        ctx.render_to_pixmap(pixmap);
        VELLO_OK
    })
}

#[cfg(feature = "png")]
#[no_mangle]
pub extern "C" fn vello_pixmap_from_png(data: *const u8, len: usize) -> *mut VelloPixmap {
    if data.is_null() || len == 0 {
        set_last_error("Null or empty PNG data");
        return std::ptr::null_mut();
    }

    ffi_catch_ptr!({
        let slice = unsafe { std::slice::from_raw_parts(data, len) };
        match Pixmap::from_png(slice) {
            Ok(pixmap) => Box::into_raw(Box::new(pixmap)) as *mut VelloPixmap,
            Err(e) => {
                set_last_error(format!("PNG decode error: {:?}", e));
                std::ptr::null_mut()
            }
        }
    })
}

#[cfg(feature = "png")]
#[no_mangle]
pub extern "C" fn vello_pixmap_to_png(
    pixmap: *const VelloPixmap,
    out_data: *mut *mut u8,
    out_len: *mut usize,
) -> c_int {
    if pixmap.is_null() || out_data.is_null() || out_len.is_null() {
        set_last_error("Null pointer");
        return VELLO_ERROR_NULL_POINTER;
    }

    ffi_catch!({
        let pixmap = unsafe { &*(pixmap as *const Pixmap) };
        match pixmap.clone().into_png() {
            Ok(png_data) => {
                let mut boxed = png_data.into_boxed_slice();
                unsafe {
                    *out_len = boxed.len();
                    *out_data = boxed.as_mut_ptr();
                    std::mem::forget(boxed); // Prevent deallocation
                }
                VELLO_OK
            }
            Err(e) => {
                set_last_error(format!("PNG encode error: {:?}", e));
                VELLO_ERROR_PNG_ENCODE
            }
        }
    })
}

#[cfg(feature = "png")]
#[no_mangle]
pub extern "C" fn vello_png_data_free(data: *mut u8, len: usize) {
    if !data.is_null() && len > 0 {
        unsafe {
            let _ = Box::from_raw(std::slice::from_raw_parts_mut(data, len));
        }
    }
}
