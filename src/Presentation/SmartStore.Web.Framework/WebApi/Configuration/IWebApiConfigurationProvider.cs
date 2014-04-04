
namespace SmartStore.Web.Framework.WebApi.Configuration
{
	public interface IWebApiConfigurationProvider
	{
		void Configure(WebApiConfigurationBroadcaster configData);

		int Priority { get; }
	}
}
