using System.Collections.Generic;
using System.Runtime.InteropServices;
using Vello;
using Vello.Geometry;

namespace Vello.Samples.Avalonia.Rendering;

internal sealed class MotionMarkScene : IDisposable
{
    private const int GridWidth = 80;
    private const int GridHeight = 40;

    private static readonly Color[] s_palette =
    [
        new Color(0x10, 0x10, 0x10),
        new Color(0x80, 0x80, 0x80),
        new Color(0xC0, 0xC0, 0xC0),
        new Color(0x10, 0x10, 0x10),
        new Color(0x80, 0x80, 0x80),
        new Color(0xC0, 0xC0, 0xC0),
        new Color(0xE0, 0x10, 0x40),
    ];

    private static readonly (int X, int Y)[] s_offsets =
    [
        (-4, 0),
        (2, 0),
        (1, -2),
        (1, 2),
    ];

    private readonly List<Element> _elements = new();
    private readonly BezPath _path = new();
    private readonly Random _random = new();
    private GridPoint _lastGridPoint = new(GridWidth / 2, GridHeight / 2);
    private int _complexity = 8;
    private bool _disposed;

    public int Complexity => _complexity;
    public int ElementCount => _elements.Count;

    public void SetComplexity(int complexity)
    {
        complexity = Math.Clamp(complexity, 0, 24);
        if (_complexity == complexity)
            return;

        _complexity = complexity;
        Resize(ComputeElementCount(_complexity));
    }

    public void Render(RenderContext context, int pixelWidth, int pixelHeight)
    {
        Resize(ComputeElementCount(_complexity));

        context.Reset();

        if (_elements.Count == 0)
        {
            context.SetPaint(new Color(0, 0, 0, 0));
            context.FillRect(Vello.Geometry.Rect.FromXYWH(0, 0, pixelWidth, pixelHeight));
            context.Flush();
            return;
        }

        double scaleX = pixelWidth / (double)(GridWidth + 1);
        double scaleY = pixelHeight / (double)(GridHeight + 1);
        double uniformScale = Math.Min(scaleX, scaleY);
        double offsetsX = (pixelWidth - uniformScale * (GridWidth + 1)) * 0.5;
        double offsetsY = (pixelHeight - uniformScale * (GridHeight + 1)) * 0.5;

        context.SetPaint(new Color(12, 16, 24));
        context.FillRect(Vello.Geometry.Rect.FromXYWH(0, 0, pixelWidth, pixelHeight));

        Span<Element> elements = CollectionsMarshal.AsSpan(_elements);
        _path.Clear();
        bool pathStarted = false;

        for (int i = 0; i < elements.Length; i++)
        {
            ref Element element = ref elements[i];
            if (!pathStarted)
            {
                Point start = element.Start.ToPoint(uniformScale, offsetsX, offsetsY);
                _path.MoveTo(start);
                pathStarted = true;
            }

            switch (element.Kind)
            {
                case SegmentKind.Line:
                {
                    Point end = element.End.ToPoint(uniformScale, offsetsX, offsetsY);
                    _path.LineTo(end);
                    break;
                }

                case SegmentKind.Quad:
                {
                    Point c1 = element.Control1.ToPoint(uniformScale, offsetsX, offsetsY);
                    Point end = element.End.ToPoint(uniformScale, offsetsX, offsetsY);
                    _path.QuadTo(c1, end);
                    break;
                }

                case SegmentKind.Cubic:
                {
                    Point c1 = element.Control1.ToPoint(uniformScale, offsetsX, offsetsY);
                    Point c2 = element.Control2.ToPoint(uniformScale, offsetsX, offsetsY);
                    Point end = element.End.ToPoint(uniformScale, offsetsX, offsetsY);
                    _path.CurveTo(c1, c2, end);
                    break;
                }
            }

            bool finalize = element.Split || i == elements.Length - 1;
            if (finalize)
            {
                context.SetPaint(element.Color);
                context.SetStroke(new Stroke(
                    width: element.Width,
                    join: Join.Round,
                    startCap: Cap.Round,
                    endCap: Cap.Round));
                context.StrokePath(_path);
                _path.Clear();
                pathStarted = false;
            }

            if (_random.NextDouble() > 0.995)
            {
                element.Split = !element.Split;
            }
        }

        context.Flush();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _path.Dispose();
        _disposed = true;
    }

    private void Resize(int count)
    {
        int current = _elements.Count;
        if (count == current)
            return;

        if (count < current)
        {
            _elements.RemoveRange(count, current - count);
            _lastGridPoint = count > 0
                ? _elements[^1].End
                : new GridPoint(GridWidth / 2, GridHeight / 2);
            return;
        }

        _elements.Capacity = Math.Max(_elements.Capacity, count);
        if (current == 0)
        {
            _lastGridPoint = new GridPoint(GridWidth / 2, GridHeight / 2);
        }
        else
        {
            _lastGridPoint = _elements[^1].End;
        }

        for (int i = current; i < count; i++)
        {
            Element element = CreateRandomElement(_lastGridPoint);
            _elements.Add(element);
            _lastGridPoint = element.End;
        }
    }

    private Element CreateRandomElement(GridPoint last)
    {
        int segType = _random.Next(4);
        GridPoint next = RandomPoint(last);

        Element element = default;
        element.Start = last;

        if (segType < 2)
        {
            element.Kind = SegmentKind.Line;
            element.End = next;
        }
        else if (segType == 2)
        {
            GridPoint p2 = RandomPoint(next);
            element.Kind = SegmentKind.Quad;
            element.Control1 = next;
            element.End = p2;
        }
        else
        {
            GridPoint p2 = RandomPoint(next);
            GridPoint p3 = RandomPoint(next);
            element.Kind = SegmentKind.Cubic;
            element.Control1 = next;
            element.Control2 = p2;
            element.End = p3;
        }

        element.Color = s_palette[_random.Next(s_palette.Length)];
        element.Width = (float)(Math.Pow(_random.NextDouble(), 5) * 20.0 + 1.0);
        element.Split = _random.Next(2) == 0;
        return element;
    }

    private static int ComputeElementCount(int complexity)
    {
        if (complexity < 10)
        {
            return (complexity + 1) * 1_000;
        }

        int extended = (complexity - 8) * 10_000;
        return Math.Min(extended, 120_000);
    }

    private GridPoint RandomPoint(GridPoint last)
    {
        var offset = s_offsets[_random.Next(s_offsets.Length)];

        int x = last.X + offset.X;
        if (x < 0 || x > GridWidth)
        {
            x -= offset.X * 2;
        }

        int y = last.Y + offset.Y;
        if (y < 0 || y > GridHeight)
        {
            y -= offset.Y * 2;
        }

        return new GridPoint(x, y);
    }

    private enum SegmentKind : byte
    {
        Line,
        Quad,
        Cubic
    }

    private struct Element
    {
        public SegmentKind Kind;
        public GridPoint Start;
        public GridPoint Control1;
        public GridPoint Control2;
        public GridPoint End;
        public Color Color;
        public float Width;
        public bool Split;
    }

    private readonly struct GridPoint
    {
        public GridPoint(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; }
        public int Y { get; }

        public Point ToPoint(double scale, double offsetX, double offsetY)
        {
            double px = offsetX + (X + 0.5) * scale;
            double py = offsetY + (Y + 0.5) * scale;
            return new Point(px, py);
        }
    }
}
