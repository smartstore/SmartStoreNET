using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core.IO;
using SmartStore.Utilities;

namespace SmartStore.Services.Media
{
	public interface IMediaFileSystem : IFileSystem
	{
	}

	public class MediaFileSystem : LocalFileSystem, IMediaFileSystem
	{
		public MediaFileSystem()
			: base(CommonHelper.GetAppSetting<string>("sm:MediaBasePath", "Media"))
		{
			this.TryCreateFolder("Thumbs");
			this.TryCreateFolder("Uploaded");
			this.TryCreateFolder("QueuedEmailAttachment");
			this.TryCreateFolder("Downloads");
		}
	}
}
