using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using SmartStore.Linq.Expressions;

namespace SmartStore.Linq
{

    public class StringPathExpander : IPathExpander
    {
        private readonly IList<string> _expands;

        public StringPathExpander()
        {
            _expands = new List<string>();
        }

        public IList<string> Expands => _expands;

        public virtual void Expand<T>(Expression<Func<T, object>> path)
        {
            this.Expand<T, T>(path);
        }

        public virtual void Expand<T, TTarget>(Expression<Func<TTarget, object>> path)
        {
            Guard.NotNull(path, "path");

            string pathExpression = String.Empty;

            var visitor = new MemberAccessPathVisitor();
            visitor.Visit(path);
            if (typeof(T) == typeof(TTarget))
                _expands.Add(visitor.Path);
            else
            {
                //The path represents a collection association. Find the property on the target type that
                //matches an IEnumerable<TTarget> property.
                pathExpression = visitor.Path;
                var rootType = typeof(T);
                var targetType = typeof(IEnumerable<TTarget>);
                var targetProperty = (from property in rootType.GetProperties()
                                      where targetType.IsAssignableFrom(property.PropertyType)
                                      select property).FirstOrDefault();
                if (targetProperty != null)
                {
                    pathExpression = String.Format("{0}.{1}", targetProperty.Name, pathExpression);
                }
            }

            if (!String.IsNullOrEmpty(pathExpression))
            {
                _expands.Add(pathExpression);
            }
        }

        public virtual void Expand<T>(string path)
        {
            Guard.NotEmpty(path, "path");
            _expands.Add(path);
        }

        //public abstract void ApplyExpansion();

    }

}
