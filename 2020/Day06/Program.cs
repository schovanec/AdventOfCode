using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace Day06
{
    class Program
    {
        static void Main(string[] args)
        {
            var file = args.DefaultIfEmpty("input.txt").First();

            var input = ReadInput(file);

            var unionSum = input.Select(g => g.SelectMany(x => x).Distinct().Count())
                                .Sum();
            Console.WriteLine($"Part 1 Result: {unionSum}");

            var intersectionSum = input.Select(g => g.Aggregate((a, b) => a.Intersect(b)).Count)
                                       .Sum();
            Console.WriteLine($"Part 2 Result: {intersectionSum}");
        }

        private static IEnumerable<ImmutableList<ImmutableHashSet<char>>> ReadInput(string file)
            => ReadInput(File.ReadLines(file));

        private static IEnumerable<ImmutableList<ImmutableHashSet<char>>> ReadInput(IEnumerable<string> lines)
        {
            var builder = ImmutableList.CreateBuilder<ImmutableHashSet<char>>();

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    yield return builder.ToImmutable();
                    builder.Clear();
                }
                else
                {
                    builder.Add(ImmutableHashSet.CreateRange(line));
                }
            }

            if (builder.Count > 0)
                yield return builder.ToImmutable();
        }
    }
}
