using Avalonia.Controls;
using Avalonia.Media;
using MarkdownViewer.Core.Elements;
using MarkdownViewer.Core.Renderers;

namespace MarkdownViewer.Core.Implementations.Modular;

public class DefaultTextRenderer : 
    ITextRenderer, 
    IQuoteRenderer
{
    public Control? RenderText(IModularMarkdownRenderer markdownRenderer, Control markdownControl, TextElement element)
    {
        return DefaultUtils.CreateTextBlock(element.Text, [DefaultClasses.Markdown]);
    }

    public Control? RenderParagraph(IModularMarkdownRenderer markdownRenderer, Control markdownControl, ParagraphElement element)
    {
        if (element.Inlines is [ImageElement imageElement])
            return markdownRenderer.ImageRenderer.RenderImage(markdownRenderer, markdownControl, imageElement);
        
        var textBlock = DefaultUtils.CreateTextBlock(null, [DefaultClasses.Markdown, DefaultClasses.Paragraph]);

        foreach (var inlineElement in element.Inlines)
            markdownRenderer.RenderInlineElement(markdownControl, textBlock, inlineElement);

        return textBlock;
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
        var textBlock = DefaultUtils.CreateTextBlock(element.Text, [DefaultClasses.Markdown]);
        if (element.IsStrong)
            textBlock.FontWeight = FontWeight.Bold;
        if (element.IsItalic)
            textBlock.FontStyle = FontStyle.Italic;

        return textBlock;
    }
    
    public Control? RenderQuote(IModularMarkdownRenderer markdownRenderer, Control markdownControl, QuoteElement element)
    {
        var textBlock = DefaultUtils.CreateTextBlock(element.Text, [DefaultClasses.Markdown, DefaultClasses.Quote]);
        
        foreach (var childElement in element.Inlines) 
            markdownRenderer.RenderInlineElement(markdownControl, textBlock, childElement);
    
        return new Border
        {
            Classes = { DefaultClasses.Markdown, DefaultClasses.Quote },
            Child = textBlock
        };
    }
}