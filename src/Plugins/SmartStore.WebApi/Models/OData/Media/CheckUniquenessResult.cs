using System.Runtime.Serialization;

namespace SmartStore.WebApi.Models.OData.Media
{
    [DataContract]
    public partial class CheckUniquenessResult
    {
        [DataMember]
        public bool Result { get; set; }

        [DataMember]
        public string NewPath { get; set; }
    }
}