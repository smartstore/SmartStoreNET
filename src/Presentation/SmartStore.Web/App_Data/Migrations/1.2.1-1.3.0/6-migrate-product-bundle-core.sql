
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[ProductBundle]') and OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	CREATE TABLE [dbo].[ProductBundle]
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
		[DisplayOrder] int NOT NULL
		
		PRIMARY KEY CLUSTERED 
		(
			[Id] ASC
		) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
	)
	
	EXEC ('
		ALTER TABLE [dbo].[ProductBundle] WITH CHECK ADD CONSTRAINT [ProductBundle_Product] FOREIGN KEY([ProductId])
		REFERENCES [dbo].[Product] ([Id]) ON DELETE CASCADE	
	')
	
	EXEC ('
		ALTER TABLE [dbo].[ProductBundle] CHECK CONSTRAINT [ProductBundle_Product]
	')
	
	EXEC ('
		CREATE NONCLUSTERED INDEX [IX_ProductBundle_ProductId] ON [ProductBundle] ([ProductId] ASC)
	')
	
	EXEC ('
		ALTER TABLE [dbo].[ProductBundle] WITH CHECK ADD CONSTRAINT [ProductBundle_ParentBundledProduct] FOREIGN KEY([ParentBundledProductId])
		REFERENCES [dbo].[Product] ([Id])
	')
	
	EXEC ('
		ALTER TABLE [dbo].[ProductBundle] CHECK CONSTRAINT [ProductBundle_ParentBundledProduct]
	')
	
	EXEC ('
		CREATE NONCLUSTERED INDEX [IX_ProductBundle_ParentBundledProductId] ON [ProductBundle] ([ParentBundledProductId] ASC)
	')
END
