using System;
using System.Collections.Generic;

namespace com.antlersoft.HostedTools.Utility.Linq
{
    /// <summary>
    /// Extension methods for FindBest
    /// </summary>
    public static class BestFinder
    {
        /// <summary>
        /// Find the best element in a collection according to some criterion
        /// </summary>
        /// <typeparam name="T">Type contained in the collection</typeparam>
        /// <param name="source">Enumerable over collection</param>
        /// <param name="isBetter">Function(a,b) that will return true iff a is "better" than b</param>
        /// <returns>A pair index of the best value in the collection and the value itself; if the collection is empty
        /// the index is -1</returns>
        public static KeyValuePair<int,T> FindBest<T>(this IEnumerable<T> source, Func<T,T,bool> isBetter)
        {
            bool isSet = false;
            T result = default(T);
            int bestIndex = -1;
            int i = 0;
            foreach (T v in source)
            {
                if (! isSet)
                {
                    isSet = true;
                    result = v;
                    bestIndex = i;
                }
                else
                {
                    if (isBetter(v, result))
                    {
                        result = v;
                        bestIndex = i;
                    }
                }
                i++;
            }
            return new KeyValuePair<int, T>(bestIndex, result);
        }
    }
}
