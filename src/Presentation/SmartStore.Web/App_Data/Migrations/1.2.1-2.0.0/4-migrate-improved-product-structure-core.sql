IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[ProductVariant]') and NAME='BasePrice_Enabled')
BEGIN
	EXEC sp_rename 'ProductVariant.BasePrice_Enabled', 'BasePriceEnabled', 'COLUMN';
END
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[ProductVariant]') and NAME='BasePrice_MeasureUnit')
BEGIN
	EXEC sp_rename 'ProductVariant.BasePrice_MeasureUnit', 'BasePriceMeasureUnit', 'COLUMN';
END
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[ProductVariant]') and NAME='BasePrice_Amount')
BEGIN
	EXEC sp_rename 'ProductVariant.BasePrice_Amount', 'BasePriceAmount', 'COLUMN';
END
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[ProductVariant]') and NAME='BasePrice_BaseAmount')
BEGIN
	EXEC sp_rename 'ProductVariant.BasePrice_BaseAmount', 'BasePriceBaseAmount', 'COLUMN';
END
GO

--rename ShipmentOrderProductVariant to ShipmentItem
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Shipment_OrderProductVariant]') and OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	EXEC sp_rename 'Shipment_OrderProductVariant', 'ShipmentItem';
END
GO

IF EXISTS (SELECT 1
           FROM sys.objects
           WHERE name = 'ShipmentOrderProductVariant_Shipment'
           AND parent_object_id = Object_id('ShipmentItem')
           AND Objectproperty(object_id,N'IsForeignKey') = 1)
BEGIN
	EXEC sp_rename 'ShipmentOrderProductVariant_Shipment', 'ShipmentItem_Shipment';
END
GO

--rename OrderProductVariant to OrderItem
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[GiftCard]') and NAME='PurchasedWithOrderProductVariantId')
BEGIN
	EXEC sp_rename 'GiftCard.PurchasedWithOrderProductVariantId', 'PurchasedWithOrderItemId', 'COLUMN';
END
GO
IF EXISTS (SELECT 1
           FROM sys.objects
           WHERE name = 'GiftCard_PurchasedWithOrderProductVariant'
           AND parent_object_id = Object_id('GiftCard')
           AND Objectproperty(object_id,N'IsForeignKey') = 1)
BEGIN
	EXEC sp_rename 'GiftCard_PurchasedWithOrderProductVariant', 'GiftCard_PurchasedWithOrderItem';
END
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[OrderProductVariant]') and OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	EXEC sp_rename 'OrderProductVariant', 'OrderItem';
END
GO

IF EXISTS (SELECT 1
           FROM sys.objects
           WHERE name = 'OrderProductVariant_Order'
           AND parent_object_id = Object_id('OrderItem')
           AND Objectproperty(object_id,N'IsForeignKey') = 1)
BEGIN
	EXEC sp_rename 'OrderProductVariant_Order', 'OrderItem_Order';
END
GO

IF EXISTS (SELECT 1
           FROM sys.objects
           WHERE name = 'OrderProductVariant_ProductVariant'
           AND parent_object_id = Object_id('OrderItem')
           AND Objectproperty(object_id,N'IsForeignKey') = 1)
BEGIN
	EXEC sp_rename 'OrderProductVariant_ProductVariant', 'OrderItem_ProductVariant';
END
GO

IF EXISTS (SELECT 1 from sys.indexes WHERE [NAME]=N'IX_OrderProductVariant_OrderId' and object_id=object_id(N'[OrderItem]'))
BEGIN
	EXEC sp_rename 'OrderItem.IX_OrderProductVariant_OrderId', 'IX_OrderItem_OrderId', 'INDEX';
END
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[ReturnRequest]') and NAME='OrderProductVariantId')
BEGIN
	EXEC sp_rename 'ReturnRequest.OrderProductVariantId', 'OrderItemId', 'COLUMN';
END
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[ShipmentItem]') and NAME='OrderProductVariantId')
BEGIN
	EXEC sp_rename 'ShipmentItem.OrderProductVariantId', 'OrderItemId', 'COLUMN';
END
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[OrderItem]') and NAME='OrderProductVariantGuid')
BEGIN
	EXEC sp_rename 'OrderItem.OrderProductVariantGuid', 'OrderItemGuid', 'COLUMN';
END
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[DiscountRequirement]') and NAME='RestrictedProductVariantIds')
BEGIN
	EXEC sp_rename 'DiscountRequirement.RestrictedProductVariantIds', 'RestrictedProductIds', 'COLUMN';
END
GO


DELETE FROM [ActivityLogType] WHERE [SystemKeyword] = N'AddNewProductVariant'
GO
DELETE FROM [ActivityLogType] WHERE [SystemKeyword] = N'DeleteProductVariant'
GO
DELETE FROM [ActivityLogType] WHERE [SystemKeyword] = N'EditProductVariant'
GO

--remove obsolete setting
DELETE FROM [Setting] WHERE [name] = N'MediaSettings.ProductVariantPictureSize'
GO
DELETE FROM [Setting] WHERE [name] = N'ElmarShopinfoSettings.ExportSpecialPrice'
GO

IF NOT EXISTS (SELECT 1 FROM [Setting] WHERE [name] = N'mediasettings.associatedproductpicturesize')
BEGIN
	INSERT [Setting] ([Name], [Value], [StoreId]) VALUES (N'mediasettings.associatedproductpicturesize', N'125', 0)
END
GO

--update some message template tokens
UPDATE [MessageTemplate]
SET [Subject] = REPLACE([Subject], 'ProductVariant.ID', 'Product.ID'),
[Body] = REPLACE([Body], 'ProductVariant.ID', 'Product.ID')
GO

UPDATE [MessageTemplate]
SET [Subject] = REPLACE([Subject], 'ProductVariant.FullProductName', 'Product.Name'),
[Body] = REPLACE([Body], 'ProductVariant.FullProductName', 'Product.Name')
GO

UPDATE [MessageTemplate]
SET [Subject] = REPLACE([Subject], 'ProductVariant.StockQuantity', 'Product.StockQuantity'),
[Body] = REPLACE([Body], 'ProductVariant.StockQuantity', 'Product.StockQuantity')
GO

--update product templates
UPDATE [ProductTemplate]
SET [Name] = N'Grouped product', [ViewPath] = N'ProductTemplate.Grouped', [DisplayOrder] = 100
WHERE [ViewPath] = N'ProductTemplate.VariantsInGrid'
GO
UPDATE [ProductTemplate]
SET [Name] = N'Simple product', [ViewPath] = N'ProductTemplate.Simple', [DisplayOrder] = 10
WHERE [ViewPath] = N'ProductTemplate.SingleVariant'
GO

IF (NOT EXISTS(SELECT 1 FROM [ProductTemplate] WHERE [ViewPath] = N'ProductTemplate.Grouped'))
BEGIN
	INSERT INTO [ProductTemplate] ([Name],[ViewPath],[DisplayOrder])
	VALUES (N'Grouped product',N'ProductTemplate.Grouped',100)
END
GO

IF (NOT EXISTS(SELECT 1 FROM [ProductTemplate] WHERE [ViewPath] = N'ProductTemplate.Simple'))
BEGIN
	INSERT INTO [ProductTemplate] ([Name],[ViewPath],[DisplayOrder])
	VALUES (N'Simple product',N'ProductTemplate.Simple',10)
END
GO

--delete products without variants
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[ProductVariant]') and OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	DELETE FROM [Product] WHERE [Id] NOT IN (SELECT [ProductId] FROM [ProductVariant])
END
GO

--move records from ProductVariant to Product
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='ProductTypeId')
BEGIN
	ALTER TABLE [Product]
	ADD [ProductTypeId] int NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='ParentGroupedProductId')
BEGIN
	ALTER TABLE [Product]
	ADD [ParentGroupedProductId] int NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='SKU')
BEGIN
	ALTER TABLE [Product]
	ADD [SKU] nvarchar(400) NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='ManufacturerPartNumber')
BEGIN
	ALTER TABLE [Product]
	ADD [ManufacturerPartNumber] nvarchar(400) NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='Gtin')
BEGIN
	ALTER TABLE [Product]
	ADD [Gtin] nvarchar(400) NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='IsGiftCard')
BEGIN
	ALTER TABLE [Product]
	ADD [IsGiftCard] bit NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='GiftCardTypeId')
BEGIN
	ALTER TABLE [Product]
	ADD [GiftCardTypeId] int NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='RequireOtherProducts')
BEGIN
	ALTER TABLE [Product]
	ADD [RequireOtherProducts] bit NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='RequiredProductIds')
BEGIN
	ALTER TABLE [Product]
	ADD [RequiredProductIds] nvarchar(1000) NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='AutomaticallyAddRequiredProducts')
BEGIN
	ALTER TABLE [Product]
	ADD [AutomaticallyAddRequiredProducts] bit NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='IsDownload')
BEGIN
	ALTER TABLE [Product]
	ADD [IsDownload] bit NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='DownloadId')
BEGIN
	ALTER TABLE [Product]
	ADD [DownloadId] int NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='UnlimitedDownloads')
BEGIN
	ALTER TABLE [Product]
	ADD [UnlimitedDownloads] bit NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='MaxNumberOfDownloads')
BEGIN
	ALTER TABLE [Product]
	ADD [MaxNumberOfDownloads] int NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='DownloadExpirationDays')
BEGIN
	ALTER TABLE [Product]
	ADD [DownloadExpirationDays] int NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='DownloadActivationTypeId')
BEGIN
	ALTER TABLE [Product]
	ADD [DownloadActivationTypeId] int NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='HasSampleDownload')
BEGIN
	ALTER TABLE [Product]
	ADD [HasSampleDownload] bit NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='SampleDownloadId')
BEGIN
	ALTER TABLE [Product]
	ADD [SampleDownloadId] int NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='HasUserAgreement')
BEGIN
	ALTER TABLE [Product]
	ADD [HasUserAgreement] bit NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='UserAgreementText')
BEGIN
	ALTER TABLE [Product]
	ADD [UserAgreementText] nvarchar(MAX) NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='IsRecurring')
BEGIN
	ALTER TABLE [Product]
	ADD [IsRecurring] bit NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='RecurringCycleLength')
BEGIN
	ALTER TABLE [Product]
	ADD [RecurringCycleLength] int NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='RecurringCyclePeriodId')
BEGIN
	ALTER TABLE [Product]
	ADD [RecurringCyclePeriodId] int NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='RecurringTotalCycles')
BEGIN
	ALTER TABLE [Product]
	ADD [RecurringTotalCycles] int NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='IsShipEnabled')
BEGIN
	ALTER TABLE [Product]
	ADD [IsShipEnabled] bit NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='IsFreeShipping')
BEGIN
	ALTER TABLE [Product]
	ADD [IsFreeShipping] bit NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='AdditionalShippingCharge')
BEGIN
	ALTER TABLE [Product]
	ADD [AdditionalShippingCharge] decimal(18,4) NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='IsTaxExempt')
BEGIN
	ALTER TABLE [Product]
	ADD [IsTaxExempt] bit NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='TaxCategoryId')
BEGIN
	ALTER TABLE [Product]
	ADD [TaxCategoryId] int NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='ManageInventoryMethodId')
BEGIN
	ALTER TABLE [Product]
	ADD [ManageInventoryMethodId] int NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='StockQuantity')
BEGIN
	ALTER TABLE [Product]
	ADD [StockQuantity] int NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='DisplayStockAvailability')
BEGIN
	ALTER TABLE [Product]
	ADD [DisplayStockAvailability] bit NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='DisplayStockQuantity')
BEGIN
	ALTER TABLE [Product]
	ADD [DisplayStockQuantity] bit NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='MinStockQuantity')
BEGIN
	ALTER TABLE [Product]
	ADD [MinStockQuantity] int NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='LowStockActivityId')
BEGIN
	ALTER TABLE [Product]
	ADD [LowStockActivityId] int NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='NotifyAdminForQuantityBelow')
BEGIN
	ALTER TABLE [Product]
	ADD [NotifyAdminForQuantityBelow] int  NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='BackorderModeId')
BEGIN
	ALTER TABLE [Product]
	ADD [BackorderModeId] int NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='AllowBackInStockSubscriptions')
BEGIN
	ALTER TABLE [Product]
	ADD [AllowBackInStockSubscriptions] bit NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='OrderMinimumQuantity')
BEGIN
	ALTER TABLE [Product]
	ADD [OrderMinimumQuantity] int NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='OrderMaximumQuantity')
BEGIN
	ALTER TABLE [Product]
	ADD [OrderMaximumQuantity] int NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='AllowedQuantities')
BEGIN
	ALTER TABLE [Product]
	ADD [AllowedQuantities] nvarchar(1000) NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='DisableBuyButton')
BEGIN
	ALTER TABLE [Product]
	ADD [DisableBuyButton] bit NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='DisableWishlistButton')
BEGIN
	ALTER TABLE [Product]
	ADD [DisableWishlistButton] bit NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='AvailableForPreOrder')
BEGIN
	ALTER TABLE [Product]
	ADD [AvailableForPreOrder] bit NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='CallForPrice')
BEGIN
	ALTER TABLE [Product]
	ADD [CallForPrice] bit NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='Price')
BEGIN
	ALTER TABLE [Product]
	ADD [Price] decimal(18, 4) NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='OldPrice')
BEGIN
	ALTER TABLE [Product]
	ADD [OldPrice] decimal(18, 4) NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='ProductCost')
BEGIN
	ALTER TABLE [Product]
	ADD [ProductCost] decimal(18, 4)  NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='SpecialPrice')
BEGIN
	ALTER TABLE [Product]
	ADD [SpecialPrice] decimal(18, 4) NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='SpecialPriceStartDateTimeUtc')
BEGIN
	ALTER TABLE [Product]
	ADD [SpecialPriceStartDateTimeUtc] datetime NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='SpecialPriceEndDateTimeUtc')
BEGIN
	ALTER TABLE [Product]
	ADD [SpecialPriceEndDateTimeUtc] datetime NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='CustomerEntersPrice')
BEGIN
	ALTER TABLE [Product]
	ADD [CustomerEntersPrice] bit NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='MinimumCustomerEnteredPrice')
BEGIN
	ALTER TABLE [Product]
	ADD [MinimumCustomerEnteredPrice] decimal(18, 4) NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='MaximumCustomerEnteredPrice')
