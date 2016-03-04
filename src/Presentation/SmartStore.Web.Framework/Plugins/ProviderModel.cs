using System;
using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Core.Plugins;
using SmartStore.Services.Payments;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Framework.Plugins
{
	public class ProviderModel : ModelBase, ILocalizedModel<ProviderLocalizedModel>
	{
		private IList<ProviderLocalizedModel> _locales;

		public Type ProviderType { get; set; }
		
		[SmartResourceDisplayName("Common.SystemName")]
		public string SystemName { get; set; }

		[SmartResourceDisplayName("Common.FriendlyName")]
		[AllowHtml]
		public string FriendlyName { get; set; }

		[SmartResourceDisplayName("Common.Description")]
		[AllowHtml]
		public string Description { get; set; }

		[SmartResourceDisplayName("Common.DisplayOrder")]
		public int DisplayOrder { get; set; }

		public bool IsEditable { get; set; }

		public bool IsConfigurable { get; set; }

		public RouteInfo ConfigurationRoute { get; set; }

		public PluginDescriptor PluginDescriptor { get; set; }

		[SmartResourceDisplayName("Admin.Providers.ProvidingPlugin")]
		public string ProvidingPluginFriendlyName { get; set; }

		/// <summary>
		/// Returns the absolute path of the provider's icon url. 
		/// </summary>
		/// <remarks>
		/// The parent plugin's icon url is returned as a fallback, if provider icon cannot be resolved.
		/// </remarks>
		public string IconUrl { get; set; }

		public IList<ProviderLocalizedModel> Locales
		{
			get
			{
				if (_locales == null)
				{
					_locales = new List<ProviderLocalizedModel>();
				}
				return _locales;
			}
			set
			{
				_locales = value;
			}
		}

		public bool IsPaymentMethod
		{
			get
			{
				return ProviderType == typeof(IPaymentMethod);
			}
		}
	}

	public class ProviderLocalizedModel : ILocalizedModelLocal
	{
		public int LanguageId { get; set; }

		[SmartResourceDisplayName("Common.FriendlyName")]
		[AllowHtml]
		public string FriendlyName { get; set; }

		[SmartResourceDisplayName("Common.Description")]
		[AllowHtml]
		public string Description { get; set; }
	}

}