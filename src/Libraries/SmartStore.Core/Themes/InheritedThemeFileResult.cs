namespace SmartStore.Core.Themes
{
    public class InheritedThemeFileResult
    {
        /// <summary>
        /// The unrooted relative path of the file (without <c>~/Themes/ThemeName/</c>)
        /// </summary>
        public string RelativePath { get; set; }

        /// <summary>
        /// The original virtual path
        /// </summary>
        public string OriginalVirtualPath { get; set; }

        /// <summary>
        /// The result virtual path (the path in which the file is actually located)
        /// </summary>
        public string ResultVirtualPath { get; set; }

        /// <summary>
        /// The result physical path (the path in which the file is actually located)
        /// </summary>
        public string ResultPhysicalPath { get; set; }

        /// <summary>
        /// The name of the requesting theme
        /// </summary>
        public string OriginalThemeName { get; set; }

        /// <summary>
        /// The name of the resulting theme where the file is actually located
        /// </summary>
        public string ResultThemeName { get; set; }

        public bool IsBased { get; set; }

        /// <summary>
        /// The query string, e.g. '?base'
        /// </summary>
        public string Query { get; set; }
    }
}
