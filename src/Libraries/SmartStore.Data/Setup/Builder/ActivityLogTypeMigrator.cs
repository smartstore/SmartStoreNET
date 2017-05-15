using System.Data.Entity;
using System.Linq;
using SmartStore.Core.Domain.Configuration;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Logging;

namespace SmartStore.Data.Setup
{
	internal class ActivityLogTypeMigrator
	{
		private readonly SmartObjectContext _ctx;
		private readonly DbSet<ActivityLogType> _activityLogTypeRecords;

		public ActivityLogTypeMigrator(SmartObjectContext ctx)
		{
			Guard.NotNull(ctx, nameof(ctx));

			_ctx = ctx;
			_activityLogTypeRecords = _ctx.Set<ActivityLogType>();
		}

		private Language GetDefaultAdminLanguage(DbSet<Setting> settingRecords, DbSet<Language> languageRecords)
		{
			const string settingKey = "LocalizationSettings.DefaultAdminLanguageId";

			var defaultAdminLanguageSetting = settingRecords.FirstOrDefault(x => x.Name == settingKey && x.StoreId == 0);

			if (defaultAdminLanguageSetting == null)
				defaultAdminLanguageSetting = settingRecords.FirstOrDefault(x => x.Name == settingKey);

			if (defaultAdminLanguageSetting != null)
			{
				var defaultAdminLanguageId = defaultAdminLanguageSetting.Value.ToInt();
				if (defaultAdminLanguageId != 0)
				{
					var language = languageRecords.FirstOrDefault(x => x.Id == defaultAdminLanguageId);
					if (language != null)
						return language;
				}
			}

			return languageRecords.First();
		}

		public void AddActivityLogType(string systemKeyword, string enName, string deName)
		{
			Guard.NotEmpty(systemKeyword, nameof(systemKeyword));
			Guard.NotEmpty(enName, nameof(enName));
			Guard.NotEmpty(deName, nameof(deName));

			var record = _activityLogTypeRecords.FirstOrDefault(x => x.SystemKeyword == systemKeyword);

			if (record == null)
			{
				var language = GetDefaultAdminLanguage(_ctx.Set<Setting>(), _ctx.Set<Language>());

				_activityLogTypeRecords.Add(new ActivityLogType
				{
					Enabled = true,
					SystemKeyword = systemKeyword,
					Name = (language.UniqueSeoCode.IsCaseInsensitiveEqual("de") ? deName : enName)
				});

				_ctx.SaveChanges();
			}
		}
	}
}
