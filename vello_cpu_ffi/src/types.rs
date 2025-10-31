// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

//! FFI type definitions matching C ABI

use std::os::raw::c_int;

/// Error codes
pub const VELLO_OK: c_int = 0;
pub const VELLO_ERROR_NULL_POINTER: c_int = -1;
pub const VELLO_ERROR_INVALID_HANDLE: c_int = -2;
pub const VELLO_ERROR_RENDER_FAILED: c_int = -3;
pub const VELLO_ERROR_OUT_OF_MEMORY: c_int = -4;
pub const VELLO_ERROR_INVALID_PARAMETER: c_int = -5;
pub const VELLO_ERROR_PNG_DECODE: c_int = -6;
pub const VELLO_ERROR_PNG_ENCODE: c_int = -7;

/// Opaque handle types (exposed as void pointers to C)
pub type VelloRenderContext = std::ffi::c_void;
pub type VelloPixmap = std::ffi::c_void;
pub type VelloBezPath = std::ffi::c_void;
pub type VelloMask = std::ffi::c_void;

/// Premultiplied RGBA8 color
#[repr(C)]
#[derive(Copy, Clone, Debug, PartialEq, Eq)]
pub struct VelloPremulRgba8 {
    pub r: u8,
    pub g: u8,
    pub b: u8,
    pub a: u8,
}

/// 2D point
#[repr(C)]
#[derive(Copy, Clone, Debug, PartialEq)]
pub struct VelloPoint {
    pub x: f64,
    pub y: f64,
}

/// Rectangle
#[repr(C)]
#[derive(Copy, Clone, Debug, PartialEq)]
pub struct VelloRect {
    pub x0: f64,
    pub y0: f64,
    pub x1: f64,
    pub y1: f64,
}

/// 2D affine transformation (2x3 matrix)
#[repr(C)]
#[derive(Copy, Clone, Debug, PartialEq)]
pub struct VelloAffine {
    pub m11: f64,
    pub m12: f64,
    pub m13: f64,
    pub m21: f64,
    pub m22: f64,
    pub m23: f64,
}

/// Stroke parameters
#[repr(C)]
#[derive(Copy, Clone, Debug, PartialEq)]
pub struct VelloStroke {
    pub width: f32,
    pub miter_limit: f32,
    pub join: VelloJoin,
    pub start_cap: VelloCap,
    pub end_cap: VelloCap,
    pub _padding: [u8; 3],
}

/// Render settings
#[repr(C)]
#[derive(Copy, Clone, Debug, PartialEq, Eq)]
pub struct VelloRenderSettings {
    pub level: VelloSimdLevel,
    pub num_threads: u16,
    pub render_mode: VelloRenderMode,
    pub _padding: u8,
}

/// Render mode enumeration
#[repr(C)]
#[derive(Copy, Clone, Debug, PartialEq, Eq)]
pub enum VelloRenderMode {
    OptimizeSpeed = 0,
    OptimizeQuality = 1,
}

/// SIMD level enumeration
#[repr(u8)]
#[derive(Copy, Clone, Debug, PartialEq, Eq)]
pub enum VelloSimdLevel {
    Fallback = 0,
    Sse2 = 1,
    Sse42 = 2,
    Avx = 3,
    Avx2 = 4,
    Avx512 = 5,
    Neon = 6,
}

/// Line join style
#[repr(C)]
#[derive(Copy, Clone, Debug, PartialEq, Eq)]
pub enum VelloJoin {
    Bevel = 0,
    Miter = 1,
    Round = 2,
}

/// Line cap style
#[repr(C)]
#[derive(Copy, Clone, Debug, PartialEq, Eq)]
pub enum VelloCap {
    Butt = 0,
    Square = 1,
    Round = 2,
}

/// Fill rule
#[repr(C)]
#[derive(Copy, Clone, Debug, PartialEq, Eq)]
pub enum VelloFillRule {
    NonZero = 0,
    EvenOdd = 1,
}

/// Blend mix mode
#[repr(C)]
#[derive(Copy, Clone, Debug, PartialEq, Eq)]
pub enum VelloMix {
    Normal = 0,
    Multiply = 1,
    Screen = 2,
    Overlay = 3,
    Darken = 4,
    Lighten = 5,
    ColorDodge = 6,
    ColorBurn = 7,
    HardLight = 8,
    SoftLight = 9,
    Difference = 10,
    Exclusion = 11,
    Hue = 12,
    Saturation = 13,
    Color = 14,
    Luminosity = 15,
}

