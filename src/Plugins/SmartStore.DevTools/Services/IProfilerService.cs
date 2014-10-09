using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartStore.DevTools.Services
{
	public interface IProfilerService
	{
		void StepStart(string key, string message, bool isVerbose = false);
		void StepStop(string key);
	}
}