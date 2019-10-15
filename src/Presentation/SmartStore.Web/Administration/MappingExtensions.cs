using AutoMapper;
using SmartStore.Admin.Models.Blogs;
using SmartStore.Admin.Models.Catalog;
using SmartStore.Admin.Models.Common;
using SmartStore.Admin.Models.Customers;
using SmartStore.Admin.Models.Directory;
using SmartStore.Admin.Models.Discounts;
using SmartStore.Admin.Models.Forums;
using SmartStore.Admin.Models.Localization;
using SmartStore.Admin.Models.Logging;
using SmartStore.Admin.Models.Messages;
using SmartStore.Admin.Models.News;
using SmartStore.Admin.Models.Orders;
using SmartStore.Admin.Models.Plugins;
using SmartStore.Admin.Models.Polls;
using SmartStore.Admin.Models.Settings;
using SmartStore.Admin.Models.Shipping;
using SmartStore.Admin.Models.Stores;
using SmartStore.Admin.Models.Tax;
using SmartStore.Admin.Models.Themes;
using SmartStore.Admin.Models.Topics;
using SmartStore.ComponentModel;
using SmartStore.Core.Domain.Blogs;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Forums;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Logging;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Domain.News;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Polls;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Domain.Themes;
using SmartStore.Core.Domain.Topics;
using SmartStore.Core.Plugins;
using SmartStore.Services.Common;
using SmartStore.Services.Shipping;
using SmartStore.Services.Tax;

namespace SmartStore.Admin
{
    public static class MappingExtensions
    {
        #region Category

        public static CategoryModel ToModel(this Category entity)
        {
            return MapperFactory.GetMapper<Category, CategoryModel>().Map(entity);
        }

        public static Category ToEntity(this CategoryModel model)
        {
            return MapperFactory.GetMapper<CategoryModel, Category>().Map(model);
        }

        public static Category ToEntity(this CategoryModel model, Category entity)
        {
            MapperFactory.GetMapper<CategoryModel, Category>().Map(model, entity);
            return entity;
        }

        #endregion

        #region Manufacturer

        public static ManufacturerModel ToModel(this Manufacturer entity)
        {
            return MapperFactory.GetMapper<Manufacturer, ManufacturerModel>().Map(entity);
        }

        public static Manufacturer ToEntity(this ManufacturerModel model)
        {
            return MapperFactory.GetMapper<ManufacturerModel, Manufacturer>().Map(model);
        }

        public static Manufacturer ToEntity(this ManufacturerModel model, Manufacturer entity)
        {
            MapperFactory.GetMapper<ManufacturerModel, Manufacturer>().Map(model, entity);
            return entity;
        }

        #endregion

        #region Products

        public static ProductModel ToModel(this Product entity)
        {
            return MapperFactory.GetMapper<Product, ProductModel>().Map(entity);
        }

        public static Product ToEntity(this ProductModel model)
        {
            return MapperFactory.GetMapper<ProductModel, Product>().Map(model);
        }

        public static Product ToEntity(this ProductModel model, Product entity)
        {
            MapperFactory.GetMapper<ProductModel, Product>().Map(model, entity);
            return entity;
        }

        #endregion

        #region Product attributes

        public static ProductAttributeModel ToModel(this ProductAttribute entity)
        {
            return MapperFactory.GetMapper<ProductAttribute, ProductAttributeModel>().Map(entity);
        }

        public static ProductAttribute ToEntity(this ProductAttributeModel model)
        {
            return MapperFactory.GetMapper<ProductAttributeModel, ProductAttribute>().Map(model);
        }

        public static ProductAttribute ToEntity(this ProductAttributeModel model, ProductAttribute entity)
        {
            MapperFactory.GetMapper<ProductAttributeModel, ProductAttribute>().Map(model, entity);
            return entity;
        }

		public static ProductAttributeOptionModel ToModel(this ProductAttributeOption entity)
		{
            return MapperFactory.GetMapper<ProductAttributeOption, ProductAttributeOptionModel>().Map(entity);
		}

		public static ProductAttributeOption ToEntity(this ProductAttributeOptionModel model)
		{
            return MapperFactory.GetMapper<ProductAttributeOptionModel, ProductAttributeOption>().Map(model);
		}

