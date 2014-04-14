# Introduction
The SmartStore.Net Web API allows direct access to the data of an online store. It is build on the most recent technologies Microsoft offers for web based data consuming, the ASP.NET Web API and the OData provider. This documentation gives you an overview of how to build an API client, also known as API consumer. It uses C# code but you can build your consumer in any common programming language. As OData is a standardized protocol there are a lot of [frameworks and toolkits][4] available for various platforms.

The ASP.NET Web API is a framework for building web APIs on top of the .NET framework. It uses HTTP as an application protocol (rather than a transport protocol) to return data based on the client requests. The Web API is able to return the data appropriately based on the media type specified with the request. By default it provides JSON and XML based responses.

The [Open Data Protocol (OData)][5] is a standardized web protocol that provides an uniform way to expose, structure, query and manipulate data using REST practices. OData also provides an uniform way to represent metadata about the data, allowing clients to know more about the type system, relationships and structure of the data.

# Prerequisites
The SmartStore.Net Web API requires configuration by the storekeeper to go into action. First of all he must install the Web API plugin in the backend of SmartStore.Net. The plugin technology gives him the opportunity to activate or deactivate the entire Web API at any time without any influence on the online store.

Next step is to configure the API on the plugin's configuration page. The main thing here is to provide individual members access to the API and the data of the online store. Therefore the storekeeper can create a public and a secret key for each registered member. Only a registered member with both keys has access to the API. To exclude a member from the API the storekeeper can either delete the keys of the member (permanent exclusion) or disable them (temporary exclusion). Roles and rights of a member are taken into consideration when accessing data via the API.

The consumer must transmit the public key through a custom HTTP header field. It identifies the member who is accessing the API. The secret key on the other hand should never ever be send over the wire! It is secret to the storekeeper and the member who is accessing the Web API. It is only used for encryption as described in the following chapters.

# The SmartStore.Net Web API in detail
You can consume API services through HTTP calls in a RESTful manner by using HTTP methods GET, POST (insert), PUT (update), PATCH (partially update) and DELETE. OData options (like $filter, $top, $select etc.) and API specific options (like SmNetFulfillCountry) can be transfered through query strings. The web API offers two services. The OData service (path `/odata/v1/`) provides all entity related data and the API service (path `/api/v1/`) provides all remaining resources.

Paging is required if you want to query multiple records. You can do that with OData $skip and $top. The maximum value for $top is returned in SmartStore-Net-Api-MaxTop header field.

A request body needs to be UTF8 encoded.

The OData metadata document
------
The metadata document describes the entity data model (EDM) of the OData service, using an XML language called the Conceptual Schema Definition Language (CSDL). The metadata document shows the structure of the data in the OData service and can be used to generate client code. It is the recommended overview for the consumer to indicate where to find a particular resource. To get the metadata document, send a

    GET http://localhost:1260/odata/v1/$metadata

Request HTTP header fields
------
**User-Agent** (optional): Short description of the API consumer. Example: `My shopping data consumer v.1.0`

**Accept** (required): The wanted response format. Example for JSON: `application/json, text/javascript, */*`. Example for XML: `application/atom+xml,application/atomsvc+xml,application/xml`

**Accept-Charset** (required): Always UTF-8.

**Content-Type** and **Content-Length** (conditional): Necessary for the methods POST, PUT, and PATCH, if new data is send via the HTTP body. Example: `application/json; charset=utf-8`

**Content-MD5** (optional): For methods POST, PUT, and PATCH. Authentication provides the error `ContentMd5NotMatching` if the hash does not match the one calculated by the server. This header field has no safety relevance, because the content carried out using the HMAC signature. Example: `lgifXydL3FhffpTIilkwOw==`

**SmartStore-Net-Api-Date** (required): Date and time of the request as coordinated universal time (UTC). Use ISO-8601 format including milliseconds. Example: `2013-11-09T11:42:48.4715986Z`

**SmartStore-Net-Api-PublicKey** (required): The public key of a member. Example: `0c6b33651708eb09c8a8d6036b79d739`

**Authorization** (required): The authorization schema and the HMAC signature, separated by a space. The schema is SmNetHmac plus its version. Example: `SmNetHmac1 +yvONYvJmQl19omu1uE3HVlQ7afd7Qqkk8DrNrfUbe8=`

