--upgrade scripts for smartstore.net (only specific parts)

--new locale resources
DECLARE @resources xml
--a resource will be deleted if its value is empty   
SET @resources='
<Language>
	
	<!-- 1. CORE -->
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

	<!-- 2. PRODUCT STRUCTURE -->
	<LocaleResource Name="Plugins.FriendlyName.DiscountRequirement.HasAllProducts">
		<Value>Customer has all of these products in the cart</Value>
		<Value lang="de">Der Kunde hat folgende Produkte in seinem Warenkorb</Value>
	</LocaleResource>
	<LocaleResource Name="Plugins.DiscountRules.HasAllProducts.Fields.Products">
		<Value>Restricted products</Value>
		<Value lang="de">Produkte</Value>
	</LocaleResource>
	<LocaleResource Name="Plugins.DiscountRules.HasAllProducts.Fields.Products.Hint">
		<Value>The comma-separated list of product identifiers (e.g. 77, 123, 156). You can find a product ID on its detail page. You can also specify the comma-separated list of product identifiers with quantities ({Product ID}:{Quantity}. for example, 77:1, 123:2, 156:3). And you can also specify the comma-separated list of product identifiers with quantity range ({Product ID}:{Min quantity}-{Max quantity}. for example, 77:1-3, 123:2-5, 156:3-8).</Value>
		<Value lang="de">Kommagetrennte Liste von Produkt-IDs (z.B. 77, 123, 156). Die ID eines Produktes findet sich in seiner Detailansicht. Man kann die Liste außerdem um Mengenangaben ({Produkt-ID}:{Menge} z.B.: 77:1, 123:2, 156:3) oder um einen Mengenbereich ergänzen ({Produkt-ID}:{Minimale Menge}-{Maximale Menge} z.B.: 77:1-3, 123:2-5, 156:3-8).</Value>
	</LocaleResource>
	<LocaleResource Name="Plugins.DiscountRules.HasAllProducts.Fields.ProductVariants">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Plugins.DiscountRules.HasAllProducts.Fields.ProductVariants.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	
	<LocaleResource Name="Plugins.FriendlyName.DiscountRequirement.HasOneProduct">
		<Value>Customer has one of these products in the cart</Value>
		<Value lang="de">Der Kunde hat einen der folgenden Produkte in seinem Warenkorb</Value>
	</LocaleResource>
	<LocaleResource Name="Plugins.DiscountRules.HasOneProduct.Fields.Products">
		<Value>Restricted products</Value>
		<Value lang="de">Produkte</Value>
	</LocaleResource>
	<LocaleResource Name="Plugins.DiscountRules.HasOneProduct.Fields.Products.Hint">
		<Value>The comma-separated list of product identifiers (e.g. 77, 123, 156). You can find a product ID on its detail page. You can also specify the comma-separated list of product identifiers with quantities ({Product ID}:{Quantity}. for example, 77:1, 123:2, 156:3). And you can also specify the comma-separated list of product identifiers with quantity range ({Product ID}:{Min quantity}-{Max quantity}. for example, 77:1-3, 123:2-5, 156:3-8).</Value>
		<Value lang="de">Kommagetrennte Liste von Produkt-IDs (z.B. 77, 123, 156). Die ID eines Produktes findet sich in seiner Detailansicht. Man kann die Liste außerdem um Mengenangaben ({Produkt-ID}:{Menge} z.B.: 77:1, 123:2, 156:3) oder um einen Mengenbereich ergänzen ({Produkt-ID}:{Minimale Menge}-{Maximale Menge} z.B.: 77:1-3, 123:2-5, 156:3-8).</Value>
	</LocaleResource>
	<LocaleResource Name="Plugins.DiscountRules.HasOneProduct.Fields.ProductVariants">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Plugins.DiscountRules.HasOneProduct.Fields.ProductVariants.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	
	<LocaleResource Name="Plugins.FriendlyName.DiscountRequirement.PurchasedAllProducts">
		<Value>Customer had previously purchased all of these products</Value>
		<Value lang="de">Der Kunde hat bereits folgende Produkte gekauft</Value>
	</LocaleResource>
	<LocaleResource Name="Plugins.DiscountRules.PurchasedAllProducts.Fields.Products">
		<Value>Restricted products</Value>
		<Value lang="de">Produkte</Value>
	</LocaleResource>
	<LocaleResource Name="Plugins.DiscountRules.PurchasedAllProducts.Fields.Products.Hint">
		<Value>The comma-separated list of product identifiers (e.g. 77, 123, 156). You can find a product ID on its detail page.</Value>
		<Value lang="de">Kommagetrennte Liste von Produkt-IDs (z.B. 77, 123, 156). Die ID eines Produktes findet sich in seiner Detailansicht.</Value>
	</LocaleResource>
	<LocaleResource Name="Plugins.DiscountRules.PurchasedAllProducts.Fields.ProductVariants">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Plugins.DiscountRules.PurchasedAllProducts.Fields.ProductVariants.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	
	<LocaleResource Name="Plugins.FriendlyName.DiscountRequirement.PurchasedOneProduct">
		<Value>Customer had previously purchased one of these products</Value>
		<Value lang="de">Der Kunde hat bereits eines der folgenden Produkte gekauft</Value>
	</LocaleResource>
	<LocaleResource Name="Plugins.DiscountRules.PurchasedOneProduct.Fields.Products">
		<Value>Restricted products</Value>
		<Value lang="de">Produkte</Value>
	</LocaleResource>
	<LocaleResource Name="Plugins.DiscountRules.PurchasedOneProduct.Fields.Products.Hint">
		<Value>The comma-separated list of product identifiers (e.g. 77, 123, 156). You can find a product ID on its detail page.</Value>
		<Value lang="de">Kommagetrennte Liste von Produkt-IDs (z.B. 77, 123, 156). Die ID eines Produktes findet sich in seiner Detailansicht.</Value>
	</LocaleResource>
	<LocaleResource Name="Plugins.DiscountRules.PurchasedOneProduct.Fields.ProductVariants">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Plugins.DiscountRules.PurchasedOneProduct.Fields.ProductVariants.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>

	<LocaleResource Name="ShoppingCart.CannotLoadProductVariant">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="ActivityLog.AddNewProductVariant">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="ActivityLog.DeleteProductVariant">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="ActivityLog.EditProductVariant">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>

	<LocaleResource Name="Admin.Catalog.Products.Variants">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Added">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.AddNew">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.AddNewForProduct">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.BackToProduct">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Deleted">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Discounts">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Discounts">
		<Value>Discounts</Value>
		<Value lang="de">Rabatte</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Discounts.NoDiscounts">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.EditProductVariantDetails">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	
	<LocaleResource Name="Admin.Catalog.Products.Discounts.NoDiscounts">
		<Value>No discounts available. Create at least one discount before mapping.</Value>
		<Value lang="de">Keine Rabatte verfügbar. Erstellen Sie zunächst mindestens einen Rabatt, bevor Sie eine Zuordung vornehmen.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.AdditionalShippingCharge">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.AdditionalShippingCharge">
		<Value>Additional shipping charge</Value>
		<Value lang="de">Transportzuschlag</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.AdditionalShippingCharge.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.AdditionalShippingCharge.Hint">
		<Value>The additional shipping charge.</Value>
		<Value lang="de">Zusätzliche Versandkosten für dieses Produkt.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.AdminComment">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.AdminComment.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.AllowBackInStockSubscriptions">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.AllowBackInStockSubscriptions.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.AllowBackInStockSubscriptions">
		<Value>Allow back in stock subscriptions</Value>
		<Value lang="de">Benachrichtigung bei Lieferbarkeit ermöglichen</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.AllowBackInStockSubscriptions.Hint">
		<Value>Allow customers to subscribe to a notification list for a product that has gone out of stock.</Value>
		<Value lang="de">Legt fest, ob Kunden News abonnieren können, die die Lieferbarkeit von zuvor vergriffenen Produkten melden.</Value>
	</LocaleResource>	
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.AllowedQuantities">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.AllowedQuantities.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.AllowedQuantities">
		<Value>Allowed quantities</Value>
		<Value lang="de">Zulässige Mengen</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.AllowedQuantities.Hint">
		<Value>Enter a comma separated list of quantities you want this product to be restricted to. Instead of a quantity textbox that allows them to enter any quantity, they will receive a dropdown list of the values you enter here.</Value>
		<Value lang="de">Eine kommagetrennte Liste der zulässigen Bestellmengen, die für dieses Produkt gelten. Kunden wählen i.d.F. eine Bestellmenge aus einem Dropdown-Menü aus, anstatt eine freie Eingabe zu tätigen.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.AutomaticallyAddRequiredProductVariants">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.AutomaticallyAddRequiredProductVariants.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.AutomaticallyAddRequiredProducts">
		<Value>Automatically add these products to the cart</Value>
		<Value lang="de">Diese Produkte automatisch dem Warenkorb hinzufügen</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.AutomaticallyAddRequiredProducts.Hint">
		<Value>Check to automatically add these products to the cart.</Value>
		<Value lang="de">Markieren, um diese Produkte automatisch dem Warenkorb hinzuzufügen.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.AvailableEndDateTime">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.AvailableEndDateTime.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.AvailableEndDateTime">
		<Value>Available end date</Value>
		<Value lang="de">Verfügbar bis</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.AvailableEndDateTime.Hint">
		<Value>The end of the product&apos;s availability in Coordinated Universal Time (UTC).</Value>
		<Value lang="de">Das Produkt ist verfügbar bis (UTC-Datum).</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.AvailableForPreOrder">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.AvailableForPreOrder.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.AvailableForPreOrder">
		<Value>Available for pre-order</Value>
		<Value lang="de">Vorbestellung möglich</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.AvailableForPreOrder.Hint">
		<Value>Check if this item is available for Pre-Order. It also displays "Pre-order" button instead of "Add to cart".</Value>
		<Value lang="de">Markieren, falls Vorbestellungen möglich sein sollen ("Vorbestellen-" statt "Warenkorb"-Button wird angezeigt).</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.AvailableStartDateTime">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.AvailableStartDateTime.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.AvailableStartDateTime">
		<Value>Available start date</Value>
		<Value lang="de">Verfügbar ab</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.AvailableStartDateTime.Hint">
		<Value>The start of the product&apos;s availability in Coordinated Universal Time (UTC).</Value>
		<Value lang="de">Das Produkt ist verfügbar ab (UTC-Datum).</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.BackorderMode">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.BackorderMode.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.BackorderMode">
		<Value>Backorders</Value>
		<Value lang="de">Lieferrückstand</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.BackorderMode.Hint">
		<Value>Select backorder mode.</Value>
		<Value lang="de">Legt den Modus für Lieferrückstände fest.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.BasePriceAmount">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.BasePriceAmount.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.BasePriceAmount.Required">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.BasePriceAmount">
		<Value>Amount</Value>
		<Value lang="de">Menge</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.BasePriceAmount.Hint">
		<Value>Actual amount of the product per packing unit in the specified measure unit (e.g. 250 ml shower gel match "0.25", if {measure unit}=liter and {base unit}=1).</Value>
		<Value lang="de">Tatsächliche Menge des Produktes pro Verpackungseinheit in der angegebenen Maßeinheit (z.B. 250 ml Duschgel entspricht "0,25", wenn {Maßeinheit}=Liter und {Grundeinheit}=1).</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.BasePriceAmount.Required">
		<Value>"Amount" is required to calculate the base price.</Value>
		<Value lang="de">"Menge" ist erforderlich, wenn der Grundpreis berechnet werden soll.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.BasePriceBaseAmount">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.BasePriceBaseAmount.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.BasePriceBaseAmount.Required">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.BasePriceBaseAmount">
		<Value>Basic unit</Value>
		<Value lang="de">Grundeinheit</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.BasePriceBaseAmount.Hint">
		<Value>The reference value for base price (e.g. "1 liter"). Formula: {base unit} {measure unit} = {selling price} / {amount}.</Value>
		<Value lang="de">Der Bezugswert für den Grundpreis (z.B. "1 Liter"). Formel: {Grundeinheit} {Maßeinheit} = {Verkaufspreis} / {Menge}.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.BasePriceBaseAmount.Required">
		<Value>"Basic unit" is required to calculate the base price.</Value>
		<Value lang="de">"Grundeinheit" ist erforderlich, wenn der Grundpreis berechnet werden soll.</Value>
	</LocaleResource>	
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.BasePriceEnabled">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.BasePriceEnabled.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.BasePriceEnabled">
		<Value>Calculate base price according to Price Indication Regulation [PAngV]</Value>
		<Value lang="de">Grundpreis gemäß PAnGV berechnen</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.BasePriceEnabled.Hint">
		<Value>The base price will be calculated on the legal basic unit for the goods and the specified amount of a packaging unit. In Germany this indication is obligatory for certain product types (B2C).</Value>
		<Value lang="de">Der Grundpreis wird auf Basis der gesetzlichen Grundeinheit für die Ware und der angegebenen Menge einer Verpackungseinheit berechnet. Die Angabe ist in Deutschland für bestimmte Produkttypen zwingend erforderlich (B2C).</Value>
	</LocaleResource>		
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.BasePriceInfo">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.BasePriceInfo">
		<Value>{0} per unit (base price: {1} per {2})</Value>
		<Value lang="de">{0} pro Einheit (Grundpreis: {1} pro {2})</Value>
	</LocaleResource>	
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.BasePriceMeasureUnit">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.BasePriceMeasureUnit.Required">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.BasePriceMeasureUnit">
		<Value>"Measure unit" is required to calculate the base price.</Value>
		<Value lang="de">"Maßeinheit" ist erforderlich, wenn der Grundpreis berechnet werden soll.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.BasePriceMeasureUnit.Required">
		<Value>"Measure unit" is required to calculate the base price.</Value>
		<Value lang="de">"Maßeinheit" ist erforderlich, wenn der Grundpreis berechnet werden soll.</Value>
	</LocaleResource>	
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.CallForPrice">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.CallForPrice.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.CallForPrice">
		<Value>Call for price</Value>
		<Value lang="de">Preis auf Anfrage</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.CallForPrice.Hint">
		<Value>Check to show "Call for Pricing" or "Call for quote" instead of price.</Value>
		<Value lang="de">Legt fest, ob "Preis auf Anfrage" anstelle des Preises angezeigt werden soll.</Value>
	</LocaleResource>	
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.CustomerEntersPrice">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.CustomerEntersPrice.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.CustomerEntersPrice">
		<Value>Customer enters price</Value>
		<Value lang="de">Preisvorschlag ermöglichen</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.CustomerEntersPrice.Hint">
		<Value>An option indicating whether customer should enter price.</Value>
		<Value lang="de">Legt fest, ob der Kunde einen Preisvorschlag abgeben kann.</Value>		
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.DeliveryTime">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.DeliveryTime.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.DeliveryTime">
		<Value>Delivery time</Value>
		<Value lang="de">Lieferzeit</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.DeliveryTime.Hint">
		<Value>The amount of time it takes to prepare the order for shipping.</Value>
		<Value lang="de">Die Zeitspanne, die es braucht, um die Bestellung für den Versand vorzubereiten.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.Description">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.Description.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.DisableBuyButton">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.DisableBuyButton.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.DisableBuyButton">
		<Value>Disable buy button</Value>
		<Value lang="de">"Kaufen"-Button verbergen</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.DisableBuyButton.Hint">
		<Value>Check to disable the buy button for this product. This may be necessary for products that are "available upon request".</Value>
		<Value lang="de">Legt fest, ob der Kaufen-Button ausgeblendet werden soll (erforderlich bspw. dann, wenn Produkte nur auf Anfrage erhältlich sind).</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.DisableWishlistButton">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.DisableWishlistButton.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.DisableWishlistButton">
		<Value>Disable wishlist button</Value>
		<Value lang="de">Wunschliste deaktivieren</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.DisableWishlistButton.Hint">
		<Value>Check to disable the wishlist button for this product.</Value>
		<Value lang="de">Legt fest, ob der Wunschlisten-Button für dieses Produkt deaktiviert werden soll.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.DisplayOrder">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.DisplayOrder.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>	
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.DisplayStockAvailability">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.DisplayStockAvailability.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.DisplayStockAvailability">
		<Value>Display stock availability</Value>
		<Value lang="de">Verfügbarkeit anzeigen</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.DisplayStockAvailability.Hint">
		<Value>Check to display stock availability. When enabled, customers will see stock availability.</Value>
		<Value lang="de">Legt fest, ob Kunden die Waren-Verfügbarkeit einsehen können.</Value>
	</LocaleResource>		
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.DisplayStockQuantity">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.DisplayStockQuantity.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.DisplayStockQuantity">
		<Value>Display stock quantity</Value>
		<Value lang="de">Warenbestand anzeigen</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.DisplayStockQuantity.Hint">
		<Value>Check to display stock quantity. When enabled, customers will see stock quantity.</Value>
		<Value lang="de">Legt fest, ob Kunden den Warenbestand einsehen können.</Value>
	</LocaleResource>	
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.Download">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.Download.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.Download">
		<Value>Download file</Value>
		<Value lang="de">Datei für den Download</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.Download.Hint">
		<Value>The download file.</Value>
		<Value lang="de">Legt die Download-Datei fest.</Value>
	</LocaleResource>		
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.DownloadActivationType">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.DownloadActivationType.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.DownloadActivationType">
		<Value>Download activation type</Value>
		<Value lang="de">Aktivierungs-Typ</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.DownloadActivationType.Hint">
		<Value>A value indicating when download links will be enabled.</Value>
		<Value lang="de">Bestimmt, wann der Kunde den Download durchführen kann.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.DownloadExpirationDays">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.DownloadExpirationDays.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.DownloadExpirationDays">
		<Value>Number of days</Value>
		<Value lang="de">Aktivierungsdauer</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.DownloadExpirationDays.Hint">
		<Value>The number of days during customers keeps access to the file (e.g. 14). Leave this field blank to allow continuous downloads.</Value>
		<Value lang="de">Anzahl der Tage, die der Kunde die Datei herunterladen kann. Lassen Sie das Feld leer, falls der Link unbegrenzt lange aktiv bleiben soll.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.GiftCardType">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.GiftCardType.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.GiftCardType">
		<Value>Gift card type</Value>
		<Value lang="de">Gutschein-Typ</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.GiftCardType.Hint">
		<Value>Select gift card type. WARNING: not recommended to change in production environment.</Value>
		<Value lang="de">Der Typ des Gutscheins. WARNUNG: Eine Wert-Änderung während des Echtbetriebes ist nicht empfohlen.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.GTIN">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.GTIN.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.GTIN">
		<Value>GTIN (global trade item number)</Value>
		<Value lang="de">EAN</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.GTIN.Hint">
		<Value>Enter global trade item number (GTIN). These identifiers include UPC (in North America), EAN (in Europe), JAN (in Japan), and ISBN (for books).</Value>
		<Value lang="de">EAN (Europa), GTIN (global trade item number), UPC (USA), JAN (Japan) oder ISBN (Bücher).</Value>
	</LocaleResource>		
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.HasSampleDownload">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.HasSampleDownload.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.HasSampleDownload">
		<Value>Has sample download file</Value>
		<Value lang="de">Hat Probedownload</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.HasSampleDownload.Hint">
		<Value>Check if this product has a sample download file that can be downloaded before checkout.</Value>
		<Value lang="de">Legt fest, ob der Kunde eine Beispieldatei vor dem Checkout runterladen kann.</Value>
	</LocaleResource>		
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.HasUserAgreement">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.HasUserAgreement.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.HasUserAgreement">
		<Value>Has user agreement</Value>
		<Value lang="de">Hat Benutzervereinbarung</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.HasUserAgreement.Hint">
		<Value>Check if the product has a user agreement.</Value>
		<Value lang="de">Legt fest, ob das Produkt eine Benutzervereinbarung hat.</Value>
	</LocaleResource>		
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.Height">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.Height.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.Height">
		<Value>Height</Value>
		<Value lang="de">Höhe</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.Height.Hint">
		<Value>The height of the product.</Value>
		<Value lang="de">Die Höhe des Produktes.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.ID">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.ID.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.ID">
		<Value>ID</Value>
		<Value lang="de">ID</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.ID.Hint">
		<Value>The product identifier.</Value>
		<Value lang="de">ID des Produktes.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.IsDownload">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.IsDownload.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.IsDownload">
		<Value>Downloadable product</Value>
		<Value lang="de">Ist Download (ESD)</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.IsDownload.Hint">
		<Value>Check if this product variant is a downloadable product. When a customer purchases a download product, they can download the item direct from your store by viewing their completed order.</Value>
		<Value lang="de">Legt fest, ob das Produkt ein downloadbares, digitales Produkt ist. Ein Kunde, der ein digitales Produkt kauft, kann den Download in seinem Account-Bereich im Shop durchführen.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.IsFreeShipping">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.IsFreeShipping.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.IsFreeShipping">
		<Value>Free shipping</Value>
		<Value lang="de">Versandkostenfrei</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.IsFreeShipping.Hint">
		<Value>Check if this product comes with FREE shipping.</Value>
		<Value lang="de">Legt fest, ob dieses Produkt versondkostenfrei geliefert wird.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.IsGiftCard">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.IsGiftCard.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.IsGiftCard">
		<Value>Is gift card</Value>
		<Value lang="de">Ist Geschenkgutschein</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.IsGiftCard.Hint">
		<Value>Check if it is a gift card.</Value>
		<Value lang="de">Legt fest, ob dies ein Geschenkgutschein ist.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.IsRecurring">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.IsRecurring.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.IsRecurring">
		<Value>Recurring product</Value>
		<Value lang="de">Ist Abo</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.IsRecurring.Hint">
		<Value>Check if this product is a recurring product.</Value>
		<Value lang="de">Legt fest, ob das Produkt wiederkehrend ist.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.IsShipEnabled">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.IsShipEnabled.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.IsShipEnabled">
		<Value>Shipping enabled</Value>
		<Value lang="de">Versand möglich</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.IsShipEnabled.Hint">
		<Value>Determines whether the product can be shipped.</Value>
		<Value lang="de">Legt fest, ob der Versand bei diesem Produkt möglich ist.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.IsTaxExempt">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.IsTaxExempt.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.IsTaxExempt">
		<Value>Tax exempt</Value>
		<Value lang="de">MwSt-frei</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.IsTaxExempt.Hint">
		<Value>Determines whether this product is tax exempt (tax will not be applied to this product at checkout).</Value>
		<Value lang="de">Legt fest, ob das Produkt mehrwertsteuerfrei ist (MwSt wird für dieses Produkt während des Bestellvorganges nicht berechnet).</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.Length">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.Length.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.Length">
		<Value>Length</Value>
		<Value lang="de">Länge</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.Length.Hint">
		<Value>The length of the product.</Value>
		<Value lang="de">Die Länge des Produktes.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.LowStockActivity">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.LowStockActivity.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.LowStockActivity">
		<Value>Low stock activity</Value>
		<Value lang="de">Aktion bei Erreichen des Meldebestandes</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.LowStockActivity.Hint">
		<Value>Action to be taken when your current stock quantity falls below the "Minimum stock quantity".</Value>
		<Value lang="de">Zu ergreifende Maßnahme, wenn die Bestandsmenge unter den Mindestbestand fällt.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.ManageInventoryMethod">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.ManageInventoryMethod.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.ManageInventoryMethod">
		<Value>Manage inventory method</Value>
		<Value lang="de">Lagerbestandsführung</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.ManageInventoryMethod.Hint">
		<Value>Select manage inventory method. When enabled, stock quantities are automatically adjusted when a customer makes a purchase. You can also set low stock activity actions and receive notifications.</Value>
		<Value lang="de">Legt die Lagerbestandsführungs-Methode fest. Wenn aktiviert, wird der Lagerbestand bei jeder Bestellung automatisch angepasst. Führt bei niedrigem Lagerbestand eine Aktion aus und/oder benachrichtigt den Betreiber.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.ManufacturerPartNumber">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.ManufacturerPartNumber.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.ManufacturerPartNumber">
		<Value>Manufacturer part number</Value>
		<Value lang="de">Hersteller-Produktnummer</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.ManufacturerPartNumber.Hint">
		<Value>The manufacturer&apos;s part number for this product variant.</Value>
		<Value lang="de">Produktnummer des Herstellers.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.MaximumCustomerEnteredPrice">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.MaximumCustomerEnteredPrice.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.MaximumCustomerEnteredPrice">
		<Value>Maximum amount</Value>
		<Value lang="de">Höchstbetrag</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.MaximumCustomerEnteredPrice.Hint">
		<Value>Enter a maximum amount.</Value>
		<Value lang="de">Legt den Höchstbetrag fest, den der Kunde eingeben kann.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.MaxNumberOfDownloads">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.MaxNumberOfDownloads.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.MaxNumberOfDownloads">
		<Value>Max. downloads</Value>
		<Value lang="de">Max. Anzahl Downloads</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.MaxNumberOfDownloads.Hint">
		<Value>The maximum number of downloads.</Value>
		<Value lang="de">Legt die maximale Anzahl der Downloads fest.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.MinimumCustomerEnteredPrice">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.MinimumCustomerEnteredPrice.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.MinimumCustomerEnteredPrice">
		<Value>Minimum amount</Value>
		<Value lang="de">Mindestbetrag</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.MinimumCustomerEnteredPrice.Hint">
		<Value>Enter a minimum amount.</Value>
		<Value lang="de">Legt den Mindestbetrag fest.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.MinStockQuantity">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.MinStockQuantity.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.MinStockQuantity">
		<Value>Minimum stock quantity</Value>
		<Value lang="de">Mindestlagerbestand</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.MinStockQuantity.Hint">
		<Value>If you have enabled "Manage Stock" you can perform a number of different actions when the current stock quantity falls below your minimum stock quantity.</Value>
		<Value lang="de">Legt den Mindestlagerbestand fest. Fällt der Bestand unter diesen Wert, so können bei aktivierter Lagerbestandsverwaltung verschiedene Aktionen ausgeführt werden (z.B. Benachrichtigung oder Deaktivierung des Produktes).</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.MUAmount">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.MUAmount.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.MUBase">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.MUBase.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.Name">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.Name.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.NotifyAdminForQuantityBelow">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.NotifyAdminForQuantityBelow.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.NotifyAdminForQuantityBelow">
		<Value>Notify admin for quantity below</Value>
		<Value lang="de">Benachrichtigt den Administrator, wenn die Mindestmenge unterschritten wird.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.NotifyAdminForQuantityBelow.Hint">
		<Value>When the current stock quantity falls below this quantity, the storekeeper (admin) will receive a notification.</Value>
		<Value lang="de">Benachrichtigt den Administrator, wenn die Mindestmenge unterschritten wird.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.OldPrice">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.OldPrice.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.OldPrice">
		<Value>Old price</Value>
		<Value lang="de">Alter Preis</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.OldPrice.Hint">
		<Value>The old price of the product. If you set an old price, this will display alongside the current price on the product page to show the difference in price.</Value>
		<Value lang="de">Legt den alten Preis fest. Um den Unterschied zum aktuellen Preis zu verdeutlichen, wird der alte Preis neben dem aktuellen Preis des Produktes dargestellt.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.OrderMaximumQuantity">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.OrderMaximumQuantity.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.OrderMaximumQuantity">
		<Value>Maximum cart quantity</Value>
		<Value lang="de">Maximale Bestellmenge</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.OrderMaximumQuantity.Hint">
		<Value>Set the maximum quantity allowed in a customer&apos;s shopping cart e.g. set to 5 to only allow customers to purchase 5 of this product.</Value>
		<Value lang="de">Legt die maximale Bestellmenge fest.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.OrderMinimumQuantity">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.OrderMinimumQuantity.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.OrderMinimumQuantity">
		<Value>Minimum cart quantity</Value>
		<Value lang="de">Minimale Bestellmenge</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.OrderMinimumQuantity.Hint">
		<Value>Set the minimum quantity allowed in a customer&apos;s shopping cart e.g. set to 3 to only allow customers to purchase 3 or more of this product.</Value>
		<Value lang="de">Legt die minimale Bestellmenge fest.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.Picture">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.Picture.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.Price">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.Price.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.Price">
		<Value>Price</Value>
		<Value lang="de">Preis</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.Price.Hint">
		<Value>The price of the product.</Value>
		<Value lang="de">Legt den Preis für das Produkt fest.</Value>
	</LocaleResource>	
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.ProductCost">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.ProductCost.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.ProductCost">
		<Value>Product cost</Value>
		<Value lang="de">Einkaufspreis</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.ProductCost.Hint">
		<Value>The product cost is the cost of all the different components which make up the product. This may either be the purchase price if the components are bought from outside suppliers, or the combined cost of materials and manufacturing processes if the component is made in-house.</Value>
		<Value lang="de">Legt den Einkaufspreis fest.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.ProductName">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.ProductName.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.ProductName.Required">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.Published">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.Published.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.RecurringCycleLength">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.RecurringCycleLength.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.RecurringCycleLength">
		<Value>Cycle length</Value>
		<Value lang="de">Erneuerungszeitraum</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.RecurringCycleLength.Hint">
		<Value>Enter cycle length.</Value>
		<Value lang="de">Legt den Zeitraum fest, nach dem das Abonnement erneuert wird.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.RecurringCyclePeriod">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.RecurringCyclePeriod.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.RecurringCyclePeriod">
		<Value>Cycle period</Value>
		<Value lang="de">Erneuerungsperiode</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.RecurringCyclePeriod.Hint">
		<Value>Select cycle period.</Value>
		<Value lang="de">Legt die Erneuerungsperiode in Tagen, Wochen, Monaten und Jahren fest.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.RecurringTotalCycles">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.RecurringTotalCycles.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.RecurringTotalCycles">
		<Value>Total cycles</Value>
		<Value lang="de">Anzahl Abonnement-Erneuerungen</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.RecurringTotalCycles.Hint">
		<Value>Enter total cycles.</Value>
		<Value lang="de">Legt max. die Anzahl der Abonnement-Erneuerungen fest.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.RequiredProductVariantIds">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.RequiredProductVariantIds.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.RequiredProductIds">
		<Value>Required product IDs</Value>
		<Value lang="de">IDs der erforderlichen Produkte</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.RequiredProductIds.Hint">
		<Value>Specify comma separated list of required product IDs. NOTE: Ensure that there are no circular references (for example, A requires B, and B requires A).</Value>
		<Value lang="de">Komma-getrennte Liste der IDs der erforderlichen Produkte. ACHTUNG: Es dürfen keine zirkulären Bezüge entstehen (z.B., A erfordert B, und B erfordert A).</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.RequireOtherProducts">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.RequireOtherProducts.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.RequireOtherProducts">
		<Value>Require other products are added to the cart</Value>
		<Value lang="de">Andere Produkte erforderlich</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.RequireOtherProducts.Hint">
		<Value>Check if this product requires that other products are added to the cart.</Value>
		<Value lang="de">Zu diesem Produkt müssen weitere Produkte mitbestellt werden.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.SampleDownload">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.SampleDownload.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.SampleDownload">
		<Value>Sample download file</Value>
		<Value lang="de">Beispiel-Download-Datei</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.SampleDownload.Hint">
		<Value>The sample download file.</Value>
		<Value lang="de">Legt eine Download-Datei als Beispiel fest.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.Sku">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.Sku.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.Sku">
		<Value>SKU</Value>
		<Value lang="de">Produktnummer (SKU)</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.Sku.Hint">
		<Value>Product stock keeping unit (SKU). Your internal unique identifier that can be used to track this product.</Value>
		<Value lang="de">Legt die Produktnummer (SKU) fest.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.SpecialPrice">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.SpecialPrice.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.SpecialPrice">
		<Value>Special price</Value>
		<Value lang="de">Aktionspreis</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.SpecialPrice.Hint">
		<Value>Set a special price for the product. New price will be valid between start and end dates. Leave empty to ignore field.</Value>
		<Value lang="de">Legt einen Aktionspreis für das Produkt fest. Der neue Preis ist zwischen Anfangs- und Enddatum gültig. Anfangs- und Enddatum freilassen, um keinen Zeitraum festzulegen</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.SpecialPriceEndDateTimeUtc">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.SpecialPriceEndDateTimeUtc.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.SpecialPriceEndDateTimeUtc">
		<Value>Special price end date</Value>
		<Value lang="de">Aktionspreis Enddatum</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.SpecialPriceEndDateTimeUtc.Hint">
		<Value>The end date of the special price in Coordinated Universal Time (UTC).</Value>
		<Value lang="de">Ende des Aktionspreises nach UTC.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.SpecialPriceStartDateTimeUtc">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.SpecialPriceStartDateTimeUtc.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.SpecialPriceStartDateTimeUtc">
		<Value>Special price start date</Value>
		<Value lang="de">Aktionspreis Anfangsdatum</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.SpecialPriceStartDateTimeUtc.Hint">
		<Value>The start date of the special price in Coordinated Universal Time (UTC).</Value>
		<Value lang="de">Angangszeit für den Aktionspreis nach UTC.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.StockQuantity">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.StockQuantity.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.StockQuantity">
		<Value>Stock quantity</Value>
		<Value lang="de">Lagerbestand</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.StockQuantity.Hint">
		<Value>The current stock quantity of this product.</Value>
		<Value lang="de">Legt den aktuellen Lagerbestand des Produktes fest.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.TaxCategory">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.TaxCategory.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.TaxCategory">
		<Value>Tax category</Value>
		<Value lang="de">Steuersatz</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.TaxCategory.Hint">
		<Value>The tax classification for this product. You can manage product tax classifications from "Configuration - Tax - Tax Classes".</Value>
		<Value lang="de">Legt den Steuersatz für dieses Produkt fest. Steuersätze werden unter "Verwaltung - Steuern - Steuersätze" festgelegt.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.UnlimitedDownloads">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.UnlimitedDownloads.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.UnlimitedDownloads">
		<Value>Unlimited downloads</Value>
		<Value lang="de">Unbegrenztes Downloaden</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.UnlimitedDownloads.Hint">
		<Value>When a customer purchases a download product, they can download the item unlimited number of times.</Value>
		<Value lang="de">Legt fest, ob ein Download-Produkt nach dem Kauf unbegrenzt oft heruntergeladen werden kann.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.UserAgreementText">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.UserAgreementText.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.UserAgreementText">
		<Value>User agreement text</Value>
		<Value lang="de">Text der Nutzungsvereinbarung</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.UserAgreementText.Hint">
		<Value>The text of the user agreement</Value>
		<Value lang="de">Legt den Text der Nutzungsvereinbarung fest.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.Weight">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.Weight.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.Weight">
		<Value>Weight</Value>
		<Value lang="de">Gewicht</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.Weight.Hint">
		<Value>The weight of the product. Can be used in shipping calculations.</Value>
		<Value lang="de">Legt das Gewicht des Produktes fest.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.Width">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Fields.Width.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.Width">
		<Value>Width</Value>
		<Value lang="de">Breite</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.Width.Hint">
		<Value>The width of the product.</Value>
		<Value lang="de">Legt die Breite des Produktes fest.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Info">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>	
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.AttributeCombinations">
		<Value>Attribute combinations</Value>
		<Value lang="de">Attribut-Kombinationen</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.AddNew">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.AttributeCombinations.AddNew">
		<Value>Add combination</Value>
		<Value lang="de">Hinzufügen</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.AddTitle">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.AttributeCombinations.AddTitle">
		<Value>Select new combination and enter details below</Value>
		<Value lang="de">Attribut-Kombination hinzufügen.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.AskToCombineAll">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.AttributeCombinations.AskToCombineAll">
		<Value>Would you like to combine all attributes? Existing combinations will be deleted!</Value>
		<Value lang="de">Möchten Sie sämtliche Attribute miteinander kombinieren? Eventuell vorhandene Kombinationen werden dabei gelöscht!</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.CombiExists">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.AttributeCombinations.CombiExists">
		<Value>The selected attribute combination already exists. Please choose a non existing combination.</Value>
		<Value lang="de">Die gewählte Attribut-Kombination existiert bereits. Bitte wählen Sie eine noch nicht existierende Kombination aus.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.CombiNotExists">
		<Value>The selected attribute combination does not exist yet.</Value>
		<Value lang="de">Die gewählte Attribut-Kombination existiert noch nicht.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.CreateAllCombinations">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.AttributeCombinations.CreateAllCombinations">
		<Value>Create all combinations</Value>
		<Value lang="de">Alle Kombinationen erstellen</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.Description">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.AttributeCombinations.Description">
		<Value>Attribute combinations are useful when your "Manage inventory method" is set to "Track inventory by product attributes".</Value>
		<Value lang="de">Attribut-Kombinationen ermöglichen die Erfassung abweichender Produkt-Eigenschaften auf Basis von spezifischen Kombinationen.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.EditTitle">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.AttributeCombinations.EditTitle">
		<Value>Edit attribute combination</Value>
		<Value lang="de">Attribut-Kombination bearbeiten</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.Fields.AllowOutOfStockOrders">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.Fields.AllowOutOfStockOrders.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.AttributeCombinations.Fields.AllowOutOfStockOrders">
		<Value>Allow out of stock</Value>
		<Value lang="de">Bestellung ohne Lagerbestand möglich</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.AttributeCombinations.Fields.AllowOutOfStockOrders.Hint">
		<Value>A value indicating whether to allow orders when out of stock.</Value>
		<Value lang="de">Legt fest, ob das Produkt auch bei einem Lagerbestand &lt;= 0 bestellt werden kann.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.Fields.Attributes">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.AttributeCombinations.Fields.Attributes">
		<Value>Attributes</Value>
		<Value lang="de">Attribute</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.Fields.Description">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.Fields.Description.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.Fields.Gtin">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.Fields.Gtin.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.Fields.Height">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.Fields.Height.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.Fields.IsActive">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.Fields.IsActive.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.Fields.IsDefaultCombination">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.Fields.IsDefaultCombination.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.Fields.IsDefaultVariant">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.Fields.IsDefaultVariant.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.Fields.Length">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.Fields.Length.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.Fields.ManufacturerPartNumber">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.Fields.ManufacturerPartNumber.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.Fields.MUAmount">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.Fields.MUAmount.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.Fields.MUBase">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.Fields.MUBase.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.Fields.Name">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.Fields.Name.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.Fields.PictureId">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.Fields.PictureId.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.Fields.Pictures">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.Fields.Pictures.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.AttributeCombinations.Fields.Pictures">
		<Value>Pictures</Value>
		<Value lang="de">Bilder</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.AttributeCombinations.Fields.Pictures.Hint">
		<Value>Check the images that shows this attribute combination</Value>
		<Value lang="de">Aktivieren Sie die Bilder, die diese Attribut-Kombination zeigen.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.Fields.Sku">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.Fields.Sku.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.Fields.StockQuantity">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.Fields.StockQuantity.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.AttributeCombinations.Fields.StockQuantity">
		<Value>Stock quantity</Value>
		<Value lang="de">Lagerbestand</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.AttributeCombinations.Fields.StockQuantity.Hint">
		<Value>The current stock quantity of this combination.</Value>
		<Value lang="de">Gibt den Lagerbestand an.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.Fields.Width">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.Fields.Width.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.Attributes">
		<Value>Attributes</Value>
		<Value lang="de">Attribute</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Fields">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Fields.Attribute">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.Attributes.Fields.Attribute">
		<Value>Attribute</Value>
		<Value lang="de">Attribut</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Fields.AttributeControlType">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.Attributes.Fields.AttributeControlType">
		<Value>Control type</Value>
		<Value lang="de">Darstellung</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Fields.DisplayOrder">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.Attributes.Fields.DisplayOrder">
		<Value>Display order</Value>
		<Value lang="de">Reihenfolge</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Fields.IsRequired">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.Attributes.Fields.IsRequired">
		<Value>Is Required</Value>
		<Value lang="de">Ist erforderlich</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Fields.TextPrompt">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.Attributes.Fields.TextPrompt">
		<Value>Text prompt</Value>
		<Value lang="de">Anzeigetext</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Values">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values">
		<Value>Values</Value>
		<Value lang="de">Werte</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Values.AddNew">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.AddNew">
		<Value>Add a new value</Value>
		<Value lang="de">Neuen Wert hinzufügen</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Values.BackToProductVariant">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.BackToProduct">
		<Value>Back to product details</Value>
		<Value lang="de">Zurück zu den Produkteigenschaften</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Values.EditAttributeDetails">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.EditAttributeDetails">
		<Value>Add/Edit values for [{0}] attribute. Product: {1}</Value>
		<Value lang="de">Hinzufügen/bearbeiten der Werte für [{0}] Attribute. Produkt: {1}.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Values.EditValueDetails">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.EditValueDetails">
		<Value>Edit value</Value>
		<Value lang="de">Wert bearbeiten</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Values.Fields">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Values.Fields.Alias">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Values.Fields.Alias.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.Alias">
		<Value>Alias</Value>
		<Value lang="de">Alias</Value>
	</LocaleResource>	
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.Alias.Hint">
		<Value>An optional, language-neutral reference name for internal use</Value>
		<Value lang="de">Ein optionaler, sprachneutraler Referenzwert für interne Zwecke</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Values.Fields.ColorSquaresRgb">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Values.Fields.ColorSquaresRgb.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.ColorSquaresRgb">
		<Value>RGB color</Value>
		<Value lang="de">RGB-Farbe</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.ColorSquaresRgb.Hint">
		<Value>Choose color to be used with the color squares attribute control.</Value>
		<Value lang="de">Wählen Sie eine Farbe die mit dem Farbflächen-Attribut-Steuerelement genutzt werden soll:</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Values.Fields.DisplayOrder">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Values.Fields.DisplayOrder.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.DisplayOrder">
		<Value>Display order</Value>
		<Value lang="de">Reihenfolge</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.DisplayOrder.Hint">
		<Value>The display order of the attribute value. 1 represents the first item in attribute value list.</Value>
		<Value lang="de">Legt die Reihenfolge fest, nach der die Attribute angezeigt werden.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Values.Fields.IsPreSelected">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Values.Fields.IsPreSelected.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.IsPreSelected">
		<Value>Is pre-selected</Value>
		<Value lang="de">Vorausgewählt</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.IsPreSelected.Hint">
		<Value>Determines whether this attribute value is pre selected for the customer</Value>
		<Value lang="de">Legt fest, ob dieses Attribut vorausgewählt ist.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Values.Fields.Name">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Values.Fields.Name.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Values.Fields.Name.Required">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.Name">
		<Value>Name</Value>
		<Value lang="de">Attribut-Wert</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.Name.Hint">
		<Value>The attribute value name e.g. "Blue" for Color attributes.</Value>
		<Value lang="de">Der Wert des Attributes, z.B. Bei Farben rot, grün oder blau.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.Name.Required">
		<Value>Please provide a name.</Value>
		<Value lang="de">Der Attribut-Wert ist erforderlich.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Values.Fields.PriceAdjustment">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Values.Fields.PriceAdjustment.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.PriceAdjustment">
		<Value>Price adjustment</Value>
		<Value lang="de">Mehr-/Minderpreis</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.PriceAdjustment.Hint">
		<Value>The price adjustment applied when choosing this attribute value e.g. "10" to add 10 dollars.</Value>
		<Value lang="de">Legt einen Aufpreis für dieses Attribut fest.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Values.Fields.WeightAdjustment">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Values.Fields.WeightAdjustment.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.WeightAdjustment">
		<Value>Weight adjustment</Value>
		<Value lang="de">Mehr-/Mindergewicht</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.WeightAdjustment.Hint">
		<Value>The weight adjustment applied when choosing this attribute value.</Value>
		<Value lang="de">Passt das Gewicht für dieses Attribut an.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Values.ViewLink">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.ViewLink">
		<Value>View/Edit value (Total: {0})</Value>
		<Value lang="de">(Anzahl: {0}) Attributwerte bearbeiten/ansehen</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.NoAttributesAvailable">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.NoAttributesAvailable">
		<Value>No product attributes available. Create at least one product attribute before mapping.</Value>
		<Value lang="de">Keine Attribute zur Erstellung einer Attributkombination verfügbar. Legen Sie mindestens ein Attribut an.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.SaveBeforeEdit">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.SaveBeforeEdit">
		<Value>You need to save the product variant before you can add attributes for this product variant page.</Value>
		<Value lang="de">Die Produktvariante muss zunächst gespeichert werden, um eine Zuordnung vornehmen zu können</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.TierPrices">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.TierPrices">
		<Value>Tier prices</Value>
		<Value lang="de">Staffelpreise</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.TierPrices.Fields">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.TierPrices.Fields.CustomerRole">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.TierPrices.Fields.CustomerRole">
		<Value>Customer role</Value>
		<Value lang="de">Kundengruppe</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.TierPrices.Fields.CustomerRole.AllRoles">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.TierPrices.Fields.CustomerRole.AllRoles">
		<Value>All customer roles</Value>
		<Value lang="de">Alle Kundengruppen</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.TierPrices.Fields.Price">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.TierPrices.Fields.Price">
		<Value>Price</Value>
		<Value lang="de">Preis</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.TierPrices.Fields.Quantity">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.TierPrices.Fields.Quantity">
		<Value>Quantity</Value>
		<Value lang="de">Menge</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.TierPrices.SaveBeforeEdit">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.TierPrices.SaveBeforeEdit">
		<Value>You need to save the product before you can add tier prices for this product page.</Value>
		<Value lang="de">Das Produkt muss zunächst gespeichert werden, um Staffelpreise festzulegen.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.Updated">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.TierPrices.Fields.Store">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.TierPrices.Fields.Store.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.TierPrices.Fields.Store">
		<Value>Store</Value>
		<Value lang="de">Shop</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.TierPrices.Fields.Store.Hint">
		<Value>Store</Value>
		<Value lang="de">Shop</Value>
	</LocaleResource>

	<LocaleResource Name="Admin.Catalog.Products.Fields.ProductType">
		<Value>Product type</Value>
		<Value lang="de">Produkttyp</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.ProductType.Hint">
		<Value>Choose your product type.</Value>
		<Value lang="de">Legt den Produkttyp fest.</Value>
	</LocaleResource>
	<LocaleResource Name="Enums.SmartStore.Core.Domain.Catalog.ProductType.SimpleProduct">
		<Value>Simple product</Value>
		<Value lang="de">Einfaches Produkt</Value>
	</LocaleResource>
	<LocaleResource Name="Enums.SmartStore.Core.Domain.Catalog.ProductType.GroupedProduct">
		<Value>Grouped product</Value>
		<Value lang="de">Gruppenprodukt</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.BulkEdit">
		<Value>Bulk edit products</Value>
		<Value lang="de">Produktmassenbearbeitung</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.LowStockReport.Manage">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Orders.Products.AddNew.Note1">
		<Value>Click on interested product</Value>
		<Value lang="de">Klicken Sie auf das relevante Produkt</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Promotions.Discounts.Fields.DiscountType.Hint">
		<Value>The type of discount.</Value>
		<Value lang="de">Der Rabatttyp.</Value>
	</LocaleResource>
	<LocaleResource Name="Enums.SmartStore.Core.Domain.Discounts.DiscountType.AssignedToSkus">
		<Value>Assigned to products</Value>
		<Value lang="de">Produkten zugewiesen</Value>
	</LocaleResource>
	<LocaleResource Name="PDFProductCatalog.UnnamedProductVariant">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.ProductTemplate.Hint">
		<Value>Choose a product template. This template defines how this product will be displayed in public store.</Value>
		<Value lang="de">Wählen Sie eine Produktvorlage. Diese Vorlage definiert, wie dieses Produkt im Shop angezeigt wird.</Value>
	</LocaleResource>

	<LocaleResource Name="Admin.Configuration.Settings.Media.ProductVariantPictureSize">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Configuration.Settings.Media.ProductVariantPictureSize.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Configuration.Settings.Media.AssociatedProductPictureSize">
		<Value>Associated product image size</Value>
		<Value lang="de">Bildgröße verknüpftes Produkt</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Configuration.Settings.Media.AssociatedProductPictureSize.Hint">
		<Value>The default size (pixels) for associated product images (part of "grouped" products).</Value>
		<Value lang="de">Die Standardbildgröße (in Pixel) von mit einem Gruppenprodukt verknüpften Produkten.</Value>
	</LocaleResource>
	
	<LocaleResource Name="Admin.Catalog.Products.AssociatedProducts">
		<Value>Associated products</Value>
		<Value lang="de">Verknüpfte Produkte</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.AssociatedProducts.AddNew">
		<Value>Add new associated product</Value>
		<Value lang="de">Verknüpftes Produkt hinzufügen</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.AssociatedProducts.Fields.Product">
		<Value>Product</Value>
		<Value lang="de">Produkt</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.AssociatedProducts.Note1">
		<Value>Associated products are used only with "grouped" products.</Value>
		<Value lang="de">Verknüpfte Produkte gibt es ausschließlich bei Gruppenprodukten.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.AssociatedProducts.Note2">
		<Value>A product could be associated to only one "grouped" product.</Value>
		<Value lang="de">Ein Produkt kann nur mit einem Gruppenprodukt verknüpft werden.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.AssociatedProducts.SaveBeforeEdit">
		<Value>You need to save the product before you can add associated products for this product page.</Value>
		<Value lang="de">Das Produkt muss gespeichert werden, bevor verknüpfte Produkte hinzugefügt werden können.</Value>
	</LocaleResource>
	
	<LocaleResource Name="Admin.Catalog.Products.Fields.VisibleIndividually">
		<Value>Visible individually</Value>
		<Value lang="de">Individuell sichtbar</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.VisibleIndividually.Hint">
		<Value>Check it if you want this product to be visible in catalog or search results. You can use this field (just uncheck) to hide associated products from catalog and make them accessible only from a parent "grouped" product details page.</Value>
		<Value lang="de">Legt fest, ob dieses Produkt im Katalog oder den Suchergebnissen angezeigt werden soll. Sie können dieses Feld verwenden (durch deaktivieren), um verknüpfte Produkte nur über deren übergeordnete Produkt-Detailseite zugänglich zu machen.</Value>
	</LocaleResource>
	
	<LocaleResource Name="Products.NoAssociatedProducts">
		<Value>This product is sold out.</Value>
		<Value lang="de">Dieses Produkt ist ausverkauft.</Value>
	</LocaleResource>
	
	<LocaleResource Name="Admin.Catalog.Products.List.SearchProductType">
		<Value>Product type</Value>
		<Value lang="de">Produkttyp</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.List.SearchProductType.Hint">
		<Value>Search by a product type.</Value>
		<Value lang="de">Nach einem Produkttyp suchen.</Value>
	</LocaleResource>

	<LocaleResource Name="Admin.Catalog.Products.AssociatedProducts.Fields.DisplayOrder">
		<Value>Display order</Value>
		<Value lang="de">Reihenfolge</Value>
	</LocaleResource>

	<LocaleResource Name="Admin.Orders.Shipments.List.TrackingNumber">
		<Value>Tracking number</Value>
		<Value lang="de">Tracking-Nummer</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Orders.Shipments.List.TrackingNumber.Hint">
		<Value>Search by a specific tracking number.</Value>
		<Value lang="de">Nach einer Tracking\Fracht-Nummer suchen.</Value>
	</LocaleResource>

	<LocaleResource Name="Admin.Catalog.Products.Fields.AssociatedToProductName">
		<Value>Associated to product</Value>
		<Value lang="de">Verknüpftes Produkt</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.AssociatedToProductName.Hint">
		<Value>The parent product which this one is associated to.</Value>
		<Value lang="de">Das übergeordnete Produkt, mit dem dieses Produkt verknüpft ist.</Value>
	</LocaleResource>

	<LocaleResource Name="ShoppingCart.ConflictingShipmentSchedules">
		<Value>Your cart has auto-ship (recurring) items with conflicting shipment schedules. Only one auto-ship schedule is allowed per order.</Value>
		<Value lang="de">Ihr Warenkorb enthält Abonnement-Produkte mit widersprüchlichen Versand-Zeitplänen. Pro Bestellung ist nur ein Abonnement-Produkt-Plan möglich.</Value>
	</LocaleResource>
	
	<LocaleResource Name="Admin.Common.CopyOf">
		<Value>Copy of</Value>
		<Value lang="de">Kopie von</Value>
	</LocaleResource>
	
	<LocaleResource Name="Admin.Promotions.Discounts.Fields.AppliedToProductVariants">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Promotions.Discounts.Fields.AppliedToProductVariants.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Promotions.Discounts.Fields.AppliedToProductVariants.NoRecords">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Promotions.Discounts.Fields.AppliedToProducts">
		<Value>Assigned to products</Value>
		<Value lang="de">Produkten zugeordnet</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Promotions.Discounts.Fields.AppliedToProducts.Hint">
		<Value>A list of products to which the discount is to be applied. You can assign this discount on a product details page.</Value>
		<Value lang="de">Eine Liste von Produkten, denen der Rabatt zugeordnet ist. Die Zuordnung kann auf der Produkt-Detailseite vorgenommen werden.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Promotions.Discounts.Fields.AppliedToProducts.NoRecords">
		<Value>No products selected</Value>
		<Value lang="de">Keine Produkte ausgewählt</Value>
	</LocaleResource>
	
	<LocaleResource Name="ShoppingCart.IsNotSimpleProduct">
		<Value>This is not a simple product.</Value>
		<Value lang="de">Hierbei handelt es sich nicht um ein einfaches Produkt.</Value>
	</LocaleResource>
	<LocaleResource Name="ShoppingCart.AddOnlySimpleProductsToCart">
		<Value>Only simple products could be added to the cart.</Value>
		<Value lang="de">Nur einfache Produkte können dem Warenkorb hinzugefügt werden.</Value>
	</LocaleResource>
	
	<LocaleResource Name="Admin.Configuration.Settings.Catalog.SuppressSkuSearch">
		<Value>Suppress SKU search</Value>
		<Value lang="de">SKU Suche unterdrücken</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Configuration.Settings.Catalog.SuppressSkuSearch.Hint">
		<Value>Check to disable the searching of product SKUs. This setting can increase the performance of searching.</Value>
		<Value lang="de">Legt fest, ob die Suche nach Produkt-SKUs unterbunden werden soll. Diese Einstellung kann die Performance der Suche erhöhen.</Value>
	</LocaleResource>

	<LocaleResource Name="Admin.Configuration.Settings.Order.DisableOrderCompletedPage">
		<Value>Disable "Order completed" page</Value>
		<Value lang="de">"Auftrag abgeschlossen" Seite unterbinden</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Configuration.Settings.Order.DisableOrderCompletedPage.Hint">
		<Value>When disabled, customers will be automatically redirected to the order details page.</Value>
		<Value lang="de">Der Kunde wird direkt auf die Auftrags-Detail-Seite geleitet, falls diese Einstellung aktiviert ist.</Value>
	</LocaleResource>
	
	<LocaleResource Name="Plugins.Feed.ElmarShopinfo.ExportSpecialPrice">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Plugins.Feed.ElmarShopinfo.ExportSpecialPrice.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Plugins.Feed.ElmarShopinfo.SpecialPrice">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Plugins.Feed.ElmarShopinfo.SpecialPrice.Hint">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.CombiNotExists">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Fields.AttributeControlType">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Variants.TierPrices.Fields.Store.All">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	
	<LocaleResource Name="Admin.Catalog.Products.Price">
		<Value>Price</Value>
		<Value lang="de">Preis</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Promotion">
		<Value>Promotion</Value>
		<Value lang="de">Promotion</Value>
	</LocaleResource>
	
	<LocaleResource Name="Admin.Catalog.Products.ProductVariantAttributes.AttributeCombinations.Description">
		<Value>Attribute combinations allow different product characteristics on the basis of specific combinations. Set a combination to inactive rather than deleting them, if it is no longer available.</Value>
		<Value lang="de">Attribut-Kombinationen ermöglichen die Erfassung abweichender Produkt-Eigenschaften auf Basis von spezifischen Kombinationen. Setzen Sie eine Kombination auf inaktiv anstatt sie zu löschen, falls sie nicht mehr verfügbar ist.</Value>
	</LocaleResource>


	<!-- 3. BUNDLES -->
	<LocaleResource Name="Enums.SmartStore.Core.Domain.Catalog.ProductType.BundledProduct">
		<Value>Bundled product</Value>
		<Value lang="de">Produkt-Bundle</Value>
	</LocaleResource>

	<LocaleResource Name="Admin.Catalog.Products.BundleItems.SaveBeforeEdit">
		<Value>You need to save the product before you can add bundled products for this product page.</Value>
		<Value lang="de">Das Produkt muss gespeichert werden, bevor Produkte zur Stückliste hinzugefügt werden können.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.BundleItems">
		<Value>Bundled products</Value>
		<Value lang="de">Stückliste</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.BundleItems.AddNew">
		<Value>Add new product to bundle</Value>
		<Value lang="de">Produkt zur Stückliste hinzufügen</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.BundleItems.EditOf">
		<Value>Edit bundle product</Value>
		<Value lang="de">Bundle-Produkt bearbeiten</Value>
	</LocaleResource>	
	
	<LocaleResource Name="Common.CreatedOn">
		<Value>Created on</Value>
		<Value lang="de">Erstellt am</Value>
	</LocaleResource>
	<LocaleResource Name="Common.CreatedOn.Hint">
		<Value>Date of creation.</Value>
		<Value lang="de">Datum der Erstellung.</Value>
	</LocaleResource>
	<LocaleResource Name="Common.UpdatedOn">
		<Value>Updated on</Value>
		<Value lang="de">Geändert am</Value>
	</LocaleResource>
	<LocaleResource Name="Common.UpdatedOn.Hint">
		<Value>Date of last modification.</Value>
		<Value lang="de">Datum der letzten Modifizierung.</Value>
	</LocaleResource>
	
	<LocaleResource Name="Admin.Catalog.Products.BundleItems.Fields.Product">
		<Value>Product</Value>
		<Value lang="de">Produkt</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.BundleItems.Fields.Quantity">
		<Value>Quantity</Value>
		<Value lang="de">Menge</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.BundleItems.Fields.Quantity.Hint">
		<Value>Every time the bundle is added to the cart, the quantity of that item will be adjusted based on the value specified here.</Value>
		<Value lang="de">Die Produktmenge wird gemäß dem hier angegebenen Wert angepasst, wenn das Bundle zum Warenkorb hinzugefügt wird.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.BundleItems.Fields.Discount">
		<Value>Discount</Value>
		<Value lang="de">Rabatt</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.BundleItems.Fields.Discount.Hint">
		<Value>The discount value.</Value>
		<Value lang="de">Der Rabattwert.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.BundleItems.Fields.DiscountPercentage">
		<Value>Discount percentage</Value>
		<Value lang="de">Rabatt in Prozent</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.BundleItems.Fields.DiscountPercentage.Hint">
		<Value>Specifies whether the discount is a percentage or a fixed value.</Value>
		<Value lang="de">Legt fest, ob es sich bei dem Rabatt um einen prozentualen oder einen festen Wert handelt.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.BundleItems.Fields.Name">
		<Value>Product name</Value>
		<Value lang="de">Produktname</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.BundleItems.Fields.Name.Hint">
		<Value>Offers the possibility to display a different product name in the bundle item list.</Value>
		<Value lang="de">Bietet die Möglichkeit, in der Stückliste einen abweichenden Produktnamen anzuzeigen.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.BundleItems.Fields.ShortDescription">
		<Value>Short description</Value>
		<Value lang="de">Kurzbeschreibung</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.BundleItems.Fields.ShortDescription.Hint">
		<Value>Offers the possibility to display a different short description in the bundle item list. Enter a space, if no short description should appear in the bundle item list, even though the product has one.</Value>
		<Value lang="de">Bietet die Möglichkeit, in der Stückliste einen abweichende Kurzbeschreibung anzuzeigen. Geben Sie ein Leerzeichen ein, falls in der Stückliste keine Kurzbeschreibung angezeigt werden soll, obwohl das Produkt eine besitzt.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.BundleItems.Fields.HideThumbnail">
		<Value>Hide thumbnail</Value>
		<Value lang="de">Vorschaubild ausblenden</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.BundleItems.Fields.HideThumbnail.Hint">
		<Value>Hide the thumbnail for the product.</Value>
		<Value lang="de">Das Vorschaubild des Produktes ausblenden.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.BundleItems.Fields.Visible">
		<Value>Visible</Value>
		<Value lang="de">Sichtbar</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.BundleItems.Fields.Visible.Hint">
		<Value>Allows to hide a product included in the bundle, so that it does not appear on the product page.</Value>
		<Value lang="de">Ermöglicht es, ein im Bundle enthaltenes Produkt zu verbergen, so dass es auf der Produktseite nicht angezeigt wird.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.BundleItems.Fields.Published">
		<Value>Active</Value>
		<Value lang="de">Aktiv</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.BundleItems.Fields.Published.Hint">
		<Value>Determines whether the product is included in the bundle. Note: Non-published products are never part of a bundle.</Value>
		<Value lang="de">Legt fest, ob das Produkt im Bundle enthalten ist. Hinweis: Nicht veröffentlichte Produkte sind niemals Teil eines Bundles.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.BundleItems.Fields.DisplayOrder">
		<Value>Bundle display order</Value>
		<Value lang="de">Reihenfolge im Bundle</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.BundleItems.Fields.DisplayOrder.Hint">
		<Value>The position of the product in the bundle.</Value>
		<Value lang="de">Die Position des Produkts im Bundle.</Value>
	</LocaleResource>

	<LocaleResource Name="Admin.Catalog.Products.Fields.BundleTitleText">
		<Value>Bundle title text</Value>
		<Value lang="de">Bundle Titeltext</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.BundleTitleText.Hint">
		<Value>Optional title text of the product bundle.</Value>
		<Value lang="de">Optionaler Titeltext des Produkt Bundle.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.BundlePerItemShipping">
		<Value>Per-item shipping</Value>
		<Value lang="de">Versand per Bundle-Bestandteil</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.BundlePerItemShipping.Hint">
		<Value>Sets whether the shipping cost of the bundle should be calculated or the amount of shipping of all products in the bundle.</Value>
		<Value lang="de">Legt fest, ob die Versandkosten des Bundles berechnet werden sollen oder die Summe der Versandkosten aller Produkte im Bundle.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.BundlePerItemPricing">
		<Value>Per-item pricing</Value>
		<Value lang="de">Preis per Bundle-Bestandteil</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.BundlePerItemPricing.Hint">
		<Value>Sets whether to calculate the price of the bundle or the sum of the individual prices of all products in the bundle.</Value>
		<Value lang="de">Legt fest, ob der Preis des Bundles oder die Summe der Einzelpreise aller Produkte im Bundle berechnet werden soll.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.BundlePerItemShoppingCart">
		<Value>Bundle item individually in the shopping cart</Value>
		<Value lang="de">Bundle-Bestandteil einzeln in den Warenkorb</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.Fields.BundlePerItemShoppingCart.Hint">
		<Value>Quantity and price of bundle items appear in the shopping cart. Inventory management based on the bundle items, rather than of the bundle.</Value>
		<Value lang="de">Menge und Preis von Bundle-Bestandteilen werden im Warenkorb angezeigt. Bestandsführung erfolgt auf Basis der Bundle-Bestandteile, anstatt des Bundles.</Value>
	</LocaleResource>
	
	<LocaleResource Name="Products.NoBundledItems">
		<Value>This product is sold out.</Value>
		<Value lang="de">Dieses Produkt ist ausverkauft.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Configuration.Settings.Media.BundledProductPictureSize">
		<Value>Bundled product image size</Value>
		<Value lang="de">Bildgröße des Produktes im Bundle</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Configuration.Settings.Media.BundledProductPictureSize.Hint">
		<Value>The default size (pixels) for bundled product images.</Value>
		<Value lang="de">Die Standardbildgröße (in Pixel) von Produkten im Bundle.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Configuration.Settings.Media.CartThumbBundleItemPictureSize">
		<Value>Cart/Wishlist thumbnail image size for bundle items</Value>
		<Value lang="de">Bildgröße der Thumbnails von Bundle-Bestandteilen im Warenkorb</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Configuration.Settings.Media.CartThumbBundleItemPictureSize.Hint">
		<Value>The default size (pixels) for product thumbnail images of bundle items on the shopping cart and wishlist.</Value>
		<Value lang="de">Standardgröße in Pixeln der Produktthumbnails von Bundle-Bestandteilen im Warenkorb und auf dem Wunschzettel.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Configuration.Settings.ShoppingCart.ShowProductBundleImagesOnShoppingCart">
		<Value>Show product images of bundle items on cart</Value>
		<Value lang="de">Anzeige der Produktbilder von Bundle-Bestandteilen im Warenkorb</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Configuration.Settings.ShoppingCart.ShowProductBundleImagesOnShoppingCart.Hint">
		<Value>Determines whether product images of bundle items should be displayed in your store shopping cart.</Value>
		<Value lang="de">Zeigt Produktbilder von Bundle-Bestandteilen im Warenkorb an.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Configuration.Settings.ShoppingCart.ShowProductBundleImagesOnWishList">
		<Value>Show product images of bundle items on wishlist</Value>
		<Value lang="de">Zeigt Produktbilder von Bundle-Bestandteilen auf dem Wunschzettel</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Configuration.Settings.ShoppingCart.ShowProductBundleImagesOnWishList.Hint">
		<Value>Determines whether product images of bundle items should be displayed on customer wishlists.</Value>
		<Value lang="de">Zeigt Produktbilder von Bundle-Bestandteilen auf dem Wunschzettel an.</Value>
	</LocaleResource>
	
	<LocaleResource Name="Admin.Catalog.Products.BundleItems.Attributes">
		<Value>Attributes</Value>
		<Value lang="de">Attribute</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.BundleItems.Fields.FilterAttributes">
		<Value>Exclude attributes</Value>
		<Value lang="de">Attribute ausschließen</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.BundleItems.Fields.FilterAttributes.Hint">
		<Value>Select this option if you want to exclude attributes for the product in the bundle.</Value>
		<Value lang="de">Aktivieren Sie diese Option, falls Sie Attribute für das Produkt im Bundle ausschließen möchten.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.BundleItems.Fields.FilterAttributes.NoneNote">
		<Value>There are no attributes specified for this product.</Value>
		<Value lang="de">Für das Produkt sind keine Attribute festgelegt.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.BundleItems.Fields.FilterPreSelect">
		<Value>Is pre-selected</Value>
		<Value lang="de">Vorausgewählt</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.BundleItems.Fields.FilterPreSelect.Hint">
		<Value>Determines whether this attribute value is pre selected for the customer</Value>
		<Value lang="de">Legt fest, ob dieses Attribut vorausgewählt ist.</Value>
	</LocaleResource>
	
	<LocaleResource Name="Admin.Common.DataSuccessfullySaved">
		<Value>The data were saved successfully.</Value>
		<Value lang="de">Die Daten wurden erfolgreich gespeichert.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Common.PleaseSelect">
		<Value>Please select</Value>
		<Value lang="de">Bitte auswählen</Value>
	</LocaleResource>

	<LocaleResource Name="Admin.Catalog.Products.ProductType.BundledProduct.Label">
		<Value>Bundle</Value>
		<Value lang="de">Bundle</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.ProductType.GroupedProduct.Label">
		<Value>Group</Value>
		<Value lang="de">Gruppe</Value>
	</LocaleResource>

	<LocaleResource Name="Admin.Catalog.Products.BundleItems.AdminNoteGeneral">
		<Value><![CDATA[
		<p>All simple products except such with recurring payments and downloads can be part of a bundle.</p>
		]]></Value>
		<Value lang="de"><![CDATA[
		<p>Alle einfachen Produkte außer Abos und Downloads können Bestandteil eines Bundle sein.</p>
		]]></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.BundleItems.AdminNotePerItemPricing">
		<Value><![CDATA[
		<p>Included or taken into account during calculation of the price of bundle items are:<br />Special price, price adjustment of attributes, per bundle item specified discounts and quantities.</p>
		<p>Not included and taken into account are:<br />Tier prices, customer enters price, call for price, weight adjustment of attributes, all other discounts.</p>
		<p>The display of basic prices can be controlled by an option (see catalog settings).</p>
		]]></Value>
		<Value lang="de"><![CDATA[
		<p>Bei der Preisberechnung per Bundle-Bestandteile enthalten bzw. berücksichtigt sind:<br />Aktionspreis, Mehr-/Minderpreis von Attributen, per Bundle-Bestandteile festgelegte Rabatte und Mengen.</p>
		<p>Nicht enthalten bzw. unberücksichtigt sind:<br />Staffelpreise, Preis auf Anfrage, Preisvorschlag, Mehr-/Mindergewichte von Attributen, alle sonstigen Rabatte.</p>
		<p>Die Anzeige von Grundpreisen kann über eine Option (s. Katalog-Einstellungen) gesteuert werden.</p>
		]]></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.BundleItems.AdminNoteBundlePricing">
		<Value><![CDATA[
		<p>Not included in bundle pricing and not taken into account are:<br />Price adjustments of attributes, discounts of bundle items. A selection of attributes by the customer is not possible.</p>
		]]></Value>
		<Value lang="de"><![CDATA[
		<p>Bei der Preisberechnung per Bundle nicht enthalten bzw. unberücksichtigt sind:<br />Mehr-/Minderpreise von Attributen, Rabatte von Bundle-Bestandteilen. Ferner ist eine Attributauswahl durch den Kunden nicht möglich.</p>
		]]></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.BundleItems.AdminNotePerItemShipping">
		<Value><![CDATA[
		<p>Included or taken into account during calculation of shipping cost of bundle items are:<br />Shipping enabled, Free shipping, Additional shipping charge.</p>
		]]></Value>
		<Value lang="de"><![CDATA[
		<p>Bei der Versandkostenberechnung per Bundle-Bestandteile enthalten bzw. berücksichtigt sind:<br />Versand möglich, Versandkostenfrei, Transportzuschlag.</p>
		]]></Value>
	</LocaleResource>
	
	<LocaleResource Name="Admin.Configuration.Settings.Catalog.BundleItemShowBasePrice">
		<Value>Base price for bundle items</Value>
		<Value lang="de">Grundpreis bei Bundle-Bestandteilen</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Configuration.Settings.Catalog.BundleItemShowBasePrice.Hint">
		<Value>Sets whether the base price should be displayed for bundle items.</Value>
		<Value lang="de">Legt fest, ob der Grundpreis bei Bundle-Bestandteilen angezeigt werden soll.</Value>
	</LocaleResource>
	
	<LocaleResource Name="Products.Bundle.PriceWithDiscount.Note">
		<Value>Now only:</Value>
		<Value lang="de">Jetzt nur:</Value>
	</LocaleResource>
	<LocaleResource Name="Products.Bundle.PriceWithoutDiscount.Note">
		<Value>Instead of:</Value>
		<Value lang="de">Anstatt:</Value>
	</LocaleResource>
	<LocaleResource Name="Products.Price">
		<Value>Price:</Value>
		<Value lang="de">Preis:</Value>
	</LocaleResource>
	
	<LocaleResource Name="ShoppingCart.IsNotSimpleProduct">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="ShoppingCart.ShoppingCart.AddOnlySimpleProductsToCart">
		<Value></Value>
		<Value lang="de"></Value>
	</LocaleResource>
	<LocaleResource Name="ShoppingCart.ProductNotAvailableForOrder">
		<Value>This product is not available for order.</Value>
		<Value lang="de">Dieses Produkt kann nicht bestellt werden.</Value>
	</LocaleResource>
	<LocaleResource Name="ShoppingCart.Bundle.BundleItemUnpublished">
		<Value>The bundle item "{0}" is not published.</Value>
		<Value lang="de">Der Bundle-Bestandteil "{0}" wurde nicht veröffentlicht.</Value>
	</LocaleResource>
	<LocaleResource Name="ShoppingCart.Bundle.BundleItemNotFound">
		<Value>The bundle item "{0}" cannot be found.</Value>
		<Value lang="de">Der Bundle-Bestandteil "{0}" wurde nicht gefunden.</Value>
	</LocaleResource>
	<LocaleResource Name="ShoppingCart.Bundle.MissingProduct">
		<Value>The bundle item "{0}" has a missing product or bundle link.</Value>
		<Value lang="de">Bei dem Bundle-Bestandteil "{0}" fehlt die Verknüpfung zum Produkt oder Bundle.</Value>
	</LocaleResource>
	<LocaleResource Name="ShoppingCart.Bundle.Quantity">
		<Value>The bundle item "{0}" has a of 0 or less 0.</Value>
		<Value lang="de">Bei dem Bundle-Bestandteil "{0}" ist die Menge kleiner gleich 0.</Value>
	</LocaleResource>
	<LocaleResource Name="ShoppingCart.Bundle.ProductResrictions">
		<Value>The bundle item "{0}" is a download or has recurring payment which is not supported.</Value>
		<Value lang="de">Bei dem Bundle-Bestandteil "{0}" handelt es sich um ein Abo oder Download, was nicht unterstützt wird.</Value>
	</LocaleResource>
	<LocaleResource Name="ShoppingCart.Bundle.NoAttributes">
		<Value>Attributes are not possible for bundles.</Value>
		<Value lang="de">Bei Bundles sind Attribute nicht möglich.</Value>
	</LocaleResource>
	<LocaleResource Name="ShoppingCart.Bundle.NoCustomerEnteredPrice">
		<Value>Price proposals are not possible for this bundle.</Value>
		<Value lang="de">Bei diesem Bundle sind Preisvorschläge nicht möglich.</Value>
	</LocaleResource>
	
	<LocaleResource Name="ShoppingCart.DeleteCartItem.Success">
		<Value>The product has been removed.</Value>
		<Value lang="de">Das Produkt wurde entfernt.</Value>
	</LocaleResource>
	<LocaleResource Name="ShoppingCart.DeleteCartItem.Failed">
		<Value>An error occurred during the removal of the product.</Value>
		<Value lang="de">Es ist ein Fehler beim Entfernen des Produktes aufgetreten.</Value>
	</LocaleResource>
	
	<LocaleResource Name="Common.NoProcessingSecurityIssue">
		<Value>The operation was not carried out for security reasons.</Value>
		<Value lang="de">Der Vorgang wurde aus Sicherheitsgründen nicht ausgeführt.</Value>
	</LocaleResource>
	<LocaleResource Name="Products.ProductNotAddedToTheCart">
		<Value>Product could not be added to the shopping cart.</Value>
		<Value lang="de">Produkt konnte nicht zum Warenkorb hinzugefügt werden.</Value>
	</LocaleResource>
	<LocaleResource Name="Products.SelectProducts">
		<Value>Please select the desired products.</Value>
		<Value lang="de">Bitte die gewünschten Produkte auswählen.</Value>
	</LocaleResource>
	
	<LocaleResource Name="Admin.Orders.Products.AddNew.Note1">
		<Value>Click on the desired product to add it to the order.</Value>
		<Value lang="de">Klicken Sie auf das gewünschte Produkt, um es dem Auftrag hinzuzufügen.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Orders.Products.AddNew.Note2">
		<Value>Do not to forget to update order totals after adding this product.</Value>
		<Value lang="de">Vergessen Sie nicht, die Auftragssummen zu aktualisieren, nachdem Sie das Produkt hinzugefügt haben.</Value>
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

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[ShippingMethod]') and NAME='IgnoreCharges')
BEGIN
	EXEC ('ALTER TABLE [ShippingMethod] ADD [IgnoreCharges] bit NULL')
	EXEC ('UPDATE [ShippingMethod] SET [IgnoreCharges] = 0 WHERE [IgnoreCharges] IS NULL')
	EXEC ('ALTER TABLE [ShippingMethod] ALTER COLUMN [IgnoreCharges] bit NOT NULL')
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Log]') and NAME='UpdatedOnUtc')
BEGIN
	ALTER TABLE [Log] ADD [UpdatedOnUtc] datetime NULL
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Log]') and NAME='Frequency')
BEGIN
	EXEC ('ALTER TABLE [Log] ADD [Frequency] int NULL')
	EXEC ('UPDATE [Log] SET [Frequency] = 1 WHERE [Frequency] IS NULL')
	EXEC ('ALTER TABLE [Log] ALTER COLUMN [Frequency] int NOT NULL')
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Log]') and NAME='ContentHash')
BEGIN
	ALTER TABLE [Log] ADD [ContentHash] nvarchar(40) NULL
