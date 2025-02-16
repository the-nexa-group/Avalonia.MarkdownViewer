using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Input;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Globalization;
using Microsoft.Extensions.Logging;
using MarkdownViewer.Core.Controls;
using MarkdownViewer.Core.Elements;
using MarkdownViewer.Core.Services;
using System.IO;

namespace MarkdownViewer.Core.Implementations
{
    public class AvaloniaMarkdownRenderer : IMarkdownRenderer
    {
        private readonly FontFamily _defaultFontFamily = FontFamily.Default;
        private readonly double _baseFontSize = 14;
        private readonly IImageCache _imageCache;
        private readonly ILogger _logger;

        public event EventHandler<string>? LinkClicked;

        public AvaloniaMarkdownRenderer(
            IImageCache imageCache,
            ILogger<AvaloniaMarkdownRenderer> logger
        )
        {
            _imageCache = imageCache;
            _logger = logger;
        }

        public Control RenderDocument(string markdown)
        {
            var parser = new MarkdigParser();
            var elements = parser.ParseTextAsync(markdown).ToBlockingEnumerable();

            var panel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(10)
            };

            foreach (var element in elements)
            {
                var control = RenderElement(element);
                panel.Children.Add(control);
            }

            return panel;
        }

        private Control RenderElement(MarkdownElement element)
        {
            return element switch
            {
                HeadingElement heading => RenderHeading(heading),
                ParagraphElement paragraph => RenderParagraph(paragraph),
                CodeBlockElement codeBlock => RenderCodeBlock(codeBlock),
                ListElement list => RenderList(list),
                TaskListElement taskList => RenderTaskList(taskList),
                QuoteElement quote => RenderQuote(quote),
                ImageElement image => RenderImage(image),
                LinkElement link => RenderLink(link),
                TableElement table => RenderTable(table),
                EmphasisElement emphasis => RenderEmphasis(emphasis),
                HorizontalRuleElement => RenderHorizontalRule(),
                _ => new TextBlock { Text = "Unsupported element" }
            };
        }

        public void UpdateElement(IControl control, MarkdownElement element)
        {
            switch (element)
            {
                case HeadingElement heading when control is TextBlock textBlock:
                    UpdateHeading(textBlock, heading);
                    break;
                case ParagraphElement paragraph when control is TextBlock textBlock:
                    UpdateParagraph(textBlock, paragraph);
                    break;
                case CodeBlockElement codeBlock when control is TextBox textBox:
                    UpdateCodeBlock(textBox, codeBlock);
                    break;
                case ListElement list when control is StackPanel panel:
                    UpdateList(panel, list);
                    break;
                case TaskListElement taskList when control is StackPanel panel:
                    UpdateTaskList(panel, taskList);
                    break;
                case QuoteElement quote when control is Border border:
                    UpdateQuote(border, quote);
                    break;
                case ImageElement image when control is Image img:
                    UpdateImage(img, image);
                    break;
                case LinkElement link when control is Button btn:
                    UpdateLink(btn, link);
                    break;
                case TableElement table when control is Grid grid:
                    UpdateTable(grid, table);
                    break;
                case EmphasisElement emphasis when control is TextBlock textBlock:
                    UpdateEmphasis(textBlock, emphasis);
                    break;
            }
        }

        private Control RenderHeading(HeadingElement heading)
        {
            var textBlock = new TextBlock
            {
                Text = heading.Text,
                FontFamily = _defaultFontFamily,
                FontWeight = FontWeight.Bold,
                FontSize = GetHeadingFontSize(heading.Level),
                Margin = new Thickness(0, heading.Level == 1 ? 20 : 15, 0, 10)
            };
            return textBlock;
        }

        private Control RenderParagraph(ParagraphElement paragraph)
        {
            // 如果段落只包含一个图片元素，直接返回图片控件
            if (paragraph.Inlines.Count == 1 && paragraph.Inlines[0] is ImageElement image)
            {
                return RenderImage(image);
            }

            var textBlock = new TextBlock
            {
                FontFamily = _defaultFontFamily,
                FontSize = _baseFontSize,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 10)
            };