		public static ProductAttributeOption ToEntity(this ProductAttributeOptionModel model, ProductAttributeOption entity)
		{
            MapperFactory.GetMapper<ProductAttributeOptionModel, ProductAttributeOption>().Map(model, entity);
            return entity;
		}

		#endregion

		#region Specification attributes

		public static SpecificationAttributeModel ToModel(this SpecificationAttribute entity)
        {
            return MapperFactory.GetMapper<SpecificationAttribute, SpecificationAttributeModel>().Map(entity);
        }

        public static SpecificationAttribute ToEntity(this SpecificationAttributeModel model)
        {
            return MapperFactory.GetMapper<SpecificationAttributeModel, SpecificationAttribute>().Map(model);
        }

        public static SpecificationAttribute ToEntity(this SpecificationAttributeModel model, SpecificationAttribute entity)
        {
            MapperFactory.GetMapper<SpecificationAttributeModel, SpecificationAttribute>().Map(model, entity);
            return entity;
        }

        public static SpecificationAttributeOptionModel ToModel(this SpecificationAttributeOption entity)
        {
            return MapperFactory.GetMapper<SpecificationAttributeOption, SpecificationAttributeOptionModel>().Map(entity);
        }

        public static SpecificationAttributeOption ToEntity(this SpecificationAttributeOptionModel model)
        {
            return MapperFactory.GetMapper<SpecificationAttributeOptionModel, SpecificationAttributeOption>().Map(model);
        }

        public static SpecificationAttributeOption ToEntity(this SpecificationAttributeOptionModel model, SpecificationAttributeOption entity)
        {
            MapperFactory.GetMapper<SpecificationAttributeOptionModel, SpecificationAttributeOption>().Map(model, entity);
            return entity;
        }

        #endregion

        #region Checkout attributes

        public static CheckoutAttributeModel ToModel(this CheckoutAttribute entity)
        {
            return MapperFactory.GetMapper<CheckoutAttribute, CheckoutAttributeModel>().Map(entity);
        }

        public static CheckoutAttribute ToEntity(this CheckoutAttributeModel model)
        {
            return MapperFactory.GetMapper<CheckoutAttributeModel, CheckoutAttribute>().Map(model);
        }

        public static CheckoutAttribute ToEntity(this CheckoutAttributeModel model, CheckoutAttribute entity)
        {
            MapperFactory.GetMapper<CheckoutAttributeModel, CheckoutAttribute>().Map(model, entity);
            return entity;
        }

        public static CheckoutAttributeValueModel ToModel(this CheckoutAttributeValue entity)
        {
            return MapperFactory.GetMapper<CheckoutAttributeValue, CheckoutAttributeValueModel>().Map(entity);
        }

        public static CheckoutAttributeValue ToEntity(this CheckoutAttributeValueModel model)
        {
            return MapperFactory.GetMapper<CheckoutAttributeValueModel, CheckoutAttributeValue>().Map(model);
        }

        public static CheckoutAttributeValue ToEntity(this CheckoutAttributeValueModel model, CheckoutAttributeValue entity)
        {
            MapperFactory.GetMapper<CheckoutAttributeValueModel, CheckoutAttributeValue>().Map(model, entity);
            return entity;
        }
        
        #endregion

		#region Product bundle items

		public static ProductBundleItemModel ToModel(this ProductBundleItem entity)
		{
            return MapperFactory.GetMapper<ProductBundleItem, ProductBundleItemModel>().Map(entity);
		}

		public static ProductBundleItem ToEntity(this ProductBundleItemModel model)
		{
            return MapperFactory.GetMapper<ProductBundleItemModel, ProductBundleItem>().Map(model);
		}

		public static ProductBundleItem ToEntity(this ProductBundleItemModel model, ProductBundleItem entity)
		{
            MapperFactory.GetMapper<ProductBundleItemModel, ProductBundleItem>().Map(model, entity);
            return entity;
		}

		#endregion

		#region Languages

		public static LanguageModel ToModel(this Language entity)
        {
            return MapperFactory.GetMapper<Language, LanguageModel>().Map(entity);
        }

        public static Language ToEntity(this LanguageModel model)
        {
            return MapperFactory.GetMapper<LanguageModel, Language>().Map(model);
        }