END
GO

IF NOT EXISTS (SELECT 1 from sys.indexes WHERE [NAME]=N'IX_Log_ContentHash' and object_id=object_id(N'[Log]'))
BEGIN
	CREATE NONCLUSTERED INDEX [IX_Log_ContentHash] ON [Log] ([ContentHash] ASC)
END
GO

--'Delete logs' schedule task (enabled by default)
IF NOT EXISTS (
		SELECT 1
		FROM [dbo].[ScheduleTask]
		WHERE [Name] = N'Delete logs')
BEGIN
	INSERT [dbo].[ScheduleTask] ([Name], [Seconds], [Type], [Enabled], [StopOnError])
	VALUES (N'Delete logs', 86400, N'SmartStore.Services.Logging.DeleteLogsTask, SmartStore.Services', 1, 0)
END
GO

-- AdminAreaSettings.RichEditorFlavor
IF NOT EXISTS (SELECT 1 FROM [Setting] WHERE [Name] = N'AdminAreaSettings.RichEditorFlavor')
BEGIN
	INSERT [Setting] ([Name], [Value], [StoreId])
	VALUES (N'AdminAreaSettings.RichEditorFlavor', N'RichEditor', 0)
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Currency]') and NAME='DomainEndings')
BEGIN
	ALTER TABLE [Currency] ADD [DomainEndings] nvarchar(1000) NULL
