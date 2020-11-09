using System.Runtime.Serialization;

namespace SmartStore.WebApi.Models.OData
{
    /// <summary>
    /// Represents a simple value range.
    /// </summary>
    [DataContract]
    public partial class SimpleRange<T>
    {
        /// <summary>
        /// Gets or sets the minimum value.
        /// </summary>
        [DataMember]
        public T Minimum { get; set; }

        /// <summary>
        /// Gets or sets the maximum value.
        /// </summary>
        [DataMember]
        public T Maximum { get; set; }
    }
}