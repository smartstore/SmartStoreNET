using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web.Routing;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Localization;
using SmartStore.Core.Plugins;
using SmartStore.Services;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Web.Framework.Plugins
{
	public class PluginMediator
	{
		private static readonly ConcurrentDictionary<string, RouteInfo> _routesCache = new ConcurrentDictionary<string, RouteInfo>();
		private readonly ICommonServices _services;

		public PluginMediator(ICommonServices services)
		{
			this._services = services;
			T = NullLocalizer.Instance;
		}

		public Localizer T { get; set; }

		public string GetLocalizedFriendlyName(ProviderMetadata metadata, int languageId = 0, bool returnDefaultValue = true)
		{
			return GetLocalizedValue(metadata, "FriendlyName", x => x.FriendlyName, languageId, returnDefaultValue);
		}

		public string GetLocalizedDescription(ProviderMetadata metadata, int languageId = 0, bool returnDefaultValue = true)
		{
			return GetLocalizedValue(metadata, "Description", x => x.Description, languageId, returnDefaultValue);
		}

		public string GetLocalizedValue(ProviderMetadata metadata,
			string propertyName,
			Expression<Func<ProviderMetadata, string>> fallback,
			int languageId = 0,
			bool returnDefaultValue = true)
		{
			Guard.ArgumentNotNull(() => metadata);

			string systemName = metadata.SystemName;
			var resourceName = metadata.ResourceKeyPattern.FormatInvariant(metadata.SystemName, propertyName);
			string result = _services.Localization.GetResource(resourceName, languageId, false, "", true);

			if (result.IsEmpty() && returnDefaultValue)
				result = fallback.Compile()(metadata);

			return result;
		}

		public void SaveLocalizedValue(ProviderMetadata metadata, int languageId, string propertyName, string value)
		{
			Guard.ArgumentNotNull(() => metadata);
			Guard.ArgumentIsPositive(languageId, "languageId");
			Guard.ArgumentNotEmpty(() => propertyName);
			Guard.ArgumentNotEmpty(() => value);

			var resourceName = metadata.ResourceKeyPattern.FormatInvariant(metadata.SystemName, propertyName);
			var resource = _services.Localization.GetLocaleStringResourceByName(resourceName, languageId, false);

			if (resource != null)
			{
				if (value.IsEmpty())
				{
					// delete
					_services.Localization.DeleteLocaleStringResource(resource);
				}
				else
				{
					// update
					resource.ResourceValue = value;
					_services.Localization.UpdateLocaleStringResource(resource);
				}
			}
			else
			{
				if (value.HasValue())
				{
					// insert
					resource = new LocaleStringResource()
					{
						LanguageId = languageId,
						ResourceName = resourceName,
						ResourceValue = value,
					};
					_services.Localization.InsertLocaleStringResource(resource);
				}
			}
		}

		public T GetSetting<T>(ProviderMetadata metadata, string propertyName, int storeId = 0)
		{
			var settingKey = metadata.SettingKeyPattern.FormatInvariant(metadata.SystemName, "DisplayOrder");
			return _services.Settings.GetSettingByKey<T>(settingKey);
		}

		public void SetDisplayOrder(ProviderMetadata metadata, int displayOrder, int storeId = 0)
		{
			Guard.ArgumentNotNull(() => metadata);

			metadata.DisplayOrder = displayOrder;
			SetSetting(metadata, "DisplayOrder", displayOrder, storeId);
		}

		public void SetSetting<T>(ProviderMetadata metadata, string propertyName, T value, int storeId = 0)
		{
			Guard.ArgumentNotNull(() => metadata);
			Guard.ArgumentNotEmpty(() => propertyName);

			var settingKey = metadata.SettingKeyPattern.FormatInvariant(metadata.SystemName, propertyName);
			_services.Settings.SetSetting<T>(settingKey, value, storeId, false);
		}

		public ProviderModel ToProviderModel(Provider<IProvider> provider, Action<Provider<IProvider>, ProviderModel> enhancer = null)
		{
			return ToProviderModel<IProvider, ProviderModel>(provider, enhancer);
		}

		public TModel ToProviderModel<TProvider, TModel>(Provider<TProvider> provider, Action<Provider<TProvider>, TModel> enhancer = null)
			where TModel : ProviderModel, new()
			where TProvider : IProvider
		{
			Guard.ArgumentNotNull(() => provider);

			var metadata = provider.Metadata;
			var model = new TModel();
			model.SystemName = metadata.SystemName;
			model.FriendlyName = GetLocalizedFriendlyName(metadata);
			model.Description = GetLocalizedDescription(metadata);
			model.DisplayOrder = metadata.DisplayOrder;
			if (metadata.IsConfigurable)
			{
				var routeInfo = _routesCache.GetOrAdd(model.SystemName, (key) =>
				{
					string actionName, controllerName;
					RouteValueDictionary routeValues;
					var configurable = (IConfigurable)provider.Value;
					configurable.GetConfigurationRoute(out actionName, out controllerName, out routeValues);

					if (actionName.IsEmpty())
					{
						metadata.IsConfigurable = false;
						return null;
					}
					else
					{
						return new RouteInfo(actionName, controllerName, routeValues);
					}
				});

				if (routeInfo != null)
				{
					model.ConfigurationRoute = new RouteInfo(routeInfo);
				}
			}

			if (enhancer != null)
			{
				enhancer(provider, model);
			}

			model.IsConfigurable = metadata.IsConfigurable;

			return model;
		}
	}
}
