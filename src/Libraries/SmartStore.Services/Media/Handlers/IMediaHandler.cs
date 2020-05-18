using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Services.Media
{
    public interface IMediaHandler
    {
        int Order { get; }
        Task ExecuteAsync(MediaHandlerContext context);
    }
}
