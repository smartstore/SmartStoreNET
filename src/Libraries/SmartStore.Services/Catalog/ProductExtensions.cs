using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Infrastructure;
using SmartStore.Data;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;

namespace SmartStore.Services.Catalog
{
    /// <summary>
    /// Extensions
    /// </summary>
    public static class ProductExtensions
    {

        #region codehint: sm-add

        /// <summary>
        /// Merges the shared properties of a product variant and the given attributes combination.
        /// </summary>
        /// <param name="source">The source variant</param>
        /// <param name="selectedAttributes">The selected attributes (XML), for which a <see cref="ProductVariantAttributeCombination"/> should be resolved</param>
        /// <param name="productAttributeParser">Service, which handles resolving of combinations</param>
        /// <returns>A new product variant instance</returns>
        public static IProductVariant GetMergedVariant(this IProductVariant source, string selectedAttributes, IProductAttributeParser productAttributeParser)
        {
            Guard.ArgumentNotNull(productAttributeParser, "productAttributeParser");

            // let's find appropriate record
            var combination = source
                .ProductVariantAttributeCombinations
                .Where(x => x.IsActive == true)
                .FirstOrDefault(x => productAttributeParser.AreProductAttributesEqual(x.AttributesXml, selectedAttributes));

            if (combination == null)
            {
                // nothing to merge: return the original "source"
                return source;
            }

            // merge and return new instance
            return source.GetMergedVariant(combination);
        }

        /// <summary>
        /// Merges the shared properties of a product variant and the given attributes combination.
        /// </summary>
        /// <param name="source">The source variant</param>
        /// <param name="mergeWith">The variant attributes combination to be applied to the variant</param>
        /// <returns>A new product variant instance</returns>
        public static IProductVariant GetMergedVariant(this IProductVariant source, ProductVariantAttributeCombination mergeWith)
        {
            var merged = new MergedProductVariant(source);
            merged.MergeWithCombination(mergeWith);
           
            return merged;
        }

		public static ProductVariantAttributeCombination MergeWithCombination(this IProductVariant variant, string selectedAttributes)
        {
            return variant.MergeWithCombination(selectedAttributes, EngineContext.Current.Resolve<IProductAttributeParser>());
        }

        public static ProductVariantAttributeCombination MergeWithCombination(this IProductVariant variant, string selectedAttributes, IProductAttributeParser productAttributeParser)
        {
            Guard.ArgumentNotNull(productAttributeParser, "productAttributeParser");

			if (selectedAttributes.IsNullOrEmpty())
				return null;

            // let's find appropriate record
            var combination = variant
                .ProductVariantAttributeCombinations
                .Where(x => x.IsActive == true)
                .FirstOrDefault(x => productAttributeParser.AreProductAttributesEqual(x.AttributesXml, selectedAttributes));

            if (combination != null)
            {
                variant.MergeWithCombination(combination);
            }
			return combination;
        }

        public static void MergeWithCombination(this IProductVariant variant, ProductVariantAttributeCombination combination)
        {
            Guard.ArgumentNotNull(variant, "variant");

            if (combination == null)
                return;		

			if (ManageInventoryMethod.ManageStockByAttributes == (ManageInventoryMethod)variant.ManageInventoryMethodId)
				variant.StockQuantity = combination.StockQuantity;

            if (combination.Sku.HasValue()) 
                variant.Sku = combination.Sku;
            if (combination.Gtin.HasValue()) 
                variant.Gtin = combination.Gtin;
            if (combination.ManufacturerPartNumber.HasValue()) 
                variant.ManufacturerPartNumber = combination.ManufacturerPartNumber;

            if (combination.DeliveryTimeId.GetValueOrDefault() > 0)
                variant.DeliveryTimeId = combination.DeliveryTimeId;

            if (combination.Length.HasValue)
                variant.Length = combination.Length.Value;
            if (combination.Width.HasValue)
                variant.Width = combination.Width.Value;
            if (combination.Height.HasValue)
                variant.Height = combination.Height.Value;

            if (combination.BasePriceAmount.HasValue)
                variant.BasePrice.Amount = combination.BasePriceAmount.Value;
            if (combination.BasePriceBaseAmount.HasValue)
                variant.BasePrice.BaseAmount = combination.BasePriceBaseAmount.Value;
        }

		public static void GetAllCombinationImageIds(this IList<ProductVariantAttributeCombination> combinations, List<int> imageIds) {
			Guard.ArgumentNotNull(imageIds, "imageIds");

			if (combinations != null) {
				var data = combinations
					.Where(x => x.IsActive && x.AssignedPictureIds != null)
					.Select(x => x.AssignedPictureIds)
					.ToList();

				if (data.Count > 0) {
					int id;
					var ids = string.Join(",", data).SplitSafe(",").Distinct();

					foreach (string str in ids)
						if (int.TryParse(str, out id) && !imageIds.Exists(i => i == id))
							imageIds.Add(id);
				}
			}
		}

