// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

//! # vello_cpu_ffi
//!
//! C FFI bindings for vello_cpu - a CPU-based 2D graphics renderer.
//!
//! This library provides a C-compatible API for the vello_cpu Rust library,
//! enabling interop with languages like C#, C++, Python, etc.
//!
//! ## Features
//!
//! - Zero-copy pixel access
//! - SIMD support (SSE2, AVX, AVX2, NEON)
//! - Multithreading support
//! - Comprehensive error handling
//! - PNG support (optional, via `png` feature)
//!
//! ## Safety
//!
//! All functions perform null pointer checks and use panic catching to prevent
//! unwinding across FFI boundaries. Error messages are stored in thread-local
//! storage and can be retrieved via `vello_get_last_error()`.

#![allow(clippy::missing_safety_doc)]

pub mod types;
pub mod error;
pub mod utils;
pub mod context;
pub mod pixmap;
pub mod path;
pub mod text;
pub mod mask;
pub mod image;

// Re-export main types for convenience
pub use types::*;

// Re-export error handling
pub use error::{vello_clear_last_error, vello_get_last_error};

// Re-export utility functions
pub use utils::{vello_simd_detect, vello_version};

// Re-export context functions
pub use context::*;

// Re-export pixmap functions
pub use pixmap::*;

// Re-export path functions
pub use path::*;

// Re-export text functions
pub use text::*;

// Re-export mask functions
pub use mask::*;

// Re-export image functions
pub use image::*;
