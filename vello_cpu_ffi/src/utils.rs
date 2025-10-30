// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

//! Utility functions and version info

use std::os::raw::c_char;

use crate::types::VelloSimdLevel;

/// Get library version string (static lifetime)
#[no_mangle]
pub extern "C" fn vello_version() -> *const c_char {
    static VERSION: &str = concat!(env!("CARGO_PKG_VERSION"), "\0");
    VERSION.as_ptr() as *const c_char
}

/// Detect SIMD capabilities of current hardware
#[no_mangle]
pub extern "C" fn vello_simd_detect() -> VelloSimdLevel {
    match vello_cpu::Level::try_detect() {
        Some(level) => level.into(),
        None => VelloSimdLevel::Fallback,
    }
}
