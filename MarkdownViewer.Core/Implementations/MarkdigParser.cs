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
                        Text = heading.Inline?.FirstChild?.ToString() ?? string.Empty
                    };
                    yield return element;
                }
                else if (block is ParagraphBlock paragraph)
                {
                    var element = new ParagraphElement
                    {
                        RawText = blockText,
                        ElementType = Elements.MarkdownElementType.Paragraph,
                        Text = paragraph.Inline?.ToString() ?? string.Empty
                    };
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
                                    text = paragraph?.Inline?.ToString() ?? string.Empty;
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
                                ? headerRow
                                    .Select(cell => cell?.ToString()?.Trim() ?? string.Empty)
                                    .ToList()
                                : new List<string>(),
                        Rows = table
                            .Skip(1)
                            .Where(row => row is TableRow)
                            .Select(
                                row =>
                                    ((TableRow)row)
                                        .Select(cell => cell?.ToString()?.Trim() ?? string.Empty)
                                        .ToList()
                            )
                            .ToList()
                    };
                    yield return element;
                }
            }
        }
    }
}
