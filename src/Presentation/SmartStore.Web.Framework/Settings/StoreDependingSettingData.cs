using System;
using System.Collections.Generic;

namespace SmartStore.Web.Framework.Settings
{
    public class StoreDependingSettingData
    {
        public int ActiveStoreScopeConfiguration { get; set; }
        public HashSet<string> OverrideSettingKeys { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public string RootSettingClass { get; set; }
    }
}
