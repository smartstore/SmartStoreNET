using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;
using SmartStore.Web.Framework.WebApi;

namespace SmartStore.Plugin.Api.WebApi.Models
{
	public class WebApiConfigModel : ModelBase
	{
        [SmartResourceDisplayName("Plugins.Api.WebApi.ApiOdataUrl")]
		public string ApiOdataUrl { get; set; }

        [SmartResourceDisplayName("Plugins.Api.WebApi.ApiOdataMetadataUrl")]
		public string ApiOdataMetadataUrl { get; set; }

        [SmartResourceDisplayName("Plugins.Api.WebApi.ValidMinutePeriod")]
		public int ValidMinutePeriod { get; set; }

        [SmartResourceDisplayName("Plugins.Api.WebApi.LogUnauthorized")]
		public bool LogUnauthorized { get; set; }

		public int GridPageSize { get; set; }

		public void Copy(WebApiSettings settings, bool fromSettings)
		{
			if (fromSettings)
			{
				ValidMinutePeriod = settings.ValidMinutePeriod;
				LogUnauthorized = settings.LogUnauthorized;
			}
			else
			{
				settings.ValidMinutePeriod = ValidMinutePeriod;
				settings.LogUnauthorized = LogUnauthorized;
			}
		}
	}
}
