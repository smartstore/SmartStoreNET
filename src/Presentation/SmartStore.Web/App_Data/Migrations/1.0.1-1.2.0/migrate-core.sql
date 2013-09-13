--upgrade scripts for smartstore.net (only specific parts)

--new locale resources
declare @resources xml
--a resource will be deleted if its value is empty   
set @resources='
<Language>
  <LocaleResource Name="Admin.Configuration.Settings.ShoppingCart.ShowConfirmOrderLegalHint">
    <Value>Show legal hints in order summary on the confirm order page</Value>
	<T>Rechtliche Hinweise in der Warenkorbübersicht auf der Bestellabschlußseite anzeigen</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.ShoppingCart.ShowConfirmOrderLegalHint.Hint">
    <Value>Determines whether to show hints in order summary on the confirm order page. This text can be altered in the language resources.</Value>
	<T>Bestimmt, ob rechtliche Hinweise in der Warenkorbübersicht auf der Bestellabschlußseite angezeigt werden. Dieser Text kann in den Sprachresourcen geändert werden.</T>
  </LocaleResource>

  <LocaleResource Name="OrderSummary.ConfirmOrderLegalHint">
    <Value>For deliveries to a non-EU state additional costs in regard to customs, fees and taxes can arise.</Value>
	<T>Bei Lieferungen in das Nicht-EU-Ausland können zusätzlich Zölle, Steuern und Gebühren anfallen.</T>
  </LocaleResource>

  <LocaleResource Name="Common.Submit">
    <Value>Submit</Value>
	<T>Absenden</T>
  </LocaleResource>
  
  <LocaleResource Name="Common.Send">
    <Value>Send</Value>
	<T>Senden</T>
  </LocaleResource>
  
  <LocaleResource Name="Common.Question">
    <Value>Question</Value>
	<T>Frage</T>
  </LocaleResource>
  
  <LocaleResource Name="Common.Error.SendMail">
    <Value>Error while sending the email. Please try again later.</Value>
	<T>Fehler beim Versenden der Email. Bitte versuchen Sie es später erneut.</T>
  </LocaleResource>
  
  <LocaleResource Name="Account.Fields.FullName">
    <Value>Name</Value>
	<T>Name</T>
  </LocaleResource>
  
  <LocaleResource Name="Account.Fields.FullName.Required">
    <Value>Name is required</Value>
	<T>Name wird benötigt</T>
  </LocaleResource>
  
  <LocaleResource Name="Products.AskQuestion">
    <Value>Question about product?</Value>
	<T>Fragen zum Artikel?</T>
  </LocaleResource>
  
  <LocaleResource Name="Products.AskQuestion.Title">
    <Value>Question about product</Value>
	<T>Frage zum Artikel</T>
  </LocaleResource>
  
  <LocaleResource Name="Products.AskQuestion.Question.Required">
    <Value>Question is required</Value>
	<T>Frage ist erforderlich</T>
  </LocaleResource>
  
  <LocaleResource Name="Products.AskQuestion.Question.Text">
    <Value>I have following questions concerning the product {0}:</Value>
	<T>Ich habe folgende Fragen zum Artikel {0}:</T>
  </LocaleResource>
  
  <LocaleResource Name="Products.AskQuestion.Sent">
    <Value>Thank you. Your inquiry has been sent successfully.</Value>
	<T>Vielen Dank. Ihre Anfage wurde erfolgreich gesendet.</T>
  </LocaleResource>
  
  <LocaleResource Name="Admin.Configuration.Settings.Catalog.AskQuestionEnabled">
    <Value>''Ask question'' enabled</Value>
	<T>''Produktanfragen'' ermöglichen</T>
  </LocaleResource>
  
  <LocaleResource Name="Admin.Configuration.Settings.Catalog.AskQuestionEnabled.Hint">
    <Value>Check to allow customers to send an inquiry concerning a product</Value>
	<T>Legt fest, ob Kunden eine Anfrage zu einem Produkt stellen können</T>
  </LocaleResource>
  
  <LocaleResource Name="Admin.Configuration.Settings.GeneralCommon.CaptchaShowOnAskQuestionPage">
    <Value>Show on ''ask question'' page</Value>
	<T>Auf der Seite ''Produktanfrage'' zeigen</T>
  </LocaleResource>
  
  <LocaleResource Name="Admin.Configuration.Settings.GeneralCommon.CaptchaShowOnAskQuestionPage.Hint">
    <Value>Check to show CAPTCHA on ''ask question'' page</Value>
	<T>Legt fest, ob ein CAPTCHA auf der ''Produktanfrage''-Seite angezeigt werden soll.</T>
  </LocaleResource>
  
  <LocaleResource Name="Admin.Configuration.Settings.Catalog.DefaultViewMode">
    <Value>Default view mode</Value>
	<T>Standard Listendarstellung</T>
  </LocaleResource>
  
  <LocaleResource Name="Admin.Configuration.Settings.Catalog.DefaultViewMode.Hint">
    <Value>Specifies how product lists should be displayed by default. The customer can also change the appearance manually.</Value>
	<T>Legt fest, wie Produktlisten standardmäßig dargestellt werden sollen. Der Kunde kann die Darstellung im Shop ändern.</T>
  </LocaleResource>
  
  <LocaleResource Name="Common.List">
    <Value>List</Value>
	<T>Liste</T>
  </LocaleResource>
  
  <LocaleResource Name="Common.Grid">
    <Value>Grid</Value>
	<T>Raster</T>
  </LocaleResource>
  
  <LocaleResource Name="ThemeVar.Alpha.SliderBgSlide">
    <Value>Background slide behaviour</Value>
	<T>Hintergrund slide Verhalten</T>
  </LocaleResource>

  <LocaleResource Name="Admin.Configuration.Settings.Catalog.ShowColorSquaresInLists">
    <Value>Show color squares in product lists</Value>
	<T>Zeige Farbvarianten in Produktlisten</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.Catalog.ShowColorSquaresInLists.Hint">
    <Value>Specifies whether the colors of the first color type attribute should be displayed in product lists</Value>
	<T>Legt fest, ob die Farben des ersten Farbattributes auch in Produktlisten angezeigt werden sollen</T>
  </LocaleResource>
  
  <LocaleResource Name="Admin.Configuration.Plugins.Resources.UpdateSuccess">
    <Value>The language resources has been successfully updated.</Value>
	<T>Die Sprachressourcen wurden erfogreich aktualisiert.</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Plugins.Resources.UpdateFailure">
    <Value>Failed to update language resources.</Value>
	<T>Das Aktualisieren der Sprachressourcen ist fehlgeschlagen.</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Plugins.Resources.Update">
    <Value>Update resources</Value>
	<T>Ressourcen aktualisieren</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Plugins.Resources.UpdateConfirm">
    <Value>Do you like to update the language resources for this plugin?</Value>
	<T>Möchten Sie die Sprachressourcen für dieses Plugin aktualisieren?</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Plugins.Resources.UpdateProgress">
    <Value>Refreshing language resources...</Value>
	<T>Aktualisiere Sprachressourcen...</T>
  </LocaleResource>
  
  <LocaleResource Name="Admin.Common.General">
	<Value>General</Value>
	<T>Allgemein</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Common.Store">
	<Value>Store</Value>
	<T>Shop</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Common.Stores">
	<Value>Stores</Value>
	<T>Shops</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Common.Info">
	<Value>Info</Value>
	<T>Info</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Common.StoresAll">
	<Value>All stores</Value>
	<T>Alle Shops</T>
  </LocaleResource>
  
  <LocaleResource Name="Admin.Common.Store.SearchFor">
	<Value>Store</Value>
	<T>Shop</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Common.Store.SearchFor.Hint">
	<Value>Search by a specific store.</Value>
	<T>Nach bestimmten Shop suchen.</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Common.Store.LimitedTo">
	<Value>Limited to stores</Value>
	<T>Auf Shops begrenzt</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Common.Store.LimitedTo.Hint">
	<Value>Determines whether the item is available only at certain stores.</Value>
	<T>Legt fest, ob der Eintrag nur für bestimmte Shops verfügbar ist.</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Common.Store.AvailableFor">
	<Value>Stores</Value>
	<T>Shops</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Common.Store.AvailableFor.Hint">
	<Value>Select stores for which the item will be shown.</Value>
	<T>Bitte Shops auswählen, für die der Eintrag angezeigt werden soll.</T>
  </LocaleResource>
  
  <LocaleResource Name="Common.On">
    <Value>On</Value>
	<T>An</T>
  </LocaleResource>
  <LocaleResource Name="Common.Off">
    <Value>Off</Value>
	<T>Aus</T>
  </LocaleResource>
  
  <LocaleResource Name="RewardPoints.Message.RegisteredAsCustomer">
    <Value>Registered as customer</Value>
	<T>Als Kunde registriert</T>
  </LocaleResource>  

  <LocaleResource Name="Plugins.Shipping.ByTotal.Fields.SmallQuantityThreshold">
    <Value>Threshold for small quantities</Value>
	<T>Mindermenge bis Bestellwert</T>
  </LocaleResource>
  <LocaleResource Name="Plugins.Shipping.ByTotal.Fields.SmallQuantityThreshold.Hint">
    <Value>Subtotal up to which a "small quantity surcharge" should be added. The surcharge will be ignored if no shipping fee is applied. Use "0" if no fee will be charged.</Value>
	<T>Warenwert, bis zu dem ein Mindermengenzuschlag erhoben werden soll. Der Zuschlag wird ignoriert, wenn keine Versandkosten anfallen. Verwenden Sie "0", wenn kein Zuschlag erhoben werden soll.</T>
  </LocaleResource>
  <LocaleResource Name="Plugins.Shipping.ByTotal.Fields.SmallQuantitySurcharge">
    <Value>Surcharge for small quantities</Value>
	<T>Mindermengenzuschlag</T>
  </LocaleResource>
  <LocaleResource Name="Plugins.Shipping.ByTotal.Fields.BaseCharge">
    <Value>Base fee</Value>
	<T>Basisgebühr</T>
  </LocaleResource>
  <LocaleResource Name="Plugins.Shipping.ByTotal.Fields.MaxCharge">
    <Value>Max. fee</Value>
	<T>Max. Gebühr</T>
  </LocaleResource>
  <LocaleResource Name="Plugins.Shipping.ByTotal.Fields.MaxCharge.Hint">
    <Value>An amount that the calculated shipping costs may not exceed.</Value>
	<T>Ein Betrag, den die berechneten Versandkosten nicht übersteigen dürfen.</T>
  </LocaleResource>   
  
  <LocaleResource Name="ErrorPage.Title">
    <Value>We''re sorry, an internal error occurred that prevents the request to complete.</Value>
	<T>Leider ist ein interner Fehler aufgetreten.</T>
  </LocaleResource>
  <LocaleResource Name="ErrorPage.Body">
    <Value>Our supporting staff has been notified with this error and will address this issue shortly. We profusely apologize for the <strong>inconvenience</strong> and for any damage this may cause. You might want to try the same action at later time.</Value>
	<T>Unser Support-Team wurde über diesen Fehler informiert und wird sich in Kürze um die Behebung kümmern. Wir entschuldigen uns für diese Unannehmlichkeit! Bitte probieren Sie den Vorgang zu einem späteren Zeitpunkt erneut.</T>
  </LocaleResource>
  
  <LocaleResource Name="AddProductToCompareList.CouldNotBeAdded">
    <Value>Product could not be added.</Value>
	<T>Produkt konnte nicht hinzugefügt werden.</T>
  </LocaleResource>
  <LocaleResource Name="AddProductToCompareList.ProductWasAdded">
    <Value>The product ''{0}'' was added to the compare list.</Value>
	<T>Das Produkt ''{0}'' wurde der Vergleichsliste hinzugefügt.</T>
  </LocaleResource>
  <LocaleResource Name="AddProductToCompareList.CouldNotBeRemoved">
    <Value>Product could not be removed.</Value>
	<T>Produkt konnte nicht entfernt werden.</T>
  </LocaleResource>
  <LocaleResource Name="AddProductToCompareList.ProductWasDeleted">
    <Value>The product ''{0}'' was removed from the compare list.</Value>
	<T>Das Produkt ''{0}'' wurde von der Vergleichsliste entfernt.</T>
  </LocaleResource>

  <LocaleResource Name="Admin.Configuration.Settings.Catalog.ShowDeliveryTimesInProductDetail">
    <Value>Show delivery times</Value>
	<T>Zeige Lieferzeiten</T>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.Catalog.ShowDeliveryTimesInProductDetail.Hint">
    <Value>Determines whether delivery times should be display on product detail page.</Value>
	<T>Bestimmt ob Lieferzeitinformationen auf der Produktdetailseite angezeigt werden.</T>
  </LocaleResource>
  

  <LocaleResource Name="Jquery.Validate.Email">
    <Value>Please enter a valid email address.</Value>
	<T>Bitte geben Sie eine gültige E-Mail-Adresse ein.</T>
  </LocaleResource>
  <LocaleResource Name="Jquery.Validate.Required">
    <Value>This field is required.</Value>
	<T>Diese Angabe ist erforderlich.</T>
  </LocaleResource>
  <LocaleResource Name="Jquery.Validate.Remote">
    <Value>Please fix this field.</Value>
	<T>Bitte korrigieren Sie dieses Feld.</T>
  </LocaleResource>
  <LocaleResource Name="Jquery.Validate.Url">
    <Value>Please enter a valid URL.</Value>
	<T>Bitte geben Sie eine gültige URL ein.</T>
  </LocaleResource>
 <LocaleResource Name="Jquery.Validate.Date">
    <Value>Please enter a valid date.</Value>
	<T>Bitte geben Sie ein gültiges Datum ein.</T>
  </LocaleResource>
  <LocaleResource Name="Jquery.Validate.DateISO">
    <Value>Please enter a valid date (ISO).</Value>
	<T>Bitte geben Sie ein gültiges Datum (nach ISO) ein.</T>
  </LocaleResource>
 <LocaleResource Name="Jquery.Validate.Number">
    <Value>Please enter a valid number.</Value>
	<T>Bitte geben Sie eine gültige Nummer ein.</T>
  </LocaleResource>
  <LocaleResource Name="Jquery.Validate.Digits">
    <Value>Please enter only digits.</Value>
	<T>Bitte geben Sie nur Ziffern ein.</T>
  </LocaleResource>
  <LocaleResource Name="Jquery.Validate.Creditcard">
    <Value>Please enter a valid credit card number.</Value>
	<T>Bitte geben Sie eine gültige Kreditkartennummer ein.</T>
  </LocaleResource>
  <LocaleResource Name="Jquery.Validate.EqualTo">
    <Value>Please enter the same value again.</Value>
	<T>Wiederholen Sie bitte die Eingabe.</T>
  </LocaleResource>
  <LocaleResource Name="Jquery.Validate.Maxlength">
    <Value>Please enter no more than {0} characters.</Value>
	<T>Bitte geben Sie nicht mehr als {0} Zeichen ein.</T>
  </LocaleResource>
  <LocaleResource Name="Jquery.Validate.Minlength">
    <Value>Please enter at least {0} characters.</Value>
	<T>Bitte geben Sie mindestens {0} Zeichen ein.</T>
  </LocaleResource>
  <LocaleResource Name="Jquery.Validate.Rangelength">
    <Value>Please enter a value between {0} and {1} characters long.</Value>
	<T>Die Länge der Eingabe darf minimal {0} und maximal {1} Zeichen lang sein.</T>
  </LocaleResource>
  <LocaleResource Name="Jquery.Validate.Range">
    <Value>Please enter a value between {0} and {1}.</Value>
	<T>Bitte geben Sie einen Wert zwischen {0} und {1} ein.</T>
  </LocaleResource>
  <LocaleResource Name="Jquery.Validate.Max">
    <Value>Please enter a value less than or equal to {0}.</Value>
	<T>Bitte geben Sie einen Wert kleiner oder gleich {0} ein.</T>
  </LocaleResource>
  <LocaleResource Name="Jquery.Validate.Min">
    <Value>Please enter a value greater than or equal to {0}.</Value>
	<T>Bitte geben Sie einen Wert größer oder gleich {0} ein.</T>
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

