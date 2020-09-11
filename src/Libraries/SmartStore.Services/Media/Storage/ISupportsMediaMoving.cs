using System.IO;
using System.Threading.Tasks;
using SmartStore.Core.Domain.Media;

namespace SmartStore.Services.Media.Storage
{
    public interface ISupportsMediaMoving
    {
        /// <summary>
        /// Moves a media item to the target provider
        /// </summary>
        /// <param name="target">Target provider</param>
        /// <param name="context">Media storage mover context</param>
        /// <param name="mediaFile">Media file item</param>
        void MoveTo(ISupportsMediaMoving target, MediaMoverContext context, MediaFile mediaFile);

        /// <summary>
        /// Data received by the source provider to be stored by the target provider
        /// </summary>
        /// <param name="context">Media storage mover context</param>
        /// <param name="mediaFile">Media file item</param>
        /// <param name="stream">Source stream</param>
        void Receive(MediaMoverContext context, MediaFile mediaFile, Stream stream);

        /// <summary>
        /// Data received by the source provider to be stored by the target provider (async)
        /// </summary>
        /// <param name="context">Media storage mover context</param>
        /// <param name="mediaFile">Media file item</param>
        /// <param name="stream">Source stream</param>
        Task ReceiveAsync(MediaMoverContext context, MediaFile mediaFile, Stream stream);

        /// <summary>
        /// Called when batch media moving completes
        /// </summary>
        /// <param name="context">Media storage mover context</param>
        /// <param name="succeeded">Whether media moving succeeded</param>
        void OnCompleted(MediaMoverContext context, bool succeeded);
    }
}