/// Blend compose mode
#[repr(C)]
#[derive(Copy, Clone, Debug, PartialEq, Eq)]
pub enum VelloCompose {
    Clear = 0,
    Copy = 1,
    Dest = 2,
    SrcOver = 3,
    DestOver = 4,
    SrcIn = 5,
    DestIn = 6,
    SrcOut = 7,
    DestOut = 8,
    SrcAtop = 9,
    DestAtop = 10,
    Xor = 11,
    Plus = 12,
    PlusLighter = 13,
}

/// Blend mode
#[repr(C)]
#[derive(Copy, Clone, Debug, PartialEq, Eq)]
pub struct VelloBlendMode {
    pub mix: VelloMix,
    pub compose: VelloCompose,
}

/// Color stop for gradients
#[repr(C)]
#[derive(Copy, Clone, Debug, PartialEq)]
pub struct VelloColorStop {
    pub offset: f32,
    pub r: u8,
    pub g: u8,
    pub b: u8,
    pub a: u8,
}

/// Gradient extend mode
#[repr(C)]
#[derive(Copy, Clone, Debug, PartialEq, Eq)]
pub enum VelloExtend {
    Pad = 0,
    Repeat = 1,
    Reflect = 2,
}

/// Image quality mode
#[repr(C)]
#[derive(Copy, Clone, Debug, PartialEq, Eq)]
pub enum VelloImageQuality {
    Low = 0,
    Medium = 1,
    High = 2,
}

// Conversion helpers
impl From<vello_common::peniko::color::PremulRgba8> for VelloPremulRgba8 {
    fn from(color: vello_common::peniko::color::PremulRgba8) -> Self {
        Self {
            r: color.r,
            g: color.g,
            b: color.b,
            a: color.a,
        }
    }
}

impl From<VelloPremulRgba8> for vello_common::peniko::color::PremulRgba8 {
    fn from(color: VelloPremulRgba8) -> Self {
        Self {
            r: color.r,
            g: color.g,
            b: color.b,
            a: color.a,
        }
    }
}

impl From<vello_cpu::RenderMode> for VelloRenderMode {
    fn from(mode: vello_cpu::RenderMode) -> Self {
        match mode {
            vello_cpu::RenderMode::OptimizeSpeed => VelloRenderMode::OptimizeSpeed,
            vello_cpu::RenderMode::OptimizeQuality => VelloRenderMode::OptimizeQuality,
        }
    }
}

impl From<VelloRenderMode> for vello_cpu::RenderMode {
    fn from(mode: VelloRenderMode) -> Self {
        match mode {
            VelloRenderMode::OptimizeSpeed => vello_cpu::RenderMode::OptimizeSpeed,
            VelloRenderMode::OptimizeQuality => vello_cpu::RenderMode::OptimizeQuality,
        }
    }
}

impl From<vello_cpu::Level> for VelloSimdLevel {
    fn from(level: vello_cpu::Level) -> Self {
        // Map based on level name
        let name = format!("{:?}", level).to_lowercase();
        if name.contains("neon") {
            VelloSimdLevel::Neon
        } else if name.contains("avx512") {
            VelloSimdLevel::Avx512
        } else if name.contains("avx2") {
            VelloSimdLevel::Avx2
        } else if name.contains("avx") {
            VelloSimdLevel::Avx
        } else if name.contains("sse4") {
            VelloSimdLevel::Sse42
        } else if name.contains("sse2") {
            VelloSimdLevel::Sse2
        } else {
            VelloSimdLevel::Fallback
        }
    }
}

impl VelloSimdLevel {
    pub fn to_vello_level(self) -> vello_cpu::Level {
        match self {
            VelloSimdLevel::Fallback => vello_cpu::Level::fallback(),
            // For other levels, try detection first, fallback if not available
            _ => vello_cpu::Level::try_detect().unwrap_or_else(|| vello_cpu::Level::fallback()),
        }
    }

    pub fn from_vello_level(level: vello_cpu::Level) -> Self {
        Self::from(level)
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use std::mem;

    #[test]
    fn test_sizes() {
        assert_eq!(mem::size_of::<VelloPremulRgba8>(), 4);
        assert_eq!(mem::size_of::<VelloPoint>(), 16);
        assert_eq!(mem::size_of::<VelloRect>(), 32);
        assert_eq!(mem::size_of::<VelloAffine>(), 48);
        assert_eq!(mem::size_of::<VelloStroke>(), 12);
        assert_eq!(mem::size_of::<VelloRenderSettings>(), 4);
    }

    #[test]
    fn test_alignment() {
        assert_eq!(mem::align_of::<VelloPremulRgba8>(), 1);
        assert_eq!(mem::align_of::<VelloPoint>(), 8);
        assert_eq!(mem::align_of::<VelloRect>(), 8);
        assert_eq!(mem::align_of::<VelloAffine>(), 8);
        assert_eq!(mem::align_of::<VelloStroke>(), 4);
    }
}
