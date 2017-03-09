using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Clickatell.Models
{
	public class SmsClickatellModel : ModelBase
    {
        [SmartResourceDisplayName("Plugins.Sms.Clickatell.Fields.Enabled")]
        public bool Enabled { get; set; } 

        [SmartResourceDisplayName("Plugins.Sms.Clickatell.Fields.PhoneNumber")]
        public string PhoneNumber { get; set; }

        [SmartResourceDisplayName("Plugins.Sms.Clickatell.Fields.ApiId")]
        public string ApiId { get; set; }

        [SmartResourceDisplayName("Plugins.Sms.Clickatell.Fields.TestMessage")]
        public string TestMessage { get; set; }

		public bool TestSucceeded { get; set; }
		public string TestSmsResult { get; set; }
		public string TestSmsDetailResult { get; set; }
	}
}