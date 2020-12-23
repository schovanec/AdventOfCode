using System;
using System.Collections.Generic;
using System.Linq;

namespace Day23
{
    class Program
    {
        static void Main(string[] args)
        {
            var input = "952438716";

            Part1(input);
            Part2(input);
        }

        private static void Part1(string input)
        {
            var cups = input.Select(ch => (int)(ch - '0')).ToList();

            var start = RunGame(cups, 100);

            var result = string.Concat(start.Tail.Select(n => n.Label));
            Console.WriteLine($"Part 1 Result = {result}");
        }

        private static void Part2(string input)
        {
            var cups = input.Select(ch => (int)(ch - '0'));
            var max = cups.Max();

            var start = RunGame(
                cups.Concat(Enumerable.Range(max + 1, 1000000 - max)),
                10000000);

            var next1 = start.Next.Label;
            var next2 = start.Next.Next.Label;

            var result = 1L * next1 * next2;
            Console.WriteLine($"Part 2 Result = {result}");
        }

        private static Node RunGame(IEnumerable<int> cups, int rounds)
        {
            var (current, index) = BuildRing(cups);
            var min = index.Values.Min(n => n.Label);
            var max = index.Values.Max(n => n.Label);

            var taken = new List<int>(3);
            for (var i = 0; i < rounds; ++i)
            {
                taken.Clear();
                taken.AddRange(current.Tail.Take(3).Select(x => x.Label));

                var destLabel = current.Label - 1;
                while (destLabel < min || taken.Contains(destLabel))
                {
                    --destLabel;
                    if (destLabel < min)
                        destLabel = max;
                }

                var firstTaken = current.Next;
                var lastTaken = current.Tail.Take(3).Last();

                current.Next = lastTaken.Next;

                var dest = index[destLabel];
                lastTaken.Next = dest.Next;
                dest.Next = firstTaken;

                current = current.Next;
            }

            return index[1];
        }

        static (Node start, Dictionary<int, Node> index) BuildRing(IEnumerable<int> cups)
        {
            Node first = null;
            Node last = null;
            var index = new Dictionary<int, Node>();
            foreach (var label in cups)
            {
                var node = new Node(label);
                if (first == null)
                    first = node;

                if (last != null)
                    last.Next = node;

                last = node;
                index[label] = node;
            }
            last.Next = first;

            return (first, index);
        }
        
        class Node
        {
            public Node(int label)
            {
                Label = label;
            }

            public int Label { get; }

            public Node Next { get; set; }

            public IEnumerable<Node> Tail
            {
                get
                {
                    var current = Next;
                    while (current != this)
                    {
                        yield return current;
                        current = current.Next;
                    }
                }
            }
        }
    }
}
