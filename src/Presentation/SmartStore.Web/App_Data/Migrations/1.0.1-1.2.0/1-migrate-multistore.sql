--upgrade scripts for smartstore.net multistore feature

--new locale resources
DECLARE @resources xml
--a resource will be deleted if its value is empty   
SET @resources='
<Language>
  <LocaleResource Name="Admin.Configuration.Stores.AddNew">
	<Value>Add a new store</Value>
	<Value lang="de">Neuen Shop hinzufügen</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Stores.BackToList">
	<Value>back to store list</Value>
	<Value lang="de">Zurück zur Shop-Liste</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Stores.EditStoreDetails">
	<Value>Edit store details</Value>
	<Value lang="de">Shop-Details ändern</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Stores.Fields.Name">
	<Value>Store name</Value>
	<Value lang="de">Shop-Name</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Stores.Fields.Name.Hint">
	<Value>Enter the name of your store e.g. Your Store.</Value>
	<Value lang="de">Bitte den Namen des Shops eingeben, z.B. Mein Online-Shop.</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Stores.Fields.Name.Required">
	<Value>Please provide a name.</Value>
	<Value lang="de">Bitte einen Namen angeben.</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Stores.Fields.StoreLogo">
	<Value>Store logo</Value>
	<Value lang="de">Shop Logo</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Stores.Fields.StoreLogo.Hint">
	<Value>Upload your store logo</Value>
	<Value lang="de">Ein Shop Logo hochladen</Value>
  </LocaleResource>  
  <LocaleResource Name="Admin.Configuration.Stores.Fields.DisplayOrder">
	<Value>Display order</Value>
	<Value lang="de">Reihenfolge</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Stores.Fields.DisplayOrder.Hint">
	<Value>The display order for this store. 1 represents the top of the list.</Value>
	<Value lang="de">Die Reihenfolge für diesen Shop. 1 bedeutet Anfang der Liste.</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Stores.Added">
	<Value>The new store has been added successfully.</Value>
	<Value lang="de">Der neue Shop wurde erfolgreich hinzugefügt.</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Stores.Updated">
	<Value>The store has been updated successfully.</Value>
	<Value lang="de">Der Shop wurde erfolgreich aktualisiert.</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Stores.Deleted">
	<Value>The store has been deleted successfully.</Value>
	<Value lang="de">Der Shop wurde erfolgreich gelöscht.</Value>
  </LocaleResource>
  
  <LocaleResource Name="Admin.Configuration.Stores.NoStoresDefined">
	<Value>No stores defined.</Value>
	<Value lang="de">Keine Shops vorhanden.</Value>
  </LocaleResource>
    
  <LocaleResource Name="Admin.Orders.Fields.Store">
	<Value>Store</Value>
	<Value lang="de">Shop</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Orders.Fields.Store.Hint">
	<Value>A store name in which this order was placed.</Value>
	<Value lang="de">Name des Shops für diese Bestellung.</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Customers.Customers.Orders.Store">
	<Value>Store</Value>
	<Value lang="de">Shop</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Customers.Customers.Orders.Store.Hint">
	<Value>Name of the store.</Value>
	<Value lang="de">Name des Shops.</Value>
  </LocaleResource>  
  
  <LocaleResource Name="Admin.Configuration.Stores.Fields.Hosts">
	<Value>HOST values</Value>
	<Value lang="de">HOST Werte</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Stores.Fields.Hosts.Hint">
	<Value>The comma separated list of possible HTTP_POST values (for example, "yourstore.com,www.yourstore.com"). This property is required only when you have a multi-store solution to determine the current store.</Value>
	<Value lang="de">Kommagetrennte Liste mit möglichen HTTP_POTS Werten (z.B. "mein-shop.com,www.mein-shop.de"). Diese Einstellung wird nur in einer Multi-Shop Umgebung benötigt, um den aktuellen Shop zu ermitteln.</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.System.SystemInfo.HTTPHOST">
	<Value>HTTP_HOST</Value>
	<Value lang="de">HTTP_HOST</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.System.SystemInfo.HTTPHOST.Hint">
	<Value>HTTP_HOST is used when you have run a multi-store solution to determine the current store.</Value>
	<Value lang="de">HTTP_HOST wird in einer Multi-Shop Umgebung benötigt, um den aktuellen Shop zu ermitteln.</Value>
  </LocaleResource>

  <LocaleResource Name="Admin.Configuration.Settings.GeneralCommon.StoreName">
	<Value></Value>
	<Value lang="de"></Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.GeneralCommon.StoreName.Hint">
	<Value></Value>
	<Value lang="de"></Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.GeneralCommon.StoreUrl">
	<Value></Value>
	<Value lang="de"></Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.GeneralCommon.StoreUrl.Hint">
	<Value></Value>
	<Value lang="de"></Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.GeneralCommon.StoreLogo">
	<Value></Value>
	<Value lang="de"></Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.GeneralCommon.StoreLogo.Hint">
	<Value></Value>
	<Value lang="de"></Value>
  </LocaleResource>  
  <LocaleResource Name="Admin.Configuration.Stores.Fields.Url">
	<Value>Store URL</Value>
	<Value lang="de">Shop URL</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Stores.Fields.Url.Hint">
	<Value>The URL of your store e.g. http://www.yourstore.com/</Value>
	<Value lang="de">Die URL zu Ihrem Shop, z.B. http://www.mein-shop.de/</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Stores.Fields.Url.Required">
	<Value>Please provide a store URL.</Value>
	<Value lang="de">Bitte eine Shop-URL angeben.</Value>
  </LocaleResource>
  
  <LocaleResource Name="Admin.ContentManagement.MessageTemplates.Deleted">
	<Value>The message template has been deleted successfully.</Value>
	<Value lang="de">Die Nachrichtenvorlage wurde erfolgreich gelöscht.</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.ContentManagement.MessageTemplates.Copy">
	<Value>Copy template</Value>
	<Value lang="de">Vorlage kopieren</Value>
  </LocaleResource>
  
  <LocaleResource Name="Admin.Configuration.Stores.Fields.SslEnabled">
	<Value>SSL enabled</Value>
	<Value lang="de">SSL aktivieren</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Stores.Fields.SslEnabled.Hint">
	<Value>Check if your store will be SSL secured.</Value>
	<Value lang="de">Aktiviert SSL, falls der Shop SSL gesichert werden soll.</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Stores.Fields.SecureUrl">
	<Value>Secure URL</Value>
	<Value lang="de">Gesicherte URL</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Stores.Fields.SecureUrl.Hint">
	<Value>The secure URL of your store e.g. https://www.yourstore.com/ or http://sharedssl.yourstore.com/. Leave it empty if you want secure URL to be detected automatically.</Value>
	<Value lang="de">Die gesicherte URL des Shops, z.B. https://www.mein-shop.de/ or http://sharedssl.mein-shop.de/. Die gesicherte URL wird automatisch erkannt, wenn dieses Feld leer ist.</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.GeneralCommon.UseSSL">
	<Value></Value>
	<Value lang="de"></Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.GeneralCommon.UseSSL.Hint">
	<Value></Value>
	<Value lang="de"></Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.GeneralCommon.SharedSSLUrl">
	<Value></Value>
	<Value lang="de"></Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.GeneralCommon.SharedSSLUrl.Hint">
	<Value></Value>
	<Value lang="de"></Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.GeneralCommon.NonSharedSSLUrl">
	<Value></Value>
	<Value lang="de"></Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.GeneralCommon.NonSharedSSLUrl.Hint">
	<Value></Value>
	<Value lang="de"></Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.GeneralCommon.SSLSettings">
	<Value></Value>
	<Value lang="de"></Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.GeneralCommon.SSLSettings.Hint">
	<Value></Value>
	<Value lang="de"></Value>
  </LocaleResource>
  
  <LocaleResource Name="Plugins.Feed.Froogle.ClickHere">
	<Value></Value>
	<Value lang="de"></Value>
  </LocaleResource>
  <LocaleResource Name="Plugins.Feed.Froogle.SuccessResult">
	<Value>Feed has been successfully generated.</Value>
	<Value lang="de">Feed wurde erfolgreich erstellt.</Value>
  </LocaleResource>
  <LocaleResource Name="Plugins.Feed.Froogle.Store">
	<Value>Store</Value>
	<Value lang="de">Shop</Value>
  </LocaleResource>
  <LocaleResource Name="Plugins.Feed.Froogle.Store.Hint">
	<Value>Select the store that will be used to generate the feed.</Value>
	<Value lang="de">Wählen Sie den Shop, für den der Feed erstellt werden soll.</Value>
  </LocaleResource>
  
  <LocaleResource Name="Plugins.Feed.Billiger.ClickHere">
	<Value></Value>
	<Value lang="de"></Value>
  </LocaleResource>
  <LocaleResource Name="Plugins.Feed.Billiger.SuccessResult">
	<Value>Feed has been successfully generated.</Value>
	<Value lang="de">Feed wurde erfolgreich erstellt.</Value>
  </LocaleResource>
  <LocaleResource Name="Plugins.Feed.Billiger.Store">
	<Value>Store</Value>
	<Value lang="de">Shop</Value>
  </LocaleResource>
  <LocaleResource Name="Plugins.Feed.Billiger.Store.Hint">
	<Value>Select the store that will be used to generate the feed.</Value>
	<Value lang="de">Wählen Sie den Shop, für den der Feed erstellt werden soll.</Value>
  </LocaleResource>
  
  <LocaleResource Name="Plugins.Feed.ElmarShopinfo.ClickHere">
	<Value></Value>
	<Value lang="de"></Value>
  </LocaleResource>
  <LocaleResource Name="Plugins.Feed.ElmarShopinfo.SuccessResult">
	<Value>Feed has been successfully generated.</Value>
	<Value lang="de">Feed wurde erfolgreich erstellt.</Value>
  </LocaleResource>
  <LocaleResource Name="Plugins.Feed.ElmarShopinfo.Store">
	<Value>Store</Value>
	<Value lang="de">Shop</Value>
  </LocaleResource>
  <LocaleResource Name="Plugins.Feed.ElmarShopinfo.Store.Hint">
	<Value>Select the store that will be used to generate the feed.</Value>
	<Value lang="de">Wählen Sie den Shop, für den der Feed erstellt werden soll.</Value>
  </LocaleResource>
  <LocaleResource Name="Plugins.Feed.ElmarShopinfo.StaticFileXmlUrl">
	<Value></Value>
	<Value lang="de"></Value>
  </LocaleResource>
  <LocaleResource Name="Plugins.Feed.ElmarShopinfo.StaticFileXmlUrl.Hint">
	<Value></Value>
	<Value lang="de"></Value>
  </LocaleResource>
  
  <LocaleResource Name="Plugins.Feed.Guenstiger.ClickHere">
	<Value></Value>
	<Value lang="de"></Value>
  </LocaleResource>
  <LocaleResource Name="Plugins.Feed.Guenstiger.SuccessResult">
	<Value>Feed has been successfully generated.</Value>
	<Value lang="de">Feed wurde erfolgreich erstellt.</Value>
  </LocaleResource>
  <LocaleResource Name="Plugins.Feed.Guenstiger.Store">
	<Value>Store</Value>
	<Value lang="de">Shop</Value>
  </LocaleResource>
  <LocaleResource Name="Plugins.Feed.Guenstiger.Store.Hint">
	<Value>Select the store that will be used to generate the feed.</Value>
	<Value lang="de">Wählen Sie den Shop, für den der Feed erstellt werden soll.</Value>
  </LocaleResource>
  
  <LocaleResource Name="Plugins.Feed.Shopwahl.ClickHere">
	<Value></Value>
	<Value lang="de"></Value>
  </LocaleResource>
  <LocaleResource Name="Plugins.Feed.Shopwahl.SuccessResult">
	<Value>Feed has been successfully generated.</Value>
	<Value lang="de">Feed wurde erfolgreich erstellt.</Value>
  </LocaleResource>
  <LocaleResource Name="Plugins.Feed.Shopwahl.Store">
	<Value>Store</Value>
	<Value lang="de">Shop</Value>
  </LocaleResource>
  <LocaleResource Name="Plugins.Feed.Shopwahl.Store.Hint">
	<Value>Select the store that will be used to generate the feed.</Value>
	<Value lang="de">Wählen Sie den Shop, für den der Feed erstellt werden soll.</Value>
  </LocaleResource>
  
  <LocaleResource Name="Admin.Configuration.Settings.NoneWithThatId">
	<Value>No setting could be loaded with the specified ID.</Value>
	<Value lang="de">Eine Einstellung mit dieser ID wurde nicht gefunden.</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.AllSettings.Fields.StoreName">
	<Value>Store</Value>
	<Value lang="de">Shop</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.AllSettings.Fields.StoreName.Hint">
	<Value>Name of the store</Value>
	<Value lang="de">Name des shops</Value>
  </LocaleResource>
  
  <LocaleResource Name="Admin.Configuration.Settings.StoreScope">
	<Value>Multi-store configuration for</Value>
	<Value lang="de">Multi-Shop Konfiguration für</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.StoreScope.CheckAll">
	<Value>Switch all on/off</Value>
	<Value lang="de">Alle aus/anschalten</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.StoreScope.CheckAll.Hint">
	<Value>Switch on if you want to set a custom value for this shop</Value>
	<Value lang="de">An- bzw. ausschalten, falls Sie für diesen Shop separate Werte festlegen möchten</Value>
  </LocaleResource>
  
  <LocaleResource Name="Admin.Configuration.Settings.GeneralCommon.CaptchaEnabledNoKeys">
	<Value>Captcha is enabled but the appropriate keys are not entered.</Value>
	<Value lang="de">Captcha wurde aktiviert, aber die zugehörigen Schlüssel fehlen.</Value>
  </LocaleResource>

  <LocaleResource Name="Plugins.Shipping.ByWeight.Fields.Store">
    <Value>Store</Value>
    <Value lang="de">Shop</Value>
  </LocaleResource>
  <LocaleResource Name="Plugins.Shipping.ByWeight.Fields.Store.Hint">
    <Value>If an asterisk is selected, then this shipping rate will apply to all stores.</Value>
    <Value lang="de">Wird das Sternchen ausgewählt, so wird die Rate auf alle Shops angewandt.</Value>
  </LocaleResource>
  
  <LocaleResource Name="Plugins.Payment.CashOnDelivery.AdditionalFeePercentage">
    <Value>Additional fee. Use percentage</Value>
    <Value lang="de">Zusätzliche Gebühren (prozentual)</Value>
  </LocaleResource>
  <LocaleResource Name="Plugins.Payment.CashOnDelivery.AdditionalFeePercentage.Hint">
    <Value>Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used.</Value>
    <Value lang="de">Zusätzliche prozentuale Gebühr zum Gesamtbetrag. Es wird ein fester Wert verwendet, falls diese Option nicht aktiviert ist.</Value>
  </LocaleResource>
  
  <LocaleResource Name="Plugins.Payment.CheckMoneyOrder.AdditionalFeePercentage">
    <Value>Additional fee. Use percentage</Value>
    <Value lang="de">Zusätzliche Gebühren (prozentual)</Value>
  </LocaleResource>
  <LocaleResource Name="Plugins.Payment.CheckMoneyOrder.AdditionalFeePercentage.Hint">
    <Value>Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used.</Value>
    <Value lang="de">Zusätzliche prozentuale Gebühr zum Gesamtbetrag. Es wird ein fester Wert verwendet, falls diese Option nicht aktiviert ist.</Value>
  </LocaleResource>

  <LocaleResource Name="Plugins.Payment.DirectDebit.AdditionalFeePercentage">
    <Value>Additional fee. Use percentage</Value>
    <Value lang="de">Zusätzliche Gebühren (prozentual)</Value>
  </LocaleResource>
  <LocaleResource Name="Plugins.Payment.DirectDebit.AdditionalFeePercentage.Hint">
    <Value>Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used.</Value>
    <Value lang="de">Zusätzliche prozentuale Gebühr zum Gesamtbetrag. Es wird ein fester Wert verwendet, falls diese Option nicht aktiviert ist.</Value>
  </LocaleResource>
  
  <LocaleResource Name="Plugins.Payment.Invoice.AdditionalFeePercentage">
    <Value>Additional fee. Use percentage</Value>
    <Value lang="de">Zusätzliche Gebühren (prozentual)</Value>
  </LocaleResource>
  <LocaleResource Name="Plugins.Payment.Invoice.AdditionalFeePercentage.Hint">
    <Value>Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used.</Value>
    <Value lang="de">Zusätzliche prozentuale Gebühr zum Gesamtbetrag. Es wird ein fester Wert verwendet, falls diese Option nicht aktiviert ist.</Value>
  </LocaleResource>
  
  <LocaleResource Name="Plugins.Payment.PayInStore.AdditionalFeePercentage">
    <Value>Additional fee. Use percentage</Value>
    <Value lang="de">Zusätzliche Gebühren (prozentual)</Value>
  </LocaleResource>
  <LocaleResource Name="Plugins.Payment.PayInStore.AdditionalFeePercentage.Hint">
    <Value>Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used.</Value>
    <Value lang="de">Zusätzliche prozentuale Gebühr zum Gesamtbetrag. Es wird ein fester Wert verwendet, falls diese Option nicht aktiviert ist.</Value>
  </LocaleResource>
  
  <LocaleResource Name="Plugins.Payment.Prepayment.AdditionalFeePercentage">
    <Value>Additional fee. Use percentage</Value>
    <Value lang="de">Zusätzliche Gebühren (prozentual)</Value>
  </LocaleResource>
  <LocaleResource Name="Plugins.Payment.Prepayment.AdditionalFeePercentage.Hint">
    <Value>Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used.</Value>
    <Value lang="de">Zusätzliche prozentuale Gebühr zum Gesamtbetrag. Es wird ein fester Wert verwendet, falls diese Option nicht aktiviert ist.</Value>
  </LocaleResource>
  
  <LocaleResource Name="Admin.Configuration.Plugins.Info">
    <Value>Plugin Info</Value>
    <Value lang="de">Plugin Info</Value>
  </LocaleResource>
 
