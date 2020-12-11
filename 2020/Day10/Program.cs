using System;
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
        {
            const int size = MaxDiff + 1;
            Span<long> orders = stackalloc long[size];
            orders[(adapters.Length - 1) % size] = 1;
            for (var i = adapters.Length - 2; i >= 0; --i)
            {
                orders[i % 4] = 0;
                for (var j = i + 1; j < adapters.Length && j <= i + MaxDiff; ++j)
                {
                    if (adapters[j] - adapters[i] <= MaxDiff)
                        orders[i % 4] += orders[j % size];
                }
            }

            return orders[0];
        }
    }
}
