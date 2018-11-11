using System.Reflection;
using System.Linq;
using AutoMapper;
using SmartStore.Admin.Models.Blogs;
using SmartStore.Admin.Models.Catalog;
using SmartStore.Admin.Models.Common;
using SmartStore.Admin.Models.ContentSlider;
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
using SmartStore.Core.Domain.Blogs;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Cms;
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
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Plugins;
using SmartStore.Services.Seo;

namespace SmartStore.Admin.Infrastructure
{
    public class AutoMapperStartupTask : IStartupTask
    {
		class OptionalFkConverter : ITypeConverter<int, int?>
		{
			public int? Convert(ResolutionContext context)
			{
				var srcName = context.PropertyMap.SourceMember.Name;

				if (context.PropertyMap.SourceMember.MemberType == MemberTypes.Property && srcName.EndsWith("Id") && !context.SourceType.IsNullable())
				{
					var src = (int)context.SourceValue;
					return src == 0 ? (int?)null : src;
				}

				return (int?)context.SourceValue;
			}
		}

		public void Execute()
        {
            //TODO remove 'CreatedOnUtc' ignore mappings because now presentation layer models have 'CreatedOn' property and core entities have 'CreatedOnUtc' property (distinct names)

			// special mapper, that avoids DbUpdate exceptions in cases where
			// optional (nullable) int FK properties are 0 instead of null 
			// after mapping model > entity.
			Mapper.CreateMap<int, int?>().ConvertUsing(new OptionalFkConverter());

            //address
            Mapper.CreateMap<Address, AddressModel>()
                .ForMember(dest => dest.AddressHtml, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableCountries, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableStates, mo => mo.Ignore())
                .ForMember(dest => dest.FirstNameEnabled, mo => mo.Ignore())
                .ForMember(dest => dest.FirstNameRequired, mo => mo.Ignore())
                .ForMember(dest => dest.LastNameEnabled, mo => mo.Ignore())
                .ForMember(dest => dest.LastNameRequired, mo => mo.Ignore())
                .ForMember(dest => dest.EmailEnabled, mo => mo.Ignore())
                .ForMember(dest => dest.EmailRequired, mo => mo.Ignore())
                .ForMember(dest => dest.ValidateEmailAddress, mo => mo.Ignore())
                .ForMember(dest => dest.CompanyEnabled, mo => mo.Ignore())
                .ForMember(dest => dest.CompanyRequired, mo => mo.Ignore())
                .ForMember(dest => dest.CountryEnabled, mo => mo.Ignore())
                .ForMember(dest => dest.StateProvinceEnabled, mo => mo.Ignore())
                .ForMember(dest => dest.CityEnabled, mo => mo.Ignore())
                .ForMember(dest => dest.CityRequired, mo => mo.Ignore())
                .ForMember(dest => dest.StreetAddressEnabled, mo => mo.Ignore())
                .ForMember(dest => dest.StreetAddressRequired, mo => mo.Ignore())
                .ForMember(dest => dest.StreetAddress2Enabled, mo => mo.Ignore())
                .ForMember(dest => dest.StreetAddress2Required, mo => mo.Ignore())
                .ForMember(dest => dest.ZipPostalCodeEnabled, mo => mo.Ignore())
                .ForMember(dest => dest.ZipPostalCodeRequired, mo => mo.Ignore())
                .ForMember(dest => dest.PhoneEnabled, mo => mo.Ignore())
                .ForMember(dest => dest.PhoneRequired, mo => mo.Ignore())
                .ForMember(dest => dest.FaxEnabled, mo => mo.Ignore())
                .ForMember(dest => dest.FaxRequired, mo => mo.Ignore())
				.ForMember(dest => dest.EmailMatch, mo => mo.Ignore())
                .ForMember(dest => dest.CountryName, mo => mo.MapFrom(src => src.Country != null ? src.Country.Name : null))
                .ForMember(dest => dest.StateProvinceName, mo => mo.MapFrom(src => src.StateProvince != null ? src.StateProvince.Name : null));
            Mapper.CreateMap<AddressModel, Address>()
                .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.Country, mo => mo.Ignore())
                .ForMember(dest => dest.StateProvince, mo => mo.Ignore());

