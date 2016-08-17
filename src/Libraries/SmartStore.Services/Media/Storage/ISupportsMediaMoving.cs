using System.Threading.Tasks;

namespace SmartStore.Services.Media.Storage
{
	public interface ISupportsMediaMoving
	{
		/// <summary>
		/// Moves a media item to the target provider
		/// </summary>
		/// <param name="target">Target provider</param>
		/// <param name="context">Media storage mover context</param>
		/// <param name="media">Media storage item</param>
		void MoveTo(ISupportsMediaMoving target, MediaMoverContext context, MediaItem media);

		/// <summary>
		/// Data received by the source provider to be stored by the target provider
		/// </summary>
		/// <param name="context">Media storage mover context</param>
		/// <param name="media">Media storage item</param>
		/// <param name="data">Binary data</param>
		void Receive(MediaMoverContext context, MediaItem media, byte[] data);

		/// <summary>
		/// Data received by the source provider to be stored by the target provider (async)
		/// </summary>
		/// <param name="context">Media storage mover context</param>
		/// <param name="media">Media storage item</param>
		/// <param name="data">Binary data</param>
		Task ReceiveAsync(MediaMoverContext context, MediaItem media, byte[] data);

		/// <summary>
		/// Called when batch media moving completes
		/// </summary>
		/// <param name="context">Media storage mover context</param>
		/// <param name="succeeded">Whether media moving succeeded</param>
		void OnCompleted(MediaMoverContext context, bool succeeded);
	}
}
