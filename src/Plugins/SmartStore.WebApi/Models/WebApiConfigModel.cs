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

        [SmartResourceDisplayName("Plugins.Api.WebApi.SwaggerUrl")]
        public string SwaggerUrl { get; set; }

        [SmartResourceDisplayName("Plugins.Api.WebApi.ValidMinutePeriod")]
        public int ValidMinutePeriod { get; set; }

        [SmartResourceDisplayName("Plugins.Api.WebApi.NoRequestTimestampValidation")]
        public bool NoRequestTimestampValidation { get; set; }

        [SmartResourceDisplayName("Plugins.Api.WebApi.AllowEmptyMd5Hash")]
        public bool AllowEmptyMd5Hash { get; set; }

        [SmartResourceDisplayName("Plugins.Api.WebApi.MaxTop")]
        public int MaxTop { get; set; }

        [SmartResourceDisplayName("Plugins.Api.WebApi.MaxExpansionDepth")]
        public int MaxExpansionDepth { get; set; }

        [SmartResourceDisplayName("Plugins.Api.WebApi.LogUnauthorized")]
        public bool LogUnauthorized { get; set; }

        public int GridPageSize { get; set; }

        public void Copy(WebApiSettings settings, bool fromSettings)
        {
            if (fromSettings)
            {
                ValidMinutePeriod = settings.ValidMinutePeriod;
                NoRequestTimestampValidation = settings.NoRequestTimestampValidation;
                AllowEmptyMd5Hash = settings.AllowEmptyMd5Hash;
                MaxTop = settings.MaxTop;
                MaxExpansionDepth = settings.MaxExpansionDepth;
                LogUnauthorized = settings.LogUnauthorized;
            }
            else
            {
                settings.ValidMinutePeriod = ValidMinutePeriod;
                settings.NoRequestTimestampValidation = NoRequestTimestampValidation;
                settings.AllowEmptyMd5Hash = AllowEmptyMd5Hash;
                settings.MaxTop = MaxTop;
                settings.MaxExpansionDepth = MaxExpansionDepth;
                settings.LogUnauthorized = LogUnauthorized;
            }
        }
    }
}
