using SmartStore.Core.Plugins;

namespace SmartStore.Core.Caching
{
    public interface IOutputCacheProvider : IProvider
    {
        OutputCacheItem Get(string key);
        void Set(string key, OutputCacheItem item);
        bool Exists(string key);

        void Remove(params string[] keys);
        void RemoveAll();

        IPagedList<OutputCacheItem> All(int pageIndex, int pageSize, bool withContent = false);
        int Count();

        int InvalidateByRoute(params string[] routes);
        int InvalidateByPrefix(string keyPrefix);
        int InvalidateByTag(params string[] tags);
    }
}
