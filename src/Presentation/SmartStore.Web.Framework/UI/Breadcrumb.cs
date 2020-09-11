using System.Collections.Generic;

namespace SmartStore.Web.Framework.UI
{
    public interface IBreadcrumb
    {
        void Track(MenuItem item, bool prepend = false);
        IReadOnlyList<MenuItem> Trail { get; }
    }

    public class DefaultBreadcrumb : IBreadcrumb
    {
        private List<MenuItem> _trail;

        public void Track(MenuItem item, bool prepend = false)
        {
            Guard.NotNull(item, nameof(item));

            if (_trail == null)
            {
                _trail = new List<MenuItem>();
            }

            if (prepend)
            {
                _trail.Insert(0, item);
            }
            else
            {
                _trail.Add(item);
            }
        }

        public IReadOnlyList<MenuItem> Trail => _trail;
    }
}