END
GO

-- QueuedEmail.ReplyTo (New)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[QueuedEmail]') and NAME='ReplyTo')
BEGIN
	ALTER TABLE [QueuedEmail] ADD [ReplyTo] nvarchar(500) NULL
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[QueuedEmail]') and NAME='ReplyToName')
BEGIN
	ALTER TABLE [QueuedEmail] ADD [ReplyToName] nvarchar(500) NULL
END
GO

-- From Migration "AddNavProps"
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='SampleDownloadId')
BEGIN
	ALTER TABLE [Product] ALTER COLUMN [SampleDownloadId] [int] NULL
END
GO

ALTER TABLE [Category] ALTER COLUMN [PictureId] [int] NULL
GO

ALTER TABLE [Manufacturer] ALTER COLUMN [PictureId] [int] NULL
GO

Update [Category] SET PictureId = null WHERE PictureId = 0
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('[Category]') and NAME='IX_PictureId')
BEGIN
	CREATE INDEX [IX_PictureId] ON [Category]([PictureId])
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Category_Picture]') AND parent_object_id = OBJECT_ID(N'[Category]'))
BEGIN
	ALTER TABLE [Category] WITH CHECK ADD CONSTRAINT [Category_Picture] FOREIGN KEY ([PictureId]) REFERENCES [Picture] ([Id])