-- CatalogSettings.AskQuestionEnabled
IF NOT EXISTS (SELECT 1 FROM [Setting] WHERE [name] = N'catalogsettings.askquestionenabled')
BEGIN
	INSERT [Setting] ([Name], [Value])
	VALUES (N'catalogsettings.askquestionenabled', N'True')
END
GO

-- CaptchaSettings.ShowOnAskQuestionPage
IF NOT EXISTS (SELECT 1 FROM [Setting] WHERE [name] = N'captchasettings.showonaskquestionpage')
BEGIN
	INSERT [Setting] ([Name], [Value])
	VALUES (N'captchasettings.showonaskquestionpage', N'False')
END
GO

-- CatalogSettings.ShowColorSquaresInLists
IF NOT EXISTS (SELECT 1 FROM [Setting] WHERE [name] = N'CatalogSettings.ShowColorSquaresInLists')
BEGIN
	INSERT [Setting] ([Name], [Value])
	VALUES (N'CatalogSettings.ShowColorSquaresInLists', N'True')
END
GO


IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[DiscountRequirement]') and NAME='RestrictedPaymentMethods')
BEGIN
	ALTER TABLE [DiscountRequirement] ADD [RestrictedPaymentMethods] [nvarchar](max) NULL
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[DiscountRequirement]') and NAME='RestrictedShippingOptions')
BEGIN
	ALTER TABLE [DiscountRequirement] ADD [RestrictedShippingOptions] [nvarchar](max) NULL
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[DiscountRequirement]') and NAME='RestrictedToStoreId')
BEGIN
	ALTER TABLE [DiscountRequirement] ADD [RestrictedToStoreId] int NULL
