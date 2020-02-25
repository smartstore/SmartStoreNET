using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core.Domain.Media;

namespace SmartStore.Services.Media
{
    public interface IMediaTrackDetector
    {
        void ConfigureTracks(string albumName, TrackedMediaPropertyTable table);
        IEnumerable<MediaTrackAction> DetectAllTracks(string albumName);
    }
}