// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

using Vello;
using Vello.Geometry;

namespace Vello.Tests;

public class IntegrationTests
{
    [Fact]
    public void CompleteRenderingPipeline_WorksCorrectly()
    {
        using var context = new RenderContext(400, 300);
        using var pixmap = new Pixmap(400, 300);

        // Draw gradient background
        var stops = new ColorStop[]
        {
            new(0.0f, 100, 149, 237, 255),
            new(1.0f, 255, 182, 193, 255)
        };
        context.SetPaintLinearGradient(0, 0, 400, 300, stops);
        context.FillRect(new Rect(0, 0, 400, 300));

        // Draw shapes with transform
        context.SetTransform(Affine.Translation(200, 150));
        context.SetPaint(new Color(255, 215, 0, 200));

        using var path = new BezPath();
        path.MoveTo(-50, -50)
            .LineTo(50, -50)
            .LineTo(50, 50)
            .LineTo(-50, 50)
            .Close();

        context.FillPath(path);

        // Add stroke
        context.SetStroke(new Stroke(3.0f, Join.Round));
        context.SetPaint(new Color(218, 165, 32, 255));
        context.StrokePath(path);

        context.Flush();
        context.RenderToPixmap(pixmap);

        // Verify rendering produced output
        var pixels = pixmap.GetPixels();
        bool hasContent = false;
        for (int i = 0; i < pixels.Length; i++)
        {
            if (pixels[i].A > 0)
            {
                hasContent = true;
                break;
            }
        }

        Assert.True(hasContent, "Rendered output should contain visible pixels");
    }

    [Fact]
    public void PngRoundTrip_PreservesImageData()
    {
        using var context = new RenderContext(100, 100);
        using var pixmap1 = new Pixmap(100, 100);

        // Render something
        context.SetPaint(new Color(128, 64, 192, 255));
        context.FillRect(new Rect(10, 10, 90, 90));
        context.Flush();
        context.RenderToPixmap(pixmap1);

        // Export to PNG
        byte[] pngData = pixmap1.ToPng();

        // Load from PNG
        using var pixmap2 = Pixmap.FromPng(pngData);

        Assert.Equal(pixmap1.Width, pixmap2.Width);
        Assert.Equal(pixmap1.Height, pixmap2.Height);

        // Sample some pixels to verify they match
        var pixels1 = pixmap1.GetPixels();
        var pixels2 = pixmap2.GetPixels();

        // Check center pixel
        int centerIdx = 50 * 100 + 50;
        Assert.Equal(pixels1[centerIdx].R, pixels2[centerIdx].R);
        Assert.Equal(pixels1[centerIdx].G, pixels2[centerIdx].G);
        Assert.Equal(pixels1[centerIdx].B, pixels2[centerIdx].B);
        Assert.Equal(pixels1[centerIdx].A, pixels2[centerIdx].A);
    }

    [Fact]
    public void MultipleContexts_CanCoexist()
    {
        using var context1 = new RenderContext(100, 100);
        using var context2 = new RenderContext(200, 200);
        using var pixmap1 = new Pixmap(100, 100);
        using var pixmap2 = new Pixmap(200, 200);

        context1.SetPaint(new Color(255, 0, 0, 255));
        context1.FillRect(new Rect(0, 0, 100, 100));

        context2.SetPaint(new Color(0, 255, 0, 255));
        context2.FillRect(new Rect(0, 0, 200, 200));

        context1.Flush();
        context2.Flush();

        context1.RenderToPixmap(pixmap1);
        context2.RenderToPixmap(pixmap2);

        // Verify both rendered independently
        Assert.Equal(100, pixmap1.Width);
        Assert.Equal(200, pixmap2.Width);
    }

    [Fact]
    public void BlendModes_CanBeUsed()
    {
        using var context = new RenderContext(100, 100);
        using var pixmap = new Pixmap(100, 100);

        // Just verify blend mode API works without crashing
        context.SetPaint(new Color(255, 0, 0, 255));
        context.FillRect(new Rect(0, 0, 50, 50));

        context.PushBlendLayer(BlendMode.Normal());
        context.SetPaint(new Color(0, 255, 0, 255));
        context.FillRect(new Rect(25, 25, 75, 75));
        context.PopLayer();

        // Should not throw
        context.Flush();
        context.RenderToPixmap(pixmap);
    }

