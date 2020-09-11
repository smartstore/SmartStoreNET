using System.IO;

namespace SmartStore.Core.Packaging
{
    public class PackagingResult
    {
        public string ExtensionType { get; set; }

        public string PackageName { get; set; }
        public string PackageVersion { get; set; }
        public Stream PackageStream { get; set; }
    }
}
