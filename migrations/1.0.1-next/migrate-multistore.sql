--upgrade scripts for smartstore.net multistore feature

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
	<Value>Store name</Value>
	<T>Shop-Name</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Stores.Fields.Name.Hint">
	<Value>Enter the name of your store e.g. Your Store.</Value>
	<T>Bitte den Namen des Shops eingeben, z.B. Mein Online-Shop.</T>
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
	<T>Bitte Shops auswählen, für die der Hersteller verfügbar ist.</T>
  </LocaleResource>
  
  <LocaleResource Name="Admin.Configuration.Stores.CannotDeleteOnlyStore">
	<Value>You cannot delete the only configured store.</Value>
	<T>Der einzigste konfigurierte Shop kann nicht gelöscht werden.</T>
  </LocaleResource>

  <LocaleResource Name="Admin.Catalog.Categories.Stores">
	<Value>Stores</Value>
	<T>Shops</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Catalog.Categories.Fields.LimitedToStores">
	<Value>Limited to stores</Value>
	<T>Auf Shop begrenzt</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Catalog.Categories.Fields.LimitedToStores.Hint">
	<Value>Determines whether the category is available only at certain stores.</Value>
	<T>Legt fest, ob die Warengruppe nur für bestimmte Shops verfügbar ist.</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Catalog.Categories.Fields.AvailableStores">
	<Value>Stores</Value>
	<T>Shops</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Catalog.Categories.Fields.AvailableStores.Hint">
	<Value>Select stores for which the category will be shown.</Value>
	<T>Bitte Shops auswählen, für die die Warengruppe verfügbar ist.</T>
  </LocaleResource>
  
  <LocaleResource Name="Admin.Catalog.Products.Stores">
	<Value>Stores</Value>
	<T>Shops</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Catalog.Products.Fields.LimitedToStores">
	<Value>Limited to stores</Value>
	<T>Auf Shop begrenzt</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Catalog.Products.Fields.LimitedToStores.Hint">
	<Value>Determines whether the product is available only at certain stores.</Value>
	<T>Legt fest, ob der Artikel nur für bestimmte Shops verfügbar ist.</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Catalog.Products.Fields.AvailableStores">
	<Value>Stores</Value>
	<T>Shops</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Catalog.Products.Fields.AvailableStores.Hint">
	<Value>Select stores for which the product will be shown.</Value>
	<T>Bitte Shops auswählen, für die der Artikel verfügbar ist.</T>
  </LocaleResource>
  
  <LocaleResource Name="Admin.Catalog.Languages.Info">
	<Value>Info</Value>
	<T>Info</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Catalog.Languages.Stores">
	<Value>Stores</Value>
	<T>Shops</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Catalog.Languages.Fields.LimitedToStores">
	<Value>Limited to stores</Value>
	<T>Auf Shop begrenzt</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Catalog.Languages.Fields.LimitedToStores.Hint">
	<Value>Determines whether the language is available only at certain stores.</Value>
	<T>Legt fest, ob die Sprache nur für bestimmte Shops verfügbar ist.</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Catalog.Languages.Fields.AvailableStores">
	<Value>Stores</Value>
	<T>Shops</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Catalog.Languages.Fields.AvailableStores.Hint">
	<Value>Select stores for which the language will be shown.</Value>
	<T>Bitte Shops auswählen, für die die Sprache verfügbar ist.</T>
  </LocaleResource>
  
  <LocaleResource Name="Admin.Catalog.Currencies.Info">
	<Value>Info</Value>
	<T>Info</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Catalog.Currencies.Stores">
	<Value>Stores</Value>
	<T>Shops</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Catalog.Currencies.Fields.LimitedToStores">
	<Value>Limited to stores</Value>
	<T>Auf Shop begrenzt</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Catalog.Currencies.Fields.LimitedToStores.Hint">
	<Value>Determines whether the currency is available only at certain stores.</Value>
	<T>Legt fest, ob die Währung nur für bestimmte Shops verfügbar ist.</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Catalog.Currencies.Fields.AvailableStores">
	<Value>Stores</Value>
	<T>Shops</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Catalog.Currencies.Fields.AvailableStores.Hint">
	<Value>Select stores for which the currency will be shown.</Value>
	<T>Bitte Shops auswählen, für die die Währung verfügbar ist.</T>
  </LocaleResource>
  
  <LocaleResource Name="Admin.CurrentCarts.Store">
	<Value>Store</Value>
	<T>Shop</T>
  </LocaleResource>
  
  <LocaleResource Name="Admin.Orders.List.Store">
	<Value>Store</Value>
	<T>Shop</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Orders.List.Store.Hint">
	<Value>Search by a specific store.</Value>
	<T>Nach bestimmten Shop suchen.</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Orders.Fields.Store">
	<Value>Store</Value>
	<T>Shop</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Orders.Fields.Store.Hint">
	<Value>A store name in which this order was placed.</Value>
	<T>Name des Shops für diese Bestellung.</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Customers.Customers.Orders.Store">
	<Value>Store</Value>
	<T>Shop</T>
  </LocaleResource>
  
  <LocaleResource Name="Admin.Configuration.Stores.Fields.Hosts">
	<Value>HOST values</Value>
	<T>HOST Werte</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Stores.Fields.Hosts.Hint">
	<Value>The comma separated list of possible HTTP_POST values (for example, "yourstore.com,www.yourstore.com"). This property is required only when you have a multi-store solution to determine the current store.</Value>
	<T>Kommagetrennte Liste mit möglichen HTTP_POTS Werten (z.B. "yourstore.com,www.yourstore.com"). Diese Einstellung wird nur in einer Multi-Shop Umgebung benötigt, um den aktuellen Shop zu ermitteln.</T>
  </LocaleResource>
  <LocaleResource Name="Admin.System.SystemInfo.HTTPHOST">
	<Value>HTTP_HOST</Value>
	<T>HTTP_HOST</T>
  </LocaleResource>
  <LocaleResource Name="Admin.System.SystemInfo.HTTPHOST.Hint">
	<Value>HTTP_HOST is used when you have run a multi-store solution to determine the current store.</Value>
	<T>HTTP_HOST wird in einer Multi-Shop Umgebung benötigt, um den aktuellen Shop zu ermitteln.</T>
  </LocaleResource>

 <LocaleResource Name="Admin.Configuration.Settings.GeneralCommon.StoreName">
	<Value></Value>
	<T></T>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.GeneralCommon.StoreName.Hint">
	<Value></Value>
	<T></T>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.GeneralCommon.StoreUrl">
	<Value></Value>
	<T></T>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.GeneralCommon.StoreUrl.Hint">
	<Value></Value>
	<T></T>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Stores.Fields.Url">
	<Value>Store URL</Value>
	<T>Shop URL</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Stores.Fields.Url.Hint">
	<Value>The URL of your store e.g. http://www.yourstore.com/</Value>
	<T>Die URL zu Ihrem Shop, z.B. http://www.yourstore.com/</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Stores.Fields.Url.Required">
	<Value>Please provide a store URL.</Value>
	<T>Bitte eine Shop-URL angeben.</T>
  </LocaleResource>

  <LocaleResource Name="Admin.Catalog.Products.List.SearchStore">
	<Value>Store</Value>
	<T>Shop</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Catalog.Products.List.SearchStore.Hint">
	<Value>Search by a specific store.</Value>
	<T>Nach bestimmten Shop suchen.</T>
  </LocaleResource>
  
  
  <LocaleResource Name="Admin.ContentManagement.MessageTemplates.Info">
	<Value>Info</Value>
	<T>Info</T>
  </LocaleResource>
  <LocaleResource Name="Admin.ContentManagement.MessageTemplates.Stores">
	<Value>Stores</Value>
	<T>Shops</T>
  </LocaleResource>
  <LocaleResource Name="Admin.ContentManagement.MessageTemplates.Fields.LimitedToStores">
	<Value>Limited to stores</Value>
	<T>Auf Shop begrenzt</T>
  </LocaleResource>
  <LocaleResource Name="Admin.ContentManagement.MessageTemplates.Fields.LimitedToStores.Hint">
	<Value>Determines whether the message template is available only at certain stores.</Value>
	<T>Legt fest, ob die Vorlage nur für bestimmte Shops verfügbar ist.</T>
  </LocaleResource>
  <LocaleResource Name="Admin.ContentManagement.MessageTemplates.Fields.AvailableStores">
	<Value>Stores</Value>
	<T>Shops</T>
  </LocaleResource>
  <LocaleResource Name="Admin.ContentManagement.MessageTemplates.Fields.AvailableStores.Hint">
	<Value>Select stores for which the message template will be active.</Value>
	<T>Bitte Shops auswählen, für die die Vorlage verfügbar ist.</T>
  </LocaleResource>
  <LocaleResource Name="Admin.ContentManagement.MessageTemplates.Deleted">
	<Value>The message template has been deleted successfully.</Value>
	<T>Die Nachrichtenvorlage wurde erfolgreich gelöscht.</T>
  </LocaleResource>
  <LocaleResource Name="Admin.ContentManagement.MessageTemplates.Copy">
	<Value>Copy template</Value>
	<T>Vorlage kopieren</T>
  </LocaleResource>
  <LocaleResource Name="Admin.ContentManagement.MessageTemplates.List.SearchStore">
	<Value>Store</Value>
	<T>Shop</T>
  </LocaleResource>
  <LocaleResource Name="Admin.ContentManagement.MessageTemplates.List.SearchStore.Hint">
	<Value>Search by a specific store.</Value>
	<T>Nach bestimmten Shop suchen.</T>
  </LocaleResource>
  
  <LocaleResource Name="Admin.ContentManagement.Topics.Stores">
	<Value>Stores</Value>
	<T>Shop</T>
  </LocaleResource>
  <LocaleResource Name="Admin.ContentManagement.Topics.Fields.LimitedToStores">
	<Value>Limited to stores</Value>
	<T>Auf Shop begrenzt</T>
  </LocaleResource>
  <LocaleResource Name="Admin.ContentManagement.Topics.Fields.LimitedToStores.Hint">
	<Value>Determines whether the topic is available only at certain stores.</Value>
	<T>Legt fest, ob die Seite nur für bestimmte Shops verfügbar ist.</T>
  </LocaleResource>
  <LocaleResource Name="Admin.ContentManagement.Topics.Fields.AvailableStores">
	<Value>Stores</Value>
	<T>Shops</T>
  </LocaleResource>
  <LocaleResource Name="Admin.ContentManagement.Topics.Fields.AvailableStores.Hint">
	<Value>Select stores for which the topic will be shown.</Value>
	<T>Bitte Shops auswählen, für die die Seite angezeigt werden soll.</T>
  </LocaleResource>
  <LocaleResource Name="Admin.ContentManagement.Topics.List.SearchStore">
	<Value>Store</Value>
	<T>Shop</T>
  </LocaleResource>
  <LocaleResource Name="Admin.ContentManagement.Topics.List.SearchStore.Hint">
	<Value>Search by a specific store.</Value>
	<T>Nach bestimmten Shop suchen.</T>
  </LocaleResource>
  <LocaleResource Name="Admin.ContentManagement.News.NewsItems.Info">
	<Value>Info</Value>
	<T>Info</T>
  </LocaleResource>
  <LocaleResource Name="Admin.ContentManagement.News.NewsItems.Stores">
	<Value>Stores</Value>
	<T>Shops</T>
  </LocaleResource>
  <LocaleResource Name="Admin.ContentManagement.News.NewsItems.Fields.LimitedToStores">
	<Value>Limited to stores</Value>
	<T>Auf Shop begrenzt</T>
  </LocaleResource>
  <LocaleResource Name="Admin.ContentManagement.News.NewsItems.Fields.LimitedToStores.Hint">
	<Value>Determines whether the news is available only at certain stores.</Value>
	<T>Legt fest, ob die News nur für bestimmte Shops verfügbar ist.</T>
  </LocaleResource>
  <LocaleResource Name="Admin.ContentManagement.News.NewsItems.Fields.AvailableStores">
	<Value>Stores</Value>
	<T>Shops</T>
  </LocaleResource>
  <LocaleResource Name="Admin.ContentManagement.News.NewsItems.Fields.AvailableStores.Hint">
	<Value>Select stores for which the news will be shown.</Value>
	<T>Bitte Shops auswählen, für die die News angezeigt werden soll.</T>
  </LocaleResource>
  <LocaleResource Name="Admin.ContentManagement.News.NewsItems.List.SearchStore">
	<Value>Store</Value>
	<T>Shop</T>
  </LocaleResource>
  <LocaleResource Name="Admin.ContentManagement.News.NewsItems.List.SearchStore.Hint">
	<Value>Search by a specific store.</Value>
	<T>Nach bestimmten Shop suchen.</T>
  </LocaleResource>

  <LocaleResource Name="Admin.Configuration.Stores.Fields.SslEnabled">
	<Value>SSL enabled</Value>
	<T>SSL aktivieren</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Stores.Fields.SslEnabled.Hint">
	<Value>Check if your store will be SSL secured.</Value>
	<T>Aktiviert SSL, falls der Shop SSL gesichert werden soll.</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Stores.Fields.SecureUrl">
	<Value>Secure URL</Value>
	<T>Gesicherte URL</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Stores.Fields.SecureUrl.Hint">
	<Value>The secure URL of your store e.g. https://www.yourstore.com/ or http://sharedssl.yourstore.com/. Leave it empty if you want secure URL to be detected automatically.</Value>
	<T>Die gesicherte URL des Shops, z.B. https://www.meinshop.de/ or http://sharedssl.meinshop.de/. Die gesicherte URL wird automatisch erkannt, wenn dieses Feld leer ist.</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.GeneralCommon.UseSSL">
	<Value></Value>
	<T></T>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.GeneralCommon.UseSSL.Hint">
	<Value></Value>
	<T></T>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.GeneralCommon.SharedSSLUrl">
	<Value></Value>
	<T></T>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.GeneralCommon.SharedSSLUrl.Hint">
	<Value></Value>
	<T></T>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.GeneralCommon.NonSharedSSLUrl">
	<Value></Value>
	<T></T>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.GeneralCommon.NonSharedSSLUrl.Hint">
	<Value></Value>
	<T></T>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.GeneralCommon.SSLSettings">
	<Value></Value>
	<T></T>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.GeneralCommon.SSLSettings.Hint">
	<Value></Value>
	<T></T>
  </LocaleResource>
  
  <LocaleResource Name="Plugins.Feed.Froogle.ClickHere">
	<Value></Value>
	<T></T>
  </LocaleResource>
  <LocaleResource Name="Plugins.Feed.Froogle.SuccessResult">
	<Value>Feed has been successfully generated.</Value>
	<T>Feed wurde erfolgreich erstellt.</T>
  </LocaleResource>
  <LocaleResource Name="Plugins.Feed.Billiger.ClickHere">
	<Value></Value>
	<T></T>
  </LocaleResource>
  <LocaleResource Name="Plugins.Feed.Billiger.SuccessResult">
	<Value>Feed has been successfully generated.</Value>
	<T>Feed wurde erfolgreich erstellt.</T>
  </LocaleResource>
  <LocaleResource Name="Plugins.Feed.ElmarShopinfo.ClickHere">
	<Value></Value>
	<T></T>
  </LocaleResource>
  <LocaleResource Name="Plugins.Feed.ElmarShopinfo.SuccessResult">
	<Value>Feed has been successfully generated.</Value>
	<T>Feed wurde erfolgreich erstellt.</T>
  </LocaleResource>
  <LocaleResource Name="Plugins.Feed.ElmarShopinfo.StaticFileXmlUrl">
	<Value></Value>
	<T></T>
  </LocaleResource>
  <LocaleResource Name="Plugins.Feed.ElmarShopinfo.StaticFileXmlUrl.Hint">
	<Value></Value>
	<T></T>
  </LocaleResource>
  <LocaleResource Name="Plugins.Feed.Guenstiger.ClickHere">
	<Value></Value>
	<T></T>
  </LocaleResource>
  <LocaleResource Name="Plugins.Feed.Guenstiger.SuccessResult">
	<Value>Feed has been successfully generated.</Value>
	<T>Feed wurde erfolgreich erstellt.</T>
  </LocaleResource>
  <LocaleResource Name="Plugins.Feed.Shopwahl.ClickHere">
	<Value></Value>
	<T></T>
  </LocaleResource>
  <LocaleResource Name="Plugins.Feed.Shopwahl.SuccessResult">
	<Value>Feed has been successfully generated.</Value>
	<T>Feed wurde erfolgreich erstellt.</T>
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



