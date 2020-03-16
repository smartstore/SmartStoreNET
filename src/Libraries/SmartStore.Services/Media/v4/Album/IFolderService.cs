using System;
using System.Collections.Generic;
using SmartStore.Collections;
using SmartStore.Core.Domain.Media;

namespace SmartStore.Services.Media
{
    public interface IFolderService
    {
        TreeNode<MediaFolderNode> GetRootNode();
        TreeNode<MediaFolderNode> GetNodeById(int id);
        TreeNode<MediaFolderNode> GetNodeByPath(string path);
        void ClearCache();

        MediaFolder GetFolderById(int id);
        bool FolderExists(string path);

        void InsertFolder(MediaFolder folder);
        void UpdateFolder(MediaFolder folder);
        void DeleteFolder(MediaFolder folder);
    }
}
