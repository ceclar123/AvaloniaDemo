using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace AvaloniaDemo.Views
{
    public class HomeView : UserControl
    {
        public HomeView()
        {
            var panel = new StackPanel();
            var tb = new TextBlock
            {
                Text = "这是 HomeView (code)",
                FontSize = 16,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(12)
            };
            panel.Children.Add(tb);
            Content = panel;
        }
    }
}
