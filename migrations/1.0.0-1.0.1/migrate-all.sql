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

-- Delivery times
if not exists (select * from INFORMATION_SCHEMA.columns where table_name = 'CustomerRole' and column_name = 'TaxDisplayType')
BEGIN
	ALTER TABLE dbo.[CustomerRole] ADD 
		TaxDisplayType int NULL
END
GO

ALTER TABLE [Customer] ALTER COLUMN [TaxDisplayTypeId] int NULL

IF EXISTS (SELECT 1 FROM [LocaleStringResource] WHERE [ResourceName] = 'ActivityLog.ExportThemeVars')
BEGIN
	UPDATE [LocaleStringResource] SET [ResourceValue] = REPLACE ([ResourceValue], '{1}', '{0}') WHERE [ResourceName] = 'ActivityLog.ExportThemeVars'
END
GO

IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE id=object_id('[ProductVariantAttributeValue]') and NAME='Alias')
BEGIN
	ALTER TABLE [ProductVariantAttributeValue]
	ADD [Alias] nvarchar(100) NULL
END
GO

IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE id=object_id('[ProductAttribute]') and NAME='Alias')
BEGIN
	ALTER TABLE [ProductAttribute]
	ADD [Alias] nvarchar(100) NULL
END
GO

--New Column "LocaleStringResource.IsTouched"
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE id=object_id('[LocaleStringResource]') and NAME='IsTouched')
BEGIN
	ALTER TABLE LocaleStringResource ADD IsTouched bit NULL	
END
GO

IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE id=object_id('[Order]') and NAME='AllowStoringDirectDebit')
BEGIN
	ALTER TABLE [Order]
	ADD [AllowStoringDirectDebit] bit NOT NULL DEFAULT 0
	ALTER TABLE [Order]
	ADD [DirectDebitAccountHolder] nvarchar(100) NULL
	ALTER TABLE [Order]
	ADD [DirectDebitAccountNumber] nvarchar(100) NULL
	ALTER TABLE [Order]
	ADD [DirectDebitBankCode] nvarchar(100) NULL
	ALTER TABLE [Order]
	ADD [DirectDebitBankName] nvarchar(100) NULL
	ALTER TABLE [Order]
	ADD [DirectDebitBIC] nvarchar(100) NULL
	ALTER TABLE [Order]
	ADD [DirectDebitCountry] nvarchar(100) NULL
	ALTER TABLE [Order]
	ADD [DirectDebitIban] nvarchar(100) NULL
END
GO

IF EXISTS (SELECT 1 FROM [MessageTemplate] WHERE [Name] = 'Blog.BlogComment')
BEGIN
	UPDATE [MessageTemplate] SET [Body] = '<style type="text/css">address, blockquote, center, del, dir, div, dl, fieldset, form, h1, h2, h3, h4, h5, h6, hr, ins, isindex, menu, noframes, noscript, ol, p, pre, table{ margin:0px; } body, td, p{ font-size: 13px;font-family: ''Segoe UI'', Tahoma, Arial, Helvetica, sans-serif; line-height: 18px; color: #163764; } body{ background:#efefef; } p{ margin-top: 0px; margin-bottom: 10px; } img{ border:0px; } th{ font-weight:bold; color: #ffffff; padding: 5px 0 5px 0; } ul{ list-style-type: square; } li{ line-height: normal; margin-bottom: 5px; } .template-body { width:800px; padding: 10px; border: 1px solid #ccc; } .attr-caption { font-weight: bold; text-align:right; } .attr-value { text-align:right; min-width:158px; width:160px; }</style><center><table border="0" cellpadding="0" cellspacing="0" align="center" bgcolor="#ffffff" class="template-body"><tbody><tr><td><p><a href=" % Store.URL % ">%Store.Name%</a>&nbsp;</p> <p>Ein neuer Kommentar wurde zu dem Blog-Eintrag&nbsp;" % BlogComment.BlogPostTitle % " abgegeben.<br /><br /></p></td></tr></tbody></table></center>'
	WHERE [Name] = 'Blog.BlogComment'
END
GO

IF EXISTS (SELECT 1 FROM [MessageTemplate] WHERE [Name] = 'Customer.BackInStock')
BEGIN
	UPDATE [MessageTemplate] SET [Body] = '<style type="text/css">address, blockquote, center, del, dir, div, dl, fieldset, form, h1, h2, h3, h4, h5, h6, hr, ins, isindex, menu, noframes, noscript, ol, p, pre, table{ margin:0px; } body, td, p{ font-size: 13px;font-family: ''Segoe UI'', Tahoma, Arial, Helvetica, sans-serif; line-height: 18px; color: #163764; } body{ background:#efefef; } p{ margin-top: 0px; margin-bottom: 10px; } img{ border:0px; } th{ font-weight:bold; color: #ffffff; padding: 5px 0 5px 0; } ul{ list-style-type: square; } li{ line-height: normal; margin-bottom: 5px; } .template-body { width:800px; padding: 10px; border: 1px solid #ccc; } .attr-caption { font-weight: bold; text-align:right; } .attr-value { text-align:right; min-width:158px; width:160px; }</style><center><table border="0" cellpadding="0" cellspacing="0" align="center" bgcolor="#ffffff" class="template-body"><tbody><tr><td><p><a href="%Store.URL%">%Store.Name%</a>&nbsp;</p> <p></p> <p>Sehr geehrte(r) Frau/Herr&nbsp;%Customer.FullName%,&nbsp;</p> <p></p> <p>der Artikel&nbsp;"%BackInStockSubscription.ProductName%" ist wieder verf&uuml;gbar.</p> <p></p> <p>Mit freundlichen Gr&uuml;&szlig;en,</p> <p></p> <p>Ihr Shop-Team</p> <p><br /><br /></p></td></tr></tbody></table></center>'
	WHERE [Name] = 'Customer.BackInStock'
END
GO

IF EXISTS (SELECT 1 FROM [MessageTemplate] WHERE [Name] = 'Customer.EmailValidationMessage')
BEGIN
	UPDATE [MessageTemplate] SET [Body] = '<style type="text/css">address, blockquote, center, del, dir, div, dl, fieldset, form, h1, h2, h3, h4, h5, h6, hr, ins, isindex, menu, noframes, noscript, ol, p, pre, table{ margin:0px; } body, td, p{ font-size: 13px;font-family: ''Segoe UI'', Tahoma, Arial, Helvetica, sans-serif; line-height: 18px; color: #163764; } body{ background:#efefef; } p{ margin-top: 0px; margin-bottom: 10px; } img{ border:0px; } th{ font-weight:bold; color: #ffffff; padding: 5px 0 5px 0; } ul{ list-style-type: square; } li{ line-height: normal; margin-bottom: 5px; } .template-body { width:800px; padding: 10px; border: 1px solid #ccc; } .attr-caption { font-weight: bold; text-align:right; } .attr-value { text-align:right; min-width:158px; width:160px; }</style><center><table border="0" cellpadding="0" cellspacing="0" align="center" bgcolor="#ffffff" class="template-body"><tbody><tr><td><p><a href="%Store.URL%">%Store.Name%</a>&nbsp;<br /><br /></p> <p>Bitte best&auml;tigen Sie Ihre Registrierung mit einem Klick auf diesen <a href="%Customer.AccountActivationURL%">Link</a>.</p> <p></p> <p><br />Ihr Shop-Team</p></td></tr></tbody></table></center>'
	WHERE [Name] = 'Customer.EmailValidationMessage'
