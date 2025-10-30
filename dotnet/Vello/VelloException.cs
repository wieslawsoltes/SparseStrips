// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

using System.Runtime.InteropServices;
using Vello.Native;

namespace Vello;

/// <summary>
/// Exception thrown when a Vello operation fails.
/// </summary>
public class VelloException : Exception
{
    /// <summary>
    /// The error code returned by the native library.
    /// </summary>
    public int ErrorCode { get; }

    internal VelloException(string message, int errorCode = 0)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    internal static void ThrowIfError(int result)
    {
        if (result < 0)
        {
            nint errorPtr = NativeMethods.GetLastError();
            string message;

            if (errorPtr != 0)
            {
                message = Marshal.PtrToStringUTF8(errorPtr) ?? "Unknown error";
            }
            else
            {
                message = GetDefaultErrorMessage(result);
            }

            NativeMethods.ClearLastError();
            throw new VelloException(message, result);
        }
    }

    private static string GetDefaultErrorMessage(int errorCode)
    {
        return errorCode switch
        {
            NativeMethods.VELLO_ERROR_NULL_POINTER =>
                "Null pointer error",
            NativeMethods.VELLO_ERROR_INVALID_HANDLE =>
                "Invalid handle - object may have been disposed",
            NativeMethods.VELLO_ERROR_RENDER_FAILED =>
                "Render operation failed",
            NativeMethods.VELLO_ERROR_OUT_OF_MEMORY =>
                "Out of memory",
            NativeMethods.VELLO_ERROR_INVALID_PARAMETER =>
                "Invalid parameter",
            NativeMethods.VELLO_ERROR_PNG_DECODE =>
                "PNG decode error",
            NativeMethods.VELLO_ERROR_PNG_ENCODE =>
                "PNG encode error",
            _ =>
                $"Unknown error (code: {errorCode})"
        };
    }
}
