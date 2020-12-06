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

            var part1Result = input.Select(g => g.Aggregate((a, b) => a.Union(b)))
                                   .Sum(x => x.Count);
            Console.WriteLine($"Part 1 Result: {part1Result}");

            var part2Result = input.Select(g => g.Aggregate((a, b) => a.Intersect(b)))
                                   .Sum(x => x.Count);
            Console.WriteLine($"Part 2 Result: {part2Result}");
        }

        private static IEnumerable<ImmutableList<ImmutableHashSet<char>>> ReadInput(string file)
        {
            var builder = ImmutableList.CreateBuilder<ImmutableHashSet<char>>();

            foreach (var answer in File.ReadLines(file).Select(ImmutableHashSet.CreateRange))
            {
                if (answer.IsEmpty)
                {
                    yield return builder.ToImmutable();
                    builder.Clear();
                }
                else
                {
                    builder.Add(answer);
                }
            }

            if (builder.Count > 0)
                yield return builder.ToImmutable();
        }
    }
}