</Language>
'

DECLARE @forceResUpdate bit = 0
SET @forceResUpdate = 0

CREATE TABLE #ResTmp
(
	[Name] [nvarchar](200) NOT NULL, [Lang] [nvarchar](2) NULL DEFAULT N'', [Value] [nvarchar](max) NOT NULL
)

--flatten the 'Value' nodes into temp table
INSERT INTO #ResTmp (Name, Lang, Value)
SELECT
	R.rref.value('@Name', 'nvarchar(200)'),
	COALESCE(V.vref.value('@lang', 'nvarchar(2)'), ''),
	COALESCE(V.vref.value('text()[1]', 'nvarchar(MAX)'), '')
FROM
	@resources.nodes('//Language/LocaleResource') AS R(rref)
CROSS APPLY
	R.rref.nodes('Value') AS V(vref)


--do it for each existing language
DECLARE @ExistingLanguageID int
DECLARE @ExistingSeoCode nvarchar(2)
DECLARE cur_existinglanguage CURSOR FOR
SELECT [ID], [UniqueSeoCode] AS Lang FROM [Language]
OPEN cur_existinglanguage
FETCH NEXT FROM cur_existinglanguage INTO @ExistingLanguageID, @ExistingSeoCode
WHILE @@FETCH_STATUS = 0
BEGIN
	DECLARE @Name nvarchar(200)
	DECLARE @Lang nvarchar(2)
	DECLARE @Value nvarchar(MAX)
	DECLARE cur_localeresource CURSOR FOR
	SELECT Name, Lang, Value FROM #ResTmp WHERE Lang = @ExistingSeoCode OR Lang = '' ORDER BY Lang, Name
	OPEN cur_localeresource
	FETCH NEXT FROM cur_localeresource INTO @Name, @Lang, @Value
	WHILE @@FETCH_STATUS = 0
	BEGIN

		IF (EXISTS (SELECT 1 FROM [LocaleStringResource] WHERE LanguageID=@ExistingLanguageID AND ResourceName=@Name))
		BEGIN
			UPDATE [LocaleStringResource]
			SET [ResourceValue]=@Value
			WHERE LanguageID=@ExistingLanguageID AND ResourceName=@Name AND (@forceResUpdate=1 OR (IsTouched is null OR IsTouched = 0))
		END
		ELSE 
		BEGIN
			INSERT INTO [LocaleStringResource] (LanguageId, ResourceName, ResourceValue) VALUES (@ExistingLanguageID, @Name, @Value)
		END
		
		IF (@Value is null or @Value = '')
		BEGIN
			DELETE [LocaleStringResource] WHERE LanguageID=@ExistingLanguageID AND ResourceName=@Name
		END
	
		FETCH NEXT FROM cur_localeresource INTO @Name, @Lang, @Value
	END
	CLOSE cur_localeresource
	DEALLOCATE cur_localeresource


	--fetch next language identifier
	FETCH NEXT FROM cur_existinglanguage INTO @ExistingLanguageID, @ExistingSeoCode
