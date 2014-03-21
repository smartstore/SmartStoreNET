using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace SmartStore.Data.Setup
{
	public interface IDataSeeder<TContext> where TContext : DbContext
	{
		/// <summary>
		/// Seeds data
		/// </summary>
		void Seed(TContext context);

		/// <summary>
		/// Gets a value indicating whether migration should be completely rolled back
		/// when an error occurs during migration seeding.
		/// </summary>
		bool RollbackOnFailure { get; }
	}
}
