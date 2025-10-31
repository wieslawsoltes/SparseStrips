// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

use crate::error::set_last_error;
use crate::types::{VelloAffine, VelloFillRule, VelloStroke};
use crate::VelloRect;
use std::ffi::c_void;
use vello_common::recording::Recording as RustRecording;
use vello_cpu::RenderContext as RustRenderContext;

/// Opaque handle to a Recording.
pub struct VelloRecording(pub(crate) RustRecording);

/// Create a new empty recording
#[no_mangle]
pub extern "C" fn vello_recording_new() -> *mut VelloRecording {
    Box::into_raw(Box::new(VelloRecording(RustRecording::new())))
}

/// Free a recording
#[no_mangle]
pub extern "C" fn vello_recording_free(recording: *mut VelloRecording) {
    if !recording.is_null() {
        unsafe {
            drop(Box::from_raw(recording));
        }
    }
}

/// Clear all recorded commands
#[no_mangle]
pub extern "C" fn vello_recording_clear(recording: *mut VelloRecording) -> i32 {
    if recording.is_null() {
        set_last_error("Null recording pointer");
        return -1;
    }

    let recording = unsafe { &mut *recording };
    recording.0.clear();
    0 // Success
}

/// Get the number of recorded commands
#[no_mangle]
pub extern "C" fn vello_recording_len(recording: *const VelloRecording) -> usize {
    if recording.is_null() {
        set_last_error("Null recording pointer");
        return 0;
    }

    let recording = unsafe { &*recording };
    recording.0.command_count()
}

/// Check if recording has cached strips
#[no_mangle]
pub extern "C" fn vello_recording_has_cached_strips(recording: *const VelloRecording) -> i32 {
    if recording.is_null() {
        set_last_error("Null recording pointer");
        return 0;
    }

    let recording = unsafe { &*recording };
    if recording.0.has_cached_strips() {
        1
    } else {
        0
    }
}

/// Get the number of cached strips
#[no_mangle]
pub extern "C" fn vello_recording_strip_count(recording: *const VelloRecording) -> usize {
    if recording.is_null() {
        set_last_error("Null recording pointer");
        return 0;
    }

    let recording = unsafe { &*recording };
    recording.0.strip_count()
}

/// Get the number of cached alpha bytes
#[no_mangle]
pub extern "C" fn vello_recording_alpha_count(recording: *const VelloRecording) -> usize {
    if recording.is_null() {
        set_last_error("Null recording pointer");
        return 0;
    }

    let recording = unsafe { &*recording };
    recording.0.alpha_count()
}

// Record drawing operations for later replay
//
// The callback will be invoked with a recorder that supports the same
// drawing operations as RenderContext. All operations will be recorded
// into the provided Recording for later playback.
#[no_mangle]
pub extern "C" fn vello_render_context_record(
    ctx: *mut c_void,
    recording: *mut VelloRecording,
    callback: extern "C" fn(*mut c_void, *mut c_void),
    user_data: *mut c_void,
) -> i32 {
    if ctx.is_null() {
        set_last_error("Null context pointer");
        return -1;
    }
    if recording.is_null() {
        set_last_error("Null recording pointer");
        return -1;
    }

    let ctx = unsafe { &mut *(ctx as *mut RustRenderContext) };
    let recording = unsafe { &mut *recording };

    use vello_common::recording::Recordable;
    ctx.record(&mut recording.0, |recorder| {
        // Pass the recorder to the callback
        callback(user_data, recorder as *mut _ as *mut c_void);
    });

    0 // Success
}

/// Prepare a recording for optimized playback
#[no_mangle]
pub extern "C" fn vello_render_context_prepare_recording(
    ctx: *mut c_void,
    recording: *mut VelloRecording,
) -> i32 {
    if ctx.is_null() {
        set_last_error("Null context pointer");
        return -1;
    }
    if recording.is_null() {
        set_last_error("Null recording pointer");
        return -1;
    }

    let ctx = unsafe { &mut *(ctx as *mut RustRenderContext) };
    let recording = unsafe { &mut *recording };

    use vello_common::recording::Recordable;
    ctx.prepare_recording(&mut recording.0);

    0 // Success
}

/// Execute a previously recorded set of drawing operations
#[no_mangle]
pub extern "C" fn vello_render_context_execute_recording(
    ctx: *mut c_void,
    recording: *const VelloRecording,
) -> i32 {
    if ctx.is_null() {
        set_last_error("Null context pointer");
        return -1;
    }
    if recording.is_null() {
        set_last_error("Null recording pointer");
        return -1;
    }

    let ctx = unsafe { &mut *(ctx as *mut RustRenderContext) };
    let recording = unsafe { &*recording };

    use vello_common::recording::Recordable;
    ctx.execute_recording(&recording.0);

    0 // Success
}