BEGIN
	ALTER TABLE [Product]
	ADD [MaximumCustomerEnteredPrice] decimal(18, 4) NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='HasTierPrices')
BEGIN
	ALTER TABLE [Product]
	ADD [HasTierPrices] bit NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='HasDiscountsApplied')
BEGIN
	ALTER TABLE [Product]
	ADD [HasDiscountsApplied] bit NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='Weight')
BEGIN
	ALTER TABLE [Product]
	ADD [Weight] decimal(18, 4) NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='Length')
BEGIN
	ALTER TABLE [Product]
	ADD [Length] decimal(18, 4) NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='Width')
BEGIN
	ALTER TABLE [Product]
	ADD [Width] decimal(18, 4) NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='Height')
BEGIN
	ALTER TABLE [Product]
	ADD [Height] decimal(18, 4) NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='AvailableStartDateTimeUtc')
BEGIN
	ALTER TABLE [Product]
	ADD [AvailableStartDateTimeUtc] datetime NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='AvailableEndDateTimeUtc')
BEGIN
	ALTER TABLE [Product]
	ADD [AvailableEndDateTimeUtc] datetime NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='DeliveryTimeId')
BEGIN
	ALTER TABLE [Product]
	ADD [DeliveryTimeId] int NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='BasePriceEnabled')
BEGIN
	ALTER TABLE [Product]
	ADD [BasePriceEnabled] bit NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='BasePriceMeasureUnit')
BEGIN
	ALTER TABLE [Product]
	ADD [BasePriceMeasureUnit] nvarchar(50) NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='BasePriceAmount')
BEGIN
	ALTER TABLE [Product]
	ADD [BasePriceAmount] decimal(18, 4) NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='BasePriceBaseAmount')
BEGIN
	ALTER TABLE [Product]
	ADD [BasePriceBaseAmount] int NULL
END
GO

--remove old product variant references
IF EXISTS (SELECT 1
           FROM   sys.objects
           WHERE  name = 'BackInStockSubscription_ProductVariant'
           AND parent_object_id = Object_id('BackInStockSubscription')
           AND Objectproperty(object_id,N'IsForeignKey') = 1)
BEGIN
	ALTER TABLE dbo.[BackInStockSubscription]
	DROP CONSTRAINT BackInStockSubscription_ProductVariant
END
GO

IF EXISTS (SELECT 1
           FROM   sys.objects
           WHERE  name = 'OrderItem_ProductVariant'
           AND parent_object_id = Object_id('OrderItem')
           AND Objectproperty(object_id,N'IsForeignKey') = 1)
BEGIN
	ALTER TABLE dbo.[OrderItem]
	DROP CONSTRAINT OrderItem_ProductVariant
END
GO

IF EXISTS (SELECT 1
           FROM   sys.objects
           WHERE  name = 'ProductVariantAttribute_ProductVariant'
           AND parent_object_id = Object_id('ProductVariant_ProductAttribute_Mapping')
           AND Objectproperty(object_id,N'IsForeignKey') = 1)
BEGIN
	ALTER TABLE dbo.[ProductVariant_ProductAttribute_Mapping]
	DROP CONSTRAINT ProductVariantAttribute_ProductVariant
END
GO

IF EXISTS (SELECT 1
           FROM   sys.objects
           WHERE  name = 'ProductVariantAttributeCombination_ProductVariant'
           AND parent_object_id = Object_id('ProductVariantAttributeCombination')
           AND Objectproperty(object_id,N'IsForeignKey') = 1)
BEGIN
	ALTER TABLE dbo.[ProductVariantAttributeCombination]
	DROP CONSTRAINT ProductVariantAttributeCombination_ProductVariant
END
GO

IF EXISTS (SELECT 1
           FROM   sys.objects
           WHERE  name = 'ShoppingCartItem_ProductVariant'
           AND parent_object_id = Object_id('ShoppingCartItem')
           AND Objectproperty(object_id,N'IsForeignKey') = 1)
BEGIN
	ALTER TABLE dbo.[ShoppingCartItem]
	DROP CONSTRAINT ShoppingCartItem_ProductVariant
END
GO

IF EXISTS (SELECT 1
           FROM   sys.objects
           WHERE  name = 'TierPrice_ProductVariant'
           AND parent_object_id = Object_id('TierPrice')
           AND Objectproperty(object_id,N'IsForeignKey') = 1)
BEGIN
	ALTER TABLE dbo.[TierPrice]
	DROP CONSTRAINT TierPrice_ProductVariant
END
GO

IF EXISTS (SELECT 1
           FROM   sys.objects
           WHERE  name = 'Discount_AppliedToProductVariants_Target'
           AND parent_object_id = Object_id('Discount_AppliedToProductVariants')
           AND Objectproperty(object_id,N'IsForeignKey') = 1)
BEGIN
	ALTER TABLE dbo.[Discount_AppliedToProductVariants]
	DROP CONSTRAINT Discount_AppliedToProductVariants_Target
END
GO

--new ProductId columns in references tables
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[BackInStockSubscription]') and NAME='ProductId')
BEGIN
	ALTER TABLE [BackInStockSubscription]
	ADD [ProductId] int NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[OrderItem]') and NAME='ProductId')
BEGIN
	ALTER TABLE [OrderItem]
	ADD [ProductId] int NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[ProductVariant_ProductAttribute_Mapping]') and NAME='ProductId')
BEGIN
	--one more validatation here because we'll rename [ProductVariant_ProductAttribute_Mapping] table a bit later
	IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Product_ProductAttribute_Mapping]') and OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
	BEGIN
		ALTER TABLE [ProductVariant_ProductAttribute_Mapping]
		ADD [ProductId] int NULL
	END
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[ProductVariantAttributeCombination]') and NAME='ProductId')
BEGIN
	ALTER TABLE [ProductVariantAttributeCombination]
	ADD [ProductId] int NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[ShoppingCartItem]') and NAME='ProductId')
BEGIN
	ALTER TABLE [ShoppingCartItem]
	ADD [ProductId] int NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[TierPrice]') and NAME='ProductId')
BEGIN
	ALTER TABLE [TierPrice]
	ADD [ProductId] int NULL
END
GO
--new table for discount <=> product mapping (have some issue with just adding and renaming columns)
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Discount_AppliedToProducts]') and OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	CREATE TABLE [dbo].[Discount_AppliedToProducts](
	[Discount_Id] [int] NOT NULL,
	[Product_Id] [int] NOT NULL,
	[ProductVariant_Id] [int] NOT NULL,
		PRIMARY KEY CLUSTERED 
		(
			[Discount_Id] ASC,
			[Product_Id] ASC
		)
	)
	
	--copy records
	DECLARE @ExistingDiscountID int
	DECLARE @ExistingDiscountProductVariantID int
	DECLARE cur_existingdiscountmapping CURSOR FOR
	SELECT [Discount_Id], [ProductVariant_Id]
	FROM [Discount_AppliedToProductVariants]
	OPEN cur_existingdiscountmapping
	FETCH NEXT FROM cur_existingdiscountmapping INTO @ExistingDiscountID,@ExistingDiscountProductVariantID
	WHILE @@FETCH_STATUS = 0
	BEGIN
		EXEC sp_executesql N'INSERT INTO [Discount_AppliedToProducts] ([Discount_Id], [Product_Id], [ProductVariant_Id])
		VALUES (@ExistingDiscountID, @ExistingDiscountProductVariantID, @ExistingDiscountProductVariantID)',
		N'@ExistingDiscountID int, 
		@ExistingDiscountProductVariantID int',
		@ExistingDiscountID,
		@ExistingDiscountProductVariantID
		
		--fetch next identifier
		FETCH NEXT FROM cur_existingdiscountmapping INTO @ExistingDiscountID,@ExistingDiscountProductVariantID
	END
	
	CLOSE cur_existingdiscountmapping
	DEALLOCATE cur_existingdiscountmapping
	
	--drop old table
	DROP TABLE [Discount_AppliedToProductVariants]
