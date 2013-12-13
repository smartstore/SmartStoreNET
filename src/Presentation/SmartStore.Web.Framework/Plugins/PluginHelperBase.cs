using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Xml;
using SmartStore.Core;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Plugins;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;

namespace SmartStore.Web.Framework.Plugins
{
	public partial class PluginHelperBase
	{
		private PluginDescriptor _plugin;
		private string _interfaceVersion;
		private Language _language;
		private int? _currencyID;
		private string _currencyCode;
		private Currency _euroCurrency;
		private Dictionary<string, string> _key2ResString = new Dictionary<string, string>();

		public PluginHelperBase(string systemName)
		{
			SystemName = systemName;
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
					_plugin = EngineContext.Current.Resolve<IPluginFinder>()
						.GetPluginDescriptors().FirstOrDefault(p => p.SystemName.IsCaseInsensitiveEqual(SystemName));
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
					_interfaceVersion = "{0}_v{1}".FormatWith(ConfigurationManager.AppSettings.Get("ApplicationName"), Plugin.Version);
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
					_language = EngineContext.Current.Resolve<IWorkContext>().WorkingLanguage;
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
		public int CurrencyID
		{
			get
			{
				if (!_currencyID.HasValue)
				{
					_currencyID = EngineContext.Current.Resolve<CurrencySettings>().PrimaryStoreCurrencyId;
				}
				return _currencyID ?? 1;
			}
		}
		public string CurrencyCode
		{
			get
			{
				try
				{
					if (_currencyCode == null)
						_currencyCode = EngineContext.Current.Resolve<IWorkContext>().WorkingCurrency.CurrencyCode;
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
					_euroCurrency = EngineContext.Current.Resolve<ICurrencyService>().GetCurrencyByCode("EUR");
				}
				return _euroCurrency;
			}
		}

		public string Resource(string keyOrShortKey)
		{
			string res = "";
			try
			{
				if (keyOrShortKey.HasValue())
				{
					if (!keyOrShortKey.Contains('.'))
					{
						if (Plugin.ResourceRootKey.HasValue())
							keyOrShortKey = "{0}.{1}".FormatWith(Plugin.ResourceRootKey, keyOrShortKey);
						else
							keyOrShortKey = "Plugins.{0}.{1}".FormatWith(SystemName, keyOrShortKey);
					}

					if (_key2ResString.ContainsKey(keyOrShortKey))
						return _key2ResString[keyOrShortKey];

					res = EngineContext.Current.Resolve<ILocalizationService>().GetResource(keyOrShortKey);

					if (res.IsNullOrEmpty())
						res = keyOrShortKey;

					_key2ResString.Add(keyOrShortKey, res);
				}
			}
			catch (Exception exc)
			{
				exc.Dump();
			}
			return res;
		}
		public XmlDocument CreateXmlDocument(Func<XmlWriter, bool> content)
		{
			XmlDocument doc = null;
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Encoding = new UTF8Encoding(false);

			using (MemoryStream ms = new MemoryStream())
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
		public Currency GetUsedCurrency(int currencyId)
		{
			var currencyService = EngineContext.Current.Resolve<ICurrencyService>();
			var currency = currencyService.GetCurrencyById(currencyId);

			if (currency == null || !currency.Published)
				currency = currencyService.GetCurrencyById(CurrencyID);

			return currency;
		}
		public decimal ConvertFromStoreCurrency(decimal price, Currency currency)
		{
			return EngineContext.Current.Resolve<ICurrencyService>().ConvertFromPrimaryStoreCurrency(price, currency);
		}
		public List<SelectListItem> AvailableCurrencies()
		{
			var lst = new List<SelectListItem>();
			var allCurrencies = EngineContext.Current.Resolve<ICurrencyService>().GetAllCurrencies(false);

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
