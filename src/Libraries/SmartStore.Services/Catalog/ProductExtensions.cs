using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Infrastructure;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;

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

			if (selectedAttributes.IsNullOrEmpty())
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
				product.MergedDataValues.Add("StockQuantity", combination.StockQuantity);

			if (combination.Sku.HasValue())
				product.MergedDataValues.Add("Sku", combination.Sku);
			if (combination.Gtin.HasValue())
				product.MergedDataValues.Add("Gtin", combination.Gtin);
			if (combination.ManufacturerPartNumber.HasValue())
				product.MergedDataValues.Add("ManufacturerPartNumber", combination.ManufacturerPartNumber);

			if (combination.DeliveryTimeId.HasValue && combination.DeliveryTimeId.Value > 0)
				product.MergedDataValues.Add("DeliveryTimeId", combination.DeliveryTimeId);

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

			// codehint: sm-edit
            if ((product.ManageInventoryMethod == ManageInventoryMethod.ManageStock || product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes)
                && product.DisplayStockAvailability)
            {
                switch (product.BackorderMode)
                {
                    case BackorderMode.NoBackorders:
                        {
                            if (product.StockQuantity > 0)
                            {
                                if (product.DisplayStockQuantity)
                                {
                                    //display "in stock" with stock quantity
                                    stockMessage = string.Format(localizationService.GetResource("Products.Availability.InStockWithQuantity"), product.StockQuantity);
                                }
                                else
                                {
                                    //display "in stock" without stock quantity
                                    stockMessage = localizationService.GetResource("Products.Availability.InStock");
                                }
                            }
                            else
                            {
                                //display "out of stock"
                                stockMessage = localizationService.GetResource("Products.Availability.OutOfStock");
                            }
                        }
                        break;
                    case BackorderMode.AllowQtyBelow0:
                        {
                            if (product.StockQuantity > 0)
                            {
                                if (product.DisplayStockQuantity)
                                {
                                    //display "in stock" with stock quantity
                                    stockMessage = string.Format(localizationService.GetResource("Products.Availability.InStockWithQuantity"), product.StockQuantity);
                                }
                                else
                                {
                                    //display "in stock" without stock quantity
                                    stockMessage = localizationService.GetResource("Products.Availability.InStock");
                                }
                            }
                            else
                            {
                                //display "in stock" without stock quantity
                                stockMessage = localizationService.GetResource("Products.Availability.InStock");
                            }
                        }
                        break;
                    case BackorderMode.AllowQtyBelow0AndNotifyCustomer:
                        {
                            if (product.StockQuantity > 0)
                            {
                                if (product.DisplayStockQuantity)
                                {
                                    //display "in stock" with stock quantity
                                    stockMessage = string.Format(localizationService.GetResource("Products.Availability.InStockWithQuantity"), product.StockQuantity);
                                }
                                else
                                {
                                    //display "in stock" without stock quantity
                                    stockMessage = localizationService.GetResource("Products.Availability.InStock");
                                }
                            }
                            else
                            {
                                //display "backorder" without stock quantity
                                stockMessage = localizationService.GetResource("Products.Availability.Backordering");
                            }
                        }
                        break;
                    default:
                        break;
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
        public static bool DisplayDeliveryTimeAccordingToStock(this Product product)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            bool displayDeliveryTime = true;

            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
            {
                switch (product.BackorderMode)
                {
                    case BackorderMode.NoBackorders:
                        {
                            if (product.StockQuantity > 0)
                            {
                                displayDeliveryTime = true;
                            }
                            else
                            {
                                displayDeliveryTime = false;
                            }
                        }
                        break;
                    case BackorderMode.AllowQtyBelow0:
                        {
                            displayDeliveryTime = true;
                        }
                        break;
                    case BackorderMode.AllowQtyBelow0AndNotifyCustomer:
                        {
                            displayDeliveryTime = true;
                        }
                        break;
                    default:
                        break;
                }
            }

            return displayDeliveryTime;
        }


        public static bool ProductTagExists(this Product product,
            int productTagId)
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
        /// gets the base price
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="localizationService">Localization service</param>
        /// <param name="priceFormatter">Price formatter</param>
		/// <param name="priceAdjustment">Price adjustment</param>
        /// <returns>The base price</returns>
        public static string GetBasePriceInfo(this Product product, ILocalizationService localizationService, IPriceFormatter priceFormatter,
			decimal priceAdjustment = decimal.Zero)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            if (localizationService == null)
                throw new ArgumentNullException("localizationService");

            if (product.BasePriceHasValue && product.BasePriceAmount != Decimal.Zero)
            {
				decimal price = decimal.Add(product.Price, priceAdjustment);
				decimal basePriceValue = Convert.ToDecimal((price / product.BasePriceAmount) * product.BasePriceBaseAmount);

				string basePrice = priceFormatter.FormatPrice(basePriceValue, false, false);
				string unit = "{0} {1}".FormatWith(product.BasePriceBaseAmount, product.BasePriceMeasureUnit);

				return localizationService.GetResource("Products.BasePriceInfo").FormatWith(basePrice, unit);
            }
			return "";
        }

		public static bool FilterOut(this ProductBundleItem bundleItem, ProductVariantAttributeValue value, out ProductBundleItemAttributeFilter filter)
		{
			if (bundleItem != null && value != null && bundleItem.FilterAttributes)
			{
				filter = bundleItem.AttributeFilters.FirstOrDefault(x => x.AttributeId == value.ProductVariantAttributeId && x.AttributeValueId == value.Id);

				return (filter == null);
			}
			filter = null;
			return false;
		}
    }
}