END
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[ProductVariant]') and OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	DECLARE @ExistingProductVariantID int
	DECLARE cur_existingproductvariant CURSOR FOR
	SELECT [ID]
	FROM [ProductVariant]
	OPEN cur_existingproductvariant
	FETCH NEXT FROM cur_existingproductvariant INTO @ExistingProductVariantID
	WHILE @@FETCH_STATUS = 0
	BEGIN
		DECLARE @ProductId int
		SET @ProductId = null -- clear cache (variable scope)
		DECLARE @Name nvarchar(400)
		SET @Name = null -- clear cache (variable scope)
		DECLARE @Description nvarchar(MAX)
		SET @Description = null -- clear cache (variable scope)
		DECLARE @Sku nvarchar(400)
		SET @Sku = null -- clear cache (variable scope)
		DECLARE @ManufacturerPartNumber nvarchar(400)
		SET @ManufacturerPartNumber = null -- clear cache (variable scope)
		DECLARE @Gtin nvarchar(400)
		SET @Gtin = null -- clear cache (variable scope)
		DECLARE @IsGiftCard bit
		SET @IsGiftCard = null -- clear cache (variable scope)
		DECLARE @GiftCardTypeId int
		SET @GiftCardTypeId = null -- clear cache (variable scope)
		DECLARE @RequireOtherProducts bit
		SET @RequireOtherProducts = null -- clear cache (variable scope)
		DECLARE @RequiredProductIds nvarchar(1000)
		SET @RequiredProductIds = null -- clear cache (variable scope)
		DECLARE @AutomaticallyAddRequiredProducts bit
		SET @AutomaticallyAddRequiredProducts = null -- clear cache (variable scope)
		DECLARE @IsDownload bit
		SET @IsDownload = null -- clear cache (variable scope)
		DECLARE @DownloadId int
		SET @DownloadId = null -- clear cache (variable scope)
		DECLARE @UnlimitedDownloads bit
		SET @UnlimitedDownloads = null -- clear cache (variable scope)
		DECLARE @MaxNumberOfDownloads int
		SET @MaxNumberOfDownloads = null -- clear cache (variable scope)
		DECLARE @DownloadExpirationDays int
		SET @DownloadExpirationDays = null -- clear cache (variable scope)
		DECLARE @DownloadActivationTypeId int
		SET @DownloadActivationTypeId = null -- clear cache (variable scope)
		DECLARE @HasSampleDownload bit
		SET @HasSampleDownload = null -- clear cache (variable scope)
		DECLARE @SampleDownloadId int
		SET @SampleDownloadId = null -- clear cache (variable scope)
		DECLARE @HasUserAgreement bit
		SET @HasUserAgreement = null -- clear cache (variable scope)
		DECLARE @UserAgreementText nvarchar(MAX)
		SET @UserAgreementText = null -- clear cache (variable scope)
		DECLARE @IsRecurring bit
		SET @IsRecurring = null -- clear cache (variable scope)
		DECLARE @RecurringCycleLength int
		SET @RecurringCycleLength = null -- clear cache (variable scope)
		DECLARE @RecurringCyclePeriodId int
		SET @RecurringCyclePeriodId = null -- clear cache (variable scope)
		DECLARE @RecurringTotalCycles int
		SET @RecurringTotalCycles = null -- clear cache (variable scope)
		DECLARE @IsShipEnabled bit
		SET @IsShipEnabled = null -- clear cache (variable scope)
		DECLARE @IsFreeShipping bit
		SET @IsFreeShipping = null -- clear cache (variable scope)
		DECLARE @AdditionalShippingCharge decimal(18,4)
		SET @AdditionalShippingCharge = null -- clear cache (variable scope)
		DECLARE @IsTaxExempt bit
		SET @IsTaxExempt = null -- clear cache (variable scope)
		DECLARE @TaxCategoryId int
		SET @TaxCategoryId = null -- clear cache (variable scope)
		DECLARE @ManageInventoryMethodId int
		SET @ManageInventoryMethodId = null -- clear cache (variable scope)
		DECLARE @StockQuantity int
		SET @StockQuantity = null -- clear cache (variable scope)
		DECLARE @DisplayStockAvailability bit
		SET @DisplayStockAvailability = null -- clear cache (variable scope)
		DECLARE @DisplayStockQuantity bit
		SET @DisplayStockQuantity = null -- clear cache (variable scope)
		DECLARE @MinStockQuantity int
		SET @MinStockQuantity = null -- clear cache (variable scope)
		DECLARE @LowStockActivityId int
		SET @LowStockActivityId = null -- clear cache (variable scope)
		DECLARE @NotifyAdminForQuantityBelow int
		SET @NotifyAdminForQuantityBelow = null -- clear cache (variable scope)
		DECLARE @BackorderModeId int
		SET @BackorderModeId = null -- clear cache (variable scope)
		DECLARE @AllowBackInStockSubscriptions bit
		SET @AllowBackInStockSubscriptions = null -- clear cache (variable scope)
		DECLARE @OrderMinimumQuantity int
		SET @OrderMinimumQuantity = null -- clear cache (variable scope)
		DECLARE @OrderMaximumQuantity int
		SET @OrderMaximumQuantity = null -- clear cache (variable scope)
		DECLARE @AllowedQuantities nvarchar(1000)
		SET @AllowedQuantities = null -- clear cache (variable scope)
		DECLARE @DisableBuyButton bit
		SET @DisableBuyButton = null -- clear cache (variable scope)
		DECLARE @DisableWishlistButton bit
		SET @DisableWishlistButton = null -- clear cache (variable scope)
		DECLARE @AvailableForPreOrder bit
		SET @AvailableForPreOrder = null -- clear cache (variable scope)
		DECLARE @CallForPrice bit
		SET @CallForPrice = null -- clear cache (variable scope)
		DECLARE @Price decimal(18,4)
		SET @Price = null -- clear cache (variable scope)
		DECLARE @OldPrice decimal(18,4)
		SET @OldPrice = null -- clear cache (variable scope)
		DECLARE @ProductCost decimal(18,4)
		SET @ProductCost = null -- clear cache (variable scope)
		DECLARE @SpecialPrice decimal(18,4)
		SET @SpecialPrice = null -- clear cache (variable scope)
		DECLARE @SpecialPriceStartDateTimeUtc datetime
		SET @SpecialPriceStartDateTimeUtc = null -- clear cache (variable scope)
		DECLARE @SpecialPriceEndDateTimeUtc datetime
		SET @SpecialPriceEndDateTimeUtc = null -- clear cache (variable scope)
		DECLARE @CustomerEntersPrice bit
		SET @CustomerEntersPrice = null -- clear cache (variable scope)
		DECLARE @MinimumCustomerEnteredPrice decimal(18,4)
		SET @MinimumCustomerEnteredPrice = null -- clear cache (variable scope)
		DECLARE @MaximumCustomerEnteredPrice decimal(18,4)
		SET @MaximumCustomerEnteredPrice = null -- clear cache (variable scope)
		DECLARE @HasTierPrices bit
		SET @HasTierPrices = null -- clear cache (variable scope)
		DECLARE @HasDiscountsApplied bit
		SET @HasDiscountsApplied = null -- clear cache (variable scope)
		DECLARE @Weight decimal(18, 4)
		SET @Weight = null -- clear cache (variable scope)
		DECLARE @Length decimal(18, 4)
		SET @Length = null -- clear cache (variable scope)
		DECLARE @Width decimal(18, 4)
		SET @Width = null -- clear cache (variable scope)
		DECLARE @Height decimal(18, 4)
		SET @Height = null -- clear cache (variable scope)
		DECLARE @PictureId int
		SET @PictureId = null -- clear cache (variable scope)
		DECLARE @AvailableStartDateTimeUtc datetime
		SET @AvailableStartDateTimeUtc = null -- clear cache (variable scope)
		DECLARE @AvailableEndDateTimeUtc datetime
		SET @AvailableEndDateTimeUtc = null -- clear cache (variable scope)
		DECLARE @Published bit
		SET @Published = null -- clear cache (variable scope)
		DECLARE @Deleted bit
		SET @Deleted = null -- clear cache (variable scope)
		DECLARE @DisplayOrder int
		SET @DisplayOrder = null -- clear cache (variable scope)
		DECLARE @CreatedOnUtc datetime
		SET @CreatedOnUtc = null -- clear cache (variable scope)
		DECLARE @UpdatedOnUtc datetime
		SET @UpdatedOnUtc = null -- clear cache (variable scope)
		DECLARE @DeliveryTimeId int
		SET @DeliveryTimeId = null
		DECLARE @BasePriceEnabled bit
		SET @BasePriceEnabled = null
		DECLARE @BasePriceMeasureUnit nvarchar(50)
		SET @BasePriceMeasureUnit = null
		DECLARE @BasePriceAmount decimal(18, 4)
		SET @BasePriceAmount = null
		DECLARE @BasePriceBaseAmount int
		SET @BasePriceBaseAmount = null

		DECLARE @sql nvarchar(4000)
		SET @sql = 'SELECT 
		@ProductId = [ProductId],
		@Name = [Name],
		@Description = [Description],
		@Sku = [Sku],
		@ManufacturerPartNumber = [ManufacturerPartNumber],
		@Gtin = [Gtin],
		@IsGiftCard = [IsGiftCard],
		@GiftCardTypeId = [GiftCardTypeId],
		@RequireOtherProducts = [RequireOtherProducts],
		@RequiredProductIds= [RequiredProductVariantIds],
		@AutomaticallyAddRequiredProducts = [AutomaticallyAddRequiredProductVariants],
		@IsDownload = [IsDownload],
		@DownloadId = [DownloadId],
		@UnlimitedDownloads = [UnlimitedDownloads],
		@MaxNumberOfDownloads = [MaxNumberOfDownloads],
		@DownloadExpirationDays = [DownloadExpirationDays],
		@DownloadActivationTypeId = [DownloadActivationTypeId],
		@HasSampleDownload = [HasSampleDownload],
		@SampleDownloadId = [SampleDownloadId],
		@HasUserAgreement = [HasUserAgreement],
		@UserAgreementText = [UserAgreementText],
		@IsRecurring = [IsRecurring],
		@RecurringCycleLength = [RecurringCycleLength],
		@RecurringCyclePeriodId = [RecurringCyclePeriodId],
		@RecurringTotalCycles = [RecurringTotalCycles],
		@IsShipEnabled = [IsShipEnabled],
		@IsFreeShipping = [IsFreeShipping],
		@AdditionalShippingCharge = [AdditionalShippingCharge],
		@IsTaxExempt = [IsTaxExempt],
		@TaxCategoryId = [TaxCategoryId],
		@ManageInventoryMethodId = [ManageInventoryMethodId],
		@StockQuantity = [StockQuantity],
		@DisplayStockAvailability = [DisplayStockAvailability],
		@DisplayStockQuantity = [DisplayStockQuantity],
		@MinStockQuantity = [MinStockQuantity],
		@LowStockActivityId = [LowStockActivityId],
		@NotifyAdminForQuantityBelow = [NotifyAdminForQuantityBelow],
		@BackorderModeId = [BackorderModeId],
		@AllowBackInStockSubscriptions = [AllowBackInStockSubscriptions],
		@OrderMinimumQuantity = [OrderMinimumQuantity],
		@OrderMaximumQuantity = [OrderMaximumQuantity],
		@AllowedQuantities = [AllowedQuantities],
		@DisableBuyButton = [DisableBuyButton],
		@DisableWishlistButton = [DisableWishlistButton],
		@AvailableForPreOrder = [AvailableForPreOrder],
		@CallForPrice = [CallForPrice],
		@Price = [Price],
		@OldPrice = [OldPrice],
		@ProductCost = [ProductCost],
		@SpecialPrice = [SpecialPrice],
		@SpecialPriceStartDateTimeUtc = [SpecialPriceStartDateTimeUtc],
		@SpecialPriceEndDateTimeUtc = [SpecialPriceEndDateTimeUtc],
		@CustomerEntersPrice = [CustomerEntersPrice],
		@MinimumCustomerEnteredPrice = [MinimumCustomerEnteredPrice],
		@MaximumCustomerEnteredPrice = [MaximumCustomerEnteredPrice],
		@HasTierPrices = [HasTierPrices],
		@HasDiscountsApplied = [HasDiscountsApplied],
		@Weight = [Weight],
		@Length = [Length],
		@Width = [Width],
		@Height = [Height],
		@PictureId = [PictureId],
		@AvailableStartDateTimeUtc = [AvailableStartDateTimeUtc],
		@AvailableEndDateTimeUtc = [AvailableEndDateTimeUtc],
		@Published = [Published],
		@Deleted = [Deleted],
		@DisplayOrder = [DisplayOrder],
		@CreatedOnUtc = [CreatedOnUtc],
		@UpdatedOnUtc = [UpdatedOnUtc],
		@DeliveryTimeId = [DeliveryTimeId],
		@BasePriceEnabled = [BasePriceEnabled],
		@BasePriceMeasureUnit = [BasePriceMeasureUnit],
		@BasePriceAmount = [BasePriceAmount],
		@BasePriceBaseAmount = [BasePriceBaseAmount]
		FROM [ProductVariant] 
		WHERE [Id]=' + ISNULL(CAST(@ExistingProductVariantID AS nvarchar(max)), '0')

		EXEC sp_executesql @sql,
		N'@ProductId int OUTPUT, 
		@Name nvarchar(400) OUTPUT,
		@Description nvarchar(MAX) OUTPUT,
		@Sku nvarchar(400) OUTPUT, 
		@ManufacturerPartNumber nvarchar(400) OUTPUT,
		@Gtin nvarchar(400) OUTPUT,
		@IsGiftCard bit OUTPUT, 
		@GiftCardTypeId int OUTPUT, 
		@RequireOtherProducts bit OUTPUT, 
		@RequiredProductIds nvarchar(1000) OUTPUT, 
		@AutomaticallyAddRequiredProducts bit OUTPUT, 
		@IsDownload bit OUTPUT, 
		@DownloadId int OUTPUT, 
		@UnlimitedDownloads bit OUTPUT, 
		@MaxNumberOfDownloads int OUTPUT, 
		@DownloadExpirationDays int OUTPUT, 
		@DownloadActivationTypeId int OUTPUT, 
		@HasSampleDownload bit OUTPUT, 
		@SampleDownloadId int OUTPUT, 
		@HasUserAgreement bit OUTPUT, 
		@UserAgreementText nvarchar(MAX) OUTPUT, 
		@IsRecurring bit OUTPUT, 
		@RecurringCycleLength int OUTPUT, 
		@RecurringCyclePeriodId int OUTPUT, 
		@RecurringTotalCycles int OUTPUT, 
		@IsShipEnabled bit OUTPUT, 
		@IsFreeShipping bit OUTPUT, 
		@AdditionalShippingCharge decimal(18,4) OUTPUT, 
		@IsTaxExempt bit OUTPUT, 
		@TaxCategoryId int OUTPUT, 
		@ManageInventoryMethodId int OUTPUT, 
		@StockQuantity int OUTPUT, 
		@DisplayStockAvailability bit OUTPUT, 
		@DisplayStockQuantity bit OUTPUT, 
		@MinStockQuantity int OUTPUT, 
		@LowStockActivityId int OUTPUT, 
		@NotifyAdminForQuantityBelow int OUTPUT, 
		@BackorderModeId int OUTPUT, 
		@AllowBackInStockSubscriptions bit OUTPUT, 
		@OrderMinimumQuantity int OUTPUT, 
		@OrderMaximumQuantity int OUTPUT, 
		@AllowedQuantities nvarchar(1000) OUTPUT, 
		@DisableBuyButton bit OUTPUT, 
		@DisableWishlistButton bit OUTPUT, 
		@AvailableForPreOrder bit OUTPUT, 
		@CallForPrice bit OUTPUT, 
		@Price decimal(18,4) OUTPUT, 
		@OldPrice decimal(18,4) OUTPUT,
		@ProductCost decimal(18,4) OUTPUT, 
		@SpecialPrice decimal(18,4) OUTPUT, 
		@SpecialPriceStartDateTimeUtc datetime OUTPUT, 
		@SpecialPriceEndDateTimeUtc datetime OUTPUT, 
		@CustomerEntersPrice bit OUTPUT, 
		@MinimumCustomerEnteredPrice decimal(18,4) OUTPUT, 
		@MaximumCustomerEnteredPrice bit OUTPUT, 
		@HasTierPrices bit OUTPUT,
		@HasDiscountsApplied bit OUTPUT,
		@Weight decimal(18, 4) OUTPUT,
		@Length decimal(18, 4) OUTPUT,
		@Width decimal(18, 4) OUTPUT,
		@Height decimal(18, 4) OUTPUT,
		@PictureId int OUTPUT,
		@AvailableStartDateTimeUtc datetime OUTPUT,
		@AvailableEndDateTimeUtc datetime OUTPUT,
		@Published bit OUTPUT,
		@Deleted bit OUTPUT,
		@DisplayOrder int OUTPUT,
		@CreatedOnUtc datetime OUTPUT,
		@UpdatedOnUtc datetime OUTPUT,
		@DeliveryTimeId int OUTPUT,
		@BasePriceEnabled bit OUTPUT,
		@BasePriceMeasureUnit nvarchar(50) OUTPUT,
		@BasePriceAmount decimal(18, 4) OUTPUT,
		@BasePriceBaseAmount int OUTPUT',
		@ProductId OUTPUT,
		@Name OUTPUT,
		@Description OUTPUT,
		@Sku OUTPUT,
		@ManufacturerPartNumber OUTPUT,
		@Gtin OUTPUT,
		@IsGiftCard OUTPUT,
		@GiftCardTypeId OUTPUT,
		@RequireOtherProducts OUTPUT,
		@RequiredProductIds OUTPUT,
		@AutomaticallyAddRequiredProducts OUTPUT,
		@IsDownload OUTPUT,
		@DownloadId OUTPUT,
		@UnlimitedDownloads OUTPUT,
		@MaxNumberOfDownloads OUTPUT,
		@DownloadExpirationDays OUTPUT,
		@DownloadActivationTypeId OUTPUT,
		@HasSampleDownload OUTPUT,
		@SampleDownloadId OUTPUT,
		@HasUserAgreement OUTPUT,
		@UserAgreementText OUTPUT,
		@IsRecurring OUTPUT,
		@RecurringCycleLength OUTPUT,
		@RecurringCyclePeriodId OUTPUT,
		@RecurringTotalCycles OUTPUT,
		@IsShipEnabled OUTPUT,
		@IsFreeShipping OUTPUT,
		@AdditionalShippingCharge OUTPUT,
		@IsTaxExempt OUTPUT,
		@TaxCategoryId OUTPUT,
		@ManageInventoryMethodId OUTPUT,
		@StockQuantity OUTPUT,
		@DisplayStockAvailability OUTPUT,
		@DisplayStockQuantity OUTPUT,
		@MinStockQuantity OUTPUT,
		@LowStockActivityId OUTPUT,
		@NotifyAdminForQuantityBelow OUTPUT,
		@BackorderModeId OUTPUT,
		@AllowBackInStockSubscriptions OUTPUT,
		@OrderMinimumQuantity OUTPUT,
		@OrderMaximumQuantity OUTPUT,
		@AllowedQuantities OUTPUT,
		@DisableBuyButton OUTPUT,
		@DisableWishlistButton OUTPUT,
		@AvailableForPreOrder OUTPUT,
		@CallForPrice OUTPUT,
		@Price OUTPUT,
		@OldPrice OUTPUT,
		@ProductCost OUTPUT,
		@SpecialPrice OUTPUT,
		@SpecialPriceStartDateTimeUtc OUTPUT,
		@SpecialPriceEndDateTimeUtc OUTPUT,
		@CustomerEntersPrice OUTPUT,
		@MinimumCustomerEnteredPrice OUTPUT,
		@MaximumCustomerEnteredPrice OUTPUT,
		@HasTierPrices OUTPUT,
		@HasDiscountsApplied OUTPUT,
		@Weight OUTPUT,
		@Length OUTPUT,
		@Width OUTPUT,
		@Height OUTPUT,
		@PictureId OUTPUT,
		@AvailableStartDateTimeUtc OUTPUT,
		@AvailableEndDateTimeUtc OUTPUT,
		@Published OUTPUT,
		@Deleted OUTPUT,
		@DisplayOrder OUTPUT,
		@CreatedOnUtc OUTPUT,
		@UpdatedOnUtc OUTPUT,
		@DeliveryTimeId OUTPUT,
		@BasePriceEnabled OUTPUT,
		@BasePriceMeasureUnit OUTPUT,
		@BasePriceAmount OUTPUT,
		@BasePriceBaseAmount OUTPUT
		
		--how many variants do we have?
		DECLARE @NumberOfVariants int
		SELECT @NumberOfVariants = COUNT(1) FROM [ProductVariant] WHERE [ProductId]=@ProductId And [Deleted] = 0
		
		DECLARE @NumberOfAllVariants int
		SELECT @NumberOfAllVariants = COUNT(1) FROM [ProductVariant] WHERE [ProductId]=@ProductId
		
		--product templates
		DECLARE @SimpleProductTemplateId int
		SELECT @SimpleProductTemplateId = [Id] FROM [ProductTemplate] WHERE [ViewPath] = N'ProductTemplate.Simple'
		DECLARE @GroupedProductTemplateId int
		SELECT @GroupedProductTemplateId = [Id] FROM [ProductTemplate] WHERE [ViewPath] = N'ProductTemplate.Grouped'
		
		--new product id:
		--if we have a simple product it'll be the same
		--if we have a grouped product, then it'll be the identifier of a new associated product 
		DECLARE @NewProductId int
		SET @NewProductId = null -- clear cache (variable scope)
			
		--process a product (simple or grouped)
		IF (@NumberOfVariants <= 1)
		BEGIN
			--simple product
			UPDATE [Product] 
			SET [ProductTypeId] = 5,
			[ParentGroupedProductId] = 0,
			[Sku] = @Sku,
			[ManufacturerPartNumber] = @ManufacturerPartNumber,
			[Gtin] = @Gtin,
			[IsGiftCard] = @IsGiftCard,
			[GiftCardTypeId] = @GiftCardTypeId,
			[RequireOtherProducts] = @RequireOtherProducts,
			--a store owner should manually update [RequiredProductIds] property after upgrade
			--[RequiredProductIds] = @RequiredProductIds,
			[AutomaticallyAddRequiredProducts] = @AutomaticallyAddRequiredProducts,
			[IsDownload] = @IsDownload,
			[DownloadId] = @DownloadId,
			[UnlimitedDownloads] = @UnlimitedDownloads,
			[MaxNumberOfDownloads] = @MaxNumberOfDownloads,
			[DownloadExpirationDays] = @DownloadExpirationDays,
			[DownloadActivationTypeId] = @DownloadActivationTypeId,
			[HasSampleDownload] = @HasSampleDownload,
			[SampleDownloadId] = @SampleDownloadId,
			[HasUserAgreement] = @HasUserAgreement,
			[UserAgreementText] = @UserAgreementText,
			[IsRecurring] = @IsRecurring,
			[RecurringCycleLength] = @RecurringCycleLength,
			[RecurringCyclePeriodId] = @RecurringCyclePeriodId,
			[RecurringTotalCycles] = @RecurringTotalCycles,
			[IsShipEnabled] = @IsShipEnabled,
			[IsFreeShipping] = @IsFreeShipping,
			[AdditionalShippingCharge] = @AdditionalShippingCharge,
			[IsTaxExempt] = @IsTaxExempt,
			[TaxCategoryId] = @TaxCategoryId,
			[ManageInventoryMethodId] = @ManageInventoryMethodId,
			[StockQuantity] = @StockQuantity,
			[DisplayStockAvailability] = @DisplayStockAvailability,
			[DisplayStockQuantity] = @DisplayStockQuantity,
			[MinStockQuantity] = @MinStockQuantity,
			[LowStockActivityId] = @LowStockActivityId,
			[NotifyAdminForQuantityBelow] = @NotifyAdminForQuantityBelow,
			[BackorderModeId] = @BackorderModeId,
			[AllowBackInStockSubscriptions] = @AllowBackInStockSubscriptions,
			[OrderMinimumQuantity] = @OrderMinimumQuantity,
			[OrderMaximumQuantity] = @OrderMaximumQuantity,
			[AllowedQuantities] = @AllowedQuantities,
			[DisableBuyButton] = @DisableBuyButton,
			[DisableWishlistButton] = @DisableWishlistButton,
			[AvailableForPreOrder] = @AvailableForPreOrder,
			[CallForPrice] = @CallForPrice,
			[Price] = @Price,
			[OldPrice] = @OldPrice,
			[ProductCost] = @ProductCost,
			[SpecialPrice] = @SpecialPrice,
			[SpecialPriceStartDateTimeUtc] = @SpecialPriceStartDateTimeUtc,
			[SpecialPriceEndDateTimeUtc] = @SpecialPriceEndDateTimeUtc,
			[CustomerEntersPrice] = @CustomerEntersPrice,
			[MinimumCustomerEnteredPrice] = @MinimumCustomerEnteredPrice,
			[MaximumCustomerEnteredPrice] = @MaximumCustomerEnteredPrice,
			[HasTierPrices] = @HasTierPrices,
			[HasDiscountsApplied] = @HasDiscountsApplied,
			[Weight] = @Weight,
			[Length] = @Length,
			[Width] = @Width,
			[Height] = @Height,
			[AvailableStartDateTimeUtc] = @AvailableStartDateTimeUtc,
			[AvailableEndDateTimeUtc] = @AvailableEndDateTimeUtc,
			[DeliveryTimeId] = @DeliveryTimeId,
			[BasePriceEnabled] = @BasePriceEnabled,
			[BasePriceMeasureUnit] = @BasePriceMeasureUnit,
			[BasePriceAmount] = @BasePriceAmount,
			[BasePriceBaseAmount] = @BasePriceBaseAmount
			WHERE [Id]=@ProductId
			
			--product type
			UPDATE [Product]
			SET [ProductTypeId]=5
			WHERE [Id]=@ProductId
			
			--product template
			UPDATE [Product]
			SET [ProductTemplateId]=@SimpleProductTemplateId
			WHERE [Id]=@ProductId
			
			--deleted?
			IF (@Deleted = 1 And @NumberOfAllVariants <= 1)
			BEGIN
				UPDATE [Product]
				SET [Deleted]=@Deleted
				WHERE [Id]=@ProductId
			END
			
			--published?
			IF (@Published = 0 And @NumberOfAllVariants <= 1)
			BEGIN
				UPDATE [Product]
				SET [Published]=@Published
				WHERE [Id]=@ProductId
			END
			
			SET @NewProductId = @ProductId
		END ELSE 
		BEGIN
			--grouped product
			UPDATE [Product] 
			SET [ProductTypeId] = 10,
			[ParentGroupedProductId] = 0,
			[Sku] = null,
			[ManufacturerPartNumber] = null,
			[Gtin] = null,
			[IsGiftCard] = 0,
			[GiftCardTypeId] = 0,
			[RequireOtherProducts] = 0,
			[RequiredProductIds] = null,
			[AutomaticallyAddRequiredProducts] = 0,
			[IsDownload] = 0,
			[DownloadId] = 0,
			[UnlimitedDownloads] = @UnlimitedDownloads,
			[MaxNumberOfDownloads] = @MaxNumberOfDownloads,
			[DownloadExpirationDays] = @DownloadExpirationDays,
			[DownloadActivationTypeId] = @DownloadActivationTypeId,
			[HasSampleDownload] = 0,
			[SampleDownloadId] = 0,
			[HasUserAgreement] = @HasUserAgreement,
			[UserAgreementText] = @UserAgreementText,
			[IsRecurring] = @IsRecurring,
			[RecurringCycleLength] = @RecurringCycleLength,
			[RecurringCyclePeriodId] = @RecurringCyclePeriodId,
			[RecurringTotalCycles] = @RecurringTotalCycles,
			[IsShipEnabled] = @IsShipEnabled,
			[IsFreeShipping] = @IsFreeShipping,
			[AdditionalShippingCharge] = @AdditionalShippingCharge,
			[IsTaxExempt] = @IsTaxExempt,
			[TaxCategoryId] = @TaxCategoryId,
			[ManageInventoryMethodId] = @ManageInventoryMethodId,
			[StockQuantity] = @StockQuantity,
			[DisplayStockAvailability] = @DisplayStockAvailability,
			[DisplayStockQuantity] = @DisplayStockQuantity,
			[MinStockQuantity] = @MinStockQuantity,
			[LowStockActivityId] = @LowStockActivityId,
			[NotifyAdminForQuantityBelow] = @NotifyAdminForQuantityBelow,
			[BackorderModeId] = @BackorderModeId,
			[AllowBackInStockSubscriptions] = @AllowBackInStockSubscriptions,
			[OrderMinimumQuantity] = @OrderMinimumQuantity,
			[OrderMaximumQuantity] = @OrderMaximumQuantity,
			[AllowedQuantities] = @AllowedQuantities,
			[DisableBuyButton] = @DisableBuyButton,
			[DisableWishlistButton] = @DisableWishlistButton,
			[AvailableForPreOrder] = @AvailableForPreOrder,
			[CallForPrice] = @CallForPrice,
			[Price] = @Price,
			[OldPrice] = @OldPrice,
			[ProductCost] = @ProductCost,
			[SpecialPrice] = @SpecialPrice,
			[SpecialPriceStartDateTimeUtc] = @SpecialPriceStartDateTimeUtc,
			[SpecialPriceEndDateTimeUtc] = @SpecialPriceEndDateTimeUtc,
			[CustomerEntersPrice] = @CustomerEntersPrice,
			[MinimumCustomerEnteredPrice] = @MinimumCustomerEnteredPrice,
			[MaximumCustomerEnteredPrice] = @MaximumCustomerEnteredPrice,
			[HasTierPrices] = 0,
			[HasDiscountsApplied] = 0,
			[Weight] = @Weight,
			[Length] = @Length,
			[Width] = @Width,
			[Height] = @Height,
			[AvailableStartDateTimeUtc] = @AvailableStartDateTimeUtc,
			[AvailableEndDateTimeUtc] = @AvailableEndDateTimeUtc,
			[DeliveryTimeId] = @DeliveryTimeId,
			[BasePriceEnabled] = @BasePriceEnabled,
			[BasePriceMeasureUnit] = @BasePriceMeasureUnit,
			[BasePriceAmount] = @BasePriceAmount,
			[BasePriceBaseAmount] = @BasePriceBaseAmount
			WHERE [Id]=@ProductId
			
			--product type
			UPDATE [Product]
			SET [ProductTypeId]=10
			WHERE [Id]=@ProductId
			--product template
			UPDATE [Product]
			SET [ProductTemplateId]=@GroupedProductTemplateId
			WHERE [Id]=@ProductId
			
			--insert a product variant (now we name it an associated product)
			DECLARE @AssociatedProductName nvarchar(1000)
			SELECT @AssociatedProductName = [Name] FROM [Product] WHERE [Id]=@ProductId
			--append a product variant name
			IF (len(@Name) > 0)
			BEGIN
				SET @AssociatedProductName = @AssociatedProductName + ' ' + @Name
			END
						
			--published?
			DECLARE @AssociatedProductPublished bit
			SELECT @AssociatedProductPublished = [Published] FROM [Product] WHERE [Id]=@ProductId
			IF (@Published = 0)
			BEGIN
				SET @AssociatedProductPublished = @Published
			END
			
			--deleted?
			DECLARE @AssociatedProductDeleted bit
			SELECT @AssociatedProductDeleted = [Deleted] FROM [Product] WHERE [Id]=@ProductId
			IF (@Deleted = 1)
			BEGIN
				SET @AssociatedProductDeleted = @Deleted
			END
			
			INSERT INTO [Product]
			(Name, ShortDescription, ProductTemplateId, ShowOnHomePage,
			AllowCustomerReviews, ApprovedRatingSum, NotApprovedRatingSum, ApprovedTotalReviews,
			NotApprovedTotalReviews, SubjectToAcl, LimitedToStores, Published, Deleted, CreatedOnUtc, UpdatedOnUtc, 
			Sku, ManufacturerPartNumber, Gtin,
			IsGiftCard, GiftCardTypeId, RequireOtherProducts, AutomaticallyAddRequiredProducts, IsDownload, 
			DownloadId, UnlimitedDownloads, MaxNumberOfDownloads, DownloadExpirationDays, DownloadActivationTypeId, HasSampleDownload,
			SampleDownloadId, HasUserAgreement, UserAgreementText, 
			IsRecurring, RecurringCycleLength, RecurringCyclePeriodId,
			RecurringTotalCycles, IsShipEnabled, IsFreeShipping, AdditionalShippingCharge, IsTaxExempt, TaxCategoryId, ManageInventoryMethodId,
			StockQuantity, DisplayStockAvailability, DisplayStockQuantity, MinStockQuantity, LowStockActivityId, 
			NotifyAdminForQuantityBelow, BackorderModeId, AllowBackInStockSubscriptions, OrderMinimumQuantity, OrderMaximumQuantity, 
			AllowedQuantities, DisableBuyButton, DisableWishlistButton, AvailableForPreOrder, CallForPrice, Price, OldPrice, ProductCost, 
			SpecialPrice, SpecialPriceStartDateTimeUtc, SpecialPriceEndDateTimeUtc,
			CustomerEntersPrice, MinimumCustomerEnteredPrice, MaximumCustomerEnteredPrice, HasTierPrices, 
			HasDiscountsApplied, Weight, Length, Width, Height,
			AvailableStartDateTimeUtc, AvailableEndDateTimeUtc,
			DeliveryTimeId, BasePriceEnabled, BasePriceMeasureUnit, BasePriceAmount, BasePriceBaseAmount,			
			ProductTypeId, ParentGroupedProductId) 
			VALUES (@AssociatedProductName, @Description, @SimpleProductTemplateId, 
			0, 0, 0, 0, 
			0, 0, 0, 0, @AssociatedProductPublished, 
			@AssociatedProductDeleted, @CreatedOnUtc, @UpdatedOnUtc, 
			@Sku, @ManufacturerPartNumber, @Gtin,
			@IsGiftCard, @GiftCardTypeId, @RequireOtherProducts, 
			--a store owner should manually update [RequiredProductIds] property after upgrade
			@AutomaticallyAddRequiredProducts, @IsDownload, @DownloadId, @UnlimitedDownloads, @MaxNumberOfDownloads, 
			@DownloadExpirationDays, @DownloadActivationTypeId, @HasSampleDownload, @SampleDownloadId, 
			@HasUserAgreement, @UserAgreementText, @IsRecurring, 
			@RecurringCycleLength, @RecurringCyclePeriodId, @RecurringTotalCycles, @IsShipEnabled, @IsFreeShipping, 
			@AdditionalShippingCharge, @IsTaxExempt, @TaxCategoryId, @ManageInventoryMethodId, @StockQuantity, 
			@DisplayStockAvailability, @DisplayStockQuantity, @MinStockQuantity, @LowStockActivityId, 
			@NotifyAdminForQuantityBelow, @BackorderModeId, @AllowBackInStockSubscriptions, @OrderMinimumQuantity, 
			@OrderMaximumQuantity, @AllowedQuantities, @DisableBuyButton, @DisableWishlistButton, @AvailableForPreOrder, @CallForPrice, 
			@Price, @OldPrice, @ProductCost, @SpecialPrice, 
			@SpecialPriceStartDateTimeUtc, @SpecialPriceEndDateTimeUtc, @CustomerEntersPrice, 
			@MinimumCustomerEnteredPrice, @MaximumCustomerEnteredPrice, @HasTierPrices, @HasDiscountsApplied, 
			@Weight, @Length, @Width, @Height, 
			@AvailableStartDateTimeUtc, @AvailableEndDateTimeUtc,
			@DeliveryTimeId, @BasePriceEnabled, @BasePriceMeasureUnit, @BasePriceAmount, @BasePriceBaseAmount,			
			--simple product
			5 , @ProductId)
			
			SET @NewProductId = @@IDENTITY
			
			--product variant picture
			IF (@PictureId > 0)
			BEGIN
				INSERT INTO [Product_Picture_Mapping] ([ProductId], [PictureId], [DisplayOrder])
				VALUES (@NewProductId, @PictureId, 1)
			END
		END
		
		--back in stock subscriptions. move ProductVariantId to the new ProductId column
		IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[BackInStockSubscription]') and NAME='ProductVariantId')
		BEGIN
			EXEC sp_executesql N'UPDATE [BackInStockSubscription]
			SET [ProductId] = @NewProductId
			WHERE [ProductVariantId] = @ExistingProductVariantID',
			N'@NewProductId int OUTPUT, 
			@ExistingProductVariantID int OUTPUT',
			@NewProductId OUTPUT,
			@ExistingProductVariantID OUTPUT			
		END
		
		--order items. move ProductVariantId to the new ProductId column
		IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[OrderItem]') and NAME='ProductVariantId')
		BEGIN
			EXEC sp_executesql N'UPDATE [OrderItem]
			SET [ProductId] = @NewProductId
			WHERE [ProductVariantId] = @ExistingProductVariantID',
			N'@NewProductId int OUTPUT, 
			@ExistingProductVariantID int OUTPUT',
			@NewProductId OUTPUT,
			@ExistingProductVariantID OUTPUT			
		END
		
		--product variant attributes. move ProductVariantId to the new ProductId column
		IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[ProductVariant_ProductAttribute_Mapping]') and NAME='ProductVariantId')
		BEGIN
			EXEC sp_executesql N'UPDATE [ProductVariant_ProductAttribute_Mapping]
			SET [ProductId] = @NewProductId
			WHERE [ProductVariantId] = @ExistingProductVariantID',
			N'@NewProductId int OUTPUT, 
			@ExistingProductVariantID int OUTPUT',
			@NewProductId OUTPUT,
			@ExistingProductVariantID OUTPUT			
		END
		
		--attribute combinations. move ProductVariantId to the new ProductId column
		IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[ProductVariantAttributeCombination]') and NAME='ProductVariantId')
		BEGIN
			EXEC sp_executesql N'UPDATE [ProductVariantAttributeCombination]
			SET [ProductId] = @NewProductId
			WHERE [ProductVariantId] = @ExistingProductVariantID',
			N'@NewProductId int OUTPUT, 
			@ExistingProductVariantID int OUTPUT',
			@NewProductId OUTPUT,
			@ExistingProductVariantID OUTPUT			
		END
		
		--shopping cart items. move ProductVariantId to the new ProductId column
		IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[ShoppingCartItem]') and NAME='ProductVariantId')
		BEGIN
			EXEC sp_executesql N'UPDATE [ShoppingCartItem]
			SET [ProductId] = @NewProductId
			WHERE [ProductVariantId] = @ExistingProductVariantID',
			N'@NewProductId int OUTPUT, 
			@ExistingProductVariantID int OUTPUT',
			@NewProductId OUTPUT,
			@ExistingProductVariantID OUTPUT			
		END
		
		--tier prices. move ProductVariantId to the new ProductId column
		IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[TierPrice]') and NAME='ProductVariantId')
		BEGIN
			EXEC sp_executesql N'UPDATE [TierPrice]
			SET [ProductId] = @NewProductId
			WHERE [ProductVariantId] = @ExistingProductVariantID',
			N'@NewProductId int OUTPUT, 
			@ExistingProductVariantID int OUTPUT',
			@NewProductId OUTPUT,
			@ExistingProductVariantID OUTPUT			
		END
		
		--discounts. move ProductVariantId to the new ProductId column
		IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Discount_AppliedToProducts]') and NAME='ProductVariant_Id')
		BEGIN
			EXEC sp_executesql N'UPDATE [Discount_AppliedToProducts]
			SET [Product_Id] = @NewProductId
			WHERE [ProductVariant_Id] = @ExistingProductVariantID',
			N'@NewProductId int, 
			@ExistingProductVariantID int',
			@NewProductId,
			@ExistingProductVariantID			
		END
		
				
		--fetch next product variant identifier
		FETCH NEXT FROM cur_existingproductvariant INTO @ExistingProductVariantID
	END
	CLOSE cur_existingproductvariant
	DEALLOCATE cur_existingproductvariant
