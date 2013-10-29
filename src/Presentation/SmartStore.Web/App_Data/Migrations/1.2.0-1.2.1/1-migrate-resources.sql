--upgrade scripts for smartstore.net (only specific parts)

--new locale resources
DECLARE @resources xml
--a resource will be deleted if its value is empty   
SET @resources='
<Language>
	<LocaleResource Name="Fields.LimitToCurrentBasketSubTotal">
		<Value>Limit to basket subtotal</Value>
		<Value lang="de">Bezogen auf Warenkorb-Zwischensumme</Value>
	</LocaleResource>
	<LocaleResource Name="Fields.LimitToCurrentBasketSubTotal.Hint">
		<Value>Specifies whether the amount refers to the current basket subtotal or - when unselected - to the sum of all previously incurred orders.</Value>
		<Value lang="de">Legt fest, ob sich der angegebene Betrag auf die Zwischensumme im aktuellen Warenkorb oder - wenn inaktiv - auf die Summe aller zuvor vom Kunden getätigten Bestellungen bezieht.</Value>
	</LocaleResource>
	<LocaleResource Name="Fields.BasketSubTotalIncludesDiscounts">
		<Value>Include discounts</Value>
		<Value lang="de">Positions-Rabatte berücktichtigen</Value>
	</LocaleResource>
	<LocaleResource Name="Fields.BasketSubTotalIncludesDiscounts.Hint">
		<Value>When selected the specified amount is compared with the basket subtotal excluding all cart item discounts.</Value>
		<Value lang="de">Wenn aktiv, ist der Vergleichswert die Warenkorbsumme abzüglich aller Positions-Rabatte.</Value>
	</LocaleResource>
	
	<LocaleResource Name="Admin.System.SystemInfo.DatabaseSize">
		<Value>Size of database</Value>
		<Value lang="de">Größe der Datenbank</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.System.SystemInfo.DatabaseSize.Hint">
		<Value>The number of megabytes that the database takes on the server hard disk.</Value>
		<Value lang="de">Die Anzahl an Megabytes, die die Datenbank auf der Server-Festplatte in Anspruch nimmt.</Value>
	</LocaleResource>
	
	<LocaleResource Name="Admin.Configuration.Stores.Fields.HtmlBodyId">
		<Value>ID of HTML body</Value>
		<Value lang="de">ID des HTML-Body</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Configuration.Stores.Fields.HtmlBodyId.Hint">
		<Value>Allows to use individual CSS and javascript for a store.</Value>
		<Value lang="de">Emöglicht es, individuelles CSS und Javascript für einen Shop zu verwenden.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Configuration.Stores.Fields.HtmlBodyId.Validation">
		<Value>Please only use letters, digits, underscores or hyphens. The first character should be a letter.</Value>
		<Value lang="de">Bitte verwenden Sie nur Buchstaben, Ziffern, Unterstriche oder Bindestriche. Das erste Zeichen sollte ein Buchstabe sein.</Value>
	</LocaleResource>
	
	<LocaleResource Name="Admin.Common.Entity.Fields.Id">
		<Value>ID</Value>
		<Value lang="de">ID</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Common.Entity.Fields.Id.Hint">
		<Value>The unique numeric id of the entity</Value>
		<Value lang="de">Die eindeutige nummerische ID des Datensatzes</Value>
	</LocaleResource>

	<LocaleResource Name="Admin.Configuration.Plugins.Fields.Install.Confirm">
		<Value></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Configuration.Plugins.Fields.Install.Progress">
		<Value></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Configuration.Plugins.Fields.Uninstall.Confirm">
		<Value></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Configuration.Plugins.Fields.Uninstall.Progress">
		<Value></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Configuration.Plugins.ExecuteTasks">
		<Value>Execute tasks</Value>
		<Value lang="de">Aufgaben ausführen</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Configuration.Plugins.ExecuteTasks.Confirm">
		<Value>Do you really want to execute the queued installation tasks?</Value>
		<Value lang="de">Möchten Sie die anstehenden Installationsaufgaben wirklich ausführen?</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Configuration.Plugins.ExecuteTasks.Progress">
		<Value>The installation tasks are being executed. Please wait...</Value>
		<Value lang="de">Die Installationsaufgaben werden ausgeführt. Bitte warten...</Value>
	</LocaleResource>

	<LocaleResource Name="Admin.Common.SEO">
		<Value></Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Common.SEO">
		<Value>Search engines (SEO)</Value>
		<Value lang="de">Suchmaschinen (SEO)</Value>
	</LocaleResource>
	
	<LocaleResource Name="Common.New">
		<Value>New</Value>
		<Value lang="de">Neu</Value>
	</LocaleResource>

	<LocaleResource Name="Admin.Configuration.Settings.Catalog.LabelAsNewForMaxDays">
		<Value>Label product as "new" for max. [x] days</Value>
		<Value lang="de">Produkt als "neu" kennzeichnen für max. [x] Tage</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Configuration.Settings.Catalog.LabelAsNewForMaxDays.Hint">
		<Value>Specifies the number of days that a "NEW" label should be displayed in the product list. Leave the field blank to disable this feature.</Value>
		<Value lang="de">Legt die Anzahl Tage fest, die das Produkt in der Liste mit einer "NEU"-Kennzeichnung versehen werden soll. Lassen Sie das Feld leer, um die Funktion zu deaktivieren.</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Configuration.Settings.Catalog.ShowDiscountSign">
		<Value>Show discount sign</Value>
		<Value lang="de">Rabattzeichen anzeigen</Value>
	</LocaleResource>
	<LocaleResource Name="Admin.Configuration.Settings.Catalog.ShowDiscountSign.Hint">
		<Value>Specifies whether a discount sign ("%") should be displayed next to the final price when discounts were applied</Value>
		<Value lang="de">Legt fest, ob ein Rabattzeichen ("%") neben dem Endpreis angezeigt werden soll, wenn Rabatte angewendet wurden.</Value>
	</LocaleResource>

	<LocaleResource Name="Common.Navigation">
		<Value>Navigation</Value>
		<Value lang="de">Navigation</Value>
	</LocaleResource>
	
	<LocaleResource Name="Imprint">
		<Value>Imprint</Value>
		<Value lang="de">Impressum</Value>
	</LocaleResource>
	
	<LocaleResource Name="Disclaimer">
		<Value>Disclaimer</Value>
		<Value lang="de">Rechtshinweise</Value>
	</LocaleResource>
	
	<LocaleResource Name="Paymentinfo">
		<Value>Payment info</Value>
		<Value lang="de">Zahlungsarten</Value>
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