// Recorder drawing methods - these will be called from the callback

/// Fill a rectangle (recorder version)
#[no_mangle]
pub extern "C" fn vello_recorder_fill_rect(
    recorder: *mut c_void,
    rect: *const VelloRect,
) -> i32 {
    if recorder.is_null() {
        set_last_error("Null recorder pointer");
        return -1;
    }
    if rect.is_null() {
        set_last_error("Null rect pointer");
        return -1;
    }

    let r = unsafe { &*rect };
    let rect = vello_cpu::kurbo::Rect::new(r.x0, r.y0, r.x1, r.y1);
    let recorder = unsafe { &mut *(recorder as *mut vello_common::recording::Recorder) };

    recorder.fill_rect(&rect);

    0 // Success
}

/// Stroke a rectangle (recorder version)
#[no_mangle]
pub extern "C" fn vello_recorder_stroke_rect(
    recorder: *mut c_void,
    rect: *const VelloRect,
) -> i32 {
    if recorder.is_null() {
        set_last_error("Null recorder pointer");
        return -1;
    }
    if rect.is_null() {
        set_last_error("Null rect pointer");
        return -1;
    }

    let r = unsafe { &*rect };
    let rect = vello_cpu::kurbo::Rect::new(r.x0, r.y0, r.x1, r.y1);
    let recorder = unsafe { &mut *(recorder as *mut vello_common::recording::Recorder) };

    recorder.stroke_rect(&rect);

    0 // Success
}

/// Fill a path (recorder version)
#[no_mangle]
pub extern "C" fn vello_recorder_fill_path(
    recorder: *mut c_void,
    path: *const c_void,
) -> i32 {
    if recorder.is_null() {
        set_last_error("Null recorder pointer");
        return -1;
    }
    if path.is_null() {
        set_last_error("Null path pointer");
        return -1;
    }

    let path = unsafe { &*(path as *const vello_cpu::peniko::kurbo::BezPath) };
    let recorder = unsafe { &mut *(recorder as *mut vello_common::recording::Recorder) };

    recorder.fill_path(path);

    0 // Success
}

/// Stroke a path (recorder version)
#[no_mangle]
pub extern "C" fn vello_recorder_stroke_path(
    recorder: *mut c_void,
    path: *const c_void,
) -> i32 {
    if recorder.is_null() {
        set_last_error("Null recorder pointer");
        return -1;
    }
    if path.is_null() {
        set_last_error("Null path pointer");
        return -1;
    }

    let path = unsafe { &*(path as *const vello_cpu::peniko::kurbo::BezPath) };
    let recorder = unsafe { &mut *(recorder as *mut vello_common::recording::Recorder) };

    recorder.stroke_path(path);

    0 // Success
}

/// Set solid color paint (recorder version)
#[no_mangle]
pub extern "C" fn vello_recorder_set_paint_solid(
    recorder: *mut c_void,
    r: u8,
    g: u8,
    b: u8,
    a: u8,
) -> i32 {
    if recorder.is_null() {
        set_last_error("Null recorder pointer");
        return -1;
    }

    let color = vello_cpu::peniko::Color::from_rgba8(r, g, b, a);
    let recorder = unsafe { &mut *(recorder as *mut vello_common::recording::Recorder) };

    recorder.set_paint(color);

    0 // Success
}

/// Set transform (recorder version)
#[no_mangle]
pub extern "C" fn vello_recorder_set_transform(
    recorder: *mut c_void,
    affine: *const VelloAffine,
) -> i32 {
    if recorder.is_null() {
        set_last_error("Null recorder pointer");
        return -1;
    }
    if affine.is_null() {
        set_last_error("Null affine pointer");
        return -1;
    }

    let a = unsafe { &*affine };
    let transform = vello_cpu::kurbo::Affine::new([a.m11, a.m12, a.m21, a.m22, a.m13, a.m23]);
    let recorder = unsafe { &mut *(recorder as *mut vello_common::recording::Recorder) };

    recorder.set_transform(transform);

    0 // Success
}

