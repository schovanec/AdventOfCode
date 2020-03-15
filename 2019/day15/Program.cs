using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace day15
{
    class Program
    {
        static void Main(string[] args)
        {
            var code = File.ReadLines("input.txt").FirstOrDefault();
            var program = IntcodeMachine.ParseCode(code);

            var movesToGenerator = CountMovesToGenerator(program, out var space, out var found);
            Console.WriteLine($"Fewest Moves = {movesToGenerator}");

            var longestTime = FindFarthestPoint(space, found);
            Console.WriteLine($"Minutes to Fill = {longestTime}");
        }

        private static long FindFarthestPoint(HashSet<(long x, long y)> space, (long x, long y) start)
        {
            var distance = new Dictionary<(long x, long y), long>() { [start] = 0L };
            var queue = new Queue<(long x, long y)>();
            queue.Enqueue(start);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var nextDistance = distance[current] + 1;

                foreach (var offset in AdjacentLocationOffsets)
                {
                    var adjacent = (current.x + offset.x, current.y + offset.y);
                    if (space.Contains(adjacent))
                    {
                        if (!distance.TryGetValue(adjacent, out var adjacentDistance) || adjacentDistance > nextDistance)
                        {
                            distance[adjacent] = nextDistance;
                            queue.Enqueue(adjacent);
                        }
                    }
                }
            }

            return distance.Values.Max();
        }

        private static ImmutableArray<(long x, long y)> AdjacentLocationOffsets
            = ImmutableArray.Create((1L, 0L), (-1L, 0L), (0L, 1L), (0L, -1L));

        private static long CountMovesToGenerator(long[] program, out HashSet<(long x, long y)> space, out (long x, long y) found)
        {
            found = (0, 0);

            var position = (x: 0L, y: 0L);
            var visited = new HashSet<(long x, long y)>() { position };

            var machine = new IntcodeMachine(program);
            var walls = new HashSet<(long x, long y)>();
            var moves = new Stack<long>();
            long? movesToGenerator = null;
            var done = false;
            while (!done)
            {
                var next = (from cmd in Enumerable.Range(1, 4)
                            let newPosition = CalculatePosition(position, cmd)
                            where !walls.Contains(newPosition)
                               && !visited.Contains(newPosition)
                            select new { Command = cmd, NewPosition = newPosition }).FirstOrDefault();

                if (next != null)
                {
                    // try to move
                    var result = machine.StepMany(next.Command);
                    if (result.Output == 0)
                    {
                        walls.Add(next.NewPosition);
                    }
                    else
                    {
                        visited.Add(next.NewPosition);
                        position = next.NewPosition;
                        moves.Push(next.Command);
                        if (result.Output == 2)
                        {
                            found = position;
                            movesToGenerator = moves.Count;
                        }
                        else if (result.Output != 1)
                        {
                            throw new InvalidOperationException("Unexpected output");
                        }
                    }
                }
                else if (moves.Count > 0)
                {
                    // backtrack
                    var cmd = InverseMove(moves.Pop());
                    var result = machine.StepMany(cmd);
                    if (result.Output != 1)
                        throw new InvalidOperationException("Unexpected output");

                    position = CalculatePosition(position, cmd);
                }
                else
                {
                    done = true;
                }
            }

            space = visited;
            return movesToGenerator ?? 0;
        }

        private static (long x, long y) CalculatePosition((long x, long y) position, long command)
            => command switch
            {
                1 => (position.x, position.y + 1),
                2 => (position.x, position.y - 1),
                3 => (position.x - 1, position.y),
                4 => (position.x + 1, position.y),
                _ => position
            };

        private static long InverseMove(long move)
            => move switch
            {
                1 => 2,
                2 => 1,
                3 => 4,
                4 => 3,
                _ => throw new ArgumentOutOfRangeException(nameof(move))
            };
    }
}
