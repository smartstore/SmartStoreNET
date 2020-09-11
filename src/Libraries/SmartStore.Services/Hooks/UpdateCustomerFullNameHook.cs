using System.Collections.Generic;
using System.Linq;
using Autofac;
using SmartStore.Core.Data;
using SmartStore.Core.Data.Hooks;
using SmartStore.Core.Domain.Customers;

namespace SmartStore.Services.Hooks
{
    [Important]
    public class UpdateCustomerFullNameHook : DbSaveHook<Customer>
    {
        private static readonly HashSet<string> _candidateProps = new HashSet<string>(new string[]
        {
            nameof(Customer.Title),
            nameof(Customer.Salutation),
            nameof(Customer.FirstName),
            nameof(Customer.LastName)
        });

        private readonly IComponentContext _ctx;

        public UpdateCustomerFullNameHook(IComponentContext ctx)
        {
            _ctx = ctx;
        }

        protected override void OnUpdating(Customer entity, IHookedEntity entry)
        {
            UpdateFullName(entity, entry);
        }

        protected override void OnInserting(Customer entity, IHookedEntity entry)
        {
            UpdateFullName(entity, entry);
        }

        private void UpdateFullName(Customer entity, IHookedEntity entry)
        {
            var shouldUpdate = entity.IsTransientRecord();

            if (!shouldUpdate)
            {
                shouldUpdate = entity.FullName.IsEmpty() && (entity.FirstName.HasValue() || entity.LastName.HasValue());
            }

            if (!shouldUpdate)
            {
                var modProps = _ctx.Resolve<IDbContext>().GetModifiedProperties(entity);
                shouldUpdate = _candidateProps.Any(x => modProps.ContainsKey(x));
            }

            if (shouldUpdate)
            {
                var parts = new[]
                {
                    entity.Salutation,
                    entity.Title,
                    entity.FirstName,
                    entity.LastName
                };

                entity.FullName = string.Join(" ", parts.Where(x => x.HasValue())).NullEmpty();
            }
        }
    }
}