        public static Language ToEntity(this LanguageModel model, Language entity)
        {
            MapperFactory.GetMapper<LanguageModel, Language>().Map(model, entity);
            return entity;
        }
        
        #endregion

        #region Email account

        public static EmailAccountModel ToModel(this EmailAccount entity)
        {
            return MapperFactory.GetMapper<EmailAccount, EmailAccountModel>().Map(entity);
        }

        public static EmailAccount ToEntity(this EmailAccountModel model)
        {
            return MapperFactory.GetMapper<EmailAccountModel, EmailAccount>().Map(model);
        }

        public static EmailAccount ToEntity(this EmailAccountModel model, EmailAccount entity)
        {
            MapperFactory.GetMapper<EmailAccountModel, EmailAccount>().Map(model, entity);
            return entity;
        }

        #endregion

        #region Message templates

        public static MessageTemplateModel ToModel(this MessageTemplate entity)
        {
            return MapperFactory.GetMapper<MessageTemplate, MessageTemplateModel>().Map(entity);
        }

        public static MessageTemplate ToEntity(this MessageTemplateModel model)
        {
            return MapperFactory.GetMapper<MessageTemplateModel, MessageTemplate>().Map(model);
        }

        public static MessageTemplate ToEntity(this MessageTemplateModel model, MessageTemplate entity)
        {
            MapperFactory.GetMapper<MessageTemplateModel, MessageTemplate>().Map(model, entity);
            return entity;
        }

        #endregion

        #region Queued email

        public static QueuedEmailModel ToModel(this QueuedEmail entity)
        {
            return MapperFactory.GetMapper<QueuedEmail, QueuedEmailModel>().Map(entity);
        }

        public static QueuedEmail ToEntity(this QueuedEmailModel model)
        {
            return MapperFactory.GetMapper<QueuedEmailModel, QueuedEmail>().Map(model);
        }

        public static QueuedEmail ToEntity(this QueuedEmailModel model, QueuedEmail entity)
        {
            MapperFactory.GetMapper<QueuedEmailModel, QueuedEmail>().Map(model, entity);
            return entity;
        }

        #endregion

        #region Campaigns

        public static CampaignModel ToModel(this Campaign entity)
        {
            return MapperFactory.GetMapper<Campaign, CampaignModel>().Map(entity);
        }

        public static Campaign ToEntity(this CampaignModel model)
        {
            return MapperFactory.GetMapper<CampaignModel, Campaign>().Map(model);
        }

        public static Campaign ToEntity(this CampaignModel model, Campaign entity)
        {
            MapperFactory.GetMapper<CampaignModel, Campaign>().Map(model, entity);
            return entity;
        }

        #endregion

        #region Topics

        public static TopicModel ToModel(this Topic entity)
        {
            return MapperFactory.GetMapper<Topic, TopicModel>().Map(entity);
        }

        public static Topic ToEntity(this TopicModel model)
        {
            return MapperFactory.GetMapper<TopicModel, Topic>().Map(model);
        }

        public static Topic ToEntity(this TopicModel model, Topic entity)
        {
            MapperFactory.GetMapper<TopicModel, Topic>().Map(model, entity);
            return entity;
        }

        #endregion

        #region Log

        public static LogModel ToModel(this Log entity)
        {
            return MapperFactory.GetMapper<Log, LogModel>().Map(entity);
        }

        public static Log ToEntity(this LogModel model)
        {
            return MapperFactory.GetMapper<LogModel, Log>().Map(model);
        }

        public static Log ToEntity(this LogModel model, Log entity)
        {
            MapperFactory.GetMapper<LogModel, Log>().Map(model, entity);
            return entity;
        }

        public static ActivityLogTypeModel ToModel(this ActivityLogType entity)
        {
            return MapperFactory.GetMapper<ActivityLogType, ActivityLogTypeModel>().Map(entity);
        }

        public static ActivityLogModel ToModel(this ActivityLog entity)
        {
            return MapperFactory.GetMapper<ActivityLog, ActivityLogModel>().Map(entity);
        }

        #endregion
        
        #region Currencies

        public static CurrencyModel ToModel(this Currency entity)
        {
            return MapperFactory.GetMapper<Currency, CurrencyModel>().Map(entity);
        }

