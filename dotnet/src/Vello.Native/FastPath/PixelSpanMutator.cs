// Copyright 2025
// SPDX-License-Identifier: Apache-2.0 OR MIT

using System;

namespace Vello.Native.FastPath;

/// <summary>
/// Delegate used to mutate pixmap pixel spans.
/// </summary>
public delegate void PixelSpanMutator(Span<VelloPremulRgba8> pixels);
