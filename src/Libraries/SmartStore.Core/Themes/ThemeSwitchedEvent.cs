using System;

namespace SmartStore.Core.Themes
{
    public class ThemeSwitchedEvent
    {
        public string OldTheme { get; set; }
        public string NewTheme { get; set; }
    }
}