        #endregion

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
        /// <param name="productVariant">Product variant</param>
        /// <param name="localizationService">Localization service</param>
        /// <returns>The stock message</returns>
        public static string FormatStockMessage(this ProductVariant productVariant, ILocalizationService localizationService)
        {
            if (productVariant == null)
                throw new ArgumentNullException("productVariant");

            if (localizationService == null)
                throw new ArgumentNullException("localizationService");

            string stockMessage = string.Empty;

			// codehint: sm-edit
            if ((productVariant.ManageInventoryMethod == ManageInventoryMethod.ManageStock || productVariant.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes)
                && productVariant.DisplayStockAvailability)
            {
                switch (productVariant.BackorderMode)
                {
                    case BackorderMode.NoBackorders:
                        {
                            if (productVariant.StockQuantity > 0)
                            {
                                if (productVariant.DisplayStockQuantity)
                                {
                                    //display "in stock" with stock quantity
                                    stockMessage = string.Format(localizationService.GetResource("Products.Availability.InStockWithQuantity"), productVariant.StockQuantity);
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
                            if (productVariant.StockQuantity > 0)
                            {
                                if (productVariant.DisplayStockQuantity)
                                {
                                    //display "in stock" with stock quantity
                                    stockMessage = string.Format(localizationService.GetResource("Products.Availability.InStockWithQuantity"), productVariant.StockQuantity);
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
                            if (productVariant.StockQuantity > 0)
                            {
                                if (productVariant.DisplayStockQuantity)
                                {
                                    //display "in stock" with stock quantity
                                    stockMessage = string.Format(localizationService.GetResource("Products.Availability.InStockWithQuantity"), productVariant.StockQuantity);
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
        /// <param name="productVariant">Product variant</param>
        /// <param name="localizationService">Localization service</param>
        /// <returns>The stock message</returns>
        public static bool DisplayDeliveryTimeAccordingToStock(this ProductVariant productVariant)
        {
            if (productVariant == null)
                throw new ArgumentNullException("productVariant");

            bool displayDeliveryTime = true;

            if (productVariant.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
            {
                switch (productVariant.BackorderMode)
                {
                    case BackorderMode.NoBackorders:
                        {
                            if (productVariant.StockQuantity > 0)
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
        /// <param name="productVariant">Product variant</param>
        /// <returns>Result</returns>
        public static int[] ParseAllowedQuatities(this ProductVariant productVariant)
        {
            if (productVariant == null)
                throw new ArgumentNullException("productVariant");

            var result = new List<int>();
            if (!String.IsNullOrWhiteSpace(productVariant.AllowedQuantities))
            {
                productVariant
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

        /// <summary>
        /// gets the base price
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        /// <param name="localizationService">Localization service</param>
        /// <param name="priceFormatter">Price formatter</param>
        /// <returns>The base price</returns>
        public static string GetBasePriceInfo(this ProductVariant productVariant, ILocalizationService localizationService, IPriceFormatter priceFormatter)
        {
            if (productVariant == null)
                throw new ArgumentNullException("productVariant");

            if (localizationService == null)
                throw new ArgumentNullException("localizationService");

            if (productVariant.BasePrice.HasValue)
            {
				//string basePriceInfo = string.Empty;

				//string unit = productVariant.BasePrice.BaseAmount.ToString() + " " + productVariant.BasePrice.MeasureUnit;

				//// preis / (QuantityUnit * MeasureUnitBase) €
				//string price = priceFormatter.FormatPrice(Convert.ToDecimal(productVariant.Price / (productVariant.BasePrice.Amount * productVariant.BasePrice.BaseAmount)), false, false);

				////Grundpreis: {0} pro {1}
				//basePriceInfo = string.Format(localizationService.GetResource("Products.BasePriceInfo"), price, unit);

				//return basePriceInfo;

				if (productVariant.BasePrice.Amount != Decimal.Zero)
				{
					decimal basePriceValue = Convert.ToDecimal((productVariant.Price / productVariant.BasePrice.Amount) * productVariant.BasePrice.BaseAmount);

					string basePrice = priceFormatter.FormatPrice(basePriceValue, false, false);
					string unit = "{0} {1}".FormatWith(productVariant.BasePrice.BaseAmount, productVariant.BasePrice.MeasureUnit);

					return localizationService.GetResource("Products.BasePriceInfo").FormatWith(basePrice, unit);
				}
            }
			return "";
        }

    }
}
