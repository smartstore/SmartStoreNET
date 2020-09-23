using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Events;
using SmartStore.Data.Caching;

namespace SmartStore.Services.Directory
{
    public partial class MeasureService : IMeasureService
    {
        private readonly IRepository<MeasureDimension> _measureDimensionRepository;
        private readonly IRepository<MeasureWeight> _measureWeightRepository;
        private readonly MeasureSettings _measureSettings;

        public MeasureService(
            IRepository<MeasureDimension> measureDimensionRepository,
            IRepository<MeasureWeight> measureWeightRepository,
            MeasureSettings measureSettings)
        {
            _measureDimensionRepository = measureDimensionRepository;
            _measureWeightRepository = measureWeightRepository;
            _measureSettings = measureSettings;
        }

        #region Dimensions

        public virtual void DeleteMeasureDimension(MeasureDimension measureDimension)
        {
            if (measureDimension == null)
                throw new ArgumentNullException("measureDimension");

            _measureDimensionRepository.Delete(measureDimension);
        }

        public virtual MeasureDimension GetMeasureDimensionById(int measureDimensionId)
        {
            if (measureDimensionId == 0)
                return null;

            return _measureDimensionRepository.GetByIdCached(measureDimensionId, "db.dimension.id-" + measureDimensionId);
        }

        public virtual MeasureDimension GetMeasureDimensionBySystemKeyword(string systemKeyword)
        {
            if (String.IsNullOrEmpty(systemKeyword))
                return null;

            var measureDimensions = GetAllMeasureDimensions();
            foreach (var measureDimension in measureDimensions)
                if (measureDimension.SystemKeyword.ToLowerInvariant() == systemKeyword.ToLowerInvariant())
                    return measureDimension;
            return null;
        }

        public virtual IList<MeasureDimension> GetAllMeasureDimensions()
        {
            var query = from md in _measureDimensionRepository.Table
                        orderby md.DisplayOrder
                        select md;
            var measureDimensions = query.ToListCached("db.dimension.all");
            return measureDimensions;
        }

        public virtual void InsertMeasureDimension(MeasureDimension measure)
        {
            if (measure == null)
                throw new ArgumentNullException("measure");

            _measureDimensionRepository.Insert(measure);
        }

        public virtual void UpdateMeasureDimension(MeasureDimension measure)
        {
            if (measure == null)
                throw new ArgumentNullException("measure");

            _measureDimensionRepository.Update(measure);
        }

        public virtual decimal ConvertDimension(decimal quantity, MeasureDimension sourceMeasureDimension, MeasureDimension targetMeasureDimension, bool round = true)
        {
            decimal result = quantity;
            if (result != decimal.Zero && sourceMeasureDimension.Id != targetMeasureDimension.Id)
            {
                result = ConvertToPrimaryMeasureDimension(result, sourceMeasureDimension);
                result = ConvertFromPrimaryMeasureDimension(result, targetMeasureDimension);
            }
            if (round)
                result = Math.Round(result, 2);
            return result;
        }

        public virtual decimal ConvertToPrimaryMeasureDimension(decimal quantity, MeasureDimension sourceMeasureDimension)
        {
            decimal result = quantity;
            var baseDimensionIn = GetMeasureDimensionById(_measureSettings.BaseDimensionId);
            if (result != decimal.Zero && sourceMeasureDimension.Id != baseDimensionIn.Id)
            {
                decimal exchangeRatio = sourceMeasureDimension.Ratio;
                if (exchangeRatio == decimal.Zero)
                    throw new SmartException(string.Format("Exchange ratio not set for dimension [{0}]", sourceMeasureDimension.Name));
                result = result / exchangeRatio;
            }
            return result;
        }

