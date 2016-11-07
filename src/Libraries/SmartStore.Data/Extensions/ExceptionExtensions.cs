using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;

namespace SmartStore
{
	public static class ExceptionExtensions
	{
		/// <summary>
		/// Checks whether the inner exception indicates uniqueness violation
		/// (is 2627 = Unique constraint error, OR is 547 = Constraint check violation, OR is 2601 = Duplicated key row error)
		/// </summary>
		/// <param name="exception">The exception wrapper</param>
		/// <returns></returns>
		public static bool IsUniquenessViolationException(this DbUpdateException exception)
		{
			var sqlException = exception?.InnerException as SqlException;

			if (sqlException == null)
				sqlException = exception?.InnerException?.InnerException as SqlException;

			if (sqlException == null)
				return false;

			switch (sqlException.Number)
			{
				case 2627:  // Unique constraint error
				case 547:   // Constraint check violation
				case 2601:  // Duplicated key row error
					return true;
				default:
					return false;
			}
		}
	}
}
