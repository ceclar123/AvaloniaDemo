using System;
using Microsoft.Extensions.DependencyInjection;
using AvaloniaDemo.Views;
using AvaloniaDemo.ExtControl.DataDisplay;

namespace AvaloniaDemo
{
    public static class ServiceRegistry
    {
        public static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Register views and controls
            // Label
            services.AddTransient<TextDisplayLabelUserControl>();
            // TextBlock
            services.AddTransient<TextDisplayTextBlockUserControl>();
            // SelectableTextBlock
            services.AddTransient<TextDisplaySelectableTextBlockUserControl>();

            services.AddTransient<HomeView>();

            // Register MainWindow as singleton so we can get the same instance from DI
            services.AddSingleton<MainWindow>();

            return services.BuildServiceProvider();
        }
    }
}
