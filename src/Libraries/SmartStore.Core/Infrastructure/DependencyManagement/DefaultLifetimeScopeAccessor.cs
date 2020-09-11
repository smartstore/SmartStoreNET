using System;
using System.Web;
using Autofac;
using SmartStore.Utilities;

namespace SmartStore.Core.Infrastructure.DependencyManagement
{
    public class DefaultLifetimeScopeAccessor : ILifetimeScopeAccessor
    {
        class ContextAwareScope : IDisposable
        {
            private readonly Action _disposer;

            public ContextAwareScope(Action disposer)
            {
                _disposer = disposer;
            }

            public void Dispose()
            {
                _disposer?.Invoke();
            }
        }

        private readonly ContextState<ILifetimeScope> _state;
        private readonly ILifetimeScope _rootContainer;
        internal static readonly object ScopeTag = "AutofacWebRequest";

        public DefaultLifetimeScopeAccessor(ILifetimeScope rootContainer)
        {
            Guard.NotNull(rootContainer, nameof(rootContainer));

            //rootContainer.ChildLifetimeScopeBeginning += OnScopeBeginning;

            _rootContainer = rootContainer;
            _state = new ContextState<ILifetimeScope>("CustomLifetimeScopeProvider.WorkScope");
        }

        public ILifetimeScope ApplicationContainer => _rootContainer;

        public IDisposable BeginContextAwareScope()
        {
            // Stack-like behaviour for Non-HttpContext thread:
            // Only the first call returns a disposer, all nested calls to this method are void.
            return HttpContext.Current != null
                ? (IDisposable)ActionDisposable.Empty
                : new ContextAwareScope(
                    _state.GetState() == null
                        ? this.EndLifetimeScope
                        : (Action)null);
        }

        public void EndLifetimeScope()
        {
            try
            {
                var scope = _state.GetState();
                if (scope != null)
                {
                    scope.Dispose();
                    _state.RemoveState();
                }
            }
            catch { }
        }

        public ILifetimeScope GetLifetimeScope(Action<ContainerBuilder> configurationAction)
        {
            var scope = _state.GetState();
            if (scope == null)
            {
                _state.SetState((scope = BeginLifetimeScope(configurationAction)));
                //scope.CurrentScopeEnding += OnScopeEnding;
            }

            return scope;
        }

        //private void OnScopeBeginning(object sender, LifetimeScopeBeginningEventArgs args)
        //{
        //	bool isWeb = System.Web.HttpContext.Current != null;
        //	Debug.WriteLine("Scope Begin, Web: " + isWeb);
        //}

        //private void OnScopeEnding(object sender, LifetimeScopeEndingEventArgs args)
        //{
        //	bool isWeb = System.Web.HttpContext.Current != null;
        //	Debug.WriteLine("Scope END, Web: " + isWeb);
        //}

        public ILifetimeScope BeginLifetimeScope(Action<ContainerBuilder> configurationAction)
        {
            return (configurationAction == null)
                ? _rootContainer.BeginLifetimeScope(ScopeTag)
                : _rootContainer.BeginLifetimeScope(ScopeTag, configurationAction);
        }

    }
}
