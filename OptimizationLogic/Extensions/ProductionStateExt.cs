using OptimizationLogic.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizationLogic.Extensions
{
    public static class ProductionStateExt
    {
        public static T Copy<T>(this T obj) where T : class, ICloneable
        {
            return obj.Clone() as T;
        }

        public static IEnumerable<double> Softmax(this IEnumerable<double> values)
        {
            var exp = values.Select(Math.Exp);
            double sum = exp.Sum();
            return exp.Select(t => t / sum).ToList();
        }

        public static IEnumerable<double> CumulativeSum(this IEnumerable<double> sequence)
        {
            double sum = 0;
            foreach (var item in sequence)
            {
                sum += item;
                yield return sum;
            }
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rng)
        {
            T[] elements = source.ToArray();
            for (int i = elements.Length - 1; i >= 0; i--)
            {
                int swapIndex = rng.Next(i + 1);
                yield return elements[swapIndex];
                elements[swapIndex] = elements[i];
            }
        }
    }
}
