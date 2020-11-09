using System;
using System.Linq.Expressions;

namespace SmartStore.Linq
{

    /// <summary>
    /// Instructs the unit of work to eager load entities.
    /// </summary>
    public interface IPathExpander
    {

        /// <summary>
        /// Instructs the unit of work to eager load entities that may be in the type's association path.
        /// </summary>
        /// <param name="path">The path expression of the child entities to eager load.</param>
        /// <typeparam name="T">The root type from which to resolve a path.</typeparam>
        /// <example>
        ///     uow.Expand{Customer}(c => c.Orders);
        ///         - should resolve to -
        ///     "Orders"
        /// </example>
        void Expand<T>(Expression<Func<T, object>> path);

        /// <summary>
        /// Instructs the unit of work to eager load entities that may be in the type's association path.
        /// </summary>
        /// <param name="path">The path expression of the child entities to eager load.</param>
        /// <typeparam name="T">The root type from which to resolve a path.</typeparam>
        /// <typeparam name="TTarget">
        ///     Generally same as <c>T</c>. If different, it is assumed that this is a type parameter of a generic enumerable.
        /// </typeparam>
        /// <example>
        /// uow.Expand{Customer, Order}(o => o.OrderLines);
        ///     - should resolve to -
        /// "Orders.OrderLines" (assuming that 'Orders' is the collection property for typeparam <c>Order</c>)
        /// </example>
        void Expand<T, TTarget>(Expression<Func<TTarget, object>> path);

        /// <summary>
        /// Instructs the unit of work to eager load entities that may be in the type's association path.
        /// </summary>
        /// <typeparam name="T">The root type from which to resolve a path.</typeparam>
        /// <param name="path">The path of the child entities to eager load.</param>
        void Expand<T>(string path);

    }

}
