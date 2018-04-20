using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Infrastructure;

namespace SmartStore.Services.Media
{	
	public static class MediaHelper
	{
		public static void UpdateDownloadTransientStateFor<TEntity>(TEntity entity, Expression<Func<TEntity, int>> downloadIdProp, bool save = false) where TEntity : BaseEntity
		{
			Guard.NotNull(entity, nameof(entity));
			Guard.NotNull(downloadIdProp, nameof(downloadIdProp));

			var propName = downloadIdProp.ExtractMemberInfo().Name;
			int currentDownloadId = downloadIdProp.Compile().Invoke(entity);
			var rs = EngineContext.Current.Resolve<IRepository<Download>>();

			UpdateTransientStateForEntityInternal(entity, propName, currentDownloadId, rs, null, save);
		}

		public static void UpdateDownloadTransientStateFor<TEntity>(TEntity entity, Expression<Func<TEntity, int?>> downloadIdProp, bool save = false) where TEntity : BaseEntity
		{
			Guard.NotNull(entity, nameof(entity));
			Guard.NotNull(downloadIdProp, nameof(downloadIdProp));

			var propName = downloadIdProp.ExtractMemberInfo().Name;
			int currentDownloadId = downloadIdProp.Compile().Invoke(entity).GetValueOrDefault();
			var rs = EngineContext.Current.Resolve<IRepository<Download>>();

			UpdateTransientStateForEntityInternal(entity, propName, currentDownloadId, rs, null, save);
		}

		public static void UpdatePictureTransientStateFor<TEntity>(TEntity entity, Expression<Func<TEntity, int>> pictureIdProp, bool save = false) where TEntity : BaseEntity
		{
			Guard.NotNull(entity, nameof(entity));
			Guard.NotNull(pictureIdProp, nameof(pictureIdProp));

			var propName = pictureIdProp.ExtractMemberInfo().Name;
			int currentPictureId = pictureIdProp.Compile().Invoke(entity);
			var rs = EngineContext.Current.Resolve<IRepository<Picture>>();

			UpdateTransientStateForEntityInternal(entity, propName, currentPictureId, rs, GetPictureDeleteAction(), save);
		}

		public static void UpdatePictureTransientStateFor<TEntity>(TEntity entity, Expression<Func<TEntity, int?>> pictureIdProp, bool save = false) where TEntity : BaseEntity
		{
			Guard.NotNull(entity, nameof(entity));
			Guard.NotNull(pictureIdProp, nameof(pictureIdProp));

			var propName = pictureIdProp.ExtractMemberInfo().Name;
			int currentPictureId = pictureIdProp.Compile().Invoke(entity).GetValueOrDefault();
			var rs = EngineContext.Current.Resolve<IRepository<Picture>>();

			UpdateTransientStateForEntityInternal(entity, propName, currentPictureId, rs, GetPictureDeleteAction(), save);
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

		public static void UpdatePictureTransientState(int? prevPictureId, int? currentPictureId, bool save = false)
		{
			var rs = EngineContext.Current.Resolve<IRepository<Picture>>();
			UpdateTransientStateCore(prevPictureId.GetValueOrDefault(), currentPictureId.GetValueOrDefault(), rs, GetPictureDeleteAction(), save);
		}

		public static void UpdatePictureTransientState(int prevPictureId, int currentPictureId, bool save = false)
		{
			var rs = EngineContext.Current.Resolve<IRepository<Picture>>();
			UpdateTransientStateCore(prevPictureId, currentPictureId, rs, GetPictureDeleteAction(), save);
		}

		private static Action<object> GetPictureDeleteAction()
		{
			Action<object> deleteAction = pic =>
			{
				var svc = EngineContext.Current.Resolve<IPictureService>();
				svc.DeletePicture((Picture)pic);
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
			object obj = null;
			int prevMediaId = 0;
			if (rs.Context.TryGetModifiedProperty(entity, propName, out obj))
			{
				prevMediaId = ((int?)obj).GetValueOrDefault();
			}

			UpdateTransientStateCore(prevMediaId, currentMediaId, rs, deleteAction, save);
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
	}

}
