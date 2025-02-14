public interface IImagePreloader
{
    Task PreloadImagesAsync(
        IEnumerable<string> urls,
        CancellationToken cancellationToken = default
    );
    void CancelPreloading();
}
