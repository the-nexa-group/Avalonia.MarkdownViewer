using Avalonia.Controls;
using MarkdownViewer.Core.Elements;
using MarkdownViewer.Core.Renderers;

namespace MarkdownViewer.Core.Implementations.Modular;

public class DefaultTextRenderer : 
    ITextRenderer, 
    IQuoteRenderer
{
    public Control? RenderText(IModularMarkdownRenderer markdownRenderer, Control markdownControl, TextElement element)
    {
        return DefaultUtils.CreateTextBlock(element.Text, [DefaultClasses.Markdown, DefaultClasses.Text]);
    }

    public Control? RenderHeading(IModularMarkdownRenderer markdownRenderer, Control markdownControl, HeadingElement element)
    {
        string headingClass = element.Level switch
        {
            MarkdownHeadingLevel.H1 => DefaultClasses.H1,
            MarkdownHeadingLevel.H2 => DefaultClasses.H2,
            MarkdownHeadingLevel.H3 => DefaultClasses.H3,
            MarkdownHeadingLevel.H4 => DefaultClasses.H4,
            MarkdownHeadingLevel.H5 => DefaultClasses.H5,
            _ => throw new ArgumentOutOfRangeException()
        };

        return DefaultUtils.CreateTextBlock(element.Text, [DefaultClasses.Markdown, headingClass]);
    }
    
    public Control? RenderEmphasis(IModularMarkdownRenderer markdownRenderer, Control markdownControl, EmphasisElement element)
    {
        throw new NotImplementedException();
    }
    
    public Control? RenderQuote(IModularMarkdownRenderer markdownRenderer, Control markdownControl, QuoteElement element)
    {
        var panel = new StackPanel();

        foreach (var childElement in element.Inlines) 
        {
            var childControl = markdownRenderer.RenderElement(markdownControl, childElement);
            if (childControl != null)
            {
                panel.Children.Add(childControl);
            }
        }
    
        return new Border
        {
            Classes = { DefaultClasses.Markdown, DefaultClasses.Quote },
            Child = panel
        };
    }
}