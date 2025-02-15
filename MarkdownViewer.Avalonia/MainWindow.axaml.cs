using Avalonia.Controls;
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

    public MainWindow()
    {
        InitializeComponent();

        httpClient = new HttpClient();
        var logger = NullLogger<MemoryImageCache>.Instance;
        imageCache = new MemoryImageCache(httpClient, logger);
        var renderer = new AvaloniaMarkdownRenderer(imageCache, NullLogger<AvaloniaMarkdownRenderer>.Instance);
        MarkdownViewer.Renderer = renderer;
        LoadSampleMarkdown();
    }

    private void LoadSampleMarkdown()
    {
        const string sampleMarkdown =
            @"# Markdown Viewer Sample

## Basic Formatting

This is a **bold** text example, this is an *italic* text.

### List Example

- Item 1
- Item 2
  - Subitem 2.1
  - Subitem 2.2
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

Pic: ![Avalonia Logo](https://avatars.githubusercontent.com/u/14075148?s=200&v=4)

### Tables

| Header1 | Header2 | Header3 |
|---------|---------|---------|
| Normal text | **Bold text** | `Code text` |
| [Link](http://example.com) | *Italic text* | Other content |

---

# Markdown 查看器示例

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
