using System;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Logging
{
    public class LogModel : EntityModelBase
    {
        public string LogLevelHint { get; set; }

        [SmartResourceDisplayName("Admin.System.Log.Fields.LogLevel")]
        public string LogLevel { get; set; }

        [SmartResourceDisplayName("Admin.System.Log.Fields.ShortMessage")]
        [AllowHtml]
        public string ShortMessage { get; set; }

        [SmartResourceDisplayName("Admin.System.Log.Fields.FullMessage")]
        [AllowHtml]
        public string FullMessage { get; set; }

        [SmartResourceDisplayName("Admin.System.Log.Fields.IPAddress")]
        [AllowHtml]
        public string IpAddress { get; set; }

        [SmartResourceDisplayName("Admin.System.Log.Fields.Customer")]
        public int? CustomerId { get; set; }
        [SmartResourceDisplayName("Admin.System.Log.Fields.Customer")]
        public string CustomerEmail { get; set; }

        [SmartResourceDisplayName("Admin.System.Log.Fields.PageURL")]
        [AllowHtml]
        public string PageUrl { get; set; }

        [SmartResourceDisplayName("Admin.System.Log.Fields.ReferrerURL")]
        [AllowHtml]
        public string ReferrerUrl { get; set; }

        [SmartResourceDisplayName("Common.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [SmartResourceDisplayName("Admin.System.Log.Fields.Logger")]
        public string Logger { get; set; }

        [SmartResourceDisplayName("Admin.System.Log.Fields.Logger")]
        public string LoggerShort { get; set; }

        [SmartResourceDisplayName("Admin.System.Log.Fields.HttpMethod")]
        public string HttpMethod { get; set; }

        [SmartResourceDisplayName("Admin.System.Log.Fields.UserName")]
        public string UserName { get; set; }
    }
}