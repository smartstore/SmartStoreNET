using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Web.Framework;

namespace SmartStore.Admin.Models.DataExchange
{
	public abstract class ExportProjectionModelBase
	{
		[SmartResourceDisplayName("Admin.Configuration.Export.Projection.StoreId")]
		public int? StoreId { get; set; }
		public List<SelectListItem> AvailableStores { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.Projection.LanguageId")]
		public int? LanguageId { get; set; }
		public List<SelectListItem> AvailableLanguages { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.Projection.CurrencyId")]
		public int? CurrencyId { get; set; }
		public List<SelectListItem> AvailableCurrencies { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.Projection.CustomerId")]
		public int? CustomerId { get; set; }
	}


	public class ExportProductProjectionModel : ExportProjectionModelBase
	{
		[SmartResourceDisplayName("Admin.Configuration.Export.Projection.DescriptionMerging")]
		public ExportDescriptionMerging? DescriptionMerging { get; set; }
		public SelectList AvailableDescriptionMergings { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.Projection.DescriptionToPlainText")]
		public bool DescriptionToPlainText { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.Projection.AppendDescriptionText")]
		[AllowHtml]
		public string AppendDescriptionText { get; set; }
		public string SerializedAppendDescriptionText { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.Projection.RemoveCriticalCharacters")]
		public bool RemoveCriticalCharacters { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.Projection.CriticalCharacters")]
		public string CriticalCharacters { get; set; }
		public string SerializedCriticalCharacters { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.Projection.PriceType")]
		public PriceDisplayType? PriceType { get; set; }
		public SelectList AvailablePriceTypes { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.Projection.ConvertNetToGrossPrices")]
		public bool ConvertNetToGrossPrices { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.Projection.Brand")]
		public string Brand { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.Projection.PictureSize")]
		public int PictureSize { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.Projection.ShippingTime")]
		public string ShippingTime { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.Projection.ShippingCosts")]
		public decimal? ShippingCosts { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.Projection.FreeShippingThreshold")]
		public decimal? FreeShippingThreshold { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.Projection.AttributeCombinationAsProduct")]
		public bool AttributeCombinationAsProduct { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.Projection.AttributeCombinationValueMerging")]
		public ExportAttributeValueMerging? AttributeCombinationValueMerging { get; set; }
		public SelectList AvailableAttributeCombinationValueMerging { get; set; }
	}
}