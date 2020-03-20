using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Services.Media
{
    public interface IMediaUrlGenerator
    {
        string GenerateUrl(MediaFileInfo file, ProcessImageQuery imageQuery, string host = null);
    }
}