Example of a complete request header:

    User-Agent: My shopping data consumer v.1.0
    Accept: application/json, text/javascript, */*
    Accept-Charset: UTF-8
    SmartStore-Net-Api-PublicKey: 0c6b33651708eb09c8a8d6036b79d739
    SmartStore-Net-Api-Date: 2013-11-09T11:42:48.4715986Z
    Content-Type: application/json; charset=utf-8
    Content-MD5: lgifXydL3FhffpTIilkwOw==
    Authorization: SmNetHmac1 +yvONYvJmQl19omu1uE3HVlQ7afd7Qqkk8DrNrfUbe8=


Response HTTP header fields
------
**SmartStore-Net-Api-Version**: The highest API version supported by the server (unsigned integer) and the version of the installed API plugin (floating-point value). The API version is only increased when there is a fundamental break in API development without the ability of downward compatibility. The plugin version is typically increased when the API has been extended, for example when new ressouces were added. 
Example: `1 1.0`

**SmartStore-Net-Api-Date**: The current server date and time in ISO-8601 UTC. Example: `2013-11-11T14:35:33.7772907Z`

**SmartStore-Net-Api-MaxTop**: The maximum value for OData $top option. Required for client driven paging.

**SmartStore-Net-Api-HmacResultDesc**: Short description of the result of the HMAC authentication. Only returned if the authentication failed. Example: `InvalidTimestamp` 

**SmartStore-Net-Api-HmacResultId**: Unsigned ID that represents the result of the HMAC authentication. Only returned if the authentication failed. Example: `5`

**WWW-Authenticate**: The name of the authentication method that failed. Note that CORS requests can lead to multiple of this header fields. The SmartStore.Net Web API returns `SmNetHmac1` if its authentication failed.

List with HmacResultId and HmacResultDesc:

- 0 Success
- 1 FailedForUnknownReason
- 2 ApiUnavailable
- 3 InvalidAuthorizationHeader
- 4 InvalidSignature
- 5 InvalidTimestamp
- 6 TimestampOutOfPeriod
- 7 TimestampOlderThanLastRequest
- 8 MissingMessageRepresentationParameter
- 9 ContentMd5NotMatching
- 10 UserUnknown
- 11 UserDisabled
- 12 UserInvalid
- 13 UserHasNoPermission

Frequent HTTP response status codes
------
**200 OK**: Standard response for successful HTTP requests.

**201 Created**: The request has been fulfilled and resulted in a new resource being created.

**204 No Content**: The server successfully processed the request, but is not returning any content. Usually used as a response to a successful delete request.

**400 BadRequest**: There is something wrong with the request, for instance an invalid or missing parameter.

**401 Unauthorized**: Authentication failed or the user is not authorized (in case of UserHasNoPermission). There is no content returned because no access can be granted to the client.

**403 Forbidden**: The resource cannot be accessed through the API, for example for security reasons.

**404 Not found**: The requested ressource cannot be found.

**406 Not Acceptable**: The requested resource is only capable of generating content not acceptable according to the accept headers sent in the request. Typically generated by OData provider.

**422 Unprocessable entity**: The request is ok but its processing failed due to sematic issues or unprocessable instructions. For example putting the payment status of an order to an unknown value.

**500 Internal Server Error**: A generic error message, given when no more specific message is suitable.

**501 Not implemented**: Accessing the ressource through the API is not implemented (yet).

Query options
------
[OData query options][2] allows to manipulate the querying of data like sorting, filtering, paging etc. They are send in the query string of the request URL. A more detailed overview can be found [here][3].

Custom query options should lighten the work with the SmartStore.Net Web API, especially when you work with entity relationships.

**SmNetFulfill{property_name}**: Entities are often in multiple relationships with other entities. In most cases an ID has to be set to create or to change such a relation. But mostly this ID is unknown to you. To reduce the amount of API round trips this option can set an entity relation indirectly. Example: You want to add a german address but you don't know the ID of the german country entity which is required for inserting an address. Rather than calling the API again to get the ID you can add the query option SmNetFulfillCountry=DE and the API resolves the relationship automatically. The API can fulfill the following properties:

- Country: The two or three letter ISO country code. Example: SmNetFulfillCountry=USA
- StateProvince: The abbreviation of a state province. Example for California: SmNetFulfillStateProvince=CA
- Language: The culture of a language. Example for German: SmNetFulfillLanguage=de-DE 
- Currency: The ISO code of a currency. Example for Euro: SmNetFulfillCurrency=EUR

# HMAC authentication
The SmartStore.Net Web API uses the HMAC authentication method to protect data from unauthorized access.

HMAC (hash-based message authentication code) is a sessionless authentication method, where the integrity of a resource request is guaranteed through a this request representing code (called message representation) and a cryptological hash function. The procedure is considered to be very safe. It is used for example by Amazon to protect their S3 Web services.

SmartStore.Net uses SHA-256 as hash function, which is why this authentication procedure is often called HMAC-SHA256.

The consumer must transmit a timestamp with each request which may not differ too far from the server time, otherwise the authentication will fail. The storekeeper can configure this time window in the plugin configuration. The default value is 15 minutes. To prevent replay attacks the timestamp must be younger than the one of a previous request.


Message representation
------
The message representation is a concatenation of various informations separated by newline characters. All values are required but can be empty (in case of Content-MD5). It must be build as follows:

    HTTP method\n
    Content-MD5\n
    Response content type (accept header)\n
    Canonicalized URI\n
    ISO-8601 UTC timestamp\n
    Public key

**HTTP method**: In lower case. Example: `post`

**Content-MD5**: The base64 encoded MD5 hash of the HTTP body, so `base64(md5Hash(content))`. For GET requests this fied is empty. The value should be equal to the optional Content-MD5 request header field. For example the MD5 hash for the content

    {"OrderId":152,"Note":"Hello world!","DisplayToCustomer":false,"CreatedOnUtc":"2013-11-09T11:15:00"}

is `lgifXydL3FhffpTIilkwOw==`.
    
**Response content type**: The value of the accept header field. In lower case. Example: `application/json, text/javascript, */*`

**Canonicalized URI**: The complete URI of the requested resource (including query string). In lower case. Example: `http://localhost:1260/odata/v1/ordernotes`

