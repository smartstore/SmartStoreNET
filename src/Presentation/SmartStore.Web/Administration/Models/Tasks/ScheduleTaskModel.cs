using System;
using System.Web.Mvc;
using FluentValidation.Attributes;
using SmartStore.Admin.Validators.Tasks;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Tasks
{
    [Validator(typeof(ScheduleTaskValidator))]
    public partial class ScheduleTaskModel : EntityModelBase
    {
        [SmartResourceDisplayName("Admin.System.ScheduleTasks.Name")]
        [AllowHtml]
        public string Name { get; set; }

		[SmartResourceDisplayName("Admin.System.ScheduleTasks.CronExpression")]
		public string CronExpression { get; set; }

		public string CronDescription { get; set; }
		
        [SmartResourceDisplayName("Admin.System.ScheduleTasks.Enabled")]
        public bool Enabled { get; set; }

        [SmartResourceDisplayName("Admin.System.ScheduleTasks.StopOnError")]
        public bool StopOnError { get; set; }

		[SmartResourceDisplayName("Admin.System.ScheduleTasks.LastStart")]
		public DateTime? LastStart { get; set; }
		public string LastStartPretty { get; set; }

        [SmartResourceDisplayName("Admin.System.ScheduleTasks.LastEnd")]
		public DateTime? LastEnd { get; set; }
        public string LastEndPretty { get; set; }

        [SmartResourceDisplayName("Admin.System.ScheduleTasks.LastSuccess")]
		public DateTime? LastSuccess { get; set; }
        public string LastSuccessPretty { get; set; }

        [SmartResourceDisplayName("Admin.System.ScheduleTasks.NextRun")]
		public DateTime? NextRun { get; set; }
        public string NextRunPretty { get; set; }

		public bool IsOverdue { get; set; }

		[SmartResourceDisplayName("Common.Error")]
		public string LastError { get; set; }

		[SmartResourceDisplayName("Common.Duration")]
		public string Duration { get; set; }

		public bool IsRunning { get; set; }
		public int? ProgressPercent { get; set; }
		public string ProgressMessage { get; set; }
		public string CancelUrl { get; set; }
		public string EditUrl { get; set; }
		public string ExecuteUrl { get; set; }

    }
}