END
CLOSE cur_existinglanguage
DEALLOCATE cur_existinglanguage

DROP TABLE #ResTmp
GO




IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Store]') and OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	CREATE TABLE [dbo].[Store](
		[Id] [int] IDENTITY(1,1) NOT NULL,
		[Name] nvarchar(400) NOT NULL,
		[Url] nvarchar(400) NOT NULL,
		[SslEnabled] bit NOT NULL,
		[SecureUrl] nvarchar(400) NULL,
		[Hosts] nvarchar(1000) NULL,
		[LogoPictureId] int NOT NULL,
		[DisplayOrder] int NOT NULL,
	PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
	)

	DECLARE @DEFAULT_STORE_NAME nvarchar(400)
	SELECT @DEFAULT_STORE_NAME = [Value] FROM [Setting] WHERE [name] = N'storeinformationsettings.storename'
	
	if (@DEFAULT_STORE_NAME is null)
		SET @DEFAULT_STORE_NAME = N'Your store name' 

	DECLARE @DEFAULT_STORE_URL nvarchar(400)
	SELECT @DEFAULT_STORE_URL = [Value] FROM [Setting] WHERE [name] = N'storeinformationsettings.storeurl'
	
	if (@DEFAULT_STORE_URL is null)
		SET @DEFAULT_STORE_URL = N'http://www.yourstore.com/'
		
	DECLARE @DEFAULT_STORE_LOGOSETTING nvarchar(400)
	SELECT @DEFAULT_STORE_LOGOSETTING = [Value] FROM [Setting] WHERE [name] = N'storeinformationsettings.logopictureid'
	
	if (@DEFAULT_STORE_LOGOSETTING is null)
		SET @DEFAULT_STORE_LOGOSETTING = 0
		
	DECLARE @DEFAULT_STORE_LOGOPICTUREID int
	SET @DEFAULT_STORE_LOGOPICTUREID = CAST(@DEFAULT_STORE_LOGOSETTING AS INT)

	--create the first store
	INSERT INTO [Store] ([Name], [Url], [SslEnabled], [Hosts], [LogoPictureId], [DisplayOrder])
	VALUES (@DEFAULT_STORE_NAME, @DEFAULT_STORE_URL, 0, N'yourstore.com,www.yourstore.com', CAST(@DEFAULT_STORE_LOGOPICTUREID AS INT), 1)

	DELETE FROM [Setting] WHERE [name] = N'storeinformationsettings.storename' 
	DELETE FROM [Setting] WHERE [name] = N'storeinformationsettings.storeurl'
	DELETE FROM [Setting] WHERE [name] = N'storeinformationsettings.logopictureid'
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


	--add it to admin role by default
	DECLARE @AdminCustomerRoleId int
	SELECT @AdminCustomerRoleId = Id
	FROM [CustomerRole]
	WHERE IsSystemRole=1 and [SystemName] = N'Administrators'

	INSERT [dbo].[PermissionRecord_Role_Mapping] ([PermissionRecord_Id], [CustomerRole_Id])
	VALUES (@PermissionRecordId, @AdminCustomerRoleId)

	--codehint: sm-add
	--add it to super-admin role by default
	DECLARE @SuperAdminCustomerRoleId int
	SELECT @SuperAdminCustomerRoleId = Id
	FROM [CustomerRole]
	WHERE IsSystemRole=1 and [SystemName] = N'SuperAdmins'

	IF NOT @SuperAdminCustomerRoleId IS NULL
	BEGIN
		INSERT [dbo].[PermissionRecord_Role_Mapping] ([PermissionRecord_Id], [CustomerRole_Id])
		VALUES (@PermissionRecordId, @SuperAdminCustomerRoleId)
	END		
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[StoreMapping]') and OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[StoreMapping](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EntityId] [int] NOT NULL,
	[EntityName] nvarchar(400) NOT NULL,
	[StoreId] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO

IF NOT EXISTS (SELECT 1 from sys.indexes WHERE [NAME]=N'IX_StoreMapping_EntityId_EntityName' and object_id=object_id(N'[StoreMapping]'))
BEGIN
	CREATE NONCLUSTERED INDEX [IX_StoreMapping_EntityId_EntityName] ON [StoreMapping] ([EntityId] ASC, [EntityName] ASC)
END
GO

--Store mapping for manufacturers
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Manufacturer]') and NAME='LimitedToStores')
BEGIN
	ALTER TABLE [Manufacturer] ADD [LimitedToStores] bit NULL
END
GO
UPDATE [Manufacturer] SET [LimitedToStores] = 0 WHERE [LimitedToStores] IS NULL
GO
ALTER TABLE [Manufacturer] ALTER COLUMN [LimitedToStores] bit NOT NULL
GO

--Store mapping for categories
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Category]') and NAME='LimitedToStores')
BEGIN
	ALTER TABLE [Category] ADD [LimitedToStores] bit NULL
END
GO
UPDATE [Category] SET [LimitedToStores] = 0 WHERE [LimitedToStores] IS NULL
GO
ALTER TABLE [Category] ALTER COLUMN [LimitedToStores] bit NOT NULL
GO

--Store mapping for products
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='LimitedToStores')
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
		FROM sys.objects
		WHERE object_id = OBJECT_ID(N'[ProductLoadAllPaged]') AND OBJECTPROPERTY(object_id,N'IsProcedure') = 1)
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
		FROM [Product_SpecificationAttribute_Mapping] [psam]
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
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Language]') and NAME='LimitedToStores')
BEGIN
	ALTER TABLE [Language] ADD [LimitedToStores] bit NULL