END
GO

IF EXISTS (SELECT 1 FROM [MessageTemplate] WHERE [Name] = 'Customer.NewPM')
BEGIN
	UPDATE [MessageTemplate] SET [Body] = '<style type="text/css">address, blockquote, center, del, dir, div, dl, fieldset, form, h1, h2, h3, h4, h5, h6, hr, ins, isindex, menu, noframes, noscript, ol, p, pre, table{ margin:0px; } body, td, p{ font-size: 13px;font-family: ''Segoe UI'', Tahoma, Arial, Helvetica, sans-serif; line-height: 18px; color: #163764; } body{ background:#efefef; } p{ margin-top: 0px; margin-bottom: 10px; } img{ border:0px; } th{ font-weight:bold; color: #ffffff; padding: 5px 0 5px 0; } ul{ list-style-type: square; } li{ line-height: normal; margin-bottom: 5px; } .template-body { width:800px; padding: 10px; border: 1px solid #ccc; } .attr-caption { font-weight: bold; text-align:right; } .attr-value { text-align:right; min-width:158px; width:160px; }</style><center><table border="0" cellpadding="0" cellspacing="0" align="center" bgcolor="#ffffff" class="template-body"><tbody><tr><td><p><a href="%Store.URL%">%Store.Name%</a>&nbsp;<br /><br />Sie haben eine neue pers&ouml;nliche Nachricht erhalten.</p></td></tr></tbody></table></center>'
	WHERE [Name] = 'Customer.NewPM'
END
GO

IF EXISTS (SELECT 1 FROM [MessageTemplate] WHERE [Name] = 'Customer.PasswordRecovery')
BEGIN
	UPDATE [MessageTemplate] SET [Body] = '<style type="text/css">address, blockquote, center, del, dir, div, dl, fieldset, form, h1, h2, h3, h4, h5, h6, hr, ins, isindex, menu, noframes, noscript, ol, p, pre, table{ margin:0px; } body, td, p{ font-size: 13px;font-family: ''Segoe UI'', Tahoma, Arial, Helvetica, sans-serif; line-height: 18px; color: #163764; } body{ background:#efefef; } p{ margin-top: 0px; margin-bottom: 10px; } img{ border:0px; } th{ font-weight:bold; color: #ffffff; padding: 5px 0 5px 0; } ul{ list-style-type: square; } li{ line-height: normal; margin-bottom: 5px; } .template-body { width:800px; padding: 10px; border: 1px solid #ccc; } .attr-caption { font-weight: bold; text-align:right; } .attr-value { text-align:right; min-width:158px; width:160px; }</style><center><table border="0" cellpadding="0" cellspacing="0" align="center" bgcolor="#ffffff" class="template-body"><tbody><tr><td><p><a href="%Store.URL%">%Store.Name%</a>&nbsp;</p> <p>Um Ihr Kennwort zur&uuml;ckzusetzen klicken Sie bitte <a href="%Customer.PasswordRecoveryURL%">hier</a>.</p> <p></p> <p>Mit freundlichen Gr&uuml;&szlig;en,</p> <p></p> <p>Ihr Shop-Team</p> <p><br /><br /></p></td></tr></tbody></table></center>'
	WHERE [Name] = 'Customer.PasswordRecovery'
END
GO

IF EXISTS (SELECT 1 FROM [MessageTemplate] WHERE [Name] = 'Customer.WelcomeMessage')
BEGIN
	UPDATE [MessageTemplate] SET [Body] = '<style type="text/css">address, blockquote, center, del, dir, div, dl, fieldset, form, h1, h2, h3, h4, h5, h6, hr, ins, isindex, menu, noframes, noscript, ol, p, pre, table{ margin:0px; } body, td, p{ font-size: 13px;font-family: ''Segoe UI'', Tahoma, Arial, Helvetica, sans-serif; line-height: 18px; color: #163764; } body{ background:#efefef; } p{ margin-top: 0px; margin-bottom: 10px; } img{ border:0px; } th{ font-weight:bold; color: #ffffff; padding: 5px 0 5px 0; } ul{ list-style-type: square; } li{ line-height: normal; margin-bottom: 5px; } .template-body { width:800px; padding: 10px; border: 1px solid #ccc; } .attr-caption { font-weight: bold; text-align:right; } .attr-value { text-align:right; min-width:158px; width:160px; }</style><center><table border="0" cellpadding="0" cellspacing="0" align="center" bgcolor="#ffffff" class="template-body"><tbody><tr><td><p>Herzlich Willkommen in unserem Online-Shop <a href="%Store.URL%">%Store.Name%</a>!</p> <p>St&ouml;bern Sie in Warengruppen und Produkte, Lesen Sie im Blog und tauschen Sie Ihre Meinung im Forum aus.</p> <p>Nehmen Sie auch an unseren Umfragen teil!</p> <p></p> <p>Mit freundlichen Gr&uuml;&szlig;en,</p> <p></p> <p>Ihr Shop-Team</p></td></tr></tbody></table></center>'
	WHERE [Name] = 'Customer.WelcomeMessage'
END
GO

IF EXISTS (SELECT 1 FROM [MessageTemplate] WHERE [Name] = 'Forums.NewForumPost')
BEGIN
	UPDATE [MessageTemplate] SET [Body] = '<style type="text/css">address, blockquote, center, del, dir, div, dl, fieldset, form, h1, h2, h3, h4, h5, h6, hr, ins, isindex, menu, noframes, noscript, ol, p, pre, table{ margin:0px; } body, td, p{ font-size: 13px;font-family: ''Segoe UI'', Tahoma, Arial, Helvetica, sans-serif; line-height: 18px; color: #163764; } body{ background:#efefef; } p{ margin-top: 0px; margin-bottom: 10px; } img{ border:0px; } th{ font-weight:bold; color: #ffffff; padding: 5px 0 5px 0; } ul{ list-style-type: square; } li{ line-height: normal; margin-bottom: 5px; } .template-body { width:800px; padding: 10px; border: 1px solid #ccc; } .attr-caption { font-weight: bold; text-align:right; } .attr-value { text-align:right; min-width:158px; width:160px; }</style><center><table border="0" cellpadding="0" cellspacing="0" align="center" bgcolor="#ffffff" class="template-body"><tbody><tr><td><p><a href="%Store.URL%">%Store.Name%</a>&nbsp;</p> <p></p> <p>Ein neuer Beitrag wurde in&nbsp;<a href="%Forums.TopicURL%">"%Forums.TopicName%"</a>&nbsp;im Forum&nbsp;<a href="%Forums.ForumURL%">"%Forums.ForumName%"</a>&nbsp;erstellt.</p> <p>Klicken Sie <a href="%Forums.TopicURL%">hier</a> f&uuml;r weitere Informationen.</p> <p>Autor des Beitrags:&nbsp;%Forums.PostAuthor%<br />Inhalt des Beitrags: %Forums.PostBody%</p> <p></p> <p>Mit freundlichen Gr&uuml;&szlig;en,</p> <p></p> <p>Ihr Shop-Team</p></td></tr></tbody></table></center>'
	WHERE [Name] = 'Forums.NewForumPost'
END
GO

