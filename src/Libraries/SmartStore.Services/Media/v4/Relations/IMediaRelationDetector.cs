using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core.Domain.Media;

namespace SmartStore.Services.Media
{
    public interface IMediaRelationDetector
    {
        IEnumerable<MediaRelation> DetectAllRelations(string albumName);
    }
}
