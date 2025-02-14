using Avalonia.Controls;
using MarkdownViewer.Core.Implementations;
using MarkdownViewer.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net.Http;

namespace MarkdownViewer.Avalonia;

public partial class MainWindow : Window
{
    private readonly HttpClient _httpClient;

    public MainWindow()
    {
        InitializeComponent();
        _httpClient = new HttpClient();
        InitializeMarkdownViewer();
        LoadSampleMarkdown();
    }

    private void InitializeMarkdownViewer()
    {
        var parser = new MarkdigParser();
        var imageCacheLogger = NullLogger<MemoryImageCache>.Instance;
        var imageCache = new MemoryImageCache(_httpClient, imageCacheLogger, 100 * 1024 * 1024); // 100MB cache
        var rendererLogger = NullLogger<AvaloniaMarkdownRenderer>.Instance;
        var renderer = new AvaloniaMarkdownRenderer(imageCache, rendererLogger);
        MarkdownViewer.Initialize(parser, renderer);
    }

    private void LoadSampleMarkdown()
    {
        const string sampleMarkdown =
            @"# Markdown 查看器示例

## 基本格式

这是一个**粗体**文本示例，这是*斜体*文本。

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
        Console.WriteLine(""Hello, World!"");
    }
}
```

### 链接和图片

[Avalonia UI](https://avaloniaui.net/)

![Avalonia Logo](https://avatars.githubusercontent.com/u/14075148?s=200&v=4)

### 表格

| 标题1 | 标题2 | 标题3 |
|-------|-------|-------|
| 普通文本 | **粗体文本** | `代码文本` |
| [链接](http://example.com) | *斜体文本* | 其他内容 |
";

        MarkdownViewer.MarkdownText = sampleMarkdown;
    }
}
