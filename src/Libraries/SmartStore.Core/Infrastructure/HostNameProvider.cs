using System;

namespace SmartStore.Core
{
	public class HostNameProvider : IHostNameProvider
	{
		public string GetHostName()
		{
			// use the current host and the process id as two servers could run on the same machine
			return System.Net.Dns.GetHostName() + ":" + System.Diagnostics.Process.GetCurrentProcess().Id;
		}
	}
}
