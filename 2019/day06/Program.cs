using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace day06
{
    class Program
    {
        static void Main(string[] args)
        {
            var input = args.Any()
                ? File.ReadLines(args.First()).Where(x => !string.IsNullOrEmpty(x))
                : TestInput;

            var graph = BuildEdgeMap(ParseOrbits(input));

            var path = FindShortestPath("YOU", "SAN", graph);

            if (path != null)
            {
                foreach (var item in path)
                    Console.WriteLine($"{item.from} to {item.to}");

                Console.WriteLine();
                Console.WriteLine($"Length = {path.Count}");
            }
            else
            {
                Console.WriteLine("Not found!");
            }

            // var totalOrbits = CountTotalOrbits(orbits);
            // Console.WriteLine(totalOrbits);
        }

        private static ImmutableList<(string from, string to)> FindShortestPath(string from, string to, ILookup<string, string> adjacent)
        {
            var parent = new Dictionary<string, string> { [from] = null };
            var queue = new Queue<string>();
            queue.Enqueue(from);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (current == to)
                {
                    var result = ImmutableList.CreateBuilder<(string, string)>();
                    var next = parent[current];
                    if (next != from)
                    {
                        var prev = next;
                        next = parent[prev];
                        while (next != from)
                        {
                            result.Insert(0, (next, prev));
                            prev = next;
                            next = parent[prev];
                        }
                    }

                    return result.ToImmutable();
                }

                foreach (var item in adjacent[current])
                {
                    if (!parent.ContainsKey(item))
                    {
                        parent[item] = current;
                        queue.Enqueue(item);
                    }
                }
            }

            return null;
        }

        private static ILookup<string, string> BuildEdgeMap(IEnumerable<(string, string)> edges)
            => MakeBidirectionalEdges(edges).Distinct().ToLookup(x => x.from, x => x.to);

        private static IEnumerable<(string from, string to)> MakeBidirectionalEdges(IEnumerable<(string a, string b)> edges)
        {
            foreach (var edge in edges)
            {
                yield return (edge.a, edge.b);
                yield return (edge.b, edge.a);
            }
        }

        private static IEnumerable<(string inner, string outer)> ParseOrbits(IEnumerable<string> orbits)
            => orbits.Select(x => x.ToUpper().Split(')'))
                     .Where(x => x.Length == 2)
                     .Select(x => (inner: x[0], outer: x[1]));

        private static int CountTotalOrbits(ILookup<string, string> orbits, string node = "COM", int depth = 0)
            => depth + orbits[node].Sum(x => CountTotalOrbits(orbits, x, depth + 1));

        private static IEnumerable<string> TestInput =>
            new[]
            {
                "COM)B",
                "B)C",
                "C)D",
                "D)E",
                "E)F",
                "B)G",
                "G)H",
                "D)I",
                "E)J",
                "J)K",
                "K)L",
                "K)YOU",
                "I)SAN"
            };
    }
}
