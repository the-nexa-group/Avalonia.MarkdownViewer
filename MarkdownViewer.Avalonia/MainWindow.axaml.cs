using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using MarkdownViewer.Core.Implementations;
using MarkdownViewer.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net.Http;

namespace MarkdownViewer.Avalonia;

public partial class MainWindow : Window
{
    private readonly HttpClient httpClient;
    private readonly MemoryImageCache imageCache;
    private readonly ILogger<MainWindow> logger;

    public MainWindow()
    {
        InitializeComponent();

        httpClient = new HttpClient();
        logger = NullLogger<MainWindow>.Instance;
        var imageCacheLogger = NullLogger<MemoryImageCache>.Instance;
        imageCache = new MemoryImageCache(httpClient, imageCacheLogger);
        var renderer = new AvaloniaMarkdownRenderer(
            imageCache,
            NullLogger<AvaloniaMarkdownRenderer>.Instance
        );

        // 初始化链接处理器
        DefaultLinkHandler.Initialize(logger);

        MarkdownViewer.Renderer = renderer;

        // 设置初始示例文本
        LoadSampleMarkdown();

        // 绑定窗口大小变更事件
        PropertyChanged += MainWindow_PropertyChanged;
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
        if (bounds.Width < bounds.Height * 1.2) // 当宽度小于高度的1.2倍时切换为上下布局
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
        else // 切换为左右布局
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
            @"# Markdown 编辑器示例

## 基本格式

这是一个**粗体**文本示例，这是*斜体*文本。


### Roadmap
- [x] Basic Markdown rendering
- [x] Image handling and caching
- [x] Link handling
- [ ] Code syntax highlighting
- [ ] Dark/Light theme support
- [ ] Custom styling options


### 列表示例

- 项目 1
- 项目 2
  - 子项目 2.1
  - 子项目 2.2
- 项目 3

### 代码示例

```csharp
public class Example
{
    public void HelloWorld()
    {
        Console.WriteLine(""你好，世界！"");
    }
}
```

### 链接和图片

链接：[Avalonia UI](https://avaloniaui.net/)

图片：![Avalonia Logo](https://avatars.githubusercontent.com/u/14075148?s=200&v=4)

### 表格

| 标题1 | 标题2 | 标题3 |
|-------|-------|-------|
| 普通文本 | **粗体文本** | `代码文本` |
| [链接](http://example.com) | *斜体文本* | 其他内容 |
";

        MarkdownViewer.MarkdownText = sampleMarkdown;
    }
}
