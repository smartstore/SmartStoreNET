IF EXISTS (SELECT 1 FROM [Setting] WHERE [name] = N'seosettings.reservedurlrecordslugs')
BEGIN
	DECLARE @ReservedSlugs nvarchar(4000)
	SELECT @ReservedSlugs = [Value] FROM [Setting] WHERE [name] = N'seosettings.reservedurlrecordslugs'
	
	IF (CHARINDEX(N'api', @ReservedSlugs) = 0)
	BEGIN
		UPDATE [Setting] SET [Value] = @ReservedSlugs + ',api' WHERE [name] = N'seosettings.reservedurlrecordslugs'
	END	
END
GO

IF EXISTS (SELECT 1 FROM [Setting] WHERE [name] = N'seosettings.reservedurlrecordslugs')
BEGIN
	DECLARE @ReservedSlugs nvarchar(4000)
	SELECT @ReservedSlugs = [Value] FROM [Setting] WHERE [name] = N'seosettings.reservedurlrecordslugs'
	
	IF (CHARINDEX(N'odata', @ReservedSlugs) = 0)
	BEGIN
		UPDATE [Setting] SET [Value] = @ReservedSlugs + ',odata' WHERE [name] = N'seosettings.reservedurlrecordslugs'
	END	
END
GO

IF EXISTS (SELECT 1 FROM [Setting] WHERE [name] = N'themesettings.csscacheenabled')
BEGIN
	DELETE FROM [Setting] WHERE [name] = N'themesettings.csscacheenabled'
END
GO

IF EXISTS (SELECT 1 FROM [Setting] WHERE [name] = N'themesettings.cssminifyenabled')
BEGIN
	DELETE FROM [Setting] WHERE [Name] = N'themesettings.cssminifyenabled'
END
GO


