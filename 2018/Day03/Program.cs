using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Day03
{
    class Program
    {
        static void Main(string[] args)
        {
            var input = File.ReadLines(args.FirstOrDefault() ?? "input.txt")
                            .Select(Claim.Parse)
                            .ToList();

            Part1(input);

            Part2(input);
        }

        private static void Part1(IEnumerable<Claim> claims)
        {
            const int size = 1000;
            var field = new int[size*size];

            foreach (var claim in claims)
            {
                for (var y = claim.Area.Top; y < claim.Area.Bottom; ++y)
                {
                    var offset = y * size;
                    for (var x = claim.Area.Left; x < claim.Area.Right; ++x)
                        ++field[offset + x];
                }
            }

            var overlapCount = field.Count(x => x > 1);
            Console.WriteLine($"Part 1 Result = {overlapCount}");
        }

        private static void Part2(IReadOnlyList<Claim> claims)
        {
            var result = claims.Where(a => claims.All(b => a == b || !a.IsOverlap(b))).First();

            Console.WriteLine($"Part 2 Result = {result.Id}");
        }

        private record Claim(int Id, Rect Area)
        {
            public bool IsOverlap(Claim other)
                => Area.IsOverlap(other.Area);

            private static readonly Regex Pattern = new Regex(@"^#(?<id>\d+)\s+@\s+(?<left>\d+),(?<top>\d+):\s+(?<width>\d+)x(?<height>\d+)$",
                                                              RegexOptions.Compiled | RegexOptions.Singleline);

            public static Claim Parse(string text)
            {
                var m = Pattern.Match(text);
                if (!m.Success)
                    throw new FormatException();

                return new Claim(
                    int.Parse(m.Groups["id"].Value),
                    new Rect(
                        int.Parse(m.Groups["left"].Value),
                        int.Parse(m.Groups["top"].Value),
                        int.Parse(m.Groups["width"].Value),
                        int.Parse(m.Groups["height"].Value)));
            }
        }

        private record Rect(int Left, int Top, int Width, int Height)
        {
            public int Right => Left + Width;

            public int Bottom => Top + Height;

            public bool IsOverlap(Rect other)
                => Left < other.Right
                && Right > other.Left
                && Top < other.Bottom
                && Bottom > other.Top;
        }
    }
}
