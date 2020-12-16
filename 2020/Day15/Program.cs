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
                               .ToDictionary(x => x.num, x => ((int?)x.turn, default(int?)));

            var previousTurn = new Dictionary<int, int>();
            var lastTurn = input.Select((n, i) => (num: n, turn: i + 1))
                               .ToDictionary(x => x.num, x => x.turn);

            var currentTurn = input.Length + 1;
            var lastNumber = input.Last();
            while (currentTurn <= 30000000)
            {
                int nextNumber;
                var lastTurnNumber = lastTurn[lastNumber];
                if (previousTurn.TryGetValue(lastNumber, out var previousTurnNumber))
                    nextNumber = lastTurnNumber - previousTurnNumber;
                else
                    nextNumber = 0;

                if (lastTurn.TryGetValue(nextNumber, out var lastTurnForNextNumber))
                    previousTurn[nextNumber] = lastTurnForNextNumber;

                lastTurn[nextNumber] = currentTurn;

                if (currentTurn == 2020)
                    Console.WriteLine($"Part 1 Result = {nextNumber}");

                lastNumber = nextNumber;
                ++currentTurn;
            }

            Console.WriteLine($"Part 2 Result = {lastNumber}");
        }
    }
}
