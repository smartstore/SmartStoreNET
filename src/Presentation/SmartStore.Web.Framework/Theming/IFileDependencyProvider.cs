using System.Collections.Generic;

namespace SmartStore.Web.Framework.Theming
{
    internal interface IFileDependencyProvider
    {
        void AddFileDependencies(ICollection<string> mappedPaths, ICollection<string> cacheKeys);
    }
}
