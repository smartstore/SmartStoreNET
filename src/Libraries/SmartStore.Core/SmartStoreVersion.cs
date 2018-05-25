using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using SmartStore;

namespace SmartStore.Core
{
	public class HelpTopic
	{
		public static HelpTopic CronExpressions = new HelpTopic("cron", "Managing+Scheduled+Tasks#ManagingScheduledTasks-Cron", "Geplante+Aufgaben+verwalten#GeplanteAufgabenverwalten-CronAusdruck");

		public HelpTopic(string name, string enPath, string dePath)
		{
			Guard.NotEmpty(name, nameof(name));
			Guard.NotEmpty(enPath, nameof(enPath));
			Guard.NotEmpty(dePath, nameof(dePath));

			Name = name;
			EnPath = enPath;
			DePath = dePath;
		}

		public string Name { get; private set; }
		public string EnPath { get; private set; }
		public string DePath { get; private set; }
	}

	public static class SmartStoreVersion
    {
        private static readonly Version s_infoVersion = new Version("1.0.0.0");
        private static readonly List<Version> s_breakingChangesHistory = new List<Version> 
        { 
            // IMPORTANT: Add app versions from low to high
            // NOTE: do not specify build & revision unless you have good reasons for it.
            //       A release with breaking changes should definitely have at least
            //       a greater minor version.
            new Version("1.2"),
            new Version("1.2.1"),
            new Version("2.0"),
			new Version("2.1"),
			new Version("2.2"),
            new Version("2.5"),
			new Version("3.0"),
			new Version("3.1"),
			new Version("3.1.5")
		};

		private const string HELP_BASEURL = "https://docs.smartstore.com/display/";

		static SmartStoreVersion()
        {
            s_breakingChangesHistory.Reverse();

            // get informational version
            var infoVersionAttr = Assembly.GetExecutingAssembly().GetAttribute<AssemblyInformationalVersionAttribute>(false);
            if (infoVersionAttr != null)
            {
                s_infoVersion = new Version(infoVersionAttr.InformationalVersion);
            }   
        }

        /// <summary>
        /// Gets the app version
        /// </summary>
        public static string CurrentVersion 
        {
            get
            {
                return "{0}.{1}".FormatInvariant(s_infoVersion.Major, s_infoVersion.Minor);
            }
        }

        /// <summary>
        /// Gets the app full version
        /// </summary>
        public static string CurrentFullVersion
        {
            get
            {
                return s_infoVersion.ToString();
            }
        }

        public static Version Version
        {
            get
            {
                return s_infoVersion;
            }
        }

		public static string GenerateHelpUrl(string languageCode, HelpTopic topic)
		{
			Guard.NotEmpty(languageCode, nameof(languageCode));
			Guard.NotNull(topic, nameof(topic));

			var path = languageCode.IsCaseInsensitiveEqual("de") ? topic.DePath : topic.EnPath;
			return GenerateHelpUrl(languageCode, path);
		}

		public static string GenerateHelpUrl(string languageCode, string path)
		{
			Guard.NotEmpty(languageCode, nameof(languageCode));

			return String.Concat(
				HELP_BASEURL,
				GetUserGuideSpaceKey(languageCode),
				"/",
				path.EmptyNull().Trim().TrimStart('/', '\\'));
		}

		public static string GetUserGuideSpaceKey(string languageCode)
		{
			return languageCode.IsCaseInsensitiveEqual("de") 
				? "SDDE31" 
				: "SMNET31";
		}

        /// <summary>
        /// Gets a list of SmartStore.NET versions in which breaking changes occured,
        /// which could lead to plugins malfunctioning.
        /// </summary>
        /// <remarks>
        /// A plugin's <c>MinAppVersion</c> is checked against this list to assume
        /// it's compatibility with the current app version.
        /// </remarks>
        internal static IEnumerable<Version> BreakingChangesHistory
        {
            get
            {
                return s_breakingChangesHistory.AsEnumerable();
            }
        }
    }
}