**ISO-8601 UTC timestamp**: Current UTC date and time including milliseconds. Must be equal to the request header field SmartStore-Net-Api-Date. Example: `2013-11-09T11:37:21.1918793Z`. The API accepts milliseconds with 7 and 3 digits. Can be created in C# as follows: `string timestamp = DateTime.UtcNow.ToString("o");`

**Public key**: The public key of the user. In lower case. Example: `0c6b33651708eb09c8a8d6036b79d739`

A complete message representation might look like this:

    post
    lgifXydL3FhffpTIilkwOw==
    application/json, text/javascript, */*
    http://localhost:1260/odata/v1/ordernotes
    2013-11-09T11:42:48.4715986Z
    0c6b33651708eb09c8a8d6036b79d739 

HMAC signature
------
The signature is the computed hash of the message representation by using SHA-256 and the user's secret key. Always UTF8 encode the secret key and the message representation while calculating the hash. Example of creating the HMAC signature:

    public string CreateSignature(string secretKey, string messageRepresentation)
    {
    	if (string.IsNullOrWhiteSpace(secretKey) || string.IsNullOrWhiteSpace(messageRepresentation))
    		return "";
    
    	string signature;
    	var secretBytes = Encoding.UTF8.GetBytes(secretKey);
    	var valueBytes = Encoding.UTF8.GetBytes(messageRepresentation);
    
    	using (var hmac = new HMACSHA256(secretBytes))
    	{
    		var hash = hmac.ComputeHash(valueBytes);
    		signature = Convert.ToBase64String(hash);
    	}
    	return signature;
    }

The signature is transmitted as a base64 encoded string together with the schema using authorization header field.
For above message representation and the secret key `3025c89ebaab20b71e0e42744239bf50` the authorization header field is

    Authorization: SmNetHmac1 +yvONYvJmQl19omu1uE3HVlQ7afd7Qqkk8DrNrfUbe8=

# Code examples
See also [this tutorial][1] for more information about using the ASP.NET web API client libraries.

Making a GET request
------
Let us read orders created after a particular date. For security reasons query results needs to be paged, so we have to specify the $top option (and optionally $skip). 

    string publicKey = "0c6b33651708eb09c8a8d6036b79d739";
    string secretKey = "3025c89ebaab20b71e0e42744239bf50";
    string method = "get";
    string accept = "application/json, text/javascript, */*"; 
    string timestamp = DateTime.UtcNow.ToString("o");	// 2013-11-11T10:15:54.1731069Z
    string url = "http://localhost:1260/odata/v1/Orders?$top=10&$filter=CreatedOnUtc gt datetime'2013-02-20T00:00:00'";

First we create the message representation.

    string messageRepresentation = string.Join("\n",
    	get.ToLower(),
    	"",
    	accept.ToLower(),
    	url.ToLower(),
    	timestamp,
    	publicKey.ToLower()
    );

