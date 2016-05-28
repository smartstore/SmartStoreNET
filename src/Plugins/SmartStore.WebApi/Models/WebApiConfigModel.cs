using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.WebApi;

namespace SmartStore.WebApi.Models
{
	public class WebApiConfigModel : ModelBase
	{
        [SmartResourceDisplayName("Plugins.Api.WebApi.ApiOdataUrl")]
		public string ApiOdataUrl { get; set; }

        [SmartResourceDisplayName("Plugins.Api.WebApi.ApiOdataMetadataUrl")]
		public string ApiOdataMetadataUrl { get; set; }

        [SmartResourceDisplayName("Plugins.Api.WebApi.ValidMinutePeriod")]
		public int ValidMinutePeriod { get; set; }

		[SmartResourceDisplayName("Plugins.Api.WebApi.NoRequestTimestampValidation")]
		public bool NoRequestTimestampValidation { get; set; }

        [SmartResourceDisplayName("Plugins.Api.WebApi.LogUnauthorized")]
		public bool LogUnauthorized { get; set; }

		public int GridPageSize { get; set; }

		public void Copy(WebApiSettings settings, bool fromSettings)
		{
			if (fromSettings)
			{
				ValidMinutePeriod = settings.ValidMinutePeriod;
				NoRequestTimestampValidation = settings.NoRequestTimestampValidation;
				LogUnauthorized = settings.LogUnauthorized;
			}
			else
			{
				settings.ValidMinutePeriod = ValidMinutePeriod;
				settings.NoRequestTimestampValidation = NoRequestTimestampValidation;
				settings.LogUnauthorized = LogUnauthorized;
			}
		}
	}
}
