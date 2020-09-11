using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using SmartStore.Core;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Localization;

namespace SmartStore.Services.Customers
{
    public interface IGdprTool
    {
        /// <summary>
        /// Exports all data stored for a customer into a dictionary. Exported data contains all
        /// personal data, addresses, order history, reviews, forum posts, private messages etc.
        /// </summary>
        /// <param name="customer">The customer to export data for.</param>
        /// <returns>The exported data</returns>
        /// <remarks>This method fulfills the "GDPR Data Portability" requirement.</remarks>
        IDictionary<string, object> ExportCustomer(Customer customer);

        /// <summary>
        /// Anonymizes a customer's (personal) data.
        /// </summary>
        /// <param name="customer">The customer to anonymize.</param>
        /// <param name="pseudomyzeContent"></param>
        /// <remarks>This method fulfills the "GDPR Right to be forgotten" requirement.</remarks>
        void AnonymizeCustomer(Customer customer, bool pseudomyzeContent);

        /// <summary>
        /// TODO
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="expression"></param>
        /// <param name="type"></param>
        /// <param name="language"></param>
        void AnonymizeData<TEntity>(TEntity entity, Expression<Func<TEntity, object>> expression, IdentifierDataType type, Language language = null) where TEntity : BaseEntity;
    }
}
