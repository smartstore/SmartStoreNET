using System.Linq;
using System.Web;
using SmartStore.Collections;
using SmartStore.Utilities;

namespace SmartStore.Services.Search
{
    public abstract partial class SearchQueryFactoryBase
    {
        protected readonly HttpContextBase _httpContext;

        private Multimap<string, string> _aliases;

        protected SearchQueryFactoryBase(HttpContextBase httpContext)
        {
            _httpContext = httpContext;
        }

        protected abstract string[] Tokens { get; }

        protected virtual Multimap<string, string> Aliases
        {
            get
            {
                if (_aliases == null)
                {
                    _aliases = new Multimap<string, string>();

                    if (_httpContext.Request != null)
                    {
                        var tokens = Tokens;
                        var form = _httpContext.Request.Form;
                        var query = _httpContext.Request.QueryString;

                        if (form != null)
                        {
                            foreach (var key in form.AllKeys)
                            {
                                if (key.HasValue() && !tokens.Contains(key))
                                {
                                    _aliases.AddRange(key, form[key].SplitSafe(","));
                                }
                            }
                        }

                        if (query != null)
                        {
                            foreach (var key in query.AllKeys)
                            {
                                if (key.HasValue() && !tokens.Contains(key))
                                {
                                    _aliases.AddRange(key, query[key].SplitSafe(","));
                                }
                            }
                        }
                    }
                }

                return _aliases;
            }
        }

        protected virtual T GetValueFor<T>(string key)
        {
            return TryGetValueFor(key, out T value) ? value : default;
        }

        protected virtual bool TryGetValueFor<T>(string key, out T value)
        {
            var strValue = _httpContext.Request?.Unvalidated.Form?[key] ?? _httpContext.Request?.Unvalidated.QueryString?[key];

            if (strValue.HasValue())
            {
                return CommonHelper.TryConvert<T>(strValue, out value);
            }

            value = default;
            return false;
        }

        protected virtual bool TryParseRange<T>(string query, out T? min, out T? max) where T : struct
        {
            min = max = null;

            if (query.IsEmpty())
            {
                return false;
            }

            // Format: from~to || from[~] || ~to
            var arr = query.Split('~').Select(x => x.Trim()).Take(2).ToArray();

            CommonHelper.TryConvert(arr[0], out min);
            if (arr.Length == 2)
            {
                CommonHelper.TryConvert(arr[1], out max);
            }

            return min != null || max != null;
        }
    }
}
