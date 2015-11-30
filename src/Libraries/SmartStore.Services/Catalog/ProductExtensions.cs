using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Infrastructure;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Seo;

namespace SmartStore.Services.Catalog
{
    public static class ProductExtensions
    {
		public static ProductVariantAttributeCombination MergeWithCombination(this Product product, string selectedAttributes)
        {
            return product.MergeWithCombination(selectedAttributes, EngineContext.Current.Resolve<IProductAttributeParser>());
        }

		public static ProductVariantAttributeCombination MergeWithCombination(this Product product, string selectedAttributes, IProductAttributeParser productAttributeParser)
        {
            Guard.ArgumentNotNull(productAttributeParser, "productAttributeParser");

			if (selectedAttributes.IsEmpty())
				return null;

            // let's find appropriate record
			var combination = product
                .ProductVariantAttributeCombinations
                .Where(x => x.IsActive == true)
                .FirstOrDefault(x => productAttributeParser.AreProductAttributesEqual(x.AttributesXml, selectedAttributes));

            if (combination != null)
            {
				product.MergeWithCombination(combination);
            }

			return combination;
        }

		public static void MergeWithCombination(this Product product, ProductVariantAttributeCombination combination)
		{
			Guard.ArgumentNotNull(product, "product");

			if (product.MergedDataValues != null)
				product.MergedDataValues.Clear();

			if (combination == null)
				return;

			if (product.MergedDataValues == null)
				product.MergedDataValues = new Dictionary<string, object>();

            if (ManageInventoryMethod.ManageStockByAttributes == (ManageInventoryMethod)product.ManageInventoryMethodId)
            {
                product.MergedDataValues.Add("StockQuantity", combination.StockQuantity);
				product.MergedDataValues.Add("BackorderModeId", combination.AllowOutOfStockOrders ? (int)BackorderMode.AllowQtyBelow0 : (int)BackorderMode.NoBackorders);
            }

			if (combination.Sku.HasValue())
				product.MergedDataValues.Add("Sku", combination.Sku);
			if (combination.Gtin.HasValue())
				product.MergedDataValues.Add("Gtin", combination.Gtin);
			if (combination.ManufacturerPartNumber.HasValue())
				product.MergedDataValues.Add("ManufacturerPartNumber", combination.ManufacturerPartNumber);

			if (combination.Price.HasValue)
				product.MergedDataValues.Add("Price", combination.Price.Value);

			if (combination.DeliveryTimeId.HasValue && combination.DeliveryTimeId.Value > 0)
				product.MergedDataValues.Add("DeliveryTimeId", combination.DeliveryTimeId);

			if (combination.QuantityUnitId.HasValue && combination.QuantityUnitId.Value > 0)
				product.MergedDataValues.Add("QuantityUnitId", combination.QuantityUnitId);

			if (combination.Length.HasValue)
				product.MergedDataValues.Add("Length", combination.Length.Value);
			if (combination.Width.HasValue)
				product.MergedDataValues.Add("Width", combination.Width.Value);
			if (combination.Height.HasValue)
				product.MergedDataValues.Add("Height", combination.Height.Value);

			if (combination.BasePriceAmount.HasValue)
				product.MergedDataValues.Add("BasePriceAmount", combination.BasePriceAmount);
			if (combination.BasePriceBaseAmount.HasValue)
				product.MergedDataValues.Add("BasePriceBaseAmount", combination.BasePriceBaseAmount);
		}

		public static void GetAllCombinationImageIds(this IList<ProductVariantAttributeCombination> combinations, List<int> imageIds)
		{
			Guard.ArgumentNotNull(imageIds, "imageIds");

			if (combinations != null)
			{
				var data = combinations
					.Where(x => x.IsActive && x.AssignedPictureIds != null)
					.Select(x => x.AssignedPictureIds)
					.ToList();

				if (data.Count > 0)
				{
					int id;
					var ids = string.Join(",", data).SplitSafe(",").Distinct();

					foreach (string str in ids)
					{
						if (int.TryParse(str, out id) && !imageIds.Exists(i => i == id))
							imageIds.Add(id);
					}
				}
			}
		}

        /// <summary>
        /// Finds a related product item by specified identifiers
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="productId1">The first product identifier</param>
        /// <param name="productId2">The second product identifier</param>
        /// <returns>Related product</returns>
        public static RelatedProduct FindRelatedProduct(this IList<RelatedProduct> source,
            int productId1, int productId2)
        {
            foreach (RelatedProduct relatedProduct in source)
                if (relatedProduct.ProductId1 == productId1 && relatedProduct.ProductId2 == productId2)
                    return relatedProduct;
            return null;
        }