END
GO

--back in stock subscriptions
ALTER TABLE [BackInStockSubscription]
ALTER COLUMN [ProductId] int NOT NULL
GO
IF NOT EXISTS (SELECT 1
           FROM   sys.objects
           WHERE  name = 'BackInStockSubscription_Product'
           AND parent_object_id = Object_id('BackInStockSubscription')
           AND Objectproperty(object_id,N'IsForeignKey') = 1)
BEGIN
	ALTER TABLE [dbo].[BackInStockSubscription] WITH CHECK ADD CONSTRAINT [BackInStockSubscription_Product] FOREIGN KEY([ProductId])
	REFERENCES [dbo].[Product] ([Id])
	ON DELETE CASCADE
END
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[BackInStockSubscription]') and NAME='ProductVariantId')
BEGIN
	ALTER TABLE [BackInStockSubscription]
	DROP COLUMN [ProductVariantId]
END
GO

--order items
ALTER TABLE [OrderItem]
ALTER COLUMN [ProductId] int NOT NULL
GO
IF NOT EXISTS (SELECT 1
           FROM   sys.objects
           WHERE  name = 'OrderItem_Product'
           AND parent_object_id = Object_id('OrderItem')
           AND Objectproperty(object_id,N'IsForeignKey') = 1)
BEGIN
	ALTER TABLE [dbo].[OrderItem] WITH CHECK ADD CONSTRAINT [OrderItem_Product] FOREIGN KEY([ProductId])
	REFERENCES [dbo].[Product] ([Id])
	ON DELETE CASCADE