IF EXISTS (SELECT 1 FROM [MessageTemplate] WHERE [Name] = 'Forums.NewForumTopic')
BEGIN
	UPDATE [MessageTemplate] SET [Body] = '<style type="text/css">address, blockquote, center, del, dir, div, dl, fieldset, form, h1, h2, h3, h4, h5, h6, hr, ins, isindex, menu, noframes, noscript, ol, p, pre, table{ margin:0px; } body, td, p{ font-size: 13px;font-family: ''Segoe UI'', Tahoma, Arial, Helvetica, sans-serif; line-height: 18px; color: #163764; } body{ background:#efefef; } p{ margin-top: 0px; margin-bottom: 10px; } img{ border:0px; } th{ font-weight:bold; color: #ffffff; padding: 5px 0 5px 0; } ul{ list-style-type: square; } li{ line-height: normal; margin-bottom: 5px; } .template-body { width:800px; padding: 10px; border: 1px solid #ccc; } .attr-caption { font-weight: bold; text-align:right; } .attr-value { text-align:right; min-width:158px; width:160px; }</style><center><table border="0" cellpadding="0" cellspacing="0" align="center" bgcolor="#ffffff" class="template-body"><tbody><tr><td><p><a href="%Store.URL%">%Store.Name%</a>&nbsp;</p> <p></p> <p>Ein neuer Beitrag <a href="%Forums.TopicURL%">"%Forums.TopicName%"</a>&nbsp;wurde im Forum &nbsp;<a href="%Forums.ForumURL%">"%Forums.ForumName%"</a>&nbsp;erstellt.</p> <p>Klicken Sie <a href="%Forums.TopicURL%">hier</a> f&uuml;r weitere Informationen.</p> <p></p> <p>Mit freundlichen Gr&uuml;&szlig;en,</p> <p></p> <p>Ihr Shop-Team</p></td></tr></tbody></table></center>'
	WHERE [Name] = 'Forums.NewForumTopic'
END
GO

IF EXISTS (SELECT 1 FROM [MessageTemplate] WHERE [Name] = 'GiftCard.Notification')
BEGIN
	UPDATE [MessageTemplate] SET [Body] = '<style type="text/css">address, blockquote, center, del, dir, div, dl, fieldset, form, h1, h2, h3, h4, h5, h6, hr, ins, isindex, menu, noframes, noscript, ol, p, pre, table{ margin:0px; } body, td, p{ font-size: 13px;font-family: ''Segoe UI'', Tahoma, Arial, Helvetica, sans-serif; line-height: 18px; color: #163764; } body{ background:#efefef; } p{ margin-top: 0px; margin-bottom: 10px; } img{ border:0px; } th{ font-weight:bold; color: #ffffff; padding: 5px 0 5px 0; } ul{ list-style-type: square; } li{ line-height: normal; margin-bottom: 5px; } .template-body { width:800px; padding: 10px; border: 1px solid #ccc; } .attr-caption { font-weight: bold; text-align:right; } .attr-value { text-align:right; min-width:158px; width:160px; }</style><center><table border="0" cellpadding="0" cellspacing="0" align="center" bgcolor="#ffffff" class="template-body"><tbody><tr><td><p>Sehr geehrte(r)&nbsp;%GiftCard.RecipientName%,</p> <p></p> <p>Sie haben einen Geschenkgutschein in H&ouml;he von %GiftCard.Amount%&nbsp;f&uuml;r den Online-Shop&nbsp;%Store.Name% erhalten</p> <p>Ihr Gutscheincode lautet&nbsp;%GiftCard.CouponCode%</p> <p>Diese Nachricht wurde mit gesendet:</p> <p>%GiftCard.Message%</p> <p></p> <p>Mit freundlichen Gr&uuml;&szlig;en,</p> <p></p> <p>Ihr&nbsp;%Store.Name% - Team</p></td></tr></tbody></table></center>'
	WHERE [Name] = 'GiftCard.Notification'
END
GO

IF EXISTS (SELECT 1 FROM [MessageTemplate] WHERE [Name] = 'NewCustomer.Notification')
BEGIN
	UPDATE [MessageTemplate] SET [Body] = '<style type="text/css">address, blockquote, center, del, dir, div, dl, fieldset, form, h1, h2, h3, h4, h5, h6, hr, ins, isindex, menu, noframes, noscript, ol, p, pre, table{ margin:0px; } body, td, p{ font-size: 13px;font-family: ''Segoe UI'', Tahoma, Arial, Helvetica, sans-serif; line-height: 18px; color: #163764; } body{ background:#efefef; } p{ margin-top: 0px; margin-bottom: 10px; } img{ border:0px; } th{ font-weight:bold; color: #ffffff; padding: 5px 0 5px 0; } ul{ list-style-type: square; } li{ line-height: normal; margin-bottom: 5px; } .template-body { width:800px; padding: 10px; border: 1px solid #ccc; } .attr-caption { font-weight: bold; text-align:right; } .attr-value { text-align:right; min-width:158px; width:160px; }</style><center><table border="0" cellpadding="0" cellspacing="0" align="center" bgcolor="#ffffff" class="template-body"><tbody><tr><td><p><a href="%Store.URL%">%Store.Name%</a>&nbsp;</p> <p>Ein neuer Kunde hat sich registriert:<br /><br />Name: %Customer.FullName%<br />E-Mail: %Customer.Email%</p></td></tr></tbody></table></center>'
	WHERE [Name] = 'NewCustomer.Notification'
END
GO

IF EXISTS (SELECT 1 FROM [MessageTemplate] WHERE [Name] = 'NewReturnRequest.StoreOwnerNotification')
BEGIN
	UPDATE [MessageTemplate] SET [Body] = '<style type="text/css">address, blockquote, center, del, dir, div, dl, fieldset, form, h1, h2, h3, h4, h5, h6, hr, ins, isindex, menu, noframes, noscript, ol, p, pre, table{ margin:0px; } body, td, p{ font-size: 13px;font-family: ''Segoe UI'', Tahoma, Arial, Helvetica, sans-serif; line-height: 18px; color: #163764; } body{ background:#efefef; } p{ margin-top: 0px; margin-bottom: 10px; } img{ border:0px; } th{ font-weight:bold; color: #ffffff; padding: 5px 0 5px 0; } ul{ list-style-type: square; } li{ line-height: normal; margin-bottom: 5px; } .template-body { width:800px; padding: 10px; border: 1px solid #ccc; } .attr-caption { font-weight: bold; text-align:right; } .attr-value { text-align:right; min-width:158px; width:160px; }</style><center><table border="0" cellpadding="0" cellspacing="0" align="center" bgcolor="#ffffff" class="template-body"><tbody><tr><td><p><a href="%Store.URL%">%Store.Name%</a>&nbsp;</p> <p>%Customer.FullName% hat eine R&uuml;ckgabe-Anforderung geschickt.&nbsp;</p> <p>Anforderungs-ID: %ReturnRequest.ID%<br />Artikel: %ReturnRequest.Product.Quantity% x %ReturnRequest.Product.Name%<br />R&uuml;ckgabegrund: %ReturnRequest.Reason%<br />Gew&uuml;nschte Aktion: %ReturnRequest.RequestedAction%<br />Nachricht vom Kunden:<br />%ReturnRequest.CustomerComment%</p></td></tr></tbody></table></center>'
	WHERE [Name] = 'NewReturnRequest.StoreOwnerNotification'