END
GO

Update [Manufacturer] SET PictureId = null WHERE PictureId = 0
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('[Manufacturer]') and NAME='IX_PictureId')
BEGIN
	CREATE INDEX [IX_PictureId] ON [Manufacturer]([PictureId])
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_dbo.Manufacturer_dbo.Picture_PictureId]') AND parent_object_id = OBJECT_ID(N'[Manufacturer]'))
BEGIN
	ALTER TABLE [Manufacturer]  WITH CHECK ADD CONSTRAINT [FK_dbo.Manufacturer_dbo.Picture_PictureId] FOREIGN KEY([PictureId]) REFERENCES [dbo].[Picture] ([Id])
END
GO








IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[ProductVariant]') and NAME='BasePrice_Enabled')
BEGIN
	EXEC sp_rename 'ProductVariant.BasePrice_Enabled', 'BasePriceEnabled', 'COLUMN';
END
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[ProductVariant]') and NAME='BasePrice_MeasureUnit')
BEGIN
	EXEC sp_rename 'ProductVariant.BasePrice_MeasureUnit', 'BasePriceMeasureUnit', 'COLUMN';
END
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[ProductVariant]') and NAME='BasePrice_Amount')
BEGIN
	EXEC sp_rename 'ProductVariant.BasePrice_Amount', 'BasePriceAmount', 'COLUMN';
END
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[ProductVariant]') and NAME='BasePrice_BaseAmount')
BEGIN
	EXEC sp_rename 'ProductVariant.BasePrice_BaseAmount', 'BasePriceBaseAmount', 'COLUMN';
END
GO

--rename ShipmentOrderProductVariant to ShipmentItem
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Shipment_OrderProductVariant]') and OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	EXEC sp_rename 'Shipment_OrderProductVariant', 'ShipmentItem';
END
GO

IF EXISTS (SELECT 1
           FROM sys.objects
           WHERE name = 'ShipmentOrderProductVariant_Shipment'
           AND parent_object_id = Object_id('ShipmentItem')
           AND Objectproperty(object_id,N'IsForeignKey') = 1)
BEGIN
	EXEC sp_rename 'ShipmentOrderProductVariant_Shipment', 'ShipmentItem_Shipment';
END
GO

--rename OrderProductVariant to OrderItem
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[GiftCard]') and NAME='PurchasedWithOrderProductVariantId')
BEGIN
	EXEC sp_rename 'GiftCard.PurchasedWithOrderProductVariantId', 'PurchasedWithOrderItemId', 'COLUMN';
END
GO
IF EXISTS (SELECT 1
           FROM sys.objects
           WHERE name = 'GiftCard_PurchasedWithOrderProductVariant'
           AND parent_object_id = Object_id('GiftCard')
           AND Objectproperty(object_id,N'IsForeignKey') = 1)
BEGIN
	EXEC sp_rename 'GiftCard_PurchasedWithOrderProductVariant', 'GiftCard_PurchasedWithOrderItem';
END
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[OrderProductVariant]') and OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	EXEC sp_rename 'OrderProductVariant', 'OrderItem';
END
GO

IF EXISTS (SELECT 1
           FROM sys.objects
           WHERE name = 'OrderProductVariant_Order'
           AND parent_object_id = Object_id('OrderItem')
           AND Objectproperty(object_id,N'IsForeignKey') = 1)
BEGIN
	EXEC sp_rename 'OrderProductVariant_Order', 'OrderItem_Order';
END
GO

IF EXISTS (SELECT 1
           FROM sys.objects
           WHERE name = 'OrderProductVariant_ProductVariant'
           AND parent_object_id = Object_id('OrderItem')
           AND Objectproperty(object_id,N'IsForeignKey') = 1)
BEGIN
	EXEC sp_rename 'OrderProductVariant_ProductVariant', 'OrderItem_ProductVariant';
END
GO

IF EXISTS (SELECT 1 from sys.indexes WHERE [NAME]=N'IX_OrderProductVariant_OrderId' and object_id=object_id(N'[OrderItem]'))
BEGIN
	EXEC sp_rename 'OrderItem.IX_OrderProductVariant_OrderId', 'IX_OrderItem_OrderId', 'INDEX';
END
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[ReturnRequest]') and NAME='OrderProductVariantId')
BEGIN
	EXEC sp_rename 'ReturnRequest.OrderProductVariantId', 'OrderItemId', 'COLUMN';
END
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[ShipmentItem]') and NAME='OrderProductVariantId')
BEGIN
	EXEC sp_rename 'ShipmentItem.OrderProductVariantId', 'OrderItemId', 'COLUMN';
END
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[OrderItem]') and NAME='OrderProductVariantGuid')
BEGIN
	EXEC sp_rename 'OrderItem.OrderProductVariantGuid', 'OrderItemGuid', 'COLUMN';
END
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[DiscountRequirement]') and NAME='RestrictedProductVariantIds')
BEGIN
	EXEC sp_rename 'DiscountRequirement.RestrictedProductVariantIds', 'RestrictedProductIds', 'COLUMN';
END
GO


DELETE FROM [ActivityLogType] WHERE [SystemKeyword] = N'AddNewProductVariant'
GO
DELETE FROM [ActivityLogType] WHERE [SystemKeyword] = N'DeleteProductVariant'
GO
DELETE FROM [ActivityLogType] WHERE [SystemKeyword] = N'EditProductVariant'
GO

--remove obsolete setting
DELETE FROM [Setting] WHERE [name] = N'MediaSettings.ProductVariantPictureSize'
GO
DELETE FROM [Setting] WHERE [name] = N'ElmarShopinfoSettings.ExportSpecialPrice'
GO

IF NOT EXISTS (SELECT 1 FROM [Setting] WHERE [name] = N'mediasettings.associatedproductpicturesize')
BEGIN
	INSERT [Setting] ([Name], [Value], [StoreId]) VALUES (N'mediasettings.associatedproductpicturesize', N'125', 0)
END
GO

--update some message template tokens
UPDATE [MessageTemplate]
SET [Subject] = REPLACE([Subject], 'ProductVariant.ID', 'Product.ID'),
[Body] = REPLACE([Body], 'ProductVariant.ID', 'Product.ID')
GO

UPDATE [MessageTemplate]
SET [Subject] = REPLACE([Subject], 'ProductVariant.FullProductName', 'Product.Name'),
[Body] = REPLACE([Body], 'ProductVariant.FullProductName', 'Product.Name')
GO

UPDATE [MessageTemplate]
SET [Subject] = REPLACE([Subject], 'ProductVariant.StockQuantity', 'Product.StockQuantity'),
[Body] = REPLACE([Body], 'ProductVariant.StockQuantity', 'Product.StockQuantity')
GO

--update product templates
UPDATE [ProductTemplate]
SET [Name] = N'Grouped product', [ViewPath] = N'ProductTemplate.Grouped', [DisplayOrder] = 100
WHERE [ViewPath] = N'ProductTemplate.VariantsInGrid'
GO
UPDATE [ProductTemplate]
SET [Name] = N'Simple product', [ViewPath] = N'ProductTemplate.Simple', [DisplayOrder] = 10
WHERE [ViewPath] = N'ProductTemplate.SingleVariant'
GO

IF (NOT EXISTS(SELECT 1 FROM [ProductTemplate] WHERE [ViewPath] = N'ProductTemplate.Grouped'))
BEGIN
	INSERT INTO [ProductTemplate] ([Name],[ViewPath],[DisplayOrder])
	VALUES (N'Grouped product',N'ProductTemplate.Grouped',100)
END
GO

IF (NOT EXISTS(SELECT 1 FROM [ProductTemplate] WHERE [ViewPath] = N'ProductTemplate.Simple'))
BEGIN
	INSERT INTO [ProductTemplate] ([Name],[ViewPath],[DisplayOrder])
	VALUES (N'Simple product',N'ProductTemplate.Simple',10)
END
GO

--delete products without variants
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[ProductVariant]') and OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	DELETE FROM [Product] WHERE [Id] NOT IN (SELECT [ProductId] FROM [ProductVariant])
END
GO

--move records from ProductVariant to Product
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='ProductTypeId')
BEGIN
	ALTER TABLE [Product]
	ADD [ProductTypeId] int NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='ParentGroupedProductId')
BEGIN
	ALTER TABLE [Product]
	ADD [ParentGroupedProductId] int NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='SKU')
BEGIN
	ALTER TABLE [Product]
	ADD [SKU] nvarchar(400) NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='ManufacturerPartNumber')
BEGIN
	ALTER TABLE [Product]
	ADD [ManufacturerPartNumber] nvarchar(400) NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='Gtin')
BEGIN
	ALTER TABLE [Product]
	ADD [Gtin] nvarchar(400) NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='IsGiftCard')
BEGIN
	ALTER TABLE [Product]
	ADD [IsGiftCard] bit NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='GiftCardTypeId')
BEGIN
	ALTER TABLE [Product]
	ADD [GiftCardTypeId] int NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='RequireOtherProducts')
BEGIN
	ALTER TABLE [Product]
	ADD [RequireOtherProducts] bit NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='RequiredProductIds')
BEGIN
	ALTER TABLE [Product]
	ADD [RequiredProductIds] nvarchar(1000) NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='AutomaticallyAddRequiredProducts')
BEGIN
	ALTER TABLE [Product]
	ADD [AutomaticallyAddRequiredProducts] bit NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='IsDownload')
BEGIN
	ALTER TABLE [Product]
	ADD [IsDownload] bit NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='DownloadId')
BEGIN
	ALTER TABLE [Product]
	ADD [DownloadId] int NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='UnlimitedDownloads')
BEGIN
	ALTER TABLE [Product]
	ADD [UnlimitedDownloads] bit NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='MaxNumberOfDownloads')
BEGIN
	ALTER TABLE [Product]
	ADD [MaxNumberOfDownloads] int NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='DownloadExpirationDays')
BEGIN
	ALTER TABLE [Product]
	ADD [DownloadExpirationDays] int NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='DownloadActivationTypeId')
BEGIN
	ALTER TABLE [Product]
	ADD [DownloadActivationTypeId] int NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='HasSampleDownload')
BEGIN
	ALTER TABLE [Product]
	ADD [HasSampleDownload] bit NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='SampleDownloadId')
BEGIN
	ALTER TABLE [Product]
	ADD [SampleDownloadId] int NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='HasUserAgreement')
BEGIN
	ALTER TABLE [Product]
	ADD [HasUserAgreement] bit NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='UserAgreementText')
BEGIN
	ALTER TABLE [Product]
	ADD [UserAgreementText] nvarchar(MAX) NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='IsRecurring')
BEGIN
	ALTER TABLE [Product]
	ADD [IsRecurring] bit NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='RecurringCycleLength')
BEGIN
	ALTER TABLE [Product]
	ADD [RecurringCycleLength] int NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='RecurringCyclePeriodId')
BEGIN
	ALTER TABLE [Product]
	ADD [RecurringCyclePeriodId] int NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='RecurringTotalCycles')
BEGIN
	ALTER TABLE [Product]
	ADD [RecurringTotalCycles] int NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='IsShipEnabled')
BEGIN
	ALTER TABLE [Product]
	ADD [IsShipEnabled] bit NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='IsFreeShipping')
BEGIN
	ALTER TABLE [Product]
	ADD [IsFreeShipping] bit NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='AdditionalShippingCharge')
BEGIN
	ALTER TABLE [Product]
	ADD [AdditionalShippingCharge] decimal(18,4) NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='IsTaxExempt')
BEGIN
	ALTER TABLE [Product]
	ADD [IsTaxExempt] bit NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='TaxCategoryId')
BEGIN
	ALTER TABLE [Product]
	ADD [TaxCategoryId] int NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='ManageInventoryMethodId')
BEGIN
	ALTER TABLE [Product]
	ADD [ManageInventoryMethodId] int NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='StockQuantity')
BEGIN
	ALTER TABLE [Product]
	ADD [StockQuantity] int NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='DisplayStockAvailability')
BEGIN
	ALTER TABLE [Product]
	ADD [DisplayStockAvailability] bit NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='DisplayStockQuantity')
BEGIN
	ALTER TABLE [Product]
	ADD [DisplayStockQuantity] bit NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='MinStockQuantity')
BEGIN
	ALTER TABLE [Product]
	ADD [MinStockQuantity] int NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='LowStockActivityId')
BEGIN
	ALTER TABLE [Product]
	ADD [LowStockActivityId] int NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='NotifyAdminForQuantityBelow')
BEGIN
	ALTER TABLE [Product]
	ADD [NotifyAdminForQuantityBelow] int  NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='BackorderModeId')
