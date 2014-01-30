IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[ProductBundleItem]') and OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	CREATE TABLE [dbo].[ProductBundleItem]
	(
		[Id] [int] IDENTITY(1,1) NOT NULL,
		[ProductId] int NOT NULL,
		[BundleProductId] int NOT NULL,		
		[Quantity] int NOT NULL,
		[Discount] [decimal](18, 4) NULL,
		[DiscountPercentage] bit NOT NULL,
		[Name] [nvarchar](400) NULL,
		[ShortDescription] [nvarchar](max) NULL,
		[FilterAttributes] bit NOT NULL,
		[HideThumbnail] bit NOT NULL,
		[Visible] bit NOT NULL,
		[Published] bit NOT NULL,
		[DisplayOrder] int NOT NULL,
		[CreatedOnUtc] [datetime] NOT NULL,
		[UpdatedOnUtc] [datetime] NOT NULL
		
		PRIMARY KEY CLUSTERED 
		(
			[Id] ASC
		) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
	)
	
	EXEC ('
		ALTER TABLE [dbo].[ProductBundleItem] WITH CHECK ADD CONSTRAINT [ProductBundleItem_Product] FOREIGN KEY([ProductId])
		REFERENCES [dbo].[Product] ([Id])
	')
	
	EXEC ('
		ALTER TABLE [dbo].[ProductBundleItem] CHECK CONSTRAINT [ProductBundleItem_Product]
	')
	
	EXEC ('
		CREATE NONCLUSTERED INDEX [IX_ProductBundleItem_ProductId] ON [ProductBundleItem] ([ProductId] ASC)
	')

	EXEC ('
		ALTER TABLE [dbo].[ProductBundleItem] WITH CHECK ADD CONSTRAINT [ProductBundleItem_BundleProduct] FOREIGN KEY([BundleProductId])
		REFERENCES [dbo].[Product] ([Id]) ON DELETE CASCADE
	')
	
	EXEC ('
		ALTER TABLE [dbo].[ProductBundleItem] CHECK CONSTRAINT [ProductBundleItem_BundleProduct]
	')
	
	EXEC ('
		CREATE NONCLUSTERED INDEX [IX_ProductBundleItem_BundleProductId] ON [ProductBundleItem] ([BundleProductId] ASC)
	')
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[ProductBundleItemAttributeFilter]') and OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	CREATE TABLE [dbo].[ProductBundleItemAttributeFilter]
	(
		[Id] [int] IDENTITY(1,1) NOT NULL,
		[BundleItemId] int NOT NULL,
		[AttributeId] int NOT NULL,
		[AttributeValueId] int NOT NULL,
		[IsPreSelected] bit NOT NULL

		PRIMARY KEY CLUSTERED 
		(
			[Id] ASC
		) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
	)

	EXEC ('
		ALTER TABLE [dbo].[ProductBundleItemAttributeFilter] WITH CHECK ADD CONSTRAINT [ProductBundleItemAttributeFilter_BundleItem] FOREIGN KEY([BundleItemId])
		REFERENCES [dbo].[ProductBundleItem] ([Id]) ON DELETE CASCADE
	')
	
	EXEC ('
		ALTER TABLE [dbo].[ProductBundleItemAttributeFilter] CHECK CONSTRAINT [ProductBundleItemAttributeFilter_BundleItem]
	')
	
	EXEC ('
		CREATE NONCLUSTERED INDEX [IX_ProductBundleItemAttributeFilter_BundleItemId] ON [ProductBundleItemAttributeFilter] ([BundleItemId] ASC)
	')	
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='BundleTitleText')
BEGIN
	ALTER TABLE [Product] ADD [BundleTitleText] nvarchar(400) NULL
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='BundlePerItemShipping')
BEGIN
	EXEC ('ALTER TABLE [Product] ADD [BundlePerItemShipping] bit NULL')
	EXEC ('UPDATE [Product] SET [BundlePerItemShipping] = 0 WHERE [BundlePerItemShipping] IS NULL')
	EXEC ('ALTER TABLE [Product] ALTER COLUMN [BundlePerItemShipping] bit NOT NULL')
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='BundlePerItemPricing')
BEGIN
	EXEC ('ALTER TABLE [Product] ADD [BundlePerItemPricing] bit NULL')
	EXEC ('UPDATE [Product] SET [BundlePerItemPricing] = 0 WHERE [BundlePerItemPricing] IS NULL')
	EXEC ('ALTER TABLE [Product] ALTER COLUMN [BundlePerItemPricing] bit NOT NULL')
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='BundlePerItemShoppingCart')
BEGIN
	EXEC ('ALTER TABLE [Product] ADD [BundlePerItemShoppingCart] bit NULL')
	EXEC ('UPDATE [Product] SET [BundlePerItemShoppingCart] = 0 WHERE [BundlePerItemShoppingCart] IS NULL')
	EXEC ('ALTER TABLE [Product] ALTER COLUMN [BundlePerItemShoppingCart] bit NOT NULL')
END
GO

IF NOT EXISTS (SELECT 1 FROM [Setting] WHERE [name] = N'mediasettings.bundledproductpicturesize')
BEGIN
	INSERT [Setting] ([Name], [Value], [StoreId]) VALUES (N'mediasettings.bundledproductpicturesize', N'125', 0)
END
GO
