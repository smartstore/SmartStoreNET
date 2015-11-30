using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Xml;
using Autofac;
using SmartStore.Core;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Plugins;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;
using SmartStore.Utilities;

namespace SmartStore.Web.Framework.Plugins
{
	public partial class PluginHelper
	{
		protected readonly IComponentContext _ctx;
		private PluginDescriptor _plugin;
		private string _interfaceVersion;
		private string _pluginResRootKey;
		private string _providerResRootKey;
		private Language _language;
		private string _currencyCode;
		private Currency _euroCurrency;
		private Dictionary<string, string> _resMap = new Dictionary<string, string>();

		public PluginHelper(IComponentContext componentContext, string systemName, string providerResRootKey = null /* Legacy */)
		{
			Guard.ArgumentNotEmpty(() => systemName);
			
			_ctx = componentContext;
			SystemName = systemName;
			_providerResRootKey = providerResRootKey.NullEmpty();
		}

		public static string NotSpecified
		{
			get
			{
				return "__nospec__";	// explicitly do not set a field
			}
		}

		public string SystemName { get; set; }

		public PluginDescriptor Plugin
		{
			get
			{
				if (_plugin == null)
				{
					_plugin = _ctx.Resolve<IPluginFinder>().GetPluginDescriptorBySystemName(SystemName);
					
					if (_plugin == null)
					{
						var provider = _ctx.Resolve<IProviderManager>().GetProvider(SystemName);
						if (provider != null)
						{
							_plugin = provider.Metadata.PluginDescriptor;
						}
					}
				}
				return _plugin;
			}
		}

		public string InterfaceVersion
		{
			get
			{
				if (_interfaceVersion == null)
				{
					_interfaceVersion = "{0}_v{1}".FormatWith(CommonHelper.GetAppSetting<string>("sm:ApplicationName"), Plugin.Version);
				}
				return _interfaceVersion;
			}
		}

		public Language Language
		{
			get
			{
				if (_language == null)
				{
					_language = _ctx.Resolve<IWorkContext>().WorkingLanguage;
				}
				return _language;
			}
		}

		public bool IsLanguageGerman
		{
			get
			{
				return Language.UniqueSeoCode.IsCaseInsensitiveEqual("DE");
			}
		}

		public string CurrencyCode
		{
			get
			{
				try
				{
					if (_currencyCode == null)
					{
						_currencyCode = _ctx.Resolve<IWorkContext>().WorkingCurrency.CurrencyCode;
					}
				}
				catch (Exception)
				{
				}
				return _currencyCode ?? "EUR";
			}
		}

		public Currency EuroCurrency
		{
			get
			{
				if (_euroCurrency == null)
				{
					_euroCurrency = _ctx.Resolve<ICurrencyService>().GetCurrencyByCode("EUR");
				}
				return _euroCurrency;
			}
		}

		public string GetResource(string keyOrShortKey)
		{
			string res = "";

			try
			{
				if (keyOrShortKey.HasValue())
				{
					var key = keyOrShortKey;
					var isFullExpr = key.Contains('.');
					var isProvider = !isFullExpr && _providerResRootKey != null;
					
					if (_pluginResRootKey == null)
					{
						_pluginResRootKey = Plugin.ResourceRootKey.HasValue() ? Plugin.ResourceRootKey : "Plugins.{0}".FormatWith(SystemName);
					}

					if (!isFullExpr)
					{
						key = "{0}.{1}".FormatWith(_providerResRootKey ?? _pluginResRootKey, key);
					}

					if (_resMap.ContainsKey(key))
					{
						return _resMap[key];
					}

					var loc = _ctx.Resolve<ILocalizationService>();

					res = loc.GetResource(key, returnEmptyIfNotFound: true).NullEmpty();

					if (res == null && isProvider)
					{
						// No match with provider root key! Try it again with plugin root key as fallback.
						res = loc.GetResource("{0}.{1}".FormatWith(_pluginResRootKey, keyOrShortKey), returnEmptyIfNotFound: true).NullEmpty();
					}

					if (res == null)
						res = key;

					_resMap[keyOrShortKey] = res;
				}
			}
			catch (Exception ex)
			{
				ex.Dump();
			}

			return res;
		}

		public XmlDocument CreateXmlDocument(Func<XmlWriter, bool> content)
		{
			XmlDocument doc = null;
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Encoding = new UTF8Encoding(false);

			using (MemoryStream ms = new MemoryStream())
			{
				using (XmlWriter xw = XmlWriter.Create(ms, settings))
				{
					if (content(xw))
					{
						xw.Flush();

						doc = new XmlDocument();
						doc.LoadXml(Encoding.UTF8.GetString(ms.ToArray()));
					}

					xw.Close();
					ms.Close();
					return doc;
				}
			}
		}

		public List<SelectListItem> AvailableCurrencies()
		{
			var lst = new List<SelectListItem>();
			var allCurrencies = _ctx.Resolve<ICurrencyService>().GetAllCurrencies(false);

			foreach (var c in allCurrencies)
			{
				lst.Add(new SelectListItem()
				{
					Text = c.Name,
					Value = c.Id.ToString()
				});
			}
			return lst;
		}

	}
}