        public static Currency ToEntity(this CurrencyModel model)
        {
            return MapperFactory.GetMapper<CurrencyModel, Currency>().Map(model);
        }

        public static Currency ToEntity(this CurrencyModel model, Currency entity)
        {
            MapperFactory.GetMapper<CurrencyModel, Currency>().Map(model, entity);
            return entity;
        }
        
        #endregion

        #region Delivery Times

        public static DeliveryTimeModel ToModel(this DeliveryTime entity)
        {
            return MapperFactory.GetMapper<DeliveryTime, DeliveryTimeModel>().Map(entity);
        }

        public static DeliveryTime ToEntity(this DeliveryTimeModel model)
        {
            return MapperFactory.GetMapper<DeliveryTimeModel, DeliveryTime>().Map(model);
        }

        public static DeliveryTime ToEntity(this DeliveryTimeModel model, DeliveryTime entity)
        {
            MapperFactory.GetMapper<DeliveryTimeModel, DeliveryTime>().Map(model, entity);
            return entity;
        }

        #endregion

        #region Attribute combinations

        public static ProductVariantAttributeCombinationModel ToModel(this ProductVariantAttributeCombination entity)
        {
            return MapperFactory.GetMapper<ProductVariantAttributeCombination, ProductVariantAttributeCombinationModel>().Map(entity);
        }

        public static ProductVariantAttributeCombination ToEntity(this ProductVariantAttributeCombinationModel model)
        {
            return MapperFactory.GetMapper<ProductVariantAttributeCombinationModel, ProductVariantAttributeCombination>().Map(model);
        }

        public static ProductVariantAttributeCombination ToEntity(this ProductVariantAttributeCombinationModel model, ProductVariantAttributeCombination entity)
        {
            MapperFactory.GetMapper<ProductVariantAttributeCombinationModel, ProductVariantAttributeCombination>().Map(model, entity);
            return entity;
        }

        #endregion

        #region Measure weights

        public static MeasureWeightModel ToModel(this MeasureWeight entity)
        {
            return MapperFactory.GetMapper<MeasureWeight, MeasureWeightModel>().Map(entity);
        }

        public static MeasureWeight ToEntity(this MeasureWeightModel model)
        {
            return MapperFactory.GetMapper<MeasureWeightModel, MeasureWeight>().Map(model);
        }

        public static MeasureWeight ToEntity(this MeasureWeightModel model, MeasureWeight entity)
        {
            MapperFactory.GetMapper<MeasureWeightModel, MeasureWeight>().Map(model, entity);
            return entity;
        }

        #endregion

        #region Measure dimension

        public static MeasureDimensionModel ToModel(this MeasureDimension entity)
        {
            return MapperFactory.GetMapper<MeasureDimension, MeasureDimensionModel>().Map(entity);
        }

        public static MeasureDimension ToEntity(this MeasureDimensionModel model)
        {
            return MapperFactory.GetMapper<MeasureDimensionModel, MeasureDimension>().Map(model);
        }

        public static MeasureDimension ToEntity(this MeasureDimensionModel model, MeasureDimension entity)
        {
            MapperFactory.GetMapper<MeasureDimensionModel, MeasureDimension>().Map(model, entity);
            return entity;
        }

        #endregion

        #region Quantity units

        public static QuantityUnitModel ToModel(this QuantityUnit entity)
        {
            return MapperFactory.GetMapper<QuantityUnit, QuantityUnitModel>().Map(entity);
        }

        public static QuantityUnit ToEntity(this QuantityUnitModel model)
        {
            return MapperFactory.GetMapper<QuantityUnitModel, QuantityUnit>().Map(model);
        }

        public static QuantityUnit ToEntity(this QuantityUnitModel model, QuantityUnit entity)
        {
            MapperFactory.GetMapper<QuantityUnitModel, QuantityUnit>().Map(model, entity);
            return entity;
        }

        #endregion

        #region Tax providers

        public static TaxProviderModel ToModel(this Provider<ITaxProvider> entity)
        {
			return Mapper.Map<Provider<ITaxProvider>, TaxProviderModel>(entity);
        }

        #endregion

        #region Tax categories

