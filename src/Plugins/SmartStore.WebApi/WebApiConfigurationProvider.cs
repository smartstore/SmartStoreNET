using System;
using System.IO;
using System.Web.Http.OData.Builder;
using SmartStore.Core.Domain.Blogs;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Configuration;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Plugins;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.Configuration;
using SmartStore.WebApi.Models.OData;
using SmartStore.WebApi.Services;
using SmartStore.WebApi.Services.Swagger;
using Swashbuckle.Application;

namespace SmartStore.WebApi
{
    public partial class WebApiConfigurationProvider : IWebApiConfigurationProvider
	{
        public int Priority => 0;

        public void Configure(WebApiConfigurationBroadcaster configData)
		{
			var m = configData.ModelBuilder;

			m.EntitySet<Address>(WebApiOdataEntitySet.Addresses);
			m.EntitySet<BlogComment>(WebApiOdataEntitySet.BlogComments);
			m.EntitySet<BlogPost>(WebApiOdataEntitySet.BlogPosts);
			m.EntitySet<Category>(WebApiOdataEntitySet.Categories);
			m.EntitySet<Country>(WebApiOdataEntitySet.Countries);
			m.EntitySet<Currency>(WebApiOdataEntitySet.Currencies);
			m.EntitySet<Customer>(WebApiOdataEntitySet.Customers);
			m.EntitySet<CustomerRole>(WebApiOdataEntitySet.CustomerRoles);
            m.EntitySet<CustomerRoleMapping>(WebApiOdataEntitySet.CustomerRoleMappings);
            m.EntitySet<DeliveryTime>(WebApiOdataEntitySet.DeliveryTimes);
			m.EntitySet<Discount>(WebApiOdataEntitySet.Discounts);
			m.EntitySet<Download>(WebApiOdataEntitySet.Downloads);
			m.EntitySet<GenericAttribute>(WebApiOdataEntitySet.GenericAttributes);
			m.EntitySet<Language>(WebApiOdataEntitySet.Languages);
			m.EntitySet<LocalizedProperty>(WebApiOdataEntitySet.LocalizedPropertys);
			m.EntitySet<Manufacturer>(WebApiOdataEntitySet.Manufacturers);
			m.EntitySet<MeasureDimension>(WebApiOdataEntitySet.MeasureDimensions);
			m.EntitySet<MeasureWeight>(WebApiOdataEntitySet.MeasureWeights);
			m.EntitySet<OrderNote>(WebApiOdataEntitySet.OrderNotes);
			m.EntitySet<Order>(WebApiOdataEntitySet.Orders);
			m.EntitySet<OrderItem>(WebApiOdataEntitySet.OrderItems);
			m.EntitySet<PaymentMethod>(WebApiOdataEntitySet.PaymentMethods);
			m.EntitySet<MediaFile>(WebApiOdataEntitySet.Pictures);
			m.EntitySet<ProductAttribute>(WebApiOdataEntitySet.ProductAttributes);
			m.EntitySet<ProductAttributeOption>(WebApiOdataEntitySet.ProductAttributeOptions);
			m.EntitySet<ProductAttributeOptionsSet>(WebApiOdataEntitySet.ProductAttributeOptionsSets);
			m.EntitySet<ProductBundleItem>(WebApiOdataEntitySet.ProductBundleItems);
			m.EntitySet<ProductCategory>(WebApiOdataEntitySet.ProductCategories);
			m.EntitySet<ProductManufacturer>(WebApiOdataEntitySet.ProductManufacturers);
			m.EntitySet<ProductMediaFile>(WebApiOdataEntitySet.ProductPictures);
			m.EntitySet<Product>(WebApiOdataEntitySet.Products);
			m.EntitySet<ProductSpecificationAttribute>(WebApiOdataEntitySet.ProductSpecificationAttributes);
			m.EntitySet<ProductTag>(WebApiOdataEntitySet.ProductTags);
			m.EntitySet<ProductVariantAttribute>(WebApiOdataEntitySet.ProductVariantAttributes);
			m.EntitySet<ProductVariantAttributeValue>(WebApiOdataEntitySet.ProductVariantAttributeValues);
			m.EntitySet<ProductVariantAttributeCombination>(WebApiOdataEntitySet.ProductVariantAttributeCombinations);
			m.EntitySet<QuantityUnit>(WebApiOdataEntitySet.QuantityUnits);
			m.EntitySet<RelatedProduct>(WebApiOdataEntitySet.RelatedProducts);
			m.EntitySet<ReturnRequest>(WebApiOdataEntitySet.ReturnRequests);
			m.EntitySet<Setting>(WebApiOdataEntitySet.Settings);
			m.EntitySet<Shipment>(WebApiOdataEntitySet.Shipments);
			m.EntitySet<ShipmentItem>(WebApiOdataEntitySet.ShipmentItems);
			m.EntitySet<ShippingMethod>(WebApiOdataEntitySet.ShippingMethods);
			m.EntitySet<SpecificationAttributeOption>(WebApiOdataEntitySet.SpecificationAttributeOptions);
			m.EntitySet<SpecificationAttribute>(WebApiOdataEntitySet.SpecificationAttributes);
			m.EntitySet<StateProvince>(WebApiOdataEntitySet.StateProvinces);
			m.EntitySet<Store>(WebApiOdataEntitySet.Stores);
			m.EntitySet<StoreMapping>(WebApiOdataEntitySet.StoreMappings);
			m.EntitySet<TaxCategory>(WebApiOdataEntitySet.TaxCategories);
			m.EntitySet<TierPrice>(WebApiOdataEntitySet.TierPrices);
			m.EntitySet<UrlRecord>(WebApiOdataEntitySet.UrlRecords);
			m.EntitySet<SyncMapping>(WebApiOdataEntitySet.SyncMappings);

			AddActionsToOrder(m.Entity<Order>());
			AddActionsToOrderItem(m.Entity<OrderItem>());
			AddActionsToProduct(m.Entity<Product>());

			// Swagger integration, see http://www.my-store.com/swagger/ui/index
			var thisAssembly = typeof(WebApiConfigurationProvider).Assembly;
			var pluginFinder = configData.Configuration.DependencyResolver.GetService(typeof(IPluginFinder)) as IPluginFinder;
			var pluginDescriptor = pluginFinder.GetPluginDescriptorBySystemName(WebApiGlobal.PluginSystemName);
			var xmlCommentPath = Path.Combine(pluginDescriptor.PhysicalPath, "SmartStore.WebApi.xml");

			try
			{
				if (!configData.Configuration.Routes.ContainsKey("swagger_docsswagger/docs/{apiVersion}"))
				{
					configData.Configuration.EnableSwagger(x =>
					{
						x.SingleApiVersion("v1", pluginDescriptor.Description)
							.Description(pluginDescriptor.Description);

						// Required to display odata endpoints
						x.DocumentFilter<SwaggerOdataDocumentFilter>();
						//x.SchemaFilter<SwaggerSchemaFilter>();
						x.OperationFilter<SwaggerDefaultValueFilter>();

						if (File.Exists(xmlCommentPath))
						{
							x.IncludeXmlComments(xmlCommentPath);
						}
					})
					.EnableSwaggerUi(x =>
					{
						// JavaScript required for authentication.
						x.InjectJavaScript(thisAssembly, "SmartStore.WebApi.Scripts.CryptoJs.components.core-min.js");
						x.InjectJavaScript(thisAssembly, "SmartStore.WebApi.Scripts.CryptoJs.components.enc-base64-min.js");
						x.InjectJavaScript(thisAssembly, "SmartStore.WebApi.Scripts.CryptoJs.components.enc-utf16-min.js");
						x.InjectJavaScript(thisAssembly, "SmartStore.WebApi.Scripts.CryptoJs.rollups.hmac-md5.js");
						x.InjectJavaScript(thisAssembly, "SmartStore.WebApi.Scripts.CryptoJs.rollups.hmac-sha256.js");

						x.InjectJavaScript(thisAssembly, "SmartStore.WebApi.Scripts.smwapi-consumer.js");
						x.InjectJavaScript(thisAssembly, "SmartStore.WebApi.Scripts.swagger-auth.js");
					});
				}
			}
			catch (Exception ex)
			{
				ex.Dump();
			}
		}

