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
            return MapperFactory.Map<Category, CategoryModel>(entity);
        }

        public static Category ToEntity(this CategoryModel model)
        {
            return MapperFactory.Map<CategoryModel, Category>(model);
        }

        public static Category ToEntity(this CategoryModel model, Category entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }

        #endregion

        #region Manufacturer

        public static ManufacturerModel ToModel(this Manufacturer entity)
        {
            return MapperFactory.Map<Manufacturer, ManufacturerModel>(entity);
        }

        public static Manufacturer ToEntity(this ManufacturerModel model)
        {
            return MapperFactory.Map<ManufacturerModel, Manufacturer>(model);
        }

        public static Manufacturer ToEntity(this ManufacturerModel model, Manufacturer entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }

        #endregion

        #region Products

        public static ProductModel ToModel(this Product entity)
        {
            return MapperFactory.Map<Product, ProductModel>(entity);
        }

        public static Product ToEntity(this ProductModel model)
        {
            return MapperFactory.Map<ProductModel, Product>(model);
        }

        public static Product ToEntity(this ProductModel model, Product entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }

        #endregion

        #region Product attributes

        public static ProductAttributeModel ToModel(this ProductAttribute entity)
        {
            return MapperFactory.Map<ProductAttribute, ProductAttributeModel>(entity);
        }

        public static ProductAttribute ToEntity(this ProductAttributeModel model)
        {
            return MapperFactory.Map<ProductAttributeModel, ProductAttribute>(model);
        }

        public static ProductAttribute ToEntity(this ProductAttributeModel model, ProductAttribute entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }

        public static ProductAttributeOptionModel ToModel(this ProductAttributeOption entity)
        {
            return MapperFactory.Map<ProductAttributeOption, ProductAttributeOptionModel>(entity);
        }

        public static ProductAttributeOption ToEntity(this ProductAttributeOptionModel model)
        {
            return MapperFactory.Map<ProductAttributeOptionModel, ProductAttributeOption>(model);
        }

        public static ProductAttributeOption ToEntity(this ProductAttributeOptionModel model, ProductAttributeOption entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }

        #endregion

        #region Specification attributes

        public static SpecificationAttributeModel ToModel(this SpecificationAttribute entity)
        {
            return MapperFactory.Map<SpecificationAttribute, SpecificationAttributeModel>(entity);
        }

        public static SpecificationAttribute ToEntity(this SpecificationAttributeModel model)
        {
            return MapperFactory.Map<SpecificationAttributeModel, SpecificationAttribute>(model);
        }

        public static SpecificationAttribute ToEntity(this SpecificationAttributeModel model, SpecificationAttribute entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }

        public static SpecificationAttributeOptionModel ToModel(this SpecificationAttributeOption entity)
        {
            return MapperFactory.Map<SpecificationAttributeOption, SpecificationAttributeOptionModel>(entity);
        }

        public static SpecificationAttributeOption ToEntity(this SpecificationAttributeOptionModel model)
        {
            return MapperFactory.Map<SpecificationAttributeOptionModel, SpecificationAttributeOption>(model);
        }

        public static SpecificationAttributeOption ToEntity(this SpecificationAttributeOptionModel model, SpecificationAttributeOption entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }

        #endregion

        #region Checkout attributes

        public static CheckoutAttributeModel ToModel(this CheckoutAttribute entity)
        {
            return MapperFactory.Map<CheckoutAttribute, CheckoutAttributeModel>(entity);
        }

        public static CheckoutAttribute ToEntity(this CheckoutAttributeModel model)
        {
            return MapperFactory.Map<CheckoutAttributeModel, CheckoutAttribute>(model);
        }

        public static CheckoutAttribute ToEntity(this CheckoutAttributeModel model, CheckoutAttribute entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }

        public static CheckoutAttributeValueModel ToModel(this CheckoutAttributeValue entity)
        {
            return MapperFactory.Map<CheckoutAttributeValue, CheckoutAttributeValueModel>(entity);
        }

        public static CheckoutAttributeValue ToEntity(this CheckoutAttributeValueModel model)
        {
            return MapperFactory.Map<CheckoutAttributeValueModel, CheckoutAttributeValue>(model);
        }

        public static CheckoutAttributeValue ToEntity(this CheckoutAttributeValueModel model, CheckoutAttributeValue entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }

        #endregion

        #region Product bundle items

        public static ProductBundleItemModel ToModel(this ProductBundleItem entity)
        {
            return MapperFactory.Map<ProductBundleItem, ProductBundleItemModel>(entity);
        }

        public static ProductBundleItem ToEntity(this ProductBundleItemModel model)
        {
            return MapperFactory.Map<ProductBundleItemModel, ProductBundleItem>(model);
        }

        public static ProductBundleItem ToEntity(this ProductBundleItemModel model, ProductBundleItem entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }

        #endregion

        #region Languages

        public static LanguageModel ToModel(this Language entity)
        {
            return MapperFactory.Map<Language, LanguageModel>(entity);
        }

        public static Language ToEntity(this LanguageModel model)
        {
            return MapperFactory.Map<LanguageModel, Language>(model);
        }

        public static Language ToEntity(this LanguageModel model, Language entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }

        #endregion

        #region Email account

        public static EmailAccountModel ToModel(this EmailAccount entity)
        {
            return MapperFactory.Map<EmailAccount, EmailAccountModel>(entity);
        }

        public static EmailAccount ToEntity(this EmailAccountModel model)
        {
            return MapperFactory.Map<EmailAccountModel, EmailAccount>(model);
        }

        public static EmailAccount ToEntity(this EmailAccountModel model, EmailAccount entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }

        #endregion

        #region Message templates

        public static MessageTemplateModel ToModel(this MessageTemplate entity)
        {
            return MapperFactory.Map<MessageTemplate, MessageTemplateModel>(entity);
        }

        public static MessageTemplate ToEntity(this MessageTemplateModel model)
        {
            return MapperFactory.Map<MessageTemplateModel, MessageTemplate>(model);
        }

        public static MessageTemplate ToEntity(this MessageTemplateModel model, MessageTemplate entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }

        #endregion

        #region Queued email

        public static QueuedEmailModel ToModel(this QueuedEmail entity)
        {
            return MapperFactory.Map<QueuedEmail, QueuedEmailModel>(entity);
        }

        public static QueuedEmail ToEntity(this QueuedEmailModel model)
        {
            return MapperFactory.Map<QueuedEmailModel, QueuedEmail>(model);
        }

        public static QueuedEmail ToEntity(this QueuedEmailModel model, QueuedEmail entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }

        #endregion

        #region Campaigns

        public static CampaignModel ToModel(this Campaign entity)
        {
            return MapperFactory.Map<Campaign, CampaignModel>(entity);
        }

        public static Campaign ToEntity(this CampaignModel model)
        {
            return MapperFactory.Map<CampaignModel, Campaign>(model);
        }

        public static Campaign ToEntity(this CampaignModel model, Campaign entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }

        #endregion

        #region Topics

        public static TopicModel ToModel(this Topic entity)
        {
            return MapperFactory.Map<Topic, TopicModel>(entity);
        }

        public static Topic ToEntity(this TopicModel model)
        {
            return MapperFactory.Map<TopicModel, Topic>(model);
        }

        public static Topic ToEntity(this TopicModel model, Topic entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }

        #endregion

        #region Log

        public static LogModel ToModel(this Log entity)
        {
            return MapperFactory.Map<Log, LogModel>(entity);
        }

        public static Log ToEntity(this LogModel model)
        {
            return MapperFactory.Map<LogModel, Log>(model);
        }

        public static Log ToEntity(this LogModel model, Log entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }

        public static ActivityLogTypeModel ToModel(this ActivityLogType entity)
        {
            return MapperFactory.Map<ActivityLogType, ActivityLogTypeModel>(entity);
        }

        public static ActivityLogModel ToModel(this ActivityLog entity)
        {
            return MapperFactory.Map<ActivityLog, ActivityLogModel>(entity);
        }

        #endregion

        #region Currencies

        public static CurrencyModel ToModel(this Currency entity)
        {
            return MapperFactory.Map<Currency, CurrencyModel>(entity);
        }

        public static Currency ToEntity(this CurrencyModel model)
        {
            return MapperFactory.Map<CurrencyModel, Currency>(model);
        }

        public static Currency ToEntity(this CurrencyModel model, Currency entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }

        #endregion

        #region Delivery Times

        public static DeliveryTimeModel ToModel(this DeliveryTime entity)
        {
            return MapperFactory.Map<DeliveryTime, DeliveryTimeModel>(entity);
        }

        public static DeliveryTime ToEntity(this DeliveryTimeModel model)
        {
            return MapperFactory.Map<DeliveryTimeModel, DeliveryTime>(model);
        }

        public static DeliveryTime ToEntity(this DeliveryTimeModel model, DeliveryTime entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }

        #endregion

        #region Attribute combinations

        public static ProductVariantAttributeCombinationModel ToModel(this ProductVariantAttributeCombination entity)
        {
            return MapperFactory.Map<ProductVariantAttributeCombination, ProductVariantAttributeCombinationModel>(entity);
        }

        public static ProductVariantAttributeCombination ToEntity(this ProductVariantAttributeCombinationModel model)
        {
            return MapperFactory.Map<ProductVariantAttributeCombinationModel, ProductVariantAttributeCombination>(model);
        }

        public static ProductVariantAttributeCombination ToEntity(this ProductVariantAttributeCombinationModel model, ProductVariantAttributeCombination entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }

        #endregion

        #region Measure weights

        public static MeasureWeightModel ToModel(this MeasureWeight entity)
        {
            return MapperFactory.Map<MeasureWeight, MeasureWeightModel>(entity);
        }

        public static MeasureWeight ToEntity(this MeasureWeightModel model)
        {
            return MapperFactory.Map<MeasureWeightModel, MeasureWeight>(model);
        }

        public static MeasureWeight ToEntity(this MeasureWeightModel model, MeasureWeight entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }

        #endregion

        #region Measure dimension

        public static MeasureDimensionModel ToModel(this MeasureDimension entity)
        {
            return MapperFactory.Map<MeasureDimension, MeasureDimensionModel>(entity);
        }

        public static MeasureDimension ToEntity(this MeasureDimensionModel model)
        {
            return MapperFactory.Map<MeasureDimensionModel, MeasureDimension>(model);
        }

        public static MeasureDimension ToEntity(this MeasureDimensionModel model, MeasureDimension entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }

        #endregion

        #region Quantity units

        public static QuantityUnitModel ToModel(this QuantityUnit entity)
        {
            return MapperFactory.Map<QuantityUnit, QuantityUnitModel>(entity);
        }

        public static QuantityUnit ToEntity(this QuantityUnitModel model)
        {
            return MapperFactory.Map<QuantityUnitModel, QuantityUnit>(model);
        }

        public static QuantityUnit ToEntity(this QuantityUnitModel model, QuantityUnit entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }

        #endregion

        #region Tax providers

        public static TaxProviderModel ToModel(this Provider<ITaxProvider> entity)
        {
            return MapperFactory.Map<Provider<ITaxProvider>, TaxProviderModel>(entity);
        }

        #endregion

        #region Tax categories

        public static TaxCategoryModel ToModel(this TaxCategory entity)
        {
            return MapperFactory.Map<TaxCategory, TaxCategoryModel>(entity);
        }

        public static TaxCategory ToEntity(this TaxCategoryModel model)
        {
            return MapperFactory.Map<TaxCategoryModel, TaxCategory>(model);
        }

        public static TaxCategory ToEntity(this TaxCategoryModel model, TaxCategory entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }

        #endregion

        #region Shipping rate computation method

        public static ShippingRateComputationMethodModel ToModel(this IShippingRateComputationMethod entity)
        {
            return MapperFactory.Map<IShippingRateComputationMethod, ShippingRateComputationMethodModel>(entity);
        }

        #endregion

        #region Shipping methods

        public static ShippingMethodModel ToModel(this ShippingMethod entity)
        {
            return MapperFactory.Map<ShippingMethod, ShippingMethodModel>(entity);
        }

        public static ShippingMethod ToEntity(this ShippingMethodModel model)
        {
            return MapperFactory.Map<ShippingMethodModel, ShippingMethod>(model);
        }

        public static ShippingMethod ToEntity(this ShippingMethodModel model, ShippingMethod entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }

        #endregion

        #region Address

        public static AddressModel ToModel(this Address entity)
        {
            return MapperFactory.Map<Address, AddressModel>(entity);
        }

        public static AddressModel ToModel(this Address entity, IAddressService addressService)
        {
            var model = MapperFactory.Map<Address, AddressModel>(entity);

            if (addressService != null)
            {
                model.FormattedAddress = addressService.FormatAddress(entity, true);
            }

            return model;
        }

        public static Address ToEntity(this AddressModel model)
        {
            return MapperFactory.Map<AddressModel, Address>(model);
        }

        public static Address ToEntity(this AddressModel model, Address entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }

        #endregion

        #region NewsLetter subscriptions

        public static NewsLetterSubscriptionModel ToModel(this NewsLetterSubscription entity)
        {
            return MapperFactory.Map<NewsLetterSubscription, NewsLetterSubscriptionModel>(entity);
        }

        public static NewsLetterSubscription ToEntity(this NewsLetterSubscriptionModel model)
        {
            return MapperFactory.Map<NewsLetterSubscriptionModel, NewsLetterSubscription>(model);
        }

        public static NewsLetterSubscription ToEntity(this NewsLetterSubscriptionModel model, NewsLetterSubscription entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }

        #endregion

        #region Discounts

        public static DiscountModel ToModel(this Discount entity)
        {
            return MapperFactory.Map<Discount, DiscountModel>(entity);
        }

        public static Discount ToEntity(this DiscountModel model)
        {
            return MapperFactory.Map<DiscountModel, Discount>(model);
        }

        public static Discount ToEntity(this DiscountModel model, Discount entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }

        #endregion

        #region Forums

        public static ForumGroupModel ToModel(this ForumGroup entity)
        {
            return MapperFactory.Map<ForumGroup, ForumGroupModel>(entity);
        }

        public static ForumGroup ToEntity(this ForumGroupModel model)
        {
            return MapperFactory.Map<ForumGroupModel, ForumGroup>(model);
        }

        public static ForumGroup ToEntity(this ForumGroupModel model, ForumGroup entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }

        public static ForumModel ToModel(this Forum entity)
        {
            return MapperFactory.Map<Forum, ForumModel>(entity);
        }

        public static Forum ToEntity(this ForumModel model)
        {
            return MapperFactory.Map<ForumModel, Forum>(model);
        }

        public static Forum ToEntity(this ForumModel model, Forum entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }

        #endregion

        #region Blog

        public static BlogPostModel ToModel(this BlogPost entity)
        {
            return MapperFactory.Map<BlogPost, BlogPostModel>(entity);
        }

        public static BlogPost ToEntity(this BlogPostModel model)
        {
            return MapperFactory.Map<BlogPostModel, BlogPost>(model);
        }

        public static BlogPost ToEntity(this BlogPostModel model, BlogPost entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }

        #endregion

        #region News

        public static NewsItemModel ToModel(this NewsItem entity)
        {
            return MapperFactory.Map<NewsItem, NewsItemModel>(entity);
        }

        public static NewsItem ToEntity(this NewsItemModel model)
        {
            return MapperFactory.Map<NewsItemModel, NewsItem>(model);
        }

        public static NewsItem ToEntity(this NewsItemModel model, NewsItem entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }

        #endregion

        #region Polls

        public static PollModel ToModel(this Poll entity)
        {
            return MapperFactory.Map<Poll, PollModel>(entity);
        }

        public static Poll ToEntity(this PollModel model)
        {
            return MapperFactory.Map<PollModel, Poll>(model);
        }

        public static Poll ToEntity(this PollModel model, Poll entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }

        #endregion

        #region Customers/users/customer roles

        public static CustomerRoleModel ToModel(this CustomerRole entity)
        {
            return MapperFactory.Map<CustomerRole, CustomerRoleModel>(entity);
        }

        public static CustomerRole ToEntity(this CustomerRoleModel model)
        {
            return MapperFactory.Map<CustomerRoleModel, CustomerRole>(model);
        }

        public static CustomerRole ToEntity(this CustomerRoleModel model, CustomerRole entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }

        #endregion

        #region Gift Cards

        public static GiftCardModel ToModel(this GiftCard entity)
        {
            return MapperFactory.Map<GiftCard, GiftCardModel>(entity);
        }

        public static GiftCard ToEntity(this GiftCardModel model)
        {
            return MapperFactory.Map<GiftCardModel, GiftCard>(model);
        }

        public static GiftCard ToEntity(this GiftCardModel model, GiftCard entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }

        #endregion

        #region Countries / states

        public static CountryModel ToModel(this Country entity)
        {
            return MapperFactory.Map<Country, CountryModel>(entity);
        }

        public static Country ToEntity(this CountryModel model)
        {
            return MapperFactory.Map<CountryModel, Country>(model);
        }

        public static Country ToEntity(this CountryModel model, Country entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }

        public static StateProvinceModel ToModel(this StateProvince entity)
        {
            return MapperFactory.Map<StateProvince, StateProvinceModel>(entity);
        }

        public static StateProvince ToEntity(this StateProvinceModel model)
        {
            return MapperFactory.Map<StateProvinceModel, StateProvince>(model);
        }

        public static StateProvince ToEntity(this StateProvinceModel model, StateProvince entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }

        #endregion

        #region Settings

        public static TaxSettingsModel ToModel(this TaxSettings entity)
        {
            return MapperFactory.Map<TaxSettings, TaxSettingsModel>(entity);
        }
        public static TaxSettings ToEntity(this TaxSettingsModel model)
        {
            return MapperFactory.Map<TaxSettingsModel, TaxSettings>(model);
        }
        public static TaxSettings ToEntity(this TaxSettingsModel model, TaxSettings entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }


        public static ShippingSettingsModel ToModel(this ShippingSettings entity)
        {
            return MapperFactory.Map<ShippingSettings, ShippingSettingsModel>(entity);
        }
        public static ShippingSettings ToEntity(this ShippingSettingsModel model)
        {
            return MapperFactory.Map<ShippingSettingsModel, ShippingSettings>(model);
        }
        public static ShippingSettings ToEntity(this ShippingSettingsModel model, ShippingSettings entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }


        public static ForumSettingsModel ToModel(this ForumSettings entity)
        {
            return MapperFactory.Map<ForumSettings, ForumSettingsModel>(entity);
        }
        public static ForumSettings ToEntity(this ForumSettingsModel model)
        {
            return MapperFactory.Map<ForumSettingsModel, ForumSettings>(model);
        }
        public static ForumSettings ToEntity(this ForumSettingsModel model, ForumSettings entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }


        public static BlogSettingsModel ToModel(this BlogSettings entity)
        {
            return MapperFactory.Map<BlogSettings, BlogSettingsModel>(entity);
        }
        public static BlogSettings ToEntity(this BlogSettingsModel model)
        {
            return MapperFactory.Map<BlogSettingsModel, BlogSettings>(model);
        }
        public static BlogSettings ToEntity(this BlogSettingsModel model, BlogSettings entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }


        public static NewsSettingsModel ToModel(this NewsSettings entity)
        {
            return MapperFactory.Map<NewsSettings, NewsSettingsModel>(entity);
        }
        public static NewsSettings ToEntity(this NewsSettingsModel model)
        {
            return MapperFactory.Map<NewsSettingsModel, NewsSettings>(model);
        }
        public static NewsSettings ToEntity(this NewsSettingsModel model, NewsSettings entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }


        public static CatalogSettingsModel ToModel(this CatalogSettings entity)
        {
            return MapperFactory.Map<CatalogSettings, CatalogSettingsModel>(entity);
        }
        public static CatalogSettings ToEntity(this CatalogSettingsModel model)
        {
            return MapperFactory.Map<CatalogSettingsModel, CatalogSettings>(model);
        }
        public static CatalogSettings ToEntity(this CatalogSettingsModel model, CatalogSettings entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }


        public static RewardPointsSettingsModel ToModel(this RewardPointsSettings entity)
        {
            return MapperFactory.Map<RewardPointsSettings, RewardPointsSettingsModel>(entity);
        }
        public static RewardPointsSettings ToEntity(this RewardPointsSettingsModel model)
        {
            return MapperFactory.Map<RewardPointsSettingsModel, RewardPointsSettings>(model);
        }
        public static RewardPointsSettings ToEntity(this RewardPointsSettingsModel model, RewardPointsSettings entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }


        public static OrderSettingsModel ToModel(this OrderSettings entity)
        {
            return MapperFactory.Map<OrderSettings, OrderSettingsModel>(entity);
        }
        public static OrderSettings ToEntity(this OrderSettingsModel model)
        {
            return MapperFactory.Map<OrderSettingsModel, OrderSettings>(model);
        }
        public static OrderSettings ToEntity(this OrderSettingsModel model, OrderSettings entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }


        public static ShoppingCartSettingsModel ToModel(this ShoppingCartSettings entity)
        {
            return MapperFactory.Map<ShoppingCartSettings, ShoppingCartSettingsModel>(entity);
        }
        public static ShoppingCartSettings ToEntity(this ShoppingCartSettingsModel model)
        {
            return MapperFactory.Map<ShoppingCartSettingsModel, ShoppingCartSettings>(model);
        }
        public static ShoppingCartSettings ToEntity(this ShoppingCartSettingsModel model, ShoppingCartSettings entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }


        public static MediaSettingsModel ToModel(this MediaSettings entity)
        {
            return MapperFactory.Map<MediaSettings, MediaSettingsModel>(entity);
        }
        public static MediaSettings ToEntity(this MediaSettingsModel model)
        {
            return MapperFactory.Map<MediaSettingsModel, MediaSettings>(model);
        }
        public static MediaSettings ToEntity(this MediaSettingsModel model, MediaSettings entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }

        public static CustomerUserSettingsModel.CustomerSettingsModel ToModel(this CustomerSettings entity)
        {
            return MapperFactory.Map<CustomerSettings, CustomerUserSettingsModel.CustomerSettingsModel>(entity);
        }
        public static CustomerSettings ToEntity(this CustomerUserSettingsModel.CustomerSettingsModel model)
        {
            return MapperFactory.Map<CustomerUserSettingsModel.CustomerSettingsModel, CustomerSettings>(model);
        }
        public static CustomerSettings ToEntity(this CustomerUserSettingsModel.CustomerSettingsModel model, CustomerSettings entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }

        public static CustomerUserSettingsModel.AddressSettingsModel ToModel(this AddressSettings entity)
        {
            return MapperFactory.Map<AddressSettings, CustomerUserSettingsModel.AddressSettingsModel>(entity);
        }
        public static AddressSettings ToEntity(this CustomerUserSettingsModel.AddressSettingsModel model)
        {
            return MapperFactory.Map<CustomerUserSettingsModel.AddressSettingsModel, AddressSettings>(model);
        }
        public static AddressSettings ToEntity(this CustomerUserSettingsModel.AddressSettingsModel model, AddressSettings entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }

        public static CustomerUserSettingsModel.PrivacySettingsModel ToModel(this PrivacySettings entity)
        {
            return MapperFactory.Map<PrivacySettings, CustomerUserSettingsModel.PrivacySettingsModel>(entity);
        }
        public static PrivacySettings ToEntity(this CustomerUserSettingsModel.PrivacySettingsModel model)
        {
            return MapperFactory.Map<CustomerUserSettingsModel.PrivacySettingsModel, PrivacySettings>(model);
        }
        public static PrivacySettings ToEntity(this CustomerUserSettingsModel.PrivacySettingsModel model, PrivacySettings entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }

        public static ThemeListModel ToModel(this ThemeSettings entity)
        {
            return MapperFactory.Map<ThemeSettings, ThemeListModel>(entity);
        }
        public static ThemeSettings ToEntity(this ThemeListModel model)
        {
            return MapperFactory.Map<ThemeListModel, ThemeSettings>(model);
        }
        public static ThemeSettings ToEntity(this ThemeListModel model, ThemeSettings entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }

        #endregion


        #region Plugins

        public static PluginModel ToModel(this PluginDescriptor entity)
        {
            return MapperFactory.Map<PluginDescriptor, PluginModel>(entity);
        }

        #endregion


        #region Stores

        public static StoreModel ToModel(this Store entity)
        {
            return MapperFactory.Map<Store, StoreModel>(entity);
        }

        public static Store ToEntity(this StoreModel model)
        {
            return MapperFactory.Map<StoreModel, Store>(model);
        }

        public static Store ToEntity(this StoreModel model, Store entity)
        {
            MapperFactory.Map(model, entity);
            return entity;
        }

        #endregion
    }
}
