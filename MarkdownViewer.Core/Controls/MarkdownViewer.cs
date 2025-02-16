using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using MarkdownViewer.Core.Implementations;

namespace MarkdownViewer.Core.Controls
{
    public class MarkdownViewer : ContentControl
    {
        private IMarkdownRenderer? _renderer;
        private string markdownText = string.Empty;

        public static readonly DirectProperty<MarkdownViewer, string> MarkdownTextProperty =
            AvaloniaProperty.RegisterDirect<MarkdownViewer, string>(
                nameof(MarkdownText),
                getter: obj => obj.MarkdownText,
                setter: (obj, value) => obj.MarkdownText = value,
                defaultBindingMode: BindingMode.TwoWay
            );

        public static readonly StyledProperty<IMarkdownRenderer> RendererProperty =
            AvaloniaProperty.Register<MarkdownViewer, IMarkdownRenderer>(
                nameof(Renderer),
                coerce: (obj, value) =>
                {
                    if (obj is MarkdownViewer viewer)
                    {
                        viewer._renderer = value;
                        viewer.RenderContent();
                    }
                    return value;
                }
            );

        public string MarkdownText
        {
            get => markdownText;
            set => SetAndRaise(MarkdownTextProperty, ref markdownText, value);
        }

        public IMarkdownRenderer Renderer
        {
            get => GetValue(RendererProperty);
            set => SetValue(RendererProperty, value);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == MarkdownTextProperty)
            {
                RenderContent();
            }
        }

        public MarkdownViewer()
        {
            // Default constructor for XAML usage
        }

        private void RenderContent()
        {
            if (_renderer != null && !string.IsNullOrEmpty(MarkdownText))
            {
                Content = _renderer.RenderDocument(MarkdownText);
            }
        }
    }
}
