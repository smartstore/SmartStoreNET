using System;
using System.Linq;
using System.Web;
using SmartStore.Core;
using SmartStore.Core.Domain.Stores;
using SmartStore.Services.Stores;

namespace SmartStore.Web.Framework
{
	/// <summary>
	/// Store context for web application
	/// </summary>
	public partial class WebStoreContext : IStoreContext
	{
		internal const string OverriddenStoreIdKey = "OverriddenStoreId";
		
		private readonly IStoreService _storeService;
		private readonly IWebHelper _webHelper;
		private readonly HttpContextBase _httpContext;

		private Store _currentStore;

		public WebStoreContext(IStoreService storeService, IWebHelper webHelper, HttpContextBase httpContext)
		{
			_storeService = storeService;
			_webHelper = webHelper;
			_httpContext = httpContext;
		}

		public void SetRequestStore(int? storeId)
		{
			try
			{
				var dataTokens = _httpContext.Request.RequestContext.RouteData.DataTokens;
				if (storeId.GetValueOrDefault() > 0)
				{
					dataTokens[OverriddenStoreIdKey] = storeId.Value;
				}
				else if (dataTokens.ContainsKey(OverriddenStoreIdKey))
				{
					dataTokens.Remove(OverriddenStoreIdKey);
				}

				_currentStore = null;
			}
			catch { }
		}

		public int? GetRequestStore()
		{
			try
			{
				var value = _httpContext.Request.RequestContext.RouteData.DataTokens[OverriddenStoreIdKey];
				if (value != null)
				{
					return (int)value;
				}

				return null;
			}
			catch
			{
				return null;
			}
		}

		public void SetPreviewStore(int? storeId)
		{
			try
			{
				_httpContext.SetPreviewModeValue(OverriddenStoreIdKey, storeId.HasValue ? storeId.Value.ToString() : null);
				_currentStore = null;
			}
			catch { }
		}

		public int? GetPreviewStore()
		{
			try
			{
				var cookie = _httpContext.GetPreviewModeCookie(false);
				if (cookie != null)
				{
					var value = cookie.Values[OverriddenStoreIdKey];
					if (value.HasValue())
					{
						return value.ToInt();
					}
				}

				return null;
			}
			catch
			{
				return null;
			}
		}

		/// <summary>
		/// Gets or sets the current store
		/// </summary>
		public Store CurrentStore
		{
			get
			{	
				if (_currentStore == null)
				{
					int? storeOverride = GetRequestStore() ?? GetPreviewStore();
					if (storeOverride.HasValue)
					{
						// the store to be used can be overwritten on request basis (e.g. for theme preview, editing etc.)
						_currentStore = _storeService.GetStoreById(storeOverride.Value);
					}

					if (_currentStore == null)
					{
						// ty to determine the current store by HTTP_HOST
						var host = _webHelper.ServerVariables("HTTP_HOST");
						var allStores = _storeService.GetAllStores();
						var store = allStores.FirstOrDefault(s => s.ContainsHostValue(host));

						if (store == null)
						{
							// load the first found store
							store = allStores.FirstOrDefault();
						}

						if (store == null)
							throw new Exception("No store could be loaded");

						_currentStore = store;
					}
				}

				return _currentStore;
			}
			set
			{
				_currentStore = value;
			}
		}

		/// <summary>
		/// IsSingleStoreMode ? 0 : CurrentStore.Id
		/// </summary>
		public int CurrentStoreIdIfMultiStoreMode
		{
			get
			{
				return _storeService.IsSingleStoreMode() ? 0 : CurrentStore.Id;
			}
		}

	}
}
