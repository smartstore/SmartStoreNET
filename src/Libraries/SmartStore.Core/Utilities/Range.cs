using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartStore.Utilities
{

    /// <summary>
    /// Helper class with shortcut methods for managing enumerables.
    /// Useful for inlining object generation in tests
    /// </summary>
    public static class Range
    {
        /// <summary> Returns empty enumerator </summary>
        /// <typeparam name="T">type of the item to enumerate</typeparam>
        /// <returns>singleton instance of the empty enumerator</returns>
        public static IEnumerable<T> Empty<T>()
        {
            return Enumerable.Empty<T>();
        }

        /// <summary>
        /// returns enumeration from 0 with <paramref name="count"/> numbers
        /// </summary>
        /// <param name="count">Number of items to create</param>
        /// <returns>enumerable</returns>
        public static IEnumerable<int> Create(int count)
        {
            return Enumerable.Range(0, count);
        }


        /// <summary>
        /// Creates sequence of the integral numbers within the specified range
        /// </summary>
        /// <param name="start">The value of the first integer in sequence.</param>
        /// <param name="count">The number of values in the sequence.</param>
        /// <returns>sequence of the integral numbers within the specified range</returns>
        public static IEnumerable<int> Create(int start, int count)
        {
            return Enumerable.Range(start, count);
        }

        /// <summary>
        /// Creates sequence that consists of a repeated value.
        /// </summary>
        /// <typeparam name="TResult">The type of the value to repeat.</typeparam>
        /// <param name="item">The value to repeat.</param>
        /// <param name="count">The number of times to repeat.</param>
        /// <returns>sequence that consists of a repeated value</returns>
        public static IEnumerable<TResult> Repeat<TResult>(TResult item, int count)
        {
            return Enumerable.Repeat(item, count);
        }


        /// <summary>
        /// Creates the generator to iterate from 1 to <see cref="int.MaxValue"/>.
        /// </summary>
        /// <typeparam name="T">type of the item to generate</typeparam>
        /// <param name="generator">The generator.</param>
        /// <returns>new enumerator</returns>
        public static IEnumerable<T> Create<T>(Func<int, T> generator)
        {
            for (int i = 0; i < int.MaxValue; i++)
            {
                yield return generator(i);
            }
            throw new InvalidOperationException("Generator has reached the end");
        }

        /// <summary>
        /// Creates the enumerable using the provided generator.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="count">The count.</param>
        /// <param name="generator">The generator.</param>
        /// <returns>enumerable instance</returns>
        public static IEnumerable<T> Create<T>(int count, Func<int, T> generator)
        {
            for (int i = 0; i < count; i++)
            {
                yield return generator(i);
            }
        }

        /// <summary>
        /// Creates the enumerable using the provided generator.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="count">The count.</param>
        /// <param name="generator">The generator.</param>
        /// <returns>enumerable instance</returns>
        public static IEnumerable<T> Create<T>(int count, Func<T> generator)
        {
            for (int i = 0; i < count; i++)
            {
                yield return generator();
            }
        }

        /// <summary>
        /// Creates the array populated with the provided generator
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="count">The count.</param>
        /// <param name="generator">The generator.</param>
        /// <returns>array</returns>
        public static TValue[] Array<TValue>(int count, Func<int, TValue> generator)
        {
            if (generator == null)
                throw new ArgumentNullException("generator");

            var array = new TValue[count];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = generator(i);
            }

            return array;
        }

        /// <summary>
        /// Creates the array of integers
        /// </summary>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        public static int[] Array(int count)
        {
            var array = new int[count];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = i;
            }
            return array;
        }
    }

}