/// Set fill rule (recorder version)
#[no_mangle]
pub extern "C" fn vello_recorder_set_fill_rule(
    recorder: *mut c_void,
    fill_rule: VelloFillRule,
) -> i32 {
    if recorder.is_null() {
        set_last_error("Null recorder pointer");
        return -1;
    }

    let fill_rule = match fill_rule {
        VelloFillRule::NonZero => vello_cpu::peniko::Fill::NonZero,
        VelloFillRule::EvenOdd => vello_cpu::peniko::Fill::EvenOdd,
    };
    let recorder = unsafe { &mut *(recorder as *mut vello_common::recording::Recorder) };

    recorder.set_fill_rule(fill_rule);

    0 // Success
}

/// Set stroke settings (recorder version)
#[no_mangle]
pub extern "C" fn vello_recorder_set_stroke(
    recorder: *mut c_void,
    stroke: *const VelloStroke,
) -> i32 {
    if recorder.is_null() {
        set_last_error("Null recorder pointer");
        return -1;
    }
    if stroke.is_null() {
        set_last_error("Null stroke pointer");
        return -1;
    }

    let s = unsafe { &*stroke };
    let mut rust_stroke = vello_cpu::kurbo::Stroke::new(s.width as f64);

    use crate::types::{VelloCap, VelloJoin};
    rust_stroke.start_cap = match s.start_cap {
        VelloCap::Butt => vello_cpu::kurbo::Cap::Butt,
        VelloCap::Square => vello_cpu::kurbo::Cap::Square,
        VelloCap::Round => vello_cpu::kurbo::Cap::Round,
    };
    rust_stroke.end_cap = match s.end_cap {
        VelloCap::Butt => vello_cpu::kurbo::Cap::Butt,
        VelloCap::Square => vello_cpu::kurbo::Cap::Square,
        VelloCap::Round => vello_cpu::kurbo::Cap::Round,
    };
    rust_stroke.join = match s.join {
        VelloJoin::Bevel => vello_cpu::kurbo::Join::Bevel,
        VelloJoin::Miter => vello_cpu::kurbo::Join::Miter,
        VelloJoin::Round => vello_cpu::kurbo::Join::Round,
    };
    rust_stroke.miter_limit = s.miter_limit as f64;

    let recorder = unsafe { &mut *(recorder as *mut vello_common::recording::Recorder) };
    recorder.set_stroke(rust_stroke);

    0 // Success
}

/// Set paint transform (recorder version)
#[no_mangle]
pub extern "C" fn vello_recorder_set_paint_transform(
    recorder: *mut c_void,
    affine: *const VelloAffine,
) -> i32 {
    if recorder.is_null() {
        set_last_error("Null recorder pointer");
        return -1;
    }
    if affine.is_null() {
        set_last_error("Null affine pointer");
        return -1;
    }

    let a = unsafe { &*affine };
    let transform = vello_cpu::kurbo::Affine::new([a.m11, a.m12, a.m21, a.m22, a.m13, a.m23]);
    let recorder = unsafe { &mut *(recorder as *mut vello_common::recording::Recorder) };

    recorder.set_paint_transform(transform);

    0 // Success
}

/// Reset paint transform (recorder version)
#[no_mangle]
pub extern "C" fn vello_recorder_reset_paint_transform(recorder: *mut c_void) -> i32 {
    if recorder.is_null() {
        set_last_error("Null recorder pointer");
        return -1;
    }

    let recorder = unsafe { &mut *(recorder as *mut vello_common::recording::Recorder) };
    recorder.reset_paint_transform();

    0 // Success
}

/// Push a clip layer (recorder version)
#[no_mangle]
pub extern "C" fn vello_recorder_push_clip_layer(
    recorder: *mut c_void,
    clip_path: *const c_void,
) -> i32 {
    if recorder.is_null() {
        set_last_error("Null recorder pointer");
        return -1;
    }
    if clip_path.is_null() {
        set_last_error("Null clip path pointer");
        return -1;
    }

    let path = unsafe { &*(clip_path as *const vello_cpu::peniko::kurbo::BezPath) };
    let recorder = unsafe { &mut *(recorder as *mut vello_common::recording::Recorder) };

    recorder.push_clip_layer(path);

    0 // Success
}

/// Pop a layer (recorder version)
#[no_mangle]
pub extern "C" fn vello_recorder_pop_layer(recorder: *mut c_void) -> i32 {
    if recorder.is_null() {
        set_last_error("Null recorder pointer");
        return -1;
    }

    let recorder = unsafe { &mut *(recorder as *mut vello_common::recording::Recorder) };
    recorder.pop_layer();

    0 // Success
}
