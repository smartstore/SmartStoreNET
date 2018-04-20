using System;
using System.Windows.Forms;

namespace SmartStoreNetWebApiClient
{
    static class Program
	{
		public static string AppName { get { return "SmartStore.Net Web API Client v.1.5"; } }
		public static string ConsumerName { get { return "My shopping data consumer v.1.5"; } }

		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		}
	}
}
