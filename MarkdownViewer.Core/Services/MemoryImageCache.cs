using System.Collections.Concurrent;
using System.Net.Http;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace MarkdownViewer.Core.Services
{
    public class MemoryImageCache : IImageCache
    {
        private readonly ConcurrentDictionary<string, byte[]> _cache = new();
        private readonly HttpClient _httpClient;
        private readonly ILogger<MemoryImageCache> _logger;
        private readonly long _maxCacheSizeInBytes;
        private long _currentCacheSize;

        public MemoryImageCache(
            HttpClient httpClient,
            ILogger<MemoryImageCache> logger,
            long maxCacheSizeInBytes = 100 * 1024 * 1024
        ) // 默认100MB
        {
            _httpClient = httpClient;
            _logger = logger;
            _maxCacheSizeInBytes = maxCacheSizeInBytes;
        }

        public async Task<byte[]> GetImageAsync(
            string url,
            CancellationToken cancellationToken = default
        )
        {
            if (_cache.TryGetValue(url, out var cachedData))
            {
                _logger.LogDebug("Cache hit for {Url}", url);
                return cachedData;
            }

            _logger.LogDebug("Cache miss for {Url}, downloading...", url);
            try
            {
                var imageData = await DownloadAndCompressImageAsync(url, cancellationToken);
                await CacheImageAsync(url, imageData, cancellationToken);
                return imageData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download image from {Url}", url);
                throw new ImageLoadException($"Failed to load image from {url}", ex);
            }
        }

        private async Task<byte[]> DownloadAndCompressImageAsync(
            string url,
            CancellationToken cancellationToken
        )
        {
            var imageData = await _httpClient.GetByteArrayAsync(url, cancellationToken);
            return await ImageCompressor.CompressImageAsync(imageData);
        }

        public Task CacheImageAsync(
            string url,
            byte[] imageData,
            CancellationToken cancellationToken = default
        )
        {
            if (imageData == null)
            {
                _logger.LogWarning("Attempted to cache null image data for {Url}", url);
                return Task.CompletedTask;
            }

            var imageSize = imageData.Length;
            if (_currentCacheSize + imageSize > _maxCacheSizeInBytes)
            {
                _logger.LogInformation("Cache size limit reached, clearing cache");
                _cache.Clear();
                _currentCacheSize = 0;
            }

            if (_cache.TryAdd(url, imageData))
            {
                Interlocked.Add(ref _currentCacheSize, imageSize);
                _logger.LogDebug("Cached image {Url} ({Size} bytes)", url, imageSize);
            }

            return Task.CompletedTask;
        }

        public void ClearCache()
        {
            _cache.Clear();
            _logger.LogInformation("Image cache cleared");
        }
    }

    public class ImageCacheEventArgs : EventArgs
    {
        public string Url { get; }
        public Exception Exception { get; }
        public int? RetryAttempt { get; }

        public ImageCacheEventArgs(string url, Exception exception, int? retryAttempt = null)
        {
            Url = url;
            Exception = exception;
            RetryAttempt = retryAttempt;
        }
    }

    public class ImageLoadException : Exception
    {
        public ImageLoadException(string message, Exception innerException)
            : base(message, innerException) { }
    }

    public class ImageCacheException : Exception
    {
        public ImageCacheException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