            //countries
            Mapper.CreateMap<CountryModel, Country>()
                .ForMember(dest => dest.StateProvinces, mo => mo.Ignore())
                .ForMember(dest => dest.RestrictedShippingMethods, mo => mo.Ignore());
            Mapper.CreateMap<Country, CountryModel>()
                .ForMember(dest => dest.NumberOfStates, mo => mo.MapFrom(src => src.StateProvinces != null ? src.StateProvinces.Count : 0))
                .ForMember(dest => dest.Locales, mo => mo.Ignore())
				.ForMember(dest => dest.AvailableStores, mo => mo.Ignore())
				.ForMember(dest => dest.SelectedStoreIds, mo => mo.Ignore());
            //state/provinces
            Mapper.CreateMap<StateProvince, StateProvinceModel>()
                .ForMember(dest => dest.DisplayOrder1, mo => mo.MapFrom(src => src.DisplayOrder))
                .ForMember(dest => dest.Locales, mo => mo.Ignore());
            Mapper.CreateMap<StateProvinceModel, StateProvince>()
                .ForMember(dest => dest.DisplayOrder, mo => mo.MapFrom(src => src.DisplayOrder1))
                .ForMember(dest => dest.Country, mo => mo.Ignore());
            //language
            Mapper.CreateMap<Language, LanguageModel>()
                .ForMember(dest => dest.AvailableFlags, mo => mo.Ignore())
				.ForMember(dest => dest.AvailableCultures, mo => mo.Ignore())
				.ForMember(dest => dest.AvailableTwoLetterLanguageCodes, mo => mo.Ignore())
				.ForMember(dest => dest.AvailableStores, mo => mo.Ignore())
				.ForMember(dest => dest.SelectedStoreIds, mo => mo.Ignore())
				.ForMember(dest => dest.FlagFileNames, mo => mo.Ignore());
            Mapper.CreateMap<LanguageModel, Language>()
                .ForMember(dest => dest.LocaleStringResources, mo => mo.Ignore());
			//email account
			Mapper.CreateMap<EmailAccount, EmailAccountModel>()
				.ForMember(dest => dest.IsDefaultEmailAccount, mo => mo.Ignore())
				.ForMember(dest => dest.SendTestEmailTo, mo => mo.Ignore())
				.ForMember(dest => dest.TestEmailShortErrorMessage, mo => mo.Ignore())
				.ForMember(dest => dest.TestEmailFullErrorMessage, mo => mo.Ignore());
			Mapper.CreateMap<EmailAccountModel, EmailAccount>();
            //message template
            Mapper.CreateMap<MessageTemplate, MessageTemplateModel>()
                .ForMember(dest => dest.TokensTree, mo => mo.Ignore())
                .ForMember(dest => dest.Locales, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableEmailAccounts, mo => mo.Ignore())
				.ForMember(dest => dest.AvailableStores, mo => mo.Ignore())
				.ForMember(dest => dest.SelectedStoreIds, mo => mo.Ignore());
            Mapper.CreateMap<MessageTemplateModel, MessageTemplate>();
            //queued email
			Mapper.CreateMap<QueuedEmail, QueuedEmailModel>()
				.ForMember(dest => dest.EmailAccountName, mo => mo.MapFrom(src => src.EmailAccount != null ? src.EmailAccount.FriendlyName : string.Empty))
				.ForMember(dest => dest.CreatedOn, mo => mo.Ignore())
				.ForMember(dest => dest.SentOn, mo => mo.Ignore())
				.ForMember(dest => dest.AttachmentsCount, mo => mo.MapFrom(src => src.Attachments.Count))
				.ForMember(dest => dest.Attachments, mo => mo.MapFrom(src => src.Attachments.Select(x => new QueuedEmailModel.QueuedEmailAttachmentModel { Id = x.Id, Name = x.Name, MimeType = x.MimeType } )));
            Mapper.CreateMap<QueuedEmailModel, QueuedEmail>()
                .ForMember(dest=> dest.CreatedOnUtc, dt=> dt.Ignore())
                .ForMember(dest => dest.SentOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.EmailAccount, mo => mo.Ignore())
                .ForMember(dest => dest.EmailAccountId, mo => mo.Ignore())
				.ForMember(dest => dest.ReplyTo, mo => mo.Ignore())
				.ForMember(dest => dest.ReplyToName, mo => mo.Ignore())
				.ForMember(dest => dest.Attachments, mo => mo.Ignore());
            //campaign
			Mapper.CreateMap<Campaign, CampaignModel>()
				.ForMember(dest => dest.CreatedOn, mo => mo.Ignore())
				.ForMember(dest => dest.AllowedTokens, mo => mo.Ignore())
				.ForMember(dest => dest.TestEmail, mo => mo.Ignore())
				.ForMember(dest => dest.AvailableStores, mo => mo.Ignore())
				.ForMember(dest => dest.SelectedStoreIds, mo => mo.Ignore());
            Mapper.CreateMap<CampaignModel, Campaign>()
                .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore());
            //topcis
			Mapper.CreateMap<Topic, TopicModel>()
				.ForMember(dest => dest.WidgetWrapContent, mo => mo.MapFrom(x => x.WidgetWrapContent.HasValue ? x.WidgetWrapContent.Value : true))
				.ForMember(dest => dest.Url, mo => mo.Ignore())
				.ForMember(dest => dest.Locales, mo => mo.Ignore())
				.ForMember(dest => dest.AvailableStores, mo => mo.Ignore())
				.ForMember(dest => dest.SelectedStoreIds, mo => mo.Ignore())
				.ForMember(dest => dest.AvailableWidgetZones, mo => mo.Ignore())
				.ForMember(dest => dest.AvailableTitleTags, mo => mo.Ignore());
            Mapper.CreateMap<TopicModel, Topic>();

