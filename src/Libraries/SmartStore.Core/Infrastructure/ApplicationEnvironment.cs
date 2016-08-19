using System;

namespace SmartStore.Core
{
	public class ApplicationEnvironment : IApplicationEnvironment
	{
		public string EnvironmentIdentifier
		{
			get
			{
				// use the current host and the process id as two servers could run on the same machine
				return Environment.MachineName + "-" + System.Diagnostics.Process.GetCurrentProcess().Id;
			}
		}
	}
}
