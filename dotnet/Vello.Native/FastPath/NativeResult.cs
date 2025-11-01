// Copyright 2025
// SPDX-License-Identifier: Apache-2.0 OR MIT

using System.Runtime.InteropServices;

namespace Vello.Native.FastPath;

/// <summary>
/// Helpers for translating native return codes into managed exceptions.
/// </summary>
public static class NativeResult
{
    /// <summary>
    /// Throws a <see cref="NativeException"/> when the return code indicates failure.
    /// </summary>
    /// <param name="errorCode">Return code from a native call.</param>
    /// <param name="operation">Name of the operation for diagnostic purposes.</param>
    public static void ThrowIfFailed(int errorCode, string operation)
    {
        if (errorCode == NativeMethods.VELLO_OK)
        {
            return;
        }

        string message = $"{operation} failed with error code {errorCode}.";
        if (TryConsumeLastErrorMessage(out string? lastError))
        {
            message += $" Last error: {lastError}";
        }

        throw new NativeException(message, errorCode);
    }

    /// <summary>
    /// Ensures that a native handle is valid.
    /// </summary>
    /// <param name="handle">Handle returned by a native call.</param>
    /// <param name="operation">Name of the operation for diagnostic purposes.</param>
    /// <returns>The validated handle when non-zero.</returns>
    public static nint EnsureHandle(nint handle, string operation)
    {
        if (handle != nint.Zero)
        {
            return handle;
        }

        string message = $"{operation} returned a null handle.";
        if (TryConsumeLastErrorMessage(out string? lastError))
        {
            message += $" Last error: {lastError}";
        }

        throw new NativeException(message, NativeMethods.VELLO_ERROR_NULL_POINTER);
    }

    private static bool TryConsumeLastErrorMessage(out string? message)
    {
        message = null;
        nint errorPtr = NativeMethods.GetLastError();
        if (errorPtr == nint.Zero)
        {
            return false;
        }

        message = Marshal.PtrToStringAnsi(errorPtr);
        NativeMethods.ClearLastError();
        return !string.IsNullOrEmpty(message);
    }
}
