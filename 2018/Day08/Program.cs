using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace Day08
{
    class Program
    {
        static void Main(string[] args)
        {
            var fileName = args.DefaultIfEmpty("input.txt").First();
            var input = Parse(File.ReadAllText(fileName));

            var tree = BuildTree(input);

            Console.WriteLine($"Part 1 Result = {tree.SumMetadata()}");
            Console.WriteLine($"Part 2 Result = {tree.GetValue()}");
        }

        private static Node BuildTree(IReadOnlyList<int> input)
        {
            var pos = 0;
            return BuildNode();

            Node BuildNode()
            {
                var childCount = input[pos++];
                var metaCount = input[pos++];

                var children = Enumerable.Range(0, childCount)
                                         .Select(_ => BuildNode())
                                         .ToImmutableArray();

                var metadata = Enumerable.Range(pos, metaCount)
                                         .Select(i => input[i])
                                         .ToImmutableArray();

                pos += metaCount;

                return new Node(children, metadata);
            }
        }

        private static ImmutableList<int> Parse(ReadOnlySpan<char> input)
        {
            var result = ImmutableList.CreateBuilder<int>();

            var start = -1;
            var current = 0;
            while (current < input.Length)
            {
                if (char.IsDigit(input[current]))
                {
                    if (start < 0)
                        start = current;
                }
                else if (start >= 0 && current > start)
                {
                    result.Add(int.Parse(input.Slice(start, current - start)));
                    start = -1;
                }

                ++current;                
            }

            if (start >= 0 && current > start)
                result.Add(int.Parse(input.Slice(start, current - start)));

            return result.ToImmutable();
        }

        record Node(ImmutableArray<Node> Children, ImmutableArray<int> Metadata)
        {
            public int SumMetadata()
                => Metadata.Sum() + Children.Select(c => c.SumMetadata()).Sum();

            public int GetValue()
                => Children.IsEmpty
                    ? Metadata.Sum()
                    : Metadata.Where(v => v > 0 && v <= Children.Length)
                              .Select(v => Children[v - 1].GetValue())
                              .Sum();
        }
    }
}
