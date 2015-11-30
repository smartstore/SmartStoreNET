using SmartStore.Core.Configuration;

namespace SmartStore.Core.Domain.Messages
{
    public class MessageTemplatesSettings : ISettings
    {
		public MessageTemplatesSettings()
		{
			Color1 = "#3A87AD";
			Color2 = "#F7F7F7";
			Color3 = "#F5F5F5";
		}
		
		/// <summary>
        /// Gets or sets a value indicating whether to replace message tokens according to case invariant rules
        /// </summary>
        public bool CaseInvariantReplacement { get; set; }

        /// <summary>
        /// Gets or sets a color1 in  hex format ("#hhhhhh") to use in workflow message formatting
        /// </summary>
        public string Color1 { get; set; }

        /// <summary>
        /// Gets or sets a color2 in  hex format ("#hhhhhh") to use in workflow message formatting
        /// </summary>
        public string Color2 { get; set; }

        /// <summary>
        /// Gets or sets a color3 in  hex format ("#hhhhhh") to use in workflow message formatting
        /// </summary>
        public string Color3 { get; set; }

    }

}
