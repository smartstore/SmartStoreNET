using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Rhino.Mocks;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Events;
using SmartStore.Core.Logging;
using SmartStore.Core.Security;
using SmartStore.Services;
using SmartStore.Services.Configuration;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Messages;
using SmartStore.Services.Stores;

namespace SmartStore.Tests
{
    public class MockCommonServices : ICommonServices
    {
        static MockCommonServices()
        {
            // Autofac contains an unreplicable attribute 'NullableContextAttribute'.
            // Add it to Castle Proxy "avoid" list. 
            // https://stackoverflow.com/questions/64011323/cant-stub-class-with-nullablecontextattribute
            var assemblies = new Assembly[] { typeof(IComponentContext).Assembly };

            foreach (var assembly in assemblies)
            {
                var attrTypeToAvoid = Array.Find(assembly.GetTypes(), t => t.Name == "NullableContextAttribute");
                if (attrTypeToAvoid != null)
                {
                    Castle.DynamicProxy.Generators.AttributesToAvoidReplicating.Add(attrTypeToAvoid);
                    Console.WriteLine($"Found attribute to avoid: {attrTypeToAvoid.AssemblyQualifiedNameWithoutVersion()}");
                }
            }
        }

        private IDbContext _dbContext;

        public MockCommonServices() : this(MockRepository.GenerateMock<IComponentContext>())
        {
        }

        public MockCommonServices(IComponentContext container)
        {
            Container = container;
            EventPublisher = MockRepository.GenerateStub<IEventPublisher>();
        }

        public IComponentContext Container { get; }
        public IApplicationEnvironment ApplicationEnvironment => MockRepository.GenerateMock<IApplicationEnvironment>();
        public ICacheManager Cache => NullCache.Instance;
        public IRequestCache RequestCache => NullRequestCache.Instance;
        public IStoreContext StoreContext => MockRepository.GenerateMock<IStoreContext>();
        public IWebHelper WebHelper => MockRepository.GenerateMock<IWebHelper>();
        public IWorkContext WorkContext => MockRepository.GenerateMock<IWorkContext>();
        public IEventPublisher EventPublisher { get; set; }
        public ILocalizationService Localization => MockRepository.GenerateMock<ILocalizationService>();
        public ICustomerActivityService CustomerActivity => MockRepository.GenerateMock<ICustomerActivityService>();
        public IMediaService MediaService => MockRepository.GenerateMock<IMediaService>();
        public INotifier Notifier => MockRepository.GenerateMock<INotifier>();
        public IPermissionService Permissions => MockRepository.GenerateMock<IPermissionService>();
        public ISettingService Settings => MockRepository.GenerateMock<ISettingService>();
        public IStoreService StoreService => MockRepository.GenerateMock<IStoreService>();
        public IDateTimeHelper DateTimeHelper => MockRepository.GenerateMock<IDateTimeHelper>();
        public IDisplayControl DisplayControl => MockRepository.GenerateMock<IDisplayControl>();
        public IChronometer Chronometer => NullChronometer.Instance;
        public IMessageFactory MessageFactory => MockRepository.GenerateMock<IMessageFactory>();

        public IDbContext DbContext
        {
            get
            {
                if (_dbContext == null)
                {
                    var ctx = MockRepository.GenerateMock<IDbContext>();
                    ctx.Stub(x => x.SaveChangesAsync()).Return(Task.FromResult<int>(0));
                    ctx.Stub(x => x.GetModifiedProperties(Arg<BaseEntity>.Is.Anything)).Return(new Dictionary<string, object>());
                    ctx.Stub(x => x.BeginTransaction(Arg<IsolationLevel>.Is.Anything)).Return(MockRepository.GenerateMock<ITransaction>());

                    _dbContext = ctx;
                }

                return _dbContext;
            }
        }
    }
}
