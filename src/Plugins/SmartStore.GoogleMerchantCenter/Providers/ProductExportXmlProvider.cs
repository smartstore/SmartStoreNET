using System;
using System.IO;
using System.Text;
using System.Xml;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.Services.DataExchange;
using SmartStore.GoogleMerchantCenter.Models;

namespace SmartStore.GoogleMerchantCenter.Providers
{
	[SystemName("Feeds.GoogleMerchantCenterProductXml")]
	[FriendlyName("Google Merchant Center XML product feed")]
	[DisplayOrder(1)]
	[ExportProjectionSupport(
		ExportProjectionSupport.Description,
		//ExportProjectionSupport.UseOwnProductNo,
		//ExportProjectionSupport.Brand,
		//ExportProjectionSupport.MainPictureUrl,
		//ExportProjectionSupport.ShippingTime,
		//ExportProjectionSupport.ShippingCosts,
		ExportProjectionSupport.AttributeCombinationAsProduct)]
	public class ProductExportXmlProvider : IExportProvider
	{
		public bool RequiresConfiguration(out string partialViewName, out Type modelType, out Action<object> initialize)
		{
			partialViewName = "~/Plugins/SmartStore.GoogleMerchantCenter/Views/FeedGoogleMerchantCenter/ProfileConfiguration.cshtml";
			modelType = typeof(ProfileConfigurationModel);
			initialize = null;
			return true;
		}

		public ExportEntityType EntityType
		{
			get { return ExportEntityType.Product; }
		}

		public string FileExtension
		{
			get { return "XML"; }
		}

		public void Execute(IExportExecuteContext context)
		{
		}

		public void ExecuteEnded(IExportExecuteContext context)
		{
			// nothing to do
		}
	}
}