BEGIN
	ALTER TABLE [Product]
	ADD [BackorderModeId] int NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='AllowBackInStockSubscriptions')
BEGIN
	ALTER TABLE [Product]
	ADD [AllowBackInStockSubscriptions] bit NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='OrderMinimumQuantity')
BEGIN
	ALTER TABLE [Product]
	ADD [OrderMinimumQuantity] int NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='OrderMaximumQuantity')
BEGIN
	ALTER TABLE [Product]
	ADD [OrderMaximumQuantity] int NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='AllowedQuantities')
BEGIN
	ALTER TABLE [Product]
	ADD [AllowedQuantities] nvarchar(1000) NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='DisableBuyButton')
BEGIN
	ALTER TABLE [Product]
	ADD [DisableBuyButton] bit NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='DisableWishlistButton')
BEGIN
	ALTER TABLE [Product]
	ADD [DisableWishlistButton] bit NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='AvailableForPreOrder')
BEGIN
	ALTER TABLE [Product]
	ADD [AvailableForPreOrder] bit NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='CallForPrice')
BEGIN
	ALTER TABLE [Product]
	ADD [CallForPrice] bit NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='Price')
BEGIN
	ALTER TABLE [Product]
	ADD [Price] decimal(18, 4) NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='OldPrice')
BEGIN
	ALTER TABLE [Product]
	ADD [OldPrice] decimal(18, 4) NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='ProductCost')
BEGIN
	ALTER TABLE [Product]
	ADD [ProductCost] decimal(18, 4)  NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='SpecialPrice')
BEGIN
	ALTER TABLE [Product]
	ADD [SpecialPrice] decimal(18, 4) NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='SpecialPriceStartDateTimeUtc')
BEGIN
	ALTER TABLE [Product]
	ADD [SpecialPriceStartDateTimeUtc] datetime NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='SpecialPriceEndDateTimeUtc')
BEGIN
	ALTER TABLE [Product]
	ADD [SpecialPriceEndDateTimeUtc] datetime NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='CustomerEntersPrice')
BEGIN
	ALTER TABLE [Product]
	ADD [CustomerEntersPrice] bit NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='MinimumCustomerEnteredPrice')
BEGIN
	ALTER TABLE [Product]
	ADD [MinimumCustomerEnteredPrice] decimal(18, 4) NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='MaximumCustomerEnteredPrice')
BEGIN
	ALTER TABLE [Product]
	ADD [MaximumCustomerEnteredPrice] decimal(18, 4) NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='HasTierPrices')
BEGIN
	ALTER TABLE [Product]
	ADD [HasTierPrices] bit NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='HasDiscountsApplied')
BEGIN
	ALTER TABLE [Product]
	ADD [HasDiscountsApplied] bit NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='Weight')
BEGIN
	ALTER TABLE [Product]
	ADD [Weight] decimal(18, 4) NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='Length')
BEGIN
	ALTER TABLE [Product]
	ADD [Length] decimal(18, 4) NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='Width')
BEGIN
	ALTER TABLE [Product]
	ADD [Width] decimal(18, 4) NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='Height')
BEGIN
	ALTER TABLE [Product]
	ADD [Height] decimal(18, 4) NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='AvailableStartDateTimeUtc')
BEGIN
	ALTER TABLE [Product]
	ADD [AvailableStartDateTimeUtc] datetime NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='AvailableEndDateTimeUtc')
BEGIN
	ALTER TABLE [Product]
	ADD [AvailableEndDateTimeUtc] datetime NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='DeliveryTimeId')
BEGIN
	ALTER TABLE [Product]
	ADD [DeliveryTimeId] int NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='BasePriceEnabled')
BEGIN
	ALTER TABLE [Product]
	ADD [BasePriceEnabled] bit NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='BasePriceMeasureUnit')
BEGIN
	ALTER TABLE [Product]
	ADD [BasePriceMeasureUnit] nvarchar(50) NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='BasePriceAmount')
BEGIN
	ALTER TABLE [Product]
	ADD [BasePriceAmount] decimal(18, 4) NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='BasePriceBaseAmount')
BEGIN
	ALTER TABLE [Product]
	ADD [BasePriceBaseAmount] int NULL
END
GO

--remove old product variant references
IF EXISTS (SELECT 1
           FROM   sys.objects
           WHERE  name = 'BackInStockSubscription_ProductVariant'
           AND parent_object_id = Object_id('BackInStockSubscription')
           AND Objectproperty(object_id,N'IsForeignKey') = 1)
BEGIN
	ALTER TABLE dbo.[BackInStockSubscription]
	DROP CONSTRAINT BackInStockSubscription_ProductVariant
END
GO

IF EXISTS (SELECT 1
           FROM   sys.objects
           WHERE  name = 'OrderItem_ProductVariant'
           AND parent_object_id = Object_id('OrderItem')
           AND Objectproperty(object_id,N'IsForeignKey') = 1)
BEGIN
	ALTER TABLE dbo.[OrderItem]
	DROP CONSTRAINT OrderItem_ProductVariant
END
GO

IF EXISTS (SELECT 1
           FROM   sys.objects
           WHERE  name = 'ProductVariantAttribute_ProductVariant'
           AND parent_object_id = Object_id('ProductVariant_ProductAttribute_Mapping')
           AND Objectproperty(object_id,N'IsForeignKey') = 1)
BEGIN
	ALTER TABLE dbo.[ProductVariant_ProductAttribute_Mapping]
	DROP CONSTRAINT ProductVariantAttribute_ProductVariant
END
GO

IF EXISTS (SELECT 1
           FROM   sys.objects
           WHERE  name = 'ProductVariantAttributeCombination_ProductVariant'
           AND parent_object_id = Object_id('ProductVariantAttributeCombination')
           AND Objectproperty(object_id,N'IsForeignKey') = 1)
BEGIN
	ALTER TABLE dbo.[ProductVariantAttributeCombination]
	DROP CONSTRAINT ProductVariantAttributeCombination_ProductVariant
END
GO

IF EXISTS (SELECT 1
           FROM   sys.objects
           WHERE  name = 'ShoppingCartItem_ProductVariant'
           AND parent_object_id = Object_id('ShoppingCartItem')
           AND Objectproperty(object_id,N'IsForeignKey') = 1)
BEGIN
	ALTER TABLE dbo.[ShoppingCartItem]
	DROP CONSTRAINT ShoppingCartItem_ProductVariant
END
GO

IF EXISTS (SELECT 1
           FROM   sys.objects
           WHERE  name = 'TierPrice_ProductVariant'
           AND parent_object_id = Object_id('TierPrice')
           AND Objectproperty(object_id,N'IsForeignKey') = 1)
BEGIN
	ALTER TABLE dbo.[TierPrice]
	DROP CONSTRAINT TierPrice_ProductVariant
END
GO

IF EXISTS (SELECT 1
           FROM   sys.objects
           WHERE  name = 'Discount_AppliedToProductVariants_Target'
           AND parent_object_id = Object_id('Discount_AppliedToProductVariants')
           AND Objectproperty(object_id,N'IsForeignKey') = 1)
BEGIN
	ALTER TABLE dbo.[Discount_AppliedToProductVariants]
	DROP CONSTRAINT Discount_AppliedToProductVariants_Target
END
GO

--new ProductId columns in references tables
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[BackInStockSubscription]') and NAME='ProductId')
BEGIN
	ALTER TABLE [BackInStockSubscription]
	ADD [ProductId] int NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[OrderItem]') and NAME='ProductId')
BEGIN
	ALTER TABLE [OrderItem]
	ADD [ProductId] int NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[ProductVariant_ProductAttribute_Mapping]') and NAME='ProductId')
BEGIN
	--one more validatation here because we'll rename [ProductVariant_ProductAttribute_Mapping] table a bit later
	IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Product_ProductAttribute_Mapping]') and OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
	BEGIN
		ALTER TABLE [ProductVariant_ProductAttribute_Mapping]
		ADD [ProductId] int NULL
	END
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[ProductVariantAttributeCombination]') and NAME='ProductId')
BEGIN
	ALTER TABLE [ProductVariantAttributeCombination]
	ADD [ProductId] int NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[ShoppingCartItem]') and NAME='ProductId')
BEGIN
	ALTER TABLE [ShoppingCartItem]
	ADD [ProductId] int NULL
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[TierPrice]') and NAME='ProductId')
BEGIN
	ALTER TABLE [TierPrice]
	ADD [ProductId] int NULL
END
GO
--new table for discount <=> product mapping (have some issue with just adding and renaming columns)
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Discount_AppliedToProducts]') and OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	CREATE TABLE [dbo].[Discount_AppliedToProducts](
	[Discount_Id] [int] NOT NULL,
	[Product_Id] [int] NOT NULL,
	[ProductVariant_Id] [int] NOT NULL,
		PRIMARY KEY CLUSTERED 
		(
			[Discount_Id] ASC,
			[Product_Id] ASC
		)
	)
	
	--copy records
	DECLARE @ExistingDiscountID int
	DECLARE @ExistingDiscountProductVariantID int
	DECLARE cur_existingdiscountmapping CURSOR FOR
	SELECT [Discount_Id], [ProductVariant_Id]
	FROM [Discount_AppliedToProductVariants]
	OPEN cur_existingdiscountmapping
	FETCH NEXT FROM cur_existingdiscountmapping INTO @ExistingDiscountID,@ExistingDiscountProductVariantID
	WHILE @@FETCH_STATUS = 0
	BEGIN
		EXEC sp_executesql N'INSERT INTO [Discount_AppliedToProducts] ([Discount_Id], [Product_Id], [ProductVariant_Id])
		VALUES (@ExistingDiscountID, @ExistingDiscountProductVariantID, @ExistingDiscountProductVariantID)',
		N'@ExistingDiscountID int, 
		@ExistingDiscountProductVariantID int',
		@ExistingDiscountID,
		@ExistingDiscountProductVariantID
		
		--fetch next identifier
		FETCH NEXT FROM cur_existingdiscountmapping INTO @ExistingDiscountID,@ExistingDiscountProductVariantID
	END
	
	CLOSE cur_existingdiscountmapping
	DEALLOCATE cur_existingdiscountmapping
	
	--drop old table
	DROP TABLE [Discount_AppliedToProductVariants]
