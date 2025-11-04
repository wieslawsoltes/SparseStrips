using System.Runtime.InteropServices;
using Vello;
using Vello.Geometry;
using Vello.Native;
using Vello.Native.FastPath;

try
{
    Console.WriteLine("Running Vello integration check...");

    ValidateNativeEnvironment();
    string managedOutput = RenderManagedScene();
    Console.WriteLine($"Managed renderer sample saved to : {managedOutput}");

    string fastPathOutput = RenderFastPathScene();
    Console.WriteLine($"Fast-path renderer sample saved to: {fastPathOutput}");

    Console.WriteLine("Integration test completed successfully.");
    Environment.Exit(0);
}
catch (Exception ex)
{
    Console.Error.WriteLine("Integration test failed.");
    Console.Error.WriteLine(ex);
    Environment.Exit(1);
}

static void ValidateNativeEnvironment()
{
    string version = Marshal.PtrToStringAnsi(NativeMethods.Version()) ?? "unknown";
    VelloSimdLevel simdLevel = NativeMethods.SimdDetect();

    Console.WriteLine($"Native library version : {version}");
    Console.WriteLine($"Detected SIMD level    : {simdLevel}");

    if (string.IsNullOrWhiteSpace(version))
    {
        throw new InvalidOperationException("Native library reported an empty version string.");
    }

    if (!Enum.IsDefined(typeof(VelloSimdLevel), simdLevel))
    {
        throw new InvalidOperationException($"Native library reported an unknown SIMD level: {simdLevel}.");
    }
}

static string RenderManagedScene()
{
    const ushort width = 256;
    const ushort height = 256;

    using var context = new RenderContext(width, height);
    using var pixmap = new Pixmap(width, height);

    context.SetPaint(Color.White);
    context.FillRect(new Rect(0, 0, width, height));

    var stops = new[]
    {
        new ColorStop(0f, new Color(255, 140, 0)),
        new ColorStop(1f, new Color(128, 0, 128))
    };

    context.SetPaintLinearGradient(0, 0, width, height, stops);
    context.FillRect(Rect.FromXYWH(32, 32, width - 64, height - 64));

    using var path = new BezPath();
    path.MoveTo(width / 2.0, 48)
        .LineTo(width - 48, height - 48)
        .LineTo(48, height - 48)
        .Close();

    context.SetPaint(new Color(30, 144, 255, 200));
    context.FillPath(path);

    context.SetStroke(new Stroke(4f, Join.Round));
    context.SetPaint(Color.White);
    context.StrokePath(path);

    context.Flush();
    context.RenderToPixmap(pixmap);

    byte[] pngData = pixmap.ToPng();
    string outputPath = Path.Combine(AppContext.BaseDirectory, "vello-integration.png");
    File.WriteAllBytes(outputPath, pngData);
    return outputPath;
}

static unsafe string RenderFastPathScene()
{
    const ushort width = 256;
    const ushort height = 256;

    using var context = new NativeRenderContext(width, height);
    using var pixmap = new NativePixmap(width, height);

    context.SetPaintSolid(255, 255, 255, 255);
    context.FillRect(new VelloRect { X0 = 0, Y0 = 0, X1 = width, Y1 = height });

    var gradientStops = new[]
    {
        new VelloColorStop { Offset = 0f, R = 255, G = 99, B = 71, A = 255 },
        new VelloColorStop { Offset = 1f, R = 65, G = 105, B = 225, A = 255 }
    };
    context.SetPaintLinearGradient(0, 0, width, height, gradientStops, VelloExtend.Pad);
    context.FillRect(new VelloRect { X0 = 40, Y0 = 40, X1 = width - 40, Y1 = height - 40 });

    using var path = new NativeBezPath();
    path.MoveTo(width / 2.0, 36);
    path.LineTo(width - 36, height - 36);
    path.LineTo(36, height - 36);
    path.Close();

    context.SetPaintSolid(30, 144, 255, 190);
    context.FillPath(path);

    var stroke = new VelloStroke
    {
        Width = 4f,
        Join = VelloJoin.Round,
        StartCap = VelloCap.Round,
        EndCap = VelloCap.Round,
        MiterLimit = 4f
    };
    context.SetStroke(in stroke);
    context.SetPaintSolid(255, 255, 255, 255);
    context.StrokePath(path);

    context.Flush();
    context.RenderToPixmap(pixmap);

    string outputPath = Path.Combine(AppContext.BaseDirectory, "vello-fastpath.png");

    byte* pngPtr = null;
    nuint pngLen = 0;
    NativeResult.ThrowIfFailed(
        NativeMethods.Pixmap_ToPng(pixmap.Handle, &pngPtr, &pngLen),
        nameof(NativeMethods.Pixmap_ToPng));
    try
    {
        int length = checked((int)pngLen);
        byte[] buffer = new byte[length];
        new ReadOnlySpan<byte>(pngPtr, length).CopyTo(buffer);
        File.WriteAllBytes(outputPath, buffer);
    }
    finally
    {
        NativeMethods.PngDataFree(pngPtr, pngLen);
    }

    return outputPath;
}
