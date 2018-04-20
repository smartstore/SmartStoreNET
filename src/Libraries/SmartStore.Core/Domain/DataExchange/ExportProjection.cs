using System;
using System.Xml.Serialization;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Core.Domain.DataExchange
{
	/// <summary>
	/// Settings projected onto an export
	/// </summary>
	/// <remarks>
	/// Note possible projection controlling: a) developer controls, b) merchant controls, c) developer controls what the merchant can control
	/// </remarks>
	[Serializable]
	public class ExportProjection
	{
		public ExportProjection()
		{
			OnlyIndividuallyVisibleAssociated = true;
		}

		#region All entity types

		/// <summary>
		/// Store identifier
		/// </summary>
		public int? StoreId { get; set; }

		/// <summary>
		/// The language to be applied to the export
		/// </summary>
		public int? LanguageId { get; set; }

		/// <summary>
		/// The currency to be applied to the export
		/// </summary>
		public int? CurrencyId { get; set; }

		/// <summary>
		/// Customer identifier
		/// </summary>
		public int? CustomerId { get; set; }

		#endregion

		#region Product

		/// <summary>
		/// Description merging identifier
		/// </summary>
		public int DescriptionMergingId { get; set; }

		/// <summary>
		/// Decription merging
		/// </summary>
		[XmlIgnore]
		public ExportDescriptionMerging DescriptionMerging
		{
			get
			{
				return (ExportDescriptionMerging)DescriptionMergingId;
			}
			set
			{
				DescriptionMergingId = (int)value;
			}
		}

		/// <summary>
		/// Convert HTML decription to plain text
		/// </summary>
		public bool DescriptionToPlainText { get; set; }

		/// <summary>
		/// Comma separated text to append to the decription
		/// </summary>
		public string AppendDescriptionText { get; set; }

		/// <summary>
		/// Remove critical characters from the description
		/// </summary>
		public bool RemoveCriticalCharacters { get; set; }

		/// <summary>
		/// Comma separated list of critical characters
		/// </summary>
		public string CriticalCharacters { get; set; }

		/// <summary>
		/// The price type for calculating the product price
		/// </summary>
		public PriceDisplayType? PriceType { get; set; }

		/// <summary>
		/// Convert net to gross prices
		/// </summary>
		public bool ConvertNetToGrossPrices { get; set; }

		/// <summary>
		/// Fallback for product brand
		/// </summary>
		public string Brand { get; set; }

		/// <summary>
		/// Number of images per object to be exported
		/// </summary>
		public int? NumberOfPictures { get; set; }

		/// <summary>
		/// Picture size
		/// </summary>
		public int PictureSize { get; set; }

		/// <summary>
		/// Fallback for shipping time
		/// </summary>
		public string ShippingTime { get; set; }

		/// <summary>
		/// Fallback for shipping costs
		/// </summary>
		public decimal? ShippingCosts { get; set; }

		/// <summary>
		/// Free shipping threshold
		/// </summary>
		public decimal? FreeShippingThreshold { get; set; }

		/// <summary>
		/// Whether to export attribute combinations as products
		/// </summary>
		public bool AttributeCombinationAsProduct { get; set; }

		/// <summary>
		/// Identifier for merging attribute values of attribute combinations
		/// </summary>
		public int AttributeCombinationValueMergingId { get; set; }

		/// <summary>
		/// Merging attribute values of attribute combinations
		/// </summary>
		[XmlIgnore]
		public ExportAttributeValueMerging AttributeCombinationValueMerging
		{
			get
			{
				return (ExportAttributeValueMerging)AttributeCombinationValueMergingId;
			}
			set
			{
				AttributeCombinationValueMergingId = (int)value;
			}
		}

		/// <summary>
		/// Whether to export grouped products
		/// </summary>
		public bool NoGroupedProducts { get; set; }

		/// <summary>
		/// Whether to export associated products that marked as "visible individually". <c>false</c> to load all records, <c>true</c> to load "visible individually" only
		/// </summary>
		public bool OnlyIndividuallyVisibleAssociated { get; set; }

		#endregion

		#region Order

		/// <summary>
		/// Identifier of the new state for orders
		/// </summary>
		public int OrderStatusChangeId { get; set; }

		/// <summary>
		/// New state for orders
		/// </summary>
		[XmlIgnore]
		public ExportOrderStatusChange OrderStatusChange
		{
			get
			{
				return (ExportOrderStatusChange)OrderStatusChangeId;
			}
			set
			{
				OrderStatusChangeId = (int)value;
			}
		}

		#endregion

		#region Shopping Cart Item

		/// <summary>
		/// Whether to export bundle products
		/// </summary>
		public bool NoBundleProducts { get; set; }

		#endregion
	}
}
