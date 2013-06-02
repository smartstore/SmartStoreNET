--upgrade scripts for smartstore.net multistore feature

IF NOT EXISTS (SELECT 1 FROM sysobjects WHERE id = OBJECT_ID(N'[Store]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[Store](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] nvarchar(400) NOT NULL,
	[DisplayOrder] int NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

DECLARE @DEFAULT_STORE_NAME nvarchar(400)
SELECT @DEFAULT_STORE_NAME = [Value] FROM [Setting] WHERE [name] = N'storeinformationsettings.storename' 

--create the first store
INSERT INTO [Store] ([Name], [DisplayOrder])
VALUES (@DEFAULT_STORE_NAME, 1)

END
GO

--new permission
IF NOT EXISTS (
		SELECT 1
		FROM [dbo].[PermissionRecord]
		WHERE [SystemName] = N'ManageStores')
BEGIN
	INSERT [dbo].[PermissionRecord] ([Name], [SystemName], [Category])
	VALUES (N'Admin area. Manage Stores', N'ManageStores', N'Configuration')

	DECLARE @PermissionRecordId INT 
	SET @PermissionRecordId = @@IDENTITY


	--add it to admin role be default
	DECLARE @AdminCustomerRoleId int
	SELECT @AdminCustomerRoleId = Id
	FROM [CustomerRole]
	WHERE IsSystemRole=1 and [SystemName] = N'Administrators'

	INSERT [dbo].[PermissionRecord_Role_Mapping] ([PermissionRecord_Id], [CustomerRole_Id])
	VALUES (@PermissionRecordId, @AdminCustomerRoleId)

	--codehint: sm-add
	--add it to super-admin role be default
	DECLARE @SuperAdminCustomerRoleId int
	SELECT @SuperAdminCustomerRoleId = Id
	FROM [CustomerRole]
	WHERE IsSystemRole=1 and [SystemName] = N'SuperAdmins'

	INSERT [dbo].[PermissionRecord_Role_Mapping] ([PermissionRecord_Id], [CustomerRole_Id])
	VALUES (@PermissionRecordId, @SuperAdminCustomerRoleId)
END
GO

IF NOT EXISTS (SELECT 1 FROM sysobjects WHERE id = OBJECT_ID(N'[StoreMapping]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[StoreMapping](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EntityId] [int] NOT NULL,
	[EntityName] nvarchar(400) NOT NULL,
	[StoreId] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT 1 from sysindexes WHERE [NAME]=N'IX_StoreMapping_EntityId_EntityName' and id=object_id(N'[StoreMapping]'))
BEGIN
	CREATE NONCLUSTERED INDEX [IX_StoreMapping_EntityId_EntityName] ON [StoreMapping] ([EntityId] ASC, [EntityName] ASC)
END
GO

--Store mapping for manufacturers
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE id=object_id('[Manufacturer]') and NAME='LimitedToStores')
BEGIN
	ALTER TABLE [Manufacturer]
	ADD [LimitedToStores] bit NULL
END
GO

UPDATE [Manufacturer]
SET [LimitedToStores] = 0
WHERE [LimitedToStores] IS NULL
GO

ALTER TABLE [Manufacturer] ALTER COLUMN [LimitedToStores] bit NOT NULL
GO





--new locale resources
declare @resources xml
--a resource will be deleted if its value is empty   
set @resources='
<Language>
  <LocaleResource Name="Admin.Configuration.Stores">
	<Value>Stores</Value>
	<T>Shops</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Stores.AddNew">
	<Value>Add a new store</Value>
	<T>Neuen Shop hinzufügen</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Stores.BackToList">
	<Value>back to store list</Value>
	<T>Zurück zur Shop-Liste</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Stores.EditStoreDetails">
	<Value>Edit store details</Value>
	<T>Shop-Details ändern</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Stores.Fields.Name">
	<Value>Name</Value>
	<T>Name</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Stores.Fields.Name.Hint">
	<Value>The name of the store.</Value>
	<T>Der Name des Shops.</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Stores.Fields.Name.Required">
	<Value>Please provide a name.</Value>
	<T>Bitte einen Namen angeben.</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Stores.Fields.DisplayOrder">
	<Value>Display order</Value>
	<T>Reihenfolge</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Stores.Fields.DisplayOrder.Hint">
	<Value>The display order for this store. 1 represents the top of the list.</Value>
	<T>Die Reihenfolge für diesen Shop. 1 bedeutet Anfang der Liste.</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Stores.Added">
	<Value>The new store has been added successfully.</Value>
	<T>Der neue Shop wurde erfolgreich hinzugefügt.</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Stores.Updated">
	<Value>The store has been updated successfully.</Value>
	<T>Der Shop wurde erfolgreich aktualisiert.</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Stores.Deleted">
	<Value>The store has been deleted successfully.</Value>
	<T>Der Shop wurde erfolgreich gelöscht.</T>
  </LocaleResource>
  
  <LocaleResource Name="Admin.Configuration.Stores.NoStoresDefined">
	<Value>No stores defined.</Value>
	<T>Keine Shops vorhanden.</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Catalog.Manufacturers.Stores">
	<Value>Stores</Value>
	<T>Shops</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Catalog.Manufacturers.Fields.LimitedToStores">
	<Value>Limited to stores</Value>
	<T>Auf Shop begrenzt</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Catalog.Manufacturers.Fields.LimitedToStores.Hint">
	<Value>Determines whether the manufacturer is available only at certain stores.</Value>
	<T>Legt fest, ob der Hersteller nur für bestimmte Shops verfügbar ist.</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Catalog.Manufacturers.Fields.AvailableStores">
	<Value>Stores</Value>
	<T>Shops</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Catalog.Manufacturers.Fields.AvailableStores.Hint">
	<Value>Select stores for which the manufacturer will be shown.</Value>
	<T>Bitte Shops auswählen für die der Hersteller verfügbar ist.</T>
  </LocaleResource>
  
  <LocaleResource Name="Admin.Configuration.Stores.CannotDeleteOnlyStore">
	<Value>You cannot delete the only configured store.</Value>
	<T>Der einzigste konfigurierte Shop kann nicht gelöscht werden.</T>
  </LocaleResource>

</Language>
'

CREATE TABLE #LocaleStringResourceTmp
	(
		[ResourceName] [nvarchar](200) NOT NULL,
		[ResourceValue] [nvarchar](max) NOT NULL
	)

INSERT INTO #LocaleStringResourceTmp (ResourceName, ResourceValue)
SELECT	nref.value('@Name', 'nvarchar(200)'), nref.value('Value[1]', 'nvarchar(MAX)')
FROM	@resources.nodes('//Language/LocaleResource') AS R(nref)

--do it for each existing language
DECLARE @ExistingLanguageID int
DECLARE cur_existinglanguage CURSOR FOR
SELECT [ID]
FROM [Language]
OPEN cur_existinglanguage
FETCH NEXT FROM cur_existinglanguage INTO @ExistingLanguageID
WHILE @@FETCH_STATUS = 0
BEGIN
	DECLARE @ResourceName nvarchar(200)
	DECLARE @ResourceValue nvarchar(MAX)
	DECLARE cur_localeresource CURSOR FOR
	SELECT ResourceName, ResourceValue
	FROM #LocaleStringResourceTmp
	OPEN cur_localeresource
	FETCH NEXT FROM cur_localeresource INTO @ResourceName, @ResourceValue
	WHILE @@FETCH_STATUS = 0
	BEGIN
		IF (EXISTS (SELECT 1 FROM [LocaleStringResource] WHERE LanguageID=@ExistingLanguageID AND ResourceName=@ResourceName))
		BEGIN
			UPDATE [LocaleStringResource]
			SET [ResourceValue]=@ResourceValue
			WHERE LanguageID=@ExistingLanguageID AND ResourceName=@ResourceName
		END
		ELSE 
		BEGIN
			INSERT INTO [LocaleStringResource]
			(
				[LanguageId],
				[ResourceName],
				[ResourceValue]
			)
			VALUES
			(
				@ExistingLanguageID,
				@ResourceName,
				@ResourceValue
			)
		END
		
		IF (@ResourceValue is null or @ResourceValue = '')
		BEGIN
			DELETE [LocaleStringResource]
			WHERE LanguageID=@ExistingLanguageID AND ResourceName=@ResourceName
		END
		
		FETCH NEXT FROM cur_localeresource INTO @ResourceName, @ResourceValue
	END
	CLOSE cur_localeresource
	DEALLOCATE cur_localeresource


	--fetch next language identifier
	FETCH NEXT FROM cur_existinglanguage INTO @ExistingLanguageID
END
CLOSE cur_existinglanguage
DEALLOCATE cur_existinglanguage

DROP TABLE #LocaleStringResourceTmp
GO