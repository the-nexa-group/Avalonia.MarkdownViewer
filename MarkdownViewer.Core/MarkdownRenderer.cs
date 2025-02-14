using MarkdownViewer.Core.Controls;
using MarkdownViewer.Core.Elements;

namespace MarkdownViewer.Core
{
    public interface IMarkdownRenderer
    {
        IControl RenderElement(MarkdownElement element);
    }
}
