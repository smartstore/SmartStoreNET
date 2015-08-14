using System.Web.Mvc;
using FluentValidation.Attributes;
using SmartStore.Admin.Validators.Tasks;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Admin.Models.Tasks
{
    [Validator(typeof(ScheduleTaskValidator))]
    public partial class ScheduleTaskModel : EntityModelBase
    {
        [SmartResourceDisplayName("Admin.System.ScheduleTasks.Name")]
        [AllowHtml]
        public string Name { get; set; }

		//[SmartResourceDisplayName("Admin.System.ScheduleTasks.Seconds")]
		//public int Seconds { get; set; }
		//public string PrettySeconds { get; set; }

		public string CronExpression { get; set; }
		public string CronDescription { get; set; }
		
        [SmartResourceDisplayName("Admin.System.ScheduleTasks.Enabled")]
        public bool Enabled { get; set; }

        [SmartResourceDisplayName("Admin.System.ScheduleTasks.StopOnError")]
        public bool StopOnError { get; set; }

        [SmartResourceDisplayName("Admin.System.ScheduleTasks.LastStart")]
        public string LastStartUtc { get; set; }

        [SmartResourceDisplayName("Admin.System.ScheduleTasks.LastEnd")]
        public string LastEndUtc { get; set; }

        [SmartResourceDisplayName("Admin.System.ScheduleTasks.LastSuccess")]
        public string LastSuccessUtc { get; set; }

        [SmartResourceDisplayName("Admin.System.ScheduleTasks.NextRun")]
        public string NextRunUtc { get; set; }

		public bool IsOverdue { get; set; }

		[SmartResourceDisplayName("Common.Error")]
		public string LastError { get; set; }

		public string Duration { get; set; }

		public bool IsRunning { get; set; }
		public int? ProgressPercent { get; set; }
		public string ProgressMessage { get; set; }
		public string CancelUrl { get; set; }
		public string EditUrl { get; set; }

    }
}