END
GO

IF EXISTS (SELECT 1 FROM [MessageTemplate] WHERE [Name] = 'News.NewsComment')
BEGIN
	UPDATE [MessageTemplate] SET [Body] = '<style type="text/css">address, blockquote, center, del, dir, div, dl, fieldset, form, h1, h2, h3, h4, h5, h6, hr, ins, isindex, menu, noframes, noscript, ol, p, pre, table{ margin:0px; } body, td, p{ font-size: 13px;font-family: ''Segoe UI'', Tahoma, Arial, Helvetica, sans-serif; line-height: 18px; color: #163764; } body{ background:#efefef; } p{ margin-top: 0px; margin-bottom: 10px; } img{ border:0px; } th{ font-weight:bold; color: #ffffff; padding: 5px 0 5px 0; } ul{ list-style-type: square; } li{ line-height: normal; margin-bottom: 5px; } .template-body { width:800px; padding: 10px; border: 1px solid #ccc; } .attr-caption { font-weight: bold; text-align:right; } .attr-value { text-align:right; min-width:158px; width:160px; }</style><center><table border="0" cellpadding="0" cellspacing="0" align="center" bgcolor="#ffffff" class="template-body"><tbody><tr><td><p><a href="%Store.URL%">%Store.Name%</a>&nbsp;</p> <p>Zu der News "%NewsComment.NewsTitle%" wurde ein neuer Kommentar eingestellt.</p></td></tr></tbody></table></center>'
	WHERE [Name] = 'News.NewsComment'
END
GO

IF EXISTS (SELECT 1 FROM [MessageTemplate] WHERE [Name] = 'NewsLetterSubscription.ActivationMessage')
BEGIN
	UPDATE [MessageTemplate] SET [Body] = '<style type="text/css">address, blockquote, center, del, dir, div, dl, fieldset, form, h1, h2, h3, h4, h5, h6, hr, ins, isindex, menu, noframes, noscript, ol, p, pre, table{ margin:0px; } body, td, p{ font-size: 13px;font-family: ''Segoe UI'', Tahoma, Arial, Helvetica, sans-serif; line-height: 18px; color: #163764; } body{ background:#efefef; } p{ margin-top: 0px; margin-bottom: 10px; } img{ border:0px; } th{ font-weight:bold; color: #ffffff; padding: 5px 0 5px 0; } ul{ list-style-type: square; } li{ line-height: normal; margin-bottom: 5px; } .template-body { width:800px; padding: 10px; border: 1px solid #ccc; } .attr-caption { font-weight: bold; text-align:right; } .attr-value { text-align:right; min-width:158px; width:160px; }</style><center><table border="0" cellpadding="0" cellspacing="0" align="center" bgcolor="#ffffff" class="template-body"><tbody><tr><td><p><a href="%NewsLetterSubscription.ActivationUrl%">Klicken Sie hier, um Ihre Newsletter-Registrierung zu stornieren.</a></p> <p>Sollten Sie diese E-Mail f&auml;lschlich erhalten haben, l&ouml;schen Sie bitte diese E-Mail.</p> <p></p> <p>Ihr&nbsp;%Store.Name% - Team</p></td></tr></tbody></table></center>'
	WHERE [Name] = 'NewsLetterSubscription.ActivationMessage'
END
GO

IF EXISTS (SELECT 1 FROM [MessageTemplate] WHERE [Name] = 'NewsLetterSubscription.DeactivationMessage')
BEGIN
	UPDATE [MessageTemplate] SET [Body] = '<style type="text/css">address, blockquote, center, del, dir, div, dl, fieldset, form, h1, h2, h3, h4, h5, h6, hr, ins, isindex, menu, noframes, noscript, ol, p, pre, table{ margin:0px; } body, td, p{ font-size: 13px;font-family: ''Segoe UI'', Tahoma, Arial, Helvetica, sans-serif; line-height: 18px; color: #163764; } body{ background:#efefef; } p{ margin-top: 0px; margin-bottom: 10px; } img{ border:0px; } th{ font-weight:bold; color: #ffffff; padding: 5px 0 5px 0; } ul{ list-style-type: square; } li{ line-height: normal; margin-bottom: 5px; } .template-body { width:800px; padding: 10px; border: 1px solid #ccc; } .attr-caption { font-weight: bold; text-align:right; } .attr-value { text-align:right; min-width:158px; width:160px; }</style><center><table border="0" cellpadding="0" cellspacing="0" align="center" bgcolor="#ffffff" class="template-body"><tbody><tr><td><p><a href="%Store.URL%">%Store.Name%</a>&nbsp;<br /><br />%Customer.FullName% (%Customer.Email%) hat eine neue Umsatzsteuer-ID &uuml;bermittelt:</p> <p><br />Umsatzsteuer-ID: %Customer.VatNumber%<br />Status: %Customer.VatNumberStatus%<br />&Uuml;bermittelt von: %VatValidationResult.Name% -&nbsp;%VatValidationResult.Address%</p></td></tr></tbody></table></center>'
	WHERE [Name] = 'NewsLetterSubscription.DeactivationMessage'
END
GO

IF EXISTS (SELECT 1 FROM [MessageTemplate] WHERE [Name] = 'NewVATSubmitted.StoreOwnerNotification')
BEGIN
	UPDATE [MessageTemplate] SET [Body] = '<style type="text/css">address, blockquote, center, del, dir, div, dl, fieldset, form, h1, h2, h3, h4, h5, h6, hr, ins, isindex, menu, noframes, noscript, ol, p, pre, table{ margin:0px; } body, td, p{ font-size: 13px;font-family: ''Segoe UI'', Tahoma, Arial, Helvetica, sans-serif; line-height: 18px; color: #163764; } body{ background:#efefef; } p{ margin-top: 0px; margin-bottom: 10px; } img{ border:0px; } th{ font-weight:bold; color: #ffffff; padding: 5px 0 5px 0; } ul{ list-style-type: square; } li{ line-height: normal; margin-bottom: 5px; } .template-body { width:800px; padding: 10px; border: 1px solid #ccc; } .attr-caption { font-weight: bold; text-align:right; } .attr-value { text-align:right; min-width:158px; width:160px; }</style><center><table border="0" cellpadding="0" cellspacing="0" align="center" bgcolor="#ffffff" class="template-body"><tbody><tr><td><p><a href="%Store.URL%">%Store.Name%</a></p> <p>Sehr geehrte(r) Herr/Frau %Order.CustomerFullName%,&nbsp;</p> <p>Ihr Auftrag wurde storniert. Details finden Sie unten.<br /><br />Auftragsnummer: %Order.OrderNumber%<br />Auftrags-Details: <a target="_blank" href="%Order.OrderURLForCustomer%">%Order.OrderURLForCustomer%</a><br />Auftrags-Datum: %Order.CreatedOn%<br /><br /><br /><br />Rechnungsadresse<br />%Order.BillingFirstName% %Order.BillingLastName%<br />%Order.BillingAddress1%<br />%Order.BillingCity% %Order.BillingZipPostalCode%<br />%Order.BillingStateProvince% %Order.BillingCountry%<br /><br /><br /><br />Lieferadresse<br />%Order.ShippingFirstName% %Order.ShippingLastName%<br />%Order.ShippingAddress1%<br />%Order.ShippingCity% %Order.ShippingZipPostalCode%<br />%Order.ShippingStateProvince% %Order.ShippingCountry%<br /><br />Versandart: %Order.ShippingMethod%<br />Zahlart: %Order.PaymentMethod%<br /><br />%Order.Product(s)%</p> <p></p> <p>Mit freundlichen Gr&uuml;&szlig;en,</p> <p></p> <p>Ihr&nbsp;%Store.Name% - Team</p></td></tr></tbody></table></center>'
	WHERE [Name] = 'NewVATSubmitted.StoreOwnerNotification'
