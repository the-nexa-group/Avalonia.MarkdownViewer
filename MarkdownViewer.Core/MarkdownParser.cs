using System.IO;

namespace MarkdownViewer.Core
{
    public interface IMarkdownParser
    {
        IAsyncEnumerable<Elements.MarkdownElement> ParseStreamAsync(
            Stream stream,
            CancellationToken cancellationToken = default
        );
        IAsyncEnumerable<Elements.MarkdownElement> ParseTextAsync(
            string text,
            CancellationToken cancellationToken = default
        );
    }
}
