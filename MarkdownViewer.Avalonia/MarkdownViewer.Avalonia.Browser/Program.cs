using Avalonia;
using Avalonia.Browser;
using Avalonia.Media;
using MarkdownViewer.Avalonia;
using System.Runtime.Versioning;
using System.Threading.Tasks;

internal sealed partial class Program
{
    private static Task Main(string[] args) =>
        BuildAvaloniaApp().WithInterFont().StartBrowserAppAsync("out");

    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
            .With(new FontManagerOptions
            {
                FontFallbacks = [
                    new FontFallback { FontFamily = new FontFamily("avares://MarkdownViewer.Avalonia/Assets/AppTextFont.ttf#Avalonia Markdown Viewer Font E") },
                    new FontFallback { FontFamily = new FontFamily("avares://MarkdownViewer.Avalonia/Assets/AppEmojiFont.ttf#Avalonia Markdown Viewer Emoji Font") }
                ]
            });
}