END
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[ShippingByTotal]') and OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	-- ShippingByTotalSettings.SmallQuantityThreshold
	IF NOT EXISTS (SELECT 1 FROM [Setting] WHERE [name] = N'ShippingByTotalSettings.SmallQuantityThreshold')
	BEGIN
		INSERT [Setting] ([Name], [Value], [StoreId])
		VALUES (N'ShippingByTotalSettings.SmallQuantityThreshold', N'0', 0)
	END

	-- ShippingByTotalSettings.SmallQuantityThreshold
	IF NOT EXISTS (SELECT 1 FROM [Setting] WHERE [name] = N'ShippingByTotalSettings.SmallQuantitySurcharge')
	BEGIN
		INSERT [Setting] ([Name], [Value], [StoreId])
		VALUES (N'ShippingByTotalSettings.SmallQuantitySurcharge', N'0', 0)
	END

	-- Add ShippingByTotalRecord.BaseCharge
	IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[ShippingByTotal]') and NAME='BaseCharge')
	BEGIN
		ALTER TABLE ShippingByTotal ADD [BaseCharge] decimal(18,2) NOT NULL DEFAULT 0
	END

	-- Add ShippingByTotalRecord.MaxCharge
	IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[ShippingByTotal]') and NAME='MaxCharge')
	BEGIN
		ALTER TABLE ShippingByTotal ADD MaxCharge decimal(18,2) NULL
	END
END
GO

