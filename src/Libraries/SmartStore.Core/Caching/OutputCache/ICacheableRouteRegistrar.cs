namespace SmartStore.Core.Caching
{
    public interface ICacheableRouteRegistrar
    {
        void RegisterCacheableRoute(params string[] routes);
        void RemoveCacheableRoute(params string[] routes);
    }

    public class NullCacheableRouteRegistrar : ICacheableRouteRegistrar
    {
        public void RegisterCacheableRoute(params string[] routes)
        {
        }

        public void RemoveCacheableRoute(params string[] routes)
        {
        }
    }
}
