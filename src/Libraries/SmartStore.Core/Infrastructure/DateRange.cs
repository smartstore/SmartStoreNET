using System;
using System.Collections.Generic;

namespace SmartStore
{

    public class DateRange : ValueObject<DateRange>, ICloneable<DateRange>
    {

        public DateRange()
        {
        }

        public DateRange(DateTime to)
            : this(default(DateTime), to)
        {
        }

        public DateRange(DateTime from, DateTime to)
        {
            DateFrom = from;
            DateTo = to;
        }

        [ObjectSignature]
        public DateTime? DateFrom { get; set; }

        [ObjectSignature]
        public DateTime? DateTo { get; set; }

        public bool IsDefined
        {
            get { return DateFrom.HasValue || DateTo.HasValue; }
        }

        //public override int GetHashCode()
        //{
        //    return SystemUtil.GetHashCode(this.DateFrom, this.DateTo);
        //}

        //public override bool Equals(object obj)
        //{
        //    if (ReferenceEquals(null, obj)) return false;
        //    if (ReferenceEquals(this, obj)) return true;

        //    DateRange other = (DateRange)obj;
        //    return this.DateFrom.Equals(other.DateFrom) && this.DateTo.Equals(other.DateTo);
        //}

        #region ICloneable<DateRange> Members

        public DateRange Clone()
        {
            return new DateRange()
            {
                DateFrom = this.DateFrom,
                DateTo = this.DateTo
            };
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        #endregion

    }

}
