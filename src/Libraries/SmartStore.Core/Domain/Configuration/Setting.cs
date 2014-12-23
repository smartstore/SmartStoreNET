using System.Diagnostics;
using System.Runtime.Serialization;
namespace SmartStore.Core.Domain.Configuration
{
    /// <summary>
    /// Represents a setting
    /// </summary>
	[DataContract]
	[DebuggerDisplay("{Name}: {Value}")]
	public partial class Setting : BaseEntity
    {
        public Setting() { }

		public Setting(string name, string value, int storeId = 0)
		{
            this.Name = name;
            this.Value = value;
			this.StoreId = storeId;
        }
        
        /// <summary>
        /// Gets or sets the name
        /// </summary>
		[DataMember]
		public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value
        /// </summary>
		[DataMember]
		public string Value { get; set; }

		/// <summary>
		/// Gets or sets the store for which this setting is valid. 0 is set when the setting is for all stores
		/// </summary>
		[DataMember]
		public int StoreId { get; set; }
    }
}
