using System.Collections.Generic;
using BundleTransformer.Core.Assets;

namespace SmartStore.Web.Framework.Theming.Assets
{
    internal class CachedAsset : IAsset
    {
        public string AssetTypeCode { get; set; }
        public bool Combined { get; set; }
        public string Content { get; set; }
        public bool IsScript { get; set; }
        public bool IsStylesheet { get; set; }
        public bool Minified { get; set; }
        public bool RelativePathsResolved { get; set; }
        public bool Autoprefixed { get; set; }
        public IList<IAsset> OriginalAssets { get; set; }
        public string Url { get; set; }
        public string VirtualPath { get; set; }
        public IList<string> VirtualPathDependencies { get; set; }
    }
}
