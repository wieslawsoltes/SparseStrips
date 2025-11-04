// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

using Vello.Geometry;

namespace Vello.Tests;

public class GeometryTests
{
    [Fact]
    public void Rect_Constructor_SetsCorrectValues()
    {
        var rect = new Rect(10, 20, 100, 200);

        Assert.Equal(10, rect.X0);
        Assert.Equal(20, rect.Y0);
        Assert.Equal(100, rect.X1);
        Assert.Equal(200, rect.Y1);
    }

    [Fact]
    public void Affine_Identity_IsCorrect()
    {
        var identity = Affine.Identity;

        Assert.Equal(1.0, identity.M11);
        Assert.Equal(0.0, identity.M12);
        Assert.Equal(0.0, identity.M13);
        Assert.Equal(0.0, identity.M21);
        Assert.Equal(1.0, identity.M22);
        Assert.Equal(0.0, identity.M23);
    }

    [Fact]
    public void Affine_Translation_CreatesCorrectMatrix()
    {
        var translate = Affine.Translation(100, 50);

        Assert.Equal(1.0, translate.M11);
        Assert.Equal(0.0, translate.M12);
        Assert.Equal(100.0, translate.M13);
        Assert.Equal(0.0, translate.M21);
        Assert.Equal(1.0, translate.M22);
        Assert.Equal(50.0, translate.M23);
    }

    [Fact]
    public void Affine_Rotation_CreatesCorrectMatrix()
    {
        var rotate = Affine.Rotation(Math.PI / 2); // 90 degrees

        Assert.Equal(0.0, rotate.M11, 5); // cos(90°) ≈ 0
        Assert.Equal(-1.0, rotate.M12, 5); // -sin(90°) ≈ -1
        Assert.Equal(1.0, rotate.M21, 5); // sin(90°) ≈ 1
        Assert.Equal(0.0, rotate.M22, 5); // cos(90°) ≈ 0
    }

    [Fact]
    public void Stroke_Constructor_SetsCorrectValues()
    {
        var stroke = new Stroke(5.0f, Join.Bevel, Cap.Square, Cap.Round, 2.5f);

        Assert.Equal(5.0f, stroke.Width);
        Assert.Equal(Join.Bevel, stroke.Join);
        Assert.Equal(Cap.Square, stroke.StartCap);
        Assert.Equal(Cap.Round, stroke.EndCap);
        Assert.Equal(2.5f, stroke.MiterLimit);
    }

    [Theory]
    [InlineData(Join.Bevel)]
    [InlineData(Join.Miter)]
    [InlineData(Join.Round)]
    public void Join_EnumValues_AreValid(Join join)
    {
        Assert.True(Enum.IsDefined(typeof(Join), join));
    }

    [Theory]
    [InlineData(Cap.Butt)]
    [InlineData(Cap.Square)]
    [InlineData(Cap.Round)]
    public void Cap_EnumValues_AreValid(Cap cap)
    {
        Assert.True(Enum.IsDefined(typeof(Cap), cap));
    }
}
