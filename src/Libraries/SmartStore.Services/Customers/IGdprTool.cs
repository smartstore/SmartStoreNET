using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core.Domain.Customers;

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
		/// <param name="deleteContent"></param>
		/// <remarks>This method fulfills the "GDPR Right to be forgotten" requirement.</remarks>
		void AnonymizeCustomer(Customer customer, bool deleteContent);
	}
}
