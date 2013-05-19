using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using SmartStore.Core.Domain.Tasks;

namespace SmartStore.Core.Events
{
	/// <summary>
	/// to initialize scheduled tasks in Application_Start
	/// </summary>
	/// <remarks>codehint: sm-add</remarks>
	public class AppInitScheduledTasksEvent
	{
		public List<ScheduleTask> ScheduledTasks { get; set; }
	}
}
