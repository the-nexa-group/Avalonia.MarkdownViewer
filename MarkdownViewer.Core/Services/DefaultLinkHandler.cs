using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
#if __ANDROID__
using Android.Content;
using Android.App;
#elif __IOS__
using Foundation;
using UIKit;
#endif

namespace MarkdownViewer.Core.Services
{
    public class DefaultLinkHandler
    {
        private static ILogger? _logger;
#if __ANDROID__
        private static Context? _context;
#endif

        public static void Initialize(ILogger logger
#if __ANDROID__
            , Context context
#endif
        )
        {
            _logger = logger;
#if __ANDROID__
            _context = context;
#endif
        }

        public static void HandleLink(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                _logger?.LogWarning("Attempted to open empty or null URL");
                return;
            }

            try
            {
                _logger?.LogInformation("Opening URL: {Url}", url);

#if __ANDROID__
                if (_context == null)
                {
                    throw new InvalidOperationException("Android Context not initialized");
                }
                var uri = Android.Net.Uri.Parse(url);
                var intent = new Intent(Intent.ActionView, uri);
                intent.AddFlags(ActivityFlags.NewTask);
                _context.StartActivity(intent);
#elif __IOS__
                var nsUrl = new NSUrl(url);
                if (UIApplication.SharedApplication.CanOpenUrl(nsUrl))
                {
                    UIApplication.SharedApplication.OpenUrl(nsUrl);
                }
                else
                {
                    _logger?.LogWarning("iOS cannot open URL: {Url}", url);
                }
#else
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
                {
                    Process.Start("xdg-open", url);
                }
                else
                {
                    // For other platforms, try using generic method
                    var psi = new ProcessStartInfo { FileName = url, UseShellExecute = true };
                    Process.Start(psi);
                }
#endif
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "Failed to open URL: {Url} on platform: {Platform}",
                    url,
                    RuntimeInformation.OSDescription
                );
                // Error notification UI code can be added here
            }
        }
    }
}
