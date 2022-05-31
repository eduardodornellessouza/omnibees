using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OB.BL.Operations.Helper;

namespace OB.BL.Operations.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<TSource> Except<TSource>(this IEnumerable<TSource> first,
            IEnumerable<TSource> second, Func<TSource, TSource, bool> comparer)
        {
            return first.Except(second, new LambdaComparer<TSource>(comparer));
        }

        public static IEnumerable<TSource> Intersect<TSource>(this IEnumerable<TSource> first,
            IEnumerable<TSource> second, Func<TSource, TSource, bool> comparer)
        {
            return first.Intersect(second, new LambdaComparer<TSource>(comparer));
        }

        public static IEnumerable<TSource> ForEach<TSource>(this IEnumerable<TSource> enumerable, Action<TSource> action)
        {
            foreach (var item in enumerable)
            {
                action(item);
                yield return item;
            }
        }


        /// <summary>
        /// Indicate if an Enumerable is null or empty.
        /// </summary>
        /// <typeparam name="T">Type of the enumerable</typeparam>
        /// <param name="e">Enumerable.</param>
        /// <returns>True if it is null or empty, or false otherwise.</returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> e)
        {
            return e?.Any() != true;
        }

        public static bool IsNullOrEmpty<T>(this ICollection<T> e)
        {
            return e == null || e.Count <= 0;
        }

        /// <summary>
        /// Check for duplicated items (only for items of base types).
        /// </summary>
        /// <typeparam name="T">Type of the enumerable</typeparam>
        /// <param name="e">Enumerable.</param>
        /// <returns>The list of duplicated items.</returns>
        public static IEnumerable<T> GetDuplicateds<T>(this IEnumerable<T> e)
        {
            if (e == null)
            {
                return null;
            }

            e = e.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key);

            return e;
        }

        /// <summary>
        /// Get duplicated items (for base data types) and the indexz where the repeatition occours.
        /// </summary>
        /// <typeparam name="T">Type,</typeparam>
        /// <param name="e">IEnumerable.</param>
        /// <returns>List of pairs with (repeated element, index).</returns>
        public static List<KeyValuePair<T, int>> GetDuplicatedsWithIndex<T>(this IEnumerable<T> e) where T : struct
        {
            var verificationHs = new HashSet<T>();
            var result = new List<KeyValuePair<T, int>>();

            if (e == null)
            {
                return result;
            }

            var i = 0;
            foreach (var item in e)
            {
                if (!verificationHs.Add(item))
                {
                    result.Add(new KeyValuePair<T, int>(item, i));
                }

                i++;
            }

            return result;
        }
    }
}
