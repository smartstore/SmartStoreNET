using System.Collections.Generic;
using SmartStore.Core.Configuration;
using SmartStore.Utilities;

namespace SmartStore.Core.Domain.Security
{
    public class SecuritySettings : ISettings
    {
		public SecuritySettings()
		{
			EncryptionKey = CommonHelper.GenerateRandomDigitCode(16);
			AdminAreaAllowedIpAddresses = new List<string>();
		}

		/// <summary>
		/// When <c>true</c>, bypasses any SSL redirection on localhost
		/// </summary>
		public bool UseSslOnLocalhost { get; set; }

		/// <summary>
		/// Gets or sets an encryption key
		/// </summary>
		public string EncryptionKey { get; set; }

        /// <summary>
        /// Gets or sets a list of adminn area allowed IP addresses
        /// </summary>
        public List<string> AdminAreaAllowedIpAddresses { get; set; }

        /// <summary>
        /// Gets or sets a vaule indicating whether to hide admin menu items based on ACL
        /// </summary>
        public bool HideAdminMenuItemsBasedOnPermissions { get; set; }

		/// <summary>
		/// Gets or sets a vaule indicating whether "Honeypot" is enabled to prevent bots from posting forms.
		/// </summary>
		public bool EnableHoneypotProtection { get; set; }
    }
}