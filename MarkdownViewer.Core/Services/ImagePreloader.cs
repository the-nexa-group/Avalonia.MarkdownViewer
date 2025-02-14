using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MarkdownViewer.Core.Services
{
    public class ImagePreloader : IImagePreloader
    {
        private readonly IImageCache _imageCache;
        private readonly ILogger _logger;
        private readonly SemaphoreSlim _semaphore;
        private CancellationTokenSource _cts = new();

        public ImagePreloader(
            IImageCache imageCache,
            ILogger logger,
            int maxConcurrentDownloads = 3
        )
        {
            _imageCache = imageCache;
            _logger = logger;
            _semaphore = new SemaphoreSlim(maxConcurrentDownloads);
        }

        public async Task PreloadImagesAsync(
            IEnumerable<string> urls,
            CancellationToken cancellationToken = default
        )
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var tasks = urls.Select(url => PreloadImageAsync(url, _cts.Token));
            await Task.WhenAll(tasks);
        }

        private async Task PreloadImageAsync(string url, CancellationToken cancellationToken)
        {
            try
            {
                await _semaphore.WaitAsync(cancellationToken);
                await _imageCache.GetImageAsync(url, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to preload image: {url}");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void CancelPreloading()
        {
            _cts?.Cancel();
        }
    }
}
