using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace Day10
{
    class Program
    {
        static void Main(string[] args)
        {
            var file = args.DefaultIfEmpty("input.txt").First();
            var adapters = File.ReadLines(file)
                               .Select(long.Parse)
                               .OrderBy(x => x)
                               .ToArray();

            Part1(adapters);
            Part2(adapters);
        }

        static void Part1(long[] adapters)
        {
            var builtInAdapter = adapters.Last() + 3;

            var chain = ImmutableList.Create(0L)
                                     .Concat(adapters)
                                     .Append(builtInAdapter);

            var differences = chain.Zip(chain.Skip(1), (a, b) => b - a)
                                   .GroupBy(x => x, (x, v) => (key: x, count: v.Count()))
                                   .ToArray();

            var product = differences.Select(x => x.count).Aggregate((a, b) => a * b);
            Console.WriteLine($"Differences: {string.Join(", ", differences.Select(x => $"{x.key}={x.count}"))}");
            Console.WriteLine($"Part 1 Result = {product}");
        }

        static void Part2(long[] adapters)
        {
            var builtInAdapter = adapters.Last() + 3;
            var count = CountAdapterArrangements(0, adapters, builtInAdapter);
            Console.WriteLine($"Part 2 Result = {count}");
        } 

        static long CountAdapterArrangements(long initial, ReadOnlySpan<long> adapters, long final)
            => CountAdapterArrangements(initial, adapters, final, new Dictionary<(long, int), long>());

        static long CountAdapterArrangements(long initial, ReadOnlySpan<long> adapters, long final, Dictionary<(long, int), long> cache)
        {
            var key = (initial, adapters.Length);
            if (cache.TryGetValue(key, out var result))
                return result;

            result = 0L;

            for (var i = 0; i < adapters.Length && adapters[i] <= initial + 3; ++i)
                result += CountAdapterArrangements(adapters[i], adapters.Slice(i + 1), final, cache);

            if (final <= initial + 3)
                ++result;

            cache.Add(key, result);

            return result;
        }
    }
}
