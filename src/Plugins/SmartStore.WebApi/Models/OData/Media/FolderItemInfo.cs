using System.Runtime.Serialization;

namespace SmartStore.WebApi.Models.OData.Media
{
    /// <summary>
    /// Folder information returned by the API.
    /// </summary>
    [DataContract]
    public partial class FolderItemInfo
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public int FilesCount { get; set; }

        [DataMember]
        public string Path { get; set; }

        [DataMember]
        public string Name { get; set; }
    }
}