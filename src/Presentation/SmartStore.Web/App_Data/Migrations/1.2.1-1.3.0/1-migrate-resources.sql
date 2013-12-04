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