        private void AddActionsToOrder(EntityTypeConfiguration<Order> config)
        {
            config.Action("Infos").Returns<OrderInfo>();

            config.Action("Pdf");

            config.Action("PaymentPending")
                .ReturnsFromEntitySet<Order>(WebApiOdataEntitySet.Orders);

            config.Action("PaymentPaid")
                .ReturnsFromEntitySet<Order>(WebApiOdataEntitySet.Orders)
                .Parameter<string>("PaymentMethodName");

            config.Action("PaymentRefund")
                .ReturnsFromEntitySet<Order>(WebApiOdataEntitySet.Orders)
                .Parameter<bool>("Online");

            config.Action("Cancel")
                .ReturnsFromEntitySet<Order>(WebApiOdataEntitySet.Orders);

            var addShipment = config.Action("AddShipment")
                .ReturnsFromEntitySet<Order>(WebApiOdataEntitySet.Orders);

            var completeOrder = config.Action("CompleteOrder")
                .ReturnsFromEntitySet<Order>(WebApiOdataEntitySet.Orders);

            addShipment.Parameter<string>("TrackingNumber");
            addShipment.Parameter<bool?>("SetAsShipped");
        }

        private void AddActionsToOrderItem(EntityTypeConfiguration<OrderItem> config)
        {
            config.Action("Infos").Returns<OrderItemInfo>();
        }

