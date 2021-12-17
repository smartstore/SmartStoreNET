using System;
using System.IO;
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
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Plugins;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.Configuration;
using SmartStore.WebApi.Models.OData.Media;
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

            m.EntitySet<Address>("Addresses");
            m.EntitySet<BlogComment>("BlogComments");
            m.EntitySet<BlogPost>("BlogPosts");
            m.EntitySet<Category>("Categories");
            m.EntitySet<Country>("Countries");
            m.EntitySet<Currency>("Currencies");
            m.EntitySet<Customer>("Customers");
            m.EntitySet<CustomerRole>("CustomerRoles");
            m.EntitySet<CustomerRoleMapping>("CustomerRoleMappings");
            m.EntitySet<DeliveryTime>("DeliveryTimes");
            m.EntitySet<Discount>("Discounts");
            m.EntitySet<Download>("Downloads");
            m.EntitySet<GenericAttribute>("GenericAttributes");
            m.EntitySet<Language>("Languages");
            m.EntitySet<LocalizedProperty>("LocalizedProperties");
            m.EntitySet<Manufacturer>("Manufacturers");
            m.EntitySet<MeasureDimension>("MeasureDimensions");
            m.EntitySet<MeasureWeight>("MeasureWeights");

            m.EntitySet<MediaFile>("MediaFiles");
            m.EntitySet<MediaFolder>("MediaFolders");
            m.EntitySet<MediaTag>("MediaTags");
            m.EntitySet<MediaTrack>("MediaTracks");

            m.EntitySet<NewsLetterSubscription>("NewsLetterSubscriptions");
            m.EntitySet<OrderNote>("OrderNotes");
            m.EntitySet<Order>("Orders");
            m.EntitySet<OrderItem>("OrderItems");
            m.EntitySet<PaymentMethod>("PaymentMethods");
            m.EntitySet<ProductAttribute>("ProductAttributes");
            m.EntitySet<ProductAttributeOption>("ProductAttributeOptions");
            m.EntitySet<ProductAttributeOptionsSet>("ProductAttributeOptionsSets");
            m.EntitySet<ProductBundleItem>("ProductBundleItems");
            m.EntitySet<ProductCategory>("ProductCategories");
            m.EntitySet<ProductManufacturer>("ProductManufacturers");
            m.EntitySet<ProductMediaFile>("ProductPictures");
            m.EntitySet<Product>("Products");
            m.EntitySet<ProductSpecificationAttribute>("ProductSpecificationAttributes");
            m.EntitySet<ProductTag>("ProductTags");
            m.EntitySet<ProductVariantAttribute>("ProductVariantAttributes");
            m.EntitySet<ProductVariantAttributeValue>("ProductVariantAttributeValues");
            m.EntitySet<ProductVariantAttributeCombination>("ProductVariantAttributeCombinations");
            m.EntitySet<QuantityUnit>("QuantityUnits");
            m.EntitySet<RelatedProduct>("RelatedProducts");
            m.EntitySet<ReturnRequest>("ReturnRequests");
            m.EntitySet<RewardPointsHistory>("RewardPointsHistory");
            m.EntitySet<Setting>("Settings");
            m.EntitySet<Shipment>("Shipments");
            m.EntitySet<ShipmentItem>("ShipmentItems");
            m.EntitySet<ShippingMethod>("ShippingMethods");
            m.EntitySet<SpecificationAttributeOption>("SpecificationAttributeOptions");
            m.EntitySet<SpecificationAttribute>("SpecificationAttributes");
            m.EntitySet<StateProvince>("StateProvinces");
            m.EntitySet<Store>("Stores");
            m.EntitySet<StoreMapping>("StoreMappings");
            m.EntitySet<TaxCategory>("TaxCategories");
            m.EntitySet<TierPrice>("TierPrices");
            m.EntitySet<UrlRecord>("UrlRecords");
            m.EntitySet<SyncMapping>("SyncMappings");

            // Register OData actions and functions.
            Controllers.OData.DeliveryTimesController.Init(configData);
            Controllers.OData.OrdersController.Init(configData);
            Controllers.OData.OrderItemsController.Init(configData);
            Controllers.OData.ProductsController.Init(configData);
            Controllers.OData.MediaFilesController.Init(configData);
            Controllers.OData.MediaFoldersController.Init(configData);

            // Custom routing convention.
            configData.RoutingConventions.Insert(0, new CustomRoutingConvention());

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
    }
}
