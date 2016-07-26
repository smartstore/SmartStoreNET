using System.Collections.Generic;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.Media.Storage
{
	public interface IMediaStorageMover
	{
		/// <summary>
		/// Moves media items from one storage provider to another
		/// </summary>
		/// <param name="sourceProvider">The source media storage provider</param>
		/// <param name="targetProvider">The target media storage provider</param>
		/// <returns><c>true</c> success, <c>failure</c></returns>
		bool Move(Provider<IMediaStorageProvider> sourceProvider, Provider<IMediaStorageProvider> targetProvider);
	}


	public interface IMovableMediaSupported
	{
		/// <summary>
		/// Moves a media item to the target provider
		/// </summary>
		/// <param name="target">Target provider</param>
		/// <param name="context">Media storage mover context</param>
		/// <param name="media">Media storage item</param>
		void MoveTo(IMovableMediaSupported target, MediaStorageMoverContext context, MediaStorageItem media);

		/// <summary>
		/// Data received by the source provider to be stored by the target provider
		/// </summary>
		/// <param name="context">Media storage mover context</param>
		/// <param name="media">Media storage item</param>
		/// <param name="data">Binary data</param>
		void StoreMovingData(MediaStorageMoverContext context, MediaStorageItem media, byte[] data);

		/// <summary>
		/// Called when media moving ended
		/// </summary>
		/// <param name="context">Media storage mover context</param>
		/// <param name="succeeded">Whether media moving succeeded</param>
		void OnMoved(MediaStorageMoverContext context, bool succeeded);
	}


	public class MediaStorageMoverContext
	{
		internal MediaStorageMoverContext(string sourceSystemName, string targetSystemName)
		{
			SourceSystemName = sourceSystemName;
			TargetSystemName = targetSystemName;
			AffectedFiles = new List<string>();
			CustomProperties = new Dictionary<string, object>();
		}

		/// <summary>
		/// The system name of the source provider
		/// </summary>
		public string SourceSystemName { get; private set; }

		/// <summary>
		/// The system name of the target provider
		/// </summary>
		public string TargetSystemName { get; private set; }

		/// <summary>
		/// Current number of moved media items
		/// </summary>
		public int MovedItems { get; internal set; }

		/// <summary>
		/// Paths of affected media files
		/// </summary>
		public List<string> AffectedFiles { get; private set; }

		/// <summary>
		/// Whether to shrink database after succesful moving
		/// </summary>
		public bool ShrinkDatabase { get; set; }

		/// <summary>
		/// Use this dictionary for any custom data required along the move operation
		/// </summary>
		public Dictionary<string, object> CustomProperties { get; set; }
	}
}
