using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Events;
using SmartStore.Services.Configuration;
using SmartStore.Services.Localization;
using SmartStore.Services.Stores;
using SmartStore.Tests;

namespace SmartStore.Services.Tests.Localization
{
    [TestFixture]
    public class LanguageServiceTests : ServiceTest
    {
        IRepository<Language> _languageRepo;
        IStoreMappingService _storeMappingService;
        IStoreService _storeService;
        IStoreContext _storeContext;
        ILanguageService _languageService;
        ISettingService _settingService;
        IEventPublisher _eventPublisher;
        LocalizationSettings _localizationSettings;

        [SetUp]
        public new void SetUp()
        {
            _languageRepo = MockRepository.GenerateMock<IRepository<Language>>();
            var lang1 = new Language
            {
                Name = "English",
                LanguageCulture = "en-Us",
                FlagImageFileName = "us.png",
                Published = true,
                DisplayOrder = 1
            };
            var lang2 = new Language
            {
                Name = "Russian",
                LanguageCulture = "ru-Ru",
                FlagImageFileName = "ru.png",
                Published = true,
                DisplayOrder = 2
            };

            _languageRepo.Expect(x => x.Table).Return(new List<Language>() { lang1, lang2 }.AsQueryable());

            _storeMappingService = MockRepository.GenerateMock<IStoreMappingService>();
            _storeService = MockRepository.GenerateMock<IStoreService>();
            _storeContext = MockRepository.GenerateMock<IStoreContext>();

            var cacheManager = new NullCache();

            _settingService = MockRepository.GenerateMock<ISettingService>();

            _eventPublisher = MockRepository.GenerateMock<IEventPublisher>();
            _eventPublisher.Expect(x => x.Publish(Arg<object>.Is.Anything));

            _localizationSettings = new LocalizationSettings();
            _languageService = new LanguageService(
                NullRequestCache.Instance,
                NullCache.Instance,
                _languageRepo,
                _settingService,
                _localizationSettings,
                _eventPublisher,
                _storeMappingService,
                _storeService,
                _storeContext);
        }

        [Test]
        public void Can_get_all_languages()
        {
            var languages = _languageService.GetAllLanguages();
            languages.ShouldNotBeNull();
            (languages.Count > 0).ShouldBeTrue();
        }
    }
}
