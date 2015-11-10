using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace SmartStore.Utilities.Reflection
{
	public class FastActivator
	{
		private Action<object[], object> _invoker;

		public FastActivator(ConstructorInfo constructorInfo)
		{
			Guard.ArgumentNotNull(() => constructorInfo);

			Constructor = constructorInfo;
			Invoker = MakeFastInvoker(constructorInfo);
		}

		/// <summary>
		/// Gets the backing <see cref="Constructor"/>.
		/// </summary>
		public ConstructorInfo Constructor { get; }

		/// <summary>
		/// Gets the constructor invoker.
		/// </summary>
		public Func<object[], object> Invoker { get; }

		/// <summary>
		/// Creates an instance of the type using the specified parameters.
		/// </summary>
		/// <returns>A reference to the newly created object.</returns>
		public object Activate(params object[] parameters)
		{
			return Invoker(parameters);
		}

		/// <summary>
		/// Creates a single fast constructor invoker. The result is not cached.
		/// </summary>
		/// <param name="constructorInfo">constructorInfo to create invoker for.</param>
		/// <returns>a fast invoker.</returns>
		public static Func<object[], object> MakeFastInvoker(ConstructorInfo constructorInfo)
		{
			// parameters to execute
			var parametersParameter = Expression.Parameter(typeof(object[]), "parameters");

			// build parameter list
			var parameterExpressions = new List<Expression>();
			var paramInfos = constructorInfo.GetParameters();
			for (int i = 0; i < paramInfos.Length; i++)
			{
				var valueObj = Expression.ArrayIndex(parametersParameter, Expression.Constant(i));
				var valueCast = Expression.Convert(valueObj, paramInfos[i].ParameterType);

				parameterExpressions.Add(valueCast);
			}

			// new T((T0)parameters[0], (T1)parameters[1], ...)
			var instanceCreate = Expression.New(constructorInfo, parameterExpressions);

			// (object)new T((T0)parameters[0], (T1)parameters[1], ...)
			var instanceCreateCast = Expression.Convert(instanceCreate, typeof(object));

			var lambda = Expression.Lambda<Func<object[], object>>(instanceCreateCast, parametersParameter);

			return lambda.Compile();
		}
	}
}