        public static TaxCategoryModel ToModel(this TaxCategory entity)
        {
            return MapperFactory.GetMapper<TaxCategory, TaxCategoryModel>().Map(entity);
        }

        public static TaxCategory ToEntity(this TaxCategoryModel model)
        {
            return MapperFactory.GetMapper<TaxCategoryModel, TaxCategory>().Map(model);
        }

        public static TaxCategory ToEntity(this TaxCategoryModel model, TaxCategory entity)
        {
            MapperFactory.GetMapper<TaxCategoryModel, TaxCategory>().Map(model, entity);
            return entity;
        }

        #endregion
        
        #region Shipping rate computation method

        public static ShippingRateComputationMethodModel ToModel(this IShippingRateComputationMethod entity)
        {
            return Mapper.Map<IShippingRateComputationMethod, ShippingRateComputationMethodModel>(entity);
        }

        #endregion

        #region Shipping methods

        public static ShippingMethodModel ToModel(this ShippingMethod entity)
        {
            return MapperFactory.GetMapper<ShippingMethod, ShippingMethodModel>().Map(entity);
        }

        public static ShippingMethod ToEntity(this ShippingMethodModel model)
        {
            return MapperFactory.GetMapper<ShippingMethodModel, ShippingMethod>().Map(model);
        }

        public static ShippingMethod ToEntity(this ShippingMethodModel model, ShippingMethod entity)
        {
            MapperFactory.GetMapper<ShippingMethodModel, ShippingMethod>().Map(model, entity);
            return entity;
        }

        #endregion

        #region Address

        public static AddressModel ToModel(this Address entity)
        {
            return ToModel(entity, null);
        }

		public static AddressModel ToModel(this Address entity, IAddressService addressService)
		{
            var model = MapperFactory.GetMapper<Address, AddressModel>().Map(entity);

            if (addressService != null)
            {
                model.FormattedAddress = addressService.FormatAddress(entity, true);
            }

			return model;
		}

		public static Address ToEntity(this AddressModel model)
        {
            return MapperFactory.GetMapper<AddressModel, Address>().Map(model);
        }

        public static Address ToEntity(this AddressModel model, Address entity)
        {
            MapperFactory.GetMapper<AddressModel, Address>().Map(model, entity);
            return entity;
        }

        #endregion

        #region NewsLetter subscriptions

        public static NewsLetterSubscriptionModel ToModel(this NewsLetterSubscription entity)
        {
            return MapperFactory.GetMapper<NewsLetterSubscription, NewsLetterSubscriptionModel>().Map(entity);
        }

        public static NewsLetterSubscription ToEntity(this NewsLetterSubscriptionModel model)
        {
            return MapperFactory.GetMapper<NewsLetterSubscriptionModel, NewsLetterSubscription>().Map(model);
        }

        public static NewsLetterSubscription ToEntity(this NewsLetterSubscriptionModel model, NewsLetterSubscription entity)
        {
            MapperFactory.GetMapper<NewsLetterSubscriptionModel, NewsLetterSubscription>().Map(model, entity);
            return entity;
        }

        #endregion

        #region Discounts

        public static DiscountModel ToModel(this Discount entity)
        {
            return MapperFactory.GetMapper<Discount, DiscountModel>().Map(entity);
        }

        public static Discount ToEntity(this DiscountModel model)
        {
            return MapperFactory.GetMapper<DiscountModel, Discount>().Map(model);
        }

        public static Discount ToEntity(this DiscountModel model, Discount entity)
        {
            MapperFactory.GetMapper<DiscountModel, Discount>().Map(model, entity);
            return entity;
        }

        #endregion

        #region Forums

        public static ForumGroupModel ToModel(this ForumGroup entity)
        {
            return MapperFactory.GetMapper<ForumGroup, ForumGroupModel>().Map(entity);
        }

        public static ForumGroup ToEntity(this ForumGroupModel model)
        {
            return MapperFactory.GetMapper<ForumGroupModel, ForumGroup>().Map(model);
        }

        public static ForumGroup ToEntity(this ForumGroupModel model, ForumGroup entity)
        {
            MapperFactory.GetMapper<ForumGroupModel, ForumGroup>().Map(model, entity);
            return entity;
        }

