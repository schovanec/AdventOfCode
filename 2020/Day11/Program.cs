using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace Day11
{
    class Program
    {
        static void Main(string[] args)
        {
            var file = args.DefaultIfEmpty("input.txt").First();
            var map = Map.Parse(File.ReadLines(file));

            var part1Result = StepUntilEqual(map, StepPart1);
            Console.WriteLine($"Part 1 Result = {part1Result.OccupiedSeatingPositions.Count()}");

            var part2Result = StepUntilEqual(map, StepPart2);
            Console.WriteLine($"Part 2 Result = {part2Result.OccupiedSeatingPositions.Count()}");
        }

        static Map StepPart1(Map map)
            => Step(map,
                    shouldSit: p => !map.AdjacentOccupiedSeats(p).Any(),
                    shouldLeave: p => map.AdjacentOccupiedSeats(p).Count() >= 4);

        static Map StepPart2(Map map)
            => Step(map,
                    shouldSit: p => !map.VisibleOccupiedSeats(p).Any(),
                    shouldLeave: p => map.VisibleOccupiedSeats(p).Count() >= 5);

        static Map Step(Map map, Func<(int x, int y), bool> shouldSit, Func<(int x, int y), bool> shouldLeave)
        {
            var seats = map.Seats.ToBuilder();

            foreach (var pos in map.AllSeatingPositions)
            {
                if (map.IsOccupied(pos))
                {
                    if (shouldLeave(pos))
                        seats[pos] = false;
                }
                else
                {
                    if (shouldSit(pos))
                        seats[pos] = true;                    
                }
            }

            return map with { Seats = seats.ToImmutable() };
        }

        private static Map StepUntilEqual(Map map, Func<Map, Map> step)
        {
            var current = map;
            while (true)
            {
                var next = step(current);
                if (current.Seats.SequenceEqual(next.Seats))
                    break;

                current = next;
            }

            return current;
        }

        record Map(int Width, int Height, ImmutableSortedDictionary<(int x, int y), bool> Seats)
        {
            private ImmutableArray<(int dx, int dy)> adjacentDirections
                = (from dx in Enumerable.Range(-1, 3)
                   from dy in Enumerable.Range(-1, 3)                   
                   where dx != 0 || dy != 0
                   select (dx, dy)).ToImmutableArray();

            public IEnumerable<(int x, int y)> AllSeatingPositions => Seats.Keys;

            public IEnumerable<(int x, int y)> OccupiedSeatingPositions
                => Seats.Where(x => x.Value).Select(x => x.Key);

            public bool IsOccupied((int x, int y) pos) => Seats.GetValueOrDefault(pos);

            public IEnumerable<(int x, int y)> AdjacentSeats((int x, int y) pos)
                => adjacentDirections.Select(d => (pos.x + d.dx, pos.y + d.dy))
                                     .Where(Seats.ContainsKey);

            public IEnumerable<(int x, int y)> AdjacentOccupiedSeats((int x, int y) pos)
                => AdjacentSeats(pos).Where(Seats.GetValueOrDefault);

            public IEnumerable<(int x, int y)> VisibleSeats((int x, int y) pos)
                => adjacentDirections.Select(d => FirstVisible(pos, d))
                                     .Where(p => p.HasValue)
                                     .Select(p => p.Value);

            public IEnumerable<(int x, int y)> VisibleOccupiedSeats((int x, int y) pos)
                => VisibleSeats(pos).Where(Seats.GetValueOrDefault);

            private (int x, int y)? FirstVisible((int x, int y) pos, (int dx, int dy) dir)
            {
                while (true)
                {
                    pos = (pos.x + dir.dx, pos.y + dir.dy);
                    if (pos.x < 0 || pos.y < 0 || pos.x >= Width || pos.y >= Height)
                        break;

                    if (Seats.ContainsKey(pos))
                        return pos;
                }

                return null;
            }

            public static Map Parse(IEnumerable<string> input)
            {
                var seats = ImmutableSortedDictionary.CreateBuilder<(int, int), bool>();

                var y = 0;
                var width = 0;
                foreach (var line in input)
                {
                    for (int x = 0; x < line.Length; ++x)
                    {
                        var ch = line[x];
                        if (ch == 'L')
                            seats[(x, y)] = false;
                        else if (ch == '#')
                            seats[(x, y)] = true;
                    }

                    if (width < line.Length)
                        width = line.Length;

                    ++y;
                }

                return new Map(width, y, seats.ToImmutable());
            }
        }
    }
}
