using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Microsoft.Extensions.DependencyInjection;
using AvaloniaDemo.Views;
using AvaloniaDemo.ExtControl;

namespace AvaloniaDemo;

public partial class MainWindow : Window
{

    // 菜单 key 到视图工厂的字典（支持注入/参数化构造）
    private readonly Dictionary<string, Func<Control>> _viewFactory;
    public MainWindow(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        BuildMenu();

        // 在构造器中初始化工厂字典，以便工厂 lambda 可以捕获 this（用于注入父窗口等）
        _viewFactory = new Dictionary<string, Func<Control>>(StringComparer.OrdinalIgnoreCase)
        {
            // 带注入的工厂：从 DI 解析并注入 ParentWindow（BaseUserControl 提供此属性）
            ["DataDisplay.TextDisplay.Label"] = () =>
            {
                var c = serviceProvider.GetRequiredService<ExtControl.DataDisplay.TextDisplayLabelUserControl>();
                if (c is ExtControl.BaseUserControl b) b.ParentWindow = this;
                return c;
            },
            ["DataDisplay.TextDisplay.TextBlock"] = () =>
            {
                var c = serviceProvider.GetRequiredService<ExtControl.DataDisplay.TextDisplayTextBlockUserControl>();
                if (c is ExtControl.BaseUserControl b) b.ParentWindow = this;
                return c;
            },
            ["DataDisplay.TextDisplay.SelectableTextBlock"] = () =>
            {
                var c = serviceProvider.GetRequiredService<ExtControl.DataDisplay.TextDisplaySelectableTextBlockUserControl>();
                if (c is ExtControl.BaseUserControl b) b.ParentWindow = this;
                return c;
            },



            // 简单工厂：直接创建 HomeView（从 DI 解析）
            ["file.new"] = () => serviceProvider.GetRequiredService<Views.HomeView>(),

            ["view.output"] = () =>
            {
                var c = serviceProvider.GetRequiredService<ExtControl.DataDisplay.TextDisplayLabelUserControl>();
                if (c is ExtControl.BaseUserControl b) b.ParentWindow = this;
                return c;
            },

            ["view.properties"] = () =>
            {
                var c = serviceProvider.GetRequiredService<ExtControl.DataDisplay.TextDisplayLabelUserControl>();
                if (c is ExtControl.BaseUserControl b) b.ParentWindow = this;
                return c;
            },

            // 工厂可以用来传入参数或设置 DataContext
            ["help.about"] = () => serviceProvider.GetRequiredService<Views.HomeView>(),
            ["edit.undo"] = () => serviceProvider.GetRequiredService<Views.HomeView>(),
            ["edit.redo"] = () => serviceProvider.GetRequiredService<Views.HomeView>()
        };
    }


    private Control CreateViewForKey(string key, string header)
    {
        try
        {
            if (_viewFactory != null && _viewFactory.TryGetValue(key, out var factory) && factory != null)
            {
                var inst = factory();
                if (inst != null)
                {
                    return inst;
                }
            }
        }
        catch
        {
            // 工厂抛出异常时回退到反射或占位
        }


        // 默认回退到简单的 TextBlock
        return new TextBlock
        {
            Text = $"这是 {header} 页 (Key={key})",
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            Margin = new Thickness(8)
        };
    }

    private void BuildMenu()
    {
        // 简单的菜单定义，可根据需要改为从配置或服务加载
        var menuDefs = new List<MenuDef>
        {
            new MenuDef("DataDisplay", children: new[] {
                new MenuDef("TextDisplay", children: new[] {
                    new MenuDef("Label", key: "DataDisplay.TextDisplay.Label"),
                    new MenuDef("TextBlock", key: "DataDisplay.TextDisplay.TextBlock"),
                    new MenuDef("SelectableTextBlock", key: "DataDisplay.TextDisplay.SelectableTextBlock"),
                }),
                new MenuDef("Open", key: "file.open"),
                new MenuDef("Exit", key: "file.exit")
            }),
            new MenuDef("Edit", children: new[] {
                new MenuDef("Undo", key: "edit.undo"),
                new MenuDef("Redo", key: "edit.redo"),
            }),
            new MenuDef("View", children: new[] {
                new MenuDef("Tool Windows", children: new[] {
                    new MenuDef("Output", key: "view.output"),
                    new MenuDef("Properties", key: "view.properties")
                })
            }),
            new MenuDef("Help", children: new[] {
                new MenuDef("About", key: "help.about")
            })
        };

        if (this.FindControl<Menu>("MainMenu") is Menu menu)
        {
            if (menu.Items is IList menuItemsList)
            {
                foreach (var m in menuDefs.Select(CreateMenuItem))
                    menuItemsList.Add(m);
            }
        }
    }

    private MenuItem CreateMenuItem(MenuDef def)
    {
        var mi = new MenuItem { Header = def.Header };
        if (def.Children != null && def.Children.Length > 0)
        {
            if (mi.Items is IList subList)
            {
                foreach (var child in def.Children.Select(CreateMenuItem))
                {
                    subList.Add(child);
                }
            }
        }
        else
        {
            // 叶子节点，响应点击打开或选中 Tab
            mi.Tag = def.Key ?? def.Header;
            mi.Click += MenuItem_Click;
        }
        return mi;
    }

    private void MenuItem_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is MenuItem mi && mi.Tag is string key)
        {
            string header = mi.Header?.ToString() ?? key;
            // 特殊处理 Exit
            if (key == "file.exit")
            {
                // 关闭窗口
                this.Close();
                return;
            }

            OpenOrSelectTab(key, header);
        }
    }

    private void OpenOrSelectTab(string key, string header)
    {
        var tabControl = this.FindControl<TabControl>("MainTabControl");
        if (tabControl == null)
            return;

        // 查找已存在的 Tab
        var existing = tabControl.Items?.OfType<TabItem>().FirstOrDefault(t => (t.Tag?.ToString()) == key);
        if (existing != null)
        {
            tabControl.SelectedItem = existing;
            return;
        }

        // 新建一个 Tab，尝试根据 key 动态创建 UserControl（约定命名空间 AvaloninaDemo.Views）
        var content = CreateViewForKey(key, header);

        var tab = new TabItem { Header = header, Tag = key, Content = content };

        // 将新 Tab 加入到 Items
        if (tabControl.Items is IList list)
        {
            list.Add(tab);
            tabControl.SelectedItem = tab;
            return;
        }
    }

    private sealed class MenuDef
    {
        public string Header { get; }
        public string? Key { get; }
        public MenuDef[]? Children { get; }

        public MenuDef(string header, string? key = null, MenuDef[]? children = null)
        {
            Header = header;
            Key = key;
            Children = children;
        }
    }
}