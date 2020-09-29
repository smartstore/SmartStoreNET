namespace SmartStore.GoogleAnalytics.Services
{
	/// <summary>
	/// Provides ready to use scripts for plugin settings. 
	/// </summary>
	/// <remarks>
	/// Code formatting (everything is squeezed to the edge) was done intentionally like this. 
	/// Else whitespace would be copied into the setting properties and effect the configuration page in a negative way.
	/// </remarks>
	public partial class GoogleAnalyticsScriptHelper
	{
		internal static string GetTrackingScript()
		{
			return @"<!-- Google code for Analytics tracking -->
<script>
	{OPTOUTCOOKIE}

	(function(i,s,o,g,r,a,m){i['GoogleAnalyticsObject']=r;i[r]=i[r]||function(){
	(i[r].q=i[r].q||[]).push(arguments)},i[r].l=1*new Date();a=s.createElement(o),
	m=s.getElementsByTagName(o)[0];a.async=1;a.src=g;m.parentNode.insertBefore(a,m)
	})(window,document,'script','//www.google-analytics.com/analytics.js','ga');

	ga('create', '{GOOGLEID}', {STORAGETYPE});
	ga('set', 'anonymizeIp', true); 
	ga('send', 'pageview');

	{ECOMMERCE}
</script>";
		}

		internal static string GetEcommerceScript()
		{
			return @"ga('require', 'ecommerce');
ga('ecommerce:addTransaction', {
	'id': '{ORDERID}',
	'affiliation': '{SITE}',
	'revenue': '{TOTAL}',
	'shipping': '{SHIP}',
	'tax': '{TAX}',
	'currency': '{CURRENCY}'
});

{DETAILS}

ga('ecommerce:send');";
		}

		internal static string GetEcommerceDetailScript()
		{
			return @"ga('ecommerce:addItem', {
	'id': '{ORDERID}',
	'name': '{PRODUCTNAME}',
	'sku': '{PRODUCTSKU}',
	'category': '{CATEGORYNAME}',
	'price': '{UNITPRICE}',
	'quantity': '{QUANTITY}'
});";
		}
	}
}