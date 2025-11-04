namespace Vello.Samples.Avalonia.Rendering;

public readonly struct FrameStats
{
    public FrameStats(int complexity, int elementCount, double frameTimeMilliseconds, double framesPerSecond)
    {
        Complexity = complexity;
        ElementCount = elementCount;
        FrameTimeMilliseconds = frameTimeMilliseconds;
        FramesPerSecond = framesPerSecond;
    }

    public int Complexity { get; }
    public int ElementCount { get; }
    public double FrameTimeMilliseconds { get; }
    public double FramesPerSecond { get; }
}
