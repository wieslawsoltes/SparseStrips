using System;
using System.Runtime.InteropServices;
using Vello;
using Vello.Native;
using Xunit;

namespace Vello.Tests.Interop;

public class RecordingNullCallbackTests
{
    [Fact]
    public void RecordWithNullRecordingThrows()
    {
        using var ctx = new RenderContext(32, 32, new RenderSettings(SimdLevel.Avx2, 0, RenderMode.OptimizeSpeed));
        Assert.Throws<ArgumentNullException>(() => ctx.Record(null!, _ => { }));
    }

    [Fact]
    public void RecordWithNullActionThrows()
    {
        using var ctx = new RenderContext(32, 32, new RenderSettings(SimdLevel.Avx2, 0, RenderMode.OptimizeSpeed));
        using var recording = new Recording();
        Assert.Throws<ArgumentNullException>(() => ctx.Record(recording, null!));
    }

    [Fact]
    public void NativeRecordWithNullRecordingReturnsError()
    {
        nint ctx = NativeMethods.RenderContext_New(32, 32);
        Assert.NotEqual(IntPtr.Zero, ctx);

        try
        {
            var result = NativeMethods.RenderContext_Record(ctx, 0, IntPtr.Zero, IntPtr.Zero);
            Assert.NotEqual(NativeMethods.VELLO_OK, result);
        }
        finally
        {
            NativeMethods.RenderContext_Free(ctx);
        }
    }
}
