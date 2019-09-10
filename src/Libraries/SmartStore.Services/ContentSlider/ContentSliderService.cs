using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.ContentSlider;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Stores;

namespace SmartStore.Services.ContentSlider
{
    public class ContentSliderService : IContentSliderService
    {
        private readonly IRepository<Core.Domain.ContentSlider.ContentSlider> _contentSliderRepository;
        private readonly IRepository<Slide> _contentSliderSlideRepository;
        private readonly IDbContext _dbContext;
        private readonly LocalizationSettings _localizationSettings;
        private readonly ICommonServices _services;
        public DbQuerySettings QuerySettings { get; set; }
        private readonly IRepository<StoreMapping> _storeMappingRepository;


        public ContentSliderService(
            IRepository<Core.Domain.ContentSlider.ContentSlider> contentSliderRepository,
            IRepository<Slide> contentSliderSlideRepository,
            IDbContext dbContext,
            LocalizationSettings localizationSettings,
            ICommonServices services,
            IRepository<StoreMapping> storeMappingRepository
            )
        {
            _contentSliderRepository = contentSliderRepository;
            _contentSliderSlideRepository = contentSliderSlideRepository;
            _dbContext = dbContext;
            _localizationSettings = localizationSettings;
            _services = services;
            this.QuerySettings = DbQuerySettings.Default;
            _storeMappingRepository = storeMappingRepository;
        }

        public virtual IPagedList<Core.Domain.ContentSlider.ContentSlider> GetAllContentSliders(string contentSliderName,
            int pageIndex, int pageSize, int storeId = 0, bool showHidden = false)
        {
            var contentsliders = GetAllContentSliders(contentSliderName, storeId, showHidden);
            return new PagedList<Core.Domain.ContentSlider.ContentSlider>(contentsliders, pageIndex, pageSize);
        }

        public virtual IPagedList<Core.Domain.ContentSlider.ContentSlider> GetAllContentSliders(int contentSlideId,
            int pageIndex, int pageSize, int storeId = 0, bool showHidden = false)
        {
            var contentsliders = GetAllContentSliders(contentSlideId, storeId, showHidden);
            return new PagedList<Core.Domain.ContentSlider.ContentSlider>(contentsliders, pageIndex, pageSize);
        }

        public virtual IList<Core.Domain.ContentSlider.ContentSlider> GetAllContentSliders(string contentSliderName, int storeId = 0, bool showHidden = false)
        {
            var query = GetContentSliders(showHidden, storeId);

            if (contentSliderName.HasValue())
                query = query.Where(m => m.SliderName.Contains(contentSliderName));

            query = query.OrderBy(m => m.SliderName);

            var contentSliders = query.ToList();
            return contentSliders;
        }

        public virtual IList<Core.Domain.ContentSlider.ContentSlider> GetAllContentSliders(int contentSliderId, int storeId = 0, bool showHidden = false)
        {
            var query = GetContentSliders(showHidden, storeId);

            query = query.Where(m => m.Id.Equals(contentSliderId));

            query = query.OrderBy(m => m.SliderName);

            var contentSliders = query.ToList();
            return contentSliders;
        }

        public virtual IQueryable<Core.Domain.ContentSlider.ContentSlider> GetContentSliders(bool showHidden = false, int storeId = 0)
        {
            var query = _contentSliderRepository.Table;

            return query;
        }

        public Core.Domain.ContentSlider.ContentSlider DeleteContentSlider(Core.Domain.ContentSlider.ContentSlider contentslider)
        {
            Guard.NotNull(contentslider, nameof(contentslider));

            _contentSliderRepository.Delete(contentslider);
            return GetContentSliders(contentslider.Id);
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
                select cs;

            var contentSliders = query.ToList();
            return contentSliders;
        }

        public Core.Domain.ContentSlider.ContentSlider GetContentSliders(int SliderId)
        {
            var query =
                from cs in _contentSliderRepository.Table
                orderby cs.Id
                where cs.Id == SliderId
                select cs;

            var contentSlider = query.FirstOrDefault();
            return contentSlider;
        }

        public IList<Core.Domain.ContentSlider.ContentSlider> GetContentSliderByType(int SliderType)
        {
            var query =
                from cs in _contentSliderRepository.Table
                orderby cs.Id
                where cs.IsActive == true && cs.SliderType == SliderType && cs.ItemId == null
                select cs;

            var contentSliders = query.ToList();
            return contentSliders;
        }

        public IList<Core.Domain.ContentSlider.ContentSlider> GetContentSliderByTypeAndItemId(int SliderType, int ItemId)
        {
            var query =
                from cs in _contentSliderRepository.Table
                orderby cs.Id
                where cs.IsActive == true && cs.SliderType == SliderType && cs.ItemId.Value == ItemId
                select cs;

            var contentSliders = query.ToList();
            return contentSliders;
        }

        public Slide GetContentSliderActiveSlideById(int slideId)
        {
            var query =
                from s in _contentSliderSlideRepository.Table
                orderby s.Id
                where s.IsActive == true && s.Id == slideId
                select s;

            var slide = query.FirstOrDefault();
            return slide;
        }

        public Slide GetContentSliderSlideById(int slideId)
        {
            var query =
                from s in _contentSliderSlideRepository.Table
                orderby s.Id
                where s.Id == slideId
                select s;

            var slide = query.FirstOrDefault();
            return slide;
        }

        public IList<Slide> GetContentSliderSlides()
        {
            var query =
                from s in _contentSliderSlideRepository.Table
                orderby s.Id
                where s.IsActive == true
                select s;

            var slides = query.ToList();
            return slides;
        }

        public IList<Slide> GetContentSliderSlidesBySliderId(int contentSliderId)
        {
            var query =
                from s in _contentSliderSlideRepository.Table
                orderby s.DisplayOrder
                where s.IsActive == true && s.SliderId == contentSliderId
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

        public virtual IPagedList<Slide> GetSlidesContentSliderBySliderId(int SliderId, int pageIndex, int pageSize, bool showHidden = false)
        {
            if (SliderId == 0)
                return new PagedList<Slide>(new List<Slide>(), pageIndex, pageSize);

            var query =
               from s in _contentSliderSlideRepository.Table
               orderby s.DisplayOrder
               where s.IsActive == true && s.SliderId == SliderId
               select s;


            var slideContentSliders = new PagedList<Slide>(query, pageIndex, pageSize);
            return slideContentSliders;
        }
    }
}
