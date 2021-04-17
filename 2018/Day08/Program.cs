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
            var tree = BuildTree(ParseInput(File.ReadAllText(fileName)));

            Console.WriteLine($"Part 1 Result = {tree.SumMetadata()}");
            Console.WriteLine($"Part 2 Result = {tree.GetValue()}");
        }

        private static Node BuildTree(IEnumerable<int> input)
            => BuildTree(new Queue<int>(input));

        private static Node BuildTree(Queue<int> input)
        {
            var childCount = input.Dequeue();
            var metaCount = input.Dequeue();

            var children = ImmutableArray.CreateBuilder<Node>(childCount);
            for (var i = 0; i < childCount; ++i)
                children.Add(BuildTree(input));

            var metadata = ImmutableArray.CreateBuilder<int>(metaCount);
            for (var i = 0; i < metaCount; ++i)
                metadata.Add(input.Dequeue());

            return new Node(
                children.ToImmutable(),
                metadata.ToImmutable());
        }

        private static IEnumerable<int> ParseInput(string input)
        {
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
                    yield return int.Parse(input.AsSpan().Slice(start, current - start));
                    start = -1;
                }

                ++current;                
            }

            if (start >= 0 && current > start)
                yield return int.Parse(input.AsSpan().Slice(start, current - start));
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
