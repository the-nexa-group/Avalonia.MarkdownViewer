using Avalonia.Controls;
using MarkdownViewer.Core.Elements;
using MarkdownViewer.Core.Renderers;

namespace MarkdownViewer.Core.Implementations.Modular;

public class DefaultLinkRenderer : ILinkRenderer
{
    public Control? RenderLink(IModularMarkdownRenderer markdownRenderer, Control markdownControl, LinkElement element)
    {
        var button = new Button
        {
            Classes = { DefaultClasses.Markdown, DefaultClasses.Link },
            Content = new TextBlock
            {
                Classes = { DefaultClasses.Markdown, DefaultClasses.Link },
                Text = element.Text,
            }
        };
        
        return button;
    }
}