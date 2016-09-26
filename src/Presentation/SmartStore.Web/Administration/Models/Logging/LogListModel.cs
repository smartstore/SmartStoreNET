using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Logging
{
    public class LogListModel : ModelBase
    {
        public LogListModel()
        {
            AvailableLogLevels = new List<SelectListItem>();
        }

        [SmartResourceDisplayName("Admin.System.Log.List.CreatedOnFrom")]
        public DateTime? CreatedOnFrom { get; set; }

        [SmartResourceDisplayName("Admin.System.Log.List.CreatedOnTo")]
        public DateTime? CreatedOnTo { get; set; }

        [SmartResourceDisplayName("Admin.System.Log.List.Message")]
        [AllowHtml]
        public string Message { get; set; }

        [SmartResourceDisplayName("Admin.System.Log.List.LogLevel")]
        public int LogLevelId { get; set; }

		[SmartResourceDisplayName("Admin.System.Log.Fields.Logger")]
		public string Logger { get; set; }

		public IList<SelectListItem> AvailableLogLevels { get; set; }
    }
}