        private void AddActionsToProduct(EntityTypeConfiguration<Product> config)
        {
            config.Collection.Action("Search")
                .ReturnsCollectionFromEntitySet<Product>(WebApiOdataEntitySet.Products);

            config.Action("FinalPrice")
                .Returns<decimal>();

            config.Action("LowestPrice")
                .Returns<decimal>();

            config.Action("CreateAttributeCombinations")
                .ReturnsCollectionFromEntitySet<ProductVariantAttributeCombination>(WebApiOdataEntitySet.ProductVariantAttributeCombinations);

            var manageAttributes = config.Action("ManageAttributes")
                .ReturnsCollectionFromEntitySet<ProductVariantAttribute>(WebApiOdataEntitySet.ProductVariantAttributes);

            manageAttributes.Parameter<bool>("Synchronize");
            manageAttributes.CollectionParameter<ManageAttributeType>("Attributes");
        }
    }


    public static class WebApiOdataEntitySet
	{
		public static string Addresses { get { return "Addresses"; } }
		public static string BlogComments { get { return "BlogComments"; } }
		public static string BlogPosts { get { return "BlogPosts"; } }
		public static string Categories { get { return "Categories"; } }
		public static string Countries { get { return "Countries"; } }
		public static string Currencies { get { return "Currencies"; } }
		public static string Customers { get { return "Customers"; } }
		public static string CustomerRoles { get { return "CustomerRoles"; } }
        public static string CustomerRoleMappings { get { return "CustomerRoleMappings"; } }
        public static string DeliveryTimes { get { return "DeliveryTimes"; } }
		public static string Discounts { get { return "Discounts"; } }
		public static string Downloads { get { return "Downloads"; } }
		public static string GenericAttributes { get { return "GenericAttributes"; } }
		public static string Languages { get { return "Languages"; } }
		public static string LocalizedPropertys { get { return "LocalizedPropertys"; } }
		public static string Manufacturers { get { return "Manufacturers"; } }
		public static string MeasureDimensions { get { return "MeasureDimensions"; } }
		public static string MeasureWeights { get { return "MeasureWeights"; } }
		public static string OrderNotes { get { return "OrderNotes"; } }
		public static string Orders { get { return "Orders"; } }
		public static string OrderItems { get { return "OrderItems"; } }
		public static string PaymentMethods { get { return "PaymentMethods"; } }
		public static string Pictures { get { return "Pictures"; } }
		public static string ProductAttributes { get { return "ProductAttributes"; } }
		public static string ProductAttributeOptions { get { return "ProductAttributeOptions"; } }
		public static string ProductAttributeOptionsSets { get { return "ProductAttributeOptionsSets"; } }
		public static string ProductBundleItems { get { return "ProductBundleItems"; } }
		public static string ProductCategories { get { return "ProductCategories"; } }
		public static string ProductManufacturers { get { return "ProductManufacturers"; } }
		public static string ProductPictures { get { return "ProductPictures"; } }
		public static string Products { get { return "Products"; } }
		public static string ProductSpecificationAttributes { get { return "ProductSpecificationAttributes"; } }
		public static string ProductTags { get { return "ProductTags"; } }
		public static string ProductVariantAttributes { get { return "ProductVariantAttributes"; } }
		public static string ProductVariantAttributeValues { get { return "ProductVariantAttributeValues"; } }
		public static string ProductVariantAttributeCombinations { get { return "ProductVariantAttributeCombinations"; } }
		public static string QuantityUnits { get { return "QuantityUnits"; } }
		public static string RelatedProducts { get { return "RelatedProducts"; } }
		public static string ReturnRequests { get { return "ReturnRequests"; } }
		public static string Settings { get { return "Settings"; } }
		public static string Shipments { get { return "Shipments"; } }
		public static string ShipmentItems { get { return "ShipmentItems"; } }
		public static string ShippingMethods { get { return "ShippingMethods"; } }
		public static string SpecificationAttributeOptions { get { return "SpecificationAttributeOptions"; } }
		public static string SpecificationAttributes { get { return "SpecificationAttributes"; } }
		public static string StateProvinces { get { return "StateProvinces"; } }
		public static string Stores { get { return "Stores"; } }
		public static string StoreMappings { get { return "StoreMappings"; } }
		public static string TaxCategories { get { return "TaxCategories"; } }
		public static string TierPrices { get { return "TierPrices"; } }
		public static string UrlRecords { get { return "UrlRecords"; } }
		public static string SyncMappings { get { return "SyncMappings"; } }
	}
}
