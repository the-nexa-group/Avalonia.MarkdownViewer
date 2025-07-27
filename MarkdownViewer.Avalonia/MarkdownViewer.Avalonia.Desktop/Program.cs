using Avalonia;
using Avalonia.Media;
using System;

namespace MarkdownViewer.Avalonia.Desktop;
internal sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .With(new FontManagerOptions
            {
                FontFallbacks = [
                    new FontFallback { FontFamily = new FontFamily("avares://MarkdownViewer.Avalonia/Assets/AppTextFont.ttf#Avalonia Markdown Viewer Font E") },
                    new FontFallback { FontFamily = new FontFamily("avares://MarkdownViewer.Avalonia/Assets/AppEmojiFont.ttf#Avalonia Markdown Viewer Emoji Font") }
                ]
            });
}
