// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

//! FFI bindings for Image

use crate::error::set_last_error;
use crate::{ffi_catch, ffi_catch_ptr};
use crate::types::{VelloExtend, VelloImageQuality, VelloPixmap, VELLO_ERROR_INVALID_PARAMETER, VELLO_ERROR_NULL_POINTER, VELLO_OK};
use std::os::raw::c_int;
use std::sync::Arc;
use vello_cpu::{Pixmap, RenderContext};
use vello_cpu::peniko::{self, Extend, ImageQuality};
use vello_common::paint::{Image, ImageSource};

#[repr(C)]
pub struct VelloImage {
    _private: [u8; 0],
}

/// Create an image from a pixmap
#[no_mangle]
pub extern "C" fn vello_image_new_from_pixmap(
    pixmap: *const VelloPixmap,
    x_extend: VelloExtend,
    y_extend: VelloExtend,
    quality: VelloImageQuality,
    alpha: f32,
) -> *mut VelloImage {
    if pixmap.is_null() {
        set_last_error("Null pixmap pointer");
        return std::ptr::null_mut();
    }

    ffi_catch_ptr!({
        let pixmap = unsafe { &*(pixmap as *const Pixmap) };

        let x_ext = match x_extend {
            VelloExtend::Pad => Extend::Pad,
            VelloExtend::Repeat => Extend::Repeat,
            VelloExtend::Reflect => Extend::Reflect,
        };

        let y_ext = match y_extend {
            VelloExtend::Pad => Extend::Pad,
            VelloExtend::Repeat => Extend::Repeat,
            VelloExtend::Reflect => Extend::Reflect,
        };

        let qual = match quality {
            VelloImageQuality::Low => ImageQuality::Low,
            VelloImageQuality::Medium => ImageQuality::Medium,
            VelloImageQuality::High => ImageQuality::High,
        };

        let image = Image {
            image: ImageSource::Pixmap(Arc::new(pixmap.clone())),
            sampler: peniko::ImageSampler {
                x_extend: x_ext,
                y_extend: y_ext,
                quality: qual,
                alpha,
            },
        };

        Box::into_raw(Box::new(image)) as *mut VelloImage
    })
}

/// Free an image
#[no_mangle]
pub extern "C" fn vello_image_free(image: *mut VelloImage) {
    if !image.is_null() {
        unsafe {
            let _ = Box::from_raw(image as *mut Image);
        }
    }
}

/// Set paint to image
#[no_mangle]
pub extern "C" fn vello_render_context_set_paint_image(
    ctx: *mut crate::types::VelloRenderContext,
    image: *const VelloImage,
) -> c_int {
    if ctx.is_null() || image.is_null() {
        set_last_error("Null pointer");
        return VELLO_ERROR_NULL_POINTER;
    }

    ffi_catch!({
        let ctx = unsafe { &mut *(ctx as *mut RenderContext) };
        let image = unsafe { &*(image as *const Image) };

        if (image.sampler.alpha - 1.0).abs() > f32::EPSILON {
            set_last_error("Image opacity is not supported yet");
            return VELLO_ERROR_INVALID_PARAMETER;
        }

        ctx.set_paint(image.clone());
        VELLO_OK
    })
}
