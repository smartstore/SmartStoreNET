using System;

namespace SmartStore.Core.Infrastructure.DependencyManagement
{
    public class Work<T> where T : class
    {
        private readonly Func<Work<T>, T> _resolve;

        public Work(Func<Work<T>, T> resolve)
        {
            _resolve = resolve;
        }

        public T Value => _resolve(this);
    }
}
