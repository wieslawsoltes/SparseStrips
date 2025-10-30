// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

//! BezPath FFI bindings

use std::os::raw::c_int;

use vello_cpu::kurbo::BezPath;

use crate::error::set_last_error;
use crate::types::*;
use crate::{ffi_catch, ffi_catch_ptr};

/// Create new empty BezPath
#[no_mangle]
pub extern "C" fn vello_bezpath_new() -> *mut VelloBezPath {
    ffi_catch_ptr!({
        let path = BezPath::new();
        Box::into_raw(Box::new(path)) as *mut VelloBezPath
    })
}

/// Free BezPath
#[no_mangle]
pub extern "C" fn vello_bezpath_free(path: *mut VelloBezPath) {
    if !path.is_null() {
        unsafe {
            drop(Box::from_raw(path as *mut BezPath));
        }
    }
}

/// Move to point
#[no_mangle]
pub extern "C" fn vello_bezpath_move_to(path: *mut VelloBezPath, x: f64, y: f64) -> c_int {
    if path.is_null() {
        set_last_error("Null path pointer");
        return VELLO_ERROR_NULL_POINTER;
    }

    ffi_catch!({
        let path = unsafe { &mut *(path as *mut BezPath) };
        path.move_to((x, y));
        VELLO_OK
    })
}

/// Line to point
#[no_mangle]
pub extern "C" fn vello_bezpath_line_to(path: *mut VelloBezPath, x: f64, y: f64) -> c_int {
    if path.is_null() {
        set_last_error("Null path pointer");
        return VELLO_ERROR_NULL_POINTER;
    }

    ffi_catch!({
        let path = unsafe { &mut *(path as *mut BezPath) };
        path.line_to((x, y));
        VELLO_OK
    })
}

/// Quadratic bezier curve
#[no_mangle]
pub extern "C" fn vello_bezpath_quad_to(
    path: *mut VelloBezPath,
    x1: f64,
    y1: f64,
    x2: f64,
    y2: f64,
) -> c_int {
    if path.is_null() {
        set_last_error("Null path pointer");
        return VELLO_ERROR_NULL_POINTER;
    }

    ffi_catch!({
        let path = unsafe { &mut *(path as *mut BezPath) };
        path.quad_to((x1, y1), (x2, y2));
        VELLO_OK
    })
}

/// Cubic bezier curve
#[no_mangle]
pub extern "C" fn vello_bezpath_curve_to(
    path: *mut VelloBezPath,
    x1: f64,
    y1: f64,
    x2: f64,
    y2: f64,
    x3: f64,
    y3: f64,
) -> c_int {
    if path.is_null() {
        set_last_error("Null path pointer");
        return VELLO_ERROR_NULL_POINTER;
    }

    ffi_catch!({
        let path = unsafe { &mut *(path as *mut BezPath) };
        path.curve_to((x1, y1), (x2, y2), (x3, y3));
        VELLO_OK
    })
}

/// Close path
#[no_mangle]
pub extern "C" fn vello_bezpath_close(path: *mut VelloBezPath) -> c_int {
    if path.is_null() {
        set_last_error("Null path pointer");
        return VELLO_ERROR_NULL_POINTER;
    }

    ffi_catch!({
        let path = unsafe { &mut *(path as *mut BezPath) };
        path.close_path();
        VELLO_OK
    })
}

/// Clear path (remove all elements)
#[no_mangle]
pub extern "C" fn vello_bezpath_clear(path: *mut VelloBezPath) -> c_int {
    if path.is_null() {
        set_last_error("Null path pointer");
        return VELLO_ERROR_NULL_POINTER;
    }

    ffi_catch!({
        let path = unsafe { &mut *(path as *mut BezPath) };
        path.truncate(0);
        VELLO_OK
    })
}

/// Fill path
#[no_mangle]
pub extern "C" fn vello_render_context_fill_path(
    ctx: *mut VelloRenderContext,
    path: *const VelloBezPath,
) -> c_int {
    if ctx.is_null() || path.is_null() {
        set_last_error("Null pointer");
        return VELLO_ERROR_NULL_POINTER;
    }

    ffi_catch!({
        let ctx = unsafe { &mut *(ctx as *mut vello_cpu::RenderContext) };
        let path = unsafe { &*(path as *const BezPath) };
        ctx.fill_path(path);
        VELLO_OK
    })
}

/// Stroke path
#[no_mangle]
pub extern "C" fn vello_render_context_stroke_path(
    ctx: *mut VelloRenderContext,
    path: *const VelloBezPath,
) -> c_int {
    if ctx.is_null() || path.is_null() {
        set_last_error("Null pointer");
        return VELLO_ERROR_NULL_POINTER;
    }

    ffi_catch!({
        let ctx = unsafe { &mut *(ctx as *mut vello_cpu::RenderContext) };
        let path = unsafe { &*(path as *const BezPath) };
        ctx.stroke_path(path);
        VELLO_OK
    })
}
