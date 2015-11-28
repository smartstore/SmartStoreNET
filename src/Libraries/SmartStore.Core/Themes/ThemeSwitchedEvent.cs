using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Core.Themes
{
    public class ThemeSwitchedEvent
    {
        public string OldTheme { get; set; }
        public string NewTheme { get; set; }
        public bool IsMobile { get; set; }
    }
}
