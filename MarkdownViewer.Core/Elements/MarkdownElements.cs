namespace MarkdownViewer.Core.Elements
{
    public enum MarkdownElementType
    {
        Heading,
        Paragraph,
        CodeBlock,
        Image,
        Link,
        List,
        ListItem,
        Quote,
        HorizontalRule,
        Table
    }

    public abstract class MarkdownElement
    {
        public required string RawText { get; set; }
        public MarkdownElementType ElementType { get; set; }
    }

    public class HeadingElement : MarkdownElement
    {
        public int Level { get; set; }
        public required string Text { get; set; }
    }

    public class ParagraphElement : MarkdownElement
    {
        public required string Text { get; set; }
    }

    public class CodeBlockElement : MarkdownElement
    {
        public required string Code { get; set; }
        public required string Language { get; set; }
    }

    public class ImageElement : MarkdownElement
    {
        public required string Source { get; set; }
        public string? Alt { get; set; }
        public string? Title { get; set; }
    }

    public class LinkElement : MarkdownElement
    {
        public required string Url { get; set; }
        public required string Text { get; set; }
        public string? Title { get; set; }
    }

    public class ListElement : MarkdownElement
    {
        public bool IsOrdered { get; set; }
        public List<ListItemElement> Items { get; set; } = new();
    }

    public class ListItemElement : MarkdownElement
    {
        public required string Text { get; set; }
        public int Level { get; set; }
    }

    public class QuoteElement : MarkdownElement
    {
        public required string Text { get; set; }
    }

    public class HorizontalRuleElement : MarkdownElement { }

    public class TableElement : MarkdownElement
    {
        public List<string> Headers { get; set; } = new();
        public List<List<string>> Rows { get; set; } = new();
    }
}
