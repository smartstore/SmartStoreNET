using System;
using System.Collections.Generic;
using SmartStore.Core.Domain.Media;

namespace SmartStore.Services.Media
{
    public interface IMediaTrackDetector
    {
        void ConfigureTracks(string albumName, TrackedMediaPropertyTable table);
        IEnumerable<MediaTrack> DetectAllTracks(string albumName);
    }
}