END
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[ProductVariant]') and OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	DECLARE @ExistingProductVariantID int
	DECLARE cur_existingproductvariant CURSOR FOR
	SELECT [ID]
	FROM [ProductVariant]
	OPEN cur_existingproductvariant
	FETCH NEXT FROM cur_existingproductvariant INTO @ExistingProductVariantID
	WHILE @@FETCH_STATUS = 0
	BEGIN
		DECLARE @ProductId int
		SET @ProductId = null -- clear cache (variable scope)
		DECLARE @Name nvarchar(400)
		SET @Name = null -- clear cache (variable scope)
		DECLARE @Description nvarchar(MAX)
		SET @Description = null -- clear cache (variable scope)
		DECLARE @Sku nvarchar(400)
		SET @Sku = null -- clear cache (variable scope)
		DECLARE @ManufacturerPartNumber nvarchar(400)
		SET @ManufacturerPartNumber = null -- clear cache (variable scope)
		DECLARE @Gtin nvarchar(400)
		SET @Gtin = null -- clear cache (variable scope)
		DECLARE @IsGiftCard bit
		SET @IsGiftCard = null -- clear cache (variable scope)
		DECLARE @GiftCardTypeId int
		SET @GiftCardTypeId = null -- clear cache (variable scope)
		DECLARE @RequireOtherProducts bit
		SET @RequireOtherProducts = null -- clear cache (variable scope)
		DECLARE @RequiredProductIds nvarchar(1000)
		SET @RequiredProductIds = null -- clear cache (variable scope)
		DECLARE @AutomaticallyAddRequiredProducts bit
		SET @AutomaticallyAddRequiredProducts = null -- clear cache (variable scope)
		DECLARE @IsDownload bit
		SET @IsDownload = null -- clear cache (variable scope)
		DECLARE @DownloadId int
		SET @DownloadId = null -- clear cache (variable scope)
		DECLARE @UnlimitedDownloads bit
		SET @UnlimitedDownloads = null -- clear cache (variable scope)
		DECLARE @MaxNumberOfDownloads int
		SET @MaxNumberOfDownloads = null -- clear cache (variable scope)
		DECLARE @DownloadExpirationDays int
		SET @DownloadExpirationDays = null -- clear cache (variable scope)
		DECLARE @DownloadActivationTypeId int
		SET @DownloadActivationTypeId = null -- clear cache (variable scope)
		DECLARE @HasSampleDownload bit
		SET @HasSampleDownload = null -- clear cache (variable scope)
		DECLARE @SampleDownloadId int
		SET @SampleDownloadId = null -- clear cache (variable scope)
		DECLARE @HasUserAgreement bit
		SET @HasUserAgreement = null -- clear cache (variable scope)
		DECLARE @UserAgreementText nvarchar(MAX)
		SET @UserAgreementText = null -- clear cache (variable scope)
		DECLARE @IsRecurring bit
		SET @IsRecurring = null -- clear cache (variable scope)
		DECLARE @RecurringCycleLength int
		SET @RecurringCycleLength = null -- clear cache (variable scope)
		DECLARE @RecurringCyclePeriodId int
		SET @RecurringCyclePeriodId = null -- clear cache (variable scope)
		DECLARE @RecurringTotalCycles int
		SET @RecurringTotalCycles = null -- clear cache (variable scope)
		DECLARE @IsShipEnabled bit
		SET @IsShipEnabled = null -- clear cache (variable scope)
		DECLARE @IsFreeShipping bit
		SET @IsFreeShipping = null -- clear cache (variable scope)
		DECLARE @AdditionalShippingCharge decimal(18,4)
		SET @AdditionalShippingCharge = null -- clear cache (variable scope)
		DECLARE @IsTaxExempt bit
		SET @IsTaxExempt = null -- clear cache (variable scope)
		DECLARE @TaxCategoryId int
		SET @TaxCategoryId = null -- clear cache (variable scope)
		DECLARE @ManageInventoryMethodId int
		SET @ManageInventoryMethodId = null -- clear cache (variable scope)
		DECLARE @StockQuantity int
		SET @StockQuantity = null -- clear cache (variable scope)
		DECLARE @DisplayStockAvailability bit
		SET @DisplayStockAvailability = null -- clear cache (variable scope)
		DECLARE @DisplayStockQuantity bit
		SET @DisplayStockQuantity = null -- clear cache (variable scope)
		DECLARE @MinStockQuantity int
		SET @MinStockQuantity = null -- clear cache (variable scope)
		DECLARE @LowStockActivityId int
		SET @LowStockActivityId = null -- clear cache (variable scope)
		DECLARE @NotifyAdminForQuantityBelow int
		SET @NotifyAdminForQuantityBelow = null -- clear cache (variable scope)
		DECLARE @BackorderModeId int
		SET @BackorderModeId = null -- clear cache (variable scope)
		DECLARE @AllowBackInStockSubscriptions bit
		SET @AllowBackInStockSubscriptions = null -- clear cache (variable scope)
		DECLARE @OrderMinimumQuantity int
		SET @OrderMinimumQuantity = null -- clear cache (variable scope)
		DECLARE @OrderMaximumQuantity int
		SET @OrderMaximumQuantity = null -- clear cache (variable scope)
		DECLARE @AllowedQuantities nvarchar(1000)
		SET @AllowedQuantities = null -- clear cache (variable scope)
		DECLARE @DisableBuyButton bit
		SET @DisableBuyButton = null -- clear cache (variable scope)
		DECLARE @DisableWishlistButton bit
		SET @DisableWishlistButton = null -- clear cache (variable scope)
		DECLARE @AvailableForPreOrder bit
		SET @AvailableForPreOrder = null -- clear cache (variable scope)
		DECLARE @CallForPrice bit
		SET @CallForPrice = null -- clear cache (variable scope)
		DECLARE @Price decimal(18,4)
		SET @Price = null -- clear cache (variable scope)
		DECLARE @OldPrice decimal(18,4)
		SET @OldPrice = null -- clear cache (variable scope)
		DECLARE @ProductCost decimal(18,4)
		SET @ProductCost = null -- clear cache (variable scope)
		DECLARE @SpecialPrice decimal(18,4)
		SET @SpecialPrice = null -- clear cache (variable scope)
		DECLARE @SpecialPriceStartDateTimeUtc datetime
		SET @SpecialPriceStartDateTimeUtc = null -- clear cache (variable scope)
		DECLARE @SpecialPriceEndDateTimeUtc datetime
		SET @SpecialPriceEndDateTimeUtc = null -- clear cache (variable scope)
		DECLARE @CustomerEntersPrice bit
		SET @CustomerEntersPrice = null -- clear cache (variable scope)
		DECLARE @MinimumCustomerEnteredPrice decimal(18,4)
		SET @MinimumCustomerEnteredPrice = null -- clear cache (variable scope)
		DECLARE @MaximumCustomerEnteredPrice decimal(18,4)
		SET @MaximumCustomerEnteredPrice = null -- clear cache (variable scope)
		DECLARE @HasTierPrices bit
		SET @HasTierPrices = null -- clear cache (variable scope)
		DECLARE @HasDiscountsApplied bit
		SET @HasDiscountsApplied = null -- clear cache (variable scope)
		DECLARE @Weight decimal(18, 4)
		SET @Weight = null -- clear cache (variable scope)
		DECLARE @Length decimal(18, 4)
		SET @Length = null -- clear cache (variable scope)
		DECLARE @Width decimal(18, 4)
		SET @Width = null -- clear cache (variable scope)
		DECLARE @Height decimal(18, 4)
		SET @Height = null -- clear cache (variable scope)
		DECLARE @PictureId int
		SET @PictureId = null -- clear cache (variable scope)
		DECLARE @AvailableStartDateTimeUtc datetime
		SET @AvailableStartDateTimeUtc = null -- clear cache (variable scope)
		DECLARE @AvailableEndDateTimeUtc datetime
		SET @AvailableEndDateTimeUtc = null -- clear cache (variable scope)
		DECLARE @Published bit
		SET @Published = null -- clear cache (variable scope)
		DECLARE @Deleted bit
		SET @Deleted = null -- clear cache (variable scope)
		DECLARE @DisplayOrder int
		SET @DisplayOrder = null -- clear cache (variable scope)
		DECLARE @CreatedOnUtc datetime
		SET @CreatedOnUtc = null -- clear cache (variable scope)
		DECLARE @UpdatedOnUtc datetime
		SET @UpdatedOnUtc = null -- clear cache (variable scope)
		DECLARE @DeliveryTimeId int
		SET @DeliveryTimeId = null
		DECLARE @BasePriceEnabled bit
		SET @BasePriceEnabled = null
		DECLARE @BasePriceMeasureUnit nvarchar(50)
		SET @BasePriceMeasureUnit = null
		DECLARE @BasePriceAmount decimal(18, 4)
		SET @BasePriceAmount = null
		DECLARE @BasePriceBaseAmount int
		SET @BasePriceBaseAmount = null

		DECLARE @sql nvarchar(4000)
		SET @sql = 'SELECT 
		@ProductId = [ProductId],
		@Name = [Name],
		@Description = [Description],
		@Sku = [Sku],
		@ManufacturerPartNumber = [ManufacturerPartNumber],
		@Gtin = [Gtin],
		@IsGiftCard = [IsGiftCard],
		@GiftCardTypeId = [GiftCardTypeId],
		@RequireOtherProducts = [RequireOtherProducts],
		@RequiredProductIds= [RequiredProductVariantIds],
		@AutomaticallyAddRequiredProducts = [AutomaticallyAddRequiredProductVariants],
		@IsDownload = [IsDownload],
		@DownloadId = [DownloadId],
		@UnlimitedDownloads = [UnlimitedDownloads],
		@MaxNumberOfDownloads = [MaxNumberOfDownloads],
		@DownloadExpirationDays = [DownloadExpirationDays],
		@DownloadActivationTypeId = [DownloadActivationTypeId],
		@HasSampleDownload = [HasSampleDownload],
		@SampleDownloadId = [SampleDownloadId],
		@HasUserAgreement = [HasUserAgreement],
		@UserAgreementText = [UserAgreementText],
		@IsRecurring = [IsRecurring],
		@RecurringCycleLength = [RecurringCycleLength],
		@RecurringCyclePeriodId = [RecurringCyclePeriodId],
		@RecurringTotalCycles = [RecurringTotalCycles],
		@IsShipEnabled = [IsShipEnabled],
		@IsFreeShipping = [IsFreeShipping],
		@AdditionalShippingCharge = [AdditionalShippingCharge],
		@IsTaxExempt = [IsTaxExempt],
		@TaxCategoryId = [TaxCategoryId],
		@ManageInventoryMethodId = [ManageInventoryMethodId],
		@StockQuantity = [StockQuantity],
		@DisplayStockAvailability = [DisplayStockAvailability],
		@DisplayStockQuantity = [DisplayStockQuantity],
		@MinStockQuantity = [MinStockQuantity],
		@LowStockActivityId = [LowStockActivityId],
		@NotifyAdminForQuantityBelow = [NotifyAdminForQuantityBelow],
		@BackorderModeId = [BackorderModeId],
		@AllowBackInStockSubscriptions = [AllowBackInStockSubscriptions],
		@OrderMinimumQuantity = [OrderMinimumQuantity],
		@OrderMaximumQuantity = [OrderMaximumQuantity],
		@AllowedQuantities = [AllowedQuantities],
		@DisableBuyButton = [DisableBuyButton],
		@DisableWishlistButton = [DisableWishlistButton],
		@AvailableForPreOrder = [AvailableForPreOrder],
		@CallForPrice = [CallForPrice],
		@Price = [Price],
		@OldPrice = [OldPrice],
		@ProductCost = [ProductCost],
		@SpecialPrice = [SpecialPrice],
		@SpecialPriceStartDateTimeUtc = [SpecialPriceStartDateTimeUtc],
		@SpecialPriceEndDateTimeUtc = [SpecialPriceEndDateTimeUtc],
		@CustomerEntersPrice = [CustomerEntersPrice],
		@MinimumCustomerEnteredPrice = [MinimumCustomerEnteredPrice],
		@MaximumCustomerEnteredPrice = [MaximumCustomerEnteredPrice],
		@HasTierPrices = [HasTierPrices],
		@HasDiscountsApplied = [HasDiscountsApplied],
		@Weight = [Weight],
		@Length = [Length],
		@Width = [Width],
		@Height = [Height],
		@PictureId = [PictureId],
		@AvailableStartDateTimeUtc = [AvailableStartDateTimeUtc],
		@AvailableEndDateTimeUtc = [AvailableEndDateTimeUtc],
		@Published = [Published],
		@Deleted = [Deleted],
		@DisplayOrder = [DisplayOrder],
		@CreatedOnUtc = [CreatedOnUtc],
		@UpdatedOnUtc = [UpdatedOnUtc],
		@DeliveryTimeId = [DeliveryTimeId],
		@BasePriceEnabled = [BasePriceEnabled],
		@BasePriceMeasureUnit = [BasePriceMeasureUnit],
		@BasePriceAmount = [BasePriceAmount],
		@BasePriceBaseAmount = [BasePriceBaseAmount]
		FROM [ProductVariant] 
		WHERE [Id]=' + ISNULL(CAST(@ExistingProductVariantID AS nvarchar(max)), '0')

		EXEC sp_executesql @sql,
		N'@ProductId int OUTPUT, 
		@Name nvarchar(400) OUTPUT,
		@Description nvarchar(MAX) OUTPUT,
		@Sku nvarchar(400) OUTPUT, 
		@ManufacturerPartNumber nvarchar(400) OUTPUT,
		@Gtin nvarchar(400) OUTPUT,
		@IsGiftCard bit OUTPUT, 
		@GiftCardTypeId int OUTPUT, 
		@RequireOtherProducts bit OUTPUT, 
		@RequiredProductIds nvarchar(1000) OUTPUT, 
		@AutomaticallyAddRequiredProducts bit OUTPUT, 
		@IsDownload bit OUTPUT, 
		@DownloadId int OUTPUT, 
		@UnlimitedDownloads bit OUTPUT, 
		@MaxNumberOfDownloads int OUTPUT, 
		@DownloadExpirationDays int OUTPUT, 
		@DownloadActivationTypeId int OUTPUT, 
		@HasSampleDownload bit OUTPUT, 
		@SampleDownloadId int OUTPUT, 
		@HasUserAgreement bit OUTPUT, 
		@UserAgreementText nvarchar(MAX) OUTPUT, 
		@IsRecurring bit OUTPUT, 
		@RecurringCycleLength int OUTPUT, 
		@RecurringCyclePeriodId int OUTPUT, 
		@RecurringTotalCycles int OUTPUT, 
		@IsShipEnabled bit OUTPUT, 
		@IsFreeShipping bit OUTPUT, 
		@AdditionalShippingCharge decimal(18,4) OUTPUT, 
		@IsTaxExempt bit OUTPUT, 
		@TaxCategoryId int OUTPUT, 
		@ManageInventoryMethodId int OUTPUT, 
		@StockQuantity int OUTPUT, 
		@DisplayStockAvailability bit OUTPUT, 
		@DisplayStockQuantity bit OUTPUT, 
		@MinStockQuantity int OUTPUT, 
		@LowStockActivityId int OUTPUT, 
		@NotifyAdminForQuantityBelow int OUTPUT, 
		@BackorderModeId int OUTPUT, 
		@AllowBackInStockSubscriptions bit OUTPUT, 
		@OrderMinimumQuantity int OUTPUT, 
		@OrderMaximumQuantity int OUTPUT, 
		@AllowedQuantities nvarchar(1000) OUTPUT, 
		@DisableBuyButton bit OUTPUT, 
		@DisableWishlistButton bit OUTPUT, 
		@AvailableForPreOrder bit OUTPUT, 
		@CallForPrice bit OUTPUT, 
		@Price decimal(18,4) OUTPUT, 
		@OldPrice decimal(18,4) OUTPUT,
		@ProductCost decimal(18,4) OUTPUT, 
		@SpecialPrice decimal(18,4) OUTPUT, 
		@SpecialPriceStartDateTimeUtc datetime OUTPUT, 
		@SpecialPriceEndDateTimeUtc datetime OUTPUT, 
		@CustomerEntersPrice bit OUTPUT, 
		@MinimumCustomerEnteredPrice decimal(18,4) OUTPUT, 
		@MaximumCustomerEnteredPrice bit OUTPUT, 
		@HasTierPrices bit OUTPUT,
		@HasDiscountsApplied bit OUTPUT,
		@Weight decimal(18, 4) OUTPUT,
		@Length decimal(18, 4) OUTPUT,
		@Width decimal(18, 4) OUTPUT,
		@Height decimal(18, 4) OUTPUT,
		@PictureId int OUTPUT,
		@AvailableStartDateTimeUtc datetime OUTPUT,
		@AvailableEndDateTimeUtc datetime OUTPUT,
		@Published bit OUTPUT,
		@Deleted bit OUTPUT,
		@DisplayOrder int OUTPUT,
		@CreatedOnUtc datetime OUTPUT,
		@UpdatedOnUtc datetime OUTPUT,
		@DeliveryTimeId int OUTPUT,
		@BasePriceEnabled bit OUTPUT,
		@BasePriceMeasureUnit nvarchar(50) OUTPUT,
		@BasePriceAmount decimal(18, 4) OUTPUT,
		@BasePriceBaseAmount int OUTPUT',
		@ProductId OUTPUT,
		@Name OUTPUT,
		@Description OUTPUT,
		@Sku OUTPUT,
		@ManufacturerPartNumber OUTPUT,
		@Gtin OUTPUT,
		@IsGiftCard OUTPUT,
		@GiftCardTypeId OUTPUT,
		@RequireOtherProducts OUTPUT,
		@RequiredProductIds OUTPUT,
		@AutomaticallyAddRequiredProducts OUTPUT,
		@IsDownload OUTPUT,
		@DownloadId OUTPUT,
		@UnlimitedDownloads OUTPUT,
		@MaxNumberOfDownloads OUTPUT,
		@DownloadExpirationDays OUTPUT,
		@DownloadActivationTypeId OUTPUT,
		@HasSampleDownload OUTPUT,
		@SampleDownloadId OUTPUT,
		@HasUserAgreement OUTPUT,
		@UserAgreementText OUTPUT,
		@IsRecurring OUTPUT,
		@RecurringCycleLength OUTPUT,
		@RecurringCyclePeriodId OUTPUT,
		@RecurringTotalCycles OUTPUT,
		@IsShipEnabled OUTPUT,
		@IsFreeShipping OUTPUT,
		@AdditionalShippingCharge OUTPUT,
		@IsTaxExempt OUTPUT,
		@TaxCategoryId OUTPUT,
		@ManageInventoryMethodId OUTPUT,
		@StockQuantity OUTPUT,
		@DisplayStockAvailability OUTPUT,
		@DisplayStockQuantity OUTPUT,
		@MinStockQuantity OUTPUT,
		@LowStockActivityId OUTPUT,
		@NotifyAdminForQuantityBelow OUTPUT,
		@BackorderModeId OUTPUT,
		@AllowBackInStockSubscriptions OUTPUT,
		@OrderMinimumQuantity OUTPUT,
		@OrderMaximumQuantity OUTPUT,
		@AllowedQuantities OUTPUT,
		@DisableBuyButton OUTPUT,
		@DisableWishlistButton OUTPUT,
		@AvailableForPreOrder OUTPUT,
		@CallForPrice OUTPUT,
		@Price OUTPUT,
		@OldPrice OUTPUT,
		@ProductCost OUTPUT,
		@SpecialPrice OUTPUT,
		@SpecialPriceStartDateTimeUtc OUTPUT,
		@SpecialPriceEndDateTimeUtc OUTPUT,
		@CustomerEntersPrice OUTPUT,
		@MinimumCustomerEnteredPrice OUTPUT,
		@MaximumCustomerEnteredPrice OUTPUT,
		@HasTierPrices OUTPUT,
		@HasDiscountsApplied OUTPUT,
		@Weight OUTPUT,
		@Length OUTPUT,
		@Width OUTPUT,
		@Height OUTPUT,
		@PictureId OUTPUT,
		@AvailableStartDateTimeUtc OUTPUT,
		@AvailableEndDateTimeUtc OUTPUT,
		@Published OUTPUT,
		@Deleted OUTPUT,
		@DisplayOrder OUTPUT,
		@CreatedOnUtc OUTPUT,
		@UpdatedOnUtc OUTPUT,
		@DeliveryTimeId OUTPUT,
		@BasePriceEnabled OUTPUT,
		@BasePriceMeasureUnit OUTPUT,
		@BasePriceAmount OUTPUT,
		@BasePriceBaseAmount OUTPUT
		
		--how many variants do we have?
		DECLARE @NumberOfVariants int
		SELECT @NumberOfVariants = COUNT(1) FROM [ProductVariant] WHERE [ProductId]=@ProductId And [Deleted] = 0
		
		DECLARE @NumberOfAllVariants int
		SELECT @NumberOfAllVariants = COUNT(1) FROM [ProductVariant] WHERE [ProductId]=@ProductId
		
		--product templates
		DECLARE @SimpleProductTemplateId int
		SELECT @SimpleProductTemplateId = [Id] FROM [ProductTemplate] WHERE [ViewPath] = N'ProductTemplate.Simple'
		DECLARE @GroupedProductTemplateId int
		SELECT @GroupedProductTemplateId = [Id] FROM [ProductTemplate] WHERE [ViewPath] = N'ProductTemplate.Grouped'
		
		--new product id:
		--if we have a simple product it'll be the same
		--if we have a grouped product, then it'll be the identifier of a new associated product 
		DECLARE @NewProductId int
		SET @NewProductId = null -- clear cache (variable scope)
			
		--process a product (simple or grouped)
		IF (@NumberOfVariants <= 1)
		BEGIN
			--simple product
			UPDATE [Product] 
			SET [ProductTypeId] = 5,
			[ParentGroupedProductId] = 0,
			[Sku] = @Sku,
			[ManufacturerPartNumber] = @ManufacturerPartNumber,
			[Gtin] = @Gtin,
			[IsGiftCard] = @IsGiftCard,
			[GiftCardTypeId] = @GiftCardTypeId,
			[RequireOtherProducts] = @RequireOtherProducts,
			--a store owner should manually update [RequiredProductIds] property after upgrade
			--[RequiredProductIds] = @RequiredProductIds,
			[AutomaticallyAddRequiredProducts] = @AutomaticallyAddRequiredProducts,
			[IsDownload] = @IsDownload,
			[DownloadId] = @DownloadId,
			[UnlimitedDownloads] = @UnlimitedDownloads,
			[MaxNumberOfDownloads] = @MaxNumberOfDownloads,
			[DownloadExpirationDays] = @DownloadExpirationDays,
			[DownloadActivationTypeId] = @DownloadActivationTypeId,
			[HasSampleDownload] = @HasSampleDownload,
			[SampleDownloadId] = @SampleDownloadId,
			[HasUserAgreement] = @HasUserAgreement,
			[UserAgreementText] = @UserAgreementText,
			[IsRecurring] = @IsRecurring,
			[RecurringCycleLength] = @RecurringCycleLength,
			[RecurringCyclePeriodId] = @RecurringCyclePeriodId,
			[RecurringTotalCycles] = @RecurringTotalCycles,
			[IsShipEnabled] = @IsShipEnabled,
			[IsFreeShipping] = @IsFreeShipping,
			[AdditionalShippingCharge] = @AdditionalShippingCharge,
			[IsTaxExempt] = @IsTaxExempt,
			[TaxCategoryId] = @TaxCategoryId,
			[ManageInventoryMethodId] = @ManageInventoryMethodId,
			[StockQuantity] = @StockQuantity,
			[DisplayStockAvailability] = @DisplayStockAvailability,
			[DisplayStockQuantity] = @DisplayStockQuantity,
			[MinStockQuantity] = @MinStockQuantity,
			[LowStockActivityId] = @LowStockActivityId,
			[NotifyAdminForQuantityBelow] = @NotifyAdminForQuantityBelow,
			[BackorderModeId] = @BackorderModeId,
			[AllowBackInStockSubscriptions] = @AllowBackInStockSubscriptions,
			[OrderMinimumQuantity] = @OrderMinimumQuantity,
			[OrderMaximumQuantity] = @OrderMaximumQuantity,
			[AllowedQuantities] = @AllowedQuantities,
			[DisableBuyButton] = @DisableBuyButton,
			[DisableWishlistButton] = @DisableWishlistButton,
			[AvailableForPreOrder] = @AvailableForPreOrder,
			[CallForPrice] = @CallForPrice,
			[Price] = @Price,
			[OldPrice] = @OldPrice,
			[ProductCost] = @ProductCost,
			[SpecialPrice] = @SpecialPrice,
			[SpecialPriceStartDateTimeUtc] = @SpecialPriceStartDateTimeUtc,
			[SpecialPriceEndDateTimeUtc] = @SpecialPriceEndDateTimeUtc,
			[CustomerEntersPrice] = @CustomerEntersPrice,
			[MinimumCustomerEnteredPrice] = @MinimumCustomerEnteredPrice,
			[MaximumCustomerEnteredPrice] = @MaximumCustomerEnteredPrice,
			[HasTierPrices] = @HasTierPrices,
			[HasDiscountsApplied] = @HasDiscountsApplied,
			[Weight] = @Weight,
			[Length] = @Length,
			[Width] = @Width,
			[Height] = @Height,
			[AvailableStartDateTimeUtc] = @AvailableStartDateTimeUtc,
			[AvailableEndDateTimeUtc] = @AvailableEndDateTimeUtc,
			[DeliveryTimeId] = @DeliveryTimeId,
			[BasePriceEnabled] = @BasePriceEnabled,
			[BasePriceMeasureUnit] = @BasePriceMeasureUnit,
			[BasePriceAmount] = @BasePriceAmount,
			[BasePriceBaseAmount] = @BasePriceBaseAmount
			WHERE [Id]=@ProductId
			
			--product type
			UPDATE [Product]
			SET [ProductTypeId]=5
			WHERE [Id]=@ProductId
			
			--product template
			UPDATE [Product]
			SET [ProductTemplateId]=@SimpleProductTemplateId
			WHERE [Id]=@ProductId
			
			--deleted?
			IF (@Deleted = 1 And @NumberOfAllVariants <= 1)
			BEGIN
				UPDATE [Product]
				SET [Deleted]=@Deleted
				WHERE [Id]=@ProductId
			END
			
			--published?
			IF (@Published = 0 And @NumberOfAllVariants <= 1)
			BEGIN
				UPDATE [Product]
				SET [Published]=@Published
				WHERE [Id]=@ProductId
			END
			
			SET @NewProductId = @ProductId
		END ELSE 
		BEGIN
			--grouped product
			UPDATE [Product] 
			SET [ProductTypeId] = 10,
			[ParentGroupedProductId] = 0,
			[Sku] = null,
			[ManufacturerPartNumber] = null,
			[Gtin] = null,
			[IsGiftCard] = 0,
			[GiftCardTypeId] = 0,
			[RequireOtherProducts] = 0,
			[RequiredProductIds] = null,
			[AutomaticallyAddRequiredProducts] = 0,
			[IsDownload] = 0,
			[DownloadId] = 0,
			[UnlimitedDownloads] = @UnlimitedDownloads,
			[MaxNumberOfDownloads] = @MaxNumberOfDownloads,
			[DownloadExpirationDays] = @DownloadExpirationDays,
			[DownloadActivationTypeId] = @DownloadActivationTypeId,
			[HasSampleDownload] = 0,
			[SampleDownloadId] = 0,
			[HasUserAgreement] = @HasUserAgreement,
			[UserAgreementText] = @UserAgreementText,
			[IsRecurring] = @IsRecurring,
			[RecurringCycleLength] = @RecurringCycleLength,
			[RecurringCyclePeriodId] = @RecurringCyclePeriodId,
			[RecurringTotalCycles] = @RecurringTotalCycles,
			[IsShipEnabled] = @IsShipEnabled,
			[IsFreeShipping] = @IsFreeShipping,
			[AdditionalShippingCharge] = @AdditionalShippingCharge,
			[IsTaxExempt] = @IsTaxExempt,
			[TaxCategoryId] = @TaxCategoryId,
			[ManageInventoryMethodId] = @ManageInventoryMethodId,
			[StockQuantity] = @StockQuantity,
			[DisplayStockAvailability] = @DisplayStockAvailability,
			[DisplayStockQuantity] = @DisplayStockQuantity,
			[MinStockQuantity] = @MinStockQuantity,
			[LowStockActivityId] = @LowStockActivityId,
			[NotifyAdminForQuantityBelow] = @NotifyAdminForQuantityBelow,
			[BackorderModeId] = @BackorderModeId,
			[AllowBackInStockSubscriptions] = @AllowBackInStockSubscriptions,
			[OrderMinimumQuantity] = @OrderMinimumQuantity,
			[OrderMaximumQuantity] = @OrderMaximumQuantity,
			[AllowedQuantities] = @AllowedQuantities,
			[DisableBuyButton] = @DisableBuyButton,
			[DisableWishlistButton] = @DisableWishlistButton,
			[AvailableForPreOrder] = @AvailableForPreOrder,
			[CallForPrice] = @CallForPrice,
			[Price] = @Price,
			[OldPrice] = @OldPrice,
			[ProductCost] = @ProductCost,
			[SpecialPrice] = @SpecialPrice,
			[SpecialPriceStartDateTimeUtc] = @SpecialPriceStartDateTimeUtc,
			[SpecialPriceEndDateTimeUtc] = @SpecialPriceEndDateTimeUtc,
			[CustomerEntersPrice] = @CustomerEntersPrice,
			[MinimumCustomerEnteredPrice] = @MinimumCustomerEnteredPrice,
			[MaximumCustomerEnteredPrice] = @MaximumCustomerEnteredPrice,
			[HasTierPrices] = 0,
			[HasDiscountsApplied] = 0,
			[Weight] = @Weight,
			[Length] = @Length,
			[Width] = @Width,
			[Height] = @Height,
			[AvailableStartDateTimeUtc] = @AvailableStartDateTimeUtc,
			[AvailableEndDateTimeUtc] = @AvailableEndDateTimeUtc,
			[DeliveryTimeId] = @DeliveryTimeId,
			[BasePriceEnabled] = @BasePriceEnabled,
			[BasePriceMeasureUnit] = @BasePriceMeasureUnit,
			[BasePriceAmount] = @BasePriceAmount,
			[BasePriceBaseAmount] = @BasePriceBaseAmount
			WHERE [Id]=@ProductId
			
			--product type
			UPDATE [Product]
			SET [ProductTypeId]=10
			WHERE [Id]=@ProductId
			--product template
			UPDATE [Product]
			SET [ProductTemplateId]=@GroupedProductTemplateId
			WHERE [Id]=@ProductId
			
			--insert a product variant (now we name it an associated product)
			DECLARE @AssociatedProductName nvarchar(1000)
			SELECT @AssociatedProductName = [Name] FROM [Product] WHERE [Id]=@ProductId
			--append a product variant name
			IF (len(@Name) > 0)
			BEGIN
				SET @AssociatedProductName = @AssociatedProductName + ' ' + @Name
			END
						
			--published?
			DECLARE @AssociatedProductPublished bit
			SELECT @AssociatedProductPublished = [Published] FROM [Product] WHERE [Id]=@ProductId
			IF (@Published = 0)
			BEGIN
				SET @AssociatedProductPublished = @Published
			END
			
			--deleted?
			DECLARE @AssociatedProductDeleted bit
			SELECT @AssociatedProductDeleted = [Deleted] FROM [Product] WHERE [Id]=@ProductId
			IF (@Deleted = 1)
			BEGIN
				SET @AssociatedProductDeleted = @Deleted
			END
			
			INSERT INTO [Product]
			(Name, ShortDescription, ProductTemplateId, ShowOnHomePage,
			AllowCustomerReviews, ApprovedRatingSum, NotApprovedRatingSum, ApprovedTotalReviews,
			NotApprovedTotalReviews, SubjectToAcl, LimitedToStores, Published, Deleted, CreatedOnUtc, UpdatedOnUtc, 
			Sku, ManufacturerPartNumber, Gtin,
			IsGiftCard, GiftCardTypeId, RequireOtherProducts, AutomaticallyAddRequiredProducts, IsDownload, 
			DownloadId, UnlimitedDownloads, MaxNumberOfDownloads, DownloadExpirationDays, DownloadActivationTypeId, HasSampleDownload,
			SampleDownloadId, HasUserAgreement, UserAgreementText, 
			IsRecurring, RecurringCycleLength, RecurringCyclePeriodId,
			RecurringTotalCycles, IsShipEnabled, IsFreeShipping, AdditionalShippingCharge, IsTaxExempt, TaxCategoryId, ManageInventoryMethodId,
			StockQuantity, DisplayStockAvailability, DisplayStockQuantity, MinStockQuantity, LowStockActivityId, 
			NotifyAdminForQuantityBelow, BackorderModeId, AllowBackInStockSubscriptions, OrderMinimumQuantity, OrderMaximumQuantity, 
			AllowedQuantities, DisableBuyButton, DisableWishlistButton, AvailableForPreOrder, CallForPrice, Price, OldPrice, ProductCost, 
			SpecialPrice, SpecialPriceStartDateTimeUtc, SpecialPriceEndDateTimeUtc,
			CustomerEntersPrice, MinimumCustomerEnteredPrice, MaximumCustomerEnteredPrice, HasTierPrices, 
			HasDiscountsApplied, Weight, Length, Width, Height,
			AvailableStartDateTimeUtc, AvailableEndDateTimeUtc,
			DeliveryTimeId, BasePriceEnabled, BasePriceMeasureUnit, BasePriceAmount, BasePriceBaseAmount,			
			ProductTypeId, ParentGroupedProductId) 
			VALUES (@AssociatedProductName, @Description, @SimpleProductTemplateId, 
			0, 0, 0, 0, 
			0, 0, 0, 0, @AssociatedProductPublished, 
			@AssociatedProductDeleted, @CreatedOnUtc, @UpdatedOnUtc, 
			@Sku, @ManufacturerPartNumber, @Gtin,
			@IsGiftCard, @GiftCardTypeId, @RequireOtherProducts, 
			--a store owner should manually update [RequiredProductIds] property after upgrade
			@AutomaticallyAddRequiredProducts, @IsDownload, @DownloadId, @UnlimitedDownloads, @MaxNumberOfDownloads, 
			@DownloadExpirationDays, @DownloadActivationTypeId, @HasSampleDownload, @SampleDownloadId, 
			@HasUserAgreement, @UserAgreementText, @IsRecurring, 
			@RecurringCycleLength, @RecurringCyclePeriodId, @RecurringTotalCycles, @IsShipEnabled, @IsFreeShipping, 
			@AdditionalShippingCharge, @IsTaxExempt, @TaxCategoryId, @ManageInventoryMethodId, @StockQuantity, 
			@DisplayStockAvailability, @DisplayStockQuantity, @MinStockQuantity, @LowStockActivityId, 
			@NotifyAdminForQuantityBelow, @BackorderModeId, @AllowBackInStockSubscriptions, @OrderMinimumQuantity, 
			@OrderMaximumQuantity, @AllowedQuantities, @DisableBuyButton, @DisableWishlistButton, @AvailableForPreOrder, @CallForPrice, 
			@Price, @OldPrice, @ProductCost, @SpecialPrice, 
			@SpecialPriceStartDateTimeUtc, @SpecialPriceEndDateTimeUtc, @CustomerEntersPrice, 
			@MinimumCustomerEnteredPrice, @MaximumCustomerEnteredPrice, @HasTierPrices, @HasDiscountsApplied, 
			@Weight, @Length, @Width, @Height, 
			@AvailableStartDateTimeUtc, @AvailableEndDateTimeUtc,
			@DeliveryTimeId, @BasePriceEnabled, @BasePriceMeasureUnit, @BasePriceAmount, @BasePriceBaseAmount,			
			--simple product
			5 , @ProductId)
			
			SET @NewProductId = @@IDENTITY
			
			--product variant picture
			IF (@PictureId > 0)
			BEGIN
				INSERT INTO [Product_Picture_Mapping] ([ProductId], [PictureId], [DisplayOrder])
				VALUES (@NewProductId, @PictureId, 1)
			END
		END
		
		--back in stock subscriptions. move ProductVariantId to the new ProductId column
		IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[BackInStockSubscription]') and NAME='ProductVariantId')
		BEGIN
			EXEC sp_executesql N'UPDATE [BackInStockSubscription]
			SET [ProductId] = @NewProductId
			WHERE [ProductVariantId] = @ExistingProductVariantID',
			N'@NewProductId int OUTPUT, 
			@ExistingProductVariantID int OUTPUT',
			@NewProductId OUTPUT,
			@ExistingProductVariantID OUTPUT			
		END
		
		--order items. move ProductVariantId to the new ProductId column
		IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[OrderItem]') and NAME='ProductVariantId')
		BEGIN
			EXEC sp_executesql N'UPDATE [OrderItem]
			SET [ProductId] = @NewProductId
			WHERE [ProductVariantId] = @ExistingProductVariantID',
			N'@NewProductId int OUTPUT, 
			@ExistingProductVariantID int OUTPUT',
			@NewProductId OUTPUT,
			@ExistingProductVariantID OUTPUT			
		END
		
		--product variant attributes. move ProductVariantId to the new ProductId column
		IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[ProductVariant_ProductAttribute_Mapping]') and NAME='ProductVariantId')
		BEGIN
			EXEC sp_executesql N'UPDATE [ProductVariant_ProductAttribute_Mapping]
			SET [ProductId] = @NewProductId
			WHERE [ProductVariantId] = @ExistingProductVariantID',
			N'@NewProductId int OUTPUT, 
			@ExistingProductVariantID int OUTPUT',
			@NewProductId OUTPUT,
			@ExistingProductVariantID OUTPUT			
		END
		
		--attribute combinations. move ProductVariantId to the new ProductId column
		IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[ProductVariantAttributeCombination]') and NAME='ProductVariantId')
		BEGIN
			EXEC sp_executesql N'UPDATE [ProductVariantAttributeCombination]
			SET [ProductId] = @NewProductId
			WHERE [ProductVariantId] = @ExistingProductVariantID',
			N'@NewProductId int OUTPUT, 
			@ExistingProductVariantID int OUTPUT',
			@NewProductId OUTPUT,
			@ExistingProductVariantID OUTPUT			
		END
		
		--shopping cart items. move ProductVariantId to the new ProductId column
		IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[ShoppingCartItem]') and NAME='ProductVariantId')
		BEGIN
			EXEC sp_executesql N'UPDATE [ShoppingCartItem]
			SET [ProductId] = @NewProductId
			WHERE [ProductVariantId] = @ExistingProductVariantID',
			N'@NewProductId int OUTPUT, 
			@ExistingProductVariantID int OUTPUT',
			@NewProductId OUTPUT,
			@ExistingProductVariantID OUTPUT			
		END
		
		--tier prices. move ProductVariantId to the new ProductId column
		IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[TierPrice]') and NAME='ProductVariantId')
		BEGIN
			EXEC sp_executesql N'UPDATE [TierPrice]
			SET [ProductId] = @NewProductId
			WHERE [ProductVariantId] = @ExistingProductVariantID',
			N'@NewProductId int OUTPUT, 
			@ExistingProductVariantID int OUTPUT',
			@NewProductId OUTPUT,
			@ExistingProductVariantID OUTPUT			
		END
		
		--discounts. move ProductVariantId to the new ProductId column
		IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Discount_AppliedToProducts]') and NAME='ProductVariant_Id')
		BEGIN
			EXEC sp_executesql N'UPDATE [Discount_AppliedToProducts]
			SET [Product_Id] = @NewProductId
			WHERE [ProductVariant_Id] = @ExistingProductVariantID',
			N'@NewProductId int, 
			@ExistingProductVariantID int',
			@NewProductId,
			@ExistingProductVariantID			
		END
		
				
		--fetch next product variant identifier
		FETCH NEXT FROM cur_existingproductvariant INTO @ExistingProductVariantID
	END
	CLOSE cur_existingproductvariant
	DEALLOCATE cur_existingproductvariant
