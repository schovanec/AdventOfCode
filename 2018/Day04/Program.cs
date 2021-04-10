using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Day04
{
    class Program
    {
        static void Main(string[] args)
        {
            var input = File.ReadLines(args.DefaultIfEmpty("input.txt").First())
                            .OrderBy(x => x)
                            .Select(LogMessage.Parse)
                            .ToList();

            var activity = AnalyseLog(input);

            Part1(activity);

            Part2(activity);
        }

        private static void Part1(IEnumerable<GuardSleep> input)
        {
            var sleeps = input.ToLookup(x => x.GuardId);

            var guardWithMostSleepTime = sleeps.OrderByDescending(g => g.Sum(x => x.TotalMinutes))
                                               .Select(g => g.Key)  
                                               .First();

            var minuteMostAsleep = (from a in sleeps[guardWithMostSleepTime]
                                    from m in a.MinutesAsleep
                                    group a by m into g
                                    orderby g.Count() descending
                                    select g.Key).First();
            
            Console.WriteLine($"Part 1 Result = {guardWithMostSleepTime * minuteMostAsleep}");
        }

        private static void Part2(IEnumerable<GuardSleep> input)
        {
            var (guardId, minute) = (from a in input
                                     from m in a.MinutesAsleep
                                     group a by (a.GuardId, m) into g
                                     orderby g.Count() descending
                                     select g.Key).First();

            Console.WriteLine($"Part 2 Result = {guardId * minute}");
        }

        private static IEnumerable<GuardSleep> AnalyseLog(IEnumerable<LogMessage> messages)
        {
            int currentGuard = 0;
            int? sleepStartMinute = null;
            foreach (var message in messages)
            {
                int minute = int.Parse(message.Time.Split(':')[1]);
                switch (message.Activity)
                {
                    case string a when a.StartsWith("Guard #"):
                        currentGuard = int.Parse(a.Substring(7).Split(' ')[0]);
                        break;

                    case "falls asleep":
                        Debug.Assert(!sleepStartMinute.HasValue);
                        sleepStartMinute = minute;
                        break;

                    case "wakes up":
                        Debug.Assert(currentGuard != 0);
                        Debug.Assert(sleepStartMinute.HasValue);
                        Debug.Assert(minute >= sleepStartMinute.Value);
                        yield return new GuardSleep(message.Date, currentGuard, sleepStartMinute.Value, minute);
                        sleepStartMinute = null;
                        break;

                    default:
                        throw new Exception("Unexpected activity");
                }
            }
        }

        private record GuardSleep(string Date, int GuardId, int StartMinute, int EndMinute)
        {
            public int TotalMinutes => EndMinute - StartMinute;

            public IEnumerable<int> MinutesAsleep => Enumerable.Range(StartMinute, TotalMinutes);
        }

        private record LogMessage(string Date, string Time, string Activity)
        {
            private static readonly Regex MatchLogMessage
                = new Regex(@"^\[(?<date>\d{4}-\d{2}-\d{2}) (?<time>\d{2}:\d{2})\]\s+(?<activity>.*)$",
                            RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant);

            public static LogMessage Parse(string message)
            {
                var m = MatchLogMessage.Match(message);
                if (!m.Success)
                    throw new FormatException();

                return new LogMessage(
                    m.Groups["date"].Value,
                    m.Groups["time"].Value,
                    m.Groups["activity"].Value
                );
            }
        }
    }
}