It looks like:

    get
    
    application/json, text/javascript, */*
    http://localhost:1260/odata/v1/orders?$top=10&$filter=createdonutc gt datetime'2013-02-20t00:00:00'
    2013-11-11T10:15:54.1731069Z
    0c6b33651708eb09c8a8d6036b79d739

Now we can calculate the HMAC signature by using our secret key.

    string signature = CreateSignature(secretKey, messageRepresentation);	// hWce6V2KA0kkB0GBbIK0GSw5QAcS3+vj+m+WN/8k9EE=

We have all informations to setup the request, so we create a web request object and pass the required headers.
    
    var request = (HttpWebRequest)WebRequest.Create(url);
    request.Method = method;
    request.UserAgent = "My shopping data consumer v.1.0";
    request.Accept = accept;
    request.Headers.Add("Accept-Charset", "UTF-8");
    request.Headers.Add("SmartStore-Net-Api-PublicKey", publicKey);
    request.Headers.Add("SmartStore-Net-Api-Date", timestamp);
    request.Headers.Add("Authorization", "SmNetHmac1 " + signature);
    
The complete header looks like:

    User-Agent: My shopping data consumer v.1.0
    Accept: application/json, text/javascript, */*
    Accept-Charset: UTF-8
    SmartStore-Net-Api-PublicKey: 0c6b33651708eb09c8a8d6036b79d739
    SmartStore-Net-Api-Date: 2013-11-11T10:15:54.1731069Z
    Authorization: SmNetHmac1 hWce6V2KA0kkB0GBbIK0GSw5QAcS3+vj+m+WN/8k9EE=

Making a POST request
------
Posting means inserting data via API. This example shows how to add a new order note "Hello world!" to the order with ID 152. Here is the function to create the MD5 hash of the request body:

    public string CreateContentMd5Hash(byte[] content)
    {
    	string result = "";
    	if (content != null && content.Length > 0)
    	{
    		using (var md5 = MD5.Create())
    		{
    			byte[] hash = md5.ComputeHash(content);
    			result = Convert.ToBase64String(hash);
    		}
    	}
    	return result;
    }

All other variables have not changed.

    string content = "{\"OrderId\":152,\"Note\":\"Hello world!\",\"DisplayToCustomer\":false,\"CreatedOnUtc\":\"2013-11-09T11:15:00\"}";

    byte[] data = Encoding.UTF8.GetBytes(content);
    string contentMd5Hash = CreateContentMd5Hash(data);

    string method = "post";
    string timestamp = DateTime.UtcNow.ToString("o");	// 2013-11-11T19:44:04.9378268Z
    string url = "http://localhost:1260/odata/v1/OrderNotes";

We add the same header fields as in the previous example and additionally:

    request.ContentLength = data.Length;
    request.ContentType = "application/json; charset=utf-8";
    request.Headers.Add("Content-MD5", contentMd5Hash);	// optional

And we write the content into the request stream.

    using (var stream = request.GetRequestStream())
    {
    	stream.Write(data, 0, data.Length);
    }

The message representation is:

    post
    lgifXydL3FhffpTIilkwOw==
    application/json, text/javascript, */*
    http://localhost:1260/odata/v1/ordernotes
    2013-11-11T19:44:04.9378268Z
    0c6b33651708eb09c8a8d6036b79d739

The header looks like:
    
    User-Agent: My shopping data consumer v.1.0
    Accept: application/json, text/javascript, */*
    Accept-Charset: UTF-8
    SmartStore-Net-Api-PublicKey: 0c6b33651708eb09c8a8d6036b79d739
    SmartStore-Net-Api-Date: 2013-11-11T19:44:04.9378268Z
    Content-Type: application/json; charset=utf-8
    Content-Length: 100
    Content-MD5: lgifXydL3FhffpTIilkwOw==
    Authorization: SmNetHmac1 ejKxxtHNJYHCtBglZPg+cbSs3YTrA50pkfTHtVb1PMo=

As a general rule POST, PUT and PATCH are returning the added or changed record. For example:

    {
      "odata.metadata":"http://localhost:1260/odata/v1/$metadata#OrderNotes/@Element","OrderId":152,"Note":"Hello world!","DisplayToCustomer":false,"CreatedOnUtc":"2013-11-09T11:15:00","Id":692
    }

Processing the response
------
Example of reading the response into a string:

    HttpWebResponse webResponse = null;
    string response;
    
    try
    {
    	webResponse = request.GetResponse() as HttpWebResponse;
	    using (var reader = new StreamReader(webResponse.GetResponseStream(), Encoding.UTF8))
	    {
	    	response = reader.ReadToEnd();
	    }
    }
    catch (WebException wexc) { /* ... */ }
    catch (Exception exc) { /* ... */ }
    finally
    {
    	if (webResponse != null)
    	{
    		webResponse.Close();
    		webResponse.Dispose();
    	}
    }

