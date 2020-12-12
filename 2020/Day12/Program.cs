using System;
using System.IO;
using System.Linq;

namespace Day12
{
    class Program
    {
        static void Main(string[] args)
        {
            var file = args.DefaultIfEmpty("input.txt").First();

            var directions = File.ReadLines(file)
                                 .Select(line => Direction.Parse(line))
                                 .ToArray();

            Part1(directions);
            Part2(directions);
        }

        static void Part1(Direction[] directions)
        {
            var position = new HeadingPosition((0, 0), (1, 0));
            foreach (var direction in directions)
                position = position.Move(direction);

            Console.WriteLine($"Part 1 Result = {ManhattanDistance(position.Current)}");
        }

        static void Part2(Direction[] directions)
        {
            var position = new WaypointPosition((0, 0), (10, 1));
            foreach (var direction in directions)
                position = position.Move(direction);

            Console.WriteLine($"Part 2 Result = {ManhattanDistance(position.Current)}");
        }

        static int ManhattanDistance((int x, int y) pos)
            => Math.Abs(pos.x) + Math.Abs(pos.y);

        record HeadingPosition((int x, int y) Current, (int dx, int dy) Heading)
        {
            public HeadingPosition Move(Direction dir)
            {
                var newHeading = dir.Rotation?.Invoke(Heading) ?? Heading;
                var (x, y) = Current;
                x += dir.Offset.x + (dir.HeadingDistance * newHeading.dx);
                y += dir.Offset.y + (dir.HeadingDistance * newHeading.dy);
                return new HeadingPosition((x, y), newHeading);
            }
        }

        record WaypointPosition((int x, int y) Current, (int x, int y) Waypoint)
        {
            public WaypointPosition Move(Direction dir)
            {
                var (wx, wy) = dir.Rotation?.Invoke(Waypoint) ?? Waypoint;
                wx += dir.Offset.x;
                wy += dir.Offset.y;

                var (x, y) = Current;
                x += dir.HeadingDistance * wx;
                y += dir.HeadingDistance * wy;
                return new WaypointPosition((x, y), (wx, wy));
            }
        }

        record Direction((int x, int y) Offset = default, int HeadingDistance = 0, Func<(int dx, int dy), (int dx, int dy)> Rotation = default)
        {
            public static Direction Parse(ReadOnlySpan<char> instruction)
            {
                var command = instruction[0];
                var arg = int.Parse(instruction.Slice(1));
                return command switch
                {
                    'N' => new Direction { Offset = (0, arg) },
                    'S' => new Direction { Offset = (0, -arg) },
                    'E' => new Direction { Offset = (arg, 0) },
                    'W' => new Direction { Offset = (-arg, 0) },
                    'L' => new Direction { Rotation = v => Rotate(v, arg) },
                    'R' => new Direction { Rotation = v => Rotate(v, -arg) },
                    'F' => new Direction { HeadingDistance = arg },
                    _ => throw new FormatException("Invalid Instruction")
                };
            }

            private static (int dx, int dy) Rotate((int dx, int dy) v, int angle)
                => angle switch 
                {
                    90 or -270 => (-v.dy, v.dx),
                    180 or -180 => (-v.dx, -v.dy),
                    270 or -90 => (v.dy, -v.dx),
                    _ => throw new InvalidOperationException("Invalid Angle")
                };
        }
    }
}