        public static ForumModel ToModel(this Forum entity)
        {
            return MapperFactory.GetMapper<Forum, ForumModel>().Map(entity);
        }

        public static Forum ToEntity(this ForumModel model)
        {
            return MapperFactory.GetMapper<ForumModel, Forum>().Map(model);
        }

        public static Forum ToEntity(this ForumModel model, Forum entity)
        {
            MapperFactory.GetMapper<ForumModel, Forum>().Map(model, entity);
            return entity;
        }
        
        #endregion

        #region Blog

        public static BlogPostModel ToModel(this BlogPost entity)
        {
            return MapperFactory.GetMapper<BlogPost, BlogPostModel>().Map(entity);
        }

        public static BlogPost ToEntity(this BlogPostModel model)
        {
            return MapperFactory.GetMapper<BlogPostModel, BlogPost>().Map(model);
        }

        public static BlogPost ToEntity(this BlogPostModel model, BlogPost entity)
        {
            MapperFactory.GetMapper<BlogPostModel, BlogPost>().Map(model, entity);
            return entity;
        }

        #endregion

        #region News

        public static NewsItemModel ToModel(this NewsItem entity)
        {
            return MapperFactory.GetMapper<NewsItem, NewsItemModel>().Map(entity);
        }

        public static NewsItem ToEntity(this NewsItemModel model)
        {
            return MapperFactory.GetMapper<NewsItemModel, NewsItem>().Map(model);
        }

        public static NewsItem ToEntity(this NewsItemModel model, NewsItem entity)
        {
            MapperFactory.GetMapper<NewsItemModel, NewsItem>().Map(model, entity);
            return entity;
        }

        #endregion

        #region Polls

        public static PollModel ToModel(this Poll entity)
        {
            return MapperFactory.GetMapper<Poll, PollModel>().Map(entity);
        }

        public static Poll ToEntity(this PollModel model)
        {
            return MapperFactory.GetMapper<PollModel, Poll>().Map(model);
        }

        public static Poll ToEntity(this PollModel model, Poll entity)
        {
            MapperFactory.GetMapper<PollModel, Poll>().Map(model, entity);
            return entity;
        }

        #endregion

        #region Customers/users/customer roles

        public static CustomerRoleModel ToModel(this CustomerRole entity)
        {
            return MapperFactory.GetMapper<CustomerRole, CustomerRoleModel>().Map(entity);
        }

        public static CustomerRole ToEntity(this CustomerRoleModel model)
        {
            return MapperFactory.GetMapper<CustomerRoleModel, CustomerRole>().Map(model);
        }

        public static CustomerRole ToEntity(this CustomerRoleModel model, CustomerRole entity)
        {
            MapperFactory.GetMapper<CustomerRoleModel, CustomerRole>().Map(model, entity);
            return entity;
        }

        #endregion

        #region Gift Cards

        public static GiftCardModel ToModel(this GiftCard entity)
        {
            return MapperFactory.GetMapper<GiftCard, GiftCardModel>().Map(entity);
        }

        public static GiftCard ToEntity(this GiftCardModel model)
        {
            return MapperFactory.GetMapper<GiftCardModel, GiftCard>().Map(model);
        }

        public static GiftCard ToEntity(this GiftCardModel model, GiftCard entity)
        {
            MapperFactory.GetMapper<GiftCardModel, GiftCard>().Map(model, entity);
            return entity;
        }

        #endregion

        #region Countries / states

        public static CountryModel ToModel(this Country entity)
        {
            return MapperFactory.GetMapper<Country, CountryModel>().Map(entity);
        }

        public static Country ToEntity(this CountryModel model)
        {
            return MapperFactory.GetMapper<CountryModel, Country>().Map(model);
        }

        public static Country ToEntity(this CountryModel model, Country entity)
        {
            MapperFactory.GetMapper<CountryModel, Country>().Map(model, entity);
            return entity;
        }

        public static StateProvinceModel ToModel(this StateProvince entity)
        {
            return MapperFactory.GetMapper<StateProvince, StateProvinceModel>().Map(entity);
        }

        public static StateProvince ToEntity(this StateProvinceModel model)
        {
            return MapperFactory.GetMapper<StateProvinceModel, StateProvince>().Map(model);
        }

