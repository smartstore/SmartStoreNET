using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DotLiquid;

namespace SmartStore.Templating.Liquid
{
    internal class EnumerableWrapper : IEnumerable<ILiquidizable>, ISafeObject
    {
        private readonly IEnumerable _enumerable;

        public EnumerableWrapper(IEnumerable enumerable)
        {
            Guard.NotNull(enumerable, nameof(enumerable));
            _enumerable = enumerable;
        }

        public IEnumerator GetEnumerator()
        {
            return GetEnumeratorInternal();
        }

        public object GetWrappedObject()
        {
            return _enumerable;
        }

        IEnumerator<ILiquidizable> IEnumerable<ILiquidizable>.GetEnumerator()
        {
            return GetEnumeratorInternal();
        }

        private IEnumerator<ILiquidizable> GetEnumeratorInternal()
        {
            return _enumerable
                .Cast<object>()
                .Select(x => LiquidUtil.CreateSafeObject(x))
                .OfType<ILiquidizable>()
                .GetEnumerator();
        }
    }
}
