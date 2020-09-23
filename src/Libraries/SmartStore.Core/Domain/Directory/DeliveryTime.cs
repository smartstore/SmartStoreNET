using System.Runtime.Serialization;
using SmartStore.Core.Domain.Localization;

namespace SmartStore.Core.Domain.Directory
{
    /// <summary>
    /// Represents a delivery time
    /// </summary>
	[DataContract]
    public partial class DeliveryTime : BaseEntity, ILocalizedEntity
    {
        /// <summary>
        /// Gets or sets the name
        /// </summary>
		[DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the hex value
        /// </summary>
		[DataMember]
        public string ColorHexValue { get; set; }

        /// <summary>
        /// Gets or sets the display locale
        /// </summary>
		[DataMember]
        public string DisplayLocale { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
		[DataMember]
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
		[DataMember]
        public bool? IsDefault { get; set; }

        /// <summary>
        /// Specifies the earliest time of delivery in days.
        /// </summary>
        [DataMember]
        public int? MinDays { get; set; }

        /// <summary>
        /// Specifies the latest time of delivery in days.
        /// </summary>
        [DataMember]
        public int? MaxDays { get; set; }

        #region Utils

        /// <summary>
        /// Intended to be used for submission of products to third parties which require the delivery days as integer e.g. price comparison portals or after sales services.
        /// </summary>
        /// <returns>Days as integer or null if no days are set for the current delivery time.</returns>
        public int? GetMinOrMaxOrAverageDays()
        {   
            if (MinDays > 0 && MaxDays > 0)
            {
                return ((int)MinDays + (int)MaxDays) / 2;
            }
            else if (MinDays > 0)
            {
                return (int)MinDays;
            }
            else if (MaxDays > 0)
            {
                return (int)MaxDays;
            }
                
            return null;
        }

        #endregion
    }


    /// <summary>
    /// Represents how to present delivery times.
    /// </summary>
    public enum DeliveryTimesPresentation
    {
        /// <summary>
        /// Do not display.
        /// </summary>
        None = 0,

        /// <summary>
        /// Display label only.
        /// </summary>
        LabelOnly = 5,

        /// <summary>
        /// Display date only (if possible).
        /// </summary>
        DateOnly = 10,

        /// <summary>
        /// Display label and date.
        /// </summary>
        LabelAndDate = 15
    }
}
