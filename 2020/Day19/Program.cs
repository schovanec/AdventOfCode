using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace Day19
{
    class Program
    {
        static void Main(string[] args)
        {
            var file = args.DefaultIfEmpty("input.txt").First();
            var input = File.ReadAllLines(file);

            var rules = ReadRuleText(input);
            var messages = input.SkipWhile(x => !string.IsNullOrEmpty(x)).Skip(1).ToList();

            Part1(rules, messages);
            Part2(rules, messages);
        }

        private static void Part1(IImmutableDictionary<int, string> rules, List<string> messages)
        {
            var rule = ParseRule(rules, 0);
            var result = messages.Count(rule.IsMatch);
            Console.WriteLine($"Part 1 Result = {result}");
        }

        private static void Part2(IImmutableDictionary<int, string> rules, List<string> messages)
        {
            var rule = ParseRule(rules, 0, true);
            var result = messages.Count(rule.IsMatch);
            Console.WriteLine($"Part 2 Result = {result}");
        }

        static IImmutableDictionary<int, string> ReadRuleText(string[] input)
        {
            return (from line in input.TakeWhile(x => !string.IsNullOrEmpty(x))
                    let split = line.Split(": ", 2)
                    select (id: int.Parse(split[0]), rule: split[1])).ToImmutableDictionary(x => x.id, x => x.rule);
        }

        static Rule ParseRule(IImmutableDictionary<int, string> rules, int index, bool insertLoops = false)
        {
            return Parse(index);

            Rule Parse(int index)
            {
                var rule = rules[index];

                if (rule.StartsWith("\""))
                    return new SimpleRule(rule[1]);

                if (insertLoops)
                {    
                    if (index == 8)
                        return new RepeatRule(Parse(42));
                    if (index == 11)
                        return new BalancedRule(Parse(42), Parse(31));
                }

                var options = rule.Split('|', StringSplitOptions.TrimEntries)
                                .Select(opt => opt.Split(' ', StringSplitOptions.TrimEntries)
                                                  .Select(x => Parse(int.Parse(x)))
                                                  .ToArray())
                                .Select(PairRule.CreateSequence)
                                .ToImmutableArray();

                return options.Length == 1
                    ? options.Single()
                    : new OptionRule(options);
            }
        }

        record MatchState(string Text, int Offset = 0)
        {
            public MatchState Consume(int count = 1)
                => this with { Offset = Offset + count };

            public bool IsEmpty => Offset >= Text.Length;

            public char FirstCharacter => Text[Offset];
        }

        abstract record Rule()
        {
            public bool IsMatch(string text)
                => Match(new MatchState(text)).Where(r => r.IsEmpty).Any();

            public abstract IEnumerable<MatchState> Match(MatchState input);
        }

        record SimpleRule(char Character) : Rule
        {
            public override IEnumerable<MatchState> Match(MatchState input)
            {
                if (!input.IsEmpty && input.FirstCharacter == Character)
                    yield return input.Consume();
            }
        }

        record OptionRule(ImmutableArray<Rule> Options) : Rule
        {
            public override IEnumerable<MatchState> Match(MatchState input)
                => Options.SelectMany(r => r.Match(input));
        }

        record PairRule(Rule First, Rule Second) : Rule
        {
            public override IEnumerable<MatchState> Match(MatchState input)
                => from next in First.Match(input)
                   from result in Second.Match(next)
                   select result;

            public static Rule CreateSequence(IList<Rule> rules)
            {
                Rule result = null;

                for (var i = rules.Count - 1; i >= 0; --i)
                {
                    result = result != null 
                        ? new PairRule(rules[i], result)
                        : rules[i];
                }

                return result;
            }
        }

        record RepeatRule(Rule First) : Rule
        {
            public override IEnumerable<MatchState> Match(MatchState input)
                => from next in First.Match(input)
                   from result in Match(next).Concat(Enumerable.Repeat(next, 1))
                   select result;
        }

        record BalancedRule(Rule First, Rule Second) : Rule
        {
            public override IEnumerable<MatchState> Match(MatchState input)
                => from next in First.Match(input)
                   from inner in Match(next).Concat(Enumerable.Repeat(next, 1))
                   from result in Second.Match(inner)
                   select result;
        }
    }
}