        public virtual decimal ConvertFromPrimaryMeasureDimension(decimal quantity, MeasureDimension targetMeasureDimension)
        {
            decimal result = quantity;
            var baseDimensionIn = GetMeasureDimensionById(_measureSettings.BaseDimensionId);
            if (result != decimal.Zero && targetMeasureDimension.Id != baseDimensionIn.Id)
            {
                decimal exchangeRatio = targetMeasureDimension.Ratio;
                if (exchangeRatio == decimal.Zero)
                    throw new SmartException(string.Format("Exchange ratio not set for dimension [{0}]", targetMeasureDimension.Name));
                result = result * exchangeRatio;
            }
            return result;
        }

        #endregion

        #region Weights

        public virtual void DeleteMeasureWeight(MeasureWeight measureWeight)
        {
            if (measureWeight == null)
                throw new ArgumentNullException("measureWeight");

            _measureWeightRepository.Delete(measureWeight);
        }

        public virtual MeasureWeight GetMeasureWeightById(int measureWeightId)
        {
            if (measureWeightId == 0)
                return null;

            return _measureWeightRepository.GetByIdCached(measureWeightId, "db.weight.id-" + measureWeightId);
        }

        public virtual MeasureWeight GetMeasureWeightBySystemKeyword(string systemKeyword)
        {
            if (!systemKeyword.HasValue())
                return null;

            var measureWeights = GetAllMeasureWeights();

            return measureWeights
                .Where(x => x.SystemKeyword.ToLowerInvariant() == systemKeyword.ToLowerInvariant())
                .FirstOrDefault();
        }

        public virtual IList<MeasureWeight> GetAllMeasureWeights()
        {
            var query = from mw in _measureWeightRepository.Table
                        orderby mw.DisplayOrder
                        select mw;

            var measureWeights = query.ToListCached("db.weight.all");
            return measureWeights;
        }

        public virtual void InsertMeasureWeight(MeasureWeight measure)
        {
            if (measure == null)
                throw new ArgumentNullException("measure");

            _measureWeightRepository.Insert(measure);
        }

        public virtual void UpdateMeasureWeight(MeasureWeight measure)
        {
            if (measure == null)
                throw new ArgumentNullException("measure");

            _measureWeightRepository.Update(measure);
        }

        public virtual decimal ConvertWeight(decimal quantity, MeasureWeight sourceMeasureWeight, MeasureWeight targetMeasureWeight, bool round = true)
        {
            decimal result = quantity;
            if (result != decimal.Zero && sourceMeasureWeight.Id != targetMeasureWeight.Id)
            {
                result = ConvertToPrimaryMeasureWeight(result, sourceMeasureWeight);
                result = ConvertFromPrimaryMeasureWeight(result, targetMeasureWeight);
            }

            if (round)
                result = Math.Round(result, 2);

            return result;
        }

        public virtual decimal ConvertToPrimaryMeasureWeight(decimal quantity, MeasureWeight sourceMeasureWeight)
        {
            decimal result = quantity;
            var baseWeightIn = GetMeasureWeightById(_measureSettings.BaseWeightId);
            if (result != decimal.Zero && sourceMeasureWeight.Id != baseWeightIn.Id)
            {
                decimal exchangeRatio = sourceMeasureWeight.Ratio;
                if (exchangeRatio == decimal.Zero)
                    throw new SmartException(string.Format("Exchange ratio not set for weight [{0}]", sourceMeasureWeight.Name));
                result = result / exchangeRatio;
            }
            return result;
        }

        public virtual decimal ConvertFromPrimaryMeasureWeight(decimal quantity, MeasureWeight targetMeasureWeight)
        {
            decimal result = quantity;
            var baseWeightIn = GetMeasureWeightById(_measureSettings.BaseWeightId);

            if (result != decimal.Zero && targetMeasureWeight.Id != baseWeightIn.Id)
            {
                decimal exchangeRatio = targetMeasureWeight.Ratio;
                if (exchangeRatio == decimal.Zero)
                    throw new SmartException(string.Format("Exchange ratio not set for weight [{0}]", targetMeasureWeight.Name));
                result = result * exchangeRatio;
            }

            return result;
        }

        #endregion
    }
}