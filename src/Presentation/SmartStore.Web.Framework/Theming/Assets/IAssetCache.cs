using System.Collections.Generic;

namespace SmartStore.Web.Framework.Theming.Assets
{
    public class CachedAssetEntry
    {
        public string Content { get; set; }
        public string OriginalVirtualPath { get; set; }
        public string PhysicalPath { get; set; }
        public IEnumerable<string> VirtualPathDependencies { get; set; }
        public string HashCode { get; set; }
        public string ThemeName { get; set; }
        public int StoreId { get; set; }
        public string[] ProcessorCodes { get; set; }
    }

    /// <summary>
    /// A file system based caching mechanism for dynamically translated assets like Sass, Less etc.
    /// </summary>
    public interface IAssetCache
    {
        CachedAssetEntry GetAsset(string virtualPath);

        CachedAssetEntry InsertAsset(string virtualPath, IEnumerable<string> virtualPathDependencies, string content, params string[] processorCodes);

        bool InvalidateAsset(string virtualPath);

        void Clear();
    }
}
