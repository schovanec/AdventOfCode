using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Day16
{
    class Program
    {
        static void Main(string[] args)
        {
            var file = args.DefaultIfEmpty("input.txt").First();
            var input = Input.Parse(File.ReadLines(file));

            Part1(input);
            Part2(input);
        }

        private static void Part1(Input input)
        {
            var invalids = from t in input.NearbyTickets
                           from n in t.Numbers
                           where !input.Rules.Any(r => r.rule.IsValid(n))
                           select n;

            var sum = invalids.Sum();
            Console.WriteLine($"Part 1 Result = {sum}");
        }

        private static void Part2(Input input)
        {
            var validTicketCount = 0;
            var fieldMatchCounts = (from name in input.Rules.Select(r => r.name).Distinct()
                                    from i in Enumerable.Range(0, input.OwnTicket.Numbers.Count)
                                    select (name, pos: i)).ToDictionary(x => x, _ => 0);

            foreach (var ticket in input.NearbyTickets)
            {
                var matches = ticket.Numbers
                                    .Select(n => input.Rules.Where(r => r.rule.IsValid(n)).Select(r => r.name).Distinct().ToArray())
                                    .ToArray();

                if (matches.All(m => m.Length > 0))
                {
                    ++validTicketCount;
                    for (var i = 0; i < matches.Length; ++i)
                    {
                        foreach (var m in matches[i])
                            fieldMatchCounts[(m, i)]++;
                    }
                }
            }

            var candidates = fieldMatchCounts.Where(m => m.Value == validTicketCount)
                                             .Select(x => x.Key)
                                             .GroupBy(x => x.pos)
                                             .OrderBy(g => g.Count());

            var assignedFieldPositions = new Dictionary<string, int>();
            foreach (var candidate in candidates)
            {
                var pick = candidate.Where(x => !assignedFieldPositions.ContainsKey(x.name))
                                    .First();

                assignedFieldPositions[pick.name] = candidate.Key;
            }

            var desiredValues = from f in assignedFieldPositions
                                where f.Key.StartsWith("departure")
                                select (long)input.OwnTicket.Numbers[f.Value];

            var product = desiredValues.Aggregate((a, b) => a * b);
            Console.WriteLine($"Part 2 Result = {product}");
        }

        record Input(List<(string name, Rule rule)> Rules, Ticket OwnTicket, List<Ticket> NearbyTickets)
        {
            public static Input Parse(IEnumerable<string> lines)
            {
                using var e = lines.GetEnumerator();
                
                var rules = new List<(string name, Rule rule)>();
                while (e.MoveNext() && !string.IsNullOrEmpty(e.Current))
                {
                    var split = e.Current.Split(':');
                    rules.AddRange(from r in split[1].Split("or")
                                   select (split[0], Rule.Parse(r)));
                }

                if (!e.MoveNext() || !e.MoveNext())
                    throw new FormatException();

                var ownTicket = new Ticket(e.Current.Split(',').Select(int.Parse).ToList());

                if (!e.MoveNext() || !e.MoveNext())
                    throw new FormatException();

                var otherTickets = new List<Ticket>();
                while (e.MoveNext())
                {
                    otherTickets.Add(new Ticket(e.Current.Split(',').Select(int.Parse).ToList()));
                }

                return new Input(rules,
                                 ownTicket,
                                 otherTickets);
            }
        }

        record Ticket(List<int> Numbers);

        record Rule(int Min, int Max)
        {
            public bool IsValid(int number) => number >= Min && number <= Max;

            public static Rule Parse(string rule)
            {
                var splits = rule.Split('-', 2);
                return new Rule(int.Parse(splits[0].Trim()),
                                int.Parse(splits[1].Trim()));
            }
        }
    }
}