END
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[OrderItem]') and NAME='ProductVariantId')
BEGIN
	ALTER TABLE [OrderItem]
	DROP COLUMN [ProductVariantId]
END
GO

--product variant attributes
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[ProductVariant_ProductAttribute_Mapping]') and OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	ALTER TABLE [ProductVariant_ProductAttribute_Mapping]
	ALTER COLUMN [ProductId] int NOT NULL
END
GO
IF NOT EXISTS (SELECT 1
           FROM   sys.objects
           WHERE  name = 'ProductVariantAttribute_Product'
           AND parent_object_id = Object_id('ProductVariant_ProductAttribute_Mapping')
           AND Objectproperty(object_id,N'IsForeignKey') = 1)
BEGIN
	--one more validatation here because we'll rename [ProductVariant_ProductAttribute_Mapping] table a bit later
	IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Product_ProductAttribute_Mapping]') and OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
	BEGIN
		ALTER TABLE [dbo].[ProductVariant_ProductAttribute_Mapping] WITH CHECK ADD CONSTRAINT [ProductVariantAttribute_Product] FOREIGN KEY([ProductId])
		REFERENCES [dbo].[Product] ([Id])
		ON DELETE CASCADE
	END
END
GO
IF EXISTS (SELECT 1 from sys.indexes WHERE [NAME]=N'IX_ProductVariant_ProductAttribute_Mapping_ProductVariantId' and object_id=object_id(N'[ProductVariant_ProductAttribute_Mapping]'))
BEGIN
	DROP INDEX [IX_ProductVariant_ProductAttribute_Mapping_ProductVariantId] ON [ProductVariant_ProductAttribute_Mapping]
	CREATE NONCLUSTERED INDEX [IX_Product_ProductAttribute_Mapping_ProductId] ON [ProductVariant_ProductAttribute_Mapping] ([ProductId] ASC)
END
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[ProductVariant_ProductAttribute_Mapping]') and NAME='ProductVariantId')
BEGIN
	ALTER TABLE [ProductVariant_ProductAttribute_Mapping]
	DROP COLUMN [ProductVariantId]
END
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[ProductVariant_ProductAttribute_Mapping]') and OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	EXEC sp_rename 'ProductVariant_ProductAttribute_Mapping', 'Product_ProductAttribute_Mapping';
END
GO
--attribute combinations
ALTER TABLE [ProductVariantAttributeCombination]
ALTER COLUMN [ProductId] int NOT NULL
GO
IF NOT EXISTS (SELECT 1
           FROM   sys.objects
           WHERE  name = 'ProductVariantAttributeCombination_Product'
           AND parent_object_id = Object_id('ProductVariantAttributeCombination')
           AND Objectproperty(object_id,N'IsForeignKey') = 1)
BEGIN
	ALTER TABLE [dbo].[ProductVariantAttributeCombination] WITH CHECK ADD CONSTRAINT [ProductVariantAttributeCombination_Product] FOREIGN KEY([ProductId])
	REFERENCES [dbo].[Product] ([Id])
	ON DELETE CASCADE
END
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[ProductVariantAttributeCombination]') and NAME='ProductVariantId')
BEGIN
	ALTER TABLE [ProductVariantAttributeCombination]
	DROP COLUMN [ProductVariantId]
END
GO
--shopping cart items
ALTER TABLE [ShoppingCartItem]
ALTER COLUMN [ProductId] int NOT NULL
GO
IF NOT EXISTS (SELECT 1
           FROM   sys.objects
           WHERE  name = 'ShoppingCartItem_Product'
           AND parent_object_id = Object_id('ShoppingCartItem')
           AND Objectproperty(object_id,N'IsForeignKey') = 1)
BEGIN
	ALTER TABLE [dbo].[ShoppingCartItem] WITH CHECK ADD CONSTRAINT [ShoppingCartItem_Product] FOREIGN KEY([ProductId])
	REFERENCES [dbo].[Product] ([Id])
	ON DELETE CASCADE
END
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[ShoppingCartItem]') and NAME='ProductVariantId')
BEGIN
	ALTER TABLE [ShoppingCartItem]
	DROP COLUMN [ProductVariantId]
END
GO
--tier prices
ALTER TABLE [TierPrice]
ALTER COLUMN [ProductId] int NOT NULL
GO
IF NOT EXISTS (SELECT 1
           FROM   sys.objects
           WHERE  name = 'TierPrice_Product'
           AND parent_object_id = Object_id('TierPrice')
           AND Objectproperty(object_id,N'IsForeignKey') = 1)
BEGIN
	ALTER TABLE [dbo].[TierPrice] WITH CHECK ADD CONSTRAINT [TierPrice_Product] FOREIGN KEY([ProductId])
	REFERENCES [dbo].[Product] ([Id])
	ON DELETE CASCADE
END
GO
IF EXISTS (SELECT 1 from sys.indexes WHERE [NAME]=N'IX_TierPrice_ProductVariantId' and object_id=object_id(N'[TierPrice]'))
BEGIN
	DROP INDEX [IX_TierPrice_ProductVariantId] ON [TierPrice]
	CREATE NONCLUSTERED INDEX [IX_TierPrice_ProductId] ON [TierPrice] ([ProductId] ASC)
END
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[TierPrice]') and NAME='ProductVariantId')
BEGIN
	ALTER TABLE [TierPrice]
	DROP COLUMN [ProductVariantId]
END
GO
--discounts
IF NOT EXISTS (SELECT 1
           FROM   sys.objects
           WHERE  name = 'Discount_AppliedToProducts_Source'
           AND parent_object_id = Object_id('Discount_AppliedToProducts')
           AND Objectproperty(object_id,N'IsForeignKey') = 1)
BEGIN
	ALTER TABLE [dbo].[Discount_AppliedToProducts] WITH CHECK ADD CONSTRAINT [Discount_AppliedToProducts_Source] FOREIGN KEY([Discount_Id])
	REFERENCES [dbo].[Discount] ([Id])
	ON DELETE CASCADE
END
GO
IF NOT EXISTS (SELECT 1
           FROM   sys.objects
           WHERE  name = 'Discount_AppliedToProducts_Target'
           AND parent_object_id = Object_id('Discount_AppliedToProducts')
           AND Objectproperty(object_id,N'IsForeignKey') = 1)
BEGIN
	ALTER TABLE [dbo].[Discount_AppliedToProducts] WITH CHECK ADD CONSTRAINT [Discount_AppliedToProducts_Target] FOREIGN KEY([Product_Id])
	REFERENCES [dbo].[Product] ([Id])
	ON DELETE CASCADE
END
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Discount_AppliedToProducts]') and NAME='ProductVariant_Id')
BEGIN
	ALTER TABLE [Discount_AppliedToProducts]
	DROP COLUMN [ProductVariant_Id]
END
GO

--drop product variant table
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[ProductVariant]') and OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	DROP TABLE [ProductVariant]
END
GO

--new Product columns. Set "NOT NULL" where required
ALTER TABLE [Product]
ALTER COLUMN [ParentGroupedProductId] int NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [ProductTypeId] int NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [IsGiftCard] bit NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [GiftCardTypeId] int NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [RequireOtherProducts] bit NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [AutomaticallyAddRequiredProducts] bit NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [IsDownload] bit NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [DownloadId] int NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [UnlimitedDownloads] bit NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [MaxNumberOfDownloads] int NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [DownloadActivationTypeId] int NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [HasSampleDownload] bit NOT NULL
GO

