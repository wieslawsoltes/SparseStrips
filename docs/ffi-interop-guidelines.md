# FFI/Interop Guidelines for Rust-C# Projects

This document outlines critical guidelines and best practices for Foreign Function Interface (FFI) and interoperability between Rust and C#, based on lessons learned from marshaling bugs discovered in the vello_cpu_ffi project.

## Table of Contents

1. [Critical Enum Representation Rules](#critical-enum-representation-rules)
2. [Struct Layout and Alignment](#struct-layout-and-alignment)
3. [Data Type Size Verification](#data-type-size-verification)
4. [Testing Strategy](#testing-strategy)
5. [Common Pitfalls](#common-pitfalls)
6. [Checklist for New FFI Types](#checklist-for-new-ffi-types)

---

## Critical Enum Representation Rules

### The Problem

**Bug Pattern**: Using `#[repr(C)]` for Rust enums when C# uses `: byte` causes silent data corruption.

```rust
// ❌ WRONG - This creates a 4-byte enum
#[repr(C)]
pub enum VelloRenderMode {
    OptimizeSpeed = 0,
    OptimizeQuality = 1,
}
```

```csharp
// C# expects 1 byte
public enum VelloRenderMode : byte {
    OptimizeSpeed = 0,
    OptimizeQuality = 1
}
```

**Impact**: When this enum is embedded in a struct, it causes memory layout misalignment. For example, a struct with an enum followed by other fields will have incorrect field offsets, leading to:
- Reading garbage data from wrong memory locations
- Silent data corruption
- Incorrect behavior (e.g., multi-threading settings ignored)
- Extremely difficult to debug issues

### The Solution

**Always use `#[repr(u8)]` in Rust when C# uses `: byte`**:

```rust
// ✅ CORRECT - This creates a 1-byte enum matching C#
#[repr(u8)]
pub enum VelloRenderMode {
    OptimizeSpeed = 0,
    OptimizeQuality = 1,
}
```

### Enum Representation Matrix

| C# Declaration | Rust Representation | Size | Notes |
|---------------|---------------------|------|-------|
| `enum Foo : byte` | `#[repr(u8)]` | 1 byte | Most common for FFI |
| `enum Foo : ushort` | `#[repr(u16)]` | 2 bytes | Less common |
| `enum Foo : uint` | `#[repr(u32)]` | 4 bytes | Avoid if possible |
| `enum Foo : int` | `#[repr(i32)]` | 4 bytes | For signed enums |

**Rule**: Match the C# underlying type exactly. Never use `#[repr(C)]` for enums crossing FFI boundaries.

---

## Struct Layout and Alignment

### Basic Struct Layout Rules

1. **Use `#[repr(C)]` for Rust structs** - This ensures C-compatible layout:

```rust
#[repr(C)]
pub struct VelloPoint {
    pub x: f64,
    pub y: f64,
}
```

2. **Use `[StructLayout(LayoutKind.Sequential)]` in C#**:

```csharp
[StructLayout(LayoutKind.Sequential)]
public struct VelloPoint {
    public double X;
    public double Y;
}
```

### Handling Padding and Alignment

When struct members have different sizes, padding may be required to maintain proper alignment.

#### Example: VelloRenderSettings

This struct contains mixed-size fields requiring careful padding:

**Rust**:
```rust
#[repr(C)]
#[derive(Copy, Clone, Debug, PartialEq, Eq)]
pub struct VelloRenderSettings {
    pub level: VelloSimdLevel,      // 1 byte (u8 enum)
    pub num_threads: u16,            // 2 bytes
    pub render_mode: VelloRenderMode, // 1 byte (u8 enum)
    pub _padding: u8,                // 1 byte explicit padding
}
```

**C# with `Pack = 1`**:
```csharp
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VelloRenderSettings {
    public VelloSimdLevel Level;      // Offset 0, 1 byte
    private byte _padding1;            // Offset 1, alignment padding
    public ushort NumThreads;          // Offset 2-3, 2 bytes
    public VelloRenderMode RenderMode; // Offset 4, 1 byte
    private byte _padding2;            // Offset 5, trailing padding
}
```

**Key Points**:
- Total size must be 6 bytes on both sides
- Use `Pack = 1` in C# to prevent automatic padding that differs from Rust
- Add explicit padding bytes to match expected layout
- Document offset positions in comments

### Alignment Rules by Type

| Type | Alignment | Notes |
|------|-----------|-------|
| `u8`/`i8`/`byte` | 1 byte | No special alignment |
| `u16`/`i16`/`ushort` | 2 bytes | Must start at even address |
| `u32`/`i32`/`uint` | 4 bytes | Must start at 4-byte boundary |
| `f32`/`float` | 4 bytes | Must start at 4-byte boundary |
| `f64`/`double` | 8 bytes | Must start at 8-byte boundary |

---

## Data Type Size Verification

### Rust Tests

**Always add compile-time size verification tests**:

```rust
#[cfg(test)]
mod tests {
    use super::*;
    use std::mem;

    #[test]
    fn test_struct_sizes() {
        // Verify struct sizes match C# expectations
        assert_eq!(mem::size_of::<VelloPremulRgba8>(), 4,
            "VelloPremulRgba8 size mismatch");
        assert_eq!(mem::size_of::<VelloPoint>(), 16,
            "VelloPoint size mismatch");
        assert_eq!(mem::size_of::<VelloRenderSettings>(), 6,
            "VelloRenderSettings size mismatch");
    }

    #[test]
    fn test_enum_sizes() {
        // All FFI enums should be 1 byte
        assert_eq!(mem::size_of::<VelloSimdLevel>(), 1,
            "VelloSimdLevel should be 1 byte");
        assert_eq!(mem::size_of::<VelloRenderMode>(), 1,
            "VelloRenderMode should be 1 byte");
    }

    #[test]
    fn test_alignment() {
        assert_eq!(mem::align_of::<VelloPremulRgba8>(), 1);
        assert_eq!(mem::align_of::<VelloPoint>(), 8);
        assert_eq!(mem::align_of::<VelloRenderSettings>(), 1);
    }
}
```

### C# Size Verification

Use `sizeof()` or `Marshal.SizeOf()` in tests:

```csharp
[Fact]
public void VerifyStructSizes() {
    Assert.Equal(4, Marshal.SizeOf<VelloPremulRgba8>());
    Assert.Equal(16, Marshal.SizeOf<VelloPoint>());
    Assert.Equal(6, Marshal.SizeOf<VelloRenderSettings>());
}
```

---

## Testing Strategy

### Echo Tests for Marshaling Verification

Create "echo" functions in Rust that return input unchanged to verify correct marshaling:

**Rust**:
```rust
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
```

**C#**:
```csharp
[DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
private static extern int vello_test_echo_render_settings(
    ref VelloRenderSettings input,
    out VelloRenderSettings output);

[Fact]
public void VelloRenderSettings_MarshalingCorrect() {
    var input = new VelloRenderSettings {
        Level = VelloSimdLevel.Avx2,
        NumThreads = 8,
        RenderMode = VelloRenderMode.OptimizeQuality
    };

    var result = vello_test_echo_render_settings(ref input, out var output);

    Assert.Equal(0, result);
    Assert.Equal(input.Level, output.Level);
    Assert.Equal(input.NumThreads, output.NumThreads);
    Assert.Equal(input.RenderMode, output.RenderMode);
}
```

### Comprehensive Test Coverage

For each FFI type, create tests for:

1. **Basic marshaling** - Echo test with typical values
2. **All enum variants** - Test every enum value can round-trip
3. **Edge values** - Test min/max values, zero, boundary conditions
4. **Multiple instances** - Test arrays or multiple calls

Example edge case test:
```csharp
[Fact]
public void VelloRenderSettings_EdgeCases_MarshalCorrectly() {
    // Test with 0 threads
    var input1 = new VelloRenderSettings {
        Level = VelloSimdLevel.Avx2,
        NumThreads = 0,
        RenderMode = VelloRenderMode.OptimizeSpeed
    };
    var result1 = vello_test_echo_render_settings(ref input1, out var output1);
    Assert.Equal(0, result1);
    Assert.Equal((ushort)0, output1.NumThreads);

    // Test with max threads
    var input2 = new VelloRenderSettings {
        Level = VelloSimdLevel.Neon,
        NumThreads = ushort.MaxValue,
        RenderMode = VelloRenderMode.OptimizeQuality
    };
    var result2 = vello_test_echo_render_settings(ref input2, out var output2);
    Assert.Equal(0, result2);
    Assert.Equal(ushort.MaxValue, output2.NumThreads);
}
```

---

## Common Pitfalls

### 1. Default Enum Representation

**Problem**: Forgetting to specify enum representation in Rust.

```rust
// ❌ WRONG - Size depends on Rust defaults (usually 4+ bytes)
pub enum MyEnum {
    Foo = 0,
    Bar = 1,
}
```

**Solution**: Always specify representation explicitly:

```rust
// ✅ CORRECT
#[repr(u8)]
pub enum MyEnum {
    Foo = 0,
    Bar = 1,
}
```

### 2. Struct Field Reordering

**Problem**: C# and Rust may reorder struct fields differently.

**Solution**: Always use `#[repr(C)]` in Rust and `LayoutKind.Sequential` in C#.

### 3. Boolean Types

**Problem**: Rust `bool` is 1 byte, C# `bool` is 4 bytes by default in marshaling.

**Solution**: Use explicit byte types or `[MarshalAs(UnmanagedType.I1)]`:

```csharp
// Option 1: Use byte
public struct Foo {
    public byte IsEnabled; // 0 or 1
}

// Option 2: Marshal bool explicitly
public struct Foo {
    [MarshalAs(UnmanagedType.I1)]
    public bool IsEnabled;
}
```

### 4. String Handling

**Problem**: String marshaling is complex and error-prone.

**Solution**: Use raw byte pointers or fixed-length buffers for FFI:

```rust
// Rust - return string via output buffer
#[no_mangle]
pub extern "C" fn get_version(buffer: *mut u8, len: usize) -> c_int {
    let version = b"1.0.0\0";
    if len < version.len() { return -1; }
    unsafe {
        std::ptr::copy_nonoverlapping(version.as_ptr(), buffer, version.len());
    }
    0
}
```

```csharp
// C# - use byte buffer and convert
[DllImport(LibName)]
private static extern int get_version(byte[] buffer, int len);

public static string GetVersion() {
    var buffer = new byte[256];
    get_version(buffer, buffer.Length);
    return Encoding.UTF8.GetString(buffer).TrimEnd('\0');
}
```

### 5. Pointer Ownership

**Problem**: Unclear ownership leads to memory leaks or use-after-free.

**Solution**: Document ownership clearly:

```rust
/// Creates a new context. Caller must call vello_context_destroy() to free.
#[no_mangle]
pub extern "C" fn vello_context_create() -> *mut VelloRenderContext {
    // Returns owned pointer
}

/// Destroys a context. Pointer must not be used after this call.
#[no_mangle]
pub extern "C" fn vello_context_destroy(ctx: *mut VelloRenderContext) {
    // Takes ownership and frees
}
```

### 6. Thread Safety

**Problem**: C# may call FFI functions from any thread.

**Solution**: Use thread-safe Rust types or document thread requirements:

```rust
// Use thread-safe types
pub struct Context {
    data: Arc<Mutex<ContextData>>,
}

// Or document thread requirements
/// WARNING: This function is not thread-safe.
/// Caller must ensure only one thread accesses this context.
#[no_mangle]
pub extern "C" fn vello_context_render(...) { }
```

---

## Checklist for New FFI Types

When adding a new type to the FFI boundary, verify:

### For Enums:

- [ ] Rust enum uses `#[repr(u8)]` (or appropriate size matching C#)
- [ ] C# enum uses `: byte` (or matching size)
- [ ] Size test added in Rust (`mem::size_of::<Enum>() == 1`)
- [ ] Echo test created and passing
- [ ] All enum variants tested

### For Structs:

- [ ] Rust struct uses `#[repr(C)]`
- [ ] C# struct uses `[StructLayout(LayoutKind.Sequential)]`
- [ ] Padding bytes added if needed (different-sized fields)
- [ ] Size test added in Rust
- [ ] Alignment test added in Rust
- [ ] C# size verification test added
- [ ] Echo test created and passing
- [ ] Edge case tests for min/max values

### For Functions:

- [ ] Uses `#[no_mangle]`
- [ ] Uses `extern "C"` calling convention
- [ ] Null pointer checks for all pointer parameters
- [ ] Error code return value (prefer `c_int` over exceptions)
- [ ] Ownership documented (who frees allocated memory)
- [ ] Thread safety documented
- [ ] C# P/Invoke declaration matches exactly

### General:

- [ ] No undefined behavior in unsafe code
- [ ] Panic catching if Rust might panic (use `std::panic::catch_unwind`)
- [ ] Error handling path tested
- [ ] Documentation includes C# usage example

---

## Example: Complete Type Definition

Here's a complete example showing all guidelines applied:

**Rust** (`types.rs`):
```rust
/// Render mode enumeration
#[repr(u8)]  // ✅ Explicit size
#[derive(Copy, Clone, Debug, PartialEq, Eq)]
pub enum VelloRenderMode {
    OptimizeSpeed = 0,
    OptimizeQuality = 1,
}

/// Render settings
#[repr(C)]  // ✅ C-compatible layout
#[derive(Copy, Clone, Debug, PartialEq, Eq)]
pub struct VelloRenderSettings {
    pub level: VelloSimdLevel,
    pub num_threads: u16,
    pub render_mode: VelloRenderMode,
    pub _padding: u8,  // ✅ Explicit padding
}

// ✅ Size verification tests
#[cfg(test)]
mod tests {
    use super::*;
    use std::mem;

    #[test]
    fn test_sizes() {
        assert_eq!(mem::size_of::<VelloRenderMode>(), 1);
        assert_eq!(mem::size_of::<VelloRenderSettings>(), 6);
    }
}
```

**Rust** (`marshaling_tests.rs`):
```rust
/// Test function: echo back VelloRenderSettings
#[no_mangle]  // ✅ No name mangling
pub extern "C" fn vello_test_echo_render_settings(  // ✅ C calling convention
    input: *const VelloRenderSettings,
    output: *mut VelloRenderSettings,
) -> c_int {
    // ✅ Null checks
    if input.is_null() || output.is_null() {
        return VELLO_ERROR_NULL_POINTER;
    }

    unsafe {
        *output = *input;
    }
    VELLO_OK
}
```

**C#** (`NativeEnums.cs`):
```csharp
/// <summary>
/// Render mode enumeration
/// </summary>
public enum VelloRenderMode : byte  // ✅ Explicit byte size
{
    OptimizeSpeed = 0,
    OptimizeQuality = 1
}
```

**C#** (`NativeStructures.cs`):
```csharp
/// <summary>
/// Render settings
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]  // ✅ Packed layout
public struct VelloRenderSettings
{
    public VelloSimdLevel Level;      // Offset 0, 1 byte
    private byte _padding1;            // Offset 1
    public ushort NumThreads;          // Offset 2-3, 2 bytes
    public VelloRenderMode RenderMode; // Offset 4, 1 byte
    private byte _padding2;            // Offset 5
}
```

**C#** (`MarshalingTests.cs`):
```csharp
public class MarshalingTests
{
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    private static extern int vello_test_echo_render_settings(
        ref VelloRenderSettings input,
        out VelloRenderSettings output);

    [Fact]
    public void VelloRenderSettings_MarshalingCorrect()
    {
        var input = new VelloRenderSettings
        {
            Level = VelloSimdLevel.Avx2,
            NumThreads = 8,
            RenderMode = VelloRenderMode.OptimizeQuality
        };

        var result = vello_test_echo_render_settings(ref input, out var output);

        Assert.Equal(0, result);
        Assert.Equal(input.Level, output.Level);
        Assert.Equal(input.NumThreads, output.NumThreads);
        Assert.Equal(input.RenderMode, output.RenderMode);
    }

    [Fact]
    public void VelloRenderSettings_AllRenderModes_MarshalCorrectly()
    {
        foreach (VelloRenderMode mode in Enum.GetValues<VelloRenderMode>())
        {
            var input = new VelloRenderSettings
            {
                Level = VelloSimdLevel.Fallback,
                NumThreads = 1,
                RenderMode = mode
            };

            var result = vello_test_echo_render_settings(ref input, out var output);

            Assert.Equal(0, result);
            Assert.Equal(mode, output.RenderMode);
        }
    }
}
```

---

## Quick Reference

### Must Remember

1. **Enums**: Use `#[repr(u8)]` in Rust, `: byte` in C#
2. **Structs**: Use `#[repr(C)]` in Rust, `LayoutKind.Sequential` in C#
3. **Size Matters**: Verify with tests on both sides
4. **Test Everything**: Echo tests catch marshaling bugs early

### Common Fixes

| Problem | Rust Fix | C# Fix |
|---------|----------|--------|
| Enum size mismatch | Change to `#[repr(u8)]` | Use `: byte` |
| Struct padding | Add explicit `_padding` fields | Use `Pack = 1` |
| Size not matching | Verify with `mem::size_of!()` | Use `Marshal.SizeOf()` |

---

## Conclusion

FFI marshaling bugs are:
- **Silent** - No compiler errors or warnings
- **Dangerous** - Can corrupt data or cause undefined behavior
- **Hard to debug** - Symptoms appear far from root cause

Following these guidelines and implementing comprehensive tests prevents these issues from reaching production.

**Golden Rule**: When in doubt, add a test. Echo tests are cheap insurance against marshaling bugs.
