using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartStore.ComponentModel
{
    public interface IMapper<in TFrom, in TTo>
        where TFrom : class
        where TTo : class
    {
        /// <summary>
        /// Maps the specified source object into the destination object.
        /// </summary>
        /// <param name="from">The source object to map from.</param>
        /// <param name="to">The destination object to map to.</param>
        void Map(TFrom from, TTo to);
    }

    public static class IMapperExtensions
    {
        /// <summary>
        /// Maps the specified source object to a new object with a type of <typeparamref name="TTo"/>.
        /// </summary>
        /// <typeparam name="TFrom">The type of the source object.</typeparam>
        /// <typeparam name="TTo">The type of the destination object.</typeparam>
        /// <param name="mapper">The mapper.</param>
        /// <param name="from">The source object.</param>
        /// <returns>The mapped object of type <typeparamref name="TTo"/>.</returns>
        public static TTo Map<TFrom, TTo>(this IMapper<TFrom, TTo> mapper, TFrom from)
            where TFrom : class
            where TTo : class, new()
        {
            Guard.NotNull(mapper, nameof(mapper));
            Guard.NotNull(from, nameof(from));

            var to = Activator.CreateInstance<TTo>();
            mapper.Map(from, to);
            return to;
        }

        /// <summary>
        /// Maps a collection of <typeparamref name="TFrom"/> into an array of <typeparamref name="TTo"/>.
        /// </summary>
        /// <typeparam name="TFrom">The type of the source objects.</typeparam>
        /// <typeparam name="TTo">The type of the destination objects.</typeparam>
        /// <param name="mapper">The mapper.</param>
        /// <param name="from">The source collection.</param>
        /// <returns>An array of <typeparamref name="TTo"/>.</returns>
        public static TTo[] MapArray<TFrom, TTo>(this IMapper<TFrom, TTo> mapper, IEnumerable<TFrom> from)
            where TFrom : class
            where TTo : class, new()
        {
            Guard.NotNull(mapper, nameof(mapper));
            Guard.NotNull(from, nameof(from));

            return from.Select(x => mapper.Map<TFrom, TTo>(x)).ToArray();
        }

        /// <summary>
        /// Maps a collection of <typeparamref name="TFrom"/> into a list of <typeparamref name="TTo"/>.
        /// </summary>
        /// <typeparam name="TFrom">The type of the source objects.</typeparam>
        /// <typeparam name="TTo">The type of the destination objects.</typeparam>
        /// <param name="mapper">The mapper.</param>
        /// <param name="from">The source collection.</param>
        /// <returns>A list of <typeparamref name="TTo"/>.</returns>
        public static List<TTo> MapList<TFrom, TTo>(this IMapper<TFrom, TTo> mapper, IEnumerable<TFrom> from)
            where TFrom : class
            where TTo : class, new()
        {
            Guard.NotNull(mapper, nameof(mapper));
            Guard.NotNull(from, nameof(from));

            return from.Select(x => mapper.Map<TFrom, TTo>(x)).ToList();
        }

        /// <summary>
        /// Maps a collection of <typeparamref name="TFrom"/> into a list of <typeparamref name="TTo"/>.
        /// </summary>
        /// <typeparam name="TFrom">The type of the source objects.</typeparam>
        /// <typeparam name="TTo">The type of the destination objects.</typeparam>
        /// <param name="mapper">The mapper.</param>
        /// <param name="from">The source collection.</param>
        /// <param name="to">The destination collection.</param>
        /// <returns>A list of <typeparamref name="TTo"/>.</returns>
        public static void MapCollection<TFrom, TTo>(this IMapper<TFrom, TTo> mapper, IEnumerable<TFrom> from, ICollection<TTo> to)
            where TFrom : class
            where TTo : class, new()
        {
            Guard.NotNull(mapper, nameof(mapper));
            Guard.NotNull(from, nameof(from));
            Guard.NotNull(to, nameof(to));

            to.Clear();
            to.AddRange(from.Select(x => mapper.Map<TFrom, TTo>(x)));
        }
    }
}
