// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

using Vello;

namespace Vello.Tests;

public class ColorTests
{
    [Fact]
    public void Color_Constructor_SetsCorrectValues()
    {
        var color = new Color(255, 128, 64, 200);

        Assert.Equal(255, color.R);
        Assert.Equal(128, color.G);
        Assert.Equal(64, color.B);
        Assert.Equal(200, color.A);
    }

    [Fact]
    public void Color_Equality_WorksCorrectly()
    {
        var color1 = new Color(255, 128, 64, 200);
        var color2 = new Color(255, 128, 64, 200);
        var color3 = new Color(255, 128, 64, 201);

        Assert.Equal(color1, color2);
        Assert.NotEqual(color1, color3);
    }

    [Fact]
    public void PremulRgba8_CanBeCreatedAndRead()
    {
        var pixel = new PremulRgba8(100, 150, 200, 255);

        Assert.Equal(100, pixel.R);
        Assert.Equal(150, pixel.G);
        Assert.Equal(200, pixel.B);
        Assert.Equal(255, pixel.A);
    }
}
