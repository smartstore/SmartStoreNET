--upgrade scripts for smartstore.net (only specific parts)

--new locale resources
declare @resources xml
--a resource will be deleted if its value is empty   
set @resources='
<Language>
  <LocaleResource Name="Admin.Customers.CustomerRoles.Fields.TaxDisplayType"> 
    <Value>Tax display type</Value>
	<T>Steueranzeige</T>
  </LocaleResource>
  <LocaleResource Name="Common.Alias"> 
    <Value>Alias</Value>
	<T>Alias</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Values.Fields.Alias"> 
    <Value>Alias</Value>
	<T>Alias</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Values.Fields.Alias.Hint"> 
    <Value>An optional, language-neutral reference name for internal use</Value>
	<T>Ein optionaler, sprachneutraler Referenzwert für interne Zwecke</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Catalog.Attributes.ProductAttributes.Fields.Alias"> 
    <Value>Alias</Value>
	<T>Alias</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Catalog.Attributes.ProductAttributes.Fields.Alias.Hint"> 
    <Value>An optional, language-neutral reference name for internal use</Value>
	<T>Ein optionaler, sprachneutraler Referenzwert für interne Zwecke</T>
  </LocaleResource>

  <LocaleResource Name="Common.FileType.Documents">
    <Value>Documents</Value>
	<T>Dokumente</T>
  </LocaleResource>
  <LocaleResource Name="Common.FileType.Audio">
    <Value>Audio</Value>
	<T>Audio</T>
  </LocaleResource>
  <LocaleResource Name="Common.FileType.Video">
    <Value>Video</Value>
	<T>Video</T>
  </LocaleResource>
  <LocaleResource Name="Common.FileType.Flash">
    <Value>Flash</Value>
	<T>Flash</T>
  </LocaleResource>
  <LocaleResource Name="Common.FileType.Texts">
    <Value>Texts</Value>
	<T>Texte</T>
  </LocaleResource>
  <LocaleResource Name="Common.CrossReferences">
    <Value>Cross references</Value>
	<T>Querverweise</T>
  </LocaleResource>
  <LocaleResource Name="Common.Misc">
    <Value>Miscellaneous</Value>
	<T>Verschiedenes</T>
  </LocaleResource>

  <LocaleResource Name="Admin.Customers.MustBeCustomerOrGuest">
    <Value>Add the customer to ''{0}'' or ''{1}'' customer role.</Value>
	<T>Ordnen Sie den Kunden der Kundengruppe ''{0}'' oder ''{1}'' zu.</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Customers.CanOnlyBeCustomerOrGuest">
    <Value>The customer cannot be in both ''{0}'' and ''{1}'' customer roles.</Value>
	<T>Der Kunde kann nicht beiden Kundengruppen ''{0}'' und ''{1}'' zugeordnet werden.</T>
  </LocaleResource>
  
  <LocaleResource Name="Admin.Configuration.ContentSlider.Slide.AddNew">
    <Value>Add new slide</Value>
	<T>Neuen Slide hinzufügen</T>
  </LocaleResource>

  <LocaleResource Name="Admin.ContentSlider.Slide.Title.Required">
    <Value>Please enter a title for this slide.</Value>
	<T>Bitte geben Sie für diesen Slide eine Überschrift an.</T>
  </LocaleResource>
  
  <LocaleResource Name="Admin.ContentSlider.Slide.ButtonUrl.Required">
    <Value>Please enter a url for this button.</Value>
	<T>Bitte geben Sie für diesen Button eine Url an.</T>
  </LocaleResource>
  <LocaleResource Name="Admin.ContentSlider.Slide.ButtonText.Required">
    <Value>Please enter a text for this button.</Value>
	<T>Bitte geben Sie für diesen Button einen Text an.</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.General.Common.Captcha.Hint">
    <Value>A CAPTCHA is a program that can tell whether its user is a human or a computer. You''ve probably seen them — colorful images with distorted text at the bottom of Web registration forms. CAPTCHAs are used by many websites to prevent abuse from "bots," or automated programs usually written to generate spam. No computer program can read distorted text as well as humans can, so bots cannot navigate sites protected by CAPTCHAs. SmartStore.NET uses <a href="http://www.google.com/recaptcha" target="_blank">reCAPTCHA</a>.</Value>
	<T>CAPTCHAs werden verwendet, damit man entscheiden kann, ob das Gegenüber ein Mensch oder eine Maschine ist. In der Regel macht man dies, um zu prüfen, ob Eingaben in Internetformulare über Menschen oder Maschinen (Roboter, kurz Bot) erfolgt sind, weil Roboter hier oft missbräuchlich eingesetzt werden. CAPTCHAs dienen also der Sicherheit.</T>
  </LocaleResource>
  
  <LocaleResource Name="Admin.Configuration.Languages.Import.InsertUpdate">
    <Value>Insert new resources and update existing</Value>
    <T>Neue Einträge hinzufügen und bestehende aktualisieren</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Languages.Import.Insert">
    <Value>Insert new resources only</Value>
    <T>Nur neue Einträge hinzufügen</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Languages.Import.Update">
    <Value>Update existing only</Value>
    <T>Nur bestehende Einträge aktualisieren</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Languages.Import.UpdateTouched">
    <Value>Update manually changed resources also</Value>
    <T>Auch manuell geänderte Einträge aktualisieren</T>
  </LocaleResource>

  <LocaleResource Name="Admin.Orders.Fields.DirectDebitAccountHolder">
    <Value>Account holder</Value>
	<T>Kontoinhaber</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Orders.Fields.DirectDebitAccountNumber">
    <Value>Account number</Value>
	<T>Kontonummer</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Orders.Fields.DirectDebitBankCode">
    <Value>Bank code</Value>
	<T>Bankleitzahl</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Orders.Fields.DirectDebitBankName">
    <Value>Bank name</Value>
	<T>Name der Bank</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Orders.Fields.DirectDebitBIC">
    <Value>BIC</Value>
	<T>BIC</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Orders.Fields.DirectDebitCountry">
    <Value>Country</Value>
	<T>Land</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Orders.Fields.DirectDebitIban">
    <Value>IBAN</Value>
	<T>IBAN</T>
  </LocaleResource>

  <LocaleResource Name="Admin.Orders.Fields.EditDD">
    <Value>Edit direct debit information</Value>
	<T>Lastschriftinformationen bearbeiten</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Orders.Fields.SaveDD">
    <Value>Save direct debit information</Value>
	<T>Lastschriftinformationen speichern</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Orders.Fields.CancelDD">
    <Value>Cancel edit</Value>
	<T>Bearbeiten abbrechen</T>
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