            //category
			Mapper.CreateMap<Category, CategoryModel>()
				.ForMember(dest => dest.AvailableCategoryTemplates, mo => mo.Ignore())
				.ForMember(dest => dest.AvailableDefaultViewModes, mo => mo.Ignore())
				.ForMember(dest => dest.Locales, mo => mo.Ignore())
				.ForMember(dest => dest.Breadcrumb, mo => mo.Ignore())
				.ForMember(dest => dest.ParentCategoryBreadcrumb, mo => mo.Ignore())
				.ForMember(dest => dest.AvailableDiscounts, mo => mo.Ignore())
				.ForMember(dest => dest.SelectedDiscountIds, mo => mo.Ignore())
				.ForMember(dest => dest.SeName, mo => mo.MapFrom(src => src.GetSeName(0, true, false)))
				.ForMember(dest => dest.AvailableCustomerRoles, mo => mo.Ignore())
				.ForMember(dest => dest.SelectedCustomerRoleIds, mo => mo.Ignore())
				.ForMember(dest => dest.AvailableStores, mo => mo.Ignore())
				.ForMember(dest => dest.SelectedStoreIds, mo => mo.Ignore())
				.ForMember(dest => dest.CreatedOn, mo => mo.Ignore())
				.ForMember(dest => dest.UpdatedOn, mo => mo.Ignore())
				.ForMember(dest => dest.GridPageSize, mo => mo.Ignore());
            Mapper.CreateMap<CategoryModel, Category>()
                .ForMember(dest => dest.HasDiscountsApplied, mo => mo.Ignore())
                .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.UpdatedOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.Deleted, mo => mo.Ignore())
                .ForMember(dest => dest.AppliedDiscounts, mo => mo.Ignore())
				.ForMember(dest => dest.Picture, mo => mo.Ignore());
            //manufacturer
			Mapper.CreateMap<Manufacturer, ManufacturerModel>()
				.ForMember(dest => dest.AvailableManufacturerTemplates, mo => mo.Ignore())
				.ForMember(dest => dest.Locales, mo => mo.Ignore())
				.ForMember(dest => dest.SeName, mo => mo.MapFrom(src => src.GetSeName(0, true, false)))
				.ForMember(dest => dest.AvailableStores, mo => mo.Ignore())
				.ForMember(dest => dest.SelectedStoreIds, mo => mo.Ignore())
				.ForMember(dest => dest.CreatedOn, mo => mo.Ignore())
				.ForMember(dest => dest.UpdatedOn, mo => mo.Ignore())
				.ForMember(dest => dest.GridPageSize, mo => mo.Ignore());
            Mapper.CreateMap<ManufacturerModel, Manufacturer>()
                .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.UpdatedOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.Deleted, mo => mo.Ignore())
				.ForMember(dest => dest.Picture, mo => mo.Ignore());
            //products
			Mapper.CreateMap<Product, ProductModel>()
				.ForMember(dest => dest.ProductTypeName, mo => mo.Ignore())
				.ForMember(dest => dest.AssociatedToProductId, mo => mo.Ignore())
				.ForMember(dest => dest.AssociatedToProductName, mo => mo.Ignore())
				.ForMember(dest => dest.ProductTags, mo => mo.Ignore())
				.ForMember(dest => dest.PictureThumbnailUrl, mo => mo.Ignore())
				.ForMember(dest => dest.NoThumb, mo => mo.Ignore())
				.ForMember(dest => dest.AvailableProductTemplates, mo => mo.Ignore())
				.ForMember(dest => dest.Locales, mo => mo.Ignore())
				.ForMember(dest => dest.NumberOfAvailableCategories, mo => mo.Ignore())
				.ForMember(dest => dest.NumberOfAvailableManufacturers, mo => mo.Ignore())
				.ForMember(dest => dest.AddPictureModel, mo => mo.Ignore())
				.ForMember(dest => dest.ProductPictureModels, mo => mo.Ignore())
				.ForMember(dest => dest.AddSpecificationAttributeModel, mo => mo.Ignore())
				.ForMember(dest => dest.CopyProductModel, mo => mo.Ignore())
				.ForMember(dest => dest.SeName, mo => mo.MapFrom(src => src.GetSeName(0, true, false)))
				.ForMember(dest => dest.AvailableCustomerRoles, mo => mo.Ignore())
				.ForMember(dest => dest.SelectedCustomerRoleIds, mo => mo.Ignore())
				.ForMember(dest => dest.AvailableProductTags, mo => mo.Ignore())
				.ForMember(dest => dest.AvailableManageInventoryMethods, mo => mo.Ignore())
				.ForMember(dest => dest.AvailableStores, mo => mo.Ignore())
				.ForMember(dest => dest.SelectedStoreIds, mo => mo.Ignore())
				.ForMember(dest => dest.AvailableTaxCategories, mo => mo.Ignore())
				.ForMember(dest => dest.AvailableMeasureUnits, mo => mo.Ignore())
				.ForMember(dest => dest.PrimaryStoreCurrencyCode, mo => mo.Ignore())
				.ForMember(dest => dest.CreatedOn, mo => mo.Ignore())
				.ForMember(dest => dest.UpdatedOn, mo => mo.Ignore())
				.ForMember(dest => dest.BaseDimensionIn, mo => mo.Ignore())
				.ForMember(dest => dest.BaseWeightIn, mo => mo.Ignore())
				.ForMember(dest => dest.NumberOfAvailableProductAttributes, mo => mo.Ignore())
				.ForMember(dest => dest.Locales, mo => mo.Ignore())
				.ForMember(dest => dest.AvailableDiscounts, mo => mo.Ignore())
				.ForMember(dest => dest.SelectedDiscountIds, mo => mo.Ignore())
				.ForMember(dest => dest.AvailableMeasureWeights, mo => mo.Ignore())
				.ForMember(dest => dest.AvailableDeliveryTimes, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableQuantityUnits, mo => mo.Ignore())
				.ForMember(dest => dest.ProductSelectCheckboxClass, mo => mo.Ignore())
				.ForMember(dest => dest.ProductUrl, mo => mo.Ignore());
			Mapper.CreateMap<ProductModel, Product>()
				.ForMember(dest => dest.DisplayOrder, mo => mo.Ignore())
				.ForMember(dest => dest.ProductTags, mo => mo.Ignore())
				.ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore())
				.ForMember(dest => dest.UpdatedOnUtc, mo => mo.Ignore())
				.ForMember(dest => dest.ParentGroupedProductId, mo => mo.Ignore())
				.ForMember(dest => dest.ProductType, mo => mo.Ignore())
				.ForMember(dest => dest.Deleted, mo => mo.Ignore())
				.ForMember(dest => dest.ApprovedRatingSum, mo => mo.Ignore())
				.ForMember(dest => dest.NotApprovedRatingSum, mo => mo.Ignore())
				.ForMember(dest => dest.ApprovedTotalReviews, mo => mo.Ignore())
				.ForMember(dest => dest.NotApprovedTotalReviews, mo => mo.Ignore())
				.ForMember(dest => dest.ProductCategories, mo => mo.Ignore())
				.ForMember(dest => dest.ProductManufacturers, mo => mo.Ignore())
				.ForMember(dest => dest.ProductPictures, mo => mo.Ignore())
				.ForMember(dest => dest.ProductReviews, mo => mo.Ignore())
				.ForMember(dest => dest.ProductSpecificationAttributes, mo => mo.Ignore())
				.ForMember(dest => dest.AppliedDiscounts, mo => mo.Ignore())
				.ForMember(dest => dest.HasTierPrices, mo => mo.Ignore())
				.ForMember(dest => dest.LowestAttributeCombinationPrice, mo => mo.Ignore())
				.ForMember(dest => dest.HasDiscountsApplied, mo => mo.Ignore())
				.ForMember(dest => dest.BackorderMode, mo => mo.Ignore())
				.ForMember(dest => dest.DownloadActivationType, mo => mo.Ignore())
				.ForMember(dest => dest.GiftCardType, mo => mo.Ignore())
				.ForMember(dest => dest.LowStockActivity, mo => mo.Ignore())
				.ForMember(dest => dest.ManageInventoryMethod, mo => mo.Ignore())
				.ForMember(dest => dest.RecurringCyclePeriod, mo => mo.Ignore())
				.ForMember(dest => dest.ProductVariantAttributes, mo => mo.Ignore())
				.ForMember(dest => dest.ProductVariantAttributeCombinations, mo => mo.Ignore())
				.ForMember(dest => dest.TierPrices, mo => mo.Ignore())
				.ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore())
				.ForMember(dest => dest.UpdatedOnUtc, mo => mo.Ignore())
				.ForMember(dest => dest.Deleted, mo => mo.Ignore())
				.ForMember(dest => dest.DeliveryTime, mo => mo.Ignore())
                .ForMember(dest => dest.QuantityUnit, mo => mo.Ignore())
				.ForMember(dest => dest.MergedDataIgnore, mo => mo.Ignore())
				.ForMember(dest => dest.MergedDataValues, mo => mo.Ignore())
				.ForMember(dest => dest.ProductBundleItems, mo => mo.Ignore())
				.ForMember(dest => dest.SampleDownload, mo => mo.Ignore());
			//logs
            Mapper.CreateMap<Log, LogModel>()
                .ForMember(dest => dest.CustomerEmail, mo => mo.Ignore())
                .ForMember(dest => dest.CreatedOn, mo => mo.Ignore())
				.ForMember(dest => dest.UpdatedOn, mo => mo.Ignore())
				.ForMember(dest => dest.LogLevelHint, mo => mo.Ignore());
            Mapper.CreateMap<LogModel, Log>()
                .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore())
				.ForMember(dest => dest.UpdatedOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.LogLevelId, mo => mo.Ignore())
                .ForMember(dest => dest.Customer, mo => mo.Ignore());
            //ActivityLogType
            Mapper.CreateMap<ActivityLogTypeModel, ActivityLogType>()
                .ForMember(dest => dest.SystemKeyword, mo => mo.Ignore());
			Mapper.CreateMap<ActivityLogType, ActivityLogTypeModel>();
            Mapper.CreateMap<ActivityLog, ActivityLogModel>()
                .ForMember(dest => dest.ActivityLogTypeName, mo => mo.MapFrom(src => src.ActivityLogType.Name))
                .ForMember(dest => dest.CustomerEmail, mo => mo.MapFrom(src => src.Customer.Email))
                .ForMember(dest => dest.CreatedOn, mo => mo.Ignore())
				.ForMember(dest => dest.IsSystemAccount, mo => mo.Ignore())
				.ForMember(dest => dest.SystemAccountName, mo => mo.Ignore());
			//currencies
			Mapper.CreateMap<Currency, CurrencyModel>()
				.ForMember(dest => dest.CreatedOn, mo => mo.Ignore())
				.ForMember(dest => dest.IsPrimaryExchangeRateCurrency, mo => mo.Ignore())
				.ForMember(dest => dest.IsPrimaryStoreCurrency, mo => mo.Ignore())
				.ForMember(dest => dest.PrimaryStoreCurrencyStores, mo => mo.Ignore())
				.ForMember(dest => dest.PrimaryExchangeRateCurrencyStores, mo => mo.Ignore())
				.ForMember(dest => dest.Locales, mo => mo.Ignore())
				.ForMember(dest => dest.AvailableStores, mo => mo.Ignore())
				.ForMember(dest => dest.SelectedStoreIds, mo => mo.Ignore())			
				.ForMember(dest => dest.AvailableDomainEndings, mo => mo.Ignore());
            Mapper.CreateMap<CurrencyModel, Currency>()
                .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.UpdatedOnUtc, mo => mo.Ignore());

            // Delivery times 
            Mapper.CreateMap<DeliveryTime, DeliveryTimeModel>()
                .ForMember(dest => dest.Locales, mo => mo.Ignore());
            Mapper.CreateMap<DeliveryTimeModel, DeliveryTime>();

            // Measure unit
            Mapper.CreateMap<QuantityUnit, QuantityUnitModel>()
                .ForMember(dest => dest.Locales, mo => mo.Ignore());
            Mapper.CreateMap<QuantityUnitModel, QuantityUnit>()
				.ForMember(dest => dest.DisplayLocale, mo => mo.Ignore());

            // ContentSlider slides
            Mapper.CreateMap<ContentSliderSettings, ContentSliderSettingsModel>()
                .ForMember(dest => dest.Id, mo => mo.Ignore())
				.ForMember(dest => dest.AvailableStores, mo => mo.Ignore())
				.ForMember(dest => dest.SearchStoreId, mo => mo.Ignore());
            Mapper.CreateMap<ContentSliderSettingsModel, ContentSliderSettings>();

			Mapper.CreateMap<ContentSliderSlideSettings, ContentSliderSlideModel>()
				.ForMember(dest => dest.Id, mo => mo.Ignore())
				.ForMember(dest => dest.SlideIndex, mo => mo.Ignore())
				.ForMember(dest => dest.AvailableStores, mo => mo.Ignore());
            Mapper.CreateMap<ContentSliderSlideModel, ContentSliderSlideSettings>();

            Mapper.CreateMap<ContentSliderButtonSettings, ContentSliderButtonModel>()
                .ForMember(dest => dest.Id, mo => mo.Ignore());
            Mapper.CreateMap<ContentSliderButtonModel, ContentSliderButtonSettings>();

            // attribute combinations
            Mapper.CreateMap<ProductVariantAttributeCombination, ProductVariantAttributeCombinationModel>()
                .ForMember(dest => dest.AssignablePictures, mo => mo.Ignore())
                .ForMember(dest => dest.ProductVariantAttributes, mo => mo.Ignore())
                .ForMember(dest => dest.AssignedPictureIds, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableDeliveryTimes, mo => mo.Ignore())
                .ForMember(dest => dest.ProductUrl, mo => mo.Ignore())
                .ForMember(dest => dest.ProductUrlTitle, mo => mo.Ignore())
                .ForMember(dest => dest.Warnings, mo => mo.Ignore())
				.ForMember(dest => dest.DisplayOrder, mo => mo.Ignore())
                .AfterMap((src, dest) => dest.AssignedPictureIds = src.GetAssignedPictureIds());
            Mapper.CreateMap<ProductVariantAttributeCombinationModel, ProductVariantAttributeCombination>()
                .ForMember(dest => dest.DeliveryTime, mo => mo.Ignore())
                .ForMember(dest => dest.QuantityUnit, mo => mo.Ignore())
                .ForMember(dest => dest.Product, mo => mo.Ignore())
                .ForMember(dest => dest.AssignedPictureIds, mo => mo.Ignore())
				.ForMember(dest => dest.QuantityUnitId, mo => mo.Ignore())
                .AfterMap((src, dest) => dest.SetAssignedPictureIds(src.AssignedPictureIds));

            //measure weights
            Mapper.CreateMap<MeasureWeight, MeasureWeightModel>()
                .ForMember(dest => dest.IsPrimaryWeight, mo => mo.Ignore());
            Mapper.CreateMap<MeasureWeightModel, MeasureWeight>();
            //measure dimensions
            Mapper.CreateMap<MeasureDimension, MeasureDimensionModel>()
                .ForMember(dest => dest.IsPrimaryDimension, mo => mo.Ignore());
            Mapper.CreateMap<MeasureDimensionModel, MeasureDimension>();
            //tax categories
            Mapper.CreateMap<TaxCategory, TaxCategoryModel>();
            Mapper.CreateMap<TaxCategoryModel, TaxCategory>();
            //shipping methods
            Mapper.CreateMap<ShippingMethod, ShippingMethodModel>()
                .ForMember(dest => dest.Locales, mo => mo.Ignore())
				.ForMember(dest => dest.FilterConfigurationUrls, mo => mo.Ignore());
			Mapper.CreateMap<ShippingMethodModel, ShippingMethod>()
				.ForMember(dest => dest.RestrictedCountries, mo => mo.Ignore());
            //plugins
            Mapper.CreateMap<PluginDescriptor, PluginModel>()
                .ForMember(dest => dest.ConfigurationUrl, mo => mo.Ignore())
				.ForMember(dest => dest.SelectedStoreIds, mo => mo.Ignore())
                .ForMember(dest => dest.Locales, mo => mo.Ignore())
                .ForMember(dest => dest.IconUrl, mo => mo.Ignore())
				.ForMember(dest => dest.ConfigurationRoute, mo => mo.Ignore())
				.ForMember(dest => dest.LicenseUrl, mo => mo.Ignore())
				.ForMember(dest => dest.IsLicensable, mo => mo.Ignore())
				.ForMember(dest => dest.LicenseState, mo => mo.Ignore())
				.ForMember(dest => dest.TruncatedLicenseKey, mo => mo.Ignore())
				.ForMember(dest => dest.RemainingDemoUsageDays, mo => mo.Ignore());
            //newsLetter subscriptions
            Mapper.CreateMap<NewsLetterSubscription, NewsLetterSubscriptionModel>()
                .ForMember(dest => dest.CreatedOn, mo => mo.Ignore())
				.ForMember(dest => dest.StoreName, mo => mo.Ignore());
            Mapper.CreateMap<NewsLetterSubscriptionModel, NewsLetterSubscription>()
                .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.NewsLetterSubscriptionGuid, mo => mo.Ignore())
				.ForMember(dest => dest.StoreId, mo => mo.Ignore());
            //forums
            Mapper.CreateMap<ForumGroup, ForumGroupModel>()
				.ForMember(dest => dest.Locales, mo => mo.Ignore())
				.ForMember(dest => dest.SeName, mo => mo.MapFrom(src => src.GetSeName(0, true, false)))
				.ForMember(dest => dest.AvailableStores, mo => mo.Ignore())
				.ForMember(dest => dest.SelectedStoreIds, mo => mo.Ignore())
				.ForMember(dest => dest.CreatedOn, mo => mo.Ignore())
                .ForMember(dest => dest.ForumModels, mo => mo.Ignore());
            Mapper.CreateMap<ForumGroupModel, ForumGroup>()
                .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.UpdatedOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.Forums, mo => mo.Ignore());
            Mapper.CreateMap<Forum, ForumModel>()
				.ForMember(dest => dest.Locales, mo => mo.Ignore())
				.ForMember(dest => dest.SeName, mo => mo.MapFrom(src => src.GetSeName(0, true, false)))
                .ForMember(dest => dest.CreatedOn, mo => mo.Ignore())
                .ForMember(dest => dest.ForumGroups, mo => mo.Ignore());
            Mapper.CreateMap<ForumModel, Forum>()
                .ForMember(dest => dest.NumTopics, mo => mo.Ignore())
                .ForMember(dest => dest.NumPosts, mo => mo.Ignore())
                .ForMember(dest => dest.LastTopicId, mo => mo.Ignore())
                .ForMember(dest => dest.LastPostId, mo => mo.Ignore())
                .ForMember(dest => dest.LastPostCustomerId, mo => mo.Ignore())
                .ForMember(dest => dest.LastPostTime, mo => mo.Ignore())
                .ForMember(dest => dest.ForumGroup, mo => mo.Ignore())
                .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.UpdatedOnUtc, mo => mo.Ignore());
            //blogs
            Mapper.CreateMap<BlogPost, BlogPostModel>()
                .ForMember(dest => dest.SeName, mo => mo.MapFrom(src => src.GetSeName(src.LanguageId, true, false)))
                .ForMember(dest => dest.Comments, mo => mo.Ignore())
                .ForMember(dest => dest.StartDate, mo => mo.Ignore())
                .ForMember(dest => dest.EndDate, mo => mo.Ignore())
                .ForMember(dest => dest.CreatedOn, mo => mo.Ignore())
				.ForMember(dest => dest.AvailableStores, mo => mo.Ignore())
				.ForMember(dest => dest.SelectedStoreIds, mo => mo.Ignore());
            Mapper.CreateMap<BlogPostModel, BlogPost>()
                .ForMember(dest => dest.BlogComments, mo => mo.Ignore())
                .ForMember(dest => dest.Language, mo => mo.Ignore())
                .ForMember(dest => dest.ApprovedCommentCount, mo => mo.Ignore())
                .ForMember(dest => dest.NotApprovedCommentCount, mo => mo.Ignore())
                .ForMember(dest => dest.StartDateUtc, mo => mo.Ignore())
                .ForMember(dest => dest.EndDateUtc, mo => mo.Ignore())
                .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore());
            //news
            Mapper.CreateMap<NewsItem, NewsItemModel>()
                .ForMember(dest => dest.SeName, mo => mo.MapFrom(src => src.GetSeName(src.LanguageId, true, false)))
                .ForMember(dest => dest.Comments, mo => mo.Ignore())
                .ForMember(dest => dest.StartDate, mo => mo.Ignore())
                .ForMember(dest => dest.EndDate, mo => mo.Ignore())
                .ForMember(dest => dest.CreatedOn, mo => mo.Ignore())
				.ForMember(dest => dest.AvailableStores, mo => mo.Ignore())
				.ForMember(dest => dest.SelectedStoreIds, mo => mo.Ignore());
            Mapper.CreateMap<NewsItemModel, NewsItem>()
                .ForMember(dest => dest.NewsComments, mo => mo.Ignore())
                .ForMember(dest => dest.Language, mo => mo.Ignore())
                .ForMember(dest => dest.ApprovedCommentCount, mo => mo.Ignore())
                .ForMember(dest => dest.NotApprovedCommentCount, mo => mo.Ignore())
                .ForMember(dest => dest.StartDateUtc, mo => mo.Ignore())
                .ForMember(dest => dest.EndDateUtc, mo => mo.Ignore())
                .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore());
            //polls
            Mapper.CreateMap<Poll, PollModel>()
                .ForMember(dest => dest.StartDate, mo => mo.Ignore())
                .ForMember(dest => dest.EndDate, mo => mo.Ignore())
				.ForMember(dest => dest.AvailableStores, mo => mo.Ignore())
				.ForMember(dest => dest.SelectedStoreIds, mo => mo.Ignore());
            Mapper.CreateMap<PollModel, Poll>()
                .ForMember(dest => dest.PollAnswers, mo => mo.Ignore())
                .ForMember(dest => dest.Language, mo => mo.Ignore())
                .ForMember(dest => dest.StartDateUtc, mo => mo.Ignore())
                .ForMember(dest => dest.EndDateUtc, mo => mo.Ignore());
            //customer roles
            Mapper.CreateMap<CustomerRole, CustomerRoleModel>()
                .ForMember(dest => dest.TaxDisplayTypes, mo => mo.Ignore())
				/*.ForMember(dest => dest.TaxDisplayType, mo => mo.MapFrom((src) => src.TaxDisplayType))*/;
			Mapper.CreateMap<CustomerRoleModel, CustomerRole>()
				.ForMember(dest => dest.PermissionRecords, mo => mo.Ignore())
				/*.ForMember(dest => dest.TaxDisplayType, mo => mo.MapFrom((src) => src.TaxDisplayType))*/;

            //product attributes
            Mapper.CreateMap<ProductAttribute, ProductAttributeModel>()
                .ForMember(dest => dest.Locales, mo => mo.Ignore());
            Mapper.CreateMap<ProductAttributeModel, ProductAttribute>();
            //specification attributes
            Mapper.CreateMap<SpecificationAttribute, SpecificationAttributeModel>()
                .ForMember(dest => dest.Locales, mo => mo.Ignore())
				.ForMember(dest => dest.OptionCount, mo => mo.Ignore());
            Mapper.CreateMap<SpecificationAttributeModel, SpecificationAttribute>()
                .ForMember(dest => dest.SpecificationAttributeOptions, mo => mo.Ignore());
            Mapper.CreateMap<SpecificationAttributeOption, SpecificationAttributeOptionModel>()
                .ForMember(dest => dest.Locales, mo => mo.Ignore())
                .ForMember(dest => dest.Multiple, mo => mo.Ignore());
            Mapper.CreateMap<SpecificationAttributeOptionModel, SpecificationAttributeOption>()
                .ForMember(dest => dest.SpecificationAttribute, mo => mo.Ignore())
                .ForMember(dest => dest.ProductSpecificationAttributes, mo => mo.Ignore());
			//checkout attributes
			Mapper.CreateMap<CheckoutAttribute, CheckoutAttributeModel>()
				.ForMember(dest => dest.AvailableTaxCategories, mo => mo.Ignore())
				.ForMember(dest => dest.AttributeControlTypeName, mo => mo.Ignore())
				.ForMember(dest => dest.Locales, mo => mo.Ignore())
				.ForMember(dest => dest.SelectedStoreIds, mo => mo.Ignore())
				.ForMember(dest => dest.AvailableStores, mo => mo.Ignore());
			Mapper.CreateMap<CheckoutAttributeModel, CheckoutAttribute>()
                .ForMember(dest => dest.AttributeControlType, mo => mo.Ignore())
                .ForMember(dest => dest.CheckoutAttributeValues, mo => mo.Ignore());
            Mapper.CreateMap<CheckoutAttributeValue, CheckoutAttributeValueModel>()
                .ForMember(dest => dest.PrimaryStoreCurrencyCode, mo => mo.Ignore())
                .ForMember(dest => dest.BaseWeightIn, mo => mo.Ignore())
                .ForMember(dest => dest.Locales, mo => mo.Ignore());
            Mapper.CreateMap<CheckoutAttributeValueModel, CheckoutAttributeValue>()
                .ForMember(dest => dest.CheckoutAttribute, mo => mo.Ignore());
			
			// product bundle items
			Mapper.CreateMap<ProductBundleItem, ProductBundleItemModel>()
				.ForMember(dest => dest.Locales, mo => mo.Ignore())
				.ForMember(dest => dest.Attributes, mo => mo.Ignore())
				.ForMember(dest => dest.IsPerItemPricing, mo => mo.Ignore())
				.ForMember(dest => dest.CreatedOn, mo => mo.Ignore())
				.ForMember(dest => dest.UpdatedOn, mo => mo.Ignore());
			Mapper.CreateMap<ProductBundleItemModel, ProductBundleItem>()
				.ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore())
				.ForMember(dest => dest.UpdatedOnUtc, mo => mo.Ignore())
				.ForMember(dest => dest.Product, mo => mo.Ignore())
				.ForMember(dest => dest.BundleProduct, mo => mo.Ignore())
				.ForMember(dest => dest.AttributeFilters, mo => mo.Ignore());

            //discounts
            Mapper.CreateMap<Discount, DiscountModel>()
                .ForMember(dest => dest.PrimaryStoreCurrencyCode, mo => mo.Ignore())
                .ForMember(dest => dest.AddDiscountRequirement, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableDiscountRequirementRules, mo => mo.Ignore())
                .ForMember(dest => dest.DiscountRequirementMetaInfos, mo => mo.Ignore())
                .ForMember(dest => dest.AppliedToCategoryModels, mo => mo.Ignore())
                .ForMember(dest => dest.AppliedToProductModels, mo => mo.Ignore());
            Mapper.CreateMap<DiscountModel, Discount>()
                .ForMember(dest => dest.DiscountType, mo => mo.Ignore())
                .ForMember(dest => dest.DiscountLimitation, mo => mo.Ignore())
                .ForMember(dest => dest.DiscountRequirements, mo => mo.Ignore())
                .ForMember(dest => dest.AppliedToCategories, mo => mo.Ignore())
				.ForMember(dest => dest.AppliedToProducts, mo => mo.Ignore());
            //gift cards
            Mapper.CreateMap<GiftCard, GiftCardModel>()
                .ForMember(dest => dest.PurchasedWithOrderId, mo => mo.Ignore())
                .ForMember(dest => dest.AmountStr, mo => mo.Ignore())
                .ForMember(dest => dest.RemainingAmountStr, mo => mo.Ignore())
                .ForMember(dest => dest.CreatedOn, mo => mo.Ignore())
                .ForMember(dest => dest.PrimaryStoreCurrencyCode, mo => mo.Ignore());
            Mapper.CreateMap<GiftCardModel, GiftCard>()
                .ForMember(dest => dest.PurchasedWithOrderItemId, mo => mo.Ignore())
                .ForMember(dest => dest.GiftCardType, mo => mo.Ignore())
                .ForMember(dest => dest.GiftCardUsageHistory, mo => mo.Ignore())
                .ForMember(dest => dest.PurchasedWithOrderItem, mo => mo.Ignore())
                .ForMember(dest => dest.IsRecipientNotified, mo => mo.Ignore())
                .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore());
			//stores
			Mapper.CreateMap<Store, StoreModel>()
				.ForMember(dest => dest.AvailableCurrencies, mo => mo.Ignore())
				.ForMember(dest => dest.PrimaryStoreCurrencyName, mo => mo.Ignore())
				.ForMember(dest => dest.PrimaryExchangeRateCurrencyName, mo => mo.Ignore());
			Mapper.CreateMap<StoreModel, Store>()
				.ForMember(dest => dest.PrimaryStoreCurrency, mo => mo.Ignore())
				.ForMember(dest => dest.PrimaryExchangeRateCurrency, mo => mo.Ignore());
            //Settings
            Mapper.CreateMap<TaxSettings, TaxSettingsModel>()
                .ForMember(dest => dest.DefaultTaxAddress, mo => mo.Ignore())
                .ForMember(dest => dest.TaxDisplayTypeValues, mo => mo.Ignore())
                .ForMember(dest => dest.TaxBasedOnValues, mo => mo.Ignore())
                .ForMember(dest => dest.PaymentMethodAdditionalFeeTaxCategories, mo => mo.Ignore())
                .ForMember(dest => dest.ShippingTaxCategories, mo => mo.Ignore())
                .ForMember(dest => dest.EuVatShopCountries, mo => mo.Ignore());
            Mapper.CreateMap<TaxSettingsModel, TaxSettings>()
                .ForMember(dest => dest.ActiveTaxProviderSystemName, mo => mo.Ignore());
            Mapper.CreateMap<NewsSettings, NewsSettingsModel>();
            Mapper.CreateMap<NewsSettingsModel, NewsSettings>();
            Mapper.CreateMap<ForumSettings, ForumSettingsModel>()
                .ForMember(dest => dest.ForumEditorValues, mo => mo.Ignore());
            Mapper.CreateMap<ForumSettingsModel, ForumSettings>()
                .ForMember(dest => dest.TopicSubjectMaxLength, mo => mo.Ignore())
                .ForMember(dest => dest.StrippedTopicMaxLength, mo => mo.Ignore())
                .ForMember(dest => dest.PostMaxLength, mo => mo.Ignore())
                .ForMember(dest => dest.TopicPostsPageLinkDisplayCount, mo => mo.Ignore())
                .ForMember(dest => dest.LatestCustomerPostsPageSize, mo => mo.Ignore())
                .ForMember(dest => dest.PrivateMessagesPageSize, mo => mo.Ignore())
                .ForMember(dest => dest.ForumSubscriptionsPageSize, mo => mo.Ignore())
                .ForMember(dest => dest.PMSubjectMaxLength, mo => mo.Ignore())
                .ForMember(dest => dest.PMTextMaxLength, mo => mo.Ignore())
                .ForMember(dest => dest.HomePageActiveDiscussionsTopicCount, mo => mo.Ignore())
                .ForMember(dest => dest.ActiveDiscussionsPageTopicCount, mo => mo.Ignore())
                .ForMember(dest => dest.ForumSearchTermMinimumLength, mo => mo.Ignore());
            Mapper.CreateMap<BlogSettings, BlogSettingsModel>();
            Mapper.CreateMap<BlogSettingsModel, BlogSettings>();
            Mapper.CreateMap<ShippingSettings, ShippingSettingsModel>()
                .ForMember(dest => dest.ShippingOriginAddress, mo => mo.Ignore());
            Mapper.CreateMap<ShippingSettingsModel, ShippingSettings>()
                .ForMember(dest => dest.ActiveShippingRateComputationMethodSystemNames, mo => mo.Ignore())
                .ForMember(dest => dest.ReturnValidOptionsIfThereAreAny, mo => mo.Ignore());
			Mapper.CreateMap<CatalogSettings, CatalogSettingsModel>()
				.ForMember(dest => dest.AvailableSubCategoryDisplayTypes, mo => mo.Ignore())
				.ForMember(dest => dest.AvailablePriceDisplayTypes, mo => mo.Ignore())
				.ForMember(dest => dest.AvailableDefaultViewModes, mo => mo.Ignore())
				.ForMember(dest => dest.AvailableDeliveryTimes, mo => mo.Ignore())
				.ForMember(dest => dest.AvailableSortOrderModes, mo => mo.Ignore());
            Mapper.CreateMap<CatalogSettingsModel, CatalogSettings>()
                .ForMember(dest => dest.PageShareCode, mo => mo.Ignore())
                .ForMember(dest => dest.DefaultProductRatingValue, mo => mo.Ignore())
                .ForMember(dest => dest.ProductSearchTermMinimumLength, mo => mo.Ignore())
                .ForMember(dest => dest.UseSmallProductBoxOnHomePage, mo => mo.Ignore())
                .ForMember(dest => dest.DefaultCategoryPageSizeOptions, mo => mo.Ignore())
                .ForMember(dest => dest.DefaultManufacturerPageSizeOptions, mo => mo.Ignore())
                .ForMember(dest => dest.MaximumBackInStockSubscriptions, mo => mo.Ignore())
                .ForMember(dest => dest.DisplayTierPricesWithDiscounts, mo => mo.Ignore())
                .ForMember(dest => dest.FileUploadMaximumSizeBytes, mo => mo.Ignore())
                .ForMember(dest => dest.FileUploadAllowedExtensions, mo => mo.Ignore())
                .ForMember(dest => dest.ProductSearchPageSize, mo => mo.Ignore())
				.ForMember(dest => dest.MostRecentlyUsedCategoriesMaxSize, mo => mo.Ignore())
				.ForMember(dest => dest.MostRecentlyUsedManufacturersMaxSize, mo => mo.Ignore());
            Mapper.CreateMap<RewardPointsSettings, RewardPointsSettingsModel>()
                .ForMember(dest => dest.PrimaryStoreCurrencyCode, mo => mo.Ignore())
				.ForMember(dest => dest.PointsForPurchases_OverrideForStore, mo => mo.Ignore());
            Mapper.CreateMap<RewardPointsSettingsModel, RewardPointsSettings>();
            Mapper.CreateMap<OrderSettings, OrderSettingsModel>()
                .ForMember(dest => dest.GiftCards_Activated_OrderStatuses, mo => mo.Ignore())
                .ForMember(dest => dest.GiftCards_Deactivated_OrderStatuses, mo => mo.Ignore())
                .ForMember(dest => dest.PrimaryStoreCurrencyCode, mo => mo.Ignore())
				.ForMember(dest => dest.StoreCount, mo => mo.Ignore())
                .ForMember(dest => dest.OrderIdent, mo => mo.Ignore())
				.ForMember(dest => dest.Locales, mo => mo.Ignore());
            Mapper.CreateMap<OrderSettingsModel, OrderSettings>()
                .ForMember(dest => dest.MinimumOrderPlacementInterval, mo => mo.Ignore())
				.ForMember(dest => dest.Id, mo => mo.Ignore());
			Mapper.CreateMap<ShoppingCartSettings, ShoppingCartSettingsModel>()
				.ForMember(dest => dest.AvailableNewsLetterSubscriptions, mo => mo.Ignore())
				.ForMember(dest => dest.AvailableThirdPartyEmailHandOver, mo => mo.Ignore())
				.ForMember(dest => dest.Locales, mo => mo.Ignore());
			Mapper.CreateMap<ShoppingCartSettingsModel, ShoppingCartSettings>()
				.ForMember(dest => dest.Id, mo => mo.Ignore())
				.ForMember(dest => dest.MoveItemsFromWishlistToCart, mo => mo.Ignore())
				.ForMember(dest => dest.ShowItemsFromWishlistToCartButton, mo => mo.Ignore());
			Mapper.CreateMap<MediaSettings, MediaSettingsModel>()
				.ForMember(dest => dest.PicturesStoredIntoDatabase, mo => mo.Ignore())
				.ForMember(dest => dest.AvailablePictureZoomTypes, mo => mo.Ignore());
            Mapper.CreateMap<MediaSettingsModel, MediaSettings>()
                //.ForMember(dest => dest.DefaultPictureZoomEnabled, mo => mo.Ignore())
                .ForMember(dest => dest.DefaultImageQuality, mo => mo.Ignore())
                .ForMember(dest => dest.MultipleThumbDirectories, mo => mo.Ignore())
                .ForMember(dest => dest.VariantValueThumbPictureSize, mo => mo.Ignore());
			Mapper.CreateMap<CustomerSettings, CustomerUserSettingsModel.CustomerSettingsModel>()
				.ForMember(dest => dest.AvailableCustomerNumberMethods, mo => mo.Ignore())
				.ForMember(dest => dest.AvailableCustomerNumberVisibilities, mo => mo.Ignore())
				.ForMember(dest => dest.AvailableRegisterCustomerRoles, mo => mo.Ignore());
			Mapper.CreateMap<CustomerUserSettingsModel.CustomerSettingsModel, CustomerSettings>()
                .ForMember(dest => dest.HashedPasswordFormat, mo => mo.Ignore())
                .ForMember(dest => dest.PasswordMinLength, mo => mo.Ignore())
                .ForMember(dest => dest.AvatarMaximumSizeBytes, mo => mo.Ignore())
                .ForMember(dest => dest.DownloadableProductsValidateUser, mo => mo.Ignore())
                .ForMember(dest => dest.OnlineCustomerMinutes, mo => mo.Ignore())
                .ForMember(dest => dest.PrefillLoginUsername, mo => mo.Ignore())
                .ForMember(dest => dest.PrefillLoginPwd, mo => mo.Ignore());
            Mapper.CreateMap<AddressSettings,  CustomerUserSettingsModel.AddressSettingsModel>();
            Mapper.CreateMap<CustomerUserSettingsModel.AddressSettingsModel, AddressSettings>();

			Mapper.CreateMap<ThemeSettings, ThemeListModel>()
				.ForMember(dest => dest.AvailableBundleOptimizationValues, mo => mo.Ignore())
				.ForMember(dest => dest.DesktopThemes, mo => mo.Ignore())
				.ForMember(dest => dest.MobileThemes, mo => mo.Ignore())
				.ForMember(dest => dest.StoreId, mo => mo.Ignore())
				.ForMember(dest => dest.AvailableStores, mo => mo.Ignore());
            Mapper.CreateMap<ThemeListModel, ThemeSettings>()
                .ForMember(dest => dest.EmulateMobileDevice, mo => mo.Ignore());
        }
        
        public int Order
        {
            get { return 0; }
        }
    }
}