using SmartStore.Web.Framework;

namespace SmartStore.Plugin.Sms.Clickatell.Models
{
    public class SmsClickatellModel
    {
        [SmartResourceDisplayName("Plugins.Sms.Clickatell.Fields.Enabled")]
        public bool Enabled { get; set; } 

        [SmartResourceDisplayName("Plugins.Sms.Clickatell.Fields.PhoneNumber")]
        public string PhoneNumber { get; set; }

        [SmartResourceDisplayName("Plugins.Sms.Clickatell.Fields.ApiId")]
        public string ApiId { get; set; }

        [SmartResourceDisplayName("Plugins.Sms.Clickatell.Fields.Username")]
        public string Username { get; set; }

        [SmartResourceDisplayName("Plugins.Sms.Clickatell.Fields.Password")]
        public string Password { get; set; }


        [SmartResourceDisplayName("Plugins.Sms.Clickatell.Fields.TestMessage")]
        public string TestMessage { get; set; }
        public string TestSmsResult { get; set; }
    }
}