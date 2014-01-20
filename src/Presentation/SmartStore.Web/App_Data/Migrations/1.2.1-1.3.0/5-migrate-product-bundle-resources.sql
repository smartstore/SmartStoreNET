--upgrade scripts for smartstore.net (only specific parts)

--new locale resources
DECLARE @resources xml
--a resource will be deleted if its value is empty   
SET @resources='
<Language>

	<LocaleResource Name="Enums.SmartStore.Core.Domain.Catalog.ProductType.BundledProduct">
		<Value>Bundled product</Value>
		<Value lang="de">Produktbündel</Value>
	</LocaleResource>

	<LocaleResource Name="Admin.Catalog.Products.BundleItems.SaveBeforeEdit">
		<Value>You need to save the product before you can add bundle products for this product page.</Value>
		<Value lang="de">Das Produkt muss gespeichert werden, bevor Bündelprodukte zur Stückliste hinzugefügt werden können.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.BundleItems">
		<Value>Bundled products</Value>
		<Value lang="de">Stückliste</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.BundleItems.AddNew">
		<Value>Add new bundle product</Value>
		<Value lang="de">Bündelprodukt hinzufügen</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.BundleItems.EditOf">
		<Value>Edit of bundle product</Value>
		<Value lang="de">Bearbeiten von Bündelprodukt</Value>
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
		<Value lang="de">Die Menge des Bündelprodukts wird dem gemäß hier angegebenen Wert angepasst, wenn das Bündel zum Warenkorb hinzugefügt wird.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.BundleItems.Fields.Discount">
		<Value>Discount (%)</Value>
		<Value lang="de">Rabatt (%)</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.BundleItems.Fields.Discount.Hint">
		<Value>Discount in percent.</Value>
		<Value lang="de">Rabatt in Prozent.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.BundleItems.Fields.OverrideName">
		<Value>Overrite name</Value>
		<Value lang="de">Namen überschreiben</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.BundleItems.Fields.OverrideName.Hint">
		<Value>Override the default product name for the bundled product.</Value>
		<Value lang="de">Den Standardproduktnamen beim gebündelten Produkt überschreiben.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.BundleItems.Fields.Name">
		<Value>Name</Value>
		<Value lang="de">Name</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.BundleItems.Fields.Name.Hint">
		<Value>The name of the bundled product.</Value>
		<Value lang="de">Der Name des gebündelten Produkts.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.BundleItems.Fields.OverrideShortDescription">
		<Value>Overrite short description</Value>
		<Value lang="de">Kurzbeschreibung überschreiben</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.BundleItems.Fields.OverrideShortDescription.Hint">
		<Value>Override the default short description for the bundled product.</Value>
		<Value lang="de">Die Standardkurzbeschreibung beim gebündelten Produkt überschreiben.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.BundleItems.Fields.ShortDescription">
		<Value>Short description</Value>
		<Value lang="de">Kurzbeschreibung</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.BundleItems.Fields.ShortDescription.Hint">
		<Value>The short description of the bundled product.</Value>
		<Value lang="de">Die Kurzbeschreibung des gebündelten Produkts.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.BundleItems.Fields.HideThumbnail">
		<Value>Hide thumbnail</Value>
		<Value lang="de">Vorschaubild ausblenden</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.BundleItems.Fields.HideThumbnail.Hint">
		<Value>Hide the thumbnail for the bundled product.</Value>
		<Value lang="de">Das Vorschaubild des gebündelten Produkts ausblenden.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.BundleItems.Fields.Published">
		<Value>Bundle published</Value>
		<Value lang="de">Veröffentlicht im Bündel</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.BundleItems.Fields.Published.Hint">
		<Value>Whether to publish the product in the bundle.</Value>
		<Value lang="de">Legt fest, ob das Produkt im Bündel angezeigt werden soll.</Value>
	</LocaleResource>	
	<LocaleResource Name="Admin.Catalog.Products.BundleItems.Fields.DisplayOrder">
		<Value>Bundle display order</Value>
		<Value lang="de">Reihenfolge im Bündel</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Catalog.Products.BundleItems.Fields.DisplayOrder.Hint">
		<Value>The position of the product in the bundle.</Value>
		<Value lang="de">Die Position des Produkts im Bündel.</Value>
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
