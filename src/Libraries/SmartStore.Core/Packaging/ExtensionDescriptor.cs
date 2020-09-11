using System;

namespace SmartStore.Core.Packaging
{

    public class ExtensionDescriptor
    {
        /// <summary>
        /// Virtual path base, "~/Themes" or "~/Plugins"
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Folder name under virtual path base
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The extension type: Plugin | Theme
        /// </summary>
        public string ExtensionType { get; set; }

        // extension metadata
        public string Name { get; set; }
        public string Path { get; set; }
        public string Description { get; set; }
        public Version Version { get; set; }
        public Version MinAppVersion { get; set; }
        public string Author { get; set; }
        public string WebSite { get; set; }
        public string Tags { get; set; }
    }

}