END
GO

IF EXISTS (SELECT 1 FROM [MessageTemplate] WHERE [Name] = 'OrderCancelled.CustomerNotification')
BEGIN
	UPDATE [MessageTemplate] SET [Body] = '<style type="text/css">address, blockquote, center, del, dir, div, dl, fieldset, form, h1, h2, h3, h4, h5, h6, hr, ins, isindex, menu, noframes, noscript, ol, p, pre, table{ margin:0px; } body, td, p{ font-size: 13px;font-family: ''Segoe UI'', Tahoma, Arial, Helvetica, sans-serif; line-height: 18px; color: #163764; } body{ background:#efefef; } p{ margin-top: 0px; margin-bottom: 10px; } img{ border:0px; } th{ font-weight:bold; color: #ffffff; padding: 5px 0 5px 0; } ul{ list-style-type: square; } li{ line-height: normal; margin-bottom: 5px; } .template-body { width:800px; padding: 10px; border: 1px solid #ccc; } .attr-caption { font-weight: bold; text-align:right; } .attr-value { text-align:right; min-width:158px; width:160px; }</style><center><table border="0" cellpadding="0" cellspacing="0" align="center" bgcolor="#ffffff" class="template-body"><tbody><tr><td><p><a href="%Store.URL%">%Store.Name%</a></p> <p>Sehr geehrte(r) Herr/Frau %Order.CustomerFullName%,&nbsp;</p> <p>Ihr Auftrag wurde storniert. Details finden Sie unten.<br /><br />Auftragsnummer: %Order.OrderNumber%<br />Auftrags-Details: <a target="_blank" href="%Order.OrderURLForCustomer%">%Order.OrderURLForCustomer%</a><br />Auftrags-Datum: %Order.CreatedOn%<br /><br /><br /><br />Rechnungsadresse<br />%Order.BillingFirstName% %Order.BillingLastName%<br />%Order.BillingAddress1%<br />%Order.BillingCity% %Order.BillingZipPostalCode%<br />%Order.BillingStateProvince% %Order.BillingCountry%<br /><br /><br /><br />Lieferadresse<br />%Order.ShippingFirstName% %Order.ShippingLastName%<br />%Order.ShippingAddress1%<br />%Order.ShippingCity% %Order.ShippingZipPostalCode%<br />%Order.ShippingStateProvince% %Order.ShippingCountry%<br /><br />Versandart: %Order.ShippingMethod%<br /><br />%Order.Product(s)%</p> <p></p> <p>Mit freundlichen Gr&uuml;&szlig;en,</p> <p></p> <p>Ihr&nbsp;%Store.Name% - Team</p></td></tr></tbody></table></center>'
	WHERE [Name] = 'OrderCancelled.CustomerNotification'
END
GO

IF EXISTS (SELECT 1 FROM [MessageTemplate] WHERE [Name] = 'OrderCompleted.CustomerNotification')
BEGIN
	UPDATE [MessageTemplate] SET [Body] = '<style type="text/css">address, blockquote, center, del, dir, div, dl, fieldset, form, h1, h2, h3, h4, h5, h6, hr, ins, isindex, menu, noframes, noscript, ol, p, pre, table{ margin:0px; } body, td, p{ font-size: 13px;font-family: ''Segoe UI'', Tahoma, Arial, Helvetica, sans-serif; line-height: 18px; color: #163764; } body{ background:#efefef; } p{ margin-top: 0px; margin-bottom: 10px; } img{ border:0px; } th{ font-weight:bold; color: #ffffff; padding: 5px 0 5px 0; } ul{ list-style-type: square; } li{ line-height: normal; margin-bottom: 5px; } .template-body { width:800px; padding: 10px; border: 1px solid #ccc; } .attr-caption { font-weight: bold; text-align:right; } .attr-value { text-align:right; min-width:158px; width:160px; }</style><center><table border="0" cellpadding="0" cellspacing="0" align="center" bgcolor="#ffffff" class="template-body"><tbody><tr><td><p><a href="%Store.URL%">%Store.Name%</a>&nbsp;<br /><br />Sehr geehrte(r) Herr/Frau %Order.CustomerFullName%,&nbsp;</p> <p>Ihre Bestellung wurde bearbeitet.&nbsp;</p> <p></p> <p>Auftrags-Nummer: %Order.OrderNumber%<br />Details zum Auftrag:&nbsp;<a target="_blank" href="%Order.OrderURLForCustomer%">%Order.OrderURLForCustomer%</a><br />Datum: %Order.CreatedOn%<br /><br /><br /><br />Rechnungsadresse<br />%Order.BillingFirstName% %Order.BillingLastName%<br />%Order.BillingAddress1%<br />%Order.BillingCity% %Order.BillingZipPostalCode%<br />%Order.BillingStateProvince% %Order.BillingCountry%<br /><br /><br /><br />Lieferadresse<br />%Order.ShippingFirstName% %Order.ShippingLastName%<br />%Order.ShippingAddress1%<br />%Order.ShippingCity% %Order.ShippingZipPostalCode%<br />%Order.ShippingStateProvince% %Order.ShippingCountry%<br /><br />Versandart: %Order.ShippingMethod%<br />Zahlart: %Order.PaymentMethod%<br /><br />%Order.Product(s)%</p> <p></p> <p>Mit freundlichen Gr&uuml;&szlig;en,</p> <p></p> <p>Ihr&nbsp;%Store.Name% - Team</p></td></tr></tbody></table></center>'
	WHERE [Name] = 'OrderCompleted.CustomerNotification'
END
GO

