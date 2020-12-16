using System;
using System.Collections.Generic;
using System.Linq;

namespace Day15
{
    class Program
    {
        static void Main(string[] args)
        {
            var input = args.DefaultIfEmpty("14,3,1,0,9,5")
                            .First()
                            .Split(',')
                            .Select(int.Parse)
                            .ToArray();

            var history = input.Select((n, i) => (num: n, turn: i + 1))
                               .ToDictionary(x => x.num, x => (last: x.turn, prev: default(int?)));

            var currentTurn = input.Length + 1;
            var lastNumber = input.Last();
            while (currentTurn <= 30000000)
            {
                int nextNumber;
                var (lastTurnNumber, previousTurnNumber) = history[lastNumber];

                if (previousTurnNumber.HasValue)
                    nextNumber = lastTurnNumber - previousTurnNumber.Value;
                else
                    nextNumber = 0;

                if (history.TryGetValue(nextNumber, out var nextHumberHistory))
                    history[nextNumber] = (currentTurn, nextHumberHistory.last);
                else
                    history[nextNumber] = (currentTurn, null);

                if (currentTurn == 2020)
                    Console.WriteLine($"Part 1 Result = {nextNumber}");

                lastNumber = nextNumber;
                ++currentTurn;
            }

            Console.WriteLine($"Part 2 Result = {lastNumber}");
        }
    }
}
