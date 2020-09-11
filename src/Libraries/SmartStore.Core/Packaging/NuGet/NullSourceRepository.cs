using System.Linq;
using NuGet;

namespace SmartStore.Core.Packaging
{
    /// <summary>
    /// This repository implementation fakes a source (remote) repository
    /// </summary>
    internal class NullSourceRepository : PackageRepositoryBase
    {
        public override IQueryable<IPackage> GetPackages()
        {
            return Enumerable.Empty<IPackage>().AsQueryable();
        }

        public override string Source => string.Empty;

        public override bool SupportsPrereleasePackages => true;

        public override void AddPackage(IPackage package) { }

        public override void RemovePackage(IPackage package) { }
    }
}