IF EXISTS (SELECT 1 FROM [MessageTemplate] WHERE [Name] = 'ShipmentDelivered.CustomerNotification')
BEGIN
	UPDATE [MessageTemplate] SET [Body] = '<style type="text/css">address, blockquote, center, del, dir, div, dl, fieldset, form, h1, h2, h3, h4, h5, h6, hr, ins, isindex, menu, noframes, noscript, ol, p, pre, table{ margin:0px; } body, td, p{ font-size: 13px;font-family: ''Segoe UI'', Tahoma, Arial, Helvetica, sans-serif; line-height: 18px; color: #163764; } body{ background:#efefef; } p{ margin-top: 0px; margin-bottom: 10px; } img{ border:0px; } th{ font-weight:bold; color: #ffffff; padding: 5px 0 5px 0; } ul{ list-style-type: square; } li{ line-height: normal; margin-bottom: 5px; } .template-body { width:800px; padding: 10px; border: 1px solid #ccc; } .attr-caption { font-weight: bold; text-align:right; } .attr-value { text-align:right; min-width:158px; width:160px; }</style><center><table border="0" cellpadding="0" cellspacing="0" align="center" bgcolor="#ffffff" class="template-body"><tbody><tr><td><p><a href="%Store.URL%">%Store.Name%</a>&nbsp;<br /><br />Sehr geehrte(r) Herr/Frau %Order.CustomerFullName%,&nbsp;</p> <p>Ihre Bestellung wurde ausgeliefert.</p> <p>Auftrags-Nummer: %Order.OrderNumber%<br />Auftrags-Details:&nbsp;<a href="%Order.OrderURLForCustomer%" target="_blank">%Order.OrderURLForCustomer%</a><br />Datum: %Order.CreatedOn%<br /><br /><br /><br />Rechnungsadresse<br />%Order.BillingFirstName% %Order.BillingLastName%<br />%Order.BillingAddress1%<br />%Order.BillingCity% %Order.BillingZipPostalCode%<br />%Order.BillingStateProvince% %Order.BillingCountry%<br /><br /><br /><br />Lieferadresse<br />%Order.ShippingFirstName% %Order.ShippingLastName%<br />%Order.ShippingAddress1%<br />%Order.ShippingCity% %Order.ShippingZipPostalCode%<br />%Order.ShippingStateProvince% %Order.ShippingCountry%<br /><br />Versandart: %Order.ShippingMethod%&nbsp;<br />Zahlart: %Order.PaymentMethod%<br /><br />Gelieferte Artikel:&nbsp;<br /><br />%Shipment.Product(s)%</p></td></tr></tbody></table></center>'
	WHERE [Name] = 'ShipmentDelivered.CustomerNotification'
END
GO

IF EXISTS (SELECT 1 FROM [MessageTemplate] WHERE [Name] = 'OrderPlaced.CustomerNotification')
BEGIN
	UPDATE [MessageTemplate] SET [Body] = '<style type="text/css">address, blockquote, center, del, dir, div, dl, fieldset, form, h1, h2, h3, h4, h5, h6, hr, ins, isindex, menu, noframes, noscript, ol, p, pre, table{ margin:0px; } body, td, p{ font-size: 13px;font-family: ''Segoe UI'', Tahoma, Arial, Helvetica, sans-serif; line-height: 18px; color: #163764; } body{ background:#efefef; } p{ margin-top: 0px; margin-bottom: 10px; } img{ border:0px; } th{ font-weight:bold; color: #ffffff; padding: 5px 0 5px 0; } ul{ list-style-type: square; } li{ line-height: normal; margin-bottom: 5px; } .template-body { width:800px; padding: 10px; border: 1px solid #ccc; } .attr-caption { font-weight: bold; text-align:right; } .attr-value { text-align:right; min-width:158px; width:160px; } .legal-infos, .legal-infos p { font-size:11px; color: #aaa}</style><center><table border="0" cellpadding="0" cellspacing="0" align="center" bgcolor="#ffffff" class="template-body"><tbody><tr><td><p><a href="%Store.URL%">%Store.Name%</a> <br /><br />Sehr geehrte(r) Herr/Frau %Order.CustomerFullName%, <br /> Vielen Dank f&uuml;r Ihre Bestellung bei <a href="%Store.URL%">%Store.Name%</a>. Eine &Uuml;bersicht &uuml;ber Ihre Bestellung finden Sie unten. <br /><br />Order Number: %Order.OrderNumber%<br /> Bestell&uuml;bersicht: <a target="_blank" href="%Order.OrderURLForCustomer%">%Order.OrderURLForCustomer%</a><br /> Datum: %Order.CreatedOn%<br /><br /><br /><br /> Rechnungsadresse<br />%Order.BillingFirstName% %Order.BillingLastName%<br />%Order.BillingAddress1%<br />%Order.BillingCity% %Order.BillingZipPostalCode%<br /> %Order.BillingStateProvince% %Order.BillingCountry%<br /><br /><br /><br /> Lieferadresse<br />%Order.ShippingFirstName% %Order.ShippingLastName%<br />%Order.ShippingAddress1%<br /> %Order.ShippingCity% %Order.ShippingZipPostalCode%<br />%Order.ShippingStateProvince% %Order.ShippingCountry%<br /><br /> Versandart: %Order.ShippingMethod%<br /> Zahlart: %Order.PaymentMethod%<br /><br />%Order.Product(s)%</p><p></p><p>%Order.ConditionsOfUse%</p><p>%Order.Disclaimer%</p></td></tr></tbody></table></center>'
	WHERE [Name] = 'OrderPlaced.CustomerNotification'
END
GO

IF EXISTS (SELECT 1 FROM [MessageTemplate] WHERE [Name] = 'OrderPlaced.StoreOwnerNotification')
BEGIN
	UPDATE [MessageTemplate] SET [Body] = '<style type="text/css">address, blockquote, center, del, dir, div, dl, fieldset, form, h1, h2, h3, h4, h5, h6, hr, ins, isindex, menu, noframes, noscript, ol, p, pre, table{ margin:0px; } body, td, p{ font-size: 13px;font-family: ''Segoe UI'', Tahoma, Arial, Helvetica, sans-serif; line-height: 18px; color: #163764; } body{ background:#efefef; } p{ margin-top: 0px; margin-bottom: 10px; } img{ border:0px; } th{ font-weight:bold; color: #ffffff; padding: 5px 0 5px 0; } ul{ list-style-type: square; } li{ line-height: normal; margin-bottom: 5px; } .template-body { width:800px; padding: 10px; border: 1px solid #ccc; } .attr-caption { font-weight: bold; text-align:right; } .attr-value { text-align:right; min-width:158px; width:160px; }</style><center><table border="0" cellpadding="0" cellspacing="0" align="center" bgcolor="#ffffff" class="template-body"><tbody><tr><td><p><a href="%Store.URL%">%Store.Name%</a>&nbsp;</p> <p></p> <p>Eine neue Bestellung wurde get&auml;tigt:</p> <p><br />Kunden: %Order.CustomerFullName% (%Order.CustomerEmail%) .&nbsp;<br /><br />Auftrags-Nummer: %Order.OrderNumber%<br />Datum: %Order.CreatedOn%<br /><br /><br /><br />Rechnungsadresse<br />%Order.BillingFirstName% %Order.BillingLastName%<br />%Order.BillingAddress1%<br />%Order.BillingCity% %Order.BillingZipPostalCode%<br />%Order.BillingStateProvince% %Order.BillingCountry%<br /><br /><br /><br />Lieferadresse<br />%Order.ShippingFirstName% %Order.ShippingLastName%<br />%Order.ShippingAddress1%<br />%Order.ShippingCity% %Order.ShippingZipPostalCode%<br />%Order.ShippingStateProvince% %Order.ShippingCountry%<br /><br />Versandart: %Order.ShippingMethod% <br /> Zahlart: %Order.PaymentMethod%<br /><br />%Order.Product(s)%</p></td></tr></tbody></table></center>'
	WHERE [Name] = 'OrderPlaced.StoreOwnerNotification'
END
GO

