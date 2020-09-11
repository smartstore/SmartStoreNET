using System;
using System.Reflection;
using Newtonsoft.Json.Serialization;

namespace SmartStore.ComponentModel
{
    public class FastPropertyValueProvider : IValueProvider
    {
        private readonly PropertyInfo _pi;

        private IValueProvider _inner;
        private Func<object, object> _getter;
        private Action<object, object> _setter;

        public FastPropertyValueProvider(PropertyInfo pi)
        {
            _pi = pi;
        }

        private void CreateInnerProvider()
        {
            _inner = new DynamicValueProvider(_pi);
        }

        public object GetValue(object target)
        {
            if (_inner != null)
            {
                return _inner.GetValue(target);
            }

            if (_getter == null)
            {
                try
                {
                    _getter = FastProperty.GetProperty(_pi, PropertyCachingStrategy.EagerCached).ValueGetter;
                    if (_getter == null) throw new Exception();
                }
                catch
                {
                    CreateInnerProvider();
                    return _inner.GetValue(target);
                }
            }

            return _getter(target);
        }

        public void SetValue(object target, object value)
        {
            if (_inner != null)
            {
                _inner.SetValue(target, value);
            }

            if (_setter == null)
            {
                try
                {
                    var fastProp = FastProperty.GetProperty(_pi, PropertyCachingStrategy.EagerCached);
                    if (fastProp.IsPublicSettable)
                    {
                        _setter = fastProp.ValueSetter;
                    }
                    if (_setter == null) throw new Exception();
                }
                catch
                {
                    CreateInnerProvider();
                    _inner.SetValue(target, value);
                    return;
                }
            }

            _setter(target, value);
        }
    }
}