            if (paragraph.Inlines != null)
            {
                foreach (var inline in paragraph.Inlines)
                {
                    if (inline == null)
                        continue;

                    switch (inline)
                    {
                        case ImageElement img:
                            var inlineImage = new Image
                            {
                                Stretch = Stretch.Uniform,
                                StretchDirection = StretchDirection.DownOnly,
                                MaxHeight = 400,
                                Margin = new Thickness(0, 0, 0, 10)
                            };
                            LoadImageAsync(inlineImage, img.Source);

                            // 创建一个内联容器来包含图片
                            var inlineContainer = new InlineUIContainer { Child = inlineImage };
                            textBlock.Inlines?.Add(inlineContainer);
                            break;
                        case Elements.TextElement text:
                            textBlock.Inlines?.Add(new Run { Text = text.Text ?? string.Empty });
                            break;
                        case EmphasisElement emphasis:
                            if (emphasis.IsStrong)
                            {
                                var bold = new Bold
                                {
                                    Inlines = { new Run { Text = emphasis.Text ?? string.Empty } }
                                };
                                textBlock.Inlines?.Add(bold);
                            }
                            else
                            {
                                var italic = new Italic
                                {
                                    Inlines = { new Run { Text = emphasis.Text ?? string.Empty } }
                                };
                                textBlock.Inlines?.Add(italic);
                            }
                            break;
                        case LinkElement link:
                            var span = new Span
                            {
                                Foreground = new SolidColorBrush(Color.FromRgb(0, 122, 255)),
                                TextDecorations = TextDecorations.Underline
                            };
                            span.Inlines?.Add(new Run { Text = link.Text ?? string.Empty });
                            textBlock.Inlines?.Add(span);
                            break;
                        case CodeInlineElement code:
                            var codeText = new TextBlock
                            {
                                Text = code.Code ?? string.Empty,
                                FontFamily = new FontFamily("Consolas, Menlo, Monaco, monospace"),
                                VerticalAlignment = VerticalAlignment.Center,
                                TextAlignment = TextAlignment.Center,
                                BaselineOffset = 1,
                                FontSize = _baseFontSize * 0.9
                            };
                            var codeBorder = new Border
                            {
                                Child = codeText,
                                Padding = new Thickness(6, 2, 6, 2),
                                Background = new SolidColorBrush(Color.FromRgb(246, 248, 250)),
                                BorderBrush = new SolidColorBrush(Color.FromRgb(234, 236, 239)),
                                BorderThickness = new Thickness(1),
                                VerticalAlignment = VerticalAlignment.Center,
                                CornerRadius = new CornerRadius(4),
                                Margin = new Thickness(0, 0, 0, -1)
                            };
                            textBlock.Inlines?.Add(new InlineUIContainer { Child = codeBorder });
                            break;
                    }
                }
            }

            return textBlock;
        }

        private string GetCopyButtonText()
        {
            // 获取当前系统的语言代码
            var currentCulture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName.ToLower();

            return currentCulture switch
            {
                "zh" => "复制",
                "ja" => "コピー",
                "ko" => "복사",
                "fr" => "Copier",
                "de" => "Kopieren",
                "es" => "Copiar",
                "it" => "Copia",
                "ru" => "Копировать",
                "pt" => "Copiar",
                "nl" => "Kopiëren",
                "pl" => "Kopiuj",
                "tr" => "Kopyala",
                "ar" => "نسخ",
                "hi" => "कॉपी",
                "th" => "คัดลอก",
                "vi" => "Sao chép",
                "cs" => "Kopírovat",
                "sv" => "Kopiera",
                "el" => "Αντιγραφή",
                "he" => "העתק",
                "hu" => "Másolás",
                "ro" => "Copiază",
                "uk" => "Копіювати",
                "fi" => "Kopioi",
                "da" => "Kopiér",
                "id" => "Salin",
                "ms" => "Salin",
                "bn" => "কপি",
                "fa" => "کپی",
                "bg" => "Копирай",
                "sk" => "Kopírovať",
                "hr" => "Kopiraj",
                "sr" => "Копирај",
                "sl" => "Kopiraj",
                "et" => "Kopeeri",
                "lv" => "Kopēt",
                "lt" => "Kopijuoti",
                "no" => "Kopier",
                _ => "Copy" // 默认英文
            };
        }

        private Control RenderCodeBlock(CodeBlockElement codeBlock)
        {
            var grid = new Grid();

            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(246, 248, 250)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(234, 236, 239)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Margin = new Thickness(0, 0, 0, 10)
            };

            var textBox = new TextBlock
            {
                Text = codeBlock.Code,
                FontFamily = new FontFamily("Consolas, Menlo, Monaco, monospace"),
                FontSize = _baseFontSize,
                Padding = new Thickness(16, 12, 16, 12),
                TextWrapping = TextWrapping.Wrap
            };

            var copyButton = new Button
            {
                Content = GetCopyButtonText(),
                Margin = new Thickness(8),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                IsVisible = false,
                Padding = new Thickness(8, 4, 8, 4),
                CornerRadius = new CornerRadius(4),
                Background = new SolidColorBrush(Color.FromRgb(246, 248, 250)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(234, 236, 239)),
                BorderThickness = new Thickness(1)
            };

            copyButton.Click += async (s, e) =>
            {
                var topLevel = TopLevel.GetTopLevel(copyButton);
                if (topLevel?.Clipboard != null)
                {
                    await topLevel.Clipboard.SetTextAsync(codeBlock.Code);
                }
            };

            border.Child = textBox;
            grid.Children.Add(border);
            Grid.SetColumn(border, 0);

            grid.Children.Add(copyButton);
            Grid.SetColumn(copyButton, 0);

            grid.PointerEntered += (s, e) => copyButton.IsVisible = true;
            grid.PointerExited += (s, e) => copyButton.IsVisible = false;

            return grid;
        }

        private void UpdateHeading(TextBlock textBlock, HeadingElement heading)
        {
            textBlock.Text = heading.Text;
            textBlock.FontSize = GetHeadingFontSize(heading.Level);
        }

        private void UpdateParagraph(TextBlock textBlock, ParagraphElement paragraph)
        {
            if (textBlock.Inlines == null)
                return;

            textBlock.Inlines.Clear();
            foreach (var inline in paragraph.Inlines)
            {
                if (inline == null)
                    continue;

                switch (inline)
                {
                    case Elements.TextElement text:
                        textBlock.Inlines.Add(new Run { Text = text.Text ?? string.Empty });
                        break;
                    case EmphasisElement emphasis:
                        if (emphasis.IsStrong)
                        {
                            var bold = new Bold
                            {
                                Inlines = { new Run { Text = emphasis.Text ?? string.Empty } }
                            };
                            textBlock.Inlines.Add(bold);
                        }
                        else
                        {
                            var italic = new Italic
                            {
                                Inlines = { new Run { Text = emphasis.Text ?? string.Empty } }
                            };
                            textBlock.Inlines.Add(italic);
                        }
                        break;
                    case LinkElement link:
                        var span = new Span
                        {
                            Foreground = new SolidColorBrush(Color.FromRgb(0, 122, 255)),
                            TextDecorations = TextDecorations.Underline
                        };
                        span.Inlines?.Add(new Run { Text = link.Text ?? string.Empty });
                        textBlock.Inlines.Add(span);
                        break;
                }
            }
        }

        private void UpdateCodeBlock(TextBox textBox, CodeBlockElement codeBlock)
        {
            textBox.Text = codeBlock.Code;
        }

        private double GetHeadingFontSize(int level)
        {
            return level switch
            {
                1 => _baseFontSize * 2.0,
                2 => _baseFontSize * 1.7,
                3 => _baseFontSize * 1.4,
                4 => _baseFontSize * 1.2,
                5 => _baseFontSize * 1.1,
                _ => _baseFontSize
            };
        }

        private Control RenderList(ListElement list)
        {
            var panel = new StackPanel { Spacing = 5, Margin = new Thickness(0, 0, 0, 10) };

            if (list.Items != null)
            {
                foreach (var item in list.Items)
                {
                    if (item == null)
                        continue;

                    var itemPanel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Margin = new Thickness(item.Level * 20, 0, 0, 0),
                        Spacing = 5
                    };

                    // 根据层级和列表类型选择不同的符号
                    string bulletText = list.IsOrdered
                        ? $"{list.Items.IndexOf(item) + 1}."
                        : (item.Level == 0 ? "•" : "◦");

                    var bullet = new TextBlock
                    {
                        Text = bulletText,
                        Width = 20,
                        TextAlignment = TextAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Top
                    };

                    var contentPanel = new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        Spacing = 5
                    };

                    var content = new TextBlock
                    {
                        Text = item.Text ?? string.Empty,
                        TextWrapping = TextWrapping.Wrap,
                        VerticalAlignment = VerticalAlignment.Top
                    };

                    contentPanel.Children?.Add(content);

                    // 处理子项
                    if (item.Children != null && item.Children.Count > 0)
                    {
                        var subList = new ListElement
                        {
                            RawText = string.Empty,
                            Items = item.Children,
                            IsOrdered = list.IsOrdered
                        };
                        var subListControl = RenderList(subList);
                        contentPanel.Children?.Add(subListControl);
                    }

                    itemPanel.Children?.Add(bullet);
                    itemPanel.Children?.Add(contentPanel);
                    panel.Children?.Add(itemPanel);
                }
            }

            return panel;
        }

        private Control RenderTaskList(TaskListElement taskList)
        {
            var panel = new StackPanel { Spacing = 5, Margin = new Thickness(0, 0, 0, 10) };

            if (taskList.Items != null)
            {
                foreach (var item in taskList.Items)
                {
                    var itemPanel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Margin = new Thickness(item.Level * 20, 0, 0, 0),
                        Spacing = 5
                    };

                    var checkbox = new CheckBox
                    {
                        IsChecked = item.IsChecked,
                        IsEnabled = false, // 设置为只读
                        VerticalAlignment = VerticalAlignment.Top
                    };

                    var contentPanel = new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        Spacing = 5
                    };

                    var content = new TextBlock
                    {
                        Text = item.Text ?? string.Empty,
                        TextWrapping = TextWrapping.Wrap,
                        VerticalAlignment = VerticalAlignment.Top
                    };

                    contentPanel.Children?.Add(content);

                    // 处理子项
                    if (item.Children != null && item.Children.Count > 0)
                    {
                        var subList = new TaskListElement
                        {
                            RawText = string.Empty,
                            Items = item.Children
                        };
                        var subListControl = RenderTaskList(subList);
                        contentPanel.Children?.Add(subListControl);
                    }

                    itemPanel.Children?.Add(checkbox);
                    itemPanel.Children?.Add(contentPanel);
                    panel.Children?.Add(itemPanel);
                }
            }

            return panel;
        }

        private Control RenderQuote(QuoteElement quote)
        {
            var textBlock = new TextBlock
            {
                Text = quote.Text,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(10),
                Foreground = new SolidColorBrush(Color.FromRgb(108, 108, 108))
            };

            return new Border
            {
                Child = textBlock,
                BorderBrush = new SolidColorBrush(Color.FromRgb(229, 229, 229)),
                BorderThickness = new Thickness(4, 0, 0, 0),
                Background = new SolidColorBrush(Color.FromRgb(249, 249, 249)),
                Margin = new Thickness(0, 0, 0, 10),
                Padding = new Thickness(10)
            };
        }

        private Control RenderImage(ImageElement image)
        {
            var img = new Image
            {
                Stretch = Stretch.Uniform,
                StretchDirection = StretchDirection.DownOnly,
                MaxHeight = 400,
                Margin = new Thickness(0, 0, 0, 10)
            };

            LoadImageAsync(img, image.Source);
            return img;
        }

        private Control RenderLink(LinkElement link)
        {
            var run = new Run
            {
                Text = link.Text ?? string.Empty,
                TextDecorations = TextDecorations.Underline,
                Foreground = new SolidColorBrush(Color.FromRgb(0, 122, 255))
            };

            var textBlock = new TextBlock();
            if (textBlock.Inlines != null)
            {
                textBlock.Inlines.Add(run);
            }

            var button = new Button
            {
                Content = textBlock,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(0),
                Cursor = new Cursor(StandardCursorType.Hand)
            };

            button.Click += (s, e) => OnLinkClicked(link.Url ?? string.Empty);
            return button;
        }

        private void UpdateList(StackPanel panel, ListElement list)
        {
            if (panel.Children == null || list.Items == null)
                return;

            panel.Children.Clear();
            foreach (var item in list.Items)
            {
                if (item == null)
                    continue;

                var itemPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(item.Level * 20, 0, 0, 0)
                };

                var bullet = new TextBlock
                {
                    Text = list.IsOrdered ? $"{list.Items.IndexOf(item) + 1}." : "•",
                    Width = 20,
                    TextAlignment = TextAlignment.Right,
                    Margin = new Thickness(0, 0, 5, 0)
                };

                var content = new TextBlock
                {
                    Text = item.Text ?? string.Empty,
                    TextWrapping = TextWrapping.Wrap
                };

                if (itemPanel.Children != null)
                {
                    itemPanel.Children.Add(bullet);
                    itemPanel.Children.Add(content);
                }

                panel.Children.Add(itemPanel);
            }
        }

        private void UpdateTaskList(StackPanel panel, TaskListElement taskList)
        {
            if (panel.Children == null || taskList.Items == null)
                return;

            panel.Children.Clear();
            foreach (var item in taskList.Items)
            {
                var itemPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(item.Level * 20, 0, 0, 0),
                    Spacing = 5
                };

                var checkbox = new CheckBox
                {
                    IsChecked = item.IsChecked,
                    IsEnabled = false, // 设置为只读
                    VerticalAlignment = VerticalAlignment.Top
                };

                var content = new TextBlock
                {
                    Text = item.Text ?? string.Empty,
                    TextWrapping = TextWrapping.Wrap,
                    VerticalAlignment = VerticalAlignment.Top
                };

                if (itemPanel.Children != null)
                {
                    itemPanel.Children.Add(checkbox);
                    itemPanel.Children.Add(content);
                }

                panel.Children.Add(itemPanel);
            }
        }

        private void UpdateQuote(Border border, QuoteElement quote)
        {
            if (border.Child is TextBlock textBlock)
            {
                textBlock.Text = quote.Text;
            }
        }

        private void UpdateImage(Image img, ImageElement image)
        {
            LoadImageAsync(img, image.Source);
        }

        private void UpdateLink(Button btn, LinkElement link)
        {
            if (btn.Content is TextBlock textBlock && textBlock.Inlines != null)
            {
                textBlock.Inlines.Clear();
                var run = new Run
                {
                    Text = link.Text ?? string.Empty,
                    TextDecorations = TextDecorations.Underline,
                    Foreground = new SolidColorBrush(Color.FromRgb(0, 122, 255))
                };
                textBlock.Inlines.Add(run);
            }
        }

        private async void LoadImageAsync(Image img, string source)
        {
            if (string.IsNullOrEmpty(source) || img == null)
                return;

            try
            {
                var imageData = await _imageCache.GetImageAsync(source);
                if (imageData != null)
                {
                    using var stream = new MemoryStream(imageData);
                    var bitmap = new Bitmap(stream);

                    // 在 UI 线程上设置图片源
                    Dispatcher.UIThread.Post(() =>
                    {
                        img.Source = bitmap;
                    });
                }
                else
                {
                    _logger.LogWarning("Failed to load image from {Source}", source);
                    img.Source = CreateErrorPlaceholder("Failed to load image");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading image from {Source}", source);
                img.Source = CreateErrorPlaceholder($"Error: {ex.Message}");
            }
        }

        private IImage CreateErrorPlaceholder(string message)
        {
            // 创建一个简单的错误占位图
            var drawingGroup = new DrawingGroup();
            using (var context = drawingGroup.Open())
            {
                context.DrawRectangle(
                    Brushes.LightGray,
                    new Pen(Brushes.Gray, 1),
                    new Rect(0, 0, 100, 100)
                );

                var text = new FormattedText(
                    message,
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface(FontFamily.Default),
                    12,
                    Brushes.Gray
                );

                context.DrawText(text, new Point(5, 40));
            }

            return new DrawingImage(drawingGroup);
        }

        private void OnLinkClicked(string url)
        {
            LinkClicked?.Invoke(this, url);
        }

        private Control RenderTable(TableElement table)
        {
            var grid = new Grid { Margin = new Thickness(0, 0, 0, 10) };

            // 添加列定义
            for (int i = 0; i < table.Headers.Count; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            }

            // 添加行定义
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 表头行
            foreach (var row in table.Rows)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            // 渲染表头
            for (int col = 0; col < table.Headers.Count; col++)
            {
                var headerCell = new Border
                {
                    Child = RenderTableCell(table.Headers[col]),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(229, 229, 229)),
                    BorderThickness = new Thickness(1),
                    Background = new SolidColorBrush(Color.FromRgb(247, 247, 247))
                };
                Grid.SetRow(headerCell, 0);
                Grid.SetColumn(headerCell, col);
                grid.Children.Add(headerCell);
            }

            // 渲染数据行
            for (int row = 0; row < table.Rows.Count; row++)
            {
                var rowData = table.Rows[row];
                for (int col = 0; col < Math.Min(rowData.Count, table.Headers.Count); col++)
                {
                    var cell = new Border
                    {
                        Child = RenderTableCell(rowData[col]),
                        BorderBrush = new SolidColorBrush(Color.FromRgb(229, 229, 229)),
                        BorderThickness = new Thickness(1)
                    };
                    Grid.SetRow(cell, row + 1);
                    Grid.SetColumn(cell, col);
                    grid.Children.Add(cell);
                }
            }

            return grid;
        }

        private Control RenderTableCell(string content)
        {
            var textBlock = new TextBlock
            {
                Padding = new Thickness(5),
                TextWrapping = TextWrapping.Wrap
            };

            // 处理代码内联
            if (content.StartsWith("`") && content.EndsWith("`"))
            {
                var code = content.Trim('`');
                textBlock.FontFamily = new FontFamily("Consolas, Menlo, Monaco, monospace");
                textBlock.Background = new SolidColorBrush(Color.FromRgb(245, 245, 245));
                textBlock.Text = code;
            }
            // 处理链接
            else if (content.Contains("[") && content.Contains("]("))
            {
                var linkStart = content.IndexOf("[");
                var linkTextEnd = content.IndexOf("]", linkStart);
                var urlStart = content.IndexOf("(", linkTextEnd);
                var urlEnd = content.IndexOf(")", urlStart);

                if (linkStart >= 0 && linkTextEnd >= 0 && urlStart >= 0 && urlEnd >= 0)
                {
                    var linkText = content.Substring(linkStart + 1, linkTextEnd - linkStart - 1);
                    var url = content.Substring(urlStart + 1, urlEnd - urlStart - 1);

                    var button = new Button
                    {
                        Content = new TextBlock
                        {
                            Text = linkText,
                            TextDecorations = TextDecorations.Underline,
                            Foreground = new SolidColorBrush(Color.FromRgb(0, 122, 255))
                        },
                        Background = Brushes.Transparent,
                        BorderThickness = new Thickness(0),
                        Padding = new Thickness(0),
                        Cursor = new Cursor(StandardCursorType.Hand)
                    };

                    button.Click += (s, e) => OnLinkClicked(url);
                    return button;
                }
            }
            else
            {
                textBlock.Text = content;
            }

            return textBlock;
        }

        private void UpdateTable(Grid grid, TableElement table)
        {
            if (grid.Children == null)
                return;

            grid.Children.Clear();
            grid.RowDefinitions.Clear();
            grid.ColumnDefinitions.Clear();

            // Add column definitions
            foreach (var _ in table.Headers)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            }

            // Add header row
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            for (int i = 0; i < table.Headers.Count; i++)
            {
                var header = table.Headers[i];
                if (header == null)
                    continue;

                var headerCell = new TextBlock
                {
                    Text = header ?? string.Empty,
                    FontWeight = FontWeight.Bold,
                    Padding = new Thickness(5),
                    Background = new SolidColorBrush(Color.FromRgb(245, 245, 245))
                };
                Grid.SetRow(headerCell, 0);
                Grid.SetColumn(headerCell, i);
                grid.Children.Add(headerCell);
            }

            // Add data rows
            for (int rowIndex = 0; rowIndex < table.Rows.Count; rowIndex++)
            {
                var row = table.Rows[rowIndex];
                if (row == null)
                    continue;

                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                for (int colIndex = 0; colIndex < row.Count; colIndex++)
                {
                    var cell = row[colIndex];
                    var textBlock = new TextBlock
                    {
                        Text = cell ?? string.Empty,
                        Padding = new Thickness(5)
                    };
                    Grid.SetRow(textBlock, rowIndex + 1);
                    Grid.SetColumn(textBlock, colIndex);
                    grid.Children.Add(textBlock);
                }
            }
        }

        private Control RenderEmphasis(EmphasisElement emphasis)
        {
            var textBlock = new TextBlock();
            if (textBlock.Inlines != null)
            {
                if (emphasis.IsStrong)
                {
                    var bold = new Bold();
                    bold.Inlines?.Add(new Run { Text = emphasis.Text ?? string.Empty });
                    textBlock.Inlines.Add(bold);
                }
                else
                {
                    var italic = new Italic();
                    italic.Inlines?.Add(new Run { Text = emphasis.Text ?? string.Empty });
                    textBlock.Inlines.Add(italic);
                }
            }
            return textBlock;
        }

        private void UpdateEmphasis(TextBlock textBlock, EmphasisElement emphasis)
        {
            if (textBlock.Inlines == null)
                return;

            textBlock.Inlines.Clear();
            if (emphasis.IsStrong)
            {
                var bold = new Bold();
                bold.Inlines?.Add(new Run { Text = emphasis.Text ?? string.Empty });
                textBlock.Inlines.Add(bold);
            }
            else
            {
                var italic = new Italic();
                italic.Inlines?.Add(new Run { Text = emphasis.Text ?? string.Empty });
                textBlock.Inlines.Add(italic);
            }
        }

        private Control RenderHorizontalRule()
        {
            return new Border
            {
                Height = 1,
                Background = new SolidColorBrush(Color.FromRgb(229, 229, 229)),
                Margin = new Thickness(0, 10, 0, 10)
            };
        }
    }
}