JSON data can be easily parsed into dynamic or strongly typed objects using [Json.NET][6]. This example deserialize a JSON string into a list of customers.

    public class Customer
    {
    	public string Id { get; set; }
    	public string CustomerGuid { get; set; }
    	public string Email { get; set; }
		// more properties...
    }
    
    JObject json = JObject.Parse(response);
    string metadata = (string)json["odata.metadata"];
    
    if (!string.IsNullOrWhiteSpace(metadata) && metadata.EndsWith("#Customers"))
    {
    	List<Customer> customers = json["value"].Select(x => x.ToObject<Customer>()).ToList();
    }

Dynamic JSON parsing might look like this:

    dynamic dynamicJson = JObject.Parse(response);
    
	foreach (dynamic customer in dynamicJson.value)
	{
		string str = string.Format("{0} {1} {2}", customer.Id, customer.CustomerGuid, customer.Email);
		Debug.WriteLine(str);
	}

    
# More examples

#### Get installed payment methods

    GET http://localhost:1260/api/v1/Payments/Methods?$filter=Installed eq true

Note that payment methods in SmartStore.Net are plugins and not entities, so we must use the `/api/v1/` service here.

#### Get return requests for customer with ID 1

     GET http://localhost:1260/odata/v1/Customers(1)/ReturnRequests?$top=25&$inlinecount=allpages

Note the inlinecount option which tells OData to return an odata.count value with total count of matching entities in the response.

#### Mark order with ID 145 as payed

    POST http://localhost:1260/odata/v1/Orders(145)/PaymentPaid
    {"PaymentMethodName":"Payments.Sofortueberweisung"}

This request is called OData action because it is initiating further data processing on the server.
Note the second line which are optional OData action parameters. They are send in the request body, not in the query string. This example additionally set the system name of the payment method to `Payments.Sofortueberweisung` with which order 145 was balanced.

Other actions that would change the payment status are `PaymentPending` and `PaymentRefund`. `PaymentRefund` supports the action parameter `Online`. *True* would call the related payment gateway to refund the payment. Use the action `Cancel` to cancel an order.

#### Get shipping address for order 145

    GET http://localhost:1260/odata/v1/Orders(145)/ShippingAddress

This request is called OData navigation. Use `BillingAddress` if you want the billing address of an order to be returned.

#### Partially update address with ID 1

    PATCH http://localhost:1260/odata/v1/Addresses(1)?SmNetFulfillCountry=US&SmNetFulfillStateProvince=NY
    {"City":"New York","Address1":"21 West 52nd Street","ZipPostalCode":"10021","FirstName":"John","LastName":"Doe"}

The example uses SmNetFulfillCountry and SmNetFulfillStateProvince options to update the country (USA) and province (New York). That avoids extra querying of country and province records and passing its IDs in the request body.
Note that you cannot use a path `/Orders(145)/ShippingAddress` to update an address because `ShippingAddress` is a navigation property and updates are not supported here.

#### Get product with name *SmartStore eBay SmartSeller*

    GET http://localhost:1260/odata/v1/Products?$top=1&$filter=Name eq 'SmartStore eBay SmartSeller'

#### Get child products of group product with ID 210

    GET http://localhost:1260/odata/v1/Products?$top=120&$filter=ParentGroupedProductId eq 210

#### Get final price of product with ID 211

    POST http://localhost:1260/odata/v1/Products(211)/FinalPrice

Note the post method. `FinalPrice` is an OData action because further data processing (price calculation) is required. There is a second action `LowestPrice` which serves the lowest possible price for a product.

#### Get email address of customer with ID 1

    GET http://localhost:1260/odata/v1/Customers(1)/Email

#### Get ID of store with name *my nice store*

    GET http://localhost:1260/odata/v1/Stores?$top=1&$filter=Name eq 'my nice store'&$select=Id

Note the select option which tells OData just to return the Id property.



<br />
[1]: http://www.asp.net/web-api/overview/web-api-clients/calling-a-web-api-from-a-net-client
[2]: http://www.asp.net/web-api/overview/odata-support-in-aspnet-web-api/supporting-odata-query-options
[3]: http://www.odata.org/documentation/odata-v2-documentation/uri-conventions/#4_Query_String_Options
[4]: http://msopentech.com/odataorg/libraries/
[5]: http://msopentech.com/odataorg/introduction/
[6]: https://json.codeplex.com/