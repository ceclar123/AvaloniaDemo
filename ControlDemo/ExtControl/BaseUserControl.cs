using System;
using System.Collections.Generic;
using System.Text;
using Avalonia;
using Avalonia.Controls;

namespace AvaloniaDemo.ExtControl
{
    public class BaseUserControl : UserControl
    {
        public Window? ParentWindow { get; set; }
    }
}