        public static StateProvince ToEntity(this StateProvinceModel model, StateProvince entity)
        {
            MapperFactory.GetMapper<StateProvinceModel, StateProvince>().Map(model, entity);
            return entity;
        }

        #endregion

        #region Settings

        public static ThemeListModel ToModel(this ThemeSettings entity)
        {
            return Mapper.Map<ThemeSettings, ThemeListModel>(entity);
        }
        public static ThemeSettings ToEntity(this ThemeListModel model)
        {
            return Mapper.Map<ThemeListModel, ThemeSettings>(model);
        }
        public static ThemeSettings ToEntity(this ThemeListModel model, ThemeSettings destination)
        {
            return Mapper.Map(model, destination);
        }

        public static TaxSettingsModel ToModel(this TaxSettings entity)
        {
            return Mapper.Map<TaxSettings, TaxSettingsModel>(entity);
        }
        public static TaxSettings ToEntity(this TaxSettingsModel model)
        {
            return Mapper.Map<TaxSettingsModel, TaxSettings>(model);
        }
        public static TaxSettings ToEntity(this TaxSettingsModel model, TaxSettings destination)
        {
            return Mapper.Map(model, destination);
        }


        public static ShippingSettingsModel ToModel(this ShippingSettings entity)
        {
            return Mapper.Map<ShippingSettings, ShippingSettingsModel>(entity);
        }
        public static ShippingSettings ToEntity(this ShippingSettingsModel model)
        {
            return Mapper.Map<ShippingSettingsModel, ShippingSettings>(model);
        }
        public static ShippingSettings ToEntity(this ShippingSettingsModel model, ShippingSettings destination)
        {
            return Mapper.Map(model, destination);
        }


        public static ForumSettingsModel ToModel(this ForumSettings entity)
        {
            return Mapper.Map<ForumSettings, ForumSettingsModel>(entity);
        }
        public static ForumSettings ToEntity(this ForumSettingsModel model)
        {
            return Mapper.Map<ForumSettingsModel, ForumSettings>(model);
        }
        public static ForumSettings ToEntity(this ForumSettingsModel model, ForumSettings destination)
        {
            return Mapper.Map(model, destination);
        }


        public static BlogSettingsModel ToModel(this BlogSettings entity)
        {
            return Mapper.Map<BlogSettings, BlogSettingsModel>(entity);
        }
        public static BlogSettings ToEntity(this BlogSettingsModel model)
        {
            return Mapper.Map<BlogSettingsModel, BlogSettings>(model);
        }
        public static BlogSettings ToEntity(this BlogSettingsModel model, BlogSettings destination)
        {
            return Mapper.Map(model, destination);
        }


        public static NewsSettingsModel ToModel(this NewsSettings entity)
        {
            return Mapper.Map<NewsSettings, NewsSettingsModel>(entity);
        }
        public static NewsSettings ToEntity(this NewsSettingsModel model)
        {
            return Mapper.Map<NewsSettingsModel, NewsSettings>(model);
        }
        public static NewsSettings ToEntity(this NewsSettingsModel model, NewsSettings destination)
        {
            return Mapper.Map(model, destination);
        }


        public static CatalogSettingsModel ToModel(this CatalogSettings entity)
        {
            return Mapper.Map<CatalogSettings, CatalogSettingsModel>(entity);
        }
        public static CatalogSettings ToEntity(this CatalogSettingsModel model)
        {
            return Mapper.Map<CatalogSettingsModel, CatalogSettings>(model);
        }
        public static CatalogSettings ToEntity(this CatalogSettingsModel model, CatalogSettings destination)
        {
            return Mapper.Map(model, destination);
        }


        public static RewardPointsSettingsModel ToModel(this RewardPointsSettings entity)
        {
            return Mapper.Map<RewardPointsSettings, RewardPointsSettingsModel>(entity);
        }
        public static RewardPointsSettings ToEntity(this RewardPointsSettingsModel model)
        {
            return Mapper.Map<RewardPointsSettingsModel, RewardPointsSettings>(model);
        }
        public static RewardPointsSettings ToEntity(this RewardPointsSettingsModel model, RewardPointsSettings destination)
        {
            return Mapper.Map(model, destination);
        }


