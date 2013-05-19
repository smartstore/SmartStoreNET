using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartStore.Admin.Models.Themes
{
    
    public class ThemeManifestModel
    {
        public string Name { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Author { get; set; }

        public string Version { get; set; }

        public string PreviewImageUrl { get; set; }

        public bool SupportsRtl { get; set; }

        public bool IsMobileTheme { get; set; }

        public bool IsConfigurable { get; set; }

        public bool IsActive { get; set; }
    }

}