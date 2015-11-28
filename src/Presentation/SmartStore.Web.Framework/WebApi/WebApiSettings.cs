using SmartStore.Core.Configuration;

namespace SmartStore.Web.Framework.WebApi
{
	public class WebApiSettings : ISettings
	{
		public int ValidMinutePeriod { get; set; }
		public bool LogUnauthorized { get; set; }
		public bool NoRequestTimestampValidation { get; set; }
	}
}