END
GO

UPDATE [Language] SET [LimitedToStores] = 0 WHERE [LimitedToStores] IS NULL
GO

ALTER TABLE [Language] ALTER COLUMN [LimitedToStores] bit NOT NULL
GO

--Store mapping for currencies
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Currency]') and NAME='LimitedToStores')
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
           FROM   sys.objects
           WHERE  name = 'Customer_Currency'
           AND parent_object_id = Object_id('Customer')
           AND Objectproperty(object_id,N'IsForeignKey') = 1)
BEGIN
	ALTER TABLE dbo.[Customer]
	DROP CONSTRAINT Customer_Currency
	
	EXEC ('UPDATE [Customer] SET [CurrencyId] = 0 WHERE [CurrencyId] IS NULL')
	EXEC ('ALTER TABLE [Customer] ALTER COLUMN [CurrencyId] int NOT NULL')
END
GO

IF EXISTS (SELECT 1
           FROM   sys.objects
           WHERE  name = 'Customer_Language'
           AND parent_object_id = Object_id('Customer')
           AND Objectproperty(object_id,N'IsForeignKey') = 1)
BEGIN
	ALTER TABLE dbo.[Customer]
	DROP CONSTRAINT Customer_Language
	
	EXEC ('UPDATE [Customer] SET [LanguageId] = 0 WHERE [LanguageId] IS NULL')
	EXEC ('ALTER TABLE [Customer] ALTER COLUMN [LanguageId] int NOT NULL')
