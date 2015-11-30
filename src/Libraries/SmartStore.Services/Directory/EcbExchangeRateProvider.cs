using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Xml;
using SmartStore.Core.Plugins;
using SmartStore.Services.Localization;

namespace SmartStore.Services.Directory
{
	[SystemName("CurrencyExchange.ECB")]
	[FriendlyName("ECB exchange rate provider")]
	[DisplayOrder(0)]
	public class EcbExchangeRateProvider : IExchangeRateProvider
    {
		private readonly ILocalizationService _localizationService;

        public EcbExchangeRateProvider(ILocalizationService localizationService)
        {
            this._localizationService = localizationService;
        }

        /// <summary>
        /// Gets currency live rates
        /// </summary>
        /// <param name="exchangeRateCurrencyCode">Exchange rate currency code</param>
        /// <returns>Exchange rates</returns>
        public IList<Core.Domain.Directory.ExchangeRate> GetCurrencyLiveRates(string exchangeRateCurrencyCode)
        {
            if (String.IsNullOrEmpty(exchangeRateCurrencyCode) ||
                exchangeRateCurrencyCode.ToLower() != "eur")
                throw new SmartException(_localizationService.GetResource("Providers.ExchangeRate.EcbExchange.SetCurrencyToEURO"));

            var exchangeRates = new List<SmartStore.Core.Domain.Directory.ExchangeRate>();

            var request = WebRequest.Create("http://www.ecb.int/stats/eurofxref/eurofxref-daily.xml") as HttpWebRequest;
            using (WebResponse response = request.GetResponse())
            {
                var document = new XmlDocument();
                document.Load(response.GetResponseStream());
                var nsmgr = new XmlNamespaceManager(document.NameTable);
                nsmgr.AddNamespace("ns", "http://www.ecb.int/vocabulary/2002-08-01/eurofxref");
                nsmgr.AddNamespace("gesmes", "http://www.gesmes.org/xml/2002-08-01");

                var node = document.SelectSingleNode("gesmes:Envelope/ns:Cube/ns:Cube", nsmgr);
                var updateDate = DateTime.ParseExact(node.Attributes["time"].Value, "yyyy-MM-dd", null);

                var provider = new NumberFormatInfo();
                provider.NumberDecimalSeparator = ".";
                provider.NumberGroupSeparator = "";
                foreach (XmlNode node2 in node.ChildNodes)
                {
                    exchangeRates.Add(new Core.Domain.Directory.ExchangeRate()
                    {
                        CurrencyCode = node2.Attributes["currency"].Value,
                        Rate = decimal.Parse(node2.Attributes["rate"].Value, provider),
                        UpdatedOn = updateDate
                    }
                    );
                }
            }
            return exchangeRates;
        }

    }
}
