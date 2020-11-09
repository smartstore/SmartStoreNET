using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Media;
using SmartStore.Services.Tasks;

namespace SmartStore.Services.Media
{
    /// <summary>
    /// Represents a task for deleting transient media from the database
	/// (files and downloads which have been uploaded but never assigned to an entity).
    /// </summary>
    public partial class TransientMediaClearTask : AsyncTask
    {
        private readonly IMediaService _mediaService;
        private readonly IDownloadService _downloadService;
        private readonly IRepository<MediaFile> _fileRepository;
        private readonly IRepository<Download> _downloadRepository;

        public TransientMediaClearTask(
            IMediaService mediaService,
            IDownloadService downloadService,
            IRepository<MediaFile> fileRepository,
            IRepository<Download> downloadRepository)
        {
            _mediaService = mediaService;
            _downloadService = downloadService;
            _fileRepository = fileRepository;
            _downloadRepository = downloadRepository;
        }

        public override async Task ExecuteAsync(TaskExecutionContext ctx)
        {
            // Delete all media records which are in transient state since at least 3 hours.
            var olderThan = DateTime.UtcNow.AddHours(-3);
            var fileAutoCommit = _fileRepository.AutoCommitEnabled;
            var downloadAutoCommit = _downloadRepository.AutoCommitEnabled;

            _fileRepository.AutoCommitEnabled = false;
            _downloadRepository.AutoCommitEnabled = false;

            try
            {
                using (var scope = new DbContextScope(autoDetectChanges: false, validateOnSave: false, hooksEnabled: false))
                {
                    var files = await _fileRepository.Table.Where(x => x.IsTransient && x.UpdatedOnUtc < olderThan).ToListAsync();
                    foreach (var file in files)
                    {
                        _mediaService.DeleteFile(file, true);
                    }

                    await _fileRepository.Context.SaveChangesAsync();

                    var downloads = await _downloadRepository.Table.Where(x => x.IsTransient && x.UpdatedOnUtc < olderThan).ToListAsync();
                    foreach (var download in downloads)
                    {
                        _downloadService.DeleteDownload(download);
                    }

                    await _downloadRepository.Context.SaveChangesAsync();

                    if (DataSettings.Current.IsSqlServer && (files.Any() || downloads.Any()))
                    {
                        try
                        {
                            _fileRepository.Context.ExecuteSqlCommand("DBCC SHRINKDATABASE(0)", true);
                        }
                        catch { }
                    }
                }
            }
            finally
            {
                _fileRepository.AutoCommitEnabled = fileAutoCommit;
                _downloadRepository.AutoCommitEnabled = downloadAutoCommit;
            }
        }
    }
}
