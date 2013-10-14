using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using SmartStore;

namespace SmartStore.Core
{
    public static class SmartStoreVersion
    {
        private static readonly Version s_version = Assembly.GetExecutingAssembly().GetName().Version;
        private static readonly List<Version> s_breakingChangesHistory = new List<Version> 
        { 
            // IMPORTANT: Add app versions from low to high
            // NOTE: do not specify build & revision unless you have good reasons for it.
            //       A release with breaking changes should definitely have at least
            //       a greater minor version.
            new Version("1.2"),
            //new Version("1.6")
        };

        static SmartStoreVersion()
        {
            s_breakingChangesHistory.Reverse();
        }

        /// <summary>
        /// Gets the app version
        /// </summary>
        public static string CurrentVersion 
        {
            get
            {
                return "{0}.{1}".FormatInvariant(s_version.Major, s_version.Minor);
            }
        }

        /// <summary>
        /// Gets the app full version
        /// </summary>
        public static string CurrentFullVersion
        {
            get
            {
                //return s_version.ToString();
                return "1.2.1.0"; // TODO: (MC) read it from attribute
            }
        }

        internal static Version FullVersion
        {
            get
            {
                return s_version;
            }
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
