using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Day07
{
    class Program
    {
        static void Main(string[] args)
        {
            var file = args.DefaultIfEmpty("input.txt").First();

            var rules = File.ReadLines(file)
                            .SelectMany(ParseRule)
                            .ToList();

            const string target = "shiny gold";
            Part1(rules, target);
            Part2(rules, target);
        }

        private static void Part1(IEnumerable<Rule> rules, string target)
        {
            var adjacent = rules.ToLookup(x => x.Inner);
            var found = new HashSet<string>();
            var queue = new Queue<string>();
            queue.Enqueue(target);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var item in adjacent[current].Select(x => x.Outer))
                {
                    if (!found.Contains(item))
                    {
                        found.Add(item);
                        queue.Enqueue(item);
                    }
                }
            }
            
            Console.WriteLine($"Part 1 Result = {found.Count}");
        }

        private static void Part2(IList<Rule> rules, string target)
        {
            var adjacent = rules.ToLookup(x => x.Outer);
            var count = CountReachable(target, adjacent);
            Console.WriteLine($"Part 1 Result = {count}");
        }

#if true
        // iterative version

        private static int CountReachable(string start, ILookup<string, Rule> adjacent)
        {
            var totalCount = -1; // ignore start
            var work = new Queue<(string, int)>();
            work.Enqueue((start, 1));
            while (work.Count > 0)
            {
                var (bag, count) = work.Dequeue();
                
                totalCount += count;

                foreach (var child in adjacent[bag])
                    work.Enqueue((child.Inner, child.Count * count));
            }

            return totalCount;
        }
#else
        // recursive version

        private static int CountReachable(string start, ILookup<string, Rule> adjacent)
            => adjacent[start].Sum(r => r.Count * (1 + CountReachable(r.Inner, adjacent)));
#endif

        private static IEnumerable<Rule> ParseRule(string rule)
        {
            var match = Regex.Match(
                rule,
                @"^(?<outer>[\w\s]+) bags? contain ((?<count>\d+) (?<inner>[\w\s]+) bags?(,\s+)?)+\.",
                RegexOptions.Singleline);

            if (match.Success)
            {
                var outer = match.Groups["outer"].Value;
                for (var i = 0; i < match.Groups["count"].Captures.Count; ++i)
                {
                    var inner = match.Groups["inner"].Captures[i].Value;
                    var count = int.Parse(match.Groups["count"].Captures[i].Value);
                    yield return new Rule(outer, inner, count);
                }
            }
        }

        record Rule(string Outer, string Inner, int Count);
    }
}
