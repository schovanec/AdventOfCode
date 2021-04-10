using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Day02
{
    class Program
    {
        static void Main(string[] args)
        {
            var input = File.ReadLines(args.DefaultIfEmpty("input.txt").First()).ToList();

            Part1(input);

            Part2(input);
        }

        private static void Part1(List<string> input)
        {
            var allCounts = input.SelectMany(GetDistinctLetterCounts)
                                 .ToList();

            var count2 = allCounts.Count(x => x == 2);
            var count3 = allCounts.Count(x => x == 3);

            var checkSum = count2 * count3;
            Console.WriteLine($"Part 1 Result = {checkSum}");
        }

        private static void Part2(List<string> input)
        {
            var (a, b) = FindPairWithOneDifferentLetter(input);
            var common = GetCommonLetters(a, b);

            Console.WriteLine($"Part 2 Result = {string.Join("", common)}");
        }

        private static IEnumerable<int> GetDistinctLetterCounts(string text)
            => text.GroupBy(x => x)
                   .Select(g => g.Count())
                   .Distinct();

        private static (string, string) FindPairWithOneDifferentLetter(IEnumerable<string> ids)
        {
            string previous = null;
            foreach (var id in ids.OrderBy(x => x))
            {
                if (previous != null && HasSingleDifferentLetter(previous, id))
                    return (previous, id);

                previous = id;
            }

            throw new Exception("Not Found!");
        }

        private static bool HasSingleDifferentLetter(string first, string second)
            => first.Length == second.Length
            && GetCommonLetters(first, second).Count() == first.Length - 1;

        private static IEnumerable<char> GetCommonLetters(string first, string second)
            => first.Zip(second)
                    .Where(x => x.First == x.Second)
                    .Select(x => x.First);
    }
}
