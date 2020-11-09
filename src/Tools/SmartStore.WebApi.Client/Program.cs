using System;
using System.Windows.Forms;

namespace SmartStore.WebApi.Client
{
    static class Program
    {
        public static string AppName => "SmartStore Web API Client v.1.7";
        public static string ConsumerName => "My shopping data consumer v.1.7";

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
