using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SmartStore.WebApi.Models.OData.Media
{
    /// <summary>
    /// Information about a folder node returned by the API.
    /// </summary>
    [DataContract]
    public class FolderNodeInfo
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public int? ParentId { get; set; }

        /// <summary>
        /// The root album name.
        /// </summary>
        [DataMember]
        public string AlbumName { get; set; }

        /// <summary>
        /// Folder name.
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Whether the folder is a root album node.
        /// </summary>
        public bool IsAlbum { get; set; }

        [DataMember]
        public string Path { get; set; }

        [DataMember]
        public string Slug { get; set; }

        [DataMember]
        public ICollection<FolderNodeInfo> Children { get; set; }
    }
}