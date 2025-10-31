// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

//! FFI marshaling test functions for verifying correct data passing between C# and Rust

use crate::types::*;
use std::os::raw::c_int;

/// Test function: echo back VelloRenderSettings to verify marshaling
#[no_mangle]
pub extern "C" fn vello_test_echo_render_settings(
    input: *const VelloRenderSettings,
    output: *mut VelloRenderSettings,
) -> c_int {
    if input.is_null() || output.is_null() {
        return VELLO_ERROR_NULL_POINTER;
    }

    unsafe {
        *output = *input;
    }
    VELLO_OK
}

/// Test function: echo back VelloStroke to verify marshaling
#[no_mangle]
pub extern "C" fn vello_test_echo_stroke(
    input: *const VelloStroke,
    output: *mut VelloStroke,
) -> c_int {
    if input.is_null() || output.is_null() {
        return VELLO_ERROR_NULL_POINTER;
    }

    unsafe {
        *output = *input;
    }
    VELLO_OK
}

/// Test function: echo back VelloBlendMode to verify marshaling
#[no_mangle]
pub extern "C" fn vello_test_echo_blend_mode(
    input: *const VelloBlendMode,
    output: *mut VelloBlendMode,
) -> c_int {
    if input.is_null() || output.is_null() {
        return VELLO_ERROR_NULL_POINTER;
    }

    unsafe {
        *output = *input;
    }
    VELLO_OK
}

/// Test function: echo back VelloColorStop to verify marshaling
#[no_mangle]
pub extern "C" fn vello_test_echo_color_stop(
    input: *const VelloColorStop,
    output: *mut VelloColorStop,
) -> c_int {
    if input.is_null() || output.is_null() {
        return VELLO_ERROR_NULL_POINTER;
    }

    unsafe {
        *output = *input;
    }
    VELLO_OK
}

/// Test function: echo back VelloPoint to verify marshaling
#[no_mangle]
pub extern "C" fn vello_test_echo_point(
    input: *const VelloPoint,
    output: *mut VelloPoint,
) -> c_int {
    if input.is_null() || output.is_null() {
        return VELLO_ERROR_NULL_POINTER;
    }

    unsafe {
        *output = *input;
    }
    VELLO_OK
}

/// Test function: echo back VelloRect to verify marshaling
#[no_mangle]
pub extern "C" fn vello_test_echo_rect(
    input: *const VelloRect,
    output: *mut VelloRect,
) -> c_int {
    if input.is_null() || output.is_null() {
        return VELLO_ERROR_NULL_POINTER;
    }

    unsafe {
        *output = *input;
    }
    VELLO_OK
}

/// Test function: echo back VelloAffine to verify marshaling
#[no_mangle]
pub extern "C" fn vello_test_echo_affine(
    input: *const VelloAffine,
    output: *mut VelloAffine,
) -> c_int {
    if input.is_null() || output.is_null() {
        return VELLO_ERROR_NULL_POINTER;
    }

    unsafe {
        *output = *input;
    }
    VELLO_OK
}

/// Test function: echo back VelloPremulRgba8 to verify marshaling
#[no_mangle]
pub extern "C" fn vello_test_echo_color(
    input: *const VelloPremulRgba8,
    output: *mut VelloPremulRgba8,
) -> c_int {
    if input.is_null() || output.is_null() {
        return VELLO_ERROR_NULL_POINTER;
    }

    unsafe {
        *output = *input;
    }
    VELLO_OK
}
