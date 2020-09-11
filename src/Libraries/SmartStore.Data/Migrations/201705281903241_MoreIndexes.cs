namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;
    using System.Web.Hosting;
    using Core.Data;
    using SmartStore.Data.Setup;

    public partial class MoreIndexes : DbMigration
    {
        public override void Up()
        {
            if (!DbMigrationContext.Current.SuppressInitialCreate<SmartObjectContext>())
            {
                UpInitial();
            }

            if (HostingEnvironment.IsHosted && DataSettings.Current.IsSqlServer)
            {
                // Avoid "Column 'Name' in table 'dbo.ProductVariantAttributeValue' is of a type that is invalid for use as a key column in an index".
                Sql("If -1 = (SELECT CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductVariantAttributeValue' AND COLUMN_NAME = 'Name') ALTER TABLE [dbo].[ProductVariantAttributeValue] ALTER COLUMN [Name] nvarchar(4000) NULL;");
            }

            CreateIndex("dbo.Product_Category_Mapping", "IsFeaturedProduct");
            CreateIndex("dbo.Product_Manufacturer_Mapping", "IsFeaturedProduct");
            CreateIndex("dbo.SpecificationAttribute", "AllowFiltering");
            CreateIndex("dbo.Product_ProductAttribute_Mapping", "AttributeControlTypeId");
            CreateIndex("dbo.ProductAttribute", "AllowFiltering");
            CreateIndex("dbo.ProductVariantAttributeValue", "ValueTypeId");

            // Useless index. The name field with 4000 characters is too long for an index.
            if (DataSettings.Current.IsSqlServer)
            {
                CreateIndex("dbo.ProductVariantAttributeValue", "Name");
            }
        }

        public override void Down()
        {
            if (DataSettings.Current.IsSqlServer)
            {
                DropIndex("dbo.ProductVariantAttributeValue", new[] { "Name" });
            }

            DropIndex("dbo.ProductVariantAttributeValue", new[] { "ValueTypeId" });
            DropIndex("dbo.ProductAttribute", new[] { "AllowFiltering" });
            DropIndex("dbo.Product_ProductAttribute_Mapping", new[] { "AttributeControlTypeId" });
            DropIndex("dbo.SpecificationAttribute", new[] { "AllowFiltering" });
            DropIndex("dbo.Product_Manufacturer_Mapping", new[] { "IsFeaturedProduct" });
            DropIndex("dbo.Product_Category_Mapping", new[] { "IsFeaturedProduct" });

            DownInitial();
        }

        private void UpInitial()
        {
            #region auto-generated

            CreateTable(
                "dbo.Topic",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    SystemName = c.String(maxLength: 4000),
                    IncludeInSitemap = c.Boolean(nullable: false),
                    IsPasswordProtected = c.Boolean(nullable: false),
                    Password = c.String(maxLength: 4000),
                    Title = c.String(maxLength: 4000),
                    Body = c.String(),
                    MetaKeywords = c.String(maxLength: 4000),
                    MetaDescription = c.String(maxLength: 4000),
                    MetaTitle = c.String(maxLength: 4000),
                    LimitedToStores = c.Boolean(nullable: false),
                    RenderAsWidget = c.Boolean(nullable: false),
                    WidgetZone = c.String(maxLength: 4000),
                    WidgetWrapContent = c.Boolean(),
                    WidgetShowTitle = c.Boolean(nullable: false),
                    WidgetBordered = c.Boolean(nullable: false),
                    Priority = c.Int(nullable: false),
                    TitleTag = c.String(),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.TaxCategory",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(nullable: false, maxLength: 400),
                    DisplayOrder = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.Setting",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(nullable: false, maxLength: 200),
                    Value = c.String(nullable: false),
                    StoreId = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.LocalizedProperty",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    EntityId = c.Int(nullable: false),
                    LanguageId = c.Int(nullable: false),
                    LocaleKeyGroup = c.String(nullable: false, maxLength: 400),
                    LocaleKey = c.String(nullable: false, maxLength: 400),
                    LocaleValue = c.String(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Language", t => t.LanguageId, cascadeDelete: true)
                .Index(t => new { t.EntityId, t.LocaleKey, t.LocaleKeyGroup, t.LanguageId }, name: "IX_LocalizedProperty_Compound");

            CreateTable(
                "dbo.Language",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(nullable: false, maxLength: 100),
                    LanguageCulture = c.String(nullable: false, maxLength: 20),
                    UniqueSeoCode = c.String(maxLength: 2),
                    FlagImageFileName = c.String(maxLength: 50),
                    Rtl = c.Boolean(nullable: false),
                    LimitedToStores = c.Boolean(nullable: false),
                    Published = c.Boolean(nullable: false),
                    DisplayOrder = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.LocaleStringResource",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    LanguageId = c.Int(nullable: false),
                    ResourceName = c.String(nullable: false, maxLength: 200),
                    ResourceValue = c.String(nullable: false),
                    IsFromPlugin = c.Boolean(),
                    IsTouched = c.Boolean(),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Language", t => t.LanguageId, cascadeDelete: true)
                .Index(t => t.LanguageId);

            CreateTable(
                "dbo.PermissionRecord",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    //Name = c.String(nullable: false),
                    SystemName = c.String(nullable: false, maxLength: 255),
                    //Category = c.String(nullable: false, maxLength: 255),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.CustomerRole",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(nullable: false, maxLength: 255),
                    FreeShipping = c.Boolean(nullable: false),
                    TaxExempt = c.Boolean(nullable: false),
                    TaxDisplayType = c.Int(),
                    Active = c.Boolean(nullable: false),
                    IsSystemRole = c.Boolean(nullable: false),
                    SystemName = c.String(maxLength: 255),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.Poll",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    LanguageId = c.Int(nullable: false),
                    Name = c.String(nullable: false, maxLength: 4000),
                    SystemKeyword = c.String(maxLength: 4000),
                    Published = c.Boolean(nullable: false),
                    ShowOnHomePage = c.Boolean(nullable: false),
                    AllowGuestsToVote = c.Boolean(nullable: false),
                    DisplayOrder = c.Int(nullable: false),
                    StartDateUtc = c.DateTime(),
                    EndDateUtc = c.DateTime(),
                    LimitedToStores = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Language", t => t.LanguageId, cascadeDelete: true)
                .Index(t => t.LanguageId);

            CreateTable(
                "dbo.PollAnswer",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    PollId = c.Int(nullable: false),
                    Name = c.String(nullable: false, maxLength: 4000),
                    NumberOfVotes = c.Int(nullable: false),
                    DisplayOrder = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Poll", t => t.PollId, cascadeDelete: true)
                .Index(t => t.PollId);

            CreateTable(
                "dbo.CustomerContent",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    CustomerId = c.Int(nullable: false),
                    IpAddress = c.String(maxLength: 200),
                    IsApproved = c.Boolean(nullable: false),
                    CreatedOnUtc = c.DateTime(nullable: false),
                    UpdatedOnUtc = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Customer", t => t.CustomerId, cascadeDelete: true)
                .Index(t => t.CustomerId);

            CreateTable(
                "dbo.Customer",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    CustomerGuid = c.Guid(nullable: false),
                    Username = c.String(maxLength: 500),
                    Email = c.String(maxLength: 500),
                    Password = c.String(maxLength: 500),
                    PasswordFormatId = c.Int(nullable: false),
                    PasswordSalt = c.String(maxLength: 500),
                    AdminComment = c.String(maxLength: 4000),
                    IsTaxExempt = c.Boolean(nullable: false),
                    AffiliateId = c.Int(nullable: false),
                    Active = c.Boolean(nullable: false),
                    Deleted = c.Boolean(nullable: false),
                    IsSystemAccount = c.Boolean(nullable: false),
                    SystemName = c.String(maxLength: 500),
                    LastIpAddress = c.String(maxLength: 100),
                    CreatedOnUtc = c.DateTime(nullable: false),
                    LastLoginDateUtc = c.DateTime(),
                    LastActivityDateUtc = c.DateTime(nullable: false),
                    BillingAddress_Id = c.Int(),
                    ShippingAddress_Id = c.Int(),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Address", t => t.BillingAddress_Id)
                .ForeignKey("dbo.Address", t => t.ShippingAddress_Id)
                .Index(t => t.Deleted)
                .Index(t => t.SystemName)
                .Index(t => t.LastIpAddress, name: "IX_Customer_LastIpAddress")
                .Index(t => t.CreatedOnUtc, name: "IX_Customer_CreatedOn")
                .Index(t => t.LastActivityDateUtc, name: "IX_Customer_LastActivity")
                .Index(t => t.BillingAddress_Id)
                .Index(t => t.ShippingAddress_Id);

            CreateTable(
                "dbo.Address",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Salutation = c.String(),
                    Title = c.String(),
                    FirstName = c.String(maxLength: 4000),
                    LastName = c.String(maxLength: 4000),
                    Email = c.String(maxLength: 4000),
                    Company = c.String(maxLength: 4000),
                    CountryId = c.Int(),
                    StateProvinceId = c.Int(),
                    City = c.String(maxLength: 4000),
                    Address1 = c.String(maxLength: 4000),
                    Address2 = c.String(maxLength: 4000),
                    ZipPostalCode = c.String(maxLength: 4000),
                    PhoneNumber = c.String(maxLength: 4000),
                    FaxNumber = c.String(maxLength: 4000),
                    CreatedOnUtc = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Country", t => t.CountryId)
                .ForeignKey("dbo.StateProvince", t => t.StateProvinceId)
                .Index(t => t.CountryId)
                .Index(t => t.StateProvinceId);

            CreateTable(
                "dbo.Country",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(nullable: false, maxLength: 100),
                    AllowsBilling = c.Boolean(nullable: false),
                    AllowsShipping = c.Boolean(nullable: false),
                    TwoLetterIsoCode = c.String(maxLength: 2),
                    ThreeLetterIsoCode = c.String(maxLength: 3),
                    NumericIsoCode = c.Int(nullable: false),
                    SubjectToVat = c.Boolean(nullable: false),
                    Published = c.Boolean(nullable: false),
                    DisplayOrder = c.Int(nullable: false),
                    LimitedToStores = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.ShippingMethod",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(nullable: false, maxLength: 400),
                    Description = c.String(maxLength: 4000),
                    DisplayOrder = c.Int(nullable: false),
                    IgnoreCharges = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.StateProvince",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    CountryId = c.Int(nullable: false),
                    Name = c.String(nullable: false, maxLength: 100),
                    Abbreviation = c.String(maxLength: 100),
                    Published = c.Boolean(nullable: false),
                    DisplayOrder = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Country", t => t.CountryId, cascadeDelete: true)
                .Index(t => t.CountryId);

            CreateTable(
                "dbo.BlogPost",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    LanguageId = c.Int(nullable: false),
                    Title = c.String(nullable: false, maxLength: 4000),
                    Body = c.String(nullable: false),
                    AllowComments = c.Boolean(nullable: false),
                    ApprovedCommentCount = c.Int(nullable: false),
                    NotApprovedCommentCount = c.Int(nullable: false),
                    Tags = c.String(maxLength: 4000),
                    StartDateUtc = c.DateTime(),
                    EndDateUtc = c.DateTime(),
                    MetaKeywords = c.String(maxLength: 400),
                    MetaDescription = c.String(maxLength: 4000),
                    MetaTitle = c.String(maxLength: 400),
                    LimitedToStores = c.Boolean(nullable: false),
                    CreatedOnUtc = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Language", t => t.LanguageId, cascadeDelete: true)
                .Index(t => t.LanguageId);

            CreateTable(
                "dbo.Product",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    ProductTypeId = c.Int(nullable: false),
                    ParentGroupedProductId = c.Int(nullable: false),
                    VisibleIndividually = c.Boolean(nullable: false),
                    Name = c.String(nullable: false, maxLength: 400),
                    ShortDescription = c.String(maxLength: 4000),
                    FullDescription = c.String(),
                    AdminComment = c.String(maxLength: 4000),
                    ProductTemplateId = c.Int(nullable: false),
                    ShowOnHomePage = c.Boolean(nullable: false),
                    HomePageDisplayOrder = c.Int(nullable: false),
                    MetaKeywords = c.String(maxLength: 400),
                    MetaDescription = c.String(maxLength: 4000),
                    MetaTitle = c.String(maxLength: 400),
                    AllowCustomerReviews = c.Boolean(nullable: false),
                    ApprovedRatingSum = c.Int(nullable: false),
                    NotApprovedRatingSum = c.Int(nullable: false),
                    ApprovedTotalReviews = c.Int(nullable: false),
                    NotApprovedTotalReviews = c.Int(nullable: false),
                    SubjectToAcl = c.Boolean(nullable: false),
                    LimitedToStores = c.Boolean(nullable: false),
                    Sku = c.String(maxLength: 400),
                    ManufacturerPartNumber = c.String(maxLength: 400),
                    Gtin = c.String(maxLength: 400),
                    IsGiftCard = c.Boolean(nullable: false),
                    GiftCardTypeId = c.Int(nullable: false),
                    RequireOtherProducts = c.Boolean(nullable: false),
                    RequiredProductIds = c.String(maxLength: 1000),
                    AutomaticallyAddRequiredProducts = c.Boolean(nullable: false),
                    IsDownload = c.Boolean(nullable: false),
                    DownloadId = c.Int(nullable: false),
                    UnlimitedDownloads = c.Boolean(nullable: false),
                    MaxNumberOfDownloads = c.Int(nullable: false),
                    DownloadExpirationDays = c.Int(),
                    DownloadActivationTypeId = c.Int(nullable: false),
                    HasSampleDownload = c.Boolean(nullable: false),
                    SampleDownloadId = c.Int(),
                    HasUserAgreement = c.Boolean(nullable: false),
                    UserAgreementText = c.String(maxLength: 4000),
                    IsRecurring = c.Boolean(nullable: false),
                    RecurringCycleLength = c.Int(nullable: false),
                    RecurringCyclePeriodId = c.Int(nullable: false),
                    RecurringTotalCycles = c.Int(nullable: false),
                    IsShipEnabled = c.Boolean(nullable: false),
                    IsFreeShipping = c.Boolean(nullable: false),
                    AdditionalShippingCharge = c.Decimal(nullable: false, precision: 18, scale: 4),
                    IsTaxExempt = c.Boolean(nullable: false),
                    IsEsd = c.Boolean(nullable: false),
                    TaxCategoryId = c.Int(nullable: false),
                    ManageInventoryMethodId = c.Int(nullable: false),
                    StockQuantity = c.Int(nullable: false),
                    DisplayStockAvailability = c.Boolean(nullable: false),
                    DisplayStockQuantity = c.Boolean(nullable: false),
                    MinStockQuantity = c.Int(nullable: false),
                    LowStockActivityId = c.Int(nullable: false),
                    NotifyAdminForQuantityBelow = c.Int(nullable: false),
                    BackorderModeId = c.Int(nullable: false),
                    AllowBackInStockSubscriptions = c.Boolean(nullable: false),
                    OrderMinimumQuantity = c.Int(nullable: false),
                    OrderMaximumQuantity = c.Int(nullable: false),
                    QuantityStep = c.Int(nullable: false),
                    QuantiyControlType = c.Int(nullable: false),
                    HideQuantityControl = c.Boolean(nullable: false),
                    AllowedQuantities = c.String(maxLength: 1000),
                    DisableBuyButton = c.Boolean(nullable: false),
                    DisableWishlistButton = c.Boolean(nullable: false),
                    AvailableForPreOrder = c.Boolean(nullable: false),
                    CallForPrice = c.Boolean(nullable: false),
                    Price = c.Decimal(nullable: false, precision: 18, scale: 4),
                    OldPrice = c.Decimal(nullable: false, precision: 18, scale: 4),
                    ProductCost = c.Decimal(nullable: false, precision: 18, scale: 4),
                    SpecialPrice = c.Decimal(precision: 18, scale: 4),
                    SpecialPriceStartDateTimeUtc = c.DateTime(),
                    SpecialPriceEndDateTimeUtc = c.DateTime(),
                    CustomerEntersPrice = c.Boolean(nullable: false),
                    MinimumCustomerEnteredPrice = c.Decimal(nullable: false, precision: 18, scale: 4),
                    MaximumCustomerEnteredPrice = c.Decimal(nullable: false, precision: 18, scale: 4),
                    HasTierPrices = c.Boolean(nullable: false),
                    LowestAttributeCombinationPrice = c.Decimal(precision: 18, scale: 4),
                    HasDiscountsApplied = c.Boolean(nullable: false),
                    Weight = c.Decimal(nullable: false, precision: 18, scale: 4),
                    Length = c.Decimal(nullable: false, precision: 18, scale: 4),
                    Width = c.Decimal(nullable: false, precision: 18, scale: 4),
                    Height = c.Decimal(nullable: false, precision: 18, scale: 4),
                    AvailableStartDateTimeUtc = c.DateTime(),
                    AvailableEndDateTimeUtc = c.DateTime(),
                    DisplayOrder = c.Int(nullable: false),
                    Published = c.Boolean(nullable: false),
                    Deleted = c.Boolean(nullable: false),
                    CreatedOnUtc = c.DateTime(nullable: false),
                    UpdatedOnUtc = c.DateTime(nullable: false),
                    DeliveryTimeId = c.Int(),
                    QuantityUnitId = c.Int(),
                    CustomsTariffNumber = c.String(maxLength: 30),
                    CountryOfOriginId = c.Int(),
                    BasePriceEnabled = c.Boolean(nullable: false),
                    BasePriceMeasureUnit = c.String(maxLength: 50),
                    BasePriceAmount = c.Decimal(precision: 18, scale: 4),
                    BasePriceBaseAmount = c.Int(),
                    BundleTitleText = c.String(maxLength: 400),
                    BundlePerItemShipping = c.Boolean(nullable: false),
                    BundlePerItemPricing = c.Boolean(nullable: false),
                    BundlePerItemShoppingCart = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Country", t => t.CountryOfOriginId)
                .ForeignKey("dbo.DeliveryTime", t => t.DeliveryTimeId)
                .ForeignKey("dbo.QuantityUnit", t => t.QuantityUnitId)
                .ForeignKey("dbo.Download", t => t.SampleDownloadId)
                .Index(t => t.ManufacturerPartNumber)
                .Index(t => t.Gtin)
                .Index(t => t.SampleDownloadId)
                .Index(t => t.Deleted)
                .Index(t => t.DeliveryTimeId)
                .Index(t => t.QuantityUnitId)
                .Index(t => t.CountryOfOriginId);

            CreateTable(
                "dbo.Discount",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(nullable: false, maxLength: 200),
                    DiscountTypeId = c.Int(nullable: false),
                    UsePercentage = c.Boolean(nullable: false),
                    DiscountPercentage = c.Decimal(nullable: false, precision: 18, scale: 4),
                    DiscountAmount = c.Decimal(nullable: false, precision: 18, scale: 4),
                    StartDateUtc = c.DateTime(),
                    EndDateUtc = c.DateTime(),
                    RequiresCouponCode = c.Boolean(nullable: false),
                    CouponCode = c.String(maxLength: 100),
                    DiscountLimitationId = c.Int(nullable: false),
                    LimitationTimes = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.Category",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(nullable: false, maxLength: 400),
                    FullName = c.String(maxLength: 400),
                    Description = c.String(),
                    BottomDescription = c.String(),
                    BadgeText = c.String(maxLength: 400),
                    BadgeStyle = c.Int(nullable: false),
                    Alias = c.String(maxLength: 100),
                    CategoryTemplateId = c.Int(nullable: false),
                    MetaKeywords = c.String(maxLength: 400),
                    MetaDescription = c.String(maxLength: 4000),
                    MetaTitle = c.String(maxLength: 400),
                    ParentCategoryId = c.Int(nullable: false),
                    PictureId = c.Int(),
                    PageSize = c.Int(),
                    AllowCustomersToSelectPageSize = c.Boolean(),
                    PageSizeOptions = c.String(maxLength: 200),
                    PriceRanges = c.String(maxLength: 400),
                    ShowOnHomePage = c.Boolean(nullable: false),
                    HasDiscountsApplied = c.Boolean(nullable: false),
                    SubjectToAcl = c.Boolean(nullable: false),
                    LimitedToStores = c.Boolean(nullable: false),
                    Published = c.Boolean(nullable: false),
                    Deleted = c.Boolean(nullable: false),
                    DisplayOrder = c.Int(nullable: false),
                    CreatedOnUtc = c.DateTime(nullable: false),
                    UpdatedOnUtc = c.DateTime(nullable: false),
                    DefaultViewMode = c.String(),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Picture", t => t.PictureId)
                .Index(t => t.PictureId)
                .Index(t => t.Deleted);

            CreateTable(
                "dbo.Picture",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    PictureBinary = c.Binary(),
                    MimeType = c.String(nullable: false, maxLength: 40),
                    Width = c.Int(),
                    Height = c.Int(),
                    SeoFilename = c.String(maxLength: 300),
                    IsNew = c.Boolean(nullable: false),
                    IsTransient = c.Boolean(nullable: false),
                    UpdatedOnUtc = c.DateTime(nullable: false),
                    MediaStorageId = c.Int(),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MediaStorage", t => t.MediaStorageId)
                .Index(t => new { t.UpdatedOnUtc, t.IsTransient }, name: "IX_UpdatedOn_IsTransient")
                .Index(t => t.MediaStorageId);

            CreateTable(
                "dbo.MediaStorage",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Data = c.Binary(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.Product_Picture_Mapping",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    ProductId = c.Int(nullable: false),
                    PictureId = c.Int(nullable: false),
                    DisplayOrder = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Picture", t => t.PictureId, cascadeDelete: true)
                .ForeignKey("dbo.Product", t => t.ProductId, cascadeDelete: true)
                .Index(t => t.ProductId)
                .Index(t => t.PictureId);

            CreateTable(
                "dbo.Manufacturer",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(nullable: false, maxLength: 400),
                    Description = c.String(),
                    ManufacturerTemplateId = c.Int(nullable: false),
                    MetaKeywords = c.String(maxLength: 400),
                    MetaDescription = c.String(maxLength: 4000),
                    MetaTitle = c.String(maxLength: 400),
                    PictureId = c.Int(),
                    PageSize = c.Int(),
                    AllowCustomersToSelectPageSize = c.Boolean(),
                    PageSizeOptions = c.String(maxLength: 200),
                    PriceRanges = c.String(maxLength: 400),
                    LimitedToStores = c.Boolean(nullable: false),
                    Published = c.Boolean(nullable: false),
                    Deleted = c.Boolean(nullable: false),
                    DisplayOrder = c.Int(nullable: false),
                    CreatedOnUtc = c.DateTime(nullable: false),
                    UpdatedOnUtc = c.DateTime(nullable: false),
                    HasDiscountsApplied = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Picture", t => t.PictureId)
                .Index(t => t.PictureId)
                .Index(t => t.Deleted);

            // Obsolete. Is now implemented by rule builder.
            //CreateTable(
            //    "dbo.DiscountRequirement",
            //    c => new
            //    {
            //        Id = c.Int(nullable: false, identity: true),
            //        DiscountId = c.Int(nullable: false),
            //        DiscountRequirementRuleSystemName = c.String(),
            //        SpentAmount = c.Decimal(nullable: false, precision: 18, scale: 4),
            //        BillingCountryId = c.Int(nullable: false),
            //        ShippingCountryId = c.Int(nullable: false),
            //        RestrictedToCustomerRoleId = c.Int(),
            //        RestrictedProductIds = c.String(),
            //        RestrictedPaymentMethods = c.String(),
            //        RestrictedShippingOptions = c.String(),
            //        RestrictedToStoreId = c.Int(),
            //        ExtraData = c.String(),
            //    })
            //    .PrimaryKey(t => t.Id)
            //    .ForeignKey("dbo.Discount", t => t.DiscountId, cascadeDelete: true)
            //    .Index(t => t.DiscountId);

            CreateTable(
                "dbo.DeliveryTime",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(nullable: false, maxLength: 50),
                    ColorHexValue = c.String(nullable: false, maxLength: 50),
                    DisplayLocale = c.String(maxLength: 50),
                    DisplayOrder = c.Int(nullable: false),
                    IsDefault = c.Boolean(),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.ProductBundleItem",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    ProductId = c.Int(nullable: false),
                    BundleProductId = c.Int(nullable: false),
                    Quantity = c.Int(nullable: false),
                    Discount = c.Decimal(precision: 18, scale: 4),
                    DiscountPercentage = c.Boolean(nullable: false),
                    Name = c.String(maxLength: 400),
                    ShortDescription = c.String(),
                    FilterAttributes = c.Boolean(nullable: false),
                    HideThumbnail = c.Boolean(nullable: false),
                    Visible = c.Boolean(nullable: false),
                    Published = c.Boolean(nullable: false),
                    DisplayOrder = c.Int(nullable: false),
                    CreatedOnUtc = c.DateTime(nullable: false),
                    UpdatedOnUtc = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Product", t => t.BundleProductId, cascadeDelete: true)
                .ForeignKey("dbo.Product", t => t.ProductId)
                .Index(t => t.ProductId)
                .Index(t => t.BundleProductId);

            CreateTable(
                "dbo.ProductBundleItemAttributeFilter",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    BundleItemId = c.Int(nullable: false),
                    AttributeId = c.Int(nullable: false),
                    AttributeValueId = c.Int(nullable: false),
                    IsPreSelected = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ProductBundleItem", t => t.BundleItemId, cascadeDelete: true)
                .Index(t => t.BundleItemId);

            CreateTable(
                "dbo.Product_Category_Mapping",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    ProductId = c.Int(nullable: false),
                    CategoryId = c.Int(nullable: false),
                    IsFeaturedProduct = c.Boolean(nullable: false),
                    DisplayOrder = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Category", t => t.CategoryId, cascadeDelete: true)
                .ForeignKey("dbo.Product", t => t.ProductId, cascadeDelete: true)
                .Index(t => t.ProductId)
                .Index(t => t.CategoryId);

            CreateTable(
                "dbo.Product_Manufacturer_Mapping",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    ProductId = c.Int(nullable: false),
                    ManufacturerId = c.Int(nullable: false),
                    IsFeaturedProduct = c.Boolean(nullable: false),
                    DisplayOrder = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Manufacturer", t => t.ManufacturerId, cascadeDelete: true)
                .ForeignKey("dbo.Product", t => t.ProductId, cascadeDelete: true)
                .Index(t => t.ProductId)
                .Index(t => t.ManufacturerId);

            CreateTable(
                "dbo.Product_SpecificationAttribute_Mapping",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    ProductId = c.Int(nullable: false),
                    SpecificationAttributeOptionId = c.Int(nullable: false),
                    AllowFiltering = c.Boolean(),
                    ShowOnProductPage = c.Boolean(),
                    DisplayOrder = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Product", t => t.ProductId, cascadeDelete: true)
                .ForeignKey("dbo.SpecificationAttributeOption", t => t.SpecificationAttributeOptionId, cascadeDelete: true)
                .Index(t => t.ProductId)
                .Index(t => t.SpecificationAttributeOptionId);

            CreateTable(
                "dbo.SpecificationAttributeOption",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    SpecificationAttributeId = c.Int(nullable: false),
                    Name = c.String(nullable: false, maxLength: 4000),
                    Alias = c.String(maxLength: 30),
                    DisplayOrder = c.Int(nullable: false),
                    NumberValue = c.Decimal(nullable: false, precision: 18, scale: 4),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SpecificationAttribute", t => t.SpecificationAttributeId, cascadeDelete: true)
                .Index(t => t.SpecificationAttributeId);

            CreateTable(
                "dbo.SpecificationAttribute",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(nullable: false, maxLength: 4000),
                    Alias = c.String(maxLength: 30),
                    DisplayOrder = c.Int(nullable: false),
                    ShowOnProductPage = c.Boolean(nullable: false),
                    AllowFiltering = c.Boolean(nullable: false),
                    FacetSorting = c.Int(nullable: false),
                    FacetTemplateHint = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.ProductTag",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(nullable: false, maxLength: 400),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.ProductVariantAttributeCombination",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Sku = c.String(maxLength: 400),
                    Gtin = c.String(maxLength: 400),
                    ManufacturerPartNumber = c.String(maxLength: 400),
                    Price = c.Decimal(precision: 18, scale: 4),
                    Length = c.Decimal(precision: 18, scale: 4),
                    Width = c.Decimal(precision: 18, scale: 4),
                    Height = c.Decimal(precision: 18, scale: 4),
                    BasePriceAmount = c.Decimal(precision: 18, scale: 4),
                    BasePriceBaseAmount = c.Int(),
                    AssignedPictureIds = c.String(maxLength: 1000),
                    DeliveryTimeId = c.Int(),
                    QuantityUnitId = c.Int(),
                    IsActive = c.Boolean(nullable: false),
                    ProductId = c.Int(nullable: false),
                    AttributesXml = c.String(),
                    StockQuantity = c.Int(nullable: false),
                    AllowOutOfStockOrders = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DeliveryTime", t => t.DeliveryTimeId)
                .ForeignKey("dbo.Product", t => t.ProductId, cascadeDelete: true)
                .ForeignKey("dbo.QuantityUnit", t => t.QuantityUnitId)
                .Index(t => t.DeliveryTimeId)
                .Index(t => t.QuantityUnitId)
                .Index(t => t.ProductId);

            CreateTable(
                "dbo.QuantityUnit",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(nullable: false, maxLength: 50),
                    Description = c.String(maxLength: 50),
                    DisplayLocale = c.String(maxLength: 50),
                    DisplayOrder = c.Int(nullable: false),
                    IsDefault = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.Product_ProductAttribute_Mapping",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    ProductId = c.Int(nullable: false),
                    ProductAttributeId = c.Int(nullable: false),
                    TextPrompt = c.String(maxLength: 4000),
                    IsRequired = c.Boolean(nullable: false),
                    AttributeControlTypeId = c.Int(nullable: false),
                    DisplayOrder = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Product", t => t.ProductId, cascadeDelete: true)
                .ForeignKey("dbo.ProductAttribute", t => t.ProductAttributeId, cascadeDelete: true)
                .Index(t => new { t.ProductId, t.DisplayOrder }, name: "IX_Product_ProductAttribute_Mapping_ProductId_DisplayOrder")
                .Index(t => t.ProductAttributeId);

            CreateTable(
                "dbo.ProductAttribute",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Alias = c.String(maxLength: 100),
                    Name = c.String(nullable: false, maxLength: 4000),
                    Description = c.String(maxLength: 4000),
                    AllowFiltering = c.Boolean(nullable: false),
                    DisplayOrder = c.Int(nullable: false),
                    FacetTemplateHint = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.ProductAttributeOptionsSet",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(maxLength: 400),
                    ProductAttributeId = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ProductAttribute", t => t.ProductAttributeId, cascadeDelete: true)
                .Index(t => t.ProductAttributeId);

            CreateTable(
                "dbo.ProductAttributeOption",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    ProductAttributeOptionsSetId = c.Int(nullable: false),
                    Alias = c.String(maxLength: 100),
                    Name = c.String(maxLength: 4000),
                    PictureId = c.Int(nullable: false),
                    Color = c.String(maxLength: 100),
                    PriceAdjustment = c.Decimal(nullable: false, precision: 18, scale: 4),
                    WeightAdjustment = c.Decimal(nullable: false, precision: 18, scale: 4),
                    IsPreSelected = c.Boolean(nullable: false),
                    DisplayOrder = c.Int(nullable: false),
                    ValueTypeId = c.Int(nullable: false),
                    LinkedProductId = c.Int(nullable: false),
                    Quantity = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ProductAttributeOptionsSet", t => t.ProductAttributeOptionsSetId, cascadeDelete: true)
                .Index(t => t.ProductAttributeOptionsSetId);

            CreateTable(
                "dbo.ProductVariantAttributeValue",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    ProductVariantAttributeId = c.Int(nullable: false),
                    Alias = c.String(maxLength: 100),
                    Name = c.String(maxLength: 4000),
                    PictureId = c.Int(nullable: false),
                    Color = c.String(maxLength: 100),
                    PriceAdjustment = c.Decimal(nullable: false, precision: 18, scale: 4),
                    WeightAdjustment = c.Decimal(nullable: false, precision: 18, scale: 4),
                    IsPreSelected = c.Boolean(nullable: false),
                    DisplayOrder = c.Int(nullable: false),
                    ValueTypeId = c.Int(nullable: false),
                    LinkedProductId = c.Int(nullable: false),
                    Quantity = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Product_ProductAttribute_Mapping", t => t.ProductVariantAttributeId, cascadeDelete: true)
                .Index(t => new { t.ProductVariantAttributeId, t.DisplayOrder }, name: "IX_ProductVariantAttributeValue_ProductVariantAttributeId_DisplayOrder");

            CreateTable(
                "dbo.Download",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    DownloadGuid = c.Guid(nullable: false),
                    UseDownloadUrl = c.Boolean(nullable: false),
                    DownloadUrl = c.String(maxLength: 4000),
                    DownloadBinary = c.Binary(),
                    ContentType = c.String(maxLength: 4000),
                    Filename = c.String(maxLength: 4000),
                    Extension = c.String(maxLength: 4000),
                    IsNew = c.Boolean(nullable: false),
                    IsTransient = c.Boolean(nullable: false),
                    UpdatedOnUtc = c.DateTime(nullable: false),
                    MediaStorageId = c.Int(),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MediaStorage", t => t.MediaStorageId)
                .Index(t => t.DownloadGuid)
                .Index(t => new { t.UpdatedOnUtc, t.IsTransient }, name: "IX_UpdatedOn_IsTransient")
                .Index(t => t.MediaStorageId);

            CreateTable(
                "dbo.TierPrice",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    ProductId = c.Int(nullable: false),
                    StoreId = c.Int(nullable: false),
                    CustomerRoleId = c.Int(),
                    Quantity = c.Int(nullable: false),
                    Price = c.Decimal(nullable: false, precision: 18, scale: 4),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CustomerRole", t => t.CustomerRoleId, cascadeDelete: true)
                .ForeignKey("dbo.Product", t => t.ProductId, cascadeDelete: true)
                .Index(t => t.ProductId)
                .Index(t => t.CustomerRoleId);

            CreateTable(
                "dbo.News",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    LanguageId = c.Int(nullable: false),
                    Title = c.String(nullable: false, maxLength: 4000),
                    Short = c.String(nullable: false, maxLength: 4000),
                    Full = c.String(nullable: false),
                    Published = c.Boolean(nullable: false),
                    StartDateUtc = c.DateTime(),
                    EndDateUtc = c.DateTime(),
                    AllowComments = c.Boolean(nullable: false),
                    ApprovedCommentCount = c.Int(nullable: false),
                    NotApprovedCommentCount = c.Int(nullable: false),
                    LimitedToStores = c.Boolean(nullable: false),
                    CreatedOnUtc = c.DateTime(nullable: false),
                    MetaKeywords = c.String(maxLength: 400),
                    MetaDescription = c.String(maxLength: 4000),
                    MetaTitle = c.String(maxLength: 400),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Language", t => t.LanguageId, cascadeDelete: true)
                .Index(t => t.LanguageId);

            CreateTable(
                "dbo.ExternalAuthenticationRecord",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    CustomerId = c.Int(nullable: false),
                    Email = c.String(maxLength: 4000),
                    ExternalIdentifier = c.String(maxLength: 4000),
                    ExternalDisplayIdentifier = c.String(maxLength: 4000),
                    OAuthToken = c.String(maxLength: 4000),
                    OAuthAccessToken = c.String(maxLength: 4000),
                    ProviderSystemName = c.String(maxLength: 4000),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Customer", t => t.CustomerId, cascadeDelete: true)
                .Index(t => t.CustomerId);

            CreateTable(
                "dbo.Forums_Post",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    TopicId = c.Int(nullable: false),
                    CustomerId = c.Int(nullable: false),
                    Text = c.String(nullable: false),
                    IPAddress = c.String(maxLength: 100),
                    CreatedOnUtc = c.DateTime(nullable: false),
                    UpdatedOnUtc = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Customer", t => t.CustomerId)
                .ForeignKey("dbo.Forums_Topic", t => t.TopicId, cascadeDelete: true)
                .Index(t => t.TopicId)
                .Index(t => t.CustomerId);

            CreateTable(
                "dbo.Forums_Topic",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    ForumId = c.Int(nullable: false),
                    CustomerId = c.Int(nullable: false),
                    TopicTypeId = c.Int(nullable: false),
                    Subject = c.String(nullable: false, maxLength: 450),
                    NumPosts = c.Int(nullable: false),
                    Views = c.Int(nullable: false),
                    LastPostId = c.Int(nullable: false),
                    LastPostCustomerId = c.Int(nullable: false),
                    LastPostTime = c.DateTime(),
                    CreatedOnUtc = c.DateTime(nullable: false),
                    UpdatedOnUtc = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Customer", t => t.CustomerId)
                .ForeignKey("dbo.Forums_Forum", t => t.ForumId, cascadeDelete: true)
                .Index(t => t.ForumId)
                .Index(t => t.CustomerId);

            CreateTable(
                "dbo.Forums_Forum",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    ForumGroupId = c.Int(nullable: false),
                    Name = c.String(nullable: false, maxLength: 200),
                    Description = c.String(),
                    NumTopics = c.Int(nullable: false),
                    NumPosts = c.Int(nullable: false),
                    LastTopicId = c.Int(nullable: false),
                    LastPostId = c.Int(nullable: false),
                    LastPostCustomerId = c.Int(nullable: false),
                    LastPostTime = c.DateTime(),
                    DisplayOrder = c.Int(nullable: false),
                    CreatedOnUtc = c.DateTime(nullable: false),
                    UpdatedOnUtc = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Forums_Group", t => t.ForumGroupId, cascadeDelete: true)
                .Index(t => t.ForumGroupId);

            CreateTable(
                "dbo.Forums_Group",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(nullable: false, maxLength: 200),
                    Description = c.String(),
                    DisplayOrder = c.Int(nullable: false),
                    CreatedOnUtc = c.DateTime(nullable: false),
                    UpdatedOnUtc = c.DateTime(nullable: false),
                    LimitedToStores = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.Order",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    OrderNumber = c.String(maxLength: 4000),
                    OrderGuid = c.Guid(nullable: false),
                    StoreId = c.Int(nullable: false),
                    CustomerId = c.Int(nullable: false),
                    BillingAddressId = c.Int(nullable: false),
                    ShippingAddressId = c.Int(),
                    OrderStatusId = c.Int(nullable: false),
                    ShippingStatusId = c.Int(nullable: false),
                    PaymentStatusId = c.Int(nullable: false),
                    PaymentMethodSystemName = c.String(maxLength: 4000),
                    CustomerCurrencyCode = c.String(maxLength: 4000),
                    CurrencyRate = c.Decimal(nullable: false, precision: 18, scale: 8),
                    CustomerTaxDisplayTypeId = c.Int(nullable: false),
                    VatNumber = c.String(maxLength: 4000),
                    OrderSubtotalInclTax = c.Decimal(nullable: false, precision: 18, scale: 4),
                    OrderSubtotalExclTax = c.Decimal(nullable: false, precision: 18, scale: 4),
                    OrderSubTotalDiscountInclTax = c.Decimal(nullable: false, precision: 18, scale: 4),
                    OrderSubTotalDiscountExclTax = c.Decimal(nullable: false, precision: 18, scale: 4),
                    OrderShippingInclTax = c.Decimal(nullable: false, precision: 18, scale: 4),
                    OrderShippingExclTax = c.Decimal(nullable: false, precision: 18, scale: 4),
                    OrderShippingTaxRate = c.Decimal(nullable: false, precision: 18, scale: 4),
                    PaymentMethodAdditionalFeeInclTax = c.Decimal(nullable: false, precision: 18, scale: 4),
                    PaymentMethodAdditionalFeeExclTax = c.Decimal(nullable: false, precision: 18, scale: 4),
                    PaymentMethodAdditionalFeeTaxRate = c.Decimal(nullable: false, precision: 18, scale: 4),
                    TaxRates = c.String(maxLength: 4000),
                    OrderTax = c.Decimal(nullable: false, precision: 18, scale: 4),
                    OrderDiscount = c.Decimal(nullable: false, precision: 18, scale: 4),
                    OrderTotal = c.Decimal(nullable: false, precision: 18, scale: 4),
                    RefundedAmount = c.Decimal(nullable: false, precision: 18, scale: 4),
                    RewardPointsWereAdded = c.Boolean(nullable: false),
                    CheckoutAttributeDescription = c.String(),
                    CheckoutAttributesXml = c.String(),
                    CustomerLanguageId = c.Int(nullable: false),
                    AffiliateId = c.Int(nullable: false),
                    CustomerIp = c.String(maxLength: 4000),
                    AllowStoringCreditCardNumber = c.Boolean(nullable: false),
                    CardType = c.String(maxLength: 4000),
                    CardName = c.String(maxLength: 4000),
                    CardNumber = c.String(maxLength: 4000),
                    MaskedCreditCardNumber = c.String(maxLength: 4000),
                    CardCvv2 = c.String(maxLength: 4000),
                    CardExpirationMonth = c.String(maxLength: 4000),
                    CardExpirationYear = c.String(maxLength: 4000),
                    AllowStoringDirectDebit = c.Boolean(nullable: false),
                    DirectDebitAccountHolder = c.String(maxLength: 4000),
                    DirectDebitAccountNumber = c.String(maxLength: 4000),
                    DirectDebitBankCode = c.String(maxLength: 4000),
                    DirectDebitBankName = c.String(maxLength: 4000),
                    DirectDebitBIC = c.String(maxLength: 4000),
                    DirectDebitCountry = c.String(maxLength: 4000),
                    DirectDebitIban = c.String(maxLength: 4000),
                    CustomerOrderComment = c.String(),
                    AuthorizationTransactionId = c.String(maxLength: 4000),
                    AuthorizationTransactionCode = c.String(maxLength: 4000),
                    AuthorizationTransactionResult = c.String(maxLength: 4000),
                    CaptureTransactionId = c.String(maxLength: 4000),
                    CaptureTransactionResult = c.String(maxLength: 4000),
                    SubscriptionTransactionId = c.String(maxLength: 4000),
                    PurchaseOrderNumber = c.String(maxLength: 4000),
                    PaidDateUtc = c.DateTime(),
                    ShippingMethod = c.String(maxLength: 4000),
                    ShippingRateComputationMethodSystemName = c.String(maxLength: 4000),
                    Deleted = c.Boolean(nullable: false),
                    CreatedOnUtc = c.DateTime(nullable: false),
                    UpdatedOnUtc = c.DateTime(nullable: false),
                    RewardPointsRemaining = c.Int(),
                    HasNewPaymentNotification = c.Boolean(nullable: false),
                    AcceptThirdPartyEmailHandOver = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Address", t => t.BillingAddressId)
                .ForeignKey("dbo.Customer", t => t.CustomerId, cascadeDelete: true)
                .ForeignKey("dbo.Address", t => t.ShippingAddressId)
                .Index(t => t.CustomerId)
                .Index(t => t.BillingAddressId)
                .Index(t => t.ShippingAddressId)
                .Index(t => t.Deleted);

            CreateTable(
                "dbo.DiscountUsageHistory",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    DiscountId = c.Int(nullable: false),
                    OrderId = c.Int(nullable: false),
                    CreatedOnUtc = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Discount", t => t.DiscountId, cascadeDelete: true)
                .ForeignKey("dbo.Order", t => t.OrderId, cascadeDelete: true)
                .Index(t => t.DiscountId)
                .Index(t => t.OrderId);

            CreateTable(
                "dbo.GiftCardUsageHistory",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    GiftCardId = c.Int(nullable: false),
                    UsedWithOrderId = c.Int(nullable: false),
                    UsedValue = c.Decimal(nullable: false, precision: 18, scale: 4),
                    CreatedOnUtc = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.GiftCard", t => t.GiftCardId, cascadeDelete: true)
                .ForeignKey("dbo.Order", t => t.UsedWithOrderId, cascadeDelete: true)
                .Index(t => t.GiftCardId)
                .Index(t => t.UsedWithOrderId);

            CreateTable(
                "dbo.GiftCard",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    PurchasedWithOrderItemId = c.Int(),
                    GiftCardTypeId = c.Int(nullable: false),
                    Amount = c.Decimal(nullable: false, precision: 18, scale: 4),
                    IsGiftCardActivated = c.Boolean(nullable: false),
                    GiftCardCouponCode = c.String(maxLength: 4000),
                    RecipientName = c.String(maxLength: 4000),
                    RecipientEmail = c.String(maxLength: 4000),
                    SenderName = c.String(maxLength: 4000),
                    SenderEmail = c.String(maxLength: 4000),
                    Message = c.String(maxLength: 4000),
                    IsRecipientNotified = c.Boolean(nullable: false),
                    CreatedOnUtc = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.OrderItem", t => t.PurchasedWithOrderItemId)
                .Index(t => t.PurchasedWithOrderItemId);

            CreateTable(
                "dbo.OrderItem",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    OrderItemGuid = c.Guid(nullable: false),
                    OrderId = c.Int(nullable: false),
                    ProductId = c.Int(nullable: false),
                    Quantity = c.Int(nullable: false),
                    UnitPriceInclTax = c.Decimal(nullable: false, precision: 18, scale: 4),
                    UnitPriceExclTax = c.Decimal(nullable: false, precision: 18, scale: 4),
                    PriceInclTax = c.Decimal(nullable: false, precision: 18, scale: 4),
                    PriceExclTax = c.Decimal(nullable: false, precision: 18, scale: 4),
                    TaxRate = c.Decimal(nullable: false, precision: 18, scale: 4),
                    DiscountAmountInclTax = c.Decimal(nullable: false, precision: 18, scale: 4),
                    DiscountAmountExclTax = c.Decimal(nullable: false, precision: 18, scale: 4),
                    AttributeDescription = c.String(),
                    AttributesXml = c.String(),
                    DownloadCount = c.Int(nullable: false),
                    IsDownloadActivated = c.Boolean(nullable: false),
                    LicenseDownloadId = c.Int(),
                    ItemWeight = c.Decimal(precision: 18, scale: 4),
                    BundleData = c.String(),
                    ProductCost = c.Decimal(nullable: false, precision: 18, scale: 4),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Order", t => t.OrderId, cascadeDelete: true)
                .ForeignKey("dbo.Product", t => t.ProductId, cascadeDelete: true)
                .Index(t => t.OrderId)
                .Index(t => t.ProductId);

            CreateTable(
                "dbo.OrderNote",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    OrderId = c.Int(nullable: false),
                    Note = c.String(nullable: false),
                    DisplayToCustomer = c.Boolean(nullable: false),
                    CreatedOnUtc = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Order", t => t.OrderId, cascadeDelete: true)
                .Index(t => t.OrderId);

            CreateTable(
                "dbo.RewardPointsHistory",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    CustomerId = c.Int(nullable: false),
                    Points = c.Int(nullable: false),
                    PointsBalance = c.Int(nullable: false),
                    UsedAmount = c.Decimal(nullable: false, precision: 18, scale: 4),
                    Message = c.String(maxLength: 4000),
                    CreatedOnUtc = c.DateTime(nullable: false),
                    UsedWithOrder_Id = c.Int(),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Customer", t => t.CustomerId, cascadeDelete: true)
                .ForeignKey("dbo.Order", t => t.UsedWithOrder_Id)
                .Index(t => t.CustomerId)
                .Index(t => t.UsedWithOrder_Id);

            CreateTable(
                "dbo.Shipment",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    OrderId = c.Int(nullable: false),
                    TrackingNumber = c.String(maxLength: 4000),
                    TotalWeight = c.Decimal(precision: 18, scale: 4),
                    ShippedDateUtc = c.DateTime(),
                    DeliveryDateUtc = c.DateTime(),
                    CreatedOnUtc = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Order", t => t.OrderId, cascadeDelete: true)
                .Index(t => t.OrderId);

            CreateTable(
                "dbo.ShipmentItem",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    ShipmentId = c.Int(nullable: false),
                    OrderItemId = c.Int(nullable: false),
                    Quantity = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Shipment", t => t.ShipmentId, cascadeDelete: true)
                .Index(t => t.ShipmentId);

            CreateTable(
                "dbo.ReturnRequest",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    StoreId = c.Int(nullable: false),
                    OrderItemId = c.Int(nullable: false),
                    CustomerId = c.Int(nullable: false),
                    Quantity = c.Int(nullable: false),
                    ReasonForReturn = c.String(nullable: false, maxLength: 4000),
                    RequestedAction = c.String(nullable: false, maxLength: 4000),
                    RequestedActionUpdatedOnUtc = c.DateTime(),
                    CustomerComments = c.String(maxLength: 4000),
                    StaffNotes = c.String(maxLength: 4000),
                    AdminComment = c.String(maxLength: 4000),
                    ReturnRequestStatusId = c.Int(nullable: false),
                    CreatedOnUtc = c.DateTime(nullable: false),
                    UpdatedOnUtc = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Customer", t => t.CustomerId, cascadeDelete: true)
                .Index(t => t.CustomerId);

            CreateTable(
                "dbo.ShoppingCartItem",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    StoreId = c.Int(nullable: false),
                    ParentItemId = c.Int(),
                    BundleItemId = c.Int(),
                    ShoppingCartTypeId = c.Int(nullable: false),
                    CustomerId = c.Int(nullable: false),
                    ProductId = c.Int(nullable: false),
                    AttributesXml = c.String(),
                    CustomerEnteredPrice = c.Decimal(nullable: false, precision: 18, scale: 4),
                    Quantity = c.Int(nullable: false),
                    CreatedOnUtc = c.DateTime(nullable: false),
                    UpdatedOnUtc = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ProductBundleItem", t => t.BundleItemId)
                .ForeignKey("dbo.Customer", t => t.CustomerId, cascadeDelete: true)
                .ForeignKey("dbo.Product", t => t.ProductId, cascadeDelete: true)
                .Index(t => t.BundleItemId)
                .Index(t => t.CustomerId)
                .Index(t => t.ProductId);

            CreateTable(
                "dbo.CheckoutAttribute",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(nullable: false, maxLength: 400),
                    TextPrompt = c.String(maxLength: 4000),
                    IsRequired = c.Boolean(nullable: false),
                    ShippableProductRequired = c.Boolean(nullable: false),
                    IsTaxExempt = c.Boolean(nullable: false),
                    TaxCategoryId = c.Int(nullable: false),
                    AttributeControlTypeId = c.Int(nullable: false),
                    DisplayOrder = c.Int(nullable: false),
                    IsActive = c.Boolean(nullable: false),
                    LimitedToStores = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.CheckoutAttributeValue",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    CheckoutAttributeId = c.Int(nullable: false),
                    Name = c.String(nullable: false, maxLength: 400),
                    PriceAdjustment = c.Decimal(nullable: false, precision: 18, scale: 4),
                    WeightAdjustment = c.Decimal(nullable: false, precision: 18, scale: 4),
                    IsPreSelected = c.Boolean(nullable: false),
                    DisplayOrder = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CheckoutAttribute", t => t.CheckoutAttributeId, cascadeDelete: true)
                .Index(t => t.CheckoutAttributeId);

            CreateTable(
                "dbo.RecurringPaymentHistory",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    RecurringPaymentId = c.Int(nullable: false),
                    OrderId = c.Int(nullable: false),
                    CreatedOnUtc = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.RecurringPayment", t => t.RecurringPaymentId, cascadeDelete: true)
                .Index(t => t.RecurringPaymentId);

            CreateTable(
                "dbo.RecurringPayment",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    CycleLength = c.Int(nullable: false),
                    CyclePeriodId = c.Int(nullable: false),
                    TotalCycles = c.Int(nullable: false),
                    StartDateUtc = c.DateTime(nullable: false),
                    IsActive = c.Boolean(nullable: false),
                    Deleted = c.Boolean(nullable: false),
                    InitialOrderId = c.Int(nullable: false),
                    CreatedOnUtc = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Order", t => t.InitialOrderId)
                .Index(t => t.InitialOrderId);

            CreateTable(
                "dbo.ActivityLog",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    ActivityLogTypeId = c.Int(nullable: false),
                    CustomerId = c.Int(nullable: false),
                    Comment = c.String(nullable: false),
                    CreatedOnUtc = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ActivityLogType", t => t.ActivityLogTypeId, cascadeDelete: true)
                .ForeignKey("dbo.Customer", t => t.CustomerId, cascadeDelete: true)
                .Index(t => t.ActivityLogTypeId)
                .Index(t => t.CustomerId);

            CreateTable(
                "dbo.ActivityLogType",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    SystemKeyword = c.String(nullable: false, maxLength: 100),
                    Name = c.String(nullable: false, maxLength: 200),
                    Enabled = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.Log",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    LogLevelId = c.Int(nullable: false),
                    ShortMessage = c.String(nullable: false, maxLength: 4000),
                    FullMessage = c.String(),
                    IpAddress = c.String(maxLength: 200),
                    CustomerId = c.Int(),
                    PageUrl = c.String(maxLength: 1500),
                    ReferrerUrl = c.String(maxLength: 1500),
                    CreatedOnUtc = c.DateTime(nullable: false),
                    Logger = c.String(nullable: false, maxLength: 400),
                    HttpMethod = c.String(maxLength: 10),
                    UserName = c.String(maxLength: 100),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Customer", t => t.CustomerId, cascadeDelete: true)
                .Index(t => t.LogLevelId, name: "IX_Log_Level")
                .Index(t => t.CustomerId)
                .Index(t => t.Logger, name: "IX_Log_Logger");

            CreateTable(
                "dbo.Forums_Subscription",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    SubscriptionGuid = c.Guid(nullable: false),
                    CustomerId = c.Int(nullable: false),
                    ForumId = c.Int(nullable: false),
                    TopicId = c.Int(nullable: false),
                    CreatedOnUtc = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Customer", t => t.CustomerId)
                .Index(t => t.CustomerId);

            CreateTable(
                "dbo.Forums_PrivateMessage",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    StoreId = c.Int(nullable: false),
                    FromCustomerId = c.Int(nullable: false),
                    ToCustomerId = c.Int(nullable: false),
                    Subject = c.String(nullable: false, maxLength: 450),
                    Text = c.String(nullable: false),
                    IsRead = c.Boolean(nullable: false),
                    IsDeletedByAuthor = c.Boolean(nullable: false),
                    IsDeletedByRecipient = c.Boolean(nullable: false),
                    CreatedOnUtc = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Customer", t => t.FromCustomerId)
                .ForeignKey("dbo.Customer", t => t.ToCustomerId)
                .Index(t => t.FromCustomerId)
                .Index(t => t.ToCustomerId);

            CreateTable(
                "dbo.Affiliate",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    AddressId = c.Int(nullable: false),
                    Deleted = c.Boolean(nullable: false),
                    Active = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Address", t => t.AddressId)
                .Index(t => t.AddressId);

            CreateTable(
                "dbo.ScheduleTask",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(nullable: false, maxLength: 500),
                    Alias = c.String(maxLength: 500),
                    CronExpression = c.String(maxLength: 1000),
                    Type = c.String(nullable: false, maxLength: 800),
                    Enabled = c.Boolean(nullable: false),
                    StopOnError = c.Boolean(nullable: false),
                    NextRunUtc = c.DateTime(),
                    LastStartUtc = c.DateTime(),
                    LastEndUtc = c.DateTime(),
                    LastSuccessUtc = c.DateTime(),
                    LastError = c.String(maxLength: 1000),
                    IsHidden = c.Boolean(nullable: false),
                    ProgressPercent = c.Int(),
                    ProgressMessage = c.String(maxLength: 1000),
                    RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Type)
                .Index(t => new { t.NextRunUtc, t.Enabled }, name: "IX_NextRun_Enabled")
                .Index(t => new { t.LastStartUtc, t.LastEndUtc }, name: "IX_LastStart_LastEnd");

            CreateTable(
                "dbo.AclRecord",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    EntityId = c.Int(nullable: false),
                    EntityName = c.String(nullable: false, maxLength: 400),
                    CustomerRoleId = c.Int(nullable: false),
                    IsIdle = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CustomerRole", t => t.CustomerRoleId, cascadeDelete: true)
                .Index(t => t.CustomerRoleId);

            CreateTable(
                "dbo.UrlRecord",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    EntityId = c.Int(nullable: false),
                    EntityName = c.String(nullable: false, maxLength: 400),
                    Slug = c.String(nullable: false, maxLength: 400),
                    IsActive = c.Boolean(nullable: false),
                    LanguageId = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.GenericAttribute",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    EntityId = c.Int(nullable: false),
                    KeyGroup = c.String(nullable: false, maxLength: 400),
                    Key = c.String(nullable: false, maxLength: 400),
                    Value = c.String(nullable: false),
                    StoreId = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Key, name: "IX_GenericAttribute_Key");

            CreateTable(
                "dbo.ThemeVariable",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Theme = c.String(maxLength: 400),
                    Name = c.String(maxLength: 400),
                    Value = c.String(maxLength: 2000),
                    StoreId = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.Store",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(nullable: false, maxLength: 400),
                    Url = c.String(nullable: false, maxLength: 400),
                    SslEnabled = c.Boolean(nullable: false),
                    SecureUrl = c.String(maxLength: 400),
                    Hosts = c.String(maxLength: 1000),
                    LogoPictureId = c.Int(nullable: false),
                    DisplayOrder = c.Int(nullable: false),
                    HtmlBodyId = c.String(maxLength: 4000),
                    ContentDeliveryNetwork = c.String(maxLength: 400),
                    PrimaryStoreCurrencyId = c.Int(nullable: false, defaultValue: 1),
                    PrimaryExchangeRateCurrencyId = c.Int(nullable: false, defaultValue: 1),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Currency", t => t.PrimaryExchangeRateCurrencyId)
                .ForeignKey("dbo.Currency", t => t.PrimaryStoreCurrencyId)
                .Index(t => t.PrimaryStoreCurrencyId)
                .Index(t => t.PrimaryExchangeRateCurrencyId);

            CreateTable(
                "dbo.Currency",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(nullable: false, maxLength: 50),
                    CurrencyCode = c.String(nullable: false, maxLength: 5),
                    Rate = c.Decimal(nullable: false, precision: 18, scale: 8),
                    DisplayLocale = c.String(maxLength: 50),
                    CustomFormatting = c.String(maxLength: 50),
                    LimitedToStores = c.Boolean(nullable: false),
                    Published = c.Boolean(nullable: false),
                    DisplayOrder = c.Int(nullable: false),
                    CreatedOnUtc = c.DateTime(nullable: false),
                    UpdatedOnUtc = c.DateTime(nullable: false),
                    DomainEndings = c.String(maxLength: 1000),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.StoreMapping",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    EntityId = c.Int(nullable: false),
                    EntityName = c.String(nullable: false, maxLength: 400),
                    StoreId = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.PaymentMethod",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    PaymentMethodSystemName = c.String(nullable: false, maxLength: 4000),
                    FullDescription = c.String(maxLength: 4000),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.QueuedEmailAttachment",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    QueuedEmailId = c.Int(nullable: false),
                    StorageLocation = c.Int(nullable: false),
                    Path = c.String(maxLength: 1000),
                    FileId = c.Int(),
                    Data = c.Binary(),
                    Name = c.String(nullable: false, maxLength: 200),
                    MimeType = c.String(nullable: false, maxLength: 200),
                    MediaStorageId = c.Int(),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Download", t => t.FileId, cascadeDelete: true)
                .ForeignKey("dbo.MediaStorage", t => t.MediaStorageId)
                .ForeignKey("dbo.QueuedEmail", t => t.QueuedEmailId, cascadeDelete: true)
                .Index(t => t.QueuedEmailId)
                .Index(t => t.FileId)
                .Index(t => t.MediaStorageId);

            CreateTable(
                "dbo.QueuedEmail",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Priority = c.Int(nullable: false),
                    From = c.String(nullable: false, maxLength: 500),
                    FromName = c.String(maxLength: 500),
                    To = c.String(nullable: false, maxLength: 500),
                    ToName = c.String(maxLength: 500),
                    ReplyTo = c.String(maxLength: 500),
                    ReplyToName = c.String(maxLength: 500),
                    CC = c.String(maxLength: 500),
                    Bcc = c.String(maxLength: 500),
                    Subject = c.String(maxLength: 1000),
                    Body = c.String(),
                    CreatedOnUtc = c.DateTime(nullable: false),
                    SentTries = c.Int(nullable: false),
                    SentOnUtc = c.DateTime(),
                    EmailAccountId = c.Int(nullable: false),
                    SendManually = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.EmailAccount", t => t.EmailAccountId, cascadeDelete: true)
                .Index(t => t.EmailAccountId);

            CreateTable(
                "dbo.EmailAccount",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Email = c.String(nullable: false, maxLength: 255),
                    DisplayName = c.String(maxLength: 255),
                    Host = c.String(nullable: false, maxLength: 255),
                    Port = c.Int(nullable: false),
                    Username = c.String(nullable: false, maxLength: 255),
                    Password = c.String(nullable: false, maxLength: 255),
                    EnableSsl = c.Boolean(nullable: false),
                    UseDefaultCredentials = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.Campaign",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(nullable: false, maxLength: 4000),
                    Subject = c.String(nullable: false, maxLength: 4000),
                    Body = c.String(nullable: false),
                    CreatedOnUtc = c.DateTime(nullable: false),
                    LimitedToStores = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.MessageTemplate",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(nullable: false, maxLength: 200),
                    BccEmailAddresses = c.String(maxLength: 200),
                    Subject = c.String(maxLength: 1000),
                    Body = c.String(),
                    IsActive = c.Boolean(nullable: false),
                    EmailAccountId = c.Int(nullable: false),
                    LimitedToStores = c.Boolean(nullable: false),
                    SendManually = c.Boolean(nullable: false),
                    Attachment1FileId = c.Int(),
                    Attachment2FileId = c.Int(),
                    Attachment3FileId = c.Int(),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.NewsLetterSubscription",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    NewsLetterSubscriptionGuid = c.Guid(nullable: false),
                    Email = c.String(nullable: false, maxLength: 255),
                    Active = c.Boolean(nullable: false),
                    CreatedOnUtc = c.DateTime(nullable: false),
                    StoreId = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .Index(t => new { t.Email, t.StoreId }, name: "IX_NewsletterSubscription_Email_StoreId");

            CreateTable(
                "dbo.MeasureWeight",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(nullable: false, maxLength: 100),
                    SystemKeyword = c.String(nullable: false, maxLength: 100),
                    Ratio = c.Decimal(nullable: false, precision: 18, scale: 8),
                    DisplayOrder = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.MeasureDimension",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(nullable: false, maxLength: 100),
                    SystemKeyword = c.String(nullable: false, maxLength: 100),
                    Ratio = c.Decimal(nullable: false, precision: 18, scale: 8),
                    DisplayOrder = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.ImportProfile",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(nullable: false, maxLength: 100),
                    FolderName = c.String(nullable: false, maxLength: 100),
                    FileTypeId = c.Int(nullable: false),
                    EntityTypeId = c.Int(nullable: false),
                    Enabled = c.Boolean(nullable: false),
                    Skip = c.Int(nullable: false),
                    Take = c.Int(nullable: false),
                    UpdateOnly = c.Boolean(nullable: false),
                    KeyFieldNames = c.String(maxLength: 1000),
                    FileTypeConfiguration = c.String(),
                    ExtraData = c.String(),
                    ColumnMapping = c.String(),
                    ResultInfo = c.String(),
                    SchedulingTaskId = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ScheduleTask", t => t.SchedulingTaskId)
                .Index(t => t.SchedulingTaskId);

            CreateTable(
                "dbo.SyncMapping",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    EntityId = c.Int(nullable: false),
                    SourceKey = c.String(nullable: false, maxLength: 150),
                    EntityName = c.String(nullable: false, maxLength: 100),
                    ContextName = c.String(nullable: false, maxLength: 100),
                    SourceHash = c.String(maxLength: 40),
                    CustomInt = c.Int(),
                    CustomString = c.String(),
                    CustomBool = c.Boolean(),
                    SyncedOnUtc = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .Index(t => new { t.EntityId, t.EntityName, t.ContextName }, unique: true, name: "IX_SyncMapping_ByEntity")
                .Index(t => new { t.SourceKey, t.EntityName, t.ContextName }, unique: true, name: "IX_SyncMapping_BySource");

            CreateTable(
                "dbo.ExportDeployment",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    ProfileId = c.Int(nullable: false),
                    Name = c.String(nullable: false, maxLength: 100),
                    Enabled = c.Boolean(nullable: false),
                    ResultInfo = c.String(),
                    DeploymentTypeId = c.Int(nullable: false),
                    Username = c.String(maxLength: 400),
                    Password = c.String(maxLength: 400),
                    Url = c.String(maxLength: 4000),
                    HttpTransmissionTypeId = c.Int(nullable: false),
                    HttpTransmissionType = c.Int(nullable: false),
                    FileSystemPath = c.String(maxLength: 400),
                    SubFolder = c.String(maxLength: 400),
                    EmailAddresses = c.String(maxLength: 4000),
                    EmailSubject = c.String(maxLength: 400),
                    EmailAccountId = c.Int(nullable: false),
                    PassiveMode = c.Boolean(nullable: false),
                    UseSsl = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ExportProfile", t => t.ProfileId, cascadeDelete: true)
                .Index(t => t.ProfileId);

            CreateTable(
                "dbo.ExportProfile",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(nullable: false, maxLength: 100),
                    FolderName = c.String(nullable: false, maxLength: 400),
                    FileNamePattern = c.String(maxLength: 400),
                    SystemName = c.String(maxLength: 400),
                    ProviderSystemName = c.String(nullable: false, maxLength: 4000),
                    IsSystemProfile = c.Boolean(nullable: false),
                    Enabled = c.Boolean(nullable: false),
                    SchedulingTaskId = c.Int(nullable: false),
                    Filtering = c.String(),
                    Projection = c.String(),
                    ProviderConfigData = c.String(),
                    ResultInfo = c.String(),
                    Offset = c.Int(nullable: false),
                    Limit = c.Int(nullable: false),
                    BatchSize = c.Int(nullable: false),
                    PerStore = c.Boolean(nullable: false),
                    EmailAccountId = c.Int(nullable: false),
                    CompletedEmailAddresses = c.String(maxLength: 400),
                    CreateZipArchive = c.Boolean(nullable: false),
                    Cleanup = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ScheduleTask", t => t.SchedulingTaskId)
                .Index(t => t.SchedulingTaskId);

            CreateTable(
                "dbo.BackInStockSubscription",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    StoreId = c.Int(nullable: false),
                    ProductId = c.Int(nullable: false),
                    CustomerId = c.Int(nullable: false),
                    CreatedOnUtc = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Customer", t => t.CustomerId, cascadeDelete: true)
                .ForeignKey("dbo.Product", t => t.ProductId, cascadeDelete: true)
                .Index(t => t.ProductId)
                .Index(t => t.CustomerId);

            CreateTable(
                "dbo.ManufacturerTemplate",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(nullable: false, maxLength: 400),
                    ViewPath = c.String(nullable: false, maxLength: 400),
                    DisplayOrder = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.CategoryTemplate",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(nullable: false, maxLength: 400),
                    ViewPath = c.String(nullable: false, maxLength: 400),
                    DisplayOrder = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.ProductTemplate",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(nullable: false, maxLength: 400),
                    ViewPath = c.String(nullable: false, maxLength: 400),
                    DisplayOrder = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.RelatedProduct",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    ProductId1 = c.Int(nullable: false),
                    ProductId2 = c.Int(nullable: false),
                    DisplayOrder = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.CrossSellProduct",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    ProductId1 = c.Int(nullable: false),
                    ProductId2 = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            //CreateTable(
            //    "dbo.PermissionRecord_Role_Mapping",
            //    c => new
            //    {
            //        PermissionRecord_Id = c.Int(nullable: false),
            //        CustomerRole_Id = c.Int(nullable: false),
            //    })
            //    .PrimaryKey(t => new { t.PermissionRecord_Id, t.CustomerRole_Id })
            //    .ForeignKey("dbo.PermissionRecord", t => t.PermissionRecord_Id, cascadeDelete: true)
            //    .ForeignKey("dbo.CustomerRole", t => t.CustomerRole_Id, cascadeDelete: true)
            //    .Index(t => t.PermissionRecord_Id)
            //    .Index(t => t.CustomerRole_Id);

            CreateTable(
                "dbo.ShippingMethodRestrictions",
                c => new
                {
                    ShippingMethod_Id = c.Int(nullable: false),
                    Country_Id = c.Int(nullable: false),
                })
                .PrimaryKey(t => new { t.ShippingMethod_Id, t.Country_Id })
                .ForeignKey("dbo.ShippingMethod", t => t.ShippingMethod_Id, cascadeDelete: true)
                .ForeignKey("dbo.Country", t => t.Country_Id, cascadeDelete: true)
                .Index(t => t.ShippingMethod_Id)
                .Index(t => t.Country_Id);

            CreateTable(
                "dbo.CustomerAddresses",
                c => new
                {
                    Customer_Id = c.Int(nullable: false),
                    Address_Id = c.Int(nullable: false),
                })
                .PrimaryKey(t => new { t.Customer_Id, t.Address_Id })
                .ForeignKey("dbo.Customer", t => t.Customer_Id, cascadeDelete: true)
                .ForeignKey("dbo.Address", t => t.Address_Id, cascadeDelete: true)
                .Index(t => t.Customer_Id)
                .Index(t => t.Address_Id);

            CreateTable(
                "dbo.Discount_AppliedToCategories",
                c => new
                {
                    Discount_Id = c.Int(nullable: false),
                    Category_Id = c.Int(nullable: false),
                })
                .PrimaryKey(t => new { t.Discount_Id, t.Category_Id })
                .ForeignKey("dbo.Discount", t => t.Discount_Id, cascadeDelete: true)
                .ForeignKey("dbo.Category", t => t.Category_Id, cascadeDelete: true)
                .Index(t => t.Discount_Id)
                .Index(t => t.Category_Id);

            CreateTable(
                "dbo.Discount_AppliedToManufacturers",
                c => new
                {
                    Discount_Id = c.Int(nullable: false),
                    Manufacturer_Id = c.Int(nullable: false),
                })
                .PrimaryKey(t => new { t.Discount_Id, t.Manufacturer_Id })
                .ForeignKey("dbo.Discount", t => t.Discount_Id, cascadeDelete: true)
                .ForeignKey("dbo.Manufacturer", t => t.Manufacturer_Id, cascadeDelete: true)
                .Index(t => t.Discount_Id)
                .Index(t => t.Manufacturer_Id);

            CreateTable(
                "dbo.Discount_AppliedToProducts",
                c => new
                {
                    Discount_Id = c.Int(nullable: false),
                    Product_Id = c.Int(nullable: false),
                })
                .PrimaryKey(t => new { t.Discount_Id, t.Product_Id })
                .ForeignKey("dbo.Discount", t => t.Discount_Id, cascadeDelete: true)
                .ForeignKey("dbo.Product", t => t.Product_Id, cascadeDelete: true)
                .Index(t => t.Discount_Id)
                .Index(t => t.Product_Id);

            CreateTable(
                "dbo.Product_ProductTag_Mapping",
                c => new
                {
                    Product_Id = c.Int(nullable: false),
                    ProductTag_Id = c.Int(nullable: false),
                })
                .PrimaryKey(t => new { t.Product_Id, t.ProductTag_Id })
                .ForeignKey("dbo.Product", t => t.Product_Id, cascadeDelete: true)
                .ForeignKey("dbo.ProductTag", t => t.ProductTag_Id, cascadeDelete: true)
                .Index(t => t.Product_Id)
                .Index(t => t.ProductTag_Id);

            // Obsolete. Is now implemented by granular permissions.
            //CreateTable(
            //    "dbo.Customer_CustomerRole_Mapping",
            //    c => new
            //    {
            //        Customer_Id = c.Int(nullable: false),
            //        CustomerRole_Id = c.Int(nullable: false),
            //    })
            //    .PrimaryKey(t => new { t.Customer_Id, t.CustomerRole_Id })
            //    .ForeignKey("dbo.Customer", t => t.Customer_Id, cascadeDelete: true)
            //    .ForeignKey("dbo.CustomerRole", t => t.CustomerRole_Id, cascadeDelete: true)
            //    .Index(t => t.Customer_Id)
            //    .Index(t => t.CustomerRole_Id);

            CreateTable(
                "dbo.PollVotingRecord",
                c => new
                {
                    Id = c.Int(nullable: false),
                    PollAnswerId = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CustomerContent", t => t.Id)
                .ForeignKey("dbo.PollAnswer", t => t.PollAnswerId, cascadeDelete: true)
                .Index(t => t.Id)
                .Index(t => t.PollAnswerId);

            CreateTable(
                "dbo.NewsComment",
                c => new
                {
                    Id = c.Int(nullable: false),
                    CommentTitle = c.String(maxLength: 4000),
                    CommentText = c.String(),
                    NewsItemId = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CustomerContent", t => t.Id)
                .ForeignKey("dbo.News", t => t.NewsItemId, cascadeDelete: true)
                .Index(t => t.Id)
                .Index(t => t.NewsItemId);

            CreateTable(
                "dbo.BlogComment",
                c => new
                {
                    Id = c.Int(nullable: false),
                    CommentText = c.String(),
                    BlogPostId = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CustomerContent", t => t.Id)
                .ForeignKey("dbo.BlogPost", t => t.BlogPostId, cascadeDelete: true)
                .Index(t => t.Id)
                .Index(t => t.BlogPostId);

            CreateTable(
                "dbo.ProductReviewHelpfulness",
                c => new
                {
                    Id = c.Int(nullable: false),
                    ProductReviewId = c.Int(nullable: false),
                    WasHelpful = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CustomerContent", t => t.Id)
                .ForeignKey("dbo.ProductReview", t => t.ProductReviewId)
                .Index(t => t.Id)
                .Index(t => t.ProductReviewId);

            CreateTable(
                "dbo.ProductReview",
                c => new
                {
                    Id = c.Int(nullable: false),
                    ProductId = c.Int(nullable: false),
                    Title = c.String(maxLength: 4000),
                    ReviewText = c.String(),
                    Rating = c.Int(nullable: false),
                    HelpfulYesTotal = c.Int(nullable: false),
                    HelpfulNoTotal = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CustomerContent", t => t.Id)
                .ForeignKey("dbo.Product", t => t.ProductId, cascadeDelete: true)
                .Index(t => t.Id)
                .Index(t => t.ProductId);

            #endregion

            #region Custom

            this.SqlFileOrResource("Indexes.sql");
            if (HostingEnvironment.IsHosted && DataSettings.Current.IsSqlServer)
            {
                // do not execute in unit tests
                this.SqlFileOrResource("Indexes.SqlServer.sql");
                this.SqlFileOrResource("StoredProcedures.sql");
            }

            #endregion
        }

        private void DownInitial()
        {
            DropForeignKey("dbo.ProductReview", "ProductId", "dbo.Product");
            DropForeignKey("dbo.ProductReview", "Id", "dbo.CustomerContent");
            DropForeignKey("dbo.ProductReviewHelpfulness", "ProductReviewId", "dbo.ProductReview");
            DropForeignKey("dbo.ProductReviewHelpfulness", "Id", "dbo.CustomerContent");
            DropForeignKey("dbo.BlogComment", "BlogPostId", "dbo.BlogPost");
            DropForeignKey("dbo.BlogComment", "Id", "dbo.CustomerContent");
            DropForeignKey("dbo.NewsComment", "NewsItemId", "dbo.News");
            DropForeignKey("dbo.NewsComment", "Id", "dbo.CustomerContent");
            DropForeignKey("dbo.PollVotingRecord", "PollAnswerId", "dbo.PollAnswer");
            DropForeignKey("dbo.PollVotingRecord", "Id", "dbo.CustomerContent");
            DropForeignKey("dbo.BackInStockSubscription", "ProductId", "dbo.Product");
            DropForeignKey("dbo.BackInStockSubscription", "CustomerId", "dbo.Customer");
            DropForeignKey("dbo.ExportDeployment", "ProfileId", "dbo.ExportProfile");
            DropForeignKey("dbo.ExportProfile", "SchedulingTaskId", "dbo.ScheduleTask");
            DropForeignKey("dbo.ImportProfile", "SchedulingTaskId", "dbo.ScheduleTask");
            DropForeignKey("dbo.QueuedEmailAttachment", "QueuedEmailId", "dbo.QueuedEmail");
            DropForeignKey("dbo.QueuedEmail", "EmailAccountId", "dbo.EmailAccount");
            DropForeignKey("dbo.QueuedEmailAttachment", "MediaStorageId", "dbo.MediaStorage");
            DropForeignKey("dbo.QueuedEmailAttachment", "FileId", "dbo.Download");
            DropForeignKey("dbo.Store", "PrimaryStoreCurrencyId", "dbo.Currency");
            DropForeignKey("dbo.Store", "PrimaryExchangeRateCurrencyId", "dbo.Currency");
            DropForeignKey("dbo.AclRecord", "CustomerRoleId", "dbo.CustomerRole");
            DropForeignKey("dbo.Affiliate", "AddressId", "dbo.Address");
            DropForeignKey("dbo.Forums_PrivateMessage", "ToCustomerId", "dbo.Customer");
            DropForeignKey("dbo.Forums_PrivateMessage", "FromCustomerId", "dbo.Customer");
            DropForeignKey("dbo.Forums_Subscription", "CustomerId", "dbo.Customer");
            DropForeignKey("dbo.Log", "CustomerId", "dbo.Customer");
            DropForeignKey("dbo.ActivityLog", "CustomerId", "dbo.Customer");
            DropForeignKey("dbo.ActivityLog", "ActivityLogTypeId", "dbo.ActivityLogType");
            DropForeignKey("dbo.RecurringPaymentHistory", "RecurringPaymentId", "dbo.RecurringPayment");
            DropForeignKey("dbo.RecurringPayment", "InitialOrderId", "dbo.Order");
            DropForeignKey("dbo.CheckoutAttributeValue", "CheckoutAttributeId", "dbo.CheckoutAttribute");
            DropForeignKey("dbo.ShoppingCartItem", "ProductId", "dbo.Product");
            DropForeignKey("dbo.ShoppingCartItem", "CustomerId", "dbo.Customer");
            DropForeignKey("dbo.ShoppingCartItem", "BundleItemId", "dbo.ProductBundleItem");
            DropForeignKey("dbo.Customer", "ShippingAddress_Id", "dbo.Address");
            DropForeignKey("dbo.ReturnRequest", "CustomerId", "dbo.Customer");
            DropForeignKey("dbo.Order", "ShippingAddressId", "dbo.Address");
            DropForeignKey("dbo.ShipmentItem", "ShipmentId", "dbo.Shipment");
            DropForeignKey("dbo.Shipment", "OrderId", "dbo.Order");
            DropForeignKey("dbo.RewardPointsHistory", "UsedWithOrder_Id", "dbo.Order");
            DropForeignKey("dbo.RewardPointsHistory", "CustomerId", "dbo.Customer");
            DropForeignKey("dbo.OrderNote", "OrderId", "dbo.Order");
            DropForeignKey("dbo.GiftCardUsageHistory", "UsedWithOrderId", "dbo.Order");
            DropForeignKey("dbo.GiftCardUsageHistory", "GiftCardId", "dbo.GiftCard");
            DropForeignKey("dbo.GiftCard", "PurchasedWithOrderItemId", "dbo.OrderItem");
            DropForeignKey("dbo.OrderItem", "ProductId", "dbo.Product");
            DropForeignKey("dbo.OrderItem", "OrderId", "dbo.Order");
            DropForeignKey("dbo.DiscountUsageHistory", "OrderId", "dbo.Order");
            DropForeignKey("dbo.DiscountUsageHistory", "DiscountId", "dbo.Discount");
            DropForeignKey("dbo.Order", "CustomerId", "dbo.Customer");
            DropForeignKey("dbo.Order", "BillingAddressId", "dbo.Address");
            DropForeignKey("dbo.Forums_Post", "TopicId", "dbo.Forums_Topic");
            DropForeignKey("dbo.Forums_Topic", "ForumId", "dbo.Forums_Forum");
            DropForeignKey("dbo.Forums_Forum", "ForumGroupId", "dbo.Forums_Group");
            DropForeignKey("dbo.Forums_Topic", "CustomerId", "dbo.Customer");
            DropForeignKey("dbo.Forums_Post", "CustomerId", "dbo.Customer");
            DropForeignKey("dbo.ExternalAuthenticationRecord", "CustomerId", "dbo.Customer");
            //DropForeignKey("dbo.Customer_CustomerRole_Mapping", "CustomerRole_Id", "dbo.CustomerRole");
            //DropForeignKey("dbo.Customer_CustomerRole_Mapping", "Customer_Id", "dbo.Customer");
            DropForeignKey("dbo.News", "LanguageId", "dbo.Language");
            DropForeignKey("dbo.TierPrice", "ProductId", "dbo.Product");
            DropForeignKey("dbo.TierPrice", "CustomerRoleId", "dbo.CustomerRole");
            DropForeignKey("dbo.Product", "SampleDownloadId", "dbo.Download");
            DropForeignKey("dbo.Download", "MediaStorageId", "dbo.MediaStorage");
            DropForeignKey("dbo.Product", "QuantityUnitId", "dbo.QuantityUnit");
            DropForeignKey("dbo.ProductVariantAttributeValue", "ProductVariantAttributeId", "dbo.Product_ProductAttribute_Mapping");
            DropForeignKey("dbo.Product_ProductAttribute_Mapping", "ProductAttributeId", "dbo.ProductAttribute");
            DropForeignKey("dbo.ProductAttributeOption", "ProductAttributeOptionsSetId", "dbo.ProductAttributeOptionsSet");
            DropForeignKey("dbo.ProductAttributeOptionsSet", "ProductAttributeId", "dbo.ProductAttribute");
            DropForeignKey("dbo.Product_ProductAttribute_Mapping", "ProductId", "dbo.Product");
            DropForeignKey("dbo.ProductVariantAttributeCombination", "QuantityUnitId", "dbo.QuantityUnit");
            DropForeignKey("dbo.ProductVariantAttributeCombination", "ProductId", "dbo.Product");
            DropForeignKey("dbo.ProductVariantAttributeCombination", "DeliveryTimeId", "dbo.DeliveryTime");
            DropForeignKey("dbo.Product_ProductTag_Mapping", "ProductTag_Id", "dbo.ProductTag");
            DropForeignKey("dbo.Product_ProductTag_Mapping", "Product_Id", "dbo.Product");
            DropForeignKey("dbo.Product_SpecificationAttribute_Mapping", "SpecificationAttributeOptionId", "dbo.SpecificationAttributeOption");
            DropForeignKey("dbo.SpecificationAttributeOption", "SpecificationAttributeId", "dbo.SpecificationAttribute");
            DropForeignKey("dbo.Product_SpecificationAttribute_Mapping", "ProductId", "dbo.Product");
            DropForeignKey("dbo.Product_Manufacturer_Mapping", "ProductId", "dbo.Product");
            DropForeignKey("dbo.Product_Manufacturer_Mapping", "ManufacturerId", "dbo.Manufacturer");
            DropForeignKey("dbo.Product_Category_Mapping", "ProductId", "dbo.Product");
            DropForeignKey("dbo.Product_Category_Mapping", "CategoryId", "dbo.Category");
            DropForeignKey("dbo.ProductBundleItem", "ProductId", "dbo.Product");
            DropForeignKey("dbo.ProductBundleItem", "BundleProductId", "dbo.Product");
            DropForeignKey("dbo.ProductBundleItemAttributeFilter", "BundleItemId", "dbo.ProductBundleItem");
            DropForeignKey("dbo.Product", "DeliveryTimeId", "dbo.DeliveryTime");
            DropForeignKey("dbo.Product", "CountryOfOriginId", "dbo.Country");
            //DropForeignKey("dbo.DiscountRequirement", "DiscountId", "dbo.Discount");
            DropForeignKey("dbo.Discount_AppliedToProducts", "Product_Id", "dbo.Product");
            DropForeignKey("dbo.Discount_AppliedToProducts", "Discount_Id", "dbo.Discount");
            DropForeignKey("dbo.Discount_AppliedToManufacturers", "Manufacturer_Id", "dbo.Manufacturer");
            DropForeignKey("dbo.Discount_AppliedToManufacturers", "Discount_Id", "dbo.Discount");
            DropForeignKey("dbo.Manufacturer", "PictureId", "dbo.Picture");
            DropForeignKey("dbo.Discount_AppliedToCategories", "Category_Id", "dbo.Category");
            DropForeignKey("dbo.Discount_AppliedToCategories", "Discount_Id", "dbo.Discount");
            DropForeignKey("dbo.Category", "PictureId", "dbo.Picture");
            DropForeignKey("dbo.Product_Picture_Mapping", "ProductId", "dbo.Product");
            DropForeignKey("dbo.Product_Picture_Mapping", "PictureId", "dbo.Picture");
            DropForeignKey("dbo.Picture", "MediaStorageId", "dbo.MediaStorage");
            DropForeignKey("dbo.BlogPost", "LanguageId", "dbo.Language");
            DropForeignKey("dbo.CustomerContent", "CustomerId", "dbo.Customer");
            DropForeignKey("dbo.Customer", "BillingAddress_Id", "dbo.Address");
            DropForeignKey("dbo.CustomerAddresses", "Address_Id", "dbo.Address");
            DropForeignKey("dbo.CustomerAddresses", "Customer_Id", "dbo.Customer");
            DropForeignKey("dbo.Address", "StateProvinceId", "dbo.StateProvince");
            DropForeignKey("dbo.Address", "CountryId", "dbo.Country");
            DropForeignKey("dbo.StateProvince", "CountryId", "dbo.Country");
            DropForeignKey("dbo.ShippingMethodRestrictions", "Country_Id", "dbo.Country");
            DropForeignKey("dbo.ShippingMethodRestrictions", "ShippingMethod_Id", "dbo.ShippingMethod");
            DropForeignKey("dbo.PollAnswer", "PollId", "dbo.Poll");
            DropForeignKey("dbo.Poll", "LanguageId", "dbo.Language");
            DropForeignKey("dbo.PermissionRecord_Role_Mapping", "CustomerRole_Id", "dbo.CustomerRole");
            DropForeignKey("dbo.PermissionRecord_Role_Mapping", "PermissionRecord_Id", "dbo.PermissionRecord");
            DropForeignKey("dbo.LocalizedProperty", "LanguageId", "dbo.Language");
            DropForeignKey("dbo.LocaleStringResource", "LanguageId", "dbo.Language");
            DropIndex("dbo.ProductReview", new[] { "ProductId" });
            DropIndex("dbo.ProductReview", new[] { "Id" });
            DropIndex("dbo.ProductReviewHelpfulness", new[] { "ProductReviewId" });
            DropIndex("dbo.ProductReviewHelpfulness", new[] { "Id" });
            DropIndex("dbo.BlogComment", new[] { "BlogPostId" });
            DropIndex("dbo.BlogComment", new[] { "Id" });
            DropIndex("dbo.NewsComment", new[] { "NewsItemId" });
            DropIndex("dbo.NewsComment", new[] { "Id" });
            DropIndex("dbo.PollVotingRecord", new[] { "PollAnswerId" });
            DropIndex("dbo.PollVotingRecord", new[] { "Id" });
            //DropIndex("dbo.Customer_CustomerRole_Mapping", new[] { "CustomerRole_Id" });
            //DropIndex("dbo.Customer_CustomerRole_Mapping", new[] { "Customer_Id" });
            DropIndex("dbo.Product_ProductTag_Mapping", new[] { "ProductTag_Id" });
            DropIndex("dbo.Product_ProductTag_Mapping", new[] { "Product_Id" });
            DropIndex("dbo.Discount_AppliedToProducts", new[] { "Product_Id" });
            DropIndex("dbo.Discount_AppliedToProducts", new[] { "Discount_Id" });
            DropIndex("dbo.Discount_AppliedToManufacturers", new[] { "Manufacturer_Id" });
            DropIndex("dbo.Discount_AppliedToManufacturers", new[] { "Discount_Id" });
            DropIndex("dbo.Discount_AppliedToCategories", new[] { "Category_Id" });
            DropIndex("dbo.Discount_AppliedToCategories", new[] { "Discount_Id" });
            DropIndex("dbo.CustomerAddresses", new[] { "Address_Id" });
            DropIndex("dbo.CustomerAddresses", new[] { "Customer_Id" });
            DropIndex("dbo.ShippingMethodRestrictions", new[] { "Country_Id" });
            DropIndex("dbo.ShippingMethodRestrictions", new[] { "ShippingMethod_Id" });
            DropIndex("dbo.PermissionRecord_Role_Mapping", new[] { "CustomerRole_Id" });
            DropIndex("dbo.PermissionRecord_Role_Mapping", new[] { "PermissionRecord_Id" });
            DropIndex("dbo.BackInStockSubscription", new[] { "CustomerId" });
            DropIndex("dbo.BackInStockSubscription", new[] { "ProductId" });
            DropIndex("dbo.ExportProfile", new[] { "SchedulingTaskId" });
            DropIndex("dbo.ExportDeployment", new[] { "ProfileId" });
            DropIndex("dbo.SyncMapping", "IX_SyncMapping_BySource");
            DropIndex("dbo.SyncMapping", "IX_SyncMapping_ByEntity");
            DropIndex("dbo.ImportProfile", new[] { "SchedulingTaskId" });
            DropIndex("dbo.NewsLetterSubscription", "IX_NewsletterSubscription_Email_StoreId");
            DropIndex("dbo.QueuedEmail", new[] { "EmailAccountId" });
            DropIndex("dbo.QueuedEmailAttachment", new[] { "MediaStorageId" });
            DropIndex("dbo.QueuedEmailAttachment", new[] { "FileId" });
            DropIndex("dbo.QueuedEmailAttachment", new[] { "QueuedEmailId" });
            DropIndex("dbo.Store", new[] { "PrimaryExchangeRateCurrencyId" });
            DropIndex("dbo.Store", new[] { "PrimaryStoreCurrencyId" });
            DropIndex("dbo.GenericAttribute", "IX_GenericAttribute_Key");
            DropIndex("dbo.AclRecord", new[] { "CustomerRoleId" });
            DropIndex("dbo.ScheduleTask", "IX_LastStart_LastEnd");
            DropIndex("dbo.ScheduleTask", "IX_NextRun_Enabled");
            DropIndex("dbo.ScheduleTask", new[] { "Type" });
            DropIndex("dbo.Affiliate", new[] { "AddressId" });
            DropIndex("dbo.Forums_PrivateMessage", new[] { "ToCustomerId" });
            DropIndex("dbo.Forums_PrivateMessage", new[] { "FromCustomerId" });
            DropIndex("dbo.Forums_Subscription", new[] { "CustomerId" });
            DropIndex("dbo.Log", "IX_Log_Logger");
            DropIndex("dbo.Log", new[] { "CustomerId" });
            DropIndex("dbo.Log", "IX_Log_Level");
            DropIndex("dbo.ActivityLog", new[] { "CustomerId" });
            DropIndex("dbo.ActivityLog", new[] { "ActivityLogTypeId" });
            DropIndex("dbo.RecurringPayment", new[] { "InitialOrderId" });
            DropIndex("dbo.RecurringPaymentHistory", new[] { "RecurringPaymentId" });
            DropIndex("dbo.CheckoutAttributeValue", new[] { "CheckoutAttributeId" });
            DropIndex("dbo.ShoppingCartItem", new[] { "ProductId" });
            DropIndex("dbo.ShoppingCartItem", new[] { "CustomerId" });
            DropIndex("dbo.ShoppingCartItem", new[] { "BundleItemId" });
            DropIndex("dbo.ReturnRequest", new[] { "CustomerId" });
            DropIndex("dbo.ShipmentItem", new[] { "ShipmentId" });
            DropIndex("dbo.Shipment", new[] { "OrderId" });
            DropIndex("dbo.RewardPointsHistory", new[] { "UsedWithOrder_Id" });
            DropIndex("dbo.RewardPointsHistory", new[] { "CustomerId" });
            DropIndex("dbo.OrderNote", new[] { "OrderId" });
            DropIndex("dbo.OrderItem", new[] { "ProductId" });
            DropIndex("dbo.OrderItem", new[] { "OrderId" });
            DropIndex("dbo.GiftCard", new[] { "PurchasedWithOrderItemId" });
            DropIndex("dbo.GiftCardUsageHistory", new[] { "UsedWithOrderId" });
            DropIndex("dbo.GiftCardUsageHistory", new[] { "GiftCardId" });
            DropIndex("dbo.DiscountUsageHistory", new[] { "OrderId" });
            DropIndex("dbo.DiscountUsageHistory", new[] { "DiscountId" });
            DropIndex("dbo.Order", new[] { "Deleted" });
            DropIndex("dbo.Order", new[] { "ShippingAddressId" });
            DropIndex("dbo.Order", new[] { "BillingAddressId" });
            DropIndex("dbo.Order", new[] { "CustomerId" });
            DropIndex("dbo.Forums_Forum", new[] { "ForumGroupId" });
            DropIndex("dbo.Forums_Topic", new[] { "CustomerId" });
            DropIndex("dbo.Forums_Topic", new[] { "ForumId" });
            DropIndex("dbo.Forums_Post", new[] { "CustomerId" });
            DropIndex("dbo.Forums_Post", new[] { "TopicId" });
            DropIndex("dbo.ExternalAuthenticationRecord", new[] { "CustomerId" });
            DropIndex("dbo.News", new[] { "LanguageId" });
            DropIndex("dbo.TierPrice", new[] { "CustomerRoleId" });
            DropIndex("dbo.TierPrice", new[] { "ProductId" });
            DropIndex("dbo.Download", new[] { "MediaStorageId" });
            DropIndex("dbo.Download", "IX_UpdatedOn_IsTransient");
            DropIndex("dbo.Download", new[] { "DownloadGuid" });
            DropIndex("dbo.ProductVariantAttributeValue", "IX_ProductVariantAttributeValue_ProductVariantAttributeId_DisplayOrder");
            DropIndex("dbo.ProductAttributeOption", new[] { "ProductAttributeOptionsSetId" });
            DropIndex("dbo.ProductAttributeOptionsSet", new[] { "ProductAttributeId" });
            DropIndex("dbo.Product_ProductAttribute_Mapping", new[] { "ProductAttributeId" });
            DropIndex("dbo.Product_ProductAttribute_Mapping", "IX_Product_ProductAttribute_Mapping_ProductId_DisplayOrder");
            DropIndex("dbo.ProductVariantAttributeCombination", new[] { "ProductId" });
            DropIndex("dbo.ProductVariantAttributeCombination", new[] { "QuantityUnitId" });
            DropIndex("dbo.ProductVariantAttributeCombination", new[] { "DeliveryTimeId" });
            DropIndex("dbo.SpecificationAttributeOption", new[] { "SpecificationAttributeId" });
            DropIndex("dbo.Product_SpecificationAttribute_Mapping", new[] { "SpecificationAttributeOptionId" });
            DropIndex("dbo.Product_SpecificationAttribute_Mapping", new[] { "ProductId" });
            DropIndex("dbo.Product_Manufacturer_Mapping", new[] { "ManufacturerId" });
            DropIndex("dbo.Product_Manufacturer_Mapping", new[] { "ProductId" });
            DropIndex("dbo.Product_Category_Mapping", new[] { "CategoryId" });
            DropIndex("dbo.Product_Category_Mapping", new[] { "ProductId" });
            DropIndex("dbo.ProductBundleItemAttributeFilter", new[] { "BundleItemId" });
            DropIndex("dbo.ProductBundleItem", new[] { "BundleProductId" });
            DropIndex("dbo.ProductBundleItem", new[] { "ProductId" });
            //DropIndex("dbo.DiscountRequirement", new[] { "DiscountId" });
            DropIndex("dbo.Manufacturer", new[] { "Deleted" });
            DropIndex("dbo.Manufacturer", new[] { "PictureId" });
            DropIndex("dbo.Product_Picture_Mapping", new[] { "PictureId" });
            DropIndex("dbo.Product_Picture_Mapping", new[] { "ProductId" });
            DropIndex("dbo.Picture", new[] { "MediaStorageId" });
            DropIndex("dbo.Picture", "IX_UpdatedOn_IsTransient");
            DropIndex("dbo.Category", new[] { "Deleted" });
            DropIndex("dbo.Category", new[] { "PictureId" });
            DropIndex("dbo.Product", new[] { "CountryOfOriginId" });
            DropIndex("dbo.Product", new[] { "QuantityUnitId" });
            DropIndex("dbo.Product", new[] { "DeliveryTimeId" });
            DropIndex("dbo.Product", new[] { "Deleted" });
            DropIndex("dbo.Product", new[] { "SampleDownloadId" });
            DropIndex("dbo.Product", new[] { "Gtin" });
            DropIndex("dbo.Product", new[] { "ManufacturerPartNumber" });
            DropIndex("dbo.BlogPost", new[] { "LanguageId" });
            DropIndex("dbo.StateProvince", new[] { "CountryId" });
            DropIndex("dbo.Address", new[] { "StateProvinceId" });
            DropIndex("dbo.Address", new[] { "CountryId" });
            DropIndex("dbo.Customer", new[] { "ShippingAddress_Id" });
            DropIndex("dbo.Customer", new[] { "BillingAddress_Id" });
            DropIndex("dbo.Customer", "IX_Customer_LastActivity");
            DropIndex("dbo.Customer", "IX_Customer_CreatedOn");
            DropIndex("dbo.Customer", "IX_Customer_LastIpAddress");
            DropIndex("dbo.Customer", new[] { "SystemName" });
            DropIndex("dbo.Customer", new[] { "Deleted" });
            DropIndex("dbo.CustomerContent", new[] { "CustomerId" });
            DropIndex("dbo.PollAnswer", new[] { "PollId" });
            DropIndex("dbo.Poll", new[] { "LanguageId" });
            DropIndex("dbo.LocaleStringResource", new[] { "LanguageId" });
            DropIndex("dbo.LocalizedProperty", "IX_LocalizedProperty_Compound");
            DropTable("dbo.ProductReview");
            DropTable("dbo.ProductReviewHelpfulness");
            DropTable("dbo.BlogComment");
            DropTable("dbo.NewsComment");
            DropTable("dbo.PollVotingRecord");
            //DropTable("dbo.Customer_CustomerRole_Mapping");
            DropTable("dbo.Product_ProductTag_Mapping");
            DropTable("dbo.Discount_AppliedToProducts");
            DropTable("dbo.Discount_AppliedToManufacturers");
            DropTable("dbo.Discount_AppliedToCategories");
            DropTable("dbo.CustomerAddresses");
            DropTable("dbo.ShippingMethodRestrictions");
            DropTable("dbo.PermissionRecord_Role_Mapping");
            DropTable("dbo.CrossSellProduct");
            DropTable("dbo.RelatedProduct");
            DropTable("dbo.ProductTemplate");
            DropTable("dbo.CategoryTemplate");
            DropTable("dbo.ManufacturerTemplate");
            DropTable("dbo.BackInStockSubscription");
            DropTable("dbo.ExportProfile");
            DropTable("dbo.ExportDeployment");
            DropTable("dbo.SyncMapping");
            DropTable("dbo.ImportProfile");
            DropTable("dbo.MeasureDimension");
            DropTable("dbo.MeasureWeight");
            DropTable("dbo.NewsLetterSubscription");
            DropTable("dbo.MessageTemplate");
            DropTable("dbo.Campaign");
            DropTable("dbo.EmailAccount");
            DropTable("dbo.QueuedEmail");
            DropTable("dbo.QueuedEmailAttachment");
            DropTable("dbo.PaymentMethod");
            DropTable("dbo.StoreMapping");
            DropTable("dbo.Currency");
            DropTable("dbo.Store");
            DropTable("dbo.ThemeVariable");
            DropTable("dbo.GenericAttribute");
            DropTable("dbo.UrlRecord");
            DropTable("dbo.AclRecord");
            DropTable("dbo.ScheduleTask");
            DropTable("dbo.Affiliate");
            DropTable("dbo.Forums_PrivateMessage");
            DropTable("dbo.Forums_Subscription");
            DropTable("dbo.Log");
            DropTable("dbo.ActivityLogType");
            DropTable("dbo.ActivityLog");
            DropTable("dbo.RecurringPayment");
            DropTable("dbo.RecurringPaymentHistory");
            DropTable("dbo.CheckoutAttributeValue");
            DropTable("dbo.CheckoutAttribute");
            DropTable("dbo.ShoppingCartItem");
            DropTable("dbo.ReturnRequest");
            DropTable("dbo.ShipmentItem");
            DropTable("dbo.Shipment");
            DropTable("dbo.RewardPointsHistory");
            DropTable("dbo.OrderNote");
            DropTable("dbo.OrderItem");
            DropTable("dbo.GiftCard");
            DropTable("dbo.GiftCardUsageHistory");
            DropTable("dbo.DiscountUsageHistory");
            DropTable("dbo.Order");
            DropTable("dbo.Forums_Group");
            DropTable("dbo.Forums_Forum");
            DropTable("dbo.Forums_Topic");
            DropTable("dbo.Forums_Post");
            DropTable("dbo.ExternalAuthenticationRecord");
            DropTable("dbo.News");
            DropTable("dbo.TierPrice");
            DropTable("dbo.Download");
            DropTable("dbo.ProductVariantAttributeValue");
            DropTable("dbo.ProductAttributeOption");
            DropTable("dbo.ProductAttributeOptionsSet");
            DropTable("dbo.ProductAttribute");
            DropTable("dbo.Product_ProductAttribute_Mapping");
            DropTable("dbo.QuantityUnit");
            DropTable("dbo.ProductVariantAttributeCombination");
            DropTable("dbo.ProductTag");
            DropTable("dbo.SpecificationAttribute");
            DropTable("dbo.SpecificationAttributeOption");
            DropTable("dbo.Product_SpecificationAttribute_Mapping");
            DropTable("dbo.Product_Manufacturer_Mapping");
            DropTable("dbo.Product_Category_Mapping");
            DropTable("dbo.ProductBundleItemAttributeFilter");
            DropTable("dbo.ProductBundleItem");
            DropTable("dbo.DeliveryTime");
            //DropTable("dbo.DiscountRequirement");
            DropTable("dbo.Manufacturer");
            DropTable("dbo.Product_Picture_Mapping");
            DropTable("dbo.MediaStorage");
            DropTable("dbo.Picture");
            DropTable("dbo.Category");
            DropTable("dbo.Discount");
            DropTable("dbo.Product");
            DropTable("dbo.BlogPost");
            DropTable("dbo.StateProvince");
            DropTable("dbo.ShippingMethod");
            DropTable("dbo.Country");
            DropTable("dbo.Address");
            DropTable("dbo.Customer");
            DropTable("dbo.CustomerContent");
            DropTable("dbo.PollAnswer");
            DropTable("dbo.Poll");
            DropTable("dbo.CustomerRole");
            DropTable("dbo.PermissionRecord");
            DropTable("dbo.LocaleStringResource");
            DropTable("dbo.Language");
            DropTable("dbo.LocalizedProperty");
            DropTable("dbo.Setting");
            DropTable("dbo.TaxCategory");
            DropTable("dbo.Topic");
        }
    }
}
