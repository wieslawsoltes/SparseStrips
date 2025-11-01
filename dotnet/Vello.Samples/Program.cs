// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

using Vello;
using Vello.Geometry;

namespace Vello.Samples;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Vello CPU Rendering Samples");
        Console.WriteLine("============================\n");

        // Example 1: Simple rectangle
        Example1_SimpleRectangle();
        
        // Example 2: Multiple shapes with transforms
        Example2_TransformsAndShapes();
        
        // Example 3: Bezier paths
        Example3_BezierPaths();
        
        // Example 4: Strokes and fills
        Example4_StrokesAndFills();
        
        // Example 5: Zero-copy pixel access
        Example5_ZeroCopyPixelAccess();

        // Example 6: Gradients
        Example6_Gradients();

        // Example 7: Blend modes
        Example7_BlendModes();

        // Example 8: PNG support
        Example8_PngSupport();

        // Example 9: Text rendering
        Example9_TextRendering();

        // Example 10: Sweep gradients
        Example10_SweepGradients();

        // Example 11: Blurred rounded rectangles
        Example11_BlurredRoundedRectangles();

        // Example 12: Clipping layers
        Example12_ClippingLayers();

        // Example 13: Opacity layers
        Example13_OpacityLayers();

        // Example 14: Mask layers
        Example14_MaskLayers();

        // Example 15: Image as paint
        Example15_ImageAsPaint();

        // Example 16: Recording and replay
        Example16_RecordingReplay();

        Console.WriteLine("\nAll examples completed successfully!");
    }

    static void Example1_SimpleRectangle()
    {
        Console.WriteLine("Example 1: Simple Rectangle");
        Console.WriteLine("---------------------------");

        using var context = new RenderContext(400, 300);
        using var pixmap = new Pixmap(400, 300);

        // Set paint color to magenta
        context.SetPaint(new Color(255, 0, 255, 255));

        // Draw a filled rectangle
        context.FillRect(new Rect(50, 50, 350, 250));

        // Flush before rendering
        context.Flush();

        // Render to pixmap
        context.RenderToPixmap(pixmap);

        Console.WriteLine($"  Created {pixmap.Width}x{pixmap.Height} pixmap");
        Console.WriteLine($"  Filled rectangle from (50,50) to (350,250)");
        Console.WriteLine();
    }

    static void Example2_TransformsAndShapes()
    {
        Console.WriteLine("Example 2: Transforms and Multiple Shapes");
        Console.WriteLine("-----------------------------------------");

        using var context = new RenderContext(800, 600);
        using var pixmap = new Pixmap(800, 600);

        // Draw first rectangle with identity transform
        context.SetPaint(new Color(255, 0, 0, 255)); // Red
        context.FillRect(new Rect(100, 100, 200, 200));

        // Apply translation and draw second rectangle
        context.SetTransform(Affine.Translation(300, 0));
        context.SetPaint(new Color(0, 255, 0, 255)); // Green
        context.FillRect(new Rect(100, 100, 200, 200));

        // Apply rotation and draw third rectangle
        context.ResetTransform();
        context.SetTransform(Affine.Rotation(Math.PI / 4));
        context.SetPaint(new Color(0, 0, 255, 255)); // Blue
        context.FillRect(new Rect(400, 300, 500, 400));

        context.Flush();
        context.RenderToPixmap(pixmap);

        Console.WriteLine($"  Drew 3 rectangles with different transforms");
        Console.WriteLine($"  Red: identity, Green: translated, Blue: rotated");
        Console.WriteLine();
    }

    static void Example3_BezierPaths()
    {
        Console.WriteLine("Example 3: Bezier Paths");
        Console.WriteLine("-----------------------");

        using var context = new RenderContext(800, 600);
        using var pixmap = new Pixmap(800, 600);
        using var path = new BezPath();

        // Create a complex path with curves
        path.MoveTo(100, 300)
            .LineTo(200, 100)
            .QuadTo(400, 50, 600, 100)
            .CurveTo(650, 200, 650, 400, 600, 500)
            .LineTo(200, 500)
            .Close();

        context.SetPaint(new Color(255, 128, 0, 255)); // Orange
        context.FillPath(path);
        context.Flush();
        context.RenderToPixmap(pixmap);

        Console.WriteLine($"  Created complex bezier path");
        Console.WriteLine($"  Path includes: move, line, quad, cubic, close");
        Console.WriteLine();
    }

    static void Example4_StrokesAndFills()
    {
        Console.WriteLine("Example 4: Strokes and Fills");
        Console.WriteLine("----------------------------");

        using var context = new RenderContext(800, 600);
        using var pixmap = new Pixmap(800, 600);
        using var path = new BezPath();

        // Create a star path
        double centerX = 400;
        double centerY = 300;
        double outerRadius = 150;
        double innerRadius = 60;
        int points = 5;

        path.MoveTo(centerX, centerY - outerRadius);
        for (int i = 1; i < points * 2; i++)
        {
            double angle = Math.PI * i / points - Math.PI / 2;
            double radius = (i % 2 == 0) ? outerRadius : innerRadius;
            double x = centerX + radius * Math.Cos(angle);
            double y = centerY + radius * Math.Sin(angle);
            path.LineTo(x, y);
        }
        path.Close();

        // Fill the star
        context.SetPaint(new Color(255, 255, 0, 255)); // Yellow fill
        context.FillPath(path);

        // Stroke the star outline
        context.SetStroke(new Stroke(
            width: 5.0f,
            join: Join.Round,
            startCap: Cap.Round,
            endCap: Cap.Round
        ));
        context.SetPaint(new Color(255, 0, 0, 255)); // Red stroke
        context.StrokePath(path);

        context.Flush();
        context.RenderToPixmap(pixmap);

        Console.WriteLine($"  Drew a 5-pointed star");
        Console.WriteLine($"  Yellow fill with red stroke (width: 5.0, round caps/joins)");
        Console.WriteLine();
    }

    static void Example5_ZeroCopyPixelAccess()
    {
        Console.WriteLine("Example 5: Zero-Copy Pixel Access");
        Console.WriteLine("----------------------------------");

        using var context = new RenderContext(100, 100);
        using var pixmap = new Pixmap(100, 100);

        // Draw some content
        context.SetPaint(new Color(128, 64, 192, 255));
        context.FillRect(new Rect(10, 10, 90, 90));
        context.Flush();
        context.RenderToPixmap(pixmap);

        // Access pixels with zero-copy via Span<T>
        ReadOnlySpan<PremulRgba8> pixels = pixmap.GetPixels();
        
        Console.WriteLine($"  Pixel data accessed via Span<PremulRgba8>");
        Console.WriteLine($"  Total pixels: {pixels.Length}");
        Console.WriteLine($"  Expected: {pixmap.Width * pixmap.Height}");
        
        // Sample a few pixels
        if (pixels.Length > 0)
        {
            var firstPixel = pixels[0];
            var centerPixel = pixels[5050]; // Center of 100x100
            
            Console.WriteLine($"  First pixel (0,0): R={firstPixel.R} G={firstPixel.G} B={firstPixel.B} A={firstPixel.A}");
            Console.WriteLine($"  Center pixel (50,50): R={centerPixel.R} G={centerPixel.G} B={centerPixel.B} A={centerPixel.A}");
        }
        
        // Convert to byte array (with copy)
        byte[] bytes = pixmap.ToByteArray();
        Console.WriteLine($"  Converted to byte array: {bytes.Length} bytes");
        Console.WriteLine();
    }

    static void Example6_Gradients()
    {
        Console.WriteLine("Example 6: Gradients");
        Console.WriteLine("-------------------");

        using var context = new RenderContext(800, 600);
        using var pixmap = new Pixmap(800, 600);

        // Linear gradient (left to right)
        var linearStops = new ColorStop[]
        {
            new(0.0f, 255, 0, 0, 255),     // Red
            new(0.5f, 255, 255, 0, 255),   // Yellow
            new(1.0f, 0, 255, 0, 255)      // Green
        };
        context.SetPaintLinearGradient(100, 100, 400, 100, linearStops, GradientExtend.Pad);
        context.FillRect(new Rect(100, 100, 400, 250));

        // Radial gradient
        var radialStops = new ColorStop[]
        {
            new(0.0f, 255, 255, 255, 255), // White center
            new(0.5f, 0, 128, 255, 255),   // Blue middle
            new(1.0f, 0, 0, 128, 255)      // Dark blue edge
        };
        context.SetPaintRadialGradient(600, 400, 100, radialStops, GradientExtend.Pad);
        context.FillRect(new Rect(500, 300, 700, 500));

        context.Flush();
        context.RenderToPixmap(pixmap);

        Console.WriteLine($"  Drew linear gradient (red → yellow → green)");
        Console.WriteLine($"  Drew radial gradient (white → blue → dark blue)");
        Console.WriteLine();
    }

    static void Example7_BlendModes()
    {
        Console.WriteLine("Example 7: Blend Modes");
        Console.WriteLine("----------------------");

        using var context = new RenderContext(800, 600);
        using var pixmap = new Pixmap(800, 600);

        // Draw base layer
        context.SetPaint(new Color(255, 0, 0, 255)); // Red
        context.FillRect(new Rect(100, 100, 300, 300));

        // Multiply blend mode
        context.PushBlendLayer(BlendMode.Multiply());
        context.SetPaint(new Color(0, 0, 255, 255)); // Blue
        context.FillRect(new Rect(150, 150, 350, 350));
        context.PopLayer();

        // Screen blend mode
        context.SetPaint(new Color(0, 255, 0, 255)); // Green
        context.FillRect(new Rect(400, 100, 600, 300));

        context.PushBlendLayer(BlendMode.Screen());
        context.SetPaint(new Color(0, 0, 255, 255)); // Blue
        context.FillRect(new Rect(450, 150, 650, 350));
        context.PopLayer();

        context.Flush();
        context.RenderToPixmap(pixmap);

        Console.WriteLine($"  Drew red square with blue overlay using Multiply blend");
        Console.WriteLine($"  Drew green square with blue overlay using Screen blend");
        Console.WriteLine();
    }

    static void Example8_PngSupport()
    {
        Console.WriteLine("Example 8: PNG Support");
        Console.WriteLine("---------------------");

        // Create a colorful scene
        using var context = new RenderContext(400, 300);
        using var pixmap = new Pixmap(400, 300);

        // Draw a gradient background
        var gradientStops = new ColorStop[]
        {
            new(0.0f, 100, 149, 237, 255),   // Cornflower blue
            new(1.0f, 255, 182, 193, 255)    // Light pink
        };
        context.SetPaintLinearGradient(0, 0, 400, 300, gradientStops, GradientExtend.Pad);
        context.FillRect(new Rect(0, 0, 400, 300));

        // Draw some shapes
        context.SetPaint(new Color(255, 215, 0, 200)); // Gold with transparency
        using var path = new BezPath();
        path.MoveTo(200, 50)
            .LineTo(250, 150)
            .LineTo(350, 150)
            .LineTo(225, 200)
            .LineTo(275, 280)
            .LineTo(200, 225)
            .LineTo(125, 280)
            .LineTo(175, 200)
            .LineTo(50, 150)
            .LineTo(150, 150)
            .Close();
        context.FillPath(path);

        // Add a stroke
        context.SetPaint(new Color(218, 165, 32, 255)); // Goldenrod
        context.SetStroke(new Stroke(width: 3.0f, join: Join.Round));
        context.StrokePath(path);

        context.Flush();
        context.RenderToPixmap(pixmap);

        // Save to PNG file
        string outputPath = "output.png";
        pixmap.SaveAsPng(outputPath);
        Console.WriteLine($"  Saved rendered image to: {outputPath}");
        Console.WriteLine($"  Image size: {pixmap.Width}x{pixmap.Height}");

        // Load the PNG back
        using var loadedPixmap = Pixmap.FromPngFile(outputPath);
        Console.WriteLine($"  Loaded PNG from file");
        Console.WriteLine($"  Loaded image size: {loadedPixmap.Width}x{loadedPixmap.Height}");

        // Verify pixels match
        var originalPixels = pixmap.GetPixels();
        var loadedPixels = loadedPixmap.GetPixels();

        bool pixelsMatch = originalPixels.Length == loadedPixels.Length;
        if (pixelsMatch)
        {
            for (int i = 0; i < Math.Min(100, originalPixels.Length); i++)
            {
                if (originalPixels[i].R != loadedPixels[i].R ||
                    originalPixels[i].G != loadedPixels[i].G ||
                    originalPixels[i].B != loadedPixels[i].B ||
                    originalPixels[i].A != loadedPixels[i].A)
                {
                    pixelsMatch = false;
                    break;
                }
            }
        }

        Console.WriteLine($"  Pixel verification: {(pixelsMatch ? "PASS (sample matches)" : "Different (expected due to PNG compression)")}");
        Console.WriteLine();
    }

    static void Example9_TextRendering()
    {
        Console.WriteLine("Example 9: Text Rendering");
        Console.WriteLine("-------------------------");

        // Try to find a system font (macOS, Linux, Windows)
        string[] possibleFontPaths = new[]
        {
            "/System/Library/Fonts/Helvetica.ttc",              // macOS
            "/System/Library/Fonts/Supplemental/Arial.ttf",     // macOS
            "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf",  // Linux
            "C:\\Windows\\Fonts\\arial.ttf",                     // Windows
            "C:\\Windows\\Fonts\\segoeui.ttf",                   // Windows
        };

        string? fontPath = possibleFontPaths.FirstOrDefault(File.Exists);

        if (fontPath == null)
        {
            Console.WriteLine("  ⚠️  No system font found - skipping text rendering example");
            Console.WriteLine("  (This is expected on systems without common fonts)");
            Console.WriteLine();
            return;
        }

        Console.WriteLine($"  Using font: {Path.GetFileName(fontPath)}");

        try
        {
            using var context = new RenderContext(600, 400);
            using var pixmap = new Pixmap(600, 400);
            using var font = FontData.FromFile(fontPath);

            // Draw gradient background
            var gradientStops = new ColorStop[]
            {
                new(0.0f, 240, 248, 255, 255),  // Alice blue
                new(1.0f, 255, 250, 240, 255)   // Floral white
            };
            context.SetPaintLinearGradient(0, 0, 0, 400, gradientStops);
            context.FillRect(new Rect(0, 0, 600, 400));

            // Draw title text
            context.SetPaint(new Color(25, 25, 112, 255)); // Midnight blue
            context.FillText(font, 48.0f, "Vello CPU", 50, 80);

            // Draw subtitle
            context.SetPaint(new Color(70, 130, 180, 255)); // Steel blue
            context.FillText(font, 32.0f, "Text Rendering", 50, 140);

            // Draw body text
            context.SetPaint(new Color(0, 0, 0, 255)); // Black
            context.FillText(font, 24.0f, "Fast 2D graphics rendering", 50, 200);
            context.FillText(font, 24.0f, "with CPU-based rasterization", 50, 240);

            // Draw stroked text
            context.SetPaint(new Color(220, 20, 60, 255)); // Crimson fill
            context.FillText(font, 36.0f, "Stroked Text", 50, 310);

            context.SetStroke(new Stroke(width: 2.0f));
            context.SetPaint(new Color(139, 0, 0, 255)); // Dark red stroke
            context.StrokeText(font, 36.0f, "Stroked Text", 50, 310);

            context.Flush();
            context.RenderToPixmap(pixmap);

            // Save to PNG
            string outputPath = "text_output.png";
            pixmap.SaveAsPng(outputPath);

            Console.WriteLine($"  Rendered text at multiple sizes (48pt, 32pt, 24pt, 36pt)");
            Console.WriteLine($"  Applied fill and stroke to text");
            Console.WriteLine($"  Saved to: {outputPath}");
            Console.WriteLine($"  Output size: {pixmap.Width}x{pixmap.Height}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ❌ Error rendering text: {ex.Message}");
        }

        Console.WriteLine();
    }

    static void Example10_SweepGradients()
    {
        Console.WriteLine("Example 10: Sweep Gradients");
        Console.WriteLine("----------------------------");

        using var context = new RenderContext(400, 400);
        using var pixmap = new Pixmap(400, 400);

        // Background
        context.SetPaint(new Color(30, 30, 30, 255));
        context.FillRect(new Rect(0, 0, 400, 400));

        // Sweep gradient (full circle)
        var stops = new ColorStop[]
        {
            new(0.0f, 255, 0, 0, 255),    // Red
            new(0.25f, 255, 255, 0, 255), // Yellow
            new(0.5f, 0, 255, 0, 255),    // Green
            new(0.75f, 0, 255, 255, 255), // Cyan
            new(1.0f, 255, 0, 0, 255),    // Back to red
        };

        context.SetPaintSweepGradient(
            200, 200,                              // Center
            0f, (float)(Math.PI * 2),              // Full circle (0 to 360 degrees)
            stops);
        context.FillRect(new Rect(50, 50, 350, 350));

        context.Flush();
        context.RenderToPixmap(pixmap);

        string outputPath = "sweep_gradient_output.png";
        pixmap.SaveAsPng(outputPath);

        Console.WriteLine($"  Created sweep gradient (angular/conical gradient)");
        Console.WriteLine($"  Center: (200, 200)");
        Console.WriteLine($"  Angle range: 0° to 360°");
        Console.WriteLine($"  Color stops: Red → Yellow → Green → Cyan → Red");
        Console.WriteLine($"  Saved to: {outputPath}");
        Console.WriteLine();
    }

    static void Example11_BlurredRoundedRectangles()
    {
        Console.WriteLine("Example 11: Blurred Rounded Rectangles");
        Console.WriteLine("---------------------------------------");

        using var context = new RenderContext(600, 400);
        using var pixmap = new Pixmap(600, 400);

        // Light background
        context.SetPaint(new Color(245, 245, 245, 255));
        context.FillRect(new Rect(0, 0, 600, 400));

        // Blurred rounded rectangle 1 - subtle blur
        context.SetPaint(new Color(100, 150, 200, 255));
        context.FillBlurredRoundedRect(
            new Rect(50, 50, 250, 150),
            radius: 20f,
            stdDev: 5f);

        // Blurred rounded rectangle 2 - heavy blur
        context.SetPaint(new Color(200, 100, 150, 255));
        context.FillBlurredRoundedRect(
            new Rect(300, 50, 550, 150),
            radius: 30f,
            stdDev: 15f);

        // Blurred rounded rectangle 3 - extreme blur
        context.SetPaint(new Color(150, 200, 100, 255));
        context.FillBlurredRoundedRect(
            new Rect(150, 220, 450, 350),
            radius: 40f,
            stdDev: 25f);

        context.Flush();
        context.RenderToPixmap(pixmap);

        string outputPath = "blurred_rounded_rect_output.png";
        pixmap.SaveAsPng(outputPath);

        Console.WriteLine($"  Created 3 blurred rounded rectangles");
        Console.WriteLine($"  Blur 1: radius=20px, stdDev=5px (subtle)");
        Console.WriteLine($"  Blur 2: radius=30px, stdDev=15px (heavy)");
        Console.WriteLine($"  Blur 3: radius=40px, stdDev=25px (extreme)");
        Console.WriteLine($"  Saved to: {outputPath}");
        Console.WriteLine();
    }

    static void Example12_ClippingLayers()
    {
        Console.WriteLine("Example 12: Clipping Layers");
        Console.WriteLine("----------------------------");

        using var context = new RenderContext(400, 400);
        using var pixmap = new Pixmap(400, 400);

        // Background
        context.SetPaint(new Color(255, 255, 255, 255));
        context.FillRect(new Rect(0, 0, 400, 400));

        // Create a circular clip path
        using var clipPath = new BezPath();
        int segments = 64;
        double centerX = 200, centerY = 200, radius = 150;

        for (int i = 0; i <= segments; i++)
        {
            double angle = (i / (double)segments) * Math.PI * 2;
            double x = centerX + Math.Cos(angle) * radius;
            double y = centerY + Math.Sin(angle) * radius;

            if (i == 0)
                clipPath.MoveTo(x, y);
            else
                clipPath.LineTo(x, y);
        }
        clipPath.Close();

        // Push clip layer
        context.PushClipLayer(clipPath);

        // Draw gradient (will be clipped to circle)
        var stops = new ColorStop[]
        {
            new(0.0f, 255, 100, 100, 255),
            new(1.0f, 100, 100, 255, 255)
        };
        context.SetPaintLinearGradient(0, 0, 400, 400, stops);
        context.FillRect(new Rect(0, 0, 400, 400));

        // Draw some shapes (will be clipped)
        context.SetPaint(new Color(255, 255, 0, 200));
        context.FillRect(new Rect(100, 100, 300, 300));

        context.SetPaint(new Color(0, 255, 255, 200));
        using (var star = new BezPath())
        {
            for (int i = 0; i < 10; i++)
            {
                double angle = (i / 10.0) * Math.PI * 2 - Math.PI / 2;
                double r = (i % 2 == 0) ? 100 : 50;
                double x = 200 + Math.Cos(angle) * r;
                double y = 200 + Math.Sin(angle) * r;

                if (i == 0)
                    star.MoveTo(x, y);
                else
                    star.LineTo(x, y);
            }
            star.Close();
            context.FillPath(star);
        }

        // Pop clip layer
        context.PopLayer();

        context.Flush();
        context.RenderToPixmap(pixmap);

        string outputPath = "clipping_output.png";
        pixmap.SaveAsPng(outputPath);

        Console.WriteLine($"  Created circular clip region");
        Console.WriteLine($"  Drew gradient, rectangle, and star within clip");
        Console.WriteLine($"  All shapes clipped to circle");
        Console.WriteLine($"  Saved to: {outputPath}");
        Console.WriteLine();
    }

    static void Example13_OpacityLayers()
    {
        Console.WriteLine("Example 13: Opacity Layers");
        Console.WriteLine("--------------------------");

        using var context = new RenderContext(500, 300);
        using var pixmap = new Pixmap(500, 300);

        // Background
        context.SetPaint(new Color(50, 50, 50, 255));
        context.FillRect(new Rect(0, 0, 500, 300));

        // Draw opaque rectangles for comparison
        context.SetPaint(new Color(255, 100, 100, 255));
        context.FillRect(new Rect(20, 20, 120, 120));

        context.SetPaint(new Color(100, 255, 100, 255));
        context.FillRect(new Rect(70, 70, 170, 170));

        // Draw with 50% opacity layer
        context.PushOpacityLayer(0.5f);

        context.SetPaint(new Color(100, 100, 255, 255));
        context.FillRect(new Rect(200, 20, 300, 120));

        context.SetPaint(new Color(255, 255, 100, 255));
        context.FillRect(new Rect(250, 70, 350, 170));

        context.PopLayer();

        // Draw with 25% opacity layer
        context.PushOpacityLayer(0.25f);

        context.SetPaint(new Color(255, 100, 255, 255));
        context.FillRect(new Rect(360, 20, 460, 120));

        context.SetPaint(new Color(100, 255, 255, 255));
        context.FillRect(new Rect(410, 70, 510, 170));

        context.PopLayer();

        context.Flush();
        context.RenderToPixmap(pixmap);

        string outputPath = "opacity_layers_output.png";
        pixmap.SaveAsPng(outputPath);

        Console.WriteLine($"  Created 3 groups of overlapping rectangles:");
        Console.WriteLine($"  - Left: 100% opacity (no layer)");
        Console.WriteLine($"  - Middle: 50% opacity layer");
        Console.WriteLine($"  - Right: 25% opacity layer");
        Console.WriteLine($"  Saved to: {outputPath}");
        Console.WriteLine();
    }

    static void Example14_MaskLayers()
    {
        Console.WriteLine("Example 14: Mask Layers");
        Console.WriteLine("-----------------------");

        using var context = new RenderContext(400, 400);
        using var pixmap = new Pixmap(400, 400);

        // Background
        context.SetPaint(new Color(40, 40, 40, 255));
        context.FillRect(new Rect(0, 0, 400, 400));

        // Create a gradient mask pixmap
        using var maskPixmap = new Pixmap(200, 200);
        using var maskContext = new RenderContext(200, 200);

        // Create radial gradient mask (white center to transparent edges)
        var maskStops = new ColorStop[]
        {
            new(0.0f, 255, 255, 255, 255),  // White/opaque at center
            new(0.7f, 255, 255, 255, 200),  // Slightly transparent
            new(1.0f, 255, 255, 255, 0)     // Fully transparent at edges
        };
        maskContext.SetPaintRadialGradient(100, 100, 100, maskStops);
        maskContext.FillRect(new Rect(0, 0, 200, 200));
        maskContext.Flush();
        maskContext.RenderToPixmap(maskPixmap);

        // Create alpha mask from the pixmap
        using var mask = Mask.NewAlpha(maskPixmap);

        Console.WriteLine($"  Created mask: {mask.Width}x{mask.Height}");

        // Use mask layer with colorful pattern
        context.PushMaskLayer(mask);

        // Draw colorful stripes that will be masked
        for (int i = 0; i < 10; i++)
        {
            byte hue = (byte)(i * 25);
            context.SetPaint(new Color(hue, (byte)(255 - hue), 150, 255));
            context.FillRect(new Rect(100 + i * 20, 100, 120 + i * 20, 300));
        }

        context.PopLayer();

        context.Flush();
        context.RenderToPixmap(pixmap);

        string outputPath = "mask_layer_output.png";
        pixmap.SaveAsPng(outputPath);

        Console.WriteLine($"  Applied radial gradient mask to colored stripes");
        Console.WriteLine($"  Mask creates soft fade-out effect at edges");
        Console.WriteLine($"  Saved to: {outputPath}");
        Console.WriteLine();
    }

    static void Example15_ImageAsPaint()
    {
        Console.WriteLine("Example 15: Image as Paint");
        Console.WriteLine("--------------------------");

        using var context = new RenderContext(500, 400);
        using var pixmap = new Pixmap(500, 400);

        // Create a small source image pixmap with a pattern
        using var sourcePixmap = new Pixmap(50, 50);
        using var sourceContext = new RenderContext(50, 50);

        // Create checkerboard pattern
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                bool isWhite = (x + y) % 2 == 0;
                sourceContext.SetPaint(isWhite ?
                    new Color(255, 255, 255, 255) :
                    new Color(100, 150, 200, 255));
                sourceContext.FillRect(new Rect(x * 10, y * 10, (x + 1) * 10, (y + 1) * 10));
            }
        }

        sourceContext.Flush();
        sourceContext.RenderToPixmap(sourcePixmap);

        // Background
        context.SetPaint(new Color(30, 30, 30, 255));
        context.FillRect(new Rect(0, 0, 500, 400));

        // Example 1: Image with Pad extend mode
        using var imagePad = Image.FromPixmap(sourcePixmap,
            xExtend: GradientExtend.Pad,
            yExtend: GradientExtend.Pad,
            quality: ImageQuality.High);

        context.SetPaintImage(imagePad);
        context.FillRect(new Rect(20, 20, 150, 150));

        // Example 2: Image with Repeat extend mode
        using var imageRepeat = Image.FromPixmap(sourcePixmap,
            xExtend: GradientExtend.Repeat,
            yExtend: GradientExtend.Repeat,
            quality: ImageQuality.High);

        context.SetPaintImage(imageRepeat);
        context.FillRect(new Rect(170, 20, 330, 180));

        // Example 3: Image with Reflect extend mode
        using var imageReflect = Image.FromPixmap(sourcePixmap,
            xExtend: GradientExtend.Reflect,
            yExtend: GradientExtend.Reflect,
            quality: ImageQuality.High);

        context.SetPaintImage(imageReflect);
        context.FillRect(new Rect(350, 20, 480, 150));

        // Example 4: Image with Low quality (fast sampling)
        using var imageLowQuality = Image.FromPixmap(sourcePixmap,
            xExtend: GradientExtend.Repeat,
            yExtend: GradientExtend.Repeat,
            quality: ImageQuality.Low);

        context.SetPaintImage(imageLowQuality);
        context.FillRect(new Rect(95, 200, 405, 380));

        context.Flush();
        context.RenderToPixmap(pixmap);

        string outputPath = "image_paint_output.png";
        pixmap.SaveAsPng(outputPath);

        Console.WriteLine($"  Created 50x50 checkerboard source image");
        Console.WriteLine($"  Drew 4 rectangles with different image modes:");
        Console.WriteLine($"    - Pad extend mode (edges extend)");
        Console.WriteLine($"    - Repeat extend mode (tiles repeat)");
        Console.WriteLine($"    - Reflect extend mode (tiles mirror)");
        Console.WriteLine($"    - Low quality sampling (fast)");
        Console.WriteLine($"  Saved to: {outputPath}");
        Console.WriteLine();
    }

    static void Example16_RecordingReplay()
    {
        Console.WriteLine("Example 16: Recording and Replay");
        Console.WriteLine("--------------------------------");

        using var context = new RenderContext(256, 256);
        using var recording = new Recording();
        using var pixmap = new Pixmap(256, 256);
        using var clipPath = new BezPath();
        using var strokePath = new BezPath();

        clipPath
            .MoveTo(48, 48)
            .LineTo(208, 48)
            .LineTo(208, 208)
            .QuadTo(96, 192, 48, 128)
            .Close();

        strokePath
            .MoveTo(0, 0)
            .LineTo(64, 32)
            .LineTo(32, 96)
            .LineTo(96, 128);

        context.Record(recording, recorder =>
        {
            recorder.SetPaint(new Color(0, 200, 255, 255));
            recorder.SetStroke(new Stroke(
                width: 4.0f,
                join: Join.Round,
                startCap: Cap.Round,
                endCap: Cap.Round));
            recorder.SetFillRule(FillRule.EvenOdd);

            recorder.SetTransform(Affine.Translation(32, 32));
            recorder.SetPaintTransform(Affine.Scale(0.75, 0.75));

            recorder.PushClipLayer(clipPath);
            recorder.FillRect(new Rect(0, 0, 160, 160));
            recorder.StrokePath(strokePath);
            recorder.PopLayer();

            recorder.ResetPaintTransform();
        });

        Console.WriteLine($"  Cached strips before prepare? {recording.HasCachedStrips} (strips={recording.StripCount}, alpha={recording.AlphaByteCount})");

        context.PrepareRecording(recording);

        Console.WriteLine($"  Cached strips after prepare?  {recording.HasCachedStrips} (strips={recording.StripCount}, alpha={recording.AlphaByteCount})");

        context.ExecuteRecording(recording);
        context.Flush();
        context.RenderToPixmap(pixmap);

        Console.WriteLine($"  Rendered recorded scene into {pixmap.Width}x{pixmap.Height} pixmap");
        Console.WriteLine();
    }
}
