using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using SmartStore.ComponentModel;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.IO;

namespace SmartStore.Services.Media
{	
	public class MediaPathData
	{
		private string _title;
		private string _ext;
		private string _mime;

		public MediaPathData(MediaFolderNode folder, string fileName)
		{
			Folder = folder;
			FileName = fileName;
		}

		public MediaFolderNode Folder { get; }
		public string FileName { get; }

		public string FileTitle 
		{ 
			get
			{
				return _title ?? (_title = Path.GetFileNameWithoutExtension(FileName));
			} 
		}

		public string Extension
		{
			get
			{
				return _ext ?? (_ext = Path.GetExtension(FileName).EmptyNull());
			}
			set
			{
				_ext = value;
			}
		}

		public string MimeType
		{
			get
			{
				return _mime ?? (_mime = MimeTypes.MapNameToMimeType(FileName));
			}
			set
			{
				_mime = value;
			}
		}
	}
	
	public partial class MediaHelper
	{
		private readonly IFolderService _folderService;
		
		public MediaHelper(IFolderService folderService)
		{
			_folderService = folderService;
		}

		public bool TokenizePath(string path, out MediaPathData data)
		{
			data = null;

			if (path.IsEmpty())
			{
				return false;
			}

			var dir = Path.GetDirectoryName(path);
			if (dir.HasValue())
			{
				var node = _folderService.GetNodeByPath(dir);
				if (node != null)
				{
					data = new MediaPathData(node.Value, path.Substring(dir.Length + 1));
					return true;
				}
			}

			return false;
		}

		#region Legacy (remove later)

		public static void UpdateDownloadTransientStateFor<TEntity>(TEntity entity, Expression<Func<TEntity, int>> downloadIdProp, bool save = false) where TEntity : BaseEntity
		{
			Guard.NotNull(entity, nameof(entity));
			Guard.NotNull(downloadIdProp, nameof(downloadIdProp));

			var propName = downloadIdProp.ExtractMemberInfo().Name;
			int currentDownloadId = downloadIdProp.CompileFast(PropertyCachingStrategy.EagerCached).Invoke(entity);
			var rs = EngineContext.Current.Resolve<IRepository<Download>>();

			UpdateTransientStateForEntityInternal(entity, propName, currentDownloadId, rs, null, save);
		}

		public static void UpdateDownloadTransientStateFor<TEntity>(TEntity entity, Expression<Func<TEntity, int?>> downloadIdProp, bool save = false) where TEntity : BaseEntity
		{
			Guard.NotNull(entity, nameof(entity));
			Guard.NotNull(downloadIdProp, nameof(downloadIdProp));

			var propName = downloadIdProp.ExtractMemberInfo().Name;
			int currentDownloadId = downloadIdProp.CompileFast(PropertyCachingStrategy.EagerCached).Invoke(entity).GetValueOrDefault();
			var rs = EngineContext.Current.Resolve<IRepository<Download>>();

			UpdateTransientStateForEntityInternal(entity, propName, currentDownloadId, rs, null, save);
		}

		public static void UpdateDownloadTransientState(int? prevDownloadId, int? currentDownloadId, bool save = false)
		{
			var rs = EngineContext.Current.Resolve<IRepository<Download>>();
			UpdateTransientStateCore(prevDownloadId.GetValueOrDefault(), currentDownloadId.GetValueOrDefault(), rs, null, save);
		}

		public static void UpdateDownloadTransientState(int prevDownloadId, int currentDownloadId, bool save = false)
		{
			var rs = EngineContext.Current.Resolve<IRepository<Download>>();
			UpdateTransientStateCore(prevDownloadId, currentDownloadId, rs, null, save);
		}

		private static Action<object> GetPictureDeleteAction()
		{
			Action<object> deleteAction = pic =>
			{
				var svc = EngineContext.Current.Resolve<IPictureService>();
				svc.DeletePicture((MediaFile)pic);
			};

			return deleteAction;
		}

		internal static void UpdateTransientStateForEntityInternal<TEntity, TMedia>(
			TEntity entity, 
			string propName, 
			int currentMediaId,
			IRepository<TMedia> rs,
			Action<object> deleteAction,
			bool save) where TEntity : BaseEntity where TMedia : BaseEntity
		{
			//object obj = null;
			//int prevMediaId = 0;
			//if (rs.Context.TryGetModifiedProperty(entity, propName, out obj))
			//{
			//	prevMediaId = ((int?)obj).GetValueOrDefault();
			//}

			//UpdateTransientStateCore(prevMediaId, currentMediaId, rs, deleteAction, save);
		}

		internal static void UpdateTransientStateCore<TMedia>(
			int prevMediaId,
			int currentMediaId,
			IRepository<TMedia> rs,
			Action<object> deleteAction,
			bool save)
			where TMedia : BaseEntity
		{
			var autoCommit = rs.AutoCommitEnabled;
			rs.AutoCommitEnabled = false;

			try
			{
				TMedia media = null;
				bool shouldSave = false;

				bool isModified = prevMediaId != currentMediaId;

				if (currentMediaId > 0 && isModified)
				{
					// new entity with a media or media replaced
					media = rs.GetById(currentMediaId);
					if (media != null)
					{
						var transient = media as ITransient;
						if (transient != null)
						{
							transient.IsTransient = false;
							shouldSave = true;
						}
					}
				}

				if (prevMediaId > 0 && isModified)
				{
					// ID changed, so delete old record
					media = rs.GetById(prevMediaId);
					if (media != null)
					{
						if (deleteAction == null)
						{
							rs.Delete(media);
						}
						else
						{
							deleteAction(media);
						}
						shouldSave = true;
					}
				}

				if (save && shouldSave)
				{
					rs.Context.SaveChanges();
				}
			}
			finally
			{
				rs.AutoCommitEnabled = autoCommit;
			}
		}

        #endregion
    }
}
