using System;
using System.Collections.Generic;
using System.Linq;

namespace ServiceNow.Api.Test.Extensions
{
	internal static class LinqExtensions
	{
		internal static IEnumerable<TSource> DistinctBy<TSource, TKey>
			(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
		{
			var seenKeys = new HashSet<TKey>();
			foreach (var element in source)
			{
				if (seenKeys.Add(keySelector(element)))
				{
					yield return element;
				}
			}
		}

		internal static bool AreDistinctBy<TSource, TKey>
			(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
		{
			var enumerable = source as TSource[] ?? source.ToArray();
			return enumerable.DistinctBy(keySelector).Count() == enumerable.Length;
		}
	}
}