--upgrade scripts for smartstore.net (only specific parts)

--new locale resources
DECLARE @resources xml
--a resource will be deleted if its value is empty   
SET @resources='
<Language>
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