END
GO

--back in stock subscriptions
ALTER TABLE [BackInStockSubscription]
ALTER COLUMN [ProductId] int NOT NULL
GO
IF NOT EXISTS (SELECT 1
           FROM   sys.objects
           WHERE  name = 'BackInStockSubscription_Product'
           AND parent_object_id = Object_id('BackInStockSubscription')
           AND Objectproperty(object_id,N'IsForeignKey') = 1)
BEGIN
	ALTER TABLE [dbo].[BackInStockSubscription] WITH CHECK ADD CONSTRAINT [BackInStockSubscription_Product] FOREIGN KEY([ProductId])
	REFERENCES [dbo].[Product] ([Id])
	ON DELETE CASCADE
END
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[BackInStockSubscription]') and NAME='ProductVariantId')
BEGIN
	ALTER TABLE [BackInStockSubscription]
	DROP COLUMN [ProductVariantId]
END
GO

--order items
ALTER TABLE [OrderItem]
ALTER COLUMN [ProductId] int NOT NULL
GO
IF NOT EXISTS (SELECT 1
           FROM   sys.objects
           WHERE  name = 'OrderItem_Product'
           AND parent_object_id = Object_id('OrderItem')
           AND Objectproperty(object_id,N'IsForeignKey') = 1)
BEGIN
	ALTER TABLE [dbo].[OrderItem] WITH CHECK ADD CONSTRAINT [OrderItem_Product] FOREIGN KEY([ProductId])
	REFERENCES [dbo].[Product] ([Id])
	ON DELETE CASCADE
END
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[OrderItem]') and NAME='ProductVariantId')
BEGIN
	ALTER TABLE [OrderItem]
	DROP COLUMN [ProductVariantId]
END
GO

--product variant attributes
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[ProductVariant_ProductAttribute_Mapping]') and OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	ALTER TABLE [ProductVariant_ProductAttribute_Mapping]
	ALTER COLUMN [ProductId] int NOT NULL
END
GO
IF NOT EXISTS (SELECT 1
           FROM   sys.objects
           WHERE  name = 'ProductVariantAttribute_Product'
           AND parent_object_id = Object_id('ProductVariant_ProductAttribute_Mapping')
           AND Objectproperty(object_id,N'IsForeignKey') = 1)
BEGIN
	--one more validatation here because we'll rename [ProductVariant_ProductAttribute_Mapping] table a bit later
	IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Product_ProductAttribute_Mapping]') and OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
	BEGIN
		ALTER TABLE [dbo].[ProductVariant_ProductAttribute_Mapping] WITH CHECK ADD CONSTRAINT [ProductVariantAttribute_Product] FOREIGN KEY([ProductId])
		REFERENCES [dbo].[Product] ([Id])
		ON DELETE CASCADE
	END
END
GO
IF EXISTS (SELECT 1 from sys.indexes WHERE [NAME]=N'IX_ProductVariant_ProductAttribute_Mapping_ProductVariantId' and object_id=object_id(N'[ProductVariant_ProductAttribute_Mapping]'))
BEGIN
	DROP INDEX [IX_ProductVariant_ProductAttribute_Mapping_ProductVariantId] ON [ProductVariant_ProductAttribute_Mapping]
	CREATE NONCLUSTERED INDEX [IX_Product_ProductAttribute_Mapping_ProductId] ON [ProductVariant_ProductAttribute_Mapping] ([ProductId] ASC)
END
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[ProductVariant_ProductAttribute_Mapping]') and NAME='ProductVariantId')
BEGIN
	ALTER TABLE [ProductVariant_ProductAttribute_Mapping]
	DROP COLUMN [ProductVariantId]
END
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[ProductVariant_ProductAttribute_Mapping]') and OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	EXEC sp_rename 'ProductVariant_ProductAttribute_Mapping', 'Product_ProductAttribute_Mapping';
END
GO
--attribute combinations
ALTER TABLE [ProductVariantAttributeCombination]
ALTER COLUMN [ProductId] int NOT NULL
GO
IF NOT EXISTS (SELECT 1
           FROM   sys.objects
           WHERE  name = 'ProductVariantAttributeCombination_Product'
           AND parent_object_id = Object_id('ProductVariantAttributeCombination')
           AND Objectproperty(object_id,N'IsForeignKey') = 1)
BEGIN
	ALTER TABLE [dbo].[ProductVariantAttributeCombination] WITH CHECK ADD CONSTRAINT [ProductVariantAttributeCombination_Product] FOREIGN KEY([ProductId])
	REFERENCES [dbo].[Product] ([Id])
	ON DELETE CASCADE
END
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[ProductVariantAttributeCombination]') and NAME='ProductVariantId')
BEGIN
	ALTER TABLE [ProductVariantAttributeCombination]
	DROP COLUMN [ProductVariantId]
END
GO
--shopping cart items
ALTER TABLE [ShoppingCartItem]
ALTER COLUMN [ProductId] int NOT NULL
GO
IF NOT EXISTS (SELECT 1
           FROM   sys.objects
           WHERE  name = 'ShoppingCartItem_Product'
           AND parent_object_id = Object_id('ShoppingCartItem')
           AND Objectproperty(object_id,N'IsForeignKey') = 1)
BEGIN
	ALTER TABLE [dbo].[ShoppingCartItem] WITH CHECK ADD CONSTRAINT [ShoppingCartItem_Product] FOREIGN KEY([ProductId])
	REFERENCES [dbo].[Product] ([Id])
	ON DELETE CASCADE
END
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[ShoppingCartItem]') and NAME='ProductVariantId')
BEGIN
	ALTER TABLE [ShoppingCartItem]
	DROP COLUMN [ProductVariantId]
END
GO
--tier prices
ALTER TABLE [TierPrice]
ALTER COLUMN [ProductId] int NOT NULL
GO
IF NOT EXISTS (SELECT 1
           FROM   sys.objects
           WHERE  name = 'TierPrice_Product'
           AND parent_object_id = Object_id('TierPrice')
           AND Objectproperty(object_id,N'IsForeignKey') = 1)
BEGIN
	ALTER TABLE [dbo].[TierPrice] WITH CHECK ADD CONSTRAINT [TierPrice_Product] FOREIGN KEY([ProductId])
	REFERENCES [dbo].[Product] ([Id])
	ON DELETE CASCADE
END
GO
IF EXISTS (SELECT 1 from sys.indexes WHERE [NAME]=N'IX_TierPrice_ProductVariantId' and object_id=object_id(N'[TierPrice]'))
BEGIN
	DROP INDEX [IX_TierPrice_ProductVariantId] ON [TierPrice]
	CREATE NONCLUSTERED INDEX [IX_TierPrice_ProductId] ON [TierPrice] ([ProductId] ASC)
END
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[TierPrice]') and NAME='ProductVariantId')
BEGIN
	ALTER TABLE [TierPrice]
	DROP COLUMN [ProductVariantId]
END
GO
--discounts
IF NOT EXISTS (SELECT 1
           FROM   sys.objects
           WHERE  name = 'Discount_AppliedToProducts_Source'
           AND parent_object_id = Object_id('Discount_AppliedToProducts')
           AND Objectproperty(object_id,N'IsForeignKey') = 1)
BEGIN
	ALTER TABLE [dbo].[Discount_AppliedToProducts] WITH CHECK ADD CONSTRAINT [Discount_AppliedToProducts_Source] FOREIGN KEY([Discount_Id])
	REFERENCES [dbo].[Discount] ([Id])
	ON DELETE CASCADE
END
GO
IF NOT EXISTS (SELECT 1
           FROM   sys.objects
           WHERE  name = 'Discount_AppliedToProducts_Target'
           AND parent_object_id = Object_id('Discount_AppliedToProducts')
           AND Objectproperty(object_id,N'IsForeignKey') = 1)
BEGIN
	ALTER TABLE [dbo].[Discount_AppliedToProducts] WITH CHECK ADD CONSTRAINT [Discount_AppliedToProducts_Target] FOREIGN KEY([Product_Id])
	REFERENCES [dbo].[Product] ([Id])
	ON DELETE CASCADE
END
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Discount_AppliedToProducts]') and NAME='ProductVariant_Id')
BEGIN
	ALTER TABLE [Discount_AppliedToProducts]
	DROP COLUMN [ProductVariant_Id]
END
GO

--drop product variant table
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[ProductVariant]') and OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	DROP TABLE [ProductVariant]
END
GO

--new Product columns. Set "NOT NULL" where required
ALTER TABLE [Product]
ALTER COLUMN [ParentGroupedProductId] int NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [ProductTypeId] int NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [IsGiftCard] bit NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [GiftCardTypeId] int NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [RequireOtherProducts] bit NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [AutomaticallyAddRequiredProducts] bit NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [IsDownload] bit NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [DownloadId] int NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [UnlimitedDownloads] bit NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [MaxNumberOfDownloads] int NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [DownloadActivationTypeId] int NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [HasSampleDownload] bit NOT NULL
GO

Update [Product] SET SampleDownloadId = null WHERE SampleDownloadId = 0
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('[Product]') and NAME='IX_SampleDownloadId')
BEGIN
	CREATE INDEX [IX_SampleDownloadId] ON [Product]([SampleDownloadId])
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE name = 'FK_dbo.Product_dbo.Download_SampleDownloadId' AND type = 'F')
BEGIN
	ALTER TABLE [Product] WITH CHECK ADD CONSTRAINT [FK_dbo.Product_dbo.Download_SampleDownloadId] FOREIGN KEY ([SampleDownloadId]) REFERENCES [Download] ([Id])
END
GO
--ALTER TABLE [Product]
--ALTER COLUMN [SampleDownloadId] int NOT NULL
--GO

ALTER TABLE [Product]
ALTER COLUMN [HasUserAgreement] bit NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [IsRecurring] bit NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [RecurringCycleLength] int NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [RecurringCyclePeriodId] int NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [RecurringTotalCycles] int NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [IsShipEnabled] bit NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [IsFreeShipping] bit NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [AdditionalShippingCharge] decimal(18,4) NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [IsTaxExempt] bit NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [TaxCategoryId] int NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [ManageInventoryMethodId] int NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [StockQuantity] int NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [DisplayStockAvailability] bit NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [DisplayStockQuantity] bit NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [MinStockQuantity] int NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [LowStockActivityId] int NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [NotifyAdminForQuantityBelow] int NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [BackorderModeId] int NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [AllowBackInStockSubscriptions] bit NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [OrderMinimumQuantity] int NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [OrderMaximumQuantity] int NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [DisableBuyButton] bit NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [DisableWishlistButton] bit NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [AvailableForPreOrder] bit NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [CallForPrice] bit NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [Price] decimal(18, 4) NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [OldPrice] decimal(18, 4) NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [ProductCost] decimal(18, 4) NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [CustomerEntersPrice] bit NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [MinimumCustomerEnteredPrice] decimal(18, 4) NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [MaximumCustomerEnteredPrice] decimal(18, 4) NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [HasTierPrices] bit NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [HasDiscountsApplied] bit NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [Weight] decimal(18, 4) NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [Length] decimal(18, 4) NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [Width] decimal(18, 4) NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [Height] decimal(18, 4) NOT NULL
GO

ALTER TABLE [Product]
ALTER COLUMN [BasePriceEnabled] bit NOT NULL
GO

-- new indexes
IF NOT EXISTS (SELECT 1 from sys.indexes WHERE [NAME]=N'IX_Product_PriceDatesEtc' and object_id=object_id(N'[Product]'))
BEGIN
	CREATE NONCLUSTERED INDEX [IX_Product_PriceDatesEtc] ON [Product]  ([Price] ASC, [AvailableStartDateTimeUtc] ASC, [AvailableEndDateTimeUtc] ASC, [Published] ASC, [Deleted] ASC)
END
GO

IF NOT EXISTS (SELECT 1 from sys.indexes WHERE [NAME]=N'IX_Product_ParentGroupedProductId' and object_id=object_id(N'[Product]'))
BEGIN
	CREATE NONCLUSTERED INDEX [IX_Product_ParentGroupedProductId] ON [Product] ([ParentGroupedProductId] ASC)
END
GO

