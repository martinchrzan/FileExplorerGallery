using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FileExplorerGallery.Extensions
{
    public static class SortingExtensions
    {
        private static Regex regex = new Regex(@"\d+", RegexOptions.Compiled);

        public static IOrderedEnumerable<T> OrderByNaturalAscending<T>(this IEnumerable<T> items, Func<T, string> selector, StringComparer stringComparer = null)
        {
            int maxDigits = items
                          .SelectMany(i => regex.Matches(selector(i)).Cast<Match>().Select(digitChunk => (int?)digitChunk.Value.Length))
                          .Max() ?? 0;

            return items.OrderBy(i => regex.Replace(selector(i), match => match.Value.PadLeft(maxDigits, '0')), stringComparer ?? StringComparer.CurrentCulture);
        }

        public static IOrderedEnumerable<T> OrderByNaturalDescending<T>(this IEnumerable<T> items, Func<T, string> selector, StringComparer stringComparer = null)
        {
            int maxDigits = items
                          .SelectMany(i => regex.Matches(selector(i)).Cast<Match>().Select(digitChunk => (int?)digitChunk.Value.Length))
                          .Max() ?? 0;

            return items.OrderByDescending(i => regex.Replace(selector(i), match => match.Value.PadLeft(maxDigits, '0')), stringComparer ?? StringComparer.CurrentCulture);
        }
    }
}
