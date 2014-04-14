--upgrade scripts for smartstore.net (only specific parts)

--new locale resources
DECLARE @resources xml
--a resource will be deleted if its value is empty   
SET @resources='
<Language>
	<LocaleResource Name="Admin.Configuration.DeliveryTime.EditDeliveryTimeDetails">
		<Value>Edit delivery time</Value>
		<Value lang="de">Lieferzeit bearbeiten</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Configuration.DeliveryTimes.Updated">
		<Value>Delivery time was successfully updated</Value>
		<Value lang="de">Die Lieferzeit wurde erfolgreich aktualisiert.</Value>
	</LocaleResource>
	
	<LocaleResource Name="products.callforprice">
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="products.callforprice">
		<Value lang="de">Preis auf Anfrage</Value>
	</LocaleResource>
	
	<LocaleResource Name="Admin.WebApi.AuthResult.Success">
		<Value>Successfully authenticated.</Value>
		<Value lang="de">Erfolgreiche Authentifizierung.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.WebApi.AuthResult.FailedForUnknownReason">
		<Value>Authentication failed for unknown reason.</Value>
		<Value lang="de">Authentifizierung aus unbekanntem Grund fehlgeschlagen.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.WebApi.AuthResult.ApiUnavailable">
		<Value>API not available.</Value>
		<Value lang="de">API ist nicht erreichbar.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.WebApi.AuthResult.InvalidAuthorizationHeader">
		<Value>Request contains an invalid authorization header.</Value>
		<Value lang="de">Anfrage enthält einen ungültigen Authorisierungs-Header.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.WebApi.AuthResult.InvalidSignature">
		<Value>The sent HMAC signature does not match the signature calculated by the server.</Value>
		<Value lang="de">Die gesendete HMAC-Signatur stimmt nicht mit der durch den Server berechneten Signatur überein.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.WebApi.AuthResult.InvalidTimestamp">
		<Value>The send timestamp is missing or has an invalid format.</Value>
		<Value lang="de">Der gesendete Zeitstempel fehlt oder besitzt ein ungültiges Format.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.WebApi.AuthResult.TimestampOutOfPeriod">
		<Value>The sent timestamp deviates too much from the server time.</Value>
		<Value lang="de">Der gesendete Zeitstempel weicht zu weit von der Server-Zeit ab.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.WebApi.AuthResult.TimestampOlderThanLastRequest">
		<Value>The sent timestamp is older than the last request of the user.</Value>
		<Value lang="de">Der gesendete Zeitstempel ist älter als die letzte Anfrage des Nutzers.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.WebApi.AuthResult.MissingMessageRepresentationParameter">
		<Value>There is at least one message parameter missing which is required for security purpose.</Value>
		<Value lang="de">Es fehlt mindestens ein aus Sicherheitsgründen zu übermittelnder Nachrichten-Parameter.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.WebApi.AuthResult.ContentMd5NotMatching">
		<Value>The sent content MD5 hash does not match the hash calculated by the server.</Value>
		<Value lang="de">Der gesendete MD5-Inhalts-Hash stimmt nicht mit dem durch den Server berechneten Hash überein.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.WebApi.AuthResult.UserUnknown">
		<Value>The user is unknown or has no access rights for the API.</Value>
		<Value lang="de">Der Benutzer ist unbekannt oder besitzt keine Zugriffberechtigung für die API.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.WebApi.AuthResult.UserDisabled">
		<Value>The user is disabled for accessing the API.</Value>
		<Value lang="de">Der Benutzer ist für den API-Zugriff gesperrt.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.WebApi.AuthResult.UserInvalid">
		<Value>The User-ID is missing or invalid.</Value>
		<Value lang="de">Die Benutzer-ID wurde nicht übermittelt oder ist ungültig.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.WebApi.AuthResult.UserHasNoPermission">
		<Value>The user does not have enough rights for his request.</Value>
		<Value lang="de">Der Benutzer besitzt nicht genügend Rechte für seine Anfrage.</Value>
	</LocaleResource>

	<LocaleResource Name="Admin.WebApi.UnauthorizedRequest">
		<Value>Unauthorized API request ({0})</Value>
		<Value lang="de">Unauthorisierte API-Anfrage ({0})</Value>
	</LocaleResource>
	
	<LocaleResource Name="Admin.Configuration.Themes.Options"><Value></Value></LocaleResource>
	<LocaleResource Name="Admin.Configuration.Themes.Options.Info"><Value></Value></LocaleResource>
	<LocaleResource Name="Admin.Configuration.Themes.Options.Info">
		<Value>Disable resource bundling and caching in order to test and debug theme changes more easily.</Value>
		<Value lang="de">Deaktivieren Sie Ressourcen-Bundling und -Caching, um Theme-Änderungen optimal testen und debuggen zu können.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Configuration.Themes.Option.CssCacheEnabled"><Value></Value></LocaleResource>
	<LocaleResource Name="Admin.Configuration.Themes.Option.CssCacheEnabled.Hint"><Value></Value></LocaleResource>
	<LocaleResource Name="Admin.Configuration.Themes.Option.CssMinifyEnabled"><Value></Value></LocaleResource>
	<LocaleResource Name="Admin.Configuration.Themes.Option.CssMinifyEnabled.Hint"><Value></Value></LocaleResource>
	
	<LocaleResource Name="Admin.Configuration.Shipping.Methods.Fields.IgnoreCharges">
		<Value>No additional charges</Value>
		<Value lang="de">Keine zusätzlichen Kosten</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Configuration.Shipping.Methods.Fields.IgnoreCharges.Hint">
		<Value>No additional charges are to be calculated when selecting this shipping method.</Value>
		<Value lang="de">Bei Auswahl dieser Versandmethode sollen keine zusätzlichen Kosten berechnet werden.</Value>
	</LocaleResource>
	
	<LocaleResource Name="Admin.Common.ChooseToken">
		<Value>Choose token</Value>
		<Value lang="de">Platzhalter auswählen</Value>
	</LocaleResource>
	
	<LocaleResource Name="Admin.System.Log.Fields.UpdatedOn">
		<Value>Updated on</Value>
		<Value lang="de">Aktualisiert am</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.System.Log.Fields.UpdatedOn.Hint">
		<Value>Date of the last update.</Value>
		<Value lang="de">Datum der letzten Aktualisierung.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.System.Log.Fields.Frequency">
		<Value>Frequency</Value>
		<Value lang="de">Häufigkeit</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.System.Log.Fields.Frequency.Hint">
		<Value>Number of occurrences of this event.</Value>
		<Value lang="de">Anzahl des Auftretens dieses Ereignisses.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.System.Log.List.MinFrequency">
		<Value>Frequency minimum</Value>
		<Value lang="de">Häufigkeit Minimum</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.System.Log.List.MinFrequency.Hint">
		<Value>How often an event minimum must have occurred.</Value>
		<Value lang="de">Wie oft ein Ereigniss minimal aufgetreten sein muss.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.System.Log.Fields.ContentHash">
		<Value>Content hash</Value>
		<Value lang="de">Inhalts-Hash</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.System.Log.Fields.ContentHash.Hint">
		<Value>Represents the log content. Is required for the frequency count.</Value>
		<Value lang="de">Repräsentiert den Ereignissinhalt. Wird für die Häufigkeitszählung benötigt.</Value>
	</LocaleResource>

	<LocaleResource Name="Admin.Configuration.Plugins.Resources.UpdateAll">
		<Value>Update language resources of all plugins</Value>
		<Value lang="de">Sprachressourcen aller Plugins aktualisieren</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Configuration.Plugins.Resources.UpdateAllConfirm">
		<Value>Do you like to update the language resources of all plugins?</Value>
		<Value lang="de">Möchten Sie die Sprachressourcen aller Plugins aktualisieren?</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Configuration.Settings.ShoppingCart.ShowDeliveryTimes">
		<Value>Display delivery times</Value>
		<Value lang="de">Lieferzeiten anzeigen</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Configuration.Settings.ShoppingCart.ShowDeliveryTimes.Hint">
		<Value>Determines whether delivery times should be displayed in shopping cart</Value>
		<Value lang="de">Bestimmt ob Lieferzeiten im Warenkorb angezeigt wird.</Value>
	</LocaleResource>
	
	<LocaleResource Name="Admin.Configuration.Settings.GeneralCommon.SocialSettings.YoutubeLink">
		<Value>Youtube Link</Value>
		<Value lang="de">Youtube Link</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Configuration.Settings.GeneralCommon.SocialSettings.YoutubeLink.Hint">
		<Value>Leave this field empty if the youtube link should not be shown</Value>
		<Value lang="de">Lassen Sie dieses Feld leer, wenn der Youtube Link nicht angezeigt werden soll</Value>
	</LocaleResource>
	
	<LocaleResource Name="RSS.RecentlyAddedProducts">
		<Value>Recently added products</Value>
		<Value lang="de">Neu zugefügte Produkte</Value>
	</LocaleResource>
	<LocaleResource Name="RSS.InformationAboutProducts">
		<Value>Information about products</Value>
		<Value lang="de">Produkt-Informationen</Value>
	</LocaleResource>
	
	<LocaleResource Name="Admin.Configuration.Currencies.Fields.DomainEndings">
		<Value>Domain endings</Value>
		<Value lang="de">Domain Endungen</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Configuration.Currencies.Fields.DomainEndings.Hint">
		<Value>Selects this currency as the default currency based on the domain extension. Example: .ch</Value>
		<Value lang="de">Wählt diese Währung als Standardwährung auf Basis der Domain-Endung aus. Beispiel: .ch</Value>
	</LocaleResource>

	<LocaleResource Name="Admin.Configuration.Settings.ShoppingCart.ShowShortDesc">
		<Value>Display product short description</Value>
		<Value lang="de">Zeige Kurzbeschreibung der Produkte</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Configuration.Settings.ShoppingCart.ShowShortDesc.Hint">
		<Value>Determines whether to display the product short description in the order summary</Value>
		<Value lang="de">Bestimmt ob die Kurzbeschreibungen der Produkte im Warenkorb angezeigt werden sollen</Value>
	</LocaleResource>
	
	<LocaleResource Name="Admin.Catalog.Products.List.DownloadPdf">
		<Value>Download catalog as pdf file</Value>
		<Value lang="de">Katalog als PDF downloaden</Value>
	</LocaleResource>

	<LocaleResource Name="Mobile.DetailImages.Next">
		<Value>Next</Value>
		<Value lang="de">Vor</Value>
	</LocaleResource>
	<LocaleResource Name="Mobile.DetailImages.Prev">
		<Value>Previous</Value>
		<Value lang="de">Zurück</Value>
	</LocaleResource>
	
	<LocaleResource Name="Admin.Configuration.Settings.Catalog.ShowVariantCombinationPriceAdjustment">
		<Value>Show variant combination price adjustments</Value>
		<Value lang="de">Mehr- und Minderpreise bei Variant-Kombinationen anzeigen</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Configuration.Settings.Catalog.ShowVariantCombinationPriceAdjustment.Hint">
		<Value>Determines whether variant combination price adjustments should be displayed.</Value>
		<Value lang="de">Bestimmt ob Mehr- und Minderpreise bei Variant-Kombinationen angezeigt werden.</Value>
	</LocaleResource>
	
	<LocaleResource Name="Admin.Configuration.Settings.ShoppingCart.ShowBasePrice">
		<Value>Display base price</Value>
		<Value lang="de">Grundpreis anzeigen</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Configuration.Settings.ShoppingCart.ShowBasePrice.Hint">
		<Value>Determines whether base price should be displayed in the shopping cart.</Value>
		<Value lang="de">Bestimmt ob der Grundpreis im Warenkorb angezeigt werden soll.</Value>
	</LocaleResource>

	<LocaleResource Name="Admin.Configuration.Currencies.Fields.CurrencyCode.Hint">
		<Value>The three letter ISO 4217 currency code.</Value>
		<Value lang="de">Der aus drei Buchstaben bestehende ISO 4217 Währungscode.</Value>
	</LocaleResource>
	
	<LocaleResource Name="Enums.SmartStore.Core.Domain.Catalog.ProductVariantAttributeValueType.Simple">
		<Value>Simple</Value>
		<Value lang="de">Einfach</Value>
	</LocaleResource>
	<LocaleResource Name="Enums.SmartStore.Core.Domain.Catalog.ProductVariantAttributeValueType.ProductLinkage">
		<Value>Product</Value>
		<Value lang="de">Produkt</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.ValueTypeId">
		<Value>Type</Value>
		<Value lang="de">Typ</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.ValueTypeId.Hint">
		<Value>The type of the attribute value.</Value>
		<Value lang="de">Der Typ des Attributwertes.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.LinkedProduct">
		<Value>Linked product</Value>
		<Value lang="de">Verknüpftes Produkt</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.LinkedProduct.Hint">
		<Value>The product with which this attribute value is linked.</Value>
		<Value lang="de">Das Produkt mit dem dieser Attributwert verknüpft ist.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.LinkedProduct.AddNew">
		<Value>Add product linkage</Value>
		<Value lang="de">Produktverknüpfung hinzufügen</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.LinkProduct">
		<Value>Link</Value>
		<Value lang="de">Verknüpfen</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.UnlinkProduct">
		<Value>Remove link</Value>
		<Value lang="de">Verknüpfung aufheben</Value>
	</LocaleResource>
	<LocaleResource Name="ShoppingCart.ProductLinkageAttributeWarning">
		<Value>{0}. {1}. {2}</Value>
		<Value lang="de">{0}. {1}. {2}</Value>
	</LocaleResource>
	<LocaleResource Name="ShoppingCart.ProductLinkageProductNotLoading">
		<Value>The linked product with the ID {0} cannot be loaded.</Value>
		<Value lang="de">Das verknüpfte Product mit der ID kann nicht geladen werden.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.BundleItems.NoAttributeWithProductLinkage">
		<Value>Products with attribute values of type "product" cannot be part of a bundle.</Value>
		<Value lang="de">Produkte, die Attributwerte vom Typ "Produkt" haben, können nicht Bestandteil eines Bundles sein.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.Quantity">
		<Value>Quantity</Value>
		<Value lang="de">Menge</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.Quantity.Hint">
		<Value>The quantity of the linked product.</Value>
		<Value lang="de">Die Menge des verknüpften Produktes.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.Quantity.GreaterOrEqualToOne">
		<Value>The quantity value must be greater or equal to 1.</Value>
		<Value lang="de">Der Mengenwert muss größer oder gleich 1 sein.</Value>
	</LocaleResource>
	
	<LocaleResource Name="Admin.Orders.List.CustomerName">
		<Value>Customer name</Value>
		<Value lang="de">Kundenname</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Orders.List.CustomerName.Hint">
		<Value>Filter order list by customer name.</Value>
		<Value lang="de">Auftragsliste nach dem Kundennamen filtern.</Value>
	</LocaleResource>
	
	<LocaleResource Name="Admin.Configuration.Settings.ShoppingCart.ShowLinkedAttributeValueQuantity">
		<Value>Show quantity of linked product</Value>
		<Value lang="de">Menge verknüpfter Produkte anzeigen</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Configuration.Settings.ShoppingCart.ShowLinkedAttributeValueQuantity.Hint">
		<Value>Determine whether the quantity of linked products to appear at a variant attribute value.</Value>
		<Value lang="de">Bestimmt, ob bei Werten von Variantattributen die Menge von verknüpften Produkten angezeigt werden soll.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Configuration.Settings.Catalog.ShowLinkedAttributeValueQuantity">
		<Value>Show quantity of linked product</Value>
		<Value lang="de">Menge verknüpfter Produkte anzeigen</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Configuration.Settings.Catalog.ShowLinkedAttributeValueQuantity.Hint">
		<Value>Determine whether the quantity of linked products to appear at a variant attribute value.</Value>
		<Value lang="de">Bestimmt, ob bei Werten von Variantattributen die Menge von verknüpften Produkten angezeigt werden soll.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Configuration.Settings.Catalog.ShowLinkedAttributeValueImage">
		<Value>Show image of linked product</Value>
		<Value lang="de">Bild zu verknüpften Produkten anzeigen</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Configuration.Settings.Catalog.ShowLinkedAttributeValueImage.Hint">
		<Value>Determine whether the image of linked products to appear at a variant attribute value.</Value>
		<Value lang="de">Bestimmt, ob bei Werten von Variantattributen das Bild von verknüpften Produkten angezeigt werden soll.</Value>
	</LocaleResource>

	<LocaleResource Name="Admin.Catalog.Products.List.SearchStore">
		<Value>Store</Value>
		<Value lang="de">Shop</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.List.SearchStore.Hint">
		<Value>Search by a specific store.</Value>
		<Value lang="de">Nach bestimmten Shop suchen.</Value>
	</LocaleResource>

</Language>
'

DECLARE @forceResUpdate bit
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