IF NOT EXISTS (SELECT 1 from sys.indexes WHERE [NAME]=N'IX_Product_Name' and object_id=object_id(N'[Product]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Product_Name] ON [Product] ([Name] ASC)
END
GO

IF NOT EXISTS (SELECT 1 from sys.indexes WHERE [NAME]=N'IX_Product_Sku' and object_id=object_id(N'[Product]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Product_Sku] ON [Product] ([Sku] ASC)
END
GO

IF NOT EXISTS (SELECT 1 from sys.indexes WHERE [NAME]=N'IX_ProductVariantAttributeCombination_SKU' and object_id=object_id(N'[ProductVariantAttributeCombination]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ProductVariantAttributeCombination_SKU] ON [ProductVariantAttributeCombination] ([SKU] ASC)
END
GO

--you have to manually re-configure "google products" (froogle) plugin
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[GoogleProduct]') and OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	DELETE FROM [GoogleProduct]
	
	IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[GoogleProduct]') and NAME='ProductVariantId')
	BEGIN
		EXEC sp_rename 'GoogleProduct.ProductVariantId', 'ProductId', 'COLUMN';
	END
END
GO

IF EXISTS (
		SELECT *
		FROM sys.objects
		WHERE object_id = OBJECT_ID(N'[temp_generate_sename]') AND OBJECTPROPERTY(object_id,N'IsProcedure') = 1)
DROP PROCEDURE [dbo].[temp_generate_sename]
GO
CREATE PROCEDURE [dbo].[temp_generate_sename]
(
    @table_name nvarchar(1000),
    @entity_id int,
    @result nvarchar(1000) OUTPUT
)
AS
BEGIN
	--get current name
	DECLARE @current_sename nvarchar(1000)
	DECLARE @sql nvarchar(4000)
	
	SET @sql = 'SELECT @current_sename = [Name] FROM [' + @table_name + '] WHERE [Id] = ' + ISNULL(CAST(@entity_id AS nvarchar(max)), '0')
	EXEC sp_executesql @sql,N'@current_sename nvarchar(1000) OUTPUT',@current_sename OUTPUT		
    
    --generate se name    
	DECLARE @new_sename nvarchar(1000)
    SET @new_sename = ''
    --ensure only allowed chars
    DECLARE @allowed_se_chars varchar(4000)
    --Note for store owners: add more chars below if want them to be supported when migrating your data
    SET @allowed_se_chars = N'abcdefghijklmnopqrstuvwxyz1234567890 _-'
    DECLARE @l int
    SET @l = len(@current_sename)
    DECLARE @p int
    SET @p = 1
    WHILE @p <= @l
    BEGIN
		DECLARE @c nvarchar(1)
        SET @c = substring(@current_sename, @p, 1)
        IF CHARINDEX(@c,@allowed_se_chars) > 0
        BEGIN
			SET @new_sename = @new_sename + @c
		END
		SET @p = @p + 1
	END
	--replace spaces with '-'
	SELECT @new_sename = REPLACE(@new_sename,' ','-');
    WHILE CHARINDEX('--',@new_sename) > 0
		SELECT @new_sename = REPLACE(@new_sename,'--','-');
    WHILE CHARINDEX('__',@new_sename) > 0
		SELECT @new_sename = REPLACE(@new_sename,'__','_');
    --ensure not empty
    IF (@new_sename is null or @new_sename = '')
		SELECT @new_sename = ISNULL(CAST(@entity_id AS nvarchar(max)), '0');
    --lowercase
	SELECT @new_sename = LOWER(@new_sename)
	--ensure this sename is not reserved
	WHILE (1=1)
	BEGIN
		DECLARE @sename_is_already_reserved bit
		SET @sename_is_already_reserved = 0
		SET @sql = 'IF EXISTS (SELECT 1 FROM [UrlRecord] WHERE [Slug] = @sename AND [EntityId] <> ' + ISNULL(CAST(@entity_id AS nvarchar(max)), '0') + ')
					BEGIN
						SELECT @sename_is_already_reserved = 1
					END'
		EXEC sp_executesql @sql,N'@sename nvarchar(1000), @sename_is_already_reserved nvarchar(4000) OUTPUT',@new_sename,@sename_is_already_reserved OUTPUT
		
		IF (@sename_is_already_reserved > 0)
		BEGIN
			--add some digit to the end in this case
			SET @new_sename = @new_sename + '-1'
		END
		ELSE
		BEGIN
			BREAK
		END
	END
	
	--return
    SET @result = @new_sename
END
GO

--set search engine friendly name (UrlRecord) for associated products (new products added before in this upgrade script). [ParentGroupedProductId] > 0
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Product]') and OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	DECLARE @sename_existing_entity_id int
	DECLARE cur_sename_existing_entity CURSOR FOR
	SELECT [Id]
	FROM [Product]
	WHERE [ParentGroupedProductId] > 0
	OPEN cur_sename_existing_entity
	FETCH NEXT FROM cur_sename_existing_entity INTO @sename_existing_entity_id
	WHILE @@FETCH_STATUS = 0
	BEGIN
		DECLARE @sename nvarchar(1000)	
		SET @sename = null -- clear cache (variable scope)
		
		DECLARE @table_name nvarchar(1000)	
		SET @table_name = N'Product'
		
		--main sename
		EXEC	[dbo].[temp_generate_sename]
				@table_name = @table_name,
				@entity_id = @sename_existing_entity_id,
				@result = @sename OUTPUT
				
		IF EXISTS(SELECT 1 FROM [UrlRecord] WHERE [LanguageId]=0 AND [EntityId]=@sename_existing_entity_id AND [EntityName]=@table_name)
		BEGIN
			UPDATE [UrlRecord]
			SET [Slug] = @sename
			WHERE [LanguageId]=0 AND [EntityId]=@sename_existing_entity_id AND [EntityName]=@table_name
		END
		ELSE
		BEGIN
			INSERT INTO [UrlRecord] ([EntityId], [EntityName], [Slug], [LanguageId], [IsActive])
			VALUES (@sename_existing_entity_id, @table_name, @sename, 0, 1)
		END		

		--fetch next identifier
		FETCH NEXT FROM cur_sename_existing_entity INTO @sename_existing_entity_id
	END
	CLOSE cur_sename_existing_entity
	DEALLOCATE cur_sename_existing_entity
END
GO

--drop temporary procedures & functions
IF EXISTS (
		SELECT *
		FROM sys.objects
		WHERE object_id = OBJECT_ID(N'[temp_generate_sename]') AND OBJECTPROPERTY(object_id,N'IsProcedure') = 1)
DROP PROCEDURE [temp_generate_sename]
GO

--new Product property
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='VisibleIndividually')
BEGIN
	ALTER TABLE [Product]
	ADD [VisibleIndividually] bit NULL
END
GO

UPDATE [Product]
SET [VisibleIndividually] = 0
WHERE [VisibleIndividually] IS NULL AND [ParentGroupedProductId] > 0
GO
UPDATE [Product]
SET [VisibleIndividually] = 1
WHERE [VisibleIndividually] IS NULL AND [ParentGroupedProductId] = 0
GO

ALTER TABLE [Product] ALTER COLUMN [VisibleIndividually] bit NOT NULL
GO

--more indexes
IF NOT EXISTS (SELECT 1 from sys.indexes WHERE [NAME]=N'IX_Product_VisibleIndividually' and object_id=object_id(N'[Product]'))
BEGIN
	CREATE NONCLUSTERED INDEX [IX_Product_VisibleIndividually] ON [Product] ([VisibleIndividually] ASC)
END
GO

--new [DisplayOrder] property
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='DisplayOrder')
BEGIN
	ALTER TABLE [Product]
	ADD [DisplayOrder] int NULL
END
GO

UPDATE [Product] SET [DisplayOrder] = 0
GO
ALTER TABLE [Product] ALTER COLUMN [DisplayOrder] int NOT NULL
GO

--updated product type values
UPDATE [Product] SET [ProductTypeId]=5 WHERE [ProductTypeId]=0
GO


IF EXISTS (SELECT *
           FROM   sys.objects
           WHERE  object_id = OBJECT_ID(N'[dbo].[nop_splitstring_to_table]')
                  AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
  DROP FUNCTION [dbo].[nop_splitstring_to_table]
GO

IF EXISTS (SELECT *
           FROM   sys.objects
           WHERE  object_id = OBJECT_ID(N'[dbo].[nop_getnotnullnotempty]')
                  AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
  DROP FUNCTION [dbo].[nop_getnotnullnotempty]
GO

IF EXISTS (SELECT *
           FROM   sys.objects
           WHERE  object_id = OBJECT_ID(N'[dbo].[nop_getprimarykey_indexname]')
                  AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
  DROP FUNCTION [dbo].[nop_getprimarykey_indexname]
GO

IF EXISTS (SELECT 1 FROM sys.fulltext_catalogs WHERE name = 'nopCommerceFullTextCatalog')
BEGIN
	EXEC('
		UPDATE [Setting] SET [Value] = ''False'' WHERE [Name] = N''commonsettings.usefulltextsearch''
	')
	EXEC('
		IF EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = object_id(''[Product]''))
			DROP FULLTEXT INDEX ON [Product]	
	')
	EXEC('
		IF EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = object_id(''[ProductVariantAttributeCombination]''))
			DROP FULLTEXT INDEX ON [ProductVariantAttributeCombination]
	')
	EXEC('
		IF EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = object_id(''[LocalizedProperty]''))
			DROP FULLTEXT INDEX ON [LocalizedProperty]
	')
	EXEC('
		IF EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = object_id(''[ProductTag]''))
			DROP FULLTEXT INDEX ON [ProductTag]
	')
	EXEC('
		IF EXISTS (SELECT 1 FROM sys.fulltext_catalogs WHERE [name] = ''nopCommerceFullTextCatalog'')
			DROP FULLTEXT CATALOG [nopCommerceFullTextCatalog]
	')
END
GO

IF NOT EXISTS (SELECT *
           FROM   sys.objects
           WHERE  object_id = OBJECT_ID(N'[dbo].[sm_splitstring_to_table]')
                  AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
BEGIN
	EXEC('
		CREATE FUNCTION [dbo].[sm_splitstring_to_table]
		(
			@string NVARCHAR(MAX),
			@delimiter CHAR(1)
		)
		RETURNS @output TABLE(
			data NVARCHAR(MAX)
		)
		BEGIN
			DECLARE @start INT, @end INT
			SELECT @start = 1, @end = CHARINDEX(@delimiter, @string)

			WHILE @start < LEN(@string) + 1 BEGIN
				IF @end = 0 
					SET @end = LEN(@string) + 1

				INSERT INTO @output (data) 
				VALUES(SUBSTRING(@string, @start, @end - @start))
				SET @start = @end + 1
				SET @end = CHARINDEX(@delimiter, @string, @start)
			END
			RETURN
		END
	')
END
GO

IF NOT EXISTS (SELECT *
           FROM   sys.objects
           WHERE  object_id = OBJECT_ID(N'[dbo].[sm_getnotnullnotempty]')
                  AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
BEGIN
	EXEC('
		CREATE FUNCTION [dbo].[sm_getnotnullnotempty]
		(
			@p1 nvarchar(max) = null, 
			@p2 nvarchar(max) = null
		)
		RETURNS nvarchar(max)
		AS
		BEGIN
			IF @p1 IS NULL
				return @p2
			IF @p1 =''''
				return @p2

			return @p1
		END
	')
END
GO

IF NOT EXISTS (SELECT *
           FROM   sys.objects
           WHERE  object_id = OBJECT_ID(N'[dbo].[sm_getprimarykey_indexname]')
                  AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
BEGIN
	EXEC('
		CREATE FUNCTION [dbo].[sm_getprimarykey_indexname]
		(
			@table_name nvarchar(1000) = null
		)
		RETURNS nvarchar(1000)
		AS
		BEGIN
			DECLARE @index_name nvarchar(1000)

			SELECT @index_name = i.name
			FROM sys.tables AS tbl
			INNER JOIN sys.indexes AS i ON (i.index_id > 0 and i.is_hypothetical = 0) AND (i.object_id=tbl.object_id)
			WHERE (i.is_unique=1 and i.is_disabled=0) and (tbl.name=@table_name)

			RETURN @index_name
		END
	')
END
GO

IF EXISTS (
		SELECT *
		FROM sys.objects
		WHERE object_id = OBJECT_ID(N'[FullText_Enable]') AND OBJECTPROPERTY(object_id,N'IsProcedure') = 1)
DROP PROCEDURE [FullText_Enable]
GO
CREATE PROCEDURE [FullText_Enable]
AS
BEGIN
	--create catalog
	EXEC('
	IF NOT EXISTS (SELECT 1 FROM sys.fulltext_catalogs WHERE [name] = ''SmartStoreNETFullTextCatalog'')
		CREATE FULLTEXT CATALOG [SmartStoreNETFullTextCatalog] AS DEFAULT')
	
	--create indexes
	DECLARE @create_index_text nvarchar(4000)
	SET @create_index_text = '
	IF NOT EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = object_id(''[Product]''))
		CREATE FULLTEXT INDEX ON [Product]([Name], [ShortDescription], [FullDescription], [Sku])
		KEY INDEX [' + dbo.[sm_getprimarykey_indexname] ('Product') +  '] ON [SmartStoreNETFullTextCatalog] WITH CHANGE_TRACKING AUTO'
	EXEC(@create_index_text)
	
	SET @create_index_text = '
	IF NOT EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = object_id(''[ProductVariantAttributeCombination]''))
		CREATE FULLTEXT INDEX ON [ProductVariantAttributeCombination]([SKU])
		KEY INDEX [' + dbo.[sm_getprimarykey_indexname] ('ProductVariantAttributeCombination') +  '] ON [SmartStoreNETFullTextCatalog] WITH CHANGE_TRACKING AUTO'
	EXEC(@create_index_text)

	SET @create_index_text = '
	IF NOT EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = object_id(''[LocalizedProperty]''))
		CREATE FULLTEXT INDEX ON [LocalizedProperty]([LocaleValue])
		KEY INDEX [' + dbo.[sm_getprimarykey_indexname] ('LocalizedProperty') +  '] ON [SmartStoreNETFullTextCatalog] WITH CHANGE_TRACKING AUTO'
	EXEC(@create_index_text)

	SET @create_index_text = '
	IF NOT EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = object_id(''[ProductTag]''))
		CREATE FULLTEXT INDEX ON [ProductTag]([Name])
		KEY INDEX [' + dbo.[sm_getprimarykey_indexname] ('ProductTag') +  '] ON [SmartStoreNETFullTextCatalog] WITH CHANGE_TRACKING AUTO'
	EXEC(@create_index_text)
END
GO

IF EXISTS (
		SELECT *
		FROM sys.objects
		WHERE object_id = OBJECT_ID(N'[FullText_Disable]') AND OBJECTPROPERTY(object_id,N'IsProcedure') = 1)
DROP PROCEDURE [FullText_Disable]
GO
CREATE PROCEDURE [FullText_Disable]
AS
BEGIN
	EXEC('
	--drop indexes
	IF EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = object_id(''[Product]''))
		DROP FULLTEXT INDEX ON [Product]
	')
	
	EXEC('
	IF EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = object_id(''[ProductVariantAttributeCombination]''))
		DROP FULLTEXT INDEX ON [ProductVariantAttributeCombination]
	')

	EXEC('
	IF EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = object_id(''[LocalizedProperty]''))
		DROP FULLTEXT INDEX ON [LocalizedProperty]
	')

	EXEC('
	IF EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = object_id(''[ProductTag]''))
		DROP FULLTEXT INDEX ON [ProductTag]
	')

	--drop catalog
	EXEC('
	IF EXISTS (SELECT 1 FROM sys.fulltext_catalogs WHERE [name] = ''SmartStoreNETFullTextCatalog'')
		DROP FULLTEXT CATALOG [SmartStoreNETFullTextCatalog]
	')
END
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
	@ParentGroupedProductId	int = 0,
	@ProductTypeId		int = null, --product type identifier, null - load all products
	@VisibleIndividuallyOnly bit = 0, 	--0 - load all products , 1 - "visible indivially" only
	@ProductTagId		int = 0,
	@FeaturedProducts	bit = null,	--0 featured only , 1 not featured only, null - load all products
	@PriceMin			decimal(18, 4) = null,
	@PriceMax			decimal(18, 4) = null,
	@Keywords			nvarchar(4000) = null,
	@SearchDescriptions bit = 0, --a value indicating whether to search by a specified "keyword" in product descriptions
	@SearchSku			bit = 0, --a value indicating whether to search by a specified "keyword" in product SKU
	@SearchProductTags  bit = 0, --a value indicating whether to search by a specified "keyword" in product tags
	@UseFullTextSearch  bit = 0,
	@FullTextMode		int = 0, --0 - using CONTAINS with <prefix_term>, 5 - using CONTAINS and OR with <prefix_term>, 10 - using CONTAINS and AND with <prefix_term>
	@FilteredSpecs		nvarchar(MAX) = null,	--filter by attributes (comma-separated list). e.g. 14,15,16
	@LanguageId			int = 0,
	@OrderBy			int = 0, --0 - position, 5 - Name: A to Z, 6 - Name: Z to A, 10 - Price: Low to High, 11 - Price: High to Low, 15 - creation date
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

		--SKU
		IF @SearchSku = 1
		BEGIN
			SET @sql = @sql + '
			UNION
			SELECT p.Id
			FROM Product p with (NOLOCK)
			LEFT OUTER JOIN ProductVariantAttributeCombination pvac with(NOLOCK) ON pvac.ProductId = p.Id
			WHERE '
			IF @UseFullTextSearch = 1
				SET @sql = @sql + '(CONTAINS(pvac.[Sku], @Keywords) OR CONTAINS(p.[Sku], @Keywords)) '
			ELSE
				SET @sql = @sql + 'PATINDEX(@Keywords, pvac.[Sku]) > 0 OR PATINDEX(@Keywords, p.[Sku]) > 0 '
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
	SELECT CAST(data as int) FROM [sm_splitstring_to_table](@CategoryIds, ',')	
	DECLARE @CategoryIdsCount int	
	SET @CategoryIdsCount = (SELECT COUNT(1) FROM #FilteredCategoryIds)

	--filter by attributes
	SET @FilteredSpecs = isnull(@FilteredSpecs, '')	
	CREATE TABLE #FilteredSpecs
	(
		SpecificationAttributeOptionId int not null
	)
	INSERT INTO #FilteredSpecs (SpecificationAttributeOptionId)
	SELECT CAST(data as int) FROM [sm_splitstring_to_table](@FilteredSpecs, ',')
	DECLARE @SpecAttributesCount int	
	SET @SpecAttributesCount = (SELECT COUNT(1) FROM #FilteredSpecs)

	--filter by customer role IDs (access control list)
	SET @AllowedCustomerRoleIds = isnull(@AllowedCustomerRoleIds, '')	
	CREATE TABLE #FilteredCustomerRoleIds
	(
		CustomerRoleId int not null
	)
	INSERT INTO #FilteredCustomerRoleIds (CustomerRoleId)
	SELECT CAST(data as int) FROM [sm_splitstring_to_table](@AllowedCustomerRoleIds, ',')
	
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
	
	--filter by parent grouped product identifer
	IF @ParentGroupedProductId > 0
	BEGIN
		SET @sql = @sql + '
		AND p.ParentGroupedProductId = ' + CAST(@ParentGroupedProductId AS nvarchar(max))
	END
	
	--filter by product type
	IF @ProductTypeId is not null
	BEGIN
		SET @sql = @sql + '
		AND p.ProductTypeId = ' + CAST(@ProductTypeId AS nvarchar(max))
	END
	
	--filter by visible individually
	IF @VisibleIndividuallyOnly = 1
	BEGIN
		SET @sql = @sql + '
		AND p.VisibleIndividually = 1'
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
		AND p.Deleted = 0
		AND (getutcdate() BETWEEN ISNULL(p.AvailableStartDateTimeUtc, ''1/1/1900'') and ISNULL(p.AvailableEndDateTimeUtc, ''1/1/2999''))'
	END
	
	--min price
	IF @PriceMin > 0
	BEGIN
		SET @sql = @sql + '
		AND (
				(
					--special price (specified price and valid date range)
					(p.SpecialPrice IS NOT NULL AND (getutcdate() BETWEEN isnull(p.SpecialPriceStartDateTimeUtc, ''1/1/1900'') AND isnull(p.SpecialPriceEndDateTimeUtc, ''1/1/2999'')))
					AND
					(p.SpecialPrice >= ' + CAST(@PriceMin AS nvarchar(max)) + ')
				)
				OR 
				(
					--regular price (price isnt specified or date range isnt valid)
					(p.SpecialPrice IS NULL OR (getutcdate() NOT BETWEEN isnull(p.SpecialPriceStartDateTimeUtc, ''1/1/1900'') AND isnull(p.SpecialPriceEndDateTimeUtc, ''1/1/2999'')))
					AND
					(p.Price >= ' + CAST(@PriceMin AS nvarchar(max)) + ')
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
					(p.SpecialPrice IS NOT NULL AND (getutcdate() BETWEEN isnull(p.SpecialPriceStartDateTimeUtc, ''1/1/1900'') AND isnull(p.SpecialPriceEndDateTimeUtc, ''1/1/2999'')))
					AND
					(p.SpecialPrice <= ' + CAST(@PriceMax AS nvarchar(max)) + ')
				)
				OR 
				(
					--regular price (price isnt specified or date range isnt valid)
					(p.SpecialPrice IS NULL OR (getutcdate() NOT BETWEEN isnull(p.SpecialPriceStartDateTimeUtc, ''1/1/1900'') AND isnull(p.SpecialPriceEndDateTimeUtc, ''1/1/2999'')))
					AND
					(p.Price <= ' + CAST(@PriceMax AS nvarchar(max)) + ')
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
					FROM [AclRecord] acl with (NOLOCK)
					WHERE [acl].EntityId = p.Id AND [acl].EntityName = ''Product''
				)
			))'
	END
	
	--show hidden and filter by store
	IF @StoreId > 0
	BEGIN
		SET @sql = @sql + '
		AND (p.LimitedToStores = 0 OR EXISTS (
			SELECT 1 FROM [StoreMapping] sm with (NOLOCK)
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
					FROM Product_SpecificationAttribute_Mapping psam with (NOLOCK)
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
		SET @sql_orderby = ' p.[Price] ASC'
	ELSE IF @OrderBy = 11 /* Price: High to Low */
		SET @sql_orderby = ' p.[Price] DESC'
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
		
		--parent grouped product specified (sort associated products)
		IF @ParentGroupedProductId > 0
		BEGIN
			IF LEN(@sql_orderby) > 0 SET @sql_orderby = @sql_orderby + ', '
			SET @sql_orderby = @sql_orderby + ' p.[DisplayOrder] ASC'
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
		FROM [Product_SpecificationAttribute_Mapping] [psam] with (NOLOCK)
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
		INNER JOIN Product p with (NOLOCK) on p.Id = [pi].[ProductId]
	WHERE
		[pi].IndexId > @PageLowerBound AND 
		[pi].IndexId < @PageUpperBound
	ORDER BY
		[pi].IndexId
	
	DROP TABLE #PageIndex
END
GO











IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[ProductBundleItem]') and OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	CREATE TABLE [dbo].[ProductBundleItem]
	(
		[Id] [int] IDENTITY(1,1) NOT NULL,
		[ProductId] int NOT NULL,
		[BundleProductId] int NOT NULL,		
		[Quantity] int NOT NULL,
		[Discount] [decimal](18, 4) NULL,
		[DiscountPercentage] bit NOT NULL,
		[Name] [nvarchar](400) NULL,
		[ShortDescription] [nvarchar](max) NULL,
		[FilterAttributes] bit NOT NULL,
		[HideThumbnail] bit NOT NULL,
		[Visible] bit NOT NULL,
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
		ALTER TABLE [dbo].[ProductBundleItem] WITH CHECK ADD CONSTRAINT [ProductBundleItem_BundleProduct] FOREIGN KEY([BundleProductId])
		REFERENCES [dbo].[Product] ([Id]) ON DELETE CASCADE
	')
	
	EXEC ('
		ALTER TABLE [dbo].[ProductBundleItem] CHECK CONSTRAINT [ProductBundleItem_BundleProduct]
	')
	
	EXEC ('
		CREATE NONCLUSTERED INDEX [IX_ProductBundleItem_BundleProductId] ON [ProductBundleItem] ([BundleProductId] ASC)
	')
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[ProductBundleItemAttributeFilter]') and OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	CREATE TABLE [dbo].[ProductBundleItemAttributeFilter]
	(
		[Id] [int] IDENTITY(1,1) NOT NULL,
		[BundleItemId] int NOT NULL,
		[AttributeId] int NOT NULL,
		[AttributeValueId] int NOT NULL,
		[IsPreSelected] bit NOT NULL

		PRIMARY KEY CLUSTERED 
		(
			[Id] ASC
		) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
	)

	EXEC ('
		ALTER TABLE [dbo].[ProductBundleItemAttributeFilter] WITH CHECK ADD CONSTRAINT [ProductBundleItemAttributeFilter_BundleItem] FOREIGN KEY([BundleItemId])
		REFERENCES [dbo].[ProductBundleItem] ([Id]) ON DELETE CASCADE
	')
	
	EXEC ('
		ALTER TABLE [dbo].[ProductBundleItemAttributeFilter] CHECK CONSTRAINT [ProductBundleItemAttributeFilter_BundleItem]
	')
	
	EXEC ('
		CREATE NONCLUSTERED INDEX [IX_ProductBundleItemAttributeFilter_BundleItemId] ON [ProductBundleItemAttributeFilter] ([BundleItemId] ASC)
	')	
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='BundleTitleText')
BEGIN
	ALTER TABLE [Product] ADD [BundleTitleText] nvarchar(400) NULL
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='BundlePerItemShipping')
BEGIN
	EXEC ('ALTER TABLE [Product] ADD [BundlePerItemShipping] bit NULL')
	EXEC ('UPDATE [Product] SET [BundlePerItemShipping] = 0 WHERE [BundlePerItemShipping] IS NULL')
	EXEC ('ALTER TABLE [Product] ALTER COLUMN [BundlePerItemShipping] bit NOT NULL')
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='BundlePerItemPricing')
BEGIN
	EXEC ('ALTER TABLE [Product] ADD [BundlePerItemPricing] bit NULL')
	EXEC ('UPDATE [Product] SET [BundlePerItemPricing] = 0 WHERE [BundlePerItemPricing] IS NULL')
	EXEC ('ALTER TABLE [Product] ALTER COLUMN [BundlePerItemPricing] bit NOT NULL')
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='BundlePerItemShoppingCart')
BEGIN
	EXEC ('ALTER TABLE [Product] ADD [BundlePerItemShoppingCart] bit NULL')
	EXEC ('UPDATE [Product] SET [BundlePerItemShoppingCart] = 0 WHERE [BundlePerItemShoppingCart] IS NULL')
	EXEC ('ALTER TABLE [Product] ALTER COLUMN [BundlePerItemShoppingCart] bit NOT NULL')
END
GO

IF NOT EXISTS (SELECT 1 FROM [Setting] WHERE [name] = N'mediasettings.bundledproductpicturesize')
BEGIN
	INSERT [Setting] ([Name], [Value], [StoreId]) VALUES (N'mediasettings.bundledproductpicturesize', N'70', 0)
END
GO

IF NOT EXISTS (SELECT 1 FROM [Setting] WHERE [name] = N'mediasettings.cartthumbbundleitempicturesize')
BEGIN
	INSERT [Setting] ([Name], [Value], [StoreId]) VALUES (N'mediasettings.cartthumbbundleitempicturesize', N'32', 0)
END
GO

IF NOT EXISTS (SELECT 1 FROM [Setting] WHERE [name] = N'shoppingcartsettings.showproductbundleimagesonshoppingcart')
BEGIN
	INSERT [Setting] ([Name], [Value], [StoreId]) VALUES (N'shoppingcartsettings.showproductbundleimagesonshoppingcart', N'True', 0)
END
GO

IF NOT EXISTS (SELECT 1 FROM [Setting] WHERE [name] = N'shoppingcartsettings.showproductbundleimagesonwishlist')
BEGIN
	INSERT [Setting] ([Name], [Value], [StoreId]) VALUES (N'shoppingcartsettings.showproductbundleimagesonwishlist', N'True', 0)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[ShoppingCartItem]') and NAME='ParentItemId')
BEGIN
	ALTER TABLE [ShoppingCartItem] ADD [ParentItemId] int NULL
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[ShoppingCartItem]') and NAME='BundleItemId')
BEGIN
	EXEC ('ALTER TABLE [ShoppingCartItem] ADD [BundleItemId] int NULL')
	
	EXEC ('
		ALTER TABLE [dbo].[ShoppingCartItem] WITH CHECK ADD CONSTRAINT [ShoppingCartItem_BundleItem] FOREIGN KEY([BundleItemId])
		REFERENCES [dbo].[ProductBundleItem] ([Id])
	')
	
	EXEC ('
		ALTER TABLE [dbo].[ShoppingCartItem] CHECK CONSTRAINT [ShoppingCartItem_BundleItem]
	')
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[OrderItem]') and NAME='BundleData')
BEGIN
	ALTER TABLE [OrderItem] ADD [BundleData] nvarchar(max) NULL
END
GO













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
