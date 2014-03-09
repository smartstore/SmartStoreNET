IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[ProductVariantAttributeValue]') and NAME='ValueTypeId')
BEGIN
	EXEC ('ALTER TABLE [ProductVariantAttributeValue] ADD [ValueTypeId] int NULL')
	EXEC ('UPDATE [ProductVariantAttributeValue] SET [ValueTypeId] = 0 WHERE [ValueTypeId] IS NULL')
	EXEC ('ALTER TABLE [ProductVariantAttributeValue] ALTER COLUMN [ValueTypeId] int NOT NULL')
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[ProductVariantAttributeValue]') and NAME='LinkedProductId')
BEGIN
	EXEC ('ALTER TABLE [ProductVariantAttributeValue] ADD [LinkedProductId] int NULL')
	EXEC ('UPDATE [ProductVariantAttributeValue] SET [LinkedProductId] = 0 WHERE [LinkedProductId] IS NULL')
	EXEC ('ALTER TABLE [ProductVariantAttributeValue] ALTER COLUMN [LinkedProductId] int NOT NULL')
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[ProductVariantAttributeValue]') and NAME='Quantity')
BEGIN
	EXEC ('ALTER TABLE [ProductVariantAttributeValue] ADD [Quantity] int NULL')
	EXEC ('UPDATE [ProductVariantAttributeValue] SET [Quantity] = 1 WHERE [Quantity] IS NULL')
	EXEC ('ALTER TABLE [ProductVariantAttributeValue] ALTER COLUMN [Quantity] int NOT NULL')
END
GO

IF NOT EXISTS (SELECT 1 FROM [Setting] WHERE [name] = N'shoppingcartsettings.showlinkedattributevaluequantity')
BEGIN
	INSERT [Setting] ([Name], [Value], [StoreId]) VALUES (N'shoppingcartsettings.showlinkedattributevaluequantity', N'True', 0)
END
GO

IF NOT EXISTS (SELECT 1 FROM [Setting] WHERE [name] = N'catalogsettings.showlinkedattributevaluequantity')
BEGIN
	INSERT [Setting] ([Name], [Value], [StoreId]) VALUES (N'catalogsettings.showlinkedattributevaluequantity', N'True', 0)
END
GO

IF NOT EXISTS (SELECT 1 FROM [Setting] WHERE [name] = N'catalogsettings.showlinkedattributevalueimage')
BEGIN
	INSERT [Setting] ([Name], [Value], [StoreId]) VALUES (N'catalogsettings.showlinkedattributevalueimage', N'True', 0)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[OrderItem]') and NAME='ProductCost')
BEGIN
	EXEC ('ALTER TABLE [OrderItem] ADD [ProductCost] decimal(18,4) NULL')
	
	EXEC ('
		UPDATE [OrderItem] SET [OrderItem].[ProductCost] = p.[ProductCost] FROM [OrderItem] oi
		INNER JOIN [Product] p ON oi.[ProductId] = p.[Id]
	')
	
	EXEC ('UPDATE [OrderItem] SET [ProductCost] = 0 WHERE [ProductCost] IS NULL')
	EXEC ('ALTER TABLE [OrderItem] ALTER COLUMN [ProductCost] decimal(18,4) NOT NULL')
END
GO
