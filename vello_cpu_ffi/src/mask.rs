// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

//! FFI bindings for Mask

use crate::error::set_last_error;
use crate::{ffi_catch, ffi_catch_ptr};
use crate::types::{VelloMask, VelloPixmap, VELLO_ERROR_NULL_POINTER, VELLO_OK};
use std::os::raw::c_int;
use vello_cpu::Pixmap;
use vello_cpu::Mask;

/// Create a new alpha mask from a pixmap
#[no_mangle]
pub extern "C" fn vello_mask_new_alpha(pixmap: *const VelloPixmap) -> *mut VelloMask {
    if pixmap.is_null() {
        set_last_error("Null pixmap pointer");
        return std::ptr::null_mut();
    }

    ffi_catch_ptr!({
        let pixmap = unsafe { &*(pixmap as *const Pixmap) };
        let mask = Mask::new_alpha(pixmap);
        Box::into_raw(Box::new(mask)) as *mut VelloMask
    })
}

/// Create a new luminance mask from a pixmap
#[no_mangle]
pub extern "C" fn vello_mask_new_luminance(pixmap: *const VelloPixmap) -> *mut VelloMask {
    if pixmap.is_null() {
        set_last_error("Null pixmap pointer");
        return std::ptr::null_mut();
    }

    ffi_catch_ptr!({
        let pixmap = unsafe { &*(pixmap as *const Pixmap) };
        let mask = Mask::new_luminance(pixmap);
        Box::into_raw(Box::new(mask)) as *mut VelloMask
    })
}

/// Free a mask
#[no_mangle]
pub extern "C" fn vello_mask_free(mask: *mut VelloMask) {
    if !mask.is_null() {
        unsafe {
            let _ = Box::from_raw(mask as *mut Mask);
        }
    }
}

/// Get the width of a mask
#[no_mangle]
pub extern "C" fn vello_mask_get_width(mask: *const VelloMask) -> u16 {
    if mask.is_null() {
        return 0;
    }

    let mask = unsafe { &*(mask as *const Mask) };
    mask.width()
}

/// Get the height of a mask
#[no_mangle]
pub extern "C" fn vello_mask_get_height(mask: *const VelloMask) -> u16 {
    if mask.is_null() {
        return 0;
    }

    let mask = unsafe { &*(mask as *const Mask) };
    mask.height()
}

/// Push a mask layer
#[no_mangle]
pub extern "C" fn vello_render_context_push_mask_layer(
    ctx: *mut crate::types::VelloRenderContext,
    mask: *const VelloMask,
) -> c_int {
    if ctx.is_null() || mask.is_null() {
        set_last_error("Null pointer");
        return VELLO_ERROR_NULL_POINTER;
    }

    ffi_catch!({
        let ctx = unsafe { &mut *(ctx as *mut vello_cpu::RenderContext) };
        let mask = unsafe { &*(mask as *const Mask) };
        ctx.push_mask_layer(mask.clone());
        VELLO_OK
    })
}
