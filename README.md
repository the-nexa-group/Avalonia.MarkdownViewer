# Avalonia Markdown Viewer

A modern, cross-platform Markdown viewer built with Avalonia UI framework, providing smooth rendering and interaction experience for Markdown documents.

## ✨ Features

- 🎯 Built with Avalonia UI 11 for cross-platform support
- 📝 High-quality Markdown rendering powered by Markdig engine
- 🖼️ Image preloading and caching mechanism
- 🔗 Built-in link handler
- 🎨 Modern Fluent Design interface
- 🚀 High-performance rendering implementation
- ⚡ Memory-optimized image compression

## 🚥 Development Status

Current Version: Pre-release
Status: Active Development

### Roadmap

- [X] Basic Markdown rendering
- [X] Image handling and caching
- [X] Link handling
- [ ] Code syntax highlighting
- [ ] Dark/Light theme support
- [ ] Custom styling options

## 📝 Markdown Support

Currently supports the following Markdown features:

### Basic Syntax

- ✅ Headers (H1-H6)
- ✅ Emphasis (bold, italic)
- ✅ Lists (ordered and unordered)
- ✅ Links
- ✅ Images
- ✅ Blockquotes
- ✅ Code blocks
- ✅ Horizontal rules

### Extended Syntax

- ✅ Tables
- ✅ Task lists
- ✅ Strikethrough
- ✅ Fenced code blocks
- ⚠️ Math equations (partial support)
- ⚠️ Footnotes (partial support)
- 🚧 Diagrams (planned)
- 🚧 Custom containers (planned)

## 🛠️ Technology Stack

- .NET 9.0
- Avalonia UI 11
- Markdig 0.40.0
- Microsoft.Extensions.Logging

## 📦 Project Structure

The project consists of two main parts:

- **MarkdownViewer.Core**: Core library containing Markdown parsing, rendering, and various service implementations
- **MarkdownViewer.Avalonia**: Avalonia UI application providing the user interface and interaction features

## 🚀 Getting Started

### System Requirements

- .NET 9.0 SDK or higher
- Supported OS: Windows, Linux, macOS

### Build Steps

1. Clone the repository:

```bash
git clone [repository-url]
```

2. Navigate to the project directory:

```bash
cd Avalonia.Markdown
```

3. Build the project:

```bash
dotnet build
```

4. Run the application:

```bash
dotnet run --project MarkdownViewer.Avalonia
```

## 🔧 Core Features

- Real-time Markdown rendering
- Automatic image preloading and caching
- Link click handling
- Memory-optimized image compression
- Modern user interface

## 🤝 Contributing

Contributions are welcome! Feel free to submit issues and pull requests.

## 📢 Known Issues

- Large images may take longer to load on first render
- Some complex math equations might not render correctly
- Custom emoji shortcodes are not currently supported

## Dependencies

This project uses the following open source components:

- [Markdig](https://github.com/xoofx/markdig) - A fast, powerful, CommonMark compliant, extensible Markdown processor for .NET (BSD-2-Clause License)
- [Avalonia](https://github.com/AvaloniaUI/Avalonia) - A cross-platform .NET UI framework (MIT License)


## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

For third-party license notices, please see [THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt).

## 📝 更新日志

### v1.0.1 (2024-03)

#### 新功能
- ✨ 增强了 Markdown 渲染器的格式化功能
- ✨ 增强了引用元素以支持内联元素
- 🌐 添加了多语言支持（代码块复制按钮）

#### 改进
- 🔧 改进了 Markdown 渲染和解析功能
- 📝 改进了代码块渲染效果
- 🔗 改进了链接处理机制