IF EXISTS (SELECT 1 FROM [MessageTemplate] WHERE [Name] = 'ShipmentSent.CustomerNotification')
BEGIN
	UPDATE [MessageTemplate] SET [Body] = '<style type="text/css">address, blockquote, center, del, dir, div, dl, fieldset, form, h1, h2, h3, h4, h5, h6, hr, ins, isindex, menu, noframes, noscript, ol, p, pre, table{ margin:0px; } body, td, p{ font-size: 13px;font-family: ''Segoe UI'', Tahoma, Arial, Helvetica, sans-serif; line-height: 18px; color: #163764; } body{ background:#efefef; } p{ margin-top: 0px; margin-bottom: 10px; } img{ border:0px; } th{ font-weight:bold; color: #ffffff; padding: 5px 0 5px 0; } ul{ list-style-type: square; } li{ line-height: normal; margin-bottom: 5px; } .template-body { width:800px; padding: 10px; border: 1px solid #ccc; } .attr-caption { font-weight: bold; text-align:right; } .attr-value { text-align:right; min-width:158px; width:160px; }</style><center><table border="0" cellpadding="0" cellspacing="0" align="center" bgcolor="#ffffff" class="template-body"><tbody><tr><td><p><a href="%Store.URL%">%Store.Name%</a>&nbsp;<br /><br />Sehr geehrter Herr/Frau %Order.CustomerFullName%,&nbsp;</p> <p><br />Ihre Bestellung wurde soeben versendet:</p> <p><br />Auftrags-Nummer: %Order.OrderNumber%<br />Auftrags-Details:&nbsp;<a href="%Order.OrderURLForCustomer%" target="_blank">%Order.OrderURLForCustomer%</a><br />Datum: %Order.CreatedOn%<br /><br /><br />Rechnungsadresse<br />%Order.BillingFirstName% %Order.BillingLastName%<br />%Order.BillingAddress1%<br />%Order.BillingCity% %Order.BillingZipPostalCode%<br />%Order.BillingStateProvince% %Order.BillingCountry%<br /><br /><br /><br />Lieferadresse<br />%Order.ShippingFirstName% %Order.ShippingLastName%<br />%Order.ShippingAddress1%<br />%Order.ShippingCity% %Order.ShippingZipPostalCode%<br />%Order.ShippingStateProvince% %Order.ShippingCountry%<br /><br />Versandart: %Order.ShippingMethod%&nbsp;<br />Zahlart: %Order.PaymentMethod%<br /><br />Versendete Artikel:&nbsp;<br /><br />%Shipment.Product(s)%</p></td></tr></tbody></table></center>'
	WHERE [Name] = 'ShipmentSent.CustomerNotification'
END
GO

IF EXISTS (SELECT 1 FROM [MessageTemplate] WHERE [Name] = 'Product.ProductReview')
BEGIN
	UPDATE [MessageTemplate] SET [Body] = '<style type="text/css">address, blockquote, center, del, dir, div, dl, fieldset, form, h1, h2, h3, h4, h5, h6, hr, ins, isindex, menu, noframes, noscript, ol, p, pre, table{ margin:0px; } body, td, p{ font-size: 13px;font-family: ''Segoe UI'', Tahoma, Arial, Helvetica, sans-serif; line-height: 18px; color: #163764; } body{ background:#efefef; } p{ margin-top: 0px; margin-bottom: 10px; } img{ border:0px; } th{ font-weight:bold; color: #ffffff; padding: 5px 0 5px 0; } ul{ list-style-type: square; } li{ line-height: normal; margin-bottom: 5px; } .template-body { width:800px; padding: 10px; border: 1px solid #ccc; } .attr-caption { font-weight: bold; text-align:right; } .attr-value { text-align:right; min-width:158px; width:160px; }</style><center><table border="0" cellpadding="0" cellspacing="0" align="center" bgcolor="#ffffff" class="template-body"><tbody><tr><td><p><a href="%Store.URL%">%Store.Name%</a>&nbsp;</p> <p>Eine neue Produktrezension zu dem Produkt&nbsp;"%ProductReview.ProductName%" wurde verfasst.<br /><br /></p></td></tr></tbody></table></center>'
	WHERE [Name] = 'Product.ProductReview'
END
GO


IF EXISTS (SELECT 1 FROM [MessageTemplate] WHERE [Name] = 'QuantityBelow.StoreOwnerNotification')
BEGIN
	UPDATE [MessageTemplate] SET [Body] = '<style type="text/css">address, blockquote, center, del, dir, div, dl, fieldset, form, h1, h2, h3, h4, h5, h6, hr, ins, isindex, menu, noframes, noscript, ol, p, pre, table{ margin:0px; } body, td, p{ font-size: 13px;font-family: ''Segoe UI'', Tahoma, Arial, Helvetica, sans-serif; line-height: 18px; color: #163764; } body{ background:#efefef; } p{ margin-top: 0px; margin-bottom: 10px; } img{ border:0px; } th{ font-weight:bold; color: #ffffff; padding: 5px 0 5px 0; } ul{ list-style-type: square; } li{ line-height: normal; margin-bottom: 5px; } .template-body { width:800px; padding: 10px; border: 1px solid #ccc; } .attr-caption { font-weight: bold; text-align:right; } .attr-value { text-align:right; min-width:158px; width:160px; }</style><center><table border="0" cellpadding="0" cellspacing="0" align="center" bgcolor="#ffffff" class="template-body"><tbody><tr><td><p><a href="%Store.URL%">%Store.Name%</a>&nbsp;</p> <p>Der Mindestlagerbestand f&uuml;r folgendes produkt wurde unterschritte;<br />%ProductVariant.FullProductName% (ID: %ProductVariant.ID%) &nbsp;<br /><br />Menge: %ProductVariant.StockQuantity%</p></td></tr></tbody></table></center>'
	WHERE [Name] = 'QuantityBelow.StoreOwnerNotification'
END
GO

IF EXISTS (SELECT 1 FROM [MessageTemplate] WHERE [Name] = 'ReturnRequestStatusChanged.CustomerNotification')
BEGIN
	UPDATE [MessageTemplate] SET [Body] = '<style type="text/css">address, blockquote, center, del, dir, div, dl, fieldset, form, h1, h2, h3, h4, h5, h6, hr, ins, isindex, menu, noframes, noscript, ol, p, pre, table{ margin:0px; } body, td, p{ font-size: 13px;font-family: ''Segoe UI'', Tahoma, Arial, Helvetica, sans-serif; line-height: 18px; color: #163764; } body{ background:#efefef; } p{ margin-top: 0px; margin-bottom: 10px; } img{ border:0px; } th{ font-weight:bold; color: #ffffff; padding: 5px 0 5px 0; } ul{ list-style-type: square; } li{ line-height: normal; margin-bottom: 5px; } .template-body { width:800px; padding: 10px; border: 1px solid #ccc; } .attr-caption { font-weight: bold; text-align:right; } .attr-value { text-align:right; min-width:158px; width:160px; }</style><center><table border="0" cellpadding="0" cellspacing="0" align="center" bgcolor="#ffffff" class="template-body"><tbody><tr><td><p><a href="%Store.URL%">%Store.Name%</a>&nbsp;<br /><br />Sehr geehrte(r) Herr/Frau %Customer.FullName%,</p> <p>der Status Ihrer R&uuml;cksendung&nbsp;#%ReturnRequest.ID% wurde aktualisiert.</p> <p></p> <p>Mit freundlichen Gr&uuml;&szlig;en,</p> <p></p> <p>Ihr&nbsp;%Store.Name% - Team</p></td></tr></tbody></table></center>'
	WHERE [Name] = 'ReturnRequestStatusChanged.CustomerNotification'
END
GO