Update [Product] SET SampleDownloadId = null WHERE SampleDownloadId = 0
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('[Product]') and NAME='IX_SampleDownloadId')
BEGIN
	CREATE INDEX [IX_SampleDownloadId] ON [Product]([SampleDownloadId])
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE name = 'FK_dbo.Product_dbo.Download_SampleDownloadId' AND type = 'F')
BEGIN
	ALTER TABLE [Product] WITH CHECK ADD CONSTRAINT [FK_dbo.Product_dbo.Download_SampleDownloadId] FOREIGN KEY ([SampleDownloadId]) REFERENCES [Download] ([Id])
END
GO
--ALTER TABLE [Product]
--ALTER COLUMN [SampleDownloadId] int NOT NULL
--GO

ALTER TABLE [Product]
ALTER COLUMN [HasUserAgreement] bit NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [IsRecurring] bit NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [RecurringCycleLength] int NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [RecurringCyclePeriodId] int NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [RecurringTotalCycles] int NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [IsShipEnabled] bit NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [IsFreeShipping] bit NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [AdditionalShippingCharge] decimal(18,4) NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [IsTaxExempt] bit NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [TaxCategoryId] int NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [ManageInventoryMethodId] int NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [StockQuantity] int NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [DisplayStockAvailability] bit NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [DisplayStockQuantity] bit NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [MinStockQuantity] int NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [LowStockActivityId] int NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [NotifyAdminForQuantityBelow] int NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [BackorderModeId] int NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [AllowBackInStockSubscriptions] bit NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [OrderMinimumQuantity] int NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [OrderMaximumQuantity] int NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [DisableBuyButton] bit NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [DisableWishlistButton] bit NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [AvailableForPreOrder] bit NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [CallForPrice] bit NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [Price] decimal(18, 4) NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [OldPrice] decimal(18, 4) NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [ProductCost] decimal(18, 4) NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [CustomerEntersPrice] bit NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [MinimumCustomerEnteredPrice] decimal(18, 4) NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [MaximumCustomerEnteredPrice] decimal(18, 4) NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [HasTierPrices] bit NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [HasDiscountsApplied] bit NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [Weight] decimal(18, 4) NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [Length] decimal(18, 4) NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [Width] decimal(18, 4) NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [Height] decimal(18, 4) NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [BasePriceEnabled] bit NOT NULL
GO

-- new indexes
IF NOT EXISTS (SELECT 1 from sys.indexes WHERE [NAME]=N'IX_Product_PriceDatesEtc' and object_id=object_id(N'[Product]'))
BEGIN
	CREATE NONCLUSTERED INDEX [IX_Product_PriceDatesEtc] ON [Product]  ([Price] ASC, [AvailableStartDateTimeUtc] ASC, [AvailableEndDateTimeUtc] ASC, [Published] ASC, [Deleted] ASC)
END
GO

IF NOT EXISTS (SELECT 1 from sys.indexes WHERE [NAME]=N'IX_Product_ParentGroupedProductId' and object_id=object_id(N'[Product]'))
BEGIN
	CREATE NONCLUSTERED INDEX [IX_Product_ParentGroupedProductId] ON [Product] ([ParentGroupedProductId] ASC)
END
GO