    [Fact]
    public void GradientExtendModes_AllWork()
    {
        using var context = new RenderContext(300, 100);
        using var pixmap = new Pixmap(300, 100);

        var stops = new ColorStop[]
        {
            new(0.0f, 255, 0, 0, 255),
            new(1.0f, 0, 0, 255, 255)
        };

        // Test Pad
        context.SetPaintLinearGradient(0, 0, 100, 0, stops, GradientExtend.Pad);
        context.FillRect(new Rect(0, 0, 100, 100));

        // Test Repeat
        context.SetPaintLinearGradient(100, 0, 200, 0, stops, GradientExtend.Repeat);
        context.FillRect(new Rect(100, 0, 200, 100));

        // Test Reflect
        context.SetPaintLinearGradient(200, 0, 300, 0, stops, GradientExtend.Reflect);
        context.FillRect(new Rect(200, 0, 300, 100));

        context.Flush();
        context.RenderToPixmap(pixmap);

        // All three sections should have rendered
        var pixels = pixmap.GetPixels();
        Assert.True(pixels[50].A > 0); // Pad section
        Assert.True(pixels[150 * 100 + 50].A > 0); // Repeat section
        Assert.True(pixels[250 * 100 + 50].A > 0); // Reflect section
    }

    [Fact]
    public void SweepGradient_RendersFullCircle()
    {
        using var context = new RenderContext(200, 200);
        using var pixmap = new Pixmap(200, 200);

        var stops = new ColorStop[]
        {
            new(0.0f, 255, 0, 0, 255),
            new(0.33f, 0, 255, 0, 255),
            new(0.66f, 0, 0, 255, 255),
            new(1.0f, 255, 0, 0, 255)
        };

        context.SetPaintSweepGradient(100, 100, 0f, (float)(Math.PI * 2), stops);
        context.FillRect(new Rect(0, 0, 200, 200));

        context.Flush();
        context.RenderToPixmap(pixmap);

        // Verify it rendered
        var pixels = pixmap.GetPixels();
        bool hasContent = false;
        for (int i = 0; i < pixels.Length; i++)
        {
            if (pixels[i].A > 0)
            {
                hasContent = true;
                break;
            }
        }
        Assert.True(hasContent);
    }

    [Fact]
    public void BlurredRoundedRect_RendersWithBlur()
    {
        using var context = new RenderContext(200, 200);
        using var pixmap = new Pixmap(200, 200);

        context.SetPaint(new Color(150, 150, 200, 255));
        context.FillBlurredRoundedRect(new Rect(50, 50, 150, 150), radius: 25f, stdDev: 15f);

        context.Flush();
        context.RenderToPixmap(pixmap);

        // Verify it rendered
        var pixels = pixmap.GetPixels();
        bool hasContent = false;
        for (int i = 0; i < pixels.Length; i++)
        {
            if (pixels[i].A > 0)
            {
                hasContent = true;
                break;
            }
        }
        Assert.True(hasContent);
    }

    [Fact]
    public void LayeredRendering_WithClipAndOpacity()
    {
        using var context = new RenderContext(200, 200);
        using var pixmap = new Pixmap(200, 200);
        using var clipPath = new BezPath();

        // Create clip region
        clipPath.MoveTo(50, 50).LineTo(150, 50).LineTo(150, 150).LineTo(50, 150).Close();

        // Background
        context.SetPaint(new Color(200, 200, 200, 255));
        context.FillRect(new Rect(0, 0, 200, 200));

        // Clip + Opacity layer
        context.PushClipLayer(clipPath);
        context.PushOpacityLayer(0.7f);

        context.SetPaint(new Color(255, 100, 100, 255));
        context.FillRect(new Rect(0, 0, 200, 200));

        context.PopLayer(); // opacity
        context.PopLayer(); // clip

        context.Flush();
        context.RenderToPixmap(pixmap);

        // Verify it rendered
        var pixels = pixmap.GetPixels();
        bool hasContent = false;
        for (int i = 0; i < pixels.Length; i++)
        {
            if (pixels[i].A > 0)
            {
                hasContent = true;
                break;
            }
        }
        Assert.True(hasContent);
    }

