using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace day03
{
    class Program
    {
        static void Main(string[] args)
        {
#if false
            var wires = File.ReadAllLines(args.FirstOrDefault() ?? "input.txt");

            var minimumIntersectDistance = FindMinimumDistance(wires);

            Console.WriteLine($"Result: {minimumIntersectDistance}");
#else
            var tests = new[]
            {
                new
                {
                    Wires = new []
                    {
                        "R75,D30,R83,U83,L12,D49,R71,U7,L72",
                        "U62,R66,U55,R34,D71,R55,D58,R83"
                    },
                    MinimumDistance = 159,
                    MinimumTotalSteps = 610
                },
                new
                {
                    Wires = new [] {
                        "R98,U47,R26,D63,R33,U87,L62,D20,R33,U53,R51",
                        "U98,R91,D20,R16,D67,R40,U7,R15,U6,R7"
                    },
                    MinimumDistance = 135,
                    MinimumTotalSteps = 410
                }
            };

            foreach (var test in tests)
            {
                foreach (var line in test.Wires)
                    Console.WriteLine(line);

                var result = FindMinimumDistance(test.Wires);

                Console.WriteLine($"Pass?: {result == test.MinimumDistance}");
                Console.WriteLine();
            }
#endif
        }

        private static int FindMinimumDistance(IEnumerable<string> wires)
        {
            var allPoints = from w in wires
                            let moves = ParseMoves(w)
                            select CollectAllPoints(moves);

            var intersections = allPoints.Aggregate((a, b) => a.Intersect(b));

            return intersections.Min(p => ManhattanDistance(p));
        }

        private static IEnumerable<Move> ParseMoves(string moves)
            => moves.Split(',').Select(Move.Parse);

        private static ImmutableHashSet<(int x, int y)> CollectAllPoints(IEnumerable<Move> moves, (int x, int y) origin = default)
        {
            var result = ImmutableHashSet.CreateBuilder<(int x, int y)>();
            var current = origin;
            foreach (var move in moves)
            {
                foreach (var position in move.GetSteps(current))
                {
                    result.Add(position);
                    current = position;
                }
            }

            return result.ToImmutable();
        }

        private static int ManhattanDistance((int x, int y) a, (int x, int y) b = default)
        {
            return Math.Abs(b.x - a.x) + Math.Abs(b.y - a.y);
        }
    }

    enum Direction
    {
        None,
        Left,
        Right,
        Up,
        Down
    }

    struct Move
    {
        public Move(Direction direction, int distance)
        {
            if (distance < 0)
                throw new ArgumentOutOfRangeException(nameof(distance));

            Direction = direction;
            Distance = distance;
        }

        public Direction Direction { get; }

        public int Distance { get; }

        public (int x, int y) UnitVector
            => Direction switch
            {
                Direction.Right => (1, 0),
                Direction.Left => (-1, 0),
                Direction.Up => (0, 1),
                Direction.Down => (0, -1),
                _ => (0, 0)
            };

        public (int x, int y) Vector
        {
            get
            {
                var unit = UnitVector;
                return (unit.x * Distance, unit.y * Distance);
            }
        }

        public (int x, int y) Transform((int x, int y) point)
        {
            var (deltaX, deltaY) = Vector;
            return (point.x + deltaX, point.y + deltaY);
        }

        public IEnumerable<(int x, int y)> GetSteps((int x, int y) point)
        {
            var (deltaX, deltaY) = UnitVector;
            for (int i = 1; i <= Distance; ++i)
                yield return (point.x + (i * deltaX), point.y + (i * deltaY));
        }

        public override string ToString()
            => $"({Direction}:{Distance})";

        public static Move Parse(string move)
        {
            var direction = move[0] switch
            {
                'R' => Direction.Right,
                'L' => Direction.Left,
                'U' => Direction.Up,
                'D' => Direction.Down,
                _ => throw new FormatException($"Invalid Direction: {move[0]}")
            };

            return new Move(
                direction,
                int.Parse(move.AsSpan().Slice(1))
            );
        }
    }
}
