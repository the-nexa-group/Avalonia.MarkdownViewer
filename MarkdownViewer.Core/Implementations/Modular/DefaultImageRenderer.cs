using Avalonia.Controls;
using MarkdownViewer.Core.Elements;
using MarkdownViewer.Core.Renderers;

namespace MarkdownViewer.Core.Implementations.Modular;

public class DefaultImageRenderer : IImageRenderer
{
    public Control? RenderImage(IModularMarkdownRenderer markdownRenderer, Control markdownControl, ImageElement element)
    {
        throw new NotImplementedException();
    }
}