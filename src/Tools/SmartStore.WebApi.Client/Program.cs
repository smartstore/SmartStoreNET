using System;
using System.Windows.Forms;

namespace SmartStoreNetWebApiClient
{
	static class Program
	{
		public static string AppName { get { return "SmartStore.Net Web API Client v.1.1"; } }
		public static string ConsumerName { get { return "My shopping data consumer v.1.1"; } }

		/// <summary>
		/// Der Haupteinstiegspunkt für die Anwendung.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		}
	}
}