IF NOT EXISTS (SELECT 1 FROM sysobjects WHERE id = OBJECT_ID(N'[Store]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
	CREATE TABLE [dbo].[Store](
		[Id] [int] IDENTITY(1,1) NOT NULL,
		[Name] nvarchar(400) NOT NULL,
		[Url] nvarchar(400) NOT NULL,
		[SslEnabled] bit NOT NULL,
		[SecureUrl] nvarchar(400) NULL,
		[Hosts] nvarchar(1000) NULL,
		[DisplayOrder] int NOT NULL,
	PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY]

	DECLARE @DEFAULT_STORE_NAME nvarchar(400)
	SELECT @DEFAULT_STORE_NAME = [Value] FROM [Setting] WHERE [name] = N'storeinformationsettings.storename'
	
	if (@DEFAULT_STORE_NAME is null)
		SET @DEFAULT_STORE_NAME = N'Your store name' 

	DECLARE @DEFAULT_STORE_URL nvarchar(400)
	SELECT @DEFAULT_STORE_URL= [Value] FROM [Setting] WHERE [name] = N'storeinformationsettings.storeurl'
	
	if (@DEFAULT_STORE_URL is null)
		SET @DEFAULT_STORE_URL = N'http://www.yourstore.com/'

	--create the first store
	INSERT INTO [Store] ([Name], [Url], [SslEnabled], [Hosts], [DisplayOrder])
	VALUES (@DEFAULT_STORE_NAME, @DEFAULT_STORE_URL, 0, N'yourstore.com,www.yourstore.com', 1)

	DELETE FROM [Setting] WHERE [name] = N'storeinformationsettings.storename' 
	DELETE FROM [Setting] WHERE [name] = N'storeinformationsettings.storeurl' 
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
	ALTER TABLE [Manufacturer] ADD [LimitedToStores] bit NULL
END
GO
UPDATE [Manufacturer] SET [LimitedToStores] = 0 WHERE [LimitedToStores] IS NULL
GO
ALTER TABLE [Manufacturer] ALTER COLUMN [LimitedToStores] bit NOT NULL
GO

--Store mapping for categories
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE id=object_id('[Category]') and NAME='LimitedToStores')
BEGIN
	ALTER TABLE [Category] ADD [LimitedToStores] bit NULL
END
GO
UPDATE [Category] SET [LimitedToStores] = 0 WHERE [LimitedToStores] IS NULL
GO
ALTER TABLE [Category] ALTER COLUMN [LimitedToStores] bit NOT NULL
GO

--Store mapping for products
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE id=object_id('[Product]') and NAME='LimitedToStores')
BEGIN
	ALTER TABLE [Product] ADD [LimitedToStores] bit NULL
