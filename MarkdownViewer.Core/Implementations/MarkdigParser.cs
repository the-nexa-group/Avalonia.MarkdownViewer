using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using MarkdownViewer.Core.Elements;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;
using Markdig.Extensions.Tables;
using System;
using Markdig.Parsers;

namespace MarkdownViewer.Core.Implementations
{
    public class MarkdigParser : IMarkdownParser
    {
        private readonly MarkdownPipeline _pipeline;

        public MarkdigParser()
        {
            _pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        }

        public async IAsyncEnumerable<MarkdownElement> ParseStreamAsync(
            Stream stream,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
        )
        {
            using var reader = new StreamReader(stream);
            var currentBlock = new StringBuilder();
            string? line;

            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (cancellationToken.IsCancellationRequested)
                    yield break;

                if (string.IsNullOrWhiteSpace(line) && currentBlock.Length > 0)
                {
                    foreach (var element in ParseBlock(currentBlock.ToString()))
                    {
                        yield return element;
                    }
                    currentBlock.Clear();
                }
                else
                {
                    currentBlock.AppendLine(line);
                }
            }

            if (currentBlock.Length > 0)
            {
                foreach (var element in ParseBlock(currentBlock.ToString()))
                {
                    yield return element;
                }
            }
        }

        public async IAsyncEnumerable<MarkdownElement> ParseTextAsync(
            string text,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
        )
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(text));
            await foreach (var element in ParseStreamAsync(stream, cancellationToken))
            {
                yield return element;
            }
        }

        private IEnumerable<MarkdownElement> ParseBlock(string blockText)
        {
            var document = Markdown.Parse(blockText, _pipeline);

            foreach (var block in document)
            {
                if (block is HeadingBlock heading)
                {
                    var element = new HeadingElement
                    {
                        RawText = blockText,
                        ElementType = Elements.MarkdownElementType.Heading,
                        Level = heading.Level,
                        Text = ProcessInlineElements(heading.Inline) ?? string.Empty
                    };
                    yield return element;
                }
                else if (block is ParagraphBlock paragraph)
                {
                    var element = new ParagraphElement
                    {
                        RawText = blockText,
                        ElementType = Elements.MarkdownElementType.Paragraph,
                        Text = string.Empty,
                        Inlines = new List<MarkdownElement>()
                    };

                    if (paragraph.Inline != null)
                    {
                        foreach (var inline in paragraph.Inline)
                        {
                            if (inline is LinkInline link)
                            {
                                if (link.IsImage)
                                {
                                    element.Inlines.Add(
                                        new ImageElement
                                        {
                                            RawText = link.ToString() ?? string.Empty,
                                            ElementType = Elements.MarkdownElementType.Image,
                                            Source = link.Url ?? string.Empty,
                                            Title = link.Title ?? string.Empty,
                                            Alt = link.Label?.ToString() ?? string.Empty
                                        }
                                    );
                                }
                                else
                                {
                                    element.Inlines.Add(
                                        new LinkElement
                                        {
                                            RawText = link.ToString() ?? string.Empty,
                                            ElementType = Elements.MarkdownElementType.Link,
                                            Text = ProcessInlineElements(link),
                                            Url = link.Url ?? string.Empty,
                                            Title = link.Title ?? string.Empty
                                        }
                                    );
                                }
                            }
                            else if (inline is EmphasisInline emphasis)
                            {
                                element.Inlines.Add(
                                    new EmphasisElement
                                    {
                                        RawText = emphasis.ToString() ?? string.Empty,
                                        ElementType = Elements.MarkdownElementType.Emphasis,
                                        Text = ProcessInlineElements(emphasis),
                                        IsStrong = emphasis.DelimiterCount == 2
                                    }
                                );
                            }
                            else if (inline is LiteralInline literal)
                            {
                                element.Inlines.Add(
                                    new TextElement
                                    {
                                        RawText = literal.ToString() ?? string.Empty,
                                        ElementType = Elements.MarkdownElementType.Text,
                                        Text = literal.Content.ToString()
                                    }
                                );
                            }
                            else
                            {
                                element.Inlines.Add(
                                    new TextElement
                                    {
                                        RawText = inline.ToString() ?? string.Empty,
                                        ElementType = Elements.MarkdownElementType.Text,
                                        Text = ProcessInline(inline)
                                    }
                                );
                            }
                        }
                    }

                    yield return element;
                }
                else if (block is CodeBlock codeBlock)
                {
                    var element = new CodeBlockElement
                    {
                        RawText = blockText,
                        ElementType = Elements.MarkdownElementType.CodeBlock,
                        Code = string.Join(
                            Environment.NewLine,
                            ((FencedCodeBlock)codeBlock).Lines.Lines.Select(x => x.ToString())
                        ),
                        Language = ((FencedCodeBlock)codeBlock).Info ?? string.Empty
                    };
                    yield return element;
                }
                else if (block is ListBlock listBlock)
                {
                    var element = new ListElement
                    {
                        RawText = blockText,
                        ElementType = Elements.MarkdownElementType.List,
                        IsOrdered = listBlock.IsOrdered,
                        Items = listBlock
                            .Select(item =>
                            {
                                var listItem = item as ListItemBlock;
                                var text = string.Empty;
                                if (listItem != null)
                                {
                                    var paragraph = listItem
                                        .Descendants<ParagraphBlock>()
                                        .FirstOrDefault();
                                    text =
                                        paragraph?.Inline != null
                                            ? ProcessInlineElements(paragraph.Inline)
                                            : string.Empty;
                                }
                                return new ListItemElement
                                {
                                    RawText = item.ToString() ?? string.Empty,
                                    ElementType = Elements.MarkdownElementType.ListItem,
                                    Text = text,
                                    Level = 0
                                };
                            })
                            .ToList()
                    };
                    yield return element;
                }
                else if (block is QuoteBlock quoteBlock)
                {
                    var element = new QuoteElement
                    {
                        RawText = blockText,
                        ElementType = Elements.MarkdownElementType.Quote,
                        Text = string.Join(
                            Environment.NewLine,
                            quoteBlock.Select(line => line.ToString())
                        )
                    };
                    yield return element;
                }
                else if (block is ThematicBreakBlock)
                {
                    var element = new HorizontalRuleElement
                    {
                        RawText = blockText,
                        ElementType = Elements.MarkdownElementType.HorizontalRule
                    };
                    yield return element;
                }
                else if (block is Table table)
                {
                    var element = new TableElement
                    {
                        RawText = blockText,
                        ElementType = Elements.MarkdownElementType.Table,
                        Headers =
                            table.Count > 0 && table[0] is TableRow headerRow
                                ? headerRow.Select(cell => GetCellContent((TableCell)cell)).ToList()
                                : new List<string>(),
                        Rows = table
                            .Skip(1)
                            .Where(row => row is TableRow)
                            .Select(
                                row =>
                                    ((TableRow)row)
                                        .Select(cell => GetCellContent((TableCell)cell))
                                        .ToList()
                            )
                            .ToList()
                    };
                    yield return element;
                }
            }
        }

        private string GetCellContent(Block cell)
        {
            if (cell == null)
                return string.Empty;

            var paragraph = cell.Descendants<ParagraphBlock>().FirstOrDefault();
            if (paragraph?.Inline == null)
                return string.Empty;

            return ProcessInlineElements(paragraph.Inline);
        }

        private string ProcessInlineElements(ContainerInline? container)
        {
            if (container == null)
                return string.Empty;

            var builder = new StringBuilder();
            foreach (var inline in container)
            {
                builder.Append(ProcessInline(inline));
            }
            return builder.ToString();
        }

        private string ProcessInline(Inline inline)
        {
            return inline switch
            {
                LiteralInline literal => literal.Content.ToString() ?? string.Empty,
                LinkInline link => link.Title ?? link.Url ?? string.Empty,
                EmphasisInline emphasis => ProcessInlineElements(emphasis),
                _ => inline.ToString() ?? string.Empty
            };
        }
    }
}
