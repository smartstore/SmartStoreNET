--upgrade scripts for smartstore.net (only specific parts)

--new locale resources
DECLARE @resources xml
--a resource will be deleted if its value is empty   
SET @resources='
<Language>

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
