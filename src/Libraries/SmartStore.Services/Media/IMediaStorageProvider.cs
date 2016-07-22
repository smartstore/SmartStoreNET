using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.Media
{
	public interface IMediaStorageProvider : IProvider
	{
		byte[] Load(Picture picture);

		void Save(Picture picture, byte[] data);

		void Remove(params Picture[] pictures);

		string GetPublicUrl(Picture picture);
	}
}
