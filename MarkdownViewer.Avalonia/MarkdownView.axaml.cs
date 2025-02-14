using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarkdownViewer.Core;
using MarkdownViewer.Core.Controls;
using MarkdownViewer.Core.Elements;
using MarkdownViewer.Core.Implementations;

namespace MarkdownViewer.Avalonia
{
    public partial class MarkdownView : UserControl
    {
        private IMarkdownParser? _parser;
        private IMarkdownRenderer? _renderer;
        private readonly StackPanel _contentPanel;
        private readonly Dictionary<MarkdownElement, IControl> _elementControls;

        public static readonly DirectProperty<MarkdownView, string> MarkdownTextProperty =
            AvaloniaProperty.RegisterDirect<MarkdownView, string>(
                nameof(MarkdownText),
                o => o.MarkdownText,
                (o, v) => o.MarkdownText = v
            );

        private string _markdownText = string.Empty;
        public string MarkdownText
        {
            get => _markdownText;
            set
            {
                if (SetAndRaise(MarkdownTextProperty, ref _markdownText, value))
                {
                    UpdateMarkdownContentAsync().ConfigureAwait(false);
                }
            }
        }

        public event EventHandler<string>? LinkClicked;

        public MarkdownView()
        {
            InitializeComponent();

            _contentPanel = new StackPanel();
            _elementControls = new Dictionary<MarkdownElement, IControl>();

            Content = new ScrollViewer
            {
                Content = _contentPanel,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
            };
        }

        public void Initialize(IMarkdownParser parser, IMarkdownRenderer renderer)
        {
            _parser = parser;
            _renderer = renderer;

            if (_renderer is AvaloniaMarkdownRenderer avaloniaRenderer)
            {
                avaloniaRenderer.LinkClicked += (s, url) => LinkClicked?.Invoke(this, url);
            }
        }

        private async Task UpdateMarkdownContentAsync()
        {
            if (_parser == null || _renderer == null)
            {
                throw new InvalidOperationException(
                    "MarkdownView must be initialized with Initialize method before use."
                );
            }

            if (string.IsNullOrEmpty(_markdownText))
            {
                await Dispatcher.UIThread.InvokeAsync(() => _contentPanel.Children.Clear());
                return;
            }

            await foreach (var element in _parser.ParseTextAsync(_markdownText))
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    var control = _renderer.RenderElement(element);
                    _elementControls[element] = control;
                    _contentPanel.Children.Add(((ControlWrapper)control).CreateControl());
                });
            }
        }

        public async Task UpdateElementAsync(MarkdownElement element)
        {
            if (_renderer == null)
            {
                throw new InvalidOperationException(
                    "MarkdownView must be initialized with Initialize method before use."
                );
            }

            if (_elementControls.TryGetValue(element, out var control))
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    var wrapper = (ControlWrapper)control;
                    var avaloniaControl = wrapper.CreateControl();
                    _renderer.RenderElement(element);
                });
            }
        }
    }
}
