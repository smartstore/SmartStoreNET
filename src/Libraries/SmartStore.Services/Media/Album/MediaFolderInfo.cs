using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SmartStore.Collections;
using SmartStore.Core.IO;

namespace SmartStore.Services.Media
{
    public partial class MediaFolderInfo : IFolder
    {
        public MediaFolderInfo(TreeNode<MediaFolderNode> node)
        {
            Node = node;
        }

        [JsonIgnore]
        public TreeNode<MediaFolderNode> Node { get; }

        [JsonProperty("filesCount")]
        public int FilesCount => Node.Value.FilesCount;

        public static implicit operator TreeNode<MediaFolderNode>(MediaFolderInfo folderInfo)
        {
            return folderInfo.Node;
        }

        [JsonProperty("id")]
        public int Id => Node.Value.Id;

        #region IFolder

        [JsonProperty("path")]
        public string Path => Node.Value.Path;

        [JsonProperty("name")]
        public string Name => Node.Value.Name;

        [JsonIgnore]
        public long Size => 0;

        [JsonIgnore]
        public bool Exists => Node.Value.Id > 0;

        [JsonIgnore]
        public DateTime LastUpdated => DateTime.UtcNow;

        [JsonIgnore]
        public IFolder Parent => Node.Parent == null ? null : new MediaFolderInfo(Node.Parent);

        #endregion
    }
}
