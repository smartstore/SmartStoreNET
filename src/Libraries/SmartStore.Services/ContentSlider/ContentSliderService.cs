using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.ContentSlider;
using SmartStore.Core.Domain.Localization;

namespace SmartStore.Services.ContentSlider
{
    class ContentSliderService : IContentSliderService
    {
        private readonly IRepository<Core.Domain.ContentSlider.ContentSlider> _contentSliderRepository;
        private readonly IRepository<Slide> _contentSliderSlideRepository;
        private readonly IDbContext _dbContext;
        private readonly LocalizationSettings _localizationSettings;
        private readonly ICommonServices _services;

        public ContentSliderService(
            IRepository<Core.Domain.ContentSlider.ContentSlider> contentSliderRepository,
            IRepository<Slide> contentSliderSlideRepository,
            IDbContext dbContext,
            LocalizationSettings localizationSettings,
            ICommonServices services)
        {
            _contentSliderRepository = contentSliderRepository;
            _contentSliderSlideRepository = contentSliderSlideRepository;
            _dbContext = dbContext;
            _localizationSettings = localizationSettings;
            _services = services;
        }

        public void DeleteContentSlider(Core.Domain.ContentSlider.ContentSlider contentslider)
        {
            Guard.NotNull(contentslider, nameof(contentslider));

            _contentSliderRepository.Delete(contentslider);
        }

        public void DeleteContentSliderSlide(Slide contentSliderSlide)
        {
            Guard.NotNull(contentSliderSlide, nameof(contentSliderSlide));

            _contentSliderSlideRepository.Delete(contentSliderSlide);
        }

        public IList<Core.Domain.ContentSlider.ContentSlider> GetAllContentSliders()
        {
            var query =
                from cs in _contentSliderRepository.Table
                orderby cs.Id
                where cs.IsActive
                select cs;

            var contentSliders = query.ToList();
            return contentSliders;
        }

        public Core.Domain.ContentSlider.ContentSlider GetContentSliders(int SliderId)
        {
            var query =
                from cs in _contentSliderRepository.Table
                orderby cs.Id
                where cs.IsActive && cs.Id==SliderId
                select cs;

            var contentSlider = query.FirstOrDefault();
            return contentSlider;
        }

        public Slide GetContentSliderSlideById(int slideId)
        {
            var query =
                from s in _contentSliderSlideRepository.Table
                orderby s.Id
                where s.IsActive && s.Id == slideId
                select s;

            var slide = query.FirstOrDefault();
            return slide;
        }

        public IList<Slide> GetContentSliderSlides()
        {
            var query =
                from s in _contentSliderSlideRepository.Table
                orderby s.Id
                where s.IsActive
                select s;

            var slides = query.ToList();
            return slides;
        }

        public IList<Slide> GetContentSliderSlidesBySliderId(int contentSliderId)
        {
            var query =
                from s in _contentSliderSlideRepository.Table
                orderby s.Id
                where s.IsActive && s.SliderId== contentSliderId
                select s;

            var slides = query.ToList();
            return slides;
        }

        public void InsertContentSlider(Core.Domain.ContentSlider.ContentSlider contentslider)
        {
            Guard.NotNull(contentslider, nameof(contentslider));
            _contentSliderRepository.Insert(contentslider);
        }

        public void InsertContentSliderSlide(Slide contentSliderSlide)
        {
            Guard.NotNull(contentSliderSlide, nameof(contentSliderSlide));
            _contentSliderSlideRepository.Insert(contentSliderSlide);
        }

        public void UpdateContentSlider(Core.Domain.ContentSlider.ContentSlider contentslider)
        {
            Guard.NotNull(contentslider, nameof(contentslider));

            _contentSliderRepository.Update(contentslider);
        }

        public void UpdateContentSliderSlide(Slide contentSliderSlide)
        {
            Guard.NotNull(contentSliderSlide, nameof(contentSliderSlide));
            _contentSliderSlideRepository.Update(contentSliderSlide);
        }
    }
}
