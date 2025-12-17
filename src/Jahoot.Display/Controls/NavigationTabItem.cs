using System;

namespace Jahoot.Display.Controls
{
    public class NavigationTabItem
    {
        public required string Header { get; set; }
        public required Type ViewType { get; set; }
    }
}
