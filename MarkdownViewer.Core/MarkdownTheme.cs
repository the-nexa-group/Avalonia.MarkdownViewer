using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Avalonia.Media;
using System;

namespace MarkdownViewer.Core
{
    /// <summary>
    /// Markdown 主题资源管理器
    /// </summary>
    public static class MarkdownTheme
    {
        private static bool _isInitialized = false;
        private static readonly object _lock = new object();
        private static ThemeVariant? _lastThemeVariant;

        /// <summary>
        /// 主题变化事件
        /// </summary>
        public static event EventHandler? ThemeChanged;

        /// <summary>
        /// 初始化 Markdown 主题资源
        /// </summary>
        public static void Initialize()
        {
            lock (_lock)
            {
                if (_isInitialized)
                    return;

                try
                {
                    // 加载主题资源
                    var themeUri = new Uri(
                        "avares://MarkdownViewer.Core/Themes/MarkdownTheme.axaml"
                    );
                    var theme = AvaloniaXamlLoader.Load(themeUri) as IStyle;

                    if (theme != null && Application.Current?.Styles != null)
                    {
                        // 检查是否已经添加过主题
                        bool themeExists = false;
                        foreach (var style in Application.Current.Styles)
                        {
                            if (style.GetType().Name.Contains("MarkdownTheme"))
                            {
                                themeExists = true;
                                break;
                            }
                        }

                        if (!themeExists)
                        {
                            Application.Current.Styles.Add(theme);
                        }
                    }

                    // 监听主题变化
                    if (Application.Current != null)
                    {
                        _lastThemeVariant = Application.Current.ActualThemeVariant;
                        Application.Current.PropertyChanged += OnApplicationPropertyChanged;
                    }

                    _isInitialized = true;
                }
                catch (Exception ex)
                {
                    // 如果加载主题失败，记录错误但不抛出异常
                    System.Diagnostics.Debug.WriteLine(
                        $"Failed to load Markdown theme: {ex.Message}"
                    );
                }
            }
        }

        private static void OnApplicationPropertyChanged(
            object? sender,
            AvaloniaPropertyChangedEventArgs e
        )
        {
            if (e.Property == Application.ActualThemeVariantProperty)
            {
                var newTheme = e.NewValue as ThemeVariant;
                if (newTheme != _lastThemeVariant)
                {
                    _lastThemeVariant = newTheme;
                    ThemeChanged?.Invoke(null, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// 获取主题画刷资源
        /// </summary>
        /// <param name="resourceKey">资源键</param>
        /// <param name="fallbackColor">回退颜色</param>
        /// <returns>画刷资源</returns>
        public static IBrush GetThemeBrush(string resourceKey, Color fallbackColor)
        {
            // 确保主题已初始化
            Initialize();

            // 尝试从应用程序资源中获取
            if (
                Application.Current?.TryGetResource(
                    resourceKey,
                    Application.Current.ActualThemeVariant,
                    out var resource
                ) == true
                && resource is IBrush brush
            )
            {
                return brush;
            }

            // 回退到默认颜色
            return new SolidColorBrush(fallbackColor);
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        public static void Cleanup()
        {
            if (Application.Current != null)
            {
                Application.Current.PropertyChanged -= OnApplicationPropertyChanged;
            }
        }
    }
}
