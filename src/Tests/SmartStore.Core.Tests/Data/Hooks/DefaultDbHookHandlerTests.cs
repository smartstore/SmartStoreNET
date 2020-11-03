using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SmartStore.Core.Data;
using SmartStore.Core.Data.Hooks;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Security;

namespace SmartStore.Core.Tests.Data.Hooks
{
    [TestFixture]
    public class DefaultDbHookHandlerTests
    {
        private Lazy<IDbSaveHook, HookMetadata>[] _hooks;
        private IDbHookHandler _handler;

        [SetUp]
        public virtual void SetUp()
        {
            _hooks = new[]
            {
                CreateHook<Hook_Acl_Deleted, IAclSupported>(),
                CreateHook<Hook_Auditable_Inserting_Updating_Important, IAuditable>(),
                CreateHook<Hook_Category_Pre, BaseEntity>(),
                CreateHook<Hook_Entity_Inserted_Deleted_Update, BaseEntity>(),
                CreateHook<Hook_LocalizedEntity_Deleted, ILocalizedEntity>(),
                CreateHook<Hook_Product_Post, BaseEntity>(),
                CreateHook<Hook_SoftDeletable_Updating_ChangingState, ISoftDeletable>()
            };

            _handler = new DefaultDbHookHandler(_hooks);
        }

        private ICollection<Type> GetExpectedSaveHooks(IEnumerable<IHookedEntity> entries, bool isPost, bool importantOnly)
        {
            var hset = new HashSet<Type>();

            foreach (var hook in _hooks)
            {
                foreach (var e in entries)
                {
                    if (ShouldHandle(hook.Metadata.ImplType, e, isPost, importantOnly))
                    {
                        hset.Add(hook.Metadata.ImplType);
                    }
                }
            }

            return hset;
        }

        private bool ShouldHandle(Type hookType, IHookedEntity entry, bool isPost, bool importantOnly)
        {
            bool result = false;

            if (hookType == typeof(Hook_Acl_Deleted))
            {
                result = isPost && !importantOnly && typeof(IAclSupported).IsAssignableFrom(entry.EntityType) && entry.State == EntityState.Deleted;
            }
            else if (hookType == typeof(Hook_Auditable_Inserting_Updating_Important))
            {
                result = !isPost && typeof(IAuditable).IsAssignableFrom(entry.EntityType) && (entry.State == EntityState.Added || entry.State == EntityState.Modified);
            }
            else if (hookType == typeof(Hook_Category_Pre))
            {
                result = !isPost && !importantOnly && typeof(Category).IsAssignableFrom(entry.EntityType);
            }
            else if (hookType == typeof(Hook_Entity_Inserted_Deleted_Update))
            {
                result =
                    (isPost && !importantOnly && (entry.State == EntityState.Added || entry.State == EntityState.Deleted)) ||
                    (!isPost && !importantOnly && (entry.State == EntityState.Modified));
            }
            else if (hookType == typeof(Hook_LocalizedEntity_Deleted))
            {
                result = isPost && !importantOnly && typeof(ILocalizedEntity).IsAssignableFrom(entry.EntityType) && entry.State == EntityState.Deleted;
            }
            else if (hookType == typeof(Hook_Product_Post))
            {
                result = isPost && !importantOnly && typeof(Product).IsAssignableFrom(entry.EntityType);
            }
            else if (hookType == typeof(Hook_SoftDeletable_Updating_ChangingState))
            {
                result = !isPost && !importantOnly && typeof(ISoftDeletable).IsAssignableFrom(entry.EntityType) && entry.State == EntityState.Modified;
            }

            return result;
        }

        [Test]
        public void Can_handle_voidness()
        {
            var entries = new[]
            {
                CreateEntry<Product>(EntityState.Modified), // > Hook_Entity_Inserted_Deleted_Update, Hook_Product_Post
				CreateEntry<GenericAttribute>(EntityState.Deleted), // Hook_Entity_Inserted_Deleted_Update
				CreateEntry<Currency>(EntityState.Deleted), // Hook_Acl_Deleted, Hook_Entity_Inserted_Deleted_Update
				CreateEntry<Category>(EntityState.Added) // Hook_Entity_Inserted_Deleted_Update
			};

            var processedHooks = _handler.TriggerPostSaveHooks(entries, false).ToArray();
            var expected = GetExpectedSaveHooks(entries, true, false);

            Assert.AreEqual(expected.Count, processedHooks.Count());
            Assert.IsTrue(processedHooks.All(x => expected.Contains(x.GetType())));
        }

        [Test]
        public void Can_handle_importance()
        {
            var entries = new[]
            {
                CreateEntry<Product>(EntityState.Modified), // > Important
				CreateEntry<GenericAttribute>(EntityState.Deleted),
                CreateEntry<Currency>(EntityState.Modified),
                CreateEntry<Category>(EntityState.Added) // > Important
			};

            bool anyStateChanged;
            var processedHooks = _handler.TriggerPreSaveHooks(entries, true, out anyStateChanged).ToArray();
            var expected = GetExpectedSaveHooks(entries, false, true);

            Assert.AreEqual(expected.Count, processedHooks.Count());
            Assert.AreEqual(false, anyStateChanged);
            Assert.IsTrue(processedHooks.All(x => expected.Contains(x.GetType())));
        }


        #region Utils

        private static IHookedEntity CreateEntry<T>(EntityState state) where T : BaseEntity, new()
        {
            return new HookedEntityMock(new T(), state);
        }

        private static Lazy<IDbSaveHook, HookMetadata> CreateHook<THook, TEntity>() where THook : IDbSaveHook, new() where TEntity : class
        {
            var hook = new Lazy<IDbSaveHook, HookMetadata>(() => new THook(), new HookMetadata
            {
                HookedType = typeof(TEntity),
                DbContextType = typeof(IDbContext),
                ImplType = typeof(THook),
                Important = typeof(THook).GetAttribute<ImportantAttribute>(false) != null
            });

            return hook;
        }

        #endregion
    }
}
