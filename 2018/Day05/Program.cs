using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Day05
{
    class Program
    {
        static void Main(string[] args)
        {
            var input = File.ReadAllText(args.DefaultIfEmpty("input.txt").First());

            Part1(input);
            Part2(input);
        }

        private static void Part1(string input)
        {
            var result = ReactPolymer(input);
            Console.WriteLine($"Part 1 Result = {result.Count()}");
        }

        private static void Part2(string input)
        {
            var best = (from u1 in input.ToLower().Distinct()
                        let u2 = char.ToUpper(u1)
                        let r = ReactPolymer(input.Where(ch => ch != u1 && ch != u2))
                        select r.Count()).Min();

            Console.WriteLine($"Part 2 Result = {best}");
        }

        private static IEnumerable<char> ReactPolymer(IEnumerable<char> input)
        {
            var list = new LinkedList<char>(input);

            for (; ; )
            {
                var current = list.First;
                var done = true;
                while (current != null && current.Next != null)
                {
                    var first = current;
                    var second = current.Next;
                    if (IsReactivePair(first.Value, second.Value))
                    {
                        current = first.Previous ?? second.Next;
                        list.Remove(first);
                        list.Remove(second);
                        done = false;
                    }
                    else
                    {
                        current = current.Next;
                    }
                }

                if (done)
                    break;
            }

            return list;
        }

        private static bool IsReactivePair(char a, char b)
            => (char.IsLower(a) && char.IsUpper(b) && a == char.ToLower(b))
            || (char.IsUpper(a) && char.IsLower(b) && b == char.ToLower(a));
    }
}
