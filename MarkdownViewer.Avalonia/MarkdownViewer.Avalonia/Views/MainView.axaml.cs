using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Styling;
using MarkdownViewer.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net.Http;

namespace MarkdownViewer.Avalonia.Views;
public partial class MainView : UserControl
{
    private readonly HttpClient httpClient;
    private readonly MemoryImageCache imageCache;
    private readonly ILogger<MainView> logger;

    public MainView()
    {
        InitializeComponent();

        logger = NullLogger<MainView>.Instance;
        //httpClient = new HttpClient();
        //var imageCacheLogger = NullLogger<MemoryImageCache>.Instance;
        //imageCache = new MemoryImageCache(httpClient, imageCacheLogger);
        //var renderer = new AvaloniaMarkdownRenderer(
        //    imageCache,
        //    NullLogger<AvaloniaMarkdownRenderer>.Instance
        //);

        // Initialize link handler
        DefaultLinkHandler.Initialize(logger);

        //MarkdownViewer.Renderer = renderer;

        // Set initial sample text
        LoadSampleMarkdown();

        // Bind window size change event
        PropertyChanged += MainWindow_PropertyChanged;

        // Initialize theme state
        InitializeThemeToggle();
    }

    private void InitializeThemeToggle()
    {
        var app = Application.Current;
        if (app != null)
        {
            var currentTheme = app.RequestedThemeVariant;
            var themeToggleButton = this.FindControl<ToggleButton>("ThemeToggleButton");
            var themeIcon = this.FindControl<TextBlock>("ThemeIcon");

            if (themeToggleButton != null && themeIcon != null)
            {
                bool isDarkTheme = currentTheme == ThemeVariant.Dark;
                themeToggleButton.IsChecked = isDarkTheme;
                themeIcon.Text = isDarkTheme ? "☀️" : "🌙";
            }
        }
    }

    private void OnThemeToggleClick(object? sender, RoutedEventArgs e)
    {
        var app = Application.Current;
        if (app != null)
        {
            var themeToggleButton = sender as ToggleButton;
            var themeIcon = this.FindControl<TextBlock>("ThemeIcon");

            if (themeToggleButton != null && themeIcon != null)
            {
                bool isDarkTheme = themeToggleButton.IsChecked == true;

                // Switch theme
                app.RequestedThemeVariant = isDarkTheme ? ThemeVariant.Dark : ThemeVariant.Light;

                // Update icon
                themeIcon.Text = isDarkTheme ? "☀️" : "🌙";

                logger.LogInformation($"Theme switched to: {(isDarkTheme ? "Dark" : "Light")}");
            }
        }
    }

    private void MainWindow_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == BoundsProperty)
        {
            UpdateLayoutOrientation();
        }
    }

    private void UpdateLayoutOrientation()
    {
        var bounds = Bounds;
        if (bounds.Width < bounds.Height * 1.2) // Switch to vertical layout when width is less than 1.2 times height
        {
            MainGrid.ColumnDefinitions.Clear();
            MainGrid.RowDefinitions.Clear();

            MainGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
            MainGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            MainGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));

            Grid.SetRow(MarkdownEditor, 0);
            Grid.SetColumn(MarkdownEditor, 0);

            var splitter = MainGrid.Children[1] as GridSplitter;
            if (splitter != null)
            {
                Grid.SetRow(splitter, 1);
                Grid.SetColumn(splitter, 0);
                splitter.ResizeDirection = GridResizeDirection.Rows;
            }

            var scrollViewer = MainGrid.Children[2] as ScrollViewer;
            if (scrollViewer != null)
            {
                Grid.SetRow(scrollViewer, 2);
                Grid.SetColumn(scrollViewer, 0);
            }
        }
        else // Switch to horizontal layout
        {
            MainGrid.RowDefinitions.Clear();
            MainGrid.ColumnDefinitions.Clear();

            MainGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

            Grid.SetRow(MarkdownEditor, 0);
            Grid.SetColumn(MarkdownEditor, 0);

            var splitter = MainGrid.Children[1] as GridSplitter;
            if (splitter != null)
            {
                Grid.SetRow(splitter, 0);
                Grid.SetColumn(splitter, 1);
                splitter.ResizeDirection = GridResizeDirection.Columns;
            }

            var scrollViewer = MainGrid.Children[2] as ScrollViewer;
            if (scrollViewer != null)
            {
                Grid.SetRow(scrollViewer, 0);
                Grid.SetColumn(scrollViewer, 2);
            }
        }
    }

    private void LoadSampleMarkdown()
    {
        const string sampleMarkdown =
            @"# Markdown Editor Example

## Basic Formatting

This is a **bold** text example, this is *italic* text.


### Roadmap
- [x] Basic Markdown rendering
- [x] Image handling and caching
- [x] Link handling
- [ ] Code syntax highlighting
- [ ] Dark/Light theme support
- [ ] Custom styling options


### List Example

- Item 1
- Item 2
  - Sub-item 2.1
  - Sub-item 2.2
- Item 3

### Code Example

```csharp
public class Example
{
    public void HelloWorld()
    {
        Console.WriteLine(""Hello, World!"");
    }
}
```

### Links and Images

Link: [Avalonia UI](https://avaloniaui.net/)

Image: ![Avalonia Logo](https://avatars.githubusercontent.com/u/14075148?s=200&v=4)

### Table

| Header1 | Header2 | Header3 |
|---------|---------|---------|
| Normal text | **Bold text** | `Code text` |
| [Link](http://example.com) | *Italic text* | Other content |

### Formula

$$
\int_0^1 x^2 dx = \frac{1}{3}
$$

#### Inline Formula

Let $x > 0$, then $f(x) = \frac{1}{x}$

";

        MarkdownViewer.MarkdownText = sampleMarkdown;
    }
}