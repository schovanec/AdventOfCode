using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace Day10
{
    class Program
    {
        const int MaxDiff = 3;

        static void Main(string[] args)
        {
            var file = args.DefaultIfEmpty("input.txt").First();
            var adapters = File.ReadLines(file)
                               .Select(int.Parse)
                               .OrderBy(x => x);

            var builder = ImmutableArray.CreateBuilder<int>();
            builder.Add(0);
            builder.AddRange(adapters);
            builder.Add(builder.Last() + MaxDiff);

            var chain = builder.ToImmutable();

            Part1(chain);
            Part2(chain);
        }

        static void Part1(ImmutableArray<int> chain)
        {
            var differences = chain.Zip(chain.Skip(1), (a, b) => b - a)
                                   .GroupBy(x => x, (x, v) => (key: x, count: v.Count()))
                                   .ToArray();

            var product = differences.Select(x => x.count).Aggregate((a, b) => a * b);
            Console.WriteLine($"Part 1 Result = {product}");
        }

        static void Part2(ImmutableArray<int> chain)
        {
            var count = CountAdapterArrangements(chain.AsSpan());
            Console.WriteLine($"Part 2 Result = {count}");
        } 

        static long CountAdapterArrangements(ReadOnlySpan<int> adapters)
            => CountAdapterArrangements(adapters, new Dictionary<(int, int), long>());

        static long CountAdapterArrangements(ReadOnlySpan<int> adapters, Dictionary<(int, int), long> cache)
        {
            if (adapters.Length <= 2)
                return 1;

            var key = (adapters[0], adapters.Length);
            if (!cache.TryGetValue(key, out var result))
            {
                result = 0L;

                for (var i = 1; i < adapters.Length && adapters[i] <= adapters[0] + MaxDiff; ++i)
                    result += CountAdapterArrangements(adapters.Slice(i), cache);

                cache.Add(key, result);
            }
            return result;
        }
    }
}