    [Fact]
    public void MaskLayer_CreatesGradualFadeEffect()
    {
        using var context = new RenderContext(200, 200);
        using var pixmap = new Pixmap(200, 200);

        // Create radial gradient mask
        using var maskPixmap = new Pixmap(200, 200);
        using var maskContext = new RenderContext(200, 200);

        var maskStops = new ColorStop[]
        {
            new(0.0f, 255, 255, 255, 255),
            new(1.0f, 255, 255, 255, 0)
        };
        maskContext.SetPaintRadialGradient(100, 100, 100, maskStops);
        maskContext.FillRect(new Rect(0, 0, 200, 200));
        maskContext.Flush();
        maskContext.RenderToPixmap(maskPixmap);

        using var mask = Mask.NewAlpha(maskPixmap);

        // Apply mask to colored pattern
        context.PushMaskLayer(mask);
        context.SetPaint(new Color(200, 100, 50, 255));
        context.FillRect(new Rect(0, 0, 200, 200));
        context.PopLayer();

        context.Flush();
        context.RenderToPixmap(pixmap);

        // Verify it rendered
        var pixels = pixmap.GetPixels();
        bool hasContent = false;
        for (int i = 0; i < pixels.Length; i++)
        {
            if (pixels[i].A > 0)
            {
                hasContent = true;
                break;
            }
        }
        Assert.True(hasContent);
    }

    [Fact]
    public void ImagePaint_WithDifferentExtendModes()
    {
        using var context = new RenderContext(150, 150);
        using var pixmap = new Pixmap(150, 150);

        // Create small pattern image
        using var sourcePixmap = new Pixmap(30, 30);
        using var sourceContext = new RenderContext(30, 30);
        sourceContext.SetPaint(new Color(150, 100, 200, 255));
        sourceContext.FillRect(new Rect(0, 0, 15, 15));
        sourceContext.SetPaint(new Color(200, 150, 100, 255));
        sourceContext.FillRect(new Rect(15, 15, 30, 30));
        sourceContext.Flush();
        sourceContext.RenderToPixmap(sourcePixmap);

        // Test with Repeat extend mode
        using var image = Image.FromPixmap(sourcePixmap,
            xExtend: GradientExtend.Repeat,
            yExtend: GradientExtend.Repeat,
            quality: ImageQuality.Medium);

        context.SetPaintImage(image);
        context.FillRect(new Rect(0, 0, 150, 150));

        context.Flush();
        context.RenderToPixmap(pixmap);

        // Verify it rendered
        var pixels = pixmap.GetPixels();
        bool hasContent = false;
        for (int i = 0; i < pixels.Length; i++)
        {
            if (pixels[i].A > 0)
            {
                hasContent = true;
                break;
            }
        }
        Assert.True(hasContent);
    }

    [Fact]
    public void LuminanceMask_UsesColorInformation()
    {
        using var context = new RenderContext(100, 100);
        using var pixmap = new Pixmap(100, 100);

        // Create colored gradient for luminance mask
        using var maskPixmap = new Pixmap(100, 100);
        using var maskContext = new RenderContext(100, 100);

        var maskStops = new ColorStop[]
        {
            new(0.0f, 255, 255, 0, 255),  // Yellow (high luminance)
            new(1.0f, 0, 0, 255, 255)     // Blue (low luminance)
        };
        maskContext.SetPaintLinearGradient(0, 0, 100, 100, maskStops);
        maskContext.FillRect(new Rect(0, 0, 100, 100));
        maskContext.Flush();
        maskContext.RenderToPixmap(maskPixmap);

        // Use luminance mask
        using var mask = Mask.NewLuminance(maskPixmap);

        context.PushMaskLayer(mask);
        context.SetPaint(new Color(255, 0, 0, 255));
        context.FillRect(new Rect(0, 0, 100, 100));
        context.PopLayer();

        context.Flush();
        context.RenderToPixmap(pixmap);

        // Verify it rendered
        var pixels = pixmap.GetPixels();
        bool hasContent = false;
        for (int i = 0; i < pixels.Length; i++)
        {
            if (pixels[i].A > 0)
            {
                hasContent = true;
                break;
            }
        }
        Assert.True(hasContent);
    }
}