        public static OrderSettingsModel ToModel(this OrderSettings entity)
        {
            return Mapper.Map<OrderSettings, OrderSettingsModel>(entity);
        }
        public static OrderSettings ToEntity(this OrderSettingsModel model)
        {
            return Mapper.Map<OrderSettingsModel, OrderSettings>(model);
        }
        public static OrderSettings ToEntity(this OrderSettingsModel model, OrderSettings destination)
        {
            return Mapper.Map(model, destination);
        }


        public static ShoppingCartSettingsModel ToModel(this ShoppingCartSettings entity)
        {
            return Mapper.Map<ShoppingCartSettings, ShoppingCartSettingsModel>(entity);
        }
        public static ShoppingCartSettings ToEntity(this ShoppingCartSettingsModel model)
        {
            return Mapper.Map<ShoppingCartSettingsModel, ShoppingCartSettings>(model);
        }
        public static ShoppingCartSettings ToEntity(this ShoppingCartSettingsModel model, ShoppingCartSettings destination)
        {
            return Mapper.Map(model, destination);
        }


		public static MediaSettingsModel ToModel(this MediaSettings entity)
        {
            return Mapper.Map<MediaSettings, MediaSettingsModel>(entity);
        }
        public static MediaSettings ToEntity(this MediaSettingsModel model)
        {
            return Mapper.Map<MediaSettingsModel, MediaSettings>(model);
        }
        public static MediaSettings ToEntity(this MediaSettingsModel model, MediaSettings destination)
        {
            return Mapper.Map(model, destination);
        }

        public static CustomerUserSettingsModel.CustomerSettingsModel ToModel(this CustomerSettings entity)
        {
            return Mapper.Map<CustomerSettings, CustomerUserSettingsModel.CustomerSettingsModel>(entity);
        }
        public static CustomerSettings ToEntity(this CustomerUserSettingsModel.CustomerSettingsModel model)
        {
            return Mapper.Map<CustomerUserSettingsModel.CustomerSettingsModel, CustomerSettings>(model);
        }
        public static CustomerSettings ToEntity(this CustomerUserSettingsModel.CustomerSettingsModel model, CustomerSettings destination)
        {
            return Mapper.Map(model, destination);
        }
        public static CustomerUserSettingsModel.AddressSettingsModel ToModel(this AddressSettings entity)
        {
            return Mapper.Map<AddressSettings, CustomerUserSettingsModel.AddressSettingsModel>(entity);
        }
        public static AddressSettings ToEntity(this CustomerUserSettingsModel.AddressSettingsModel model)
        {
            return Mapper.Map<CustomerUserSettingsModel.AddressSettingsModel, AddressSettings>(model);
        }
        public static AddressSettings ToEntity(this CustomerUserSettingsModel.AddressSettingsModel model, AddressSettings destination)
        {
            return Mapper.Map(model, destination);
        }
		public static CustomerUserSettingsModel.PrivacySettingsModel ToModel(this PrivacySettings entity)
		{
			return Mapper.Map<PrivacySettings, CustomerUserSettingsModel.PrivacySettingsModel>(entity);
		}
		public static PrivacySettings ToEntity(this CustomerUserSettingsModel.PrivacySettingsModel model)
		{
			return Mapper.Map<CustomerUserSettingsModel.PrivacySettingsModel, PrivacySettings>(model);
		}
		public static PrivacySettings ToEntity(this CustomerUserSettingsModel.PrivacySettingsModel model, PrivacySettings destination)
		{
			return Mapper.Map(model, destination);
		}

		#endregion


		#region Plugins

		public static PluginModel ToModel(this PluginDescriptor entity)
        {
            return MapperFactory.GetMapper<PluginDescriptor, PluginModel>().Map(entity);
        }

        #endregion


		#region Stores

		public static StoreModel ToModel(this Store entity)
		{
            return MapperFactory.GetMapper<Store, StoreModel>().Map(entity);
		}

		public static Store ToEntity(this StoreModel model)
		{
            return MapperFactory.GetMapper<StoreModel, Store>().Map(model);
		}

		public static Store ToEntity(this StoreModel model, Store entity)
		{
            MapperFactory.GetMapper<StoreModel, Store>().Map(model, entity);
            return entity;
		}

		#endregion
    }
}