END
GO

IF EXISTS (SELECT 1
           FROM   sys.objects
           WHERE  name = 'Customer_Affiliate'
           AND parent_object_id = Object_id('Customer')
           AND Objectproperty(object_id,N'IsForeignKey') = 1)
BEGIN
	ALTER TABLE dbo.[Customer] DROP CONSTRAINT Customer_Affiliate
END
GO

IF EXISTS (SELECT 1
           FROM   sys.objects
           WHERE  name = 'Affiliate_AffiliatedCustomers'
           AND parent_object_id = Object_id('Customer')
           AND Objectproperty(object_id,N'IsForeignKey') = 1)
BEGIN
	ALTER TABLE dbo.[Customer] DROP CONSTRAINT Affiliate_AffiliatedCustomers
END
GO

UPDATE [Customer] SET [AffiliateId] = 0 WHERE [AffiliateId] IS NULL
GO

ALTER TABLE [Customer] ALTER COLUMN [AffiliateId] int NOT NULL
GO

IF EXISTS (SELECT 1
           FROM   sys.objects
           WHERE  name = 'Order_Affiliate'
           AND parent_object_id = Object_id('Order')
           AND Objectproperty(object_id,N'IsForeignKey') = 1)
BEGIN
	ALTER TABLE dbo.[Order]	DROP CONSTRAINT Order_Affiliate
END
GO

IF EXISTS (SELECT 1
           FROM   sys.objects
           WHERE  name = 'Affiliate_AffiliatedOrders'
           AND parent_object_id = Object_id('Order')
           AND Objectproperty(object_id,N'IsForeignKey') = 1)
BEGIN
	ALTER TABLE dbo.[Order] DROP CONSTRAINT Affiliate_AffiliatedOrders
END
GO

UPDATE [Order] SET [AffiliateId] = 0 WHERE [AffiliateId] IS NULL
GO

ALTER TABLE [Order] ALTER COLUMN [AffiliateId] int NOT NULL
GO


--Store mapping to shopping cart items
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[ShoppingCartItem]') and NAME='StoreId')
BEGIN
	ALTER TABLE [ShoppingCartItem] ADD [StoreId] int NULL
END
GO

DECLARE @DEFAULT_STORE_ID int
SELECT @DEFAULT_STORE_ID = [Id] FROM [Store] ORDER BY [DisplayOrder]
UPDATE [ShoppingCartItem] SET [StoreId] = @DEFAULT_STORE_ID WHERE [StoreId] IS NULL
GO

ALTER TABLE [ShoppingCartItem] ALTER COLUMN [StoreId] int NOT NULL
GO

--Store mapping to orders
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Order]') and NAME='StoreId')
BEGIN
	ALTER TABLE [Order] ADD [StoreId] int NULL
END
GO

DECLARE @DEFAULT_STORE_ID int
SELECT @DEFAULT_STORE_ID = [Id] FROM [Store] ORDER BY [DisplayOrder]
UPDATE [Order] SET [StoreId] = @DEFAULT_STORE_ID WHERE [StoreId] IS NULL
GO

ALTER TABLE [Order] ALTER COLUMN [StoreId] int NOT NULL
GO

--Store mapping to return requests
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[ReturnRequest]') and NAME='StoreId')
BEGIN
	ALTER TABLE [ReturnRequest] ADD [StoreId] int NULL
END
GO

DECLARE @DEFAULT_STORE_ID int
SELECT @DEFAULT_STORE_ID = [Id] FROM [Store] ORDER BY [DisplayOrder]
UPDATE [ReturnRequest] SET [StoreId] = @DEFAULT_STORE_ID WHERE [StoreId] IS NULL
GO

ALTER TABLE [ReturnRequest] ALTER COLUMN [StoreId] int NOT NULL
GO

--Store mapping to message templates
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[MessageTemplate]') and NAME='LimitedToStores')
BEGIN
	ALTER TABLE [MessageTemplate] ADD [LimitedToStores] bit NULL
END
GO

UPDATE [MessageTemplate] SET [LimitedToStores] = 0 WHERE [LimitedToStores] IS NULL
GO

ALTER TABLE [MessageTemplate] ALTER COLUMN [LimitedToStores] bit NOT NULL
GO

--Store mapping for topics
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Topic]') and NAME='LimitedToStores')
BEGIN
	ALTER TABLE [Topic]	ADD [LimitedToStores] bit NULL
END
GO

UPDATE [Topic] SET [LimitedToStores] = 0 WHERE [LimitedToStores] IS NULL
GO

ALTER TABLE [Topic] ALTER COLUMN [LimitedToStores] bit NOT NULL
GO

--Store mapping to news
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[News]') and NAME='LimitedToStores')
BEGIN
	ALTER TABLE [News] ADD [LimitedToStores] bit NULL
END
GO

UPDATE [News] SET [LimitedToStores] = 0 WHERE [LimitedToStores] IS NULL
GO