        /// <summary>
        /// Finds a cross-sell product item by specified identifiers
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="productId1">The first product identifier</param>
        /// <param name="productId2">The second product identifier</param>
        /// <returns>Cross-sell product</returns>
        public static CrossSellProduct FindCrossSellProduct(this IList<CrossSellProduct> source,
            int productId1, int productId2)
        {
            foreach (CrossSellProduct crossSellProduct in source)
                if (crossSellProduct.ProductId1 == productId1 && crossSellProduct.ProductId2 == productId2)
                    return crossSellProduct;
            return null;
        }

        /// <summary>
        /// Get a default picture of a product 
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="pictureService">Picture service</param>
        /// <returns>Product picture</returns>
        public static Picture GetDefaultProductPicture(this Product source, IPictureService pictureService)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            if (pictureService == null)
                throw new ArgumentNullException("pictureService");

            var picture = pictureService.GetPicturesByProductId(source.Id, 1).FirstOrDefault();
            return picture;
        }

		public static bool IsAvailableByStock(this Product product)
		{
			if (product == null)
				throw new ArgumentNullException("product");

			if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock || product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes)
			{
				if (product.StockQuantity <= 0 && product.BackorderMode == BackorderMode.NoBackorders)
					return false;
			}
			return true;
		}

        /// <summary>
        /// Formats the stock availability/quantity message
        /// </summary>
		/// <param name="product">Product</param>
        /// <param name="localizationService">Localization service</param>
        /// <returns>The stock message</returns>
        public static string FormatStockMessage(this Product product, ILocalizationService localizationService)
        {
			if (product == null)
				throw new ArgumentNullException("product");

            if (localizationService == null)
                throw new ArgumentNullException("localizationService");

            string stockMessage = string.Empty;

            if ((product.ManageInventoryMethod == ManageInventoryMethod.ManageStock || product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes)
                && product.DisplayStockAvailability)
            {
				if (product.StockQuantity > 0)
				{
					if (product.DisplayStockQuantity)
						stockMessage = string.Format(localizationService.GetResource("Products.Availability.InStockWithQuantity"), product.StockQuantity);
					else
						stockMessage = localizationService.GetResource("Products.Availability.InStock");
				}
				else
				{
					if (product.BackorderMode == BackorderMode.NoBackorders || product.BackorderMode == BackorderMode.AllowQtyBelow0)
						stockMessage = localizationService.GetResource("Products.Availability.OutOfStock");
					else if (product.BackorderMode == BackorderMode.AllowQtyBelow0AndNotifyCustomer)
						stockMessage = localizationService.GetResource("Products.Availability.Backordering");
				}
            }

            return stockMessage;
        }

        /// <summary>
        /// Formats the stock availability/quantity message
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="localizationService">Localization service</param>
        /// <returns>The stock message</returns>
        public static bool DisplayDeliveryTimeAccordingToStock(this Product product, CatalogSettings catalogSettings)
        {
            if (product == null)
                throw new ArgumentNullException("product");

			if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock || product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes)
			{
				if (catalogSettings.DeliveryTimeIdForEmptyStock.HasValue && product.StockQuantity <= 0)
					return true;

				return (product.StockQuantity > 0);
			}
            return true;
        }


        public static bool ProductTagExists(this Product product, int productTagId)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            bool result = product.ProductTags.ToList().Find(pt => pt.Id == productTagId) != null;
            return result;
        }

        /// <summary>
        /// Get a list of allowed quanities (parse 'AllowedQuanities' property)
        /// </summary>
		/// <param name="product">Product</param>
        /// <returns>Result</returns>
		public static int[] ParseAllowedQuatities(this Product product)
        {
			if (product == null)
				throw new ArgumentNullException("product");

            var result = new List<int>();
            if (!String.IsNullOrWhiteSpace(product.AllowedQuantities))
            {
                product
                    .AllowedQuantities
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .ToList()
                    .ForEach(qtyStr =>
                    {
                        int qty = 0;
                        if (int.TryParse(qtyStr.Trim(), out qty))
                        {
                            result.Add(qty);
                        }
                    });
            }

            return result.ToArray();
        }

		public static int[] ParseRequiredProductIds(this Product product)
		{
			if (product == null)
				throw new ArgumentNullException("product");

			if (String.IsNullOrEmpty(product.RequiredProductIds))
				return new int[0];

			var ids = new List<int>();

			foreach (var idStr in product.RequiredProductIds
				.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(x => x.Trim()))
			{
				int id = 0;
				if (int.TryParse(idStr, out id))
					ids.Add(id);
			}

			return ids.ToArray();
		}

        /// <summary>
        /// Gets the base price
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="localizationService">Localization service</param>
        /// <param name="priceFormatter">Price formatter</param>
		/// <param name="priceAdjustment">Price adjustment</param>
		/// <param name="languageIndependent">Whether the result string should be language independent</param>
        /// <returns>The base price</returns>
        public static string GetBasePriceInfo(this Product product, ILocalizationService localizationService, IPriceFormatter priceFormatter,
			decimal priceAdjustment = decimal.Zero, bool languageIndependent = false)
        {
            if (product == null)
                throw new ArgumentNullException("product");

			if (localizationService == null && !languageIndependent)
                throw new ArgumentNullException("localizationService");

            if (product.BasePriceHasValue && product.BasePriceAmount != Decimal.Zero)
            {
				decimal price = decimal.Add(product.Price, priceAdjustment);
				decimal basePriceValue = Convert.ToDecimal((price / product.BasePriceAmount) * product.BasePriceBaseAmount);

				string basePrice = priceFormatter.FormatPrice(basePriceValue, true, false);
				string unit = "{0} {1}".FormatWith(product.BasePriceBaseAmount, product.BasePriceMeasureUnit);

				if (languageIndependent)
				{
					return "{0} / {1}".FormatWith(basePrice, unit);
				}

				return localizationService.GetResource("Products.BasePriceInfo").FormatWith(basePrice, unit);
            }
			return "";
        }

		public static string GetProductTypeLabel(this Product product, ILocalizationService localizationService)
		{
			if (product != null && product.ProductType != ProductType.SimpleProduct)
			{
				string key = "Admin.Catalog.Products.ProductType.{0}.Label".FormatWith(product.ProductType.ToString());
				return localizationService.GetResource(key);
			}
			return "";
		}

		public static bool CanBeBundleItem(this Product product)
		{
			return (product != null && product.ProductType == ProductType.SimpleProduct && !product.IsRecurring && !product.IsDownload);
		}

		public static bool IsValid(this ProductBundleItemData bundleItemData)
		{
			return (bundleItemData != null && bundleItemData.Item != null);
		}
		public static bool FilterOut(this ProductBundleItemData bundleItemData, ProductVariantAttributeValue value, out ProductBundleItemAttributeFilter filter)
		{
			if (bundleItemData.IsValid() && value != null && bundleItemData.Item.FilterAttributes)
			{
				filter = bundleItemData.Item.AttributeFilters.FirstOrDefault(x => x.AttributeId == value.ProductVariantAttributeId && x.AttributeValueId == value.Id);

				return (filter == null);
			}
			filter = null;
			return false;
		}
		public static string GetLocalizedName(this ProductBundleItem bundleItem)
		{
			if (bundleItem != null)
			{
				string name = bundleItem.GetLocalized(x => x.Name);
				return (name.HasValue() ? name : bundleItem.Product.GetLocalized(x => x.Name));
			}
			return null;
		}
		public static string GetLocalizedName(this ProductBundleItem bundleItem, int languageId)
		{
			if (bundleItem != null)
			{
				string name = bundleItem.GetLocalized(x => x.Name, languageId);
				return (name.HasValue() ? name : bundleItem.Product.GetLocalized(x => x.Name, languageId));
			}
			return null;
		}
		
		public static ProductBundleItemOrderData ToOrderData(this ProductBundleItemData bundleItemData, decimal priceWithDiscount = decimal.Zero, 
			string attributesXml = null, string attributesInfo = null)
		{
			if (!bundleItemData.IsValid())
				return null;

			var item = bundleItemData.Item;
			string bundleItemName = item.GetLocalized(x => x.Name);

			var bundleData = new ProductBundleItemOrderData()
			{
				BundleItemId = item.Id,
				ProductId = item.ProductId,
				Sku = item.Product.Sku,
				ProductName = (bundleItemName ?? item.Product.GetLocalized(x => x.Name)),
				ProductSeName = item.Product.GetSeName(),
				VisibleIndividually = item.Product.VisibleIndividually,
				Quantity = item.Quantity,
				DisplayOrder = item.DisplayOrder,
				PriceWithDiscount = priceWithDiscount,
				AttributesXml = attributesXml,
				AttributesInfo = attributesInfo,
				PerItemShoppingCart = item.BundleProduct.BundlePerItemShoppingCart
			};

			return bundleData;
		}
		public static void ToOrderData(this ProductBundleItemData bundleItemData, IList<ProductBundleItemOrderData> bundleData, decimal priceWithDiscount = decimal.Zero,
			string attributesXml = null, string attributesInfo = null)
		{
			var item = bundleItemData.ToOrderData(priceWithDiscount, attributesXml, attributesInfo);

			if (item != null && item.ProductId != 0 && item.BundleItemId != 0)
				bundleData.Add(item);
		}

    }
}
