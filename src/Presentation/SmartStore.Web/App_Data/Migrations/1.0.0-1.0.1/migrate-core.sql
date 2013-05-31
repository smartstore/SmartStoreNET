
-- Delivery times
if not exists (select * from INFORMATION_SCHEMA.columns where table_name = 'CustomerRole' and column_name = 'TaxDisplayType')
BEGIN
	ALTER TABLE dbo.[CustomerRole] ADD 
		TaxDisplayType int NULL
END
GO

ALTER TABLE [Customer] ALTER COLUMN [TaxDisplayTypeId] int NULL

IF EXISTS (SELECT 1 FROM [LocaleStringResource] WHERE [ResourceName] = 'ActivityLog.ExportThemeVars')
BEGIN
	UPDATE [LocaleStringResource] SET [ResourceValue] = REPLACE ([ResourceValue], '{1}', '{0}') WHERE [ResourceName] = 'ActivityLog.ExportThemeVars'
END
GO

IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE id=object_id('[ProductVariantAttributeValue]') and NAME='Alias')
BEGIN
	ALTER TABLE [ProductVariantAttributeValue]
	ADD [Alias] nvarchar(100) NULL
END
GO

IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE id=object_id('[ProductAttribute]') and NAME='Alias')
BEGIN
	ALTER TABLE [ProductAttribute]
	ADD [Alias] nvarchar(100) NULL
END
GO

--New Column "LocaleStringResource.IsTouched"
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE id=object_id('[LocaleStringResource]') and NAME='IsTouched')
BEGIN
	ALTER TABLE LocaleStringResource ADD IsTouched bit NULL	
END
GO

IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE id=object_id('[Order]') and NAME='AllowStoringDirectDebit')
BEGIN
	ALTER TABLE [Order]
	ADD [AllowStoringDirectDebit] bit NOT NULL DEFAULT 0
	ALTER TABLE [Order]
	ADD [DirectDebitAccountHolder] nvarchar(100) NULL
	ALTER TABLE [Order]
	ADD [DirectDebitAccountNumber] nvarchar(100) NULL
	ALTER TABLE [Order]
	ADD [DirectDebitBankCode] nvarchar(100) NULL
	ALTER TABLE [Order]
	ADD [DirectDebitBankName] nvarchar(100) NULL
	ALTER TABLE [Order]
	ADD [DirectDebitBIC] nvarchar(100) NULL
	ALTER TABLE [Order]
	ADD [DirectDebitCountry] nvarchar(100) NULL
	ALTER TABLE [Order]
	ADD [DirectDebitIban] nvarchar(100) NULL
END
GO
