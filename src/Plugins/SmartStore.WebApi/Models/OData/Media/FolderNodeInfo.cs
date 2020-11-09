using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SmartStore.WebApi.Models.OData.Media
{
    /// <summary>
    /// Information about a folder node returned by the API.
    /// </summary>
    [DataContract]
    public partial class FolderNodeInfo
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
        [DataMember]
        public bool IsAlbum { get; set; }

        [DataMember]
        public string Path { get; set; }

        [DataMember]
        public string Slug { get; set; }

        [DataMember]
        public bool HasChildren { get; set; }

        // Not supported yet: "The complex type 'FolderNodeInfo' has a reference to itself through the property 'Children'. A recursive loop of complex types is not allowed."
        // See https://github.com/OData/WebApi/issues/1248
        //[DataMember]
        //[AutoExpand]
        //public ICollection<FolderNodeInfo> Children { get; set; }

        [DataMember]
        public ICollection<FolderChildNodeInfo> Children { get; set; }

        [DataContract]
        public partial class FolderChildNodeInfo
        {
            [DataMember]
            public int Id { get; set; }

            [DataMember]
            public string Name { get; set; }

            [DataMember]
            public string Path { get; set; }
        }
    }
}