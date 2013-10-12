--upgrade scripts for smartstore.net (only specific parts)

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[DiscountRequirement]') and NAME='ExtraData')
BEGIN
	ALTER TABLE DiscountRequirement ADD ExtraData nvarchar(max) NULL
END
GO
