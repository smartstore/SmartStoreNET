using System;
using System.ComponentModel.DataAnnotations;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Tasks
{
    public partial class ScheduleTaskHistoryModel : EntityModelBase
    {
        public int ScheduleTaskId { get; set; }
        public bool IsRunning { get; set; }

        [SmartResourceDisplayName("Admin.System.ScheduleTasks.LastStart")]
        [DisplayFormat(DataFormatString = "g")]
        public DateTime StartedOn { get; set; }
        public string StartedOnString { get; set; }
        public string StartedOnPretty { get; set; }

        [SmartResourceDisplayName("Admin.System.ScheduleTasks.LastEnd")]
        [DisplayFormat(DataFormatString = "g")]
        public DateTime? FinishedOn { get; set; }
        public string FinishedOnString { get; set; }
        public string FinishedOnPretty { get; set; }

        [SmartResourceDisplayName("Admin.System.ScheduleTasks.LastSuccess")]
        [DisplayFormat(DataFormatString = "g")]
        public DateTime? SucceededOn { get; set; }
        public string SucceededOnPretty { get; set; }

        [SmartResourceDisplayName("Common.Status")]
        public bool Succeeded => SucceededOn.HasValue && Error.IsEmpty();

        [SmartResourceDisplayName("Common.Error")]
        public string Error { get; set; }

        public int? ProgressPercent { get; set; }
        public string ProgressMessage { get; set; }

        [SmartResourceDisplayName("Common.Duration")]
        public string Duration { get; set; }

        [SmartResourceDisplayName("Common.MachineName")]
        public string MachineName { get; set; }
    }
}