using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using SmartStore.Core;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Localization;

namespace SmartStore.Services.Customers
{
	/// <summary>
	/// TODO
	/// </summary>
	public class CustomerAnonymizedEvent
	{
		private readonly IGdprTool _gdprTool;

		public CustomerAnonymizedEvent(Customer customer, IGdprTool gdprTool)
		{
			Guard.NotNull(customer, nameof(customer));

			Customer = customer;
			_gdprTool = gdprTool;
		}

		public Customer Customer { get; private set; }

		/// <summary>
		/// TODO
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="entity"></param>
		/// <param name="expression"></param>
		/// <param name="type"></param>
		public void AnonymizeData<TEntity>(TEntity entity, Expression<Func<TEntity, object>> expression, IdentifierDataType type, Language language = null) 
			where TEntity : BaseEntity
		{
			_gdprTool.AnonymizeData(entity, expression, type, language);
		}
	}
}
