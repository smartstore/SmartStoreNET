using System.Web;

namespace SmartStore.Services.Search
{
    public abstract partial class SearchQueryFactoryBase
    {
        protected readonly HttpContextBase _httpContext;

        protected SearchQueryFactoryBase(HttpContextBase httpContext)
        {
            _httpContext = httpContext;
        }

        protected virtual T GetValueFor<T>(string key)
        {
            T value;
            return GetValueFor(key, out value) ? value : default(T);
        }

        protected virtual bool GetValueFor<T>(string key, out T value)
        {
            var strValue = _httpContext.Request?.Unvalidated.Form?[key] ?? _httpContext.Request?.Unvalidated.QueryString?[key];

            if (strValue.HasValue())
            {
                value = strValue.Convert<T>();
                return true;
            }

            value = default(T);
            return false;
        }
    }
}
