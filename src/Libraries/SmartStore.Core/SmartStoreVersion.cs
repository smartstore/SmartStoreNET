using System;
using System.Reflection;
using SmartStore;

namespace SmartStore.Core
{
    public static class SmartStoreVersion
    {
        private static readonly Version s_version = Assembly.GetExecutingAssembly().GetName().Version;
        
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
                return "1.2.0.0"; // TODO: (MC) read it from attribute
            }
        }

        internal static Version FullVersion
        {
            get
            {
                return s_version;
            }
        }
    }
}
