using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Day07
{
    class Program
    {
        static void Main(string[] args)
        {
            var input = File.ReadLines(args.DefaultIfEmpty("input.txt").First())
                            .Select(ParseInstruction)
                            .ToList();

            var allTasks = input.Select(x => x.Before)
                                .Concat(input.Select(x => x.After))
                                .ToImmutableSortedSet();
            var required = input.ToLookup(x => x.After, x => x.Before);

            Part1(allTasks, required);
            Part2(allTasks, required);
        }

        private static void Part1(ImmutableSortedSet<char> allTasks, ILookup<char, char> required)
        {
            var incomplete = allTasks;
            var complete = ImmutableSortedSet<char>.Empty;
            var order = new List<char>(allTasks.Count);

            while (incomplete.Count > 0)
            {
                var next = GetNextTask(incomplete, complete, required).Value;
                order.Add(next);
                incomplete = incomplete.Remove(next);
                complete = complete.Add(next);                                 
            }

            Console.WriteLine($"Part 1 Result = {string.Concat(order)}");
        }

        private static void Part2(ImmutableSortedSet<char> allTasks, ILookup<char, char> required, int workers = 5, int minTime = 60, int stepTime = 1)
        {
            var incomplete = allTasks;
            var complete = ImmutableSortedSet<char>.Empty;            
            var work = ImmutableList<(char Task, int FinishTime)>.Empty;
            var time = 0;
            while (incomplete.Count > 0)
            {
                while (work.Count < workers)
                {
                    var available = incomplete.Except(work.Select(x => x.Task));
                    var next = GetNextTask(available, complete, required);
                    if (!next.HasValue)
                        break;

                    var taskTime = minTime + ((next.Value - 'A' + 1) * stepTime);
                    var finishTime = time + taskTime;
                    work = work.Add((next.Value, finishTime));
                }

                time = work.Select(x => x.FinishTime)
                           .DefaultIfEmpty(time)
                           .Min();

                var done = work.Where(x => x.FinishTime <= time)
                               .Select(x => x.Task);
                incomplete = incomplete.Except(done);
                complete = complete.Union(done);

                work = work.RemoveAll(x => x.FinishTime <= time);
            }

            Console.WriteLine($"Part 2 Result = {time}");
        }

        private static char? GetNextTask(IImmutableSet<char> tasks, IImmutableSet<char> complete, ILookup<char, char> required)
            => tasks.Where(x => required[x].All(complete.Contains))
                    .Select(x => (char?)x)
                    .FirstOrDefault<char?>();

        private static (char Before, char After) ParseInstruction(string input)
        {
            var m = Regex.Match(input,
                                @"Step (?<before>[A-Z]) must be finished before step (?<after>[A-Z]) can begin.");
            if (!m.Success)
                throw new FormatException();

            return (
                m.Groups["before"].Value[0],
                m.Groups["after"].Value[0]
            );
        }
    }
}
