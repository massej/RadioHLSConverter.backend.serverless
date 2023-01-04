/*
 * EnumerableHelpers.cs
 * This helper add extension method to IList<T>.
 * Date : 2023-01-03.
 * By : Jonathan Massé
 */


// Includes.
using System.Collections.Generic;


namespace RadioHLSConverter.backend.serverless.Helpers
{
    public static class EnumerableHelpers
    {
        public static void AddRange<T>(this IList<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                collection.Add(item);
            }
        }
    }
}