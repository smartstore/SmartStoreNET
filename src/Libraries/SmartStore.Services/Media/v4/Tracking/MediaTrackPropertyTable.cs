using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Collections;
using SmartStore.Core;

namespace SmartStore.Services.Media
{
    public class MediaTrackPropertyTable
    {
        private readonly Multimap<Type, PropertyInfo> _propertyMap = new Multimap<Type, PropertyInfo>(x => new HashSet<PropertyInfo>(x));
        
        protected internal MediaTrackPropertyTable()
        {
        }

        public void Register<T>(Expression<Func<T, int>> foreignKeyProperty) where T : BaseEntity
        {
            _propertyMap.Add(typeof(T), foreignKeyProperty.ExtractPropertyInfo());
        }

        public void Register<T>(Expression<Func<T, int?>> foreignKeyProperty) where T : BaseEntity
        {
            _propertyMap.Add(typeof(T), foreignKeyProperty.ExtractPropertyInfo());
        }
    }
}