ALTER TABLE [News] ALTER COLUMN [LimitedToStores] bit NOT NULL
GO


--Store mapping to BackInStockSubscription
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[BackInStockSubscription]') and NAME='StoreId')
BEGIN
	ALTER TABLE [BackInStockSubscription] ADD [StoreId] int NULL
END
GO

DECLARE @DEFAULT_STORE_ID int
SELECT @DEFAULT_STORE_ID = [Id] FROM [Store] ORDER BY [DisplayOrder]
UPDATE [BackInStockSubscription] SET [StoreId] = @DEFAULT_STORE_ID WHERE [StoreId] IS NULL
GO

ALTER TABLE [BackInStockSubscription] ALTER COLUMN [StoreId] int NOT NULL
GO


--Store mapping to Forums_PrivateMessage
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Forums_PrivateMessage]') and NAME='StoreId')
BEGIN
	ALTER TABLE [Forums_PrivateMessage] ADD [StoreId] int NULL
END
GO

DECLARE @DEFAULT_STORE_ID int
SELECT @DEFAULT_STORE_ID = [Id] FROM [Store] ORDER BY [DisplayOrder]
UPDATE [Forums_PrivateMessage] SET [StoreId] = @DEFAULT_STORE_ID WHERE [StoreId] IS NULL
GO

ALTER TABLE [Forums_PrivateMessage] ALTER COLUMN [StoreId] int NOT NULL
GO


--GenericAttributes cuold be limited to some specific store name
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[GenericAttribute]') and NAME='StoreId')
BEGIN
	ALTER TABLE [GenericAttribute] ADD [StoreId] int NULL
END
GO

UPDATE [GenericAttribute] SET [StoreId] = 0 WHERE [StoreId] IS NULL
GO

ALTER TABLE [GenericAttribute] ALTER COLUMN [StoreId] int NOT NULL
GO

--delete generic attributes which depends on a specific store now
DELETE FROM [GenericAttribute]
WHERE [KeyGroup] =N'Customer' and [Key]=N'NotifiedAboutNewPrivateMessages' and [StoreId] = 0
GO

DELETE FROM [GenericAttribute]
WHERE [KeyGroup] =N'Customer' and [Key]=N'WorkingDesktopThemeName' and [StoreId] = 0
GO

DELETE FROM [GenericAttribute]
WHERE [KeyGroup] =N'Customer' and [Key]=N'DontUseMobileVersion' and [StoreId] = 0
GO

DELETE FROM [GenericAttribute]
WHERE [KeyGroup] =N'Customer' and [Key]=N'LastContinueShoppingPage' and [StoreId] = 0
GO

DELETE FROM [GenericAttribute]
WHERE [KeyGroup] =N'Customer' and [Key]=N'LastShippingOption' and [StoreId] = 0
GO
DELETE FROM [GenericAttribute]
WHERE [KeyGroup] =N'Customer' and [Key]=N'OfferedShippingOptions' and [StoreId] = 0
GO

--Moved several properties from [Customer] to [GenericAtrribute]
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Customer]') and NAME='TaxDisplayTypeId')
BEGIN
	ALTER TABLE [Customer] DROP COLUMN [TaxDisplayTypeId]
END
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Customer]') and NAME='SelectedPaymentMethodSystemName')
BEGIN
	ALTER TABLE [Customer] DROP COLUMN [SelectedPaymentMethodSystemName]
END
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Customer]') and NAME='UseRewardPointsDuringCheckout')
BEGIN
	ALTER TABLE [Customer] DROP COLUMN [UseRewardPointsDuringCheckout]
END
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Customer]') and NAME='CurrencyId')
BEGIN
	ALTER TABLE [Customer] DROP COLUMN [CurrencyId]
END
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Customer]') and NAME='LanguageId')
BEGIN
	ALTER TABLE [Customer] DROP COLUMN [LanguageId]
END
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Customer]') and NAME='VatNumber')
BEGIN
	ALTER TABLE [Customer] DROP COLUMN [VatNumber]
END
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Customer]') and NAME='VatNumberStatusId')
BEGIN
	ALTER TABLE [Customer] DROP COLUMN [VatNumberStatusId]
END
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Customer]') and NAME='TimeZoneId')
BEGIN
	ALTER TABLE [Customer] DROP COLUMN [TimeZoneId]
END
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Customer]') and NAME='DiscountCouponCode')
BEGIN
	ALTER TABLE [Customer] DROP COLUMN [DiscountCouponCode]
END
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Customer]') and NAME='GiftCardCouponCodes')
BEGIN
	ALTER TABLE [Customer] DROP COLUMN [GiftCardCouponCodes]
END
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Customer]') and NAME='CheckoutAttributes')
BEGIN
	ALTER TABLE [Customer] DROP COLUMN [CheckoutAttributes]
END
GO


--Store mapping to Setting
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Setting]') and NAME='StoreId')
BEGIN
	ALTER TABLE [Setting] ADD [StoreId] int NOT NULL DEFAULT 0
END
GO

UPDATE [Setting] SET [StoreId] = 0 WHERE [StoreId] IS NULL
GO

ALTER TABLE [Setting] ALTER COLUMN [StoreId] int NOT NULL
GO

--Store mapping for blog posts
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[BlogPost]') and NAME='LimitedToStores')
BEGIN
	ALTER TABLE [BlogPost] ADD [LimitedToStores] bit NULL
END
GO

UPDATE [BlogPost] SET [LimitedToStores] = 0 WHERE [LimitedToStores] IS NULL
GO

ALTER TABLE [BlogPost] ALTER COLUMN [LimitedToStores] bit NOT NULL
GO

