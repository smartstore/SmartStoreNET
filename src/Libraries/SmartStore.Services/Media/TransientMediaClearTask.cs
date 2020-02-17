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
	/// (pictures and downloads which have been uploaded but never assigned to an entity).
    /// </summary>
    public partial class TransientMediaClearTask : AsyncTask
    {
		private readonly IPictureService _pictureService;
        private readonly IDownloadService _downloadService;
		private readonly IRepository<MediaFile> _pictureRepository;
		private readonly IRepository<Download> _downloadRepository;

		public TransientMediaClearTask(
			IPictureService pictureService,
            IDownloadService downloadService,
            IRepository<MediaFile> pictureRepository, 
			IRepository<Download> downloadRepository)
        {
			_pictureService = pictureService;
            _downloadService = downloadService;
			_pictureRepository = pictureRepository;
			_downloadRepository = downloadRepository;
        }

		public override async Task ExecuteAsync(TaskExecutionContext ctx)
        {
			// Delete all media records which are in transient state since at least 3 hours.
			var olderThan = DateTime.UtcNow.AddHours(-3);
			var pictureAutoCommit = _pictureRepository.AutoCommitEnabled;
            var downloadAutoCommit = _downloadRepository.AutoCommitEnabled;
			
            _pictureRepository.AutoCommitEnabled = false;
            _downloadRepository.AutoCommitEnabled = false;

			try
			{
				using (var scope = new DbContextScope(autoDetectChanges: false, validateOnSave: false, hooksEnabled: false))
				{
					var pictures = await _pictureRepository.Table.Where(x => x.IsTransient && x.UpdatedOnUtc < olderThan).ToListAsync();
					foreach (var picture in pictures)
					{
						_pictureService.DeletePicture(picture);
					}

					await _pictureRepository.Context.SaveChangesAsync();

                    var downloads = await _downloadRepository.Table.Where(x => x.IsTransient && x.UpdatedOnUtc < olderThan).ToListAsync();
                    foreach (var download in downloads)
                    {
                        _downloadService.DeleteDownload(download);
                    }

                    await _downloadRepository.Context.SaveChangesAsync();

                    if (DataSettings.Current.IsSqlServer && (pictures.Any() || downloads.Any()))
                    {
                        try
                        {
                            _pictureRepository.Context.ExecuteSqlCommand("DBCC SHRINKDATABASE(0)", true);
                        }
                        catch { }
                    }
                }
            }
            finally
			{
				_pictureRepository.AutoCommitEnabled = pictureAutoCommit;
                _downloadRepository.AutoCommitEnabled = downloadAutoCommit;
			}
        }
    }
}
