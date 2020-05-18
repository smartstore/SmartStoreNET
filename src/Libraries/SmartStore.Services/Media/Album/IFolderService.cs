using System;
using System.Collections.Generic;
using SmartStore.Collections;
using SmartStore.Core.Domain.Media;

namespace SmartStore.Services.Media
{
    public interface IFolderService : IScopedService
    {
        TreeNode<MediaFolderNode> GetRootNode();
        TreeNode<MediaFolderNode> GetNodeById(int id);
        TreeNode<MediaFolderNode> GetNodeByPath(string path);
        bool CheckUniqueFolderName(string path, out string newName);

        MediaFolder GetFolderById(int id, bool withFiles = false);
        void InsertFolder(MediaFolder folder);
        void DeleteFolder(MediaFolder folder);
        void UpdateFolder(MediaFolder folder);
    }
}
