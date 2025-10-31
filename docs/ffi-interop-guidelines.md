# FFI/Interop Guidelines for Rust-C# Projects

This document outlines critical guidelines and best practices for Foreign Function Interface (FFI) and interoperability between Rust and C#, based on lessons learned from marshaling bugs discovered in the vello_cpu_ffi project.

## Table of Contents

1. [Critical Enum Representation Rules](#critical-enum-representation-rules)
2. [Struct Layout and Alignment](#struct-layout-and-alignment)
3. [Data Type Size Verification](#data-type-size-verification)
4. [Method Binding Guidelines](#method-binding-guidelines)
5. [Testing Strategy](#testing-strategy)
6. [Common Pitfalls](#common-pitfalls)
7. [Checklist for New FFI Types](#checklist-for-new-ffi-types)

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

## Method Binding Guidelines

This section covers comprehensive guidelines for declaring and binding FFI methods between Rust and C#, including calling conventions, parameter passing, return values, and error handling.

### 4.1. Basic FFI Function Declaration

#### Rust Side

Every FFI function in Rust must follow this pattern:

```rust
#[no_mangle]  // Prevent name mangling for C compatibility
pub extern "C" fn function_name(params...) -> return_type {
    // Implementation
}
```

**Key Requirements**:
1. `#[no_mangle]` - Prevents Rust from mangling the function name
2. `pub` - Makes function visible outside the module
3. `extern "C"` - Uses C calling convention (stack cleanup by caller)
4. Function name should use snake_case with module prefix (e.g., `vello_context_new`)

#### C# Side (Modern: LibraryImport)

```csharp
[LibraryImport("library_name", EntryPoint = "function_name")]
public static partial ReturnType FunctionName(params...);
```

**Key Requirements**:
1. `[LibraryImport]` - Modern source-generated P/Invoke (.NET 7+)
2. `EntryPoint` - Specifies exact native function name
3. `partial` - Required for source generation
4. Method name should use PascalCase (C# convention)

#### C# Side (Legacy: DllImport)

```csharp
[DllImport("library_name", EntryPoint = "function_name",
    CallingConvention = CallingConvention.Cdecl)]
public static extern ReturnType FunctionName(params...);
```

**Notes**:
- Use `CallingConvention.Cdecl` to match Rust's `extern "C"`
- `LibraryImport` is preferred for new code (better performance, AOT support)

### 4.2. Parameter Passing Patterns

#### Pattern 1: Value Types (Pass by Value)

**Simple value types** (primitives, small structs):

```rust
#[no_mangle]
pub extern "C" fn vello_add(a: i32, b: i32) -> i32 {
    a + b
}
```

```csharp
[LibraryImport(LibName, EntryPoint = "vello_add")]
public static partial int Add(int a, int b);
```

**Size limits**: Only pass small structs by value (≤16 bytes). Larger structs should use pointers.

#### Pattern 2: Input Parameters (Pass by Pointer - Read Only)

Use `*const T` in Rust, `ref` or `in` in C#:

```rust
#[no_mangle]
pub extern "C" fn vello_context_set_transform(
    ctx: *mut VelloRenderContext,
    transform: *const VelloAffine,  // Read-only input
) -> c_int {
    if ctx.is_null() || transform.is_null() {
        return VELLO_ERROR_NULL_POINTER;
    }

    unsafe {
        let ctx_ref = &mut *ctx;
        let transform_ref = &*transform;
        ctx_ref.set_transform(*transform_ref);
    }
    VELLO_OK
}
```

```csharp
[LibraryImport(LibName, EntryPoint = "vello_context_set_transform")]
public static partial int RenderContext_SetTransform(
    nint ctx,
    ref VelloAffine transform);  // or: in VelloAffine transform

// Usage:
var transform = new VelloAffine { ... };
RenderContext_SetTransform(ctx, ref transform);
```

**Guidelines**:
- Use `ref` for explicit reference passing
- Use `in` for read-only reference (C# 7.2+, prevents accidental modification)
- Always null-check pointers in Rust

#### Pattern 3: Output Parameters (Pass by Pointer - Write Only)

Use `*mut T` in Rust, `out` in C#:

```rust
#[no_mangle]
pub extern "C" fn vello_context_get_transform(
    ctx: *const VelloRenderContext,
    out_transform: *mut VelloAffine,  // Output parameter
) -> c_int {
    if ctx.is_null() || out_transform.is_null() {
        return VELLO_ERROR_NULL_POINTER;
    }

    unsafe {
        let ctx_ref = &*ctx;
        *out_transform = ctx_ref.get_transform();
    }
    VELLO_OK
}
```

```csharp
[LibraryImport(LibName, EntryPoint = "vello_context_get_transform")]
public static partial int RenderContext_GetTransform(
    nint ctx,
    out VelloAffine transform);

// Usage:
RenderContext_GetTransform(ctx, out var transform);
// transform is now populated
```

**Guidelines**:
- Use `out` for output-only parameters (C# doesn't require initialization)
- Always null-check in Rust before writing
- Document that output pointer must be valid

#### Pattern 4: Arrays and Buffers

**Fixed-size buffer output**:

```rust
#[no_mangle]
pub extern "C" fn vello_get_version(
    buffer: *mut u8,
    buffer_len: usize,
) -> c_int {
    let version = b"1.0.0\0";

    if buffer.is_null() {
        return VELLO_ERROR_NULL_POINTER;
    }

    if buffer_len < version.len() {
        return VELLO_ERROR_INVALID_PARAMETER;
    }

    unsafe {
        std::ptr::copy_nonoverlapping(
            version.as_ptr(),
            buffer,
            version.len()
        );
    }
    VELLO_OK
}
```

```csharp
[LibraryImport(LibName, EntryPoint = "vello_get_version")]
public static partial int GetVersion(byte[] buffer, nuint bufferLen);

// Usage:
var buffer = new byte[256];
GetVersion(buffer, (nuint)buffer.Length);
string version = Encoding.UTF8.GetString(buffer).TrimEnd('\0');
```

**Array input**:

```rust
#[no_mangle]
pub extern "C" fn vello_context_fill_glyphs(
    ctx: *mut VelloRenderContext,
    font: *const FontData,
    font_size: f32,
    glyphs: *const VelloGlyph,  // Array input
    glyph_count: usize,
) -> c_int {
    if ctx.is_null() || font.is_null() || glyphs.is_null() {
        return VELLO_ERROR_NULL_POINTER;
    }

    unsafe {
        let ctx_ref = &mut *ctx;
        let font_ref = &*font;
        let glyphs_slice = std::slice::from_raw_parts(glyphs, glyph_count);

        ctx_ref.fill_glyphs(font_ref, font_size, glyphs_slice);
    }
    VELLO_OK
}
```

```csharp
[LibraryImport(LibName, EntryPoint = "vello_context_fill_glyphs")]
public static unsafe partial int RenderContext_FillGlyphs(
    nint ctx,
    nint font,
    float fontSize,
    VelloGlyph* glyphs,    // Pointer to array
    nuint glyphCount);

// Usage - Option 1: Fixed buffer
VelloGlyph[] glyphs = new VelloGlyph[100];
// ... populate glyphs
fixed (VelloGlyph* pGlyphs = glyphs)
{
    RenderContext_FillGlyphs(ctx, font, 16.0f, pGlyphs, (nuint)glyphs.Length);
}

// Usage - Option 2: Span (modern)
Span<VelloGlyph> glyphs = stackalloc VelloGlyph[100];
// ... populate glyphs
fixed (VelloGlyph* pGlyphs = glyphs)
{
    RenderContext_FillGlyphs(ctx, font, 16.0f, pGlyphs, (nuint)glyphs.Length);
}
```

#### Pattern 5: Opaque Handles (Object Lifetime Management)

For complex objects, return opaque pointers (handles):

```rust
pub struct VelloRenderContext {
    // Internal implementation hidden from C#
    width: u16,
    height: u16,
    // ... more fields
}

/// Creates a new render context. Caller must call vello_render_context_free().
#[no_mangle]
pub extern "C" fn vello_render_context_new(
    width: u16,
    height: u16,
) -> *mut VelloRenderContext {
    let ctx = Box::new(VelloRenderContext::new(width, height));
    Box::into_raw(ctx)  // Transfer ownership to caller
}

/// Destroys a render context. Handle becomes invalid after this call.
#[no_mangle]
pub extern "C" fn vello_render_context_free(ctx: *mut VelloRenderContext) {
    if ctx.is_null() {
        return;
    }
    unsafe {
        let _ = Box::from_raw(ctx);  // Takes ownership and drops
    }
}

/// Uses the context (context remains valid after call)
#[no_mangle]
pub extern "C" fn vello_render_context_width(
    ctx: *const VelloRenderContext
) -> u16 {
    if ctx.is_null() {
        return 0;
    }
    unsafe {
        (*ctx).width
    }
}
```

```csharp
[LibraryImport(LibName, EntryPoint = "vello_render_context_new")]
public static partial nint RenderContext_New(ushort width, ushort height);

[LibraryImport(LibName, EntryPoint = "vello_render_context_free")]
public static partial void RenderContext_Free(nint ctx);

[LibraryImport(LibName, EntryPoint = "vello_render_context_width")]
public static partial ushort RenderContext_Width(nint ctx);

// Safe wrapper class (recommended)
public class RenderContext : IDisposable
{
    private nint _handle;
    private bool _disposed;

    public RenderContext(ushort width, ushort height)
    {
        _handle = NativeMethods.RenderContext_New(width, height);
        if (_handle == 0)
            throw new InvalidOperationException("Failed to create context");
    }

    public ushort Width
    {
        get
        {
            ThrowIfDisposed();
            return NativeMethods.RenderContext_Width(_handle);
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_handle != 0)
            {
                NativeMethods.RenderContext_Free(_handle);
                _handle = 0;
            }
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    ~RenderContext()
    {
        Dispose();
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(RenderContext));
    }

    internal nint Handle
    {
        get
        {
            ThrowIfDisposed();
            return _handle;
        }
    }
}
```

**Guidelines for Opaque Handles**:
1. **Creation**: Use `Box::into_raw()` to transfer ownership to C#
2. **Destruction**: Use `Box::from_raw()` to reclaim ownership and drop
3. **Usage**: Borrow reference with `&*ptr` or `&mut *ptr`, never take ownership
4. **C# Wrapper**: Always provide a managed wrapper implementing `IDisposable`
5. **Null Checks**: Always check for null before dereferencing in Rust
6. **Documentation**: Clearly state ownership and lifetime requirements

### 4.3. Return Values and Error Handling

#### Pattern 1: Integer Error Codes (Recommended)

Return `c_int` (i32) with error codes:

```rust
use std::os::raw::c_int;

// Error code constants (should match C#)
pub const VELLO_OK: c_int = 0;
pub const VELLO_ERROR_NULL_POINTER: c_int = -1;
pub const VELLO_ERROR_INVALID_HANDLE: c_int = -2;
pub const VELLO_ERROR_RENDER_FAILED: c_int = -3;
pub const VELLO_ERROR_OUT_OF_MEMORY: c_int = -4;

#[no_mangle]
pub extern "C" fn vello_context_render(
    ctx: *mut VelloRenderContext,
) -> c_int {
    if ctx.is_null() {
        return VELLO_ERROR_NULL_POINTER;
    }

    unsafe {
        match (*ctx).render() {
            Ok(_) => VELLO_OK,
            Err(e) => {
                // Optionally store error in thread-local storage
                set_last_error(e);
                VELLO_ERROR_RENDER_FAILED
            }
        }
    }
}
```

```csharp
public static class NativeMethods
{
    public const int VELLO_OK = 0;
    public const int VELLO_ERROR_NULL_POINTER = -1;
    public const int VELLO_ERROR_INVALID_HANDLE = -2;
    public const int VELLO_ERROR_RENDER_FAILED = -3;

    [LibraryImport(LibName, EntryPoint = "vello_context_render")]
    public static partial int RenderContext_Render(nint ctx);
}

// High-level wrapper
public class RenderContext
{
    public void Render()
    {
        int result = NativeMethods.RenderContext_Render(_handle);
        if (result != NativeMethods.VELLO_OK)
        {
            throw new VelloException($"Render failed with error code: {result}");
        }
    }
}
```

**Advantages**:
- Simple and fast
- No exceptions across FFI boundary
- Easy to check multiple error conditions

#### Pattern 2: Last Error Pattern

Store detailed error info in thread-local storage:

```rust
use std::cell::RefCell;

thread_local! {
    static LAST_ERROR: RefCell<Option<String>> = RefCell::new(None);
}

fn set_last_error(err: impl ToString) {
    LAST_ERROR.with(|e| {
        *e.borrow_mut() = Some(err.to_string());
    });
}

#[no_mangle]
pub extern "C" fn vello_get_last_error() -> *const u8 {
    LAST_ERROR.with(|e| {
        match &*e.borrow() {
            Some(err) => err.as_ptr(),
            None => std::ptr::null(),
        }
    })
}

#[no_mangle]
pub extern "C" fn vello_clear_last_error() {
    LAST_ERROR.with(|e| {
        *e.borrow_mut() = None;
    });
}
```

```csharp
[LibraryImport(LibName, EntryPoint = "vello_get_last_error")]
public static partial nint GetLastError();

[LibraryImport(LibName, EntryPoint = "vello_clear_last_error")]
public static partial void ClearLastError();

public static string? GetLastErrorMessage()
{
    nint ptr = GetLastError();
    if (ptr == 0)
        return null;

    return Marshal.PtrToStringUTF8(ptr);
}
```

#### Pattern 3: Boolean Success Indicator

For simple success/failure:

```rust
#[no_mangle]
pub extern "C" fn vello_context_is_valid(ctx: *const VelloRenderContext) -> u8 {
    if ctx.is_null() {
        return 0;  // false
    }
    unsafe {
        if (*ctx).is_valid() { 1 } else { 0 }
    }
}
```

```csharp
[LibraryImport(LibName, EntryPoint = "vello_context_is_valid")]
[return: MarshalAs(UnmanagedType.U1)]
public static partial bool RenderContext_IsValid(nint ctx);
```

### 4.4. String Handling

#### Pattern 1: Return Static String (Rust-owned, read-only)

```rust
#[no_mangle]
pub extern "C" fn vello_version() -> *const u8 {
    // Static string, valid for program lifetime
    b"1.0.0\0".as_ptr()
}
```

```csharp
[LibraryImport(LibName, EntryPoint = "vello_version")]
public static partial nint Version();

public static string GetVersion()
{
    nint ptr = Version();
    return Marshal.PtrToStringUTF8(ptr) ?? string.Empty;
}
```

#### Pattern 2: Output Buffer (Caller-allocated)

```rust
#[no_mangle]
pub extern "C" fn vello_get_name(
    buffer: *mut u8,
    buffer_len: usize,
) -> c_int {
    let name = b"VelloCPU\0";

    if buffer.is_null() {
        return VELLO_ERROR_NULL_POINTER;
    }

    if buffer_len < name.len() {
        return -(name.len() as c_int);  // Return required size as negative
    }

    unsafe {
        std::ptr::copy_nonoverlapping(name.as_ptr(), buffer, name.len());
    }
    name.len() as c_int  // Return bytes written
}
```

```csharp
[LibraryImport(LibName, EntryPoint = "vello_get_name")]
public static partial int GetName(byte[] buffer, nuint bufferLen);

public static string GetName()
{
    var buffer = new byte[256];
    int result = GetName(buffer, (nuint)buffer.Length);

    if (result < 0)
    {
        // Buffer too small, retry with required size
        buffer = new byte[-result];
        result = GetName(buffer, (nuint)buffer.Length);
    }

    return Encoding.UTF8.GetString(buffer, 0, result).TrimEnd('\0');
}
```

#### Pattern 3: Allocated String (Rust-owned, caller must free)

```rust
#[no_mangle]
pub extern "C" fn vello_format_error(error_code: c_int) -> *mut u8 {
    let message = format!("Error code: {}\0", error_code);
    let bytes = message.into_bytes();
    let ptr = bytes.as_ptr() as *mut u8;
    std::mem::forget(bytes);  // Don't drop, C# will free it
    ptr
}

#[no_mangle]
pub extern "C" fn vello_string_free(ptr: *mut u8, len: usize) {
    if ptr.is_null() {
        return;
    }
    unsafe {
        let _ = Vec::from_raw_parts(ptr, len, len);  // Reconstruct and drop
    }
}
```

```csharp
[LibraryImport(LibName, EntryPoint = "vello_format_error")]
public static partial nint FormatError(int errorCode);

[LibraryImport(LibName, EntryPoint = "vello_string_free")]
public static partial void StringFree(nint ptr, nuint len);

public static string FormatError(int errorCode)
{
    nint ptr = FormatError(errorCode);
    if (ptr == 0)
        return string.Empty;

    string result = Marshal.PtrToStringUTF8(ptr) ?? string.Empty;

    // Free the allocated string
    StringFree(ptr, (nuint)result.Length + 1);

    return result;
}
```

### 4.5. Callback Functions (C# to Rust)

For callbacks from C# to Rust:

```rust
// Define callback type
pub type ProgressCallback = extern "C" fn(progress: f32, user_data: *mut c_void);

#[no_mangle]
pub extern "C" fn vello_render_with_progress(
    ctx: *mut VelloRenderContext,
    callback: Option<ProgressCallback>,
    user_data: *mut c_void,
) -> c_int {
    if ctx.is_null() {
        return VELLO_ERROR_NULL_POINTER;
    }

    unsafe {
        let ctx_ref = &mut *ctx;

        // Report progress
        if let Some(cb) = callback {
            cb(0.0, user_data);  // Starting
        }

        // Do work...

        if let Some(cb) = callback {
            cb(0.5, user_data);  // Halfway
        }

        // More work...

        if let Some(cb) = callback {
            cb(1.0, user_data);  // Complete
        }
    }

    VELLO_OK
}
```

```csharp
// Define matching delegate
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void ProgressCallback(float progress, nint userData);

[LibraryImport(LibName, EntryPoint = "vello_render_with_progress")]
public static partial int RenderContext_RenderWithProgress(
    nint ctx,
    ProgressCallback? callback,
    nint userData);

// Usage
public void RenderWithProgress(Action<float>? progressHandler)
{
    // Keep delegate alive during call
    ProgressCallback? callback = null;
    if (progressHandler != null)
    {
        callback = (progress, _) => progressHandler(progress);
    }

    int result = NativeMethods.RenderContext_RenderWithProgress(
        _handle,
        callback,
        0);

    if (result != NativeMethods.VELLO_OK)
        throw new VelloException($"Render failed: {result}");

    // Keep callback alive until here
    GC.KeepAlive(callback);
}
```

**Important**: Store delegates as fields if they're used across multiple calls to prevent GC collection.

### 4.6. Thread Safety Considerations

#### Pattern 1: Thread-Safe Functions

```rust
use std::sync::{Arc, Mutex};

pub struct VelloRenderContext {
    data: Arc<Mutex<ContextData>>,
}

#[no_mangle]
pub extern "C" fn vello_context_render(ctx: *mut VelloRenderContext) -> c_int {
    if ctx.is_null() {
        return VELLO_ERROR_NULL_POINTER;
    }

    unsafe {
        let ctx_ref = &*ctx;
        // Lock is automatically released when guard drops
        let mut data = ctx_ref.data.lock().unwrap();
        data.render();
    }

    VELLO_OK
}
```

Document thread safety:
```rust
/// Thread-safe: This function can be called from any thread.
/// Multiple threads can access the same context concurrently.
#[no_mangle]
pub extern "C" fn vello_context_render(ctx: *mut VelloRenderContext) -> c_int {
    // ...
}
```

#### Pattern 2: Thread-Unsafe Functions (Document!)

```rust
/// WARNING: NOT thread-safe!
/// This function must only be called from one thread at a time.
/// The same context must not be accessed concurrently from multiple threads.
#[no_mangle]
pub extern "C" fn vello_context_render_fast(ctx: *mut VelloRenderContext) -> c_int {
    // No locking for performance
    // ...
}
```

### 4.7. Type Mapping Reference

| Rust Type | C# Type | Notes |
|-----------|---------|-------|
| `i8` | `sbyte` | 8-bit signed |
| `u8` | `byte` | 8-bit unsigned |
| `i16` | `short` | 16-bit signed |
| `u16` | `ushort` | 16-bit unsigned |
| `i32` / `c_int` | `int` | 32-bit signed |
| `u32` | `uint` | 32-bit unsigned |
| `i64` | `long` | 64-bit signed |
| `u64` | `ulong` | 64-bit unsigned |
| `isize` | `nint` | Pointer-sized signed |
| `usize` | `nuint` | Pointer-sized unsigned |
| `f32` | `float` | 32-bit float |
| `f64` | `double` | 64-bit float |
| `*const T` | `ref T` or `in T` | Read-only pointer |
| `*mut T` | `ref T` or `out T` | Mutable pointer |
| `*mut T` (array) | `T*` (unsafe) | Array pointer |
| `*mut c_void` | `nint` | Opaque pointer/handle |
| `bool` (avoid) | `byte` + conversion | Use `u8` (0/1) |
| `()` (unit) | `void` | No return value |

### 4.8. Complete Example: CRUD Operations

Here's a complete example showing all patterns together:

**Rust** (`lib.rs`):
```rust
use std::os::raw::c_int;
use std::ffi::CStr;

pub struct Database {
    items: Vec<String>,
}

const DB_OK: c_int = 0;
const DB_ERROR_NULL: c_int = -1;
const DB_ERROR_NOT_FOUND: c_int = -2;

/// Create a new database
#[no_mangle]
pub extern "C" fn db_create() -> *mut Database {
    Box::into_raw(Box::new(Database { items: Vec::new() }))
}

/// Free a database
#[no_mangle]
pub extern "C" fn db_free(db: *mut Database) {
    if !db.is_null() {
        unsafe { let _ = Box::from_raw(db); }
    }
}

/// Add an item (string input)
#[no_mangle]
pub extern "C" fn db_add(
    db: *mut Database,
    item: *const u8,
) -> c_int {
    if db.is_null() || item.is_null() {
        return DB_ERROR_NULL;
    }

    unsafe {
        let db_ref = &mut *db;
        let item_str = CStr::from_ptr(item as *const i8)
            .to_str()
            .unwrap()
            .to_string();
        db_ref.items.push(item_str);
    }

    DB_OK
}

/// Get item count
#[no_mangle]
pub extern "C" fn db_count(db: *const Database) -> usize {
    if db.is_null() {
        return 0;
    }
    unsafe { (*db).items.len() }
}

/// Get item by index (output buffer pattern)
#[no_mangle]
pub extern "C" fn db_get(
    db: *const Database,
    index: usize,
    buffer: *mut u8,
    buffer_len: usize,
) -> c_int {
    if db.is_null() || buffer.is_null() {
        return DB_ERROR_NULL;
    }

    unsafe {
        let db_ref = &*db;

        if index >= db_ref.items.len() {
            return DB_ERROR_NOT_FOUND;
        }

        let item = &db_ref.items[index];
        let bytes = item.as_bytes();

        if buffer_len < bytes.len() + 1 {
            return -(bytes.len() as c_int + 1);
        }

        std::ptr::copy_nonoverlapping(bytes.as_ptr(), buffer, bytes.len());
        *buffer.add(bytes.len()) = 0;  // Null terminator

        bytes.len() as c_int
    }
}

/// Delete item by index
#[no_mangle]
pub extern "C" fn db_delete(
    db: *mut Database,
    index: usize,
) -> c_int {
    if db.is_null() {
        return DB_ERROR_NULL;
    }

    unsafe {
        let db_ref = &mut *db;

        if index >= db_ref.items.len() {
            return DB_ERROR_NOT_FOUND;
        }

        db_ref.items.remove(index);
    }

    DB_OK
}
```

**C#** (`Database.cs`):
```csharp
using System.Runtime.InteropServices;
using System.Text;

public static class NativeMethods
{
    private const string LibName = "database";

    public const int DB_OK = 0;
    public const int DB_ERROR_NULL = -1;
    public const int DB_ERROR_NOT_FOUND = -2;

    [LibraryImport(LibName, EntryPoint = "db_create")]
    public static partial nint Create();

    [LibraryImport(LibName, EntryPoint = "db_free")]
    public static partial void Free(nint db);

    [LibraryImport(LibName, EntryPoint = "db_add")]
    public static partial int Add(nint db, nint item);

    [LibraryImport(LibName, EntryPoint = "db_count")]
    public static partial nuint Count(nint db);

    [LibraryImport(LibName, EntryPoint = "db_get")]
    public static partial int Get(nint db, nuint index, byte[] buffer, nuint bufferLen);

    [LibraryImport(LibName, EntryPoint = "db_delete")]
    public static partial int Delete(nint db, nuint index);
}

public class Database : IDisposable
{
    private nint _handle;
    private bool _disposed;

    public Database()
    {
        _handle = NativeMethods.Create();
        if (_handle == 0)
            throw new InvalidOperationException("Failed to create database");
    }

    public void Add(string item)
    {
        ThrowIfDisposed();

        var bytes = Encoding.UTF8.GetBytes(item + "\0");
        fixed (byte* ptr = bytes)
        {
            int result = NativeMethods.Add(_handle, (nint)ptr);
            if (result != NativeMethods.DB_OK)
                throw new DatabaseException($"Add failed: {result}");
        }
    }

    public int Count
    {
        get
        {
            ThrowIfDisposed();
            return (int)NativeMethods.Count(_handle);
        }
    }

    public string Get(int index)
    {
        ThrowIfDisposed();

        var buffer = new byte[256];
        int result = NativeMethods.Get(_handle, (nuint)index, buffer, (nuint)buffer.Length);

        if (result < 0)
        {
            if (result == NativeMethods.DB_ERROR_NOT_FOUND)
                throw new IndexOutOfRangeException();

            // Buffer too small
            buffer = new byte[-result];
            result = NativeMethods.Get(_handle, (nuint)index, buffer, (nuint)buffer.Length);
        }

        return Encoding.UTF8.GetString(buffer, 0, result);
    }

    public void Delete(int index)
    {
        ThrowIfDisposed();

        int result = NativeMethods.Delete(_handle, (nuint)index);
        if (result == NativeMethods.DB_ERROR_NOT_FOUND)
            throw new IndexOutOfRangeException();
        else if (result != NativeMethods.DB_OK)
            throw new DatabaseException($"Delete failed: {result}");
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_handle != 0)
            {
                NativeMethods.Free(_handle);
                _handle = 0;
            }
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    ~Database() => Dispose();

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(Database));
    }
}

// Usage:
using (var db = new Database())
{
    db.Add("Item 1");
    db.Add("Item 2");
    Console.WriteLine($"Count: {db.Count}");
    Console.WriteLine($"Item 0: {db.Get(0)}");
    db.Delete(0);
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

- [ ] **Rust Declaration:**
  - [ ] Uses `#[no_mangle]` attribute
  - [ ] Uses `pub extern "C"` calling convention
  - [ ] Function name uses snake_case with module prefix
  - [ ] Null pointer checks for all pointer parameters
  - [ ] Error code return value (prefer `c_int` over exceptions)
  - [ ] Panic handling with `catch_unwind` if needed
  - [ ] Proper unsafe blocks with safety comments
- [ ] **C# Declaration:**
  - [ ] Uses `[LibraryImport]` (preferred) or `[DllImport]`
  - [ ] `EntryPoint` matches Rust function name exactly
  - [ ] `CallingConvention.Cdecl` specified (if using DllImport)
  - [ ] Return type matches Rust exactly
  - [ ] All parameter types match Rust exactly
  - [ ] Correct use of `ref`, `in`, `out` for pointers
  - [ ] `unsafe` keyword if using pointers directly
- [ ] **Documentation:**
  - [ ] Ownership documented (who allocates/frees memory)
  - [ ] Thread safety explicitly documented
  - [ ] Null pointer handling documented
  - [ ] Error codes documented
  - [ ] Parameter validation requirements documented
  - [ ] C# usage example provided
- [ ] **Testing:**
  - [ ] Basic success case tested
  - [ ] Null pointer handling tested
  - [ ] Error conditions tested
  - [ ] Edge cases tested
  - [ ] Memory leaks checked (allocation/free pairs)

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
| Function not found | Add `#[no_mangle]` | Check `EntryPoint` matches |
| Wrong calling convention | Use `extern "C"` | Use `CallingConvention.Cdecl` |
| Parameter mismatch | Check pointer types (`*const`/`*mut`) | Use `ref`/`in`/`out` correctly |
| String marshaling error | Use byte buffers | Use `byte[]` with UTF8 encoding |
| Memory leak | Ensure `Box::from_raw()` for cleanup | Call free functions in `Dispose()` |
| Callback crashes | Store callback properly | Use `GC.KeepAlive()` or field storage |

### Method Binding Patterns Quick Reference

| Use Case | Rust Pattern | C# Pattern |
|----------|--------------|------------|
| Simple value | `fn foo(x: i32) -> i32` | `int Foo(int x)` |
| Input struct | `fn foo(data: *const Data)` | `Foo(ref Data data)` or `Foo(in Data data)` |
| Output struct | `fn foo(out: *mut Data)` | `Foo(out Data data)` |
| Array input | `fn foo(arr: *const T, len: usize)` | `Foo(T* arr, nuint len)` (unsafe) |
| String input | `fn foo(s: *const u8)` | `Foo(byte[] s)` with UTF8+null |
| String output | `fn foo(buf: *mut u8, len: usize)` | `Foo(byte[] buf, nuint len)` |
| Create object | `fn create() -> *mut Obj` | `nint Create()` + wrapper |
| Destroy object | `fn destroy(obj: *mut Obj)` | `void Destroy(nint obj)` in `Dispose()` |
| Error handling | Return `c_int` (0=OK, <0=error) | Check result, throw on error |
| Callback | `fn foo(cb: Option<extern "C" fn(...)>)` | Delegate + `GC.KeepAlive()` |

---

## Conclusion

FFI marshaling bugs are:
- **Silent** - No compiler errors or warnings
- **Dangerous** - Can corrupt data or cause undefined behavior
- **Hard to debug** - Symptoms appear far from root cause

Following these guidelines and implementing comprehensive tests prevents these issues from reaching production.

**Golden Rule**: When in doubt, add a test. Echo tests are cheap insurance against marshaling bugs.