IF EXISTS (SELECT 1 FROM [MessageTemplate] WHERE [Name] = 'Service.EmailAFriend')
BEGIN
	UPDATE [MessageTemplate] SET [Body] = '<style type="text/css">address, blockquote, center, del, dir, div, dl, fieldset, form, h1, h2, h3, h4, h5, h6, hr, ins, isindex, menu, noframes, noscript, ol, p, pre, table{ margin:0px; } body, td, p{ font-size: 13px;font-family: ''Segoe UI'', Tahoma, Arial, Helvetica, sans-serif; line-height: 18px; color: #163764; } body{ background:#efefef; } p{ margin-top: 0px; margin-bottom: 10px; } img{ border:0px; } th{ font-weight:bold; color: #ffffff; padding: 5px 0 5px 0; } ul{ list-style-type: square; } li{ line-height: normal; margin-bottom: 5px; } .template-body { width:800px; padding: 10px; border: 1px solid #ccc; } .attr-caption { font-weight: bold; text-align:right; } .attr-value { text-align:right; min-width:158px; width:160px; }</style><center><table border="0" cellpadding="0" cellspacing="0" align="center" bgcolor="#ffffff" class="template-body"><tbody><tr><td><p><a href="%Store.URL%">%Store.Name%</a>&nbsp;<br /><br />%EmailAFriend.Email% m&ouml;chte Ihnen bei %Store.Name% ein Produkt empfehlen:<br /><br /><b><a target="_blank" href="%Product.ProductURLForCustomer%">%Product.Name%</a></b>&nbsp;<br />%Product.ShortDescription%&nbsp;</p> <p></p> <p>Weitere Details finden Sie <a target="_blank" href="%Product.ProductURLForCustomer%">hier</a><br /><br /><br />%EmailAFriend.PersonalMessage%</p> <p></p> <p>Mit freundlichen Gr&uuml;&szlig;en,</p> <p><br />Ihr %Store.Name% - Team</p></td></tr></tbody></table></center>'
	WHERE [Name] = 'Service.EmailAFriend'
END
GO

IF EXISTS (SELECT 1 FROM [MessageTemplate] WHERE [Name] = 'Wishlist.EmailAFriend')
BEGIN
	UPDATE [MessageTemplate] SET [Body] = '<style type="text/css">address, blockquote, center, del, dir, div, dl, fieldset, form, h1, h2, h3, h4, h5, h6, hr, ins, isindex, menu, noframes, noscript, ol, p, pre, table{ margin:0px; } body, td, p{ font-size: 13px;font-family: ''Segoe UI'', Tahoma, Arial, Helvetica, sans-serif; line-height: 18px; color: #163764; } body{ background:#efefef; } p{ margin-top: 0px; margin-bottom: 10px; } img{ border:0px; } th{ font-weight:bold; color: #ffffff; padding: 5px 0 5px 0; } ul{ list-style-type: square; } li{ line-height: normal; margin-bottom: 5px; } .template-body { width:800px; padding: 10px; border: 1px solid #ccc; } .attr-caption { font-weight: bold; text-align:right; } .attr-value { text-align:right; min-width:158px; width:160px; }</style><center><table border="0" cellpadding="0" cellspacing="0" align="center" bgcolor="#ffffff" class="template-body"><tbody><tr><td><p><a href="%Store.URL%">%Store.Name%</a>&nbsp;<br /><br />%Wishlist.Email% m&ouml;chte mit Ihnen ihre/seine Wunschliste teilen.<br /><br /></p> <p>Um die Wunschliste einzusehen, klicken Sie bitte <a target="_blank" href="%Wishlist.URLForCustomer%">hier</a>.<br /><br /><br /></p> <p>%Wishlist.PersonalMessage%<br /><br />%Store.Name%</p></td></tr></tbody></table></center>'
	WHERE [Name] = 'Wishlist.EmailAFriend'
END
GO

IF EXISTS (SELECT 1 FROM [MessageTemplate] WHERE [Name] = 'Customer.NewOrderNote')
BEGIN
	UPDATE [MessageTemplate] SET [Body] = '<style type="text/css">address, blockquote, center, del, dir, div, dl, fieldset, form, h1, h2, h3, h4, h5, h6, hr, ins, isindex, menu, noframes, noscript, ol, p, pre, table{ margin:0px; } body, td, p{ font-size: 13px;font-family: ''Segoe UI'', Tahoma, Arial, Helvetica, sans-serif; line-height: 18px; color: #163764; } body{ background:#efefef; } p{ margin-top: 0px; margin-bottom: 10px; } img{ border:0px; } th{ font-weight:bold; color: #ffffff; padding: 5px 0 5px 0; } ul{ list-style-type: square; } li{ line-height: normal; margin-bottom: 5px; } .template-body { width:800px; padding: 10px; border: 1px solid #ccc; } .attr-caption { font-weight: bold; text-align:right; } .attr-value { text-align:right; min-width:158px; width:160px; }</style><center><table border="0" cellpadding="0" cellspacing="0" align="center" bgcolor="#ffffff" class="template-body"><tbody><tr><td><p><a href="%Store.URL%">%Store.Name%</a>&nbsp;</p> <p></p> <p>Sehr geehrte(r) Herr/Frau&nbsp;%Customer.FullName%,&nbsp;</p> <p></p> <p>Ihrem Auftrag wurde eine Notiz hinterlegt:</p> <p>"%Order.NewNoteText%".<br /><a target="_blank" href="%Order.OrderURLForCustomer%">%Order.OrderURLForCustomer%</a></p> <p></p> <p>Mit freundlichen Gr&uuml;&szlig;en,</p> <p></p> <p>Ihr Shop-Team</p></td></tr></tbody></table></center>'
	WHERE [Name] = 'Customer.NewOrderNote'
END
GO

IF EXISTS (SELECT 1 FROM [MessageTemplate] WHERE [Name] = 'RecurringPaymentCancelled.StoreOwnerNotification')
BEGIN
	UPDATE [MessageTemplate] SET [Body] = '<style type="text/css">address, blockquote, center, del, dir, div, dl, fieldset, form, h1, h2, h3, h4, h5, h6, hr, ins, isindex, menu, noframes, noscript, ol, p, pre, table{ margin:0px; } body, td, p{ font-size: 13px;font-family: ''Segoe UI'', Tahoma, Arial, Helvetica, sans-serif; line-height: 18px; color: #163764; } body{ background:#efefef; } p{ margin-top: 0px; margin-bottom: 10px; } img{ border:0px; } th{ font-weight:bold; color: #ffffff; padding: 5px 0 5px 0; } ul{ list-style-type: square; } li{ line-height: normal; margin-bottom: 5px; } .template-body { width:800px; padding: 10px; border: 1px solid #ccc; } .attr-caption { font-weight: bold; text-align:right; } .attr-value { text-align:right; min-width:158px; width:160px; }</style><center><table border="0" cellpadding="0" cellspacing="0" align="center" bgcolor="#ffffff" class="template-body"><tbody><tr><td><p><a href="%Store.URL%">%Store.Name%</a>&nbsp;</p> <p>Folgende wiederkehrende Zahlung wurde vom Kunden storniert:</p> <p>Zahlungs-ID=%RecurringPayment.ID%<br />Kunden-Name und E-Mail: %Customer.FullName% (%Customer.Email%)&nbsp;</p></td></tr></tbody></table></center>'
	WHERE [Name] = 'RecurringPaymentCancelled.StoreOwnerNotification'
END
GO