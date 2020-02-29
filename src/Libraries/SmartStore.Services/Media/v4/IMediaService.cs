using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core.Domain.Media;

namespace SmartStore.Services.Media
{
    public class InsertFileCommand
    {
        public byte[] Data { get; set; }
        public int FolderId { get; set; }
        public string Name { get; set; }
        public string MimeType { get; set; }
        public bool IsNew { get; set; }
        public bool IsTransient { get; set; } = true;
        public bool ValidateBinary { get; set; } = true;
        public Size Dimensions { get; set; }
    }
    
    public interface IMediaService
    {
        MediaFileInfo GetFileByPath(string path);
        void CopyFile(MediaFile file, string newPath, bool overwrite = false);
        int CountFiles(MediaQuery query);
        IEnumerable<MediaFileInfo> SearchFiles(MediaQuery query);
        bool FileExists(string path);

        MediaFileInfo CreateFile(string path);
        MediaFileInfo CreateFile(int folderId, string fileName);
        MediaFileInfo InsertFile(InsertFileCommand command);
        void DeleteFile(MediaFile file);
    }
}
