// Copyright 2025
// SPDX-License-Identifier: Apache-2.0 OR MIT

using System;

namespace Vello.Native.FastPath;

/// <summary>
/// Exception thrown when a native Vello fast-path operation fails.
/// </summary>
public sealed class NativeException : InvalidOperationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NativeException"/> class.
    /// </summary>
    /// <param name="message">Error message.</param>
    /// <param name="errorCode">Native error code returned by the operation.</param>
    public NativeException(string message, int errorCode)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Gets the native error code associated with the failure.
    /// </summary>
    public int ErrorCode { get; }
}
