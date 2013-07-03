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

-- New MessageTemplate "Product.AskQuestion"
IF NOT EXISTS (
  SELECT 1
  FROM [dbo].[MessageTemplate]
  WHERE [Name] = N'Product.AskQuestion')
BEGIN
	INSERT [dbo].[MessageTemplate] ([Name], [Subject], [IsActive], [EmailAccountId], [Body])
	VALUES (
		N'Product.AskQuestion', 
		N'%Store.Name% - Question concerning "%Product.Name%" from %ProductQuestion.SenderName%', 
		1, 
		0, 
		N'<style type="text/css"><!--
address, blockquote, center, del, dir, div, dl, fieldset, form, h1, h2, h3, h4, h5, h6, hr, ins, isindex, menu, noframes, noscript, ol, p, pre, table{ margin:0px; } body, td, p{ font-size: 13px; font-family: "Segoe UI", Tahoma, Arial, Helvetica, sans-serif; line-height: 18px; color: #163764; } body{ background:#efefef; } p{ margin-top: 0px; margin-bottom: 10px; } img{ border:0px; } th{ font-weight:bold; color: #ffffff; padding: 5px 0 5px 0; } ul{ list-style-type: square; } li{ line-height: normal; margin-bottom: 5px; } .template-body { width:800px; padding: 10px; border: 1px solid #ccc; } .attr-caption { font-weight: bold; text-align:right; } .attr-value { text-align:right; min-width:158px; width:160px; }
--></style>
<center>
<table border="0" cellpadding="0" cellspacing="0" align="center" bgcolor="#ffffff" class="template-body">
<tbody>
<tr>
<td>
<p>%ProductQuestion.Message%</p>
<p><strong>Email:</strong> %ProductQuestion.SenderEmail%<br /><strong>Name:</strong>&nbsp;%ProductQuestion.SenderName%<br /><strong>Phone:</strong>&nbsp;%ProductQuestion.SenderPhone%&nbsp;</p>
</td>
</tr>
</tbody>
</table>
</center>')
END
GO

-- CatalogSettings.ShowColorSquaresInLists
IF NOT EXISTS (SELECT 1 FROM [Setting] WHERE [name] = N'CatalogSettings.ShowColorSquaresInLists')
BEGIN
	INSERT [Setting] ([Name], [Value])
	VALUES (N'CatalogSettings.ShowColorSquaresInLists', N'True')
END
GO

