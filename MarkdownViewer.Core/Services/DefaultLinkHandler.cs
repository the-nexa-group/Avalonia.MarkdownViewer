using System.Diagnostics;

namespace MarkdownViewer.Core.Services
{
    public class DefaultLinkHandler
    {
        public static void HandleLink(string url)
        {
            if (
                OperatingSystem.IsWindows()
                || OperatingSystem.IsLinux()
                || OperatingSystem.IsMacOS()
            )
            {
                try
                {
                    Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
                }
                catch (Exception)
                {
                    // 处理链接打开失败的情况
                }
            }
        }
    }
}
