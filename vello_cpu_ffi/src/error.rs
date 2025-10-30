// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

//! Error handling for FFI

use std::cell::RefCell;
use std::ffi::CString;
use std::os::raw::c_char;

thread_local! {
    static LAST_ERROR: RefCell<Option<CString>> = RefCell::new(None);
}

/// Set the last error message
pub fn set_last_error(err: impl Into<String>) {
    LAST_ERROR.with(|e| {
        let err_string = err.into();
        if let Ok(c_string) = CString::new(err_string) {
            *e.borrow_mut() = Some(c_string);
        }
    });
}

/// Get the last error message (thread-local, UTF-8)
#[no_mangle]
pub extern "C" fn vello_get_last_error() -> *const c_char {
    LAST_ERROR.with(|e| match &*e.borrow() {
        Some(err) => err.as_ptr(),
        None => std::ptr::null(),
    })
}

/// Clear the last error
#[no_mangle]
pub extern "C" fn vello_clear_last_error() {
    LAST_ERROR.with(|e| {
        *e.borrow_mut() = None;
    });
}

/// Helper macro for wrapping FFI functions with panic catching (returns error code)
#[macro_export]
macro_rules! ffi_catch {
    ($body:expr) => {
        match std::panic::catch_unwind(std::panic::AssertUnwindSafe(|| $body)) {
            Ok(result) => result,
            Err(e) => {
                let msg = if let Some(s) = e.downcast_ref::<&str>() {
                    s.to_string()
                } else if let Some(s) = e.downcast_ref::<String>() {
                    s.clone()
                } else {
                    "Unknown panic occurred".to_string()
                };
                $crate::error::set_last_error(format!("Panic: {}", msg));
                $crate::types::VELLO_ERROR_RENDER_FAILED
            }
        }
    };
}

/// Helper macro for wrapping FFI functions that return pointers
#[macro_export]
macro_rules! ffi_catch_ptr {
    ($body:expr) => {
        match std::panic::catch_unwind(std::panic::AssertUnwindSafe(|| $body)) {
            Ok(result) => result,
            Err(e) => {
                let msg = if let Some(s) = e.downcast_ref::<&str>() {
                    s.to_string()
                } else if let Some(s) = e.downcast_ref::<String>() {
                    s.clone()
                } else {
                    "Unknown panic occurred".to_string()
                };
                $crate::error::set_last_error(format!("Panic: {}", msg));
                std::ptr::null_mut()
            }
        }
    };
}