END
GO
UPDATE [Product] SET [LimitedToStores] = 0 WHERE [LimitedToStores] IS NULL
GO
ALTER TABLE [Product] ALTER COLUMN [LimitedToStores] bit NOT NULL
GO

IF EXISTS (
		SELECT *
		FROM sysobjects
		WHERE id = OBJECT_ID(N'[ProductLoadAllPaged]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
DROP PROCEDURE [ProductLoadAllPaged]
GO
CREATE PROCEDURE [dbo].[ProductLoadAllPaged]
(
	@CategoryIds		nvarchar(MAX) = null,	--a list of category IDs (comma-separated list). e.g. 1,2,3
	@ManufacturerId		int = 0,
	@StoreId			int = 0,
	@ProductTagId		int = 0,
	@FeaturedProducts	bit = null,	--0 featured only , 1 not featured only, null - load all products
	@PriceMin			decimal(18, 4) = null,
	@PriceMax			decimal(18, 4) = null,
	@Keywords			nvarchar(4000) = null,
	@SearchDescriptions bit = 0, --a value indicating whether to search by a specified "keyword" in product descriptions
	@SearchProductTags  bit = 0, --a value indicating whether to search by a specified "keyword" in product tags
	@UseFullTextSearch  bit = 0,
	@FullTextMode		int = 0, --0 using CONTAINS with <prefix_term>, 5 - using CONTAINS and OR with <prefix_term>, 10 - using CONTAINS and AND with <prefix_term>
	@FilteredSpecs		nvarchar(MAX) = null,	--filter by attributes (comma-separated list). e.g. 14,15,16
	@LanguageId			int = 0,
	@OrderBy			int = 0, --0 position, 5 - Name: A to Z, 6 - Name: Z to A, 10 - Price: Low to High, 11 - Price: High to Low, 15 - creation date
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

				--remove wrong chars (' ")
				SET @Keywords = REPLACE(@Keywords, '''', '')
				SET @Keywords = REPLACE(@Keywords, '"', '')
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


		--product variant name
		SET @sql = @sql + '
		UNION
		SELECT pv.ProductId
		FROM ProductVariant pv with (NOLOCK)
		WHERE '
		IF @UseFullTextSearch = 1
			SET @sql = @sql + 'CONTAINS(pv.[Name], @Keywords) '
		ELSE
			SET @sql = @sql + 'PATINDEX(@Keywords, pv.[Name]) > 0 '


		--SKU
		SET @sql = @sql + '
		UNION
		SELECT pv.ProductId
		FROM ProductVariant pv with (NOLOCK)
		WHERE '
		IF @UseFullTextSearch = 1
			SET @sql = @sql + 'CONTAINS(pv.[Sku], @Keywords) '
		ELSE
			SET @sql = @sql + 'PATINDEX(@Keywords, pv.[Sku]) > 0 '


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


			--product variant description
			SET @sql = @sql + '
			UNION
			SELECT pv.ProductId
			FROM ProductVariant pv with (NOLOCK)
			WHERE '
			IF @UseFullTextSearch = 1
				SET @sql = @sql + 'CONTAINS(pv.[Description], @Keywords) '
			ELSE
				SET @sql = @sql + 'PATINDEX(@Keywords, pv.[Description]) > 0 '


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
	SELECT CAST(data as int) FROM [nop_splitstring_to_table](@CategoryIds, ',')	
	DECLARE @CategoryIdsCount int	
	SET @CategoryIdsCount = (SELECT COUNT(1) FROM #FilteredCategoryIds)

	--filter by attributes
	SET @FilteredSpecs = isnull(@FilteredSpecs, '')	
	CREATE TABLE #FilteredSpecs
	(
		SpecificationAttributeOptionId int not null
	)
	INSERT INTO #FilteredSpecs (SpecificationAttributeOptionId)
	SELECT CAST(data as int) FROM [nop_splitstring_to_table](@FilteredSpecs, ',')
	DECLARE @SpecAttributesCount int	
	SET @SpecAttributesCount = (SELECT COUNT(1) FROM #FilteredSpecs)

	--filter by customer role IDs (access control list)
	SET @AllowedCustomerRoleIds = isnull(@AllowedCustomerRoleIds, '')	
	CREATE TABLE #FilteredCustomerRoleIds
	(
		CustomerRoleId int not null
	)
	INSERT INTO #FilteredCustomerRoleIds (CustomerRoleId)
	SELECT CAST(data as int) FROM [nop_splitstring_to_table](@AllowedCustomerRoleIds, ',')
	
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
	
	IF @ShowHidden = 0
	OR @PriceMin > 0
	OR @PriceMax > 0
	OR @OrderBy = 10 /* Price: Low to High */
	OR @OrderBy = 11 /* Price: High to Low */
	BEGIN
		SET @sql = @sql + '
		LEFT JOIN ProductVariant pv with (NOLOCK)
			ON p.Id = pv.ProductId'
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
		AND pv.Published = 1
		AND pv.Deleted = 0
		AND (getutcdate() BETWEEN ISNULL(pv.AvailableStartDateTimeUtc, ''1/1/1900'') and ISNULL(pv.AvailableEndDateTimeUtc, ''1/1/2999''))'
	END
	
	--min price
	IF @PriceMin > 0
	BEGIN
		SET @sql = @sql + '
		AND (
				(
					--special price (specified price and valid date range)
					(pv.SpecialPrice IS NOT NULL AND (getutcdate() BETWEEN isnull(pv.SpecialPriceStartDateTimeUtc, ''1/1/1900'') AND isnull(pv.SpecialPriceEndDateTimeUtc, ''1/1/2999'')))
					AND
					(pv.SpecialPrice >= ' + CAST(@PriceMin AS nvarchar(max)) + ')
				)
				OR 
				(
					--regular price (price isnt specified or date range isnt valid)
					(pv.SpecialPrice IS NULL OR (getutcdate() NOT BETWEEN isnull(pv.SpecialPriceStartDateTimeUtc, ''1/1/1900'') AND isnull(pv.SpecialPriceEndDateTimeUtc, ''1/1/2999'')))
					AND
					(pv.Price >= ' + CAST(@PriceMin AS nvarchar(max)) + ')
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
					(pv.SpecialPrice IS NOT NULL AND (getutcdate() BETWEEN isnull(pv.SpecialPriceStartDateTimeUtc, ''1/1/1900'') AND isnull(pv.SpecialPriceEndDateTimeUtc, ''1/1/2999'')))
					AND
					(pv.SpecialPrice <= ' + CAST(@PriceMax AS nvarchar(max)) + ')
				)
				OR 
				(
					--regular price (price isnt specified or date range isnt valid)
					(pv.SpecialPrice IS NULL OR (getutcdate() NOT BETWEEN isnull(pv.SpecialPriceStartDateTimeUtc, ''1/1/1900'') AND isnull(pv.SpecialPriceEndDateTimeUtc, ''1/1/2999'')))
					AND
					(pv.Price <= ' + CAST(@PriceMax AS nvarchar(max)) + ')
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
					FROM [AclRecord] acl
					WHERE [acl].EntityId = p.Id AND [acl].EntityName = ''Product''
				)
			))'
	END
	
	--filter by store
	IF @StoreId > 0
	BEGIN
		SET @sql = @sql + '
		AND (p.LimitedToStores = 0 OR EXISTS (
			SELECT 1 FROM [StoreMapping] sm
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
					FROM Product_SpecificationAttribute_Mapping psam
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
		SET @sql_orderby = ' pv.[Price] ASC'
	ELSE IF @OrderBy = 11 /* Price: High to Low */
		SET @sql_orderby = ' pv.[Price] DESC'
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
		FROM [Product_SpecificationAttribute_Mapping] [psam]
		WHERE [psam].[AllowFiltering] = 1
		AND [psam].[ProductId] IN (SELECT [pi].ProductId FROM #PageIndex [pi])

		--build comma separated list of filterable identifiers
		SELECT @FilterableSpecificationAttributeOptionIds = COALESCE(@FilterableSpecificationAttributeOptionIds + ',' , '') + CAST(SpecificationAttributeOptionId as nvarchar(1000))
		FROM #FilterableSpecs

		DROP TABLE #FilterableSpecs
 	END

	--return products
	SELECT TOP (@RowsToReturn)
		p.*
	FROM
		#PageIndex [pi]
		INNER JOIN Product p on p.Id = [pi].[ProductId]
	WHERE
		[pi].IndexId > @PageLowerBound AND 
		[pi].IndexId < @PageUpperBound
	ORDER BY
		[pi].IndexId
	
	DROP TABLE #PageIndex
END
GO


--Store mapping for languages
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE id=object_id('[Language]') and NAME='LimitedToStores')
BEGIN
	ALTER TABLE [Language] ADD [LimitedToStores] bit NULL
END
GO

UPDATE [Language] SET [LimitedToStores] = 0 WHERE [LimitedToStores] IS NULL
GO

ALTER TABLE [Language] ALTER COLUMN [LimitedToStores] bit NOT NULL
GO

--Store mapping for currencies
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE id=object_id('[Currency]') and NAME='LimitedToStores')
BEGIN
	ALTER TABLE [Currency] ADD [LimitedToStores] bit NULL
END
GO

UPDATE [Currency] SET [LimitedToStores] = 0 WHERE [LimitedToStores] IS NULL
GO

ALTER TABLE [Currency] ALTER COLUMN [LimitedToStores] bit NOT NULL
GO

--drop some constraints
IF EXISTS (SELECT 1
           FROM   sysobjects
           WHERE  name = 'Customer_Currency'
           AND parent_obj = Object_id('Customer')
           AND Objectproperty(id,N'IsForeignKey') = 1)
BEGIN
	ALTER TABLE dbo.[Customer]
	DROP CONSTRAINT Customer_Currency
END
GO

UPDATE [Customer] SET [CurrencyId] = 0 WHERE [CurrencyId] IS NULL
GO

ALTER TABLE [Customer] ALTER COLUMN [CurrencyId] int NOT NULL
GO

IF EXISTS (SELECT 1
           FROM   sysobjects
           WHERE  name = 'Customer_Language'
           AND parent_obj = Object_id('Customer')
           AND Objectproperty(id,N'IsForeignKey') = 1)
BEGIN
	ALTER TABLE dbo.[Customer]
	DROP CONSTRAINT Customer_Language
END
GO

UPDATE [Customer] SET [LanguageId] = 0 WHERE [LanguageId] IS NULL
GO

ALTER TABLE [Customer] ALTER COLUMN [LanguageId] int NOT NULL
GO

IF EXISTS (SELECT 1
           FROM   sysobjects
           WHERE  name = 'Affiliate_AffiliatedCustomers'
           AND parent_obj = Object_id('Customer')
           AND Objectproperty(id,N'IsForeignKey') = 1)
BEGIN
	ALTER TABLE dbo.[Customer]
	DROP CONSTRAINT Affiliate_AffiliatedCustomers
END
GO

UPDATE [Customer] SET [AffiliateId] = 0 WHERE [AffiliateId] IS NULL
GO

ALTER TABLE [Customer] ALTER COLUMN [AffiliateId] int NOT NULL
GO

IF EXISTS (SELECT 1
           FROM   sysobjects
           WHERE  name = 'Affiliate_AffiliatedOrders'
           AND parent_obj = Object_id('Order')
           AND Objectproperty(id,N'IsForeignKey') = 1)
BEGIN
	ALTER TABLE dbo.[Order]
	DROP CONSTRAINT Affiliate_AffiliatedOrders
END
GO

UPDATE [Order] SET [AffiliateId] = 0 WHERE [AffiliateId] IS NULL
GO

ALTER TABLE [Order] ALTER COLUMN [AffiliateId] int NOT NULL
GO

--Store mapping to shopping cart items

IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE id=object_id('[ShoppingCartItem]') and NAME='StoreId')
BEGIN
	ALTER TABLE [ShoppingCartItem]
	ADD [StoreId] bit NULL
END
GO

DECLARE @DEFAULT_STORE_ID int
SELECT @DEFAULT_STORE_ID = [Id] FROM [Store] ORDER BY [DisplayOrder]
UPDATE [ShoppingCartItem] SET [StoreId] = @DEFAULT_STORE_ID WHERE [StoreId] IS NULL
GO

ALTER TABLE [ShoppingCartItem] ALTER COLUMN [StoreId] int NOT NULL
GO

IF NOT EXISTS (SELECT 1
           FROM   sysobjects
           WHERE  name = 'ShoppingCartItem_Store'
           AND parent_obj = Object_id('ShoppingCartItem')
           AND Objectproperty(id,N'IsForeignKey') = 1)
BEGIN
	ALTER TABLE [dbo].[ShoppingCartItem] WITH CHECK ADD CONSTRAINT [ShoppingCartItem_Store] FOREIGN KEY([StoreId])
	REFERENCES [dbo].[Store] ([Id])
	ON DELETE CASCADE
END
GO

--Store mapping to orders
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE id=object_id('[Order]') and NAME='StoreId')
BEGIN
	ALTER TABLE [Order]
	ADD [StoreId] bit NULL
END
GO

DECLARE @DEFAULT_STORE_ID int
SELECT @DEFAULT_STORE_ID = [Id] FROM [Store] ORDER BY [DisplayOrder]
UPDATE [Order] SET [StoreId] = @DEFAULT_STORE_ID WHERE [StoreId] IS NULL
GO

ALTER TABLE [Order] ALTER COLUMN [StoreId] int NOT NULL
GO

--Store mapping to return requests
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE id=object_id('[ReturnRequest]') and NAME='StoreId')
BEGIN
	ALTER TABLE [ReturnRequest]
	ADD [StoreId] bit NULL
END
GO

DECLARE @DEFAULT_STORE_ID int
SELECT @DEFAULT_STORE_ID = [Id] FROM [Store] ORDER BY [DisplayOrder]
UPDATE [ReturnRequest] SET [StoreId] = @DEFAULT_STORE_ID WHERE [StoreId] IS NULL
GO

ALTER TABLE [ReturnRequest] ALTER COLUMN [StoreId] int NOT NULL
GO

--Store mapping to message templates
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE id=object_id('[MessageTemplate]') and NAME='LimitedToStores')
BEGIN
	ALTER TABLE [MessageTemplate]
	ADD [LimitedToStores] bit NULL
END
GO

UPDATE [MessageTemplate] SET [LimitedToStores] = 0 WHERE [LimitedToStores] IS NULL
GO

ALTER TABLE [MessageTemplate] ALTER COLUMN [LimitedToStores] bit NOT NULL
GO

--Store mapping for topics
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE id=object_id('[Topic]') and NAME='LimitedToStores')
BEGIN
	ALTER TABLE [Topic]
	ADD [LimitedToStores] bit NULL
END
GO

UPDATE [Topic] SET [LimitedToStores] = 0 WHERE [LimitedToStores] IS NULL
GO

ALTER TABLE [Topic] ALTER COLUMN [LimitedToStores] bit NOT NULL
GO

--Store mapping to news
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE id=object_id('[News]') and NAME='LimitedToStores')
BEGIN
	ALTER TABLE [News]
	ADD [LimitedToStores] bit NULL
END
GO

UPDATE [News] SET [LimitedToStores] = 0 WHERE [LimitedToStores] IS NULL
GO

ALTER TABLE [News] ALTER COLUMN [LimitedToStores] bit NOT NULL
GO


--Store mapping to BackInStockSubscription
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE id=object_id('[BackInStockSubscription]') and NAME='StoreId')
BEGIN
	ALTER TABLE [BackInStockSubscription]
	ADD [StoreId] bit NULL
END
GO

DECLARE @DEFAULT_STORE_ID int
SELECT @DEFAULT_STORE_ID = [Id] FROM [Store] ORDER BY [DisplayOrder]
UPDATE [BackInStockSubscription] SET [StoreId] = @DEFAULT_STORE_ID WHERE [StoreId] IS NULL
GO

ALTER TABLE [BackInStockSubscription] ALTER COLUMN [StoreId] int NOT NULL
GO

IF NOT EXISTS (SELECT 1
           FROM   sysobjects
           WHERE  name = 'BackInStockSubscription_Store'
           AND parent_obj = Object_id('BackInStockSubscription')
           AND Objectproperty(id,N'IsForeignKey') = 1)
BEGIN
	ALTER TABLE [dbo].[BackInStockSubscription] WITH CHECK ADD CONSTRAINT [BackInStockSubscription_Store] FOREIGN KEY([StoreId])
	REFERENCES [dbo].[Store] ([Id])
	ON DELETE CASCADE
END
GO


--Store mapping to Forums_PrivateMessage
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE id=object_id('[Forums_PrivateMessage]') and NAME='StoreId')
BEGIN
	ALTER TABLE [Forums_PrivateMessage]
	ADD [StoreId] bit NULL
END
GO

DECLARE @DEFAULT_STORE_ID int
SELECT @DEFAULT_STORE_ID = [Id] FROM [Store] ORDER BY [DisplayOrder]
UPDATE [Forums_PrivateMessage] SET [StoreId] = @DEFAULT_STORE_ID WHERE [StoreId] IS NULL
GO

ALTER TABLE [Forums_PrivateMessage] ALTER COLUMN [StoreId] int NOT NULL
GO

IF NOT EXISTS (SELECT 1
           FROM   sysobjects
           WHERE  name = 'Forums_PrivateMessage_Store'
           AND parent_obj = Object_id('Forums_PrivateMessage')
           AND Objectproperty(id,N'IsForeignKey') = 1)
BEGIN
	ALTER TABLE [dbo].[Forums_PrivateMessage] WITH CHECK ADD CONSTRAINT [Forums_PrivateMessage_Store] FOREIGN KEY([StoreId])
	REFERENCES [dbo].[Store] ([Id])
	ON DELETE CASCADE
END
GO