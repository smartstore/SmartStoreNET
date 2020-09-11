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

namespace SmartStore.Services.Media
{
    public partial class MediaHelper
    {
        #region Static

        private readonly static char[] _invalidFileNameChars = Path.GetInvalidFileNameChars().Concat(new[] { '&' }).ToArray();
        private readonly static char[] _invalidFolderNameChars = Path.GetInvalidPathChars().Concat(new[] { '&', '/', '\\' }).ToArray();

        public static string NormalizeFileName(string fileName)
        {
            return string.Join("-", fileName.ToSafe().Split(_invalidFileNameChars));
        }

        public static string NormalizeFolderName(string folderName)
        {
            return string.Join("-", folderName.ToSafe().Split(_invalidFolderNameChars));
        }

        #endregion

        private readonly IFolderService _folderService;

        public MediaHelper(IFolderService folderService)
        {
            _folderService = folderService;
        }

        public bool TokenizePath(string path, bool normalizeFileName, out MediaPathData data)
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
                    data = new MediaPathData(node, path.Substring(dir.Length + 1), normalizeFileName);
                    return true;
                }
            }

            return false;
        }

        public bool CheckUniqueFileName(string title, string ext, string destFileName, out string uniqueName)
        {
            return CheckUniqueFileName(title, ext, new HashSet<string>(new[] { destFileName }, StringComparer.CurrentCultureIgnoreCase), out uniqueName);
        }

        public bool CheckUniqueFileName(string title, string ext, HashSet<string> destFileNames, out string uniqueName)
        {
            uniqueName = null;

            if (destFileNames.Count == 0)
            {
                return false;
            }

            int i = 1;
            while (true)
            {
                var test = string.Concat(title, "-", i, ".", ext.TrimStart('.'));
                if (!destFileNames.Contains(test))
                {
                    // Found our gap
                    uniqueName = test;
                    return true;
                }

                i++;
            }
        }

        #region Legacy (remove later)

        // TODO: (mm) (mc) remove this stuff

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

        //public static void UpdateDownloadTransientState(int? prevDownloadId, int? currentDownloadId, bool save = false)
        //{
        //	var rs = EngineContext.Current.Resolve<IRepository<Download>>();
        //	UpdateTransientStateCore(prevDownloadId.GetValueOrDefault(), currentDownloadId.GetValueOrDefault(), rs, null, save);
        //}

        //public static void UpdateDownloadTransientState(int prevDownloadId, int currentDownloadId, bool save = false)
        //{
        //	var rs = EngineContext.Current.Resolve<IRepository<Download>>();
        //	UpdateTransientStateCore(prevDownloadId, currentDownloadId, rs, null, save);
        //}

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

        #endregion
    }
}
