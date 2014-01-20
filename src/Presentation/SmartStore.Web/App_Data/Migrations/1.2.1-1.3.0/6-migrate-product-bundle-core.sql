IF (NOT EXISTS(SELECT 1 FROM [ProductTemplate] WHERE [ViewPath] = N'ProductTemplate.Bundled'))
BEGIN
	INSERT INTO [ProductTemplate] ([Name],[ViewPath],[DisplayOrder])
	VALUES (N'Bundled product',N'ProductTemplate.Bundled',200)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[ProductBundleItem]') and OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	CREATE TABLE [dbo].[ProductBundleItem]
	(
		[Id] [int] IDENTITY(1,1) NOT NULL,
		[ProductId] int NOT NULL,
		[ParentBundledProductId] int NOT NULL,		
		[Quantity] int NOT NULL,
		[Discount] [decimal](18, 4) NULL,
		[OverrideName] bit NOT NULL,
		[Name] [nvarchar](400) NULL,
		[OverrideShortDescription] bit NOT NULL,
		[ShortDescription] [nvarchar](max) NULL,
		[HideThumbnail] bit NOT NULL,
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
		ALTER TABLE [dbo].[ProductBundleItem] WITH CHECK ADD CONSTRAINT [ProductBundleItem_ParentBundledProduct] FOREIGN KEY([ParentBundledProductId])
		REFERENCES [dbo].[Product] ([Id]) ON DELETE CASCADE
	')
	
	EXEC ('
		ALTER TABLE [dbo].[ProductBundleItem] CHECK CONSTRAINT [ProductBundleItem_ParentBundledProduct]
	')
	
	EXEC ('
		CREATE NONCLUSTERED INDEX [IX_ProductBundleItem_ParentBundledProductId] ON [ProductBundleItem] ([ParentBundledProductId] ASC)
	')
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='BundleTitleText')
BEGIN
	ALTER TABLE [Product] ADD [BundleTitleText] nvarchar(400) NULL
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='BundleNonBundledShipping')
BEGIN
	EXEC ('ALTER TABLE [Product] ADD [BundleNonBundledShipping] bit NULL')
	EXEC ('UPDATE [Product] SET [BundleNonBundledShipping] = 0 WHERE [BundleNonBundledShipping] IS NULL')
	EXEC ('ALTER TABLE [Product] ALTER COLUMN [BundleNonBundledShipping] bit NOT NULL')
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='BundlePerItemPricing')
BEGIN
	EXEC ('ALTER TABLE [Product] ADD [BundlePerItemPricing] bit NULL')
	EXEC ('UPDATE [Product] SET [BundlePerItemPricing] = 0 WHERE [BundlePerItemPricing] IS NULL')
	EXEC ('ALTER TABLE [Product] ALTER COLUMN [BundlePerItemPricing] bit NOT NULL')
END
GO
