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
                               .ToDictionary(x => x.num, x => new List<int> { x.turn });

            var turn = input.Length + 1;
            var last = input.Last();
            while (turn <= 2020)
            {
                var turns = history[last];
                int current;
                if (turns.Count == 1) 
                    current = 0;
                else
                    current = turns[^1] - turns[^2];

                if (history.TryGetValue(current, out var currentTurns))
                    currentTurns.Add(turn);
                else
                    history[current] = new List<int> { turn };
                    
                last = current;
                ++turn;
            }

            Console.WriteLine($"Part 1 Result = {last}");
        }
    }
}