--do not store product tag count
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[ProductTag]') and NAME='ProductCount')
BEGIN
	ALTER TABLE [ProductTag] DROP COLUMN [ProductCount]
END
GO

--stored procedure to load product tags
IF EXISTS (
		SELECT *
		FROM sys.objects
		WHERE OBJECT_ID = OBJECT_ID(N'[ProductTagCountLoadAll]') AND OBJECTPROPERTY(OBJECT_ID,N'IsProcedure') = 1)
DROP PROCEDURE [ProductTagCountLoadAll]
GO
CREATE PROCEDURE [dbo].[ProductTagCountLoadAll]
(
	@StoreId int
)
AS
BEGIN
	SET NOCOUNT ON
	
	SELECT pt.Id as [ProductTagId], COUNT(p.Id) as [ProductCount]
	FROM ProductTag pt with (NOLOCK)
	LEFT JOIN Product_ProductTag_Mapping pptm with (NOLOCK) ON pt.[Id] = pptm.[ProductTag_Id]
	LEFT JOIN Product p with (NOLOCK) ON pptm.[Product_Id] = p.[Id]
	WHERE
		p.[Deleted] = 0
		AND p.Published = 1
		AND (@StoreId = 0 or (p.LimitedToStores = 0 OR EXISTS (
			SELECT 1 FROM [StoreMapping] sm
			WHERE [sm].EntityId = p.Id AND [sm].EntityName = 'Product' and [sm].StoreId=@StoreId
			)))
	GROUP BY pt.Id
	ORDER BY pt.Id
END
GO

--more indexes
IF NOT EXISTS (SELECT 1 from sys.indexes WHERE [NAME]=N'IX_Category_LimitedToStores' and object_id=object_id(N'[Category]'))
BEGIN
	CREATE NONCLUSTERED INDEX [IX_Category_LimitedToStores] ON [Category] ([LimitedToStores] ASC)
END
GO

IF NOT EXISTS (SELECT 1 from sys.indexes WHERE [NAME]=N'IX_Manufacturer_LimitedToStores' and object_id=object_id(N'[Manufacturer]'))
BEGIN
	CREATE NONCLUSTERED INDEX [IX_Manufacturer_LimitedToStores] ON [Manufacturer] ([LimitedToStores] ASC)
END
GO

IF NOT EXISTS (SELECT 1 from sys.indexes WHERE [NAME]=N'IX_Product_LimitedToStores' and object_id=object_id(N'[Product]'))
BEGIN
	CREATE NONCLUSTERED INDEX [IX_Product_LimitedToStores] ON [Product] ([LimitedToStores] ASC)
END
GO


--new column 
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[TierPrice]') and NAME='StoreId')
BEGIN
	ALTER TABLE [TierPrice] ADD [StoreId] int NULL
END
GO

UPDATE [TierPrice] SET [StoreId] = 0 WHERE [StoreId] IS NULL
GO

ALTER TABLE [TierPrice] ALTER COLUMN [StoreId] int NOT NULL
GO


--shipping by weight plugin
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[ShippingByWeight]') and OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	--new [StoreId] column
	EXEC ('IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id(''[ShippingByWeight]'') and NAME=''StoreId'')
	BEGIN
		ALTER TABLE [ShippingByWeight] ADD [StoreId] int NULL

		exec(''UPDATE [ShippingByWeight] SET [StoreId] = 0'')
		
		EXEC (''ALTER TABLE [ShippingByWeight] ALTER COLUMN [StoreId] int NOT NULL'')
	END')
END
GO

--shipping by total plugin
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[ShippingByTotal]') and OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	--new [StoreId] column
	EXEC ('IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id(''[ShippingByTotal]'') and NAME=''StoreId'')
	BEGIN
		ALTER TABLE [ShippingByTotal] ADD [StoreId] int NULL

		exec(''UPDATE [ShippingByTotal] SET [StoreId] = 0'')

		EXEC (''ALTER TABLE [ShippingByTotal] ALTER COLUMN [StoreId] int NOT NULL'')
	END')
END
GO 

-- StoreMapping Store foreign key
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE 
	name = 'StoreMapping_Store' AND parent_object_id = Object_id('StoreMapping') AND Objectproperty(object_id,N'IsForeignKey') = 1)
BEGIN
	ALTER TABLE [dbo].[StoreMapping] WITH CHECK ADD CONSTRAINT [StoreMapping_Store]
	FOREIGN KEY([StoreId]) REFERENCES [dbo].[Store] ([Id])
	ON DELETE CASCADE
END
GO

--Store mapping to theme variables
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[ThemeVariable]') and NAME='StoreId')
BEGIN
	ALTER TABLE [ThemeVariable] ADD [StoreId] int NULL
END
GO

DECLARE @DEFAULT_STORE_ID int
SELECT TOP 1 @DEFAULT_STORE_ID = [Id] FROM [Store] ORDER BY [DisplayOrder]
UPDATE [ThemeVariable] SET [StoreId] = @DEFAULT_STORE_ID WHERE [StoreId] IS NULL
GO

ALTER TABLE [ThemeVariable] ALTER COLUMN [StoreId] int NOT NULL
GO

--StoreId of theme settings must have valid store identifier
DECLARE @DEFAULT_STORE_ID int
SELECT TOP 1 @DEFAULT_STORE_ID = [Id] FROM [Store] ORDER BY [DisplayOrder]
UPDATE [Setting] SET [StoreId] = @DEFAULT_STORE_ID WHERE [StoreId] = 0 And [Name] Like 'themesettings.%'
GO