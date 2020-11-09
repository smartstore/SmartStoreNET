using SmartStore.Collections;
using SmartStore.Core.Themes;

namespace SmartStore.Admin.Models.Themes
{

    public class ThemeManifestModel : ITopologicSortable<string>
    {
        public string Name { get; set; }

        public string BaseTheme { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Author { get; set; }

        public string Url { get; set; }

        public string Version { get; set; }

        public string PreviewImageUrl { get; set; }

        public bool IsConfigurable { get; set; }

        public bool IsActive { get; set; }

        public ThemeManifestState State { get; set; }

        string ITopologicSortable<string>.Key => this.Name;

        string[] ITopologicSortable<string>.DependsOn
        {
            get
            {
                if (this.BaseTheme.IsEmpty())
                    return null;

                return new string[] { this.BaseTheme };
            }
        }
    }

}