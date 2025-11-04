using Vello;

namespace Vello.Avalonia.Rendering;

/// <summary>
/// Represents a render source capable of drawing into a <see cref="RenderContext"/>.
/// </summary>
public interface IVelloRenderer
{
    /// <summary>
    /// Renders content into the supplied <paramref name="context"/>.
    /// </summary>
    /// <param name="context">The Vello render context to draw into.</param>
    /// <param name="pixelWidth">The target surface width in device pixels.</param>
    /// <param name="pixelHeight">The target surface height in device pixels.</param>
    void Render(RenderContext context, int pixelWidth, int pixelHeight);
}
