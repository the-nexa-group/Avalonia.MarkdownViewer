using Avalonia;
using Avalonia.Media.Imaging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MarkdownViewer.Core.Services
{
    public class ImageCompressor
    {
        public static async Task<byte[]> CompressImageAsync(
            byte[] imageData,
            int maxWidth = 1920,
            int maxHeight = 1080,
            CancellationToken cancellationToken = default
        )
        {
            if (imageData == null || imageData.Length == 0)
            {
                throw new ArgumentException(
                    "Image data cannot be null or empty",
                    nameof(imageData)
                );
            }

            if (maxWidth <= 0 || maxHeight <= 0)
            {
                throw new ArgumentException(
                    "Width and height must be positive numbers",
                    $"{nameof(maxWidth)}: {maxWidth}, {nameof(maxHeight)}: {maxHeight}"
                );
            }

            try
            {
                return await Task.Run(
                    () =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        using var inputStream = new MemoryStream(imageData);
                        using var bitmap = new Bitmap(inputStream);

                        var (width, height) = CalculateNewSize(
                            bitmap.PixelSize.Width,
                            bitmap.PixelSize.Height,
                            maxWidth,
                            maxHeight
                        );

                        if (width == bitmap.PixelSize.Width && height == bitmap.PixelSize.Height)
                        {
                            return imageData;
                        }

                        cancellationToken.ThrowIfCancellationRequested();

                        using var resized = bitmap.CreateScaledBitmap(new PixelSize(width, height));
                        using var outputStream = new MemoryStream();
                        resized.Save(outputStream);
                        return outputStream.ToArray();
                    },
                    cancellationToken
                );
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ImageProcessingException(
                    $"Failed to compress image. Original size: {imageData.Length} bytes",
                    ex
                );
            }
        }

        private static (int width, int height) CalculateNewSize(
            int originalWidth,
            int originalHeight,
            int maxWidth,
            int maxHeight
        )
        {
            if (originalWidth <= 0 || originalHeight <= 0)
            {
                throw new ArgumentException(
                    "Original dimensions must be positive numbers",
                    $"{nameof(originalWidth)}: {originalWidth}, {nameof(originalHeight)}: {originalHeight}"
                );
            }

            var ratioX = (double)maxWidth / originalWidth;
            var ratioY = (double)maxHeight / originalHeight;
            var ratio = Math.Min(ratioX, ratioY);

            if (ratio >= 1)
                return (originalWidth, originalHeight);

            return ((int)(originalWidth * ratio), (int)(originalHeight * ratio));
        }
    }

    public class ImageProcessingException : Exception
    {
        public ImageProcessingException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
