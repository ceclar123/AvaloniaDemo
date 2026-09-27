using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace AvaloniaDemo;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Configure DI and resolve MainWindow from the service provider
            var provider = ServiceRegistry.ConfigureServices();
            var main = provider.GetRequiredService<MainWindow>();
            desktop.MainWindow = main;
            desktop.MainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            // 启动时将窗口宽度设为屏幕宽度的一半并居中
            try
            {
                var primary = desktop.MainWindow.Screens?.Primary;
                if (primary != null)
                {
                    desktop.MainWindow.Width = primary.Bounds.Width / 2.0;
                }
            }
            catch
            {
                // 若获取屏幕信息失败，保持默认尺寸
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}