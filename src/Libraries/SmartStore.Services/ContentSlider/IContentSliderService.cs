using System;
using System.Collections.Generic;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Domain.ContentSlider;

namespace SmartStore.Services.ContentSlider
{
    /// <summary>
    /// Product service
    /// </summary>
    public partial interface IContentSliderService
    {
        Core.Domain.ContentSlider.ContentSlider DeleteContentSlider(SmartStore.Core.Domain.ContentSlider.ContentSlider contentslider);
        IPagedList<Core.Domain.ContentSlider.ContentSlider> GetAllContentSliders(string contentSliderName,
           int pageIndex, int pageSize, int storeId = 0, bool showHidden = false);
        IPagedList<Core.Domain.ContentSlider.ContentSlider> GetAllContentSliders(int contentSliderId,
           int pageIndex, int pageSize, int storeId = 0, bool showHidden = false);
        IList<SmartStore.Core.Domain.ContentSlider.ContentSlider> GetAllContentSliders();
        SmartStore.Core.Domain.ContentSlider.ContentSlider GetContentSliders(int SliderId);
        IList<Core.Domain.ContentSlider.ContentSlider> GetContentSliderByType(int SliderType);
        IList<Core.Domain.ContentSlider.ContentSlider> GetContentSliderByTypeAndItemId(int SliderType,int ItemId);

        void InsertContentSlider(SmartStore.Core.Domain.ContentSlider.ContentSlider contentslider);

		void UpdateContentSlider(SmartStore.Core.Domain.ContentSlider.ContentSlider contentslider);

        void DeleteContentSliderSlide(Slide contentSliderSlide);

        IList<Slide> GetContentSliderSlidesBySliderId(int contentSliderId);
        IList<Slide> GetContentSliderSlides();

        Slide GetContentSliderSlideById(int slideId);
        Slide GetContentSliderActiveSlideById(int slideId);

        void InsertContentSliderSlide(Slide contentSliderSlide);

        void UpdateContentSliderSlide(Slide contentSliderSlide);
        IPagedList<Slide> GetSlidesContentSliderBySliderId(int SliderId, int pageIndex, int pageSize, bool showHidden = false);
    }
}
