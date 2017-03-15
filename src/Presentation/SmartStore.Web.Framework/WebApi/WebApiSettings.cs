using SmartStore.Core.Configuration;

namespace SmartStore.Web.Framework.WebApi
{
	public class WebApiSettings : ISettings
	{
		public WebApiSettings()
		{
			LogUnauthorized = true;
			ValidMinutePeriod = WebApiGlobal.DefaultTimePeriodMinutes;
			MaxTop = WebApiGlobal.DefaultMaxTop;
			MaxExpansionDepth = WebApiGlobal.DefaultMaxExpansionDepth;
		}

		public int ValidMinutePeriod { get; set; }
		public bool LogUnauthorized { get; set; }
		public bool NoRequestTimestampValidation { get; set; }
		public bool AllowEmptyMd5Hash { get; set; }
		public int MaxTop { get; set; }
		public int MaxExpansionDepth { get; set; }
	}
}
