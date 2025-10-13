using Avalonia.Controls;
using MarkdownViewer.Core.Elements;
using MarkdownViewer.Core.Renderers;
using MarkdownViewer.Core.Services;

namespace MarkdownViewer.Core.Implementations.Modular;

public class DefaultLinkRenderer : ILinkRenderer
{
    public Control? RenderLink(IModularMarkdownRenderer markdownRenderer, Control markdownControl, LinkElement element)
    {
        var textBlock = DefaultUtils.CreateTextBlock(element.Text, [DefaultClasses.Markdown, DefaultClasses.Link]);
        
        textBlock.PointerPressed += (s, e) =>
        {
            if (e.GetCurrentPoint(null).Properties.IsLeftButtonPressed)
                DefaultLinkHandler.HandleLink(element.Url);
        };
        
        return textBlock;
    }
}