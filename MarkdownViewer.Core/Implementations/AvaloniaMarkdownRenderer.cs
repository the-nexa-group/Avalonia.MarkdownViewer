using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Input;
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
        private readonly ILogger<AvaloniaMarkdownRenderer> _logger;

        public event EventHandler<string>? LinkClicked;

        public AvaloniaMarkdownRenderer(
            IImageCache imageCache,
            ILogger<AvaloniaMarkdownRenderer> logger
        )
        {
            _imageCache = imageCache;
            _logger = logger;
        }

        public IControl RenderElement(MarkdownElement element)
        {
            var control = element switch
            {
                HeadingElement heading => RenderHeading(heading),
                ParagraphElement paragraph => RenderParagraph(paragraph),
                CodeBlockElement codeBlock => RenderCodeBlock(codeBlock),
                ListElement list => RenderList(list),
                QuoteElement quote => RenderQuote(quote),
                ImageElement image => RenderImage(image),
                LinkElement link => RenderLink(link),
                _ => new TextBlock { Text = "Unsupported element" }
            };

            return new ControlWrapper(control);
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
                case QuoteElement quote when control is Border border:
                    UpdateQuote(border, quote);
                    break;
                case ImageElement image when control is Image img:
                    UpdateImage(img, image);
                    break;
                case LinkElement link when control is Button btn:
                    UpdateLink(btn, link);
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
            var textBlock = new TextBlock
            {
                Text = paragraph.Text,
                FontFamily = _defaultFontFamily,
                FontSize = _baseFontSize,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 10)
            };
            return textBlock;
        }

        private Control RenderCodeBlock(CodeBlockElement codeBlock)
        {
            var textBox = new TextBox
            {
                Text = codeBlock.Code,
                FontFamily = new FontFamily("Consolas, Menlo, Monaco, monospace"),
                FontSize = _baseFontSize,
                IsReadOnly = true,
                AcceptsReturn = true,
                Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)),
                Padding = new Thickness(10),
                Margin = new Thickness(0, 0, 0, 10),
                BorderThickness = new Thickness(0)
            };
            return textBox;
        }

        private void UpdateHeading(TextBlock textBlock, HeadingElement heading)
        {
            textBlock.Text = heading.Text;
            textBlock.FontSize = GetHeadingFontSize(heading.Level);
        }

        private void UpdateParagraph(TextBlock textBlock, ParagraphElement paragraph)
        {
            textBlock.Text = paragraph.Text;
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

            foreach (var item in list.Items)
            {
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

                var content = new TextBlock { Text = item.Text, TextWrapping = TextWrapping.Wrap };

                itemPanel.Children.Add(bullet);
                itemPanel.Children.Add(content);
                panel.Children.Add(itemPanel);
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
            var button = new Button
            {
                Content = new TextBlock
                {
                    Text = link.Text,
                    TextDecorations = TextDecorations.Underline,
                    Foreground = new SolidColorBrush(Color.FromRgb(0, 122, 255))
                },
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(0),
                Cursor = new Cursor(StandardCursorType.Hand)
            };

            button.Click += (s, e) => OnLinkClicked(link.Url);
            return button;
        }

        private void UpdateList(StackPanel panel, ListElement list)
        {
            panel.Children.Clear();
            foreach (var item in list.Items)
            {
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

                var content = new TextBlock { Text = item.Text, TextWrapping = TextWrapping.Wrap };

                itemPanel.Children.Add(bullet);
                itemPanel.Children.Add(content);
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
            if (btn.Content is TextBlock textBlock)
            {
                textBlock.Text = link.Text;
            }
        }

        private async void LoadImageAsync(Image img, string source)
        {
            try
            {
                var imageData = await _imageCache.GetImageAsync(source);
                if (imageData != null)
                {
                    using var stream = new MemoryStream(imageData);
                    var bitmap = new Bitmap(stream);
                    img.Source = bitmap;
                }
                else
                {
                    img.Source = CreateErrorPlaceholder("Failed to load image");
                }
            }
            catch (Exception ex)
            {
                img.Source = CreateErrorPlaceholder("Error loading image");
                _logger.LogError(ex, "Failed to load image from {Source}", source);
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
    }
}
