using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace SmartStore.Data.Setup
{
	/// <summary>
	/// Provides data seeding capabities to the EF Migration classes
	/// </summary>
	public interface IMigrationSeeder<TContext> where TContext : DbContext
	{
		/// <summary>
		/// Seeds this migration
		/// </summary>
		void Seed(TContext context);
	}
}