IF NOT EXISTS (SELECT 1 from sys.indexes WHERE [NAME]=N'IX_Product_Name' and object_id=object_id(N'[Product]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Product_Name] ON [Product] ([Name] ASC)
END
GO

IF NOT EXISTS (SELECT 1 from sys.indexes WHERE [NAME]=N'IX_Product_Sku' and object_id=object_id(N'[Product]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Product_Sku] ON [Product] ([Sku] ASC)
END
GO

IF NOT EXISTS (SELECT 1 from sys.indexes WHERE [NAME]=N'IX_ProductVariantAttributeCombination_SKU' and object_id=object_id(N'[ProductVariantAttributeCombination]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ProductVariantAttributeCombination_SKU] ON [ProductVariantAttributeCombination] ([SKU] ASC)
END
GO

--you have to manually re-configure "google products" (froogle) plugin
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[GoogleProduct]') and OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	DELETE FROM [GoogleProduct]
	
	IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[GoogleProduct]') and NAME='ProductVariantId')
	BEGIN
		EXEC sp_rename 'GoogleProduct.ProductVariantId', 'ProductId', 'COLUMN';
	END
END
GO

IF EXISTS (
		SELECT *
		FROM sys.objects
		WHERE object_id = OBJECT_ID(N'[temp_generate_sename]') AND OBJECTPROPERTY(object_id,N'IsProcedure') = 1)
DROP PROCEDURE [dbo].[temp_generate_sename]
GO
CREATE PROCEDURE [dbo].[temp_generate_sename]
(
    @table_name nvarchar(1000),
    @entity_id int,
    @result nvarchar(1000) OUTPUT
)
AS
BEGIN
	--get current name
	DECLARE @current_sename nvarchar(1000)
	DECLARE @sql nvarchar(4000)
	
	SET @sql = 'SELECT @current_sename = [Name] FROM [' + @table_name + '] WHERE [Id] = ' + ISNULL(CAST(@entity_id AS nvarchar(max)), '0')
	EXEC sp_executesql @sql,N'@current_sename nvarchar(1000) OUTPUT',@current_sename OUTPUT		
    
    --generate se name    
	DECLARE @new_sename nvarchar(1000)
    SET @new_sename = ''
    --ensure only allowed chars
    DECLARE @allowed_se_chars varchar(4000)
    --Note for store owners: add more chars below if want them to be supported when migrating your data
    SET @allowed_se_chars = N'abcdefghijklmnopqrstuvwxyz1234567890 _-'
    DECLARE @l int
    SET @l = len(@current_sename)
    DECLARE @p int
    SET @p = 1
    WHILE @p <= @l
    BEGIN
		DECLARE @c nvarchar(1)
        SET @c = substring(@current_sename, @p, 1)
        IF CHARINDEX(@c,@allowed_se_chars) > 0
        BEGIN
			SET @new_sename = @new_sename + @c
		END
		SET @p = @p + 1
	END
	--replace spaces with '-'
	SELECT @new_sename = REPLACE(@new_sename,' ','-');
    WHILE CHARINDEX('--',@new_sename) > 0
		SELECT @new_sename = REPLACE(@new_sename,'--','-');
    WHILE CHARINDEX('__',@new_sename) > 0
		SELECT @new_sename = REPLACE(@new_sename,'__','_');
    --ensure not empty
    IF (@new_sename is null or @new_sename = '')
		SELECT @new_sename = ISNULL(CAST(@entity_id AS nvarchar(max)), '0');
    --lowercase
	SELECT @new_sename = LOWER(@new_sename)
	--ensure this sename is not reserved
	WHILE (1=1)
	BEGIN
		DECLARE @sename_is_already_reserved bit
		SET @sename_is_already_reserved = 0
		SET @sql = 'IF EXISTS (SELECT 1 FROM [UrlRecord] WHERE [Slug] = @sename AND [EntityId] <> ' + ISNULL(CAST(@entity_id AS nvarchar(max)), '0') + ')
					BEGIN
						SELECT @sename_is_already_reserved = 1
					END'
		EXEC sp_executesql @sql,N'@sename nvarchar(1000), @sename_is_already_reserved nvarchar(4000) OUTPUT',@new_sename,@sename_is_already_reserved OUTPUT
		
		IF (@sename_is_already_reserved > 0)
		BEGIN
			--add some digit to the end in this case
			SET @new_sename = @new_sename + '-1'
		END
		ELSE
		BEGIN
			BREAK
		END
	END
	
	--return
    SET @result = @new_sename
END
GO

--set search engine friendly name (UrlRecord) for associated products (new products added before in this upgrade script). [ParentGroupedProductId] > 0
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Product]') and OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	DECLARE @sename_existing_entity_id int
	DECLARE cur_sename_existing_entity CURSOR FOR
	SELECT [Id]
	FROM [Product]
	WHERE [ParentGroupedProductId] > 0
	OPEN cur_sename_existing_entity
	FETCH NEXT FROM cur_sename_existing_entity INTO @sename_existing_entity_id
	WHILE @@FETCH_STATUS = 0
	BEGIN
		DECLARE @sename nvarchar(1000)	
		SET @sename = null -- clear cache (variable scope)
		
		DECLARE @table_name nvarchar(1000)	
		SET @table_name = N'Product'
		
		--main sename
		EXEC	[dbo].[temp_generate_sename]
				@table_name = @table_name,
				@entity_id = @sename_existing_entity_id,
				@result = @sename OUTPUT
				
		IF EXISTS(SELECT 1 FROM [UrlRecord] WHERE [LanguageId]=0 AND [EntityId]=@sename_existing_entity_id AND [EntityName]=@table_name)
		BEGIN
			UPDATE [UrlRecord]
			SET [Slug] = @sename
			WHERE [LanguageId]=0 AND [EntityId]=@sename_existing_entity_id AND [EntityName]=@table_name
		END
		ELSE
		BEGIN
			INSERT INTO [UrlRecord] ([EntityId], [EntityName], [Slug], [LanguageId], [IsActive])
			VALUES (@sename_existing_entity_id, @table_name, @sename, 0, 1)
		END		

		--fetch next identifier
		FETCH NEXT FROM cur_sename_existing_entity INTO @sename_existing_entity_id
	END
	CLOSE cur_sename_existing_entity
	DEALLOCATE cur_sename_existing_entity
END
GO

--drop temporary procedures & functions
IF EXISTS (
		SELECT *
		FROM sys.objects
		WHERE object_id = OBJECT_ID(N'[temp_generate_sename]') AND OBJECTPROPERTY(object_id,N'IsProcedure') = 1)
DROP PROCEDURE [temp_generate_sename]
GO

--new Product property
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='VisibleIndividually')
BEGIN
	ALTER TABLE [Product]
	ADD [VisibleIndividually] bit NULL
END
GO

UPDATE [Product]
SET [VisibleIndividually] = 0
WHERE [VisibleIndividually] IS NULL AND [ParentGroupedProductId] > 0
GO
UPDATE [Product]
SET [VisibleIndividually] = 1
WHERE [VisibleIndividually] IS NULL AND [ParentGroupedProductId] = 0
GO

ALTER TABLE [Product] ALTER COLUMN [VisibleIndividually] bit NOT NULL
GO

--more indexes
IF NOT EXISTS (SELECT 1 from sys.indexes WHERE [NAME]=N'IX_Product_VisibleIndividually' and object_id=object_id(N'[Product]'))
BEGIN
	CREATE NONCLUSTERED INDEX [IX_Product_VisibleIndividually] ON [Product] ([VisibleIndividually] ASC)
END
GO

--new [DisplayOrder] property
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='DisplayOrder')
BEGIN
	ALTER TABLE [Product]
	ADD [DisplayOrder] int NULL
END
GO

UPDATE [Product] SET [DisplayOrder] = 0
GO
ALTER TABLE [Product] ALTER COLUMN [DisplayOrder] int NOT NULL
GO

--updated product type values
UPDATE [Product] SET [ProductTypeId]=5 WHERE [ProductTypeId]=0
GO


IF EXISTS (SELECT *
           FROM   sys.objects
           WHERE  object_id = OBJECT_ID(N'[dbo].[nop_splitstring_to_table]')
                  AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
  DROP FUNCTION [dbo].[nop_splitstring_to_table]
GO

IF EXISTS (SELECT *
           FROM   sys.objects
           WHERE  object_id = OBJECT_ID(N'[dbo].[nop_getnotnullnotempty]')
                  AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
  DROP FUNCTION [dbo].[nop_getnotnullnotempty]
GO

IF EXISTS (SELECT *
           FROM   sys.objects
           WHERE  object_id = OBJECT_ID(N'[dbo].[nop_getprimarykey_indexname]')
                  AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
  DROP FUNCTION [dbo].[nop_getprimarykey_indexname]
GO

IF EXISTS (SELECT 1 FROM sys.fulltext_catalogs WHERE name = 'nopCommerceFullTextCatalog')
BEGIN
	EXEC('
		UPDATE [Setting] SET [Value] = ''False'' WHERE [Name] = N''commonsettings.usefulltextsearch''
	')
	EXEC('
		IF EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = object_id(''[Product]''))
			DROP FULLTEXT INDEX ON [Product]	
	')
	EXEC('
		IF EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = object_id(''[ProductVariantAttributeCombination]''))
			DROP FULLTEXT INDEX ON [ProductVariantAttributeCombination]
	')
	EXEC('
		IF EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = object_id(''[LocalizedProperty]''))
			DROP FULLTEXT INDEX ON [LocalizedProperty]
	')
	EXEC('
		IF EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = object_id(''[ProductTag]''))
			DROP FULLTEXT INDEX ON [ProductTag]
	')
	EXEC('
		IF EXISTS (SELECT 1 FROM sys.fulltext_catalogs WHERE [name] = ''nopCommerceFullTextCatalog'')
			DROP FULLTEXT CATALOG [nopCommerceFullTextCatalog]
	')
END
GO

IF NOT EXISTS (SELECT *
           FROM   sys.objects
           WHERE  object_id = OBJECT_ID(N'[dbo].[sm_splitstring_to_table]')
                  AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
BEGIN
	EXEC('
		CREATE FUNCTION [dbo].[sm_splitstring_to_table]
		(
			@string NVARCHAR(MAX),
			@delimiter CHAR(1)
		)
		RETURNS @output TABLE(
			data NVARCHAR(MAX)
		)
		BEGIN
			DECLARE @start INT, @end INT
			SELECT @start = 1, @end = CHARINDEX(@delimiter, @string)

			WHILE @start < LEN(@string) + 1 BEGIN
				IF @end = 0 
					SET @end = LEN(@string) + 1

				INSERT INTO @output (data) 
				VALUES(SUBSTRING(@string, @start, @end - @start))
				SET @start = @end + 1
				SET @end = CHARINDEX(@delimiter, @string, @start)
			END
			RETURN
		END
	')
END
GO

IF NOT EXISTS (SELECT *
           FROM   sys.objects
           WHERE  object_id = OBJECT_ID(N'[dbo].[sm_getnotnullnotempty]')
                  AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
BEGIN
	EXEC('
		CREATE FUNCTION [dbo].[sm_getnotnullnotempty]
		(
			@p1 nvarchar(max) = null, 
			@p2 nvarchar(max) = null
		)
		RETURNS nvarchar(max)
		AS
		BEGIN
			IF @p1 IS NULL
				return @p2
			IF @p1 =''''
				return @p2

			return @p1
		END
	')
END
GO

IF NOT EXISTS (SELECT *
           FROM   sys.objects
           WHERE  object_id = OBJECT_ID(N'[dbo].[sm_getprimarykey_indexname]')
                  AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
BEGIN
	EXEC('
		CREATE FUNCTION [dbo].[sm_getprimarykey_indexname]
		(
			@table_name nvarchar(1000) = null
		)
		RETURNS nvarchar(1000)
		AS
		BEGIN
			DECLARE @index_name nvarchar(1000)

			SELECT @index_name = i.name
			FROM sys.tables AS tbl
			INNER JOIN sys.indexes AS i ON (i.index_id > 0 and i.is_hypothetical = 0) AND (i.object_id=tbl.object_id)
			WHERE (i.is_unique=1 and i.is_disabled=0) and (tbl.name=@table_name)

			RETURN @index_name
		END
	')
END
GO

IF EXISTS (
		SELECT *
		FROM sys.objects
		WHERE object_id = OBJECT_ID(N'[FullText_Enable]') AND OBJECTPROPERTY(object_id,N'IsProcedure') = 1)
DROP PROCEDURE [FullText_Enable]
GO
CREATE PROCEDURE [FullText_Enable]
AS
BEGIN
	--create catalog
	EXEC('
	IF NOT EXISTS (SELECT 1 FROM sys.fulltext_catalogs WHERE [name] = ''SmartStoreNETFullTextCatalog'')
		CREATE FULLTEXT CATALOG [SmartStoreNETFullTextCatalog] AS DEFAULT')
	
	--create indexes
	DECLARE @create_index_text nvarchar(4000)
	SET @create_index_text = '
	IF NOT EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = object_id(''[Product]''))
		CREATE FULLTEXT INDEX ON [Product]([Name], [ShortDescription], [FullDescription], [Sku])
		KEY INDEX [' + dbo.[sm_getprimarykey_indexname] ('Product') +  '] ON [SmartStoreNETFullTextCatalog] WITH CHANGE_TRACKING AUTO'
	EXEC(@create_index_text)
	
	SET @create_index_text = '
	IF NOT EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = object_id(''[ProductVariantAttributeCombination]''))
		CREATE FULLTEXT INDEX ON [ProductVariantAttributeCombination]([SKU])
		KEY INDEX [' + dbo.[sm_getprimarykey_indexname] ('ProductVariantAttributeCombination') +  '] ON [SmartStoreNETFullTextCatalog] WITH CHANGE_TRACKING AUTO'
	EXEC(@create_index_text)

	SET @create_index_text = '
	IF NOT EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = object_id(''[LocalizedProperty]''))
		CREATE FULLTEXT INDEX ON [LocalizedProperty]([LocaleValue])
		KEY INDEX [' + dbo.[sm_getprimarykey_indexname] ('LocalizedProperty') +  '] ON [SmartStoreNETFullTextCatalog] WITH CHANGE_TRACKING AUTO'
	EXEC(@create_index_text)

	SET @create_index_text = '
	IF NOT EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = object_id(''[ProductTag]''))
		CREATE FULLTEXT INDEX ON [ProductTag]([Name])
		KEY INDEX [' + dbo.[sm_getprimarykey_indexname] ('ProductTag') +  '] ON [SmartStoreNETFullTextCatalog] WITH CHANGE_TRACKING AUTO'
	EXEC(@create_index_text)
END
GO

IF EXISTS (
		SELECT *
		FROM sys.objects
		WHERE object_id = OBJECT_ID(N'[FullText_Disable]') AND OBJECTPROPERTY(object_id,N'IsProcedure') = 1)
DROP PROCEDURE [FullText_Disable]
GO
CREATE PROCEDURE [FullText_Disable]
AS
BEGIN
	EXEC('
	--drop indexes
	IF EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = object_id(''[Product]''))
		DROP FULLTEXT INDEX ON [Product]
	')
	
	EXEC('
	IF EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = object_id(''[ProductVariantAttributeCombination]''))
		DROP FULLTEXT INDEX ON [ProductVariantAttributeCombination]
	')

	EXEC('
	IF EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = object_id(''[LocalizedProperty]''))
		DROP FULLTEXT INDEX ON [LocalizedProperty]
	')

	EXEC('
	IF EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = object_id(''[ProductTag]''))
		DROP FULLTEXT INDEX ON [ProductTag]
	')

	--drop catalog
	EXEC('
	IF EXISTS (SELECT 1 FROM sys.fulltext_catalogs WHERE [name] = ''SmartStoreNETFullTextCatalog'')
		DROP FULLTEXT CATALOG [SmartStoreNETFullTextCatalog]
	')
END
GO



IF EXISTS (
		SELECT *
		FROM sys.objects
		WHERE object_id = OBJECT_ID(N'[ProductLoadAllPaged]') AND OBJECTPROPERTY(object_id,N'IsProcedure') = 1)
DROP PROCEDURE [ProductLoadAllPaged]
GO
CREATE PROCEDURE [dbo].[ProductLoadAllPaged]
(
	@CategoryIds		nvarchar(MAX) = null,	--a list of category IDs (comma-separated list). e.g. 1,2,3
	@ManufacturerId		int = 0,
	@StoreId			int = 0,
	@ParentGroupedProductId	int = 0,
	@ProductTypeId		int = null, --product type identifier, null - load all products
	@VisibleIndividuallyOnly bit = 0, 	--0 - load all products , 1 - "visible indivially" only
	@ProductTagId		int = 0,
	@FeaturedProducts	bit = null,	--0 featured only , 1 not featured only, null - load all products
	@PriceMin			decimal(18, 4) = null,
	@PriceMax			decimal(18, 4) = null,
	@Keywords			nvarchar(4000) = null,
	@SearchDescriptions bit = 0, --a value indicating whether to search by a specified "keyword" in product descriptions
	@SearchSku			bit = 0, --a value indicating whether to search by a specified "keyword" in product SKU
	@SearchProductTags  bit = 0, --a value indicating whether to search by a specified "keyword" in product tags
	@UseFullTextSearch  bit = 0,
	@FullTextMode		int = 0, --0 - using CONTAINS with <prefix_term>, 5 - using CONTAINS and OR with <prefix_term>, 10 - using CONTAINS and AND with <prefix_term>
	@FilteredSpecs		nvarchar(MAX) = null,	--filter by attributes (comma-separated list). e.g. 14,15,16
	@LanguageId			int = 0,
	@OrderBy			int = 0, --0 - position, 5 - Name: A to Z, 6 - Name: Z to A, 10 - Price: Low to High, 11 - Price: High to Low, 15 - creation date
	@AllowedCustomerRoleIds	nvarchar(MAX) = null,	--a list of customer role IDs (comma-separated list) for which a product should be shown (if a subjet to ACL)
	@PageIndex			int = 0, 
	@PageSize			int = 2147483644,
	@ShowHidden			bit = 0,
	@LoadFilterableSpecificationAttributeOptionIds bit = 0, --a value indicating whether we should load the specification attribute option identifiers applied to loaded products (all pages)
	@FilterableSpecificationAttributeOptionIds nvarchar(MAX) = null OUTPUT, --the specification attribute option identifiers applied to loaded products (all pages). returned as a comma separated list of identifiers
	@TotalRecords		int = null OUTPUT
)
AS
BEGIN
	
	/* Products that filtered by keywords */
	CREATE TABLE #KeywordProducts
	(
		[ProductId] int NOT NULL
	)

	DECLARE
		@SearchKeywords bit,
		@sql nvarchar(max),
		@sql_orderby nvarchar(max)

	SET NOCOUNT ON
	
	--filter by keywords
	SET @Keywords = isnull(@Keywords, '')
	SET @Keywords = rtrim(ltrim(@Keywords))
	IF ISNULL(@Keywords, '') != ''
	BEGIN
		SET @SearchKeywords = 1
		
		IF @UseFullTextSearch = 1
		BEGIN
			--remove wrong chars (' ")
			SET @Keywords = REPLACE(@Keywords, '''', '')
			SET @Keywords = REPLACE(@Keywords, '"', '')
			
			--full-text search
			IF @FullTextMode = 0 
			BEGIN
				--0 - using CONTAINS with <prefix_term>
				SET @Keywords = ' "' + @Keywords + '*" '
			END
			ELSE
			BEGIN
				--5 - using CONTAINS and OR with <prefix_term>
				--10 - using CONTAINS and AND with <prefix_term>

				--clean multiple spaces
				WHILE CHARINDEX('  ', @Keywords) > 0 
					SET @Keywords = REPLACE(@Keywords, '  ', ' ')

				DECLARE @concat_term nvarchar(100)				
				IF @FullTextMode = 5 --5 - using CONTAINS and OR with <prefix_term>
				BEGIN
					SET @concat_term = 'OR'
				END 
				IF @FullTextMode = 10 --10 - using CONTAINS and AND with <prefix_term>
				BEGIN
					SET @concat_term = 'AND'
				END

				--now let's build search string
				declare @fulltext_keywords nvarchar(4000)
				set @fulltext_keywords = N''
				declare @index int		
		
				set @index = CHARINDEX(' ', @Keywords, 0)

				-- if index = 0, then only one field was passed
				IF(@index = 0)
					set @fulltext_keywords = ' "' + @Keywords + '*" '
				ELSE
				BEGIN		
					DECLARE @first BIT
					SET  @first = 1			
					WHILE @index > 0
					BEGIN
						IF (@first = 0)
							SET @fulltext_keywords = @fulltext_keywords + ' ' + @concat_term + ' '
						ELSE
							SET @first = 0

						SET @fulltext_keywords = @fulltext_keywords + '"' + SUBSTRING(@Keywords, 1, @index - 1) + '*"'					
						SET @Keywords = SUBSTRING(@Keywords, @index + 1, LEN(@Keywords) - @index)						
						SET @index = CHARINDEX(' ', @Keywords, 0)
					end
					
					-- add the last field
					IF LEN(@fulltext_keywords) > 0
						SET @fulltext_keywords = @fulltext_keywords + ' ' + @concat_term + ' ' + '"' + SUBSTRING(@Keywords, 1, LEN(@Keywords)) + '*"'	
				END
				SET @Keywords = @fulltext_keywords
			END
		END
		ELSE
		BEGIN
			--usual search by PATINDEX
			SET @Keywords = '%' + @Keywords + '%'
		END
		--PRINT @Keywords

		--product name
		SET @sql = '
		INSERT INTO #KeywordProducts ([ProductId])
		SELECT p.Id
		FROM Product p with (NOLOCK)
		WHERE '
		IF @UseFullTextSearch = 1
			SET @sql = @sql + 'CONTAINS(p.[Name], @Keywords) '
		ELSE
			SET @sql = @sql + 'PATINDEX(@Keywords, p.[Name]) > 0 '


		--localized product name
		SET @sql = @sql + '
		UNION
		SELECT lp.EntityId
		FROM LocalizedProperty lp with (NOLOCK)
		WHERE
			lp.LocaleKeyGroup = N''Product''
			AND lp.LanguageId = ' + ISNULL(CAST(@LanguageId AS nvarchar(max)), '0') + '
			AND lp.LocaleKey = N''Name'''
		IF @UseFullTextSearch = 1
			SET @sql = @sql + ' AND CONTAINS(lp.[LocaleValue], @Keywords) '
		ELSE
			SET @sql = @sql + ' AND PATINDEX(@Keywords, lp.[LocaleValue]) > 0 '
	

		IF @SearchDescriptions = 1
		BEGIN
			--product short description
			SET @sql = @sql + '
			UNION
			SELECT p.Id
			FROM Product p with (NOLOCK)
			WHERE '
			IF @UseFullTextSearch = 1
				SET @sql = @sql + 'CONTAINS(p.[ShortDescription], @Keywords) '
			ELSE
				SET @sql = @sql + 'PATINDEX(@Keywords, p.[ShortDescription]) > 0 '


			--product full description
			SET @sql = @sql + '
			UNION
			SELECT p.Id
			FROM Product p with (NOLOCK)
			WHERE '
			IF @UseFullTextSearch = 1
				SET @sql = @sql + 'CONTAINS(p.[FullDescription], @Keywords) '
			ELSE
				SET @sql = @sql + 'PATINDEX(@Keywords, p.[FullDescription]) > 0 '



			--localized product short description
			SET @sql = @sql + '
			UNION
			SELECT lp.EntityId
			FROM LocalizedProperty lp with (NOLOCK)
			WHERE
				lp.LocaleKeyGroup = N''Product''
				AND lp.LanguageId = ' + ISNULL(CAST(@LanguageId AS nvarchar(max)), '0') + '
				AND lp.LocaleKey = N''ShortDescription'''
			IF @UseFullTextSearch = 1
				SET @sql = @sql + ' AND CONTAINS(lp.[LocaleValue], @Keywords) '
			ELSE
				SET @sql = @sql + ' AND PATINDEX(@Keywords, lp.[LocaleValue]) > 0 '
				

			--localized product full description
			SET @sql = @sql + '
			UNION
			SELECT lp.EntityId
			FROM LocalizedProperty lp with (NOLOCK)
			WHERE
				lp.LocaleKeyGroup = N''Product''
				AND lp.LanguageId = ' + ISNULL(CAST(@LanguageId AS nvarchar(max)), '0') + '
				AND lp.LocaleKey = N''FullDescription'''
			IF @UseFullTextSearch = 1
				SET @sql = @sql + ' AND CONTAINS(lp.[LocaleValue], @Keywords) '
			ELSE
				SET @sql = @sql + ' AND PATINDEX(@Keywords, lp.[LocaleValue]) > 0 '
		END

		--SKU
		IF @SearchSku = 1
		BEGIN
			SET @sql = @sql + '
			UNION
			SELECT p.Id
			FROM Product p with (NOLOCK)
			LEFT OUTER JOIN ProductVariantAttributeCombination pvac with(NOLOCK) ON pvac.ProductId = p.Id
			WHERE '
			IF @UseFullTextSearch = 1
				SET @sql = @sql + '(CONTAINS(pvac.[Sku], @Keywords) OR CONTAINS(p.[Sku], @Keywords)) '
			ELSE
				SET @sql = @sql + 'PATINDEX(@Keywords, pvac.[Sku]) > 0 OR PATINDEX(@Keywords, p.[Sku]) > 0 '
		END

		IF @SearchProductTags = 1
		BEGIN
			--product tag
			SET @sql = @sql + '
			UNION
			SELECT pptm.Product_Id
			FROM Product_ProductTag_Mapping pptm with(NOLOCK) INNER JOIN ProductTag pt with(NOLOCK) ON pt.Id = pptm.ProductTag_Id
			WHERE '
			IF @UseFullTextSearch = 1
				SET @sql = @sql + 'CONTAINS(pt.[Name], @Keywords) '
			ELSE
				SET @sql = @sql + 'PATINDEX(@Keywords, pt.[Name]) > 0 '

			--localized product tag
			SET @sql = @sql + '
			UNION
			SELECT pptm.Product_Id
			FROM LocalizedProperty lp with (NOLOCK) INNER JOIN Product_ProductTag_Mapping pptm with(NOLOCK) ON lp.EntityId = pptm.ProductTag_Id
			WHERE
				lp.LocaleKeyGroup = N''ProductTag''
				AND lp.LanguageId = ' + ISNULL(CAST(@LanguageId AS nvarchar(max)), '0') + '
				AND lp.LocaleKey = N''Name'''
			IF @UseFullTextSearch = 1
				SET @sql = @sql + ' AND CONTAINS(lp.[LocaleValue], @Keywords) '
			ELSE
				SET @sql = @sql + ' AND PATINDEX(@Keywords, lp.[LocaleValue]) > 0 '
		END

		--PRINT (@sql)
		EXEC sp_executesql @sql, N'@Keywords nvarchar(4000)', @Keywords

	END
	ELSE
	BEGIN
		SET @SearchKeywords = 0
	END

	--filter by category IDs
	SET @CategoryIds = isnull(@CategoryIds, '')	
	CREATE TABLE #FilteredCategoryIds
	(
		CategoryId int not null
	)
	INSERT INTO #FilteredCategoryIds (CategoryId)
	SELECT CAST(data as int) FROM [sm_splitstring_to_table](@CategoryIds, ',')	
	DECLARE @CategoryIdsCount int	
	SET @CategoryIdsCount = (SELECT COUNT(1) FROM #FilteredCategoryIds)

	--filter by attributes
	SET @FilteredSpecs = isnull(@FilteredSpecs, '')	
	CREATE TABLE #FilteredSpecs
	(
		SpecificationAttributeOptionId int not null
	)
	INSERT INTO #FilteredSpecs (SpecificationAttributeOptionId)
	SELECT CAST(data as int) FROM [sm_splitstring_to_table](@FilteredSpecs, ',')
	DECLARE @SpecAttributesCount int	
	SET @SpecAttributesCount = (SELECT COUNT(1) FROM #FilteredSpecs)

	--filter by customer role IDs (access control list)
	SET @AllowedCustomerRoleIds = isnull(@AllowedCustomerRoleIds, '')	
	CREATE TABLE #FilteredCustomerRoleIds
	(
		CustomerRoleId int not null
	)
	INSERT INTO #FilteredCustomerRoleIds (CustomerRoleId)
	SELECT CAST(data as int) FROM [sm_splitstring_to_table](@AllowedCustomerRoleIds, ',')
	
	--paging
	DECLARE @PageLowerBound int
	DECLARE @PageUpperBound int
	DECLARE @RowsToReturn int
	SET @RowsToReturn = @PageSize * (@PageIndex + 1)	
	SET @PageLowerBound = @PageSize * @PageIndex
	SET @PageUpperBound = @PageLowerBound + @PageSize + 1
	
	CREATE TABLE #DisplayOrderTmp 
	(
		[Id] int IDENTITY (1, 1) NOT NULL,
		[ProductId] int NOT NULL
	)

	SET @sql = '
	INSERT INTO #DisplayOrderTmp ([ProductId])
	SELECT p.Id
	FROM
		Product p with (NOLOCK)'
	
	IF @CategoryIdsCount > 0
	BEGIN
		SET @sql = @sql + '
		LEFT JOIN Product_Category_Mapping pcm with (NOLOCK)
			ON p.Id = pcm.ProductId'
	END
	
	IF @ManufacturerId > 0
	BEGIN
		SET @sql = @sql + '
		LEFT JOIN Product_Manufacturer_Mapping pmm with (NOLOCK)
			ON p.Id = pmm.ProductId'
	END
	
	IF ISNULL(@ProductTagId, 0) != 0
	BEGIN
		SET @sql = @sql + '
		LEFT JOIN Product_ProductTag_Mapping pptm with (NOLOCK)
			ON p.Id = pptm.Product_Id'
	END
		
	--searching by keywords
	IF @SearchKeywords = 1
	BEGIN
		SET @sql = @sql + '
		JOIN #KeywordProducts kp
			ON  p.Id = kp.ProductId'
	END
	
	SET @sql = @sql + '
	WHERE
		p.Deleted = 0'
	
	--filter by category
	IF @CategoryIdsCount > 0
	BEGIN
		SET @sql = @sql + '
		AND pcm.CategoryId IN (SELECT CategoryId FROM #FilteredCategoryIds)'
		
		IF @FeaturedProducts IS NOT NULL
		BEGIN
			SET @sql = @sql + '
		AND pcm.IsFeaturedProduct = ' + CAST(@FeaturedProducts AS nvarchar(max))
		END
	END
	
	--filter by manufacturer
	IF @ManufacturerId > 0
	BEGIN
		SET @sql = @sql + '
		AND pmm.ManufacturerId = ' + CAST(@ManufacturerId AS nvarchar(max))
		
		IF @FeaturedProducts IS NOT NULL
		BEGIN
			SET @sql = @sql + '
		AND pmm.IsFeaturedProduct = ' + CAST(@FeaturedProducts AS nvarchar(max))
		END
	END
	
	--filter by parent grouped product identifer
	IF @ParentGroupedProductId > 0
	BEGIN
		SET @sql = @sql + '
		AND p.ParentGroupedProductId = ' + CAST(@ParentGroupedProductId AS nvarchar(max))
	END
	
	--filter by product type
	IF @ProductTypeId is not null
	BEGIN
		SET @sql = @sql + '
		AND p.ProductTypeId = ' + CAST(@ProductTypeId AS nvarchar(max))
	END
	
	--filter by visible individually
	IF @VisibleIndividuallyOnly = 1
	BEGIN
		SET @sql = @sql + '
		AND p.VisibleIndividually = 1'
	END
	
	--filter by product tag
	IF ISNULL(@ProductTagId, 0) != 0
	BEGIN
		SET @sql = @sql + '
		AND pptm.ProductTag_Id = ' + CAST(@ProductTagId AS nvarchar(max))
	END
	
	--show hidden
	IF @ShowHidden = 0
	BEGIN
		SET @sql = @sql + '
		AND p.Published = 1
		AND p.Deleted = 0
		AND (getutcdate() BETWEEN ISNULL(p.AvailableStartDateTimeUtc, ''1/1/1900'') and ISNULL(p.AvailableEndDateTimeUtc, ''1/1/2999''))'
	END
	
	--min price
	IF @PriceMin > 0
	BEGIN
		SET @sql = @sql + '
		AND (
				(
					--special price (specified price and valid date range)
					(p.SpecialPrice IS NOT NULL AND (getutcdate() BETWEEN isnull(p.SpecialPriceStartDateTimeUtc, ''1/1/1900'') AND isnull(p.SpecialPriceEndDateTimeUtc, ''1/1/2999'')))
					AND
					(p.SpecialPrice >= ' + CAST(@PriceMin AS nvarchar(max)) + ')
				)
				OR 
				(
					--regular price (price isnt specified or date range isnt valid)
					(p.SpecialPrice IS NULL OR (getutcdate() NOT BETWEEN isnull(p.SpecialPriceStartDateTimeUtc, ''1/1/1900'') AND isnull(p.SpecialPriceEndDateTimeUtc, ''1/1/2999'')))
					AND
					(p.Price >= ' + CAST(@PriceMin AS nvarchar(max)) + ')
				)
			)'
	END
	
	--max price
	IF @PriceMax > 0
	BEGIN
		SET @sql = @sql + '
		AND (
				(
					--special price (specified price and valid date range)
					(p.SpecialPrice IS NOT NULL AND (getutcdate() BETWEEN isnull(p.SpecialPriceStartDateTimeUtc, ''1/1/1900'') AND isnull(p.SpecialPriceEndDateTimeUtc, ''1/1/2999'')))
					AND
					(p.SpecialPrice <= ' + CAST(@PriceMax AS nvarchar(max)) + ')
				)
				OR 
				(
					--regular price (price isnt specified or date range isnt valid)
					(p.SpecialPrice IS NULL OR (getutcdate() NOT BETWEEN isnull(p.SpecialPriceStartDateTimeUtc, ''1/1/1900'') AND isnull(p.SpecialPriceEndDateTimeUtc, ''1/1/2999'')))
					AND
					(p.Price <= ' + CAST(@PriceMax AS nvarchar(max)) + ')
				)
			)'
	END
	
	--show hidden and ACL
	IF @ShowHidden = 0
	BEGIN
		SET @sql = @sql + '
		AND (p.SubjectToAcl = 0 OR EXISTS (
			SELECT 1 FROM #FilteredCustomerRoleIds [fcr]
			WHERE
				[fcr].CustomerRoleId IN (
					SELECT [acl].CustomerRoleId
					FROM [AclRecord] acl with (NOLOCK)
					WHERE [acl].EntityId = p.Id AND [acl].EntityName = ''Product''
				)
			))'
	END
	
	--show hidden and filter by store
	IF @StoreId > 0
	BEGIN
		SET @sql = @sql + '
		AND (p.LimitedToStores = 0 OR EXISTS (
			SELECT 1 FROM [StoreMapping] sm with (NOLOCK)
			WHERE [sm].EntityId = p.Id AND [sm].EntityName = ''Product'' and [sm].StoreId=' + CAST(@StoreId AS nvarchar(max)) + '
			))'
	END
	
	--filter by specs
	IF @SpecAttributesCount > 0
	BEGIN
		SET @sql = @sql + '
		AND NOT EXISTS (
			SELECT 1 FROM #FilteredSpecs [fs]
			WHERE
				[fs].SpecificationAttributeOptionId NOT IN (
					SELECT psam.SpecificationAttributeOptionId
					FROM Product_SpecificationAttribute_Mapping psam with (NOLOCK)
					WHERE psam.AllowFiltering = 1 AND psam.ProductId = p.Id
				)
			)'
	END
	
	--sorting
	SET @sql_orderby = ''	
	IF @OrderBy = 5 /* Name: A to Z */
		SET @sql_orderby = ' p.[Name] ASC'
	ELSE IF @OrderBy = 6 /* Name: Z to A */
		SET @sql_orderby = ' p.[Name] DESC'
	ELSE IF @OrderBy = 10 /* Price: Low to High */
		SET @sql_orderby = ' p.[Price] ASC'
	ELSE IF @OrderBy = 11 /* Price: High to Low */
		SET @sql_orderby = ' p.[Price] DESC'
	ELSE IF @OrderBy = 15 /* creation date */
		SET @sql_orderby = ' p.[CreatedOnUtc] DESC'
	ELSE /* default sorting, 0 (position) */
	BEGIN
		--category position (display order)
		IF @CategoryIdsCount > 0 SET @sql_orderby = ' pcm.DisplayOrder ASC'
		
		--manufacturer position (display order)
		IF @ManufacturerId > 0
		BEGIN
			IF LEN(@sql_orderby) > 0 SET @sql_orderby = @sql_orderby + ', '
			SET @sql_orderby = @sql_orderby + ' pmm.DisplayOrder ASC'
		END
		
		--parent grouped product specified (sort associated products)
		IF @ParentGroupedProductId > 0
		BEGIN
			IF LEN(@sql_orderby) > 0 SET @sql_orderby = @sql_orderby + ', '
			SET @sql_orderby = @sql_orderby + ' p.[DisplayOrder] ASC'
		END

		--name
		IF LEN(@sql_orderby) > 0 SET @sql_orderby = @sql_orderby + ', '
		SET @sql_orderby = @sql_orderby + ' p.[Name] ASC'
	END
	
	SET @sql = @sql + '
	ORDER BY' + @sql_orderby
	
	--PRINT (@sql)
	EXEC sp_executesql @sql

	DROP TABLE #FilteredCategoryIds
	DROP TABLE #FilteredSpecs
	DROP TABLE #FilteredCustomerRoleIds
	DROP TABLE #KeywordProducts

	CREATE TABLE #PageIndex 
	(
		[IndexId] int IDENTITY (1, 1) NOT NULL,
		[ProductId] int NOT NULL
	)
	INSERT INTO #PageIndex ([ProductId])
	SELECT ProductId
	FROM #DisplayOrderTmp
	GROUP BY ProductId
	ORDER BY min([Id])

	--total records
	SET @TotalRecords = @@rowcount
	
	DROP TABLE #DisplayOrderTmp

	--prepare filterable specification attribute option identifier (if requested)
	IF @LoadFilterableSpecificationAttributeOptionIds = 1
	BEGIN		
		CREATE TABLE #FilterableSpecs 
		(
			[SpecificationAttributeOptionId] int NOT NULL
		)
		INSERT INTO #FilterableSpecs ([SpecificationAttributeOptionId])
		SELECT DISTINCT [psam].SpecificationAttributeOptionId
		FROM [Product_SpecificationAttribute_Mapping] [psam] with (NOLOCK)
		WHERE [psam].[AllowFiltering] = 1
		AND [psam].[ProductId] IN (SELECT [pi].ProductId FROM #PageIndex [pi])

		--build comma separated list of filterable identifiers
		SELECT @FilterableSpecificationAttributeOptionIds = COALESCE(@FilterableSpecificationAttributeOptionIds + ',' , '') + CAST(SpecificationAttributeOptionId as nvarchar(4000))
		FROM #FilterableSpecs

		DROP TABLE #FilterableSpecs
 	END

	--return products
	SELECT TOP (@RowsToReturn)
		p.*
	FROM
		#PageIndex [pi]
		INNER JOIN Product p with (NOLOCK) on p.Id = [pi].[ProductId]
	WHERE
		[pi].IndexId > @PageLowerBound AND 
		[pi].IndexId < @PageUpperBound
	ORDER BY
		[pi].IndexId
	
	DROP TABLE #PageIndex
END
GO
