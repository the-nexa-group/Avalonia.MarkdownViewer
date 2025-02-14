using System.Threading;
using System.Threading.Tasks;

namespace MarkdownViewer.Core.Services
{
    public interface IImageCache
    {
        Task<byte[]> GetImageAsync(string url, CancellationToken cancellationToken = default);
        Task CacheImageAsync(
            string url,
            byte[] imageData,
            CancellationToken cancellationToken = default
        );
    }
}
