using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Day01
{
    class Program
    {
        static void Main(string[] args)
        {
            var input = File.ReadLines(args.DefaultIfEmpty("input.txt").First())
                            .Select(int.Parse)
                            .ToList(); ;

            Part1(input);

            Part2(input);
        }

        private static void Part1(List<int> input)
        {
            var result1 = input.Sum();
            Console.WriteLine($"Part 1 Result = {result1}");
        }

        private static void Part2(List<int> input)
        {
            var seen = new HashSet<int>();
            var sum = 0;
            var index = 0;
            while (true)
            {
                if (index >= input.Count)
                    index = 0;

                sum += input[index];
                ++index;

                if (seen.Contains(sum))
                    break;

                seen.Add(sum);
            }

            Console.WriteLine($"Part 2 Result = {sum}");
        }

    }
}
