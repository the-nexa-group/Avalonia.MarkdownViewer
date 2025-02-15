using Avalonia.Controls;
using MarkdownViewer.Core.Controls;
using MarkdownViewer.Core.Elements;

namespace MarkdownViewer.Core
{
    public interface IMarkdownRenderer
    {
        /// <summary>
        /// Renders the complete markdown document
        /// </summary>
        /// <param name="markdown">The markdown text to render</param>
        /// <returns>The rendered control</returns>
        Control RenderDocument(string markdown);
    }
}
