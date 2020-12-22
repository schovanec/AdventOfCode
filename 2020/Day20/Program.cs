using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;

namespace Day20
{
    class Program
    {
        static void Main(string[] args)
        {
            var file = args.DefaultIfEmpty("input.txt").First();
            var images = ParseInput(File.ReadLines(file)).ToImmutableList();

            Part1(images);
            Part2(images);
        }

        private static void Part1(ImmutableList<Image> images)
        {
            var lookup = (from img in images
                          from edge in img.Edges.All
                          select (img, edge)).ToLookup(x => x.edge, x => x.img);

            var corners = (from img in images
                           from edge in img.Edges.All
                           from m in lookup[edge]
                           where m != img
                           group m by img into g
                           where g.Distinct().Count() == 2
                           select g.Key);

            foreach (var corner in corners)
                Console.WriteLine(corner.Id);

            var result1 = corners.Aggregate(1L, (p, x) => p * x.Id);
            Console.WriteLine($"Part 1 Result = {result1}");
        }

        private static void Part2(IReadOnlyList<Image> images)
        {
            var grid = Arrange(images);
            var result = (from map in EnumMapVariations(Join(grid))
                          let occupied = FindMonsters(map)
                          where occupied.Count > 0
                          let unoccupied = map.Except(occupied)
                          select unoccupied.Count).First();
                                              
            Console.WriteLine($"Part 2 Result = {result}");
        }

        //                   # 
        // #    ##    ##    ###
        //  #  #  #  #  #  #   
        static readonly ImmutableArray<(int x, int y)> monster = ImmutableArray.Create(
            (0, 0),
            (1, 1),
            (4, 1),
            (5, 0),
            (6, 0),
            (7, 1),
            (10, 1),
            (11, 0),
            (12, 0),
            (13, 1),
            (16, 1),
            (17, 0),
            (18, 0),
            (18, -1),
            (19, 0)
        );

        const int MonsterWidth = 18;
        const int MonsterHeight = 3;
        const int MonsterOffsetX = 0;
        const int MonsterOffsetY = 1;

        static ImmutableHashSet<(int x, int y)> FindMonsters(ImmutableHashSet<(int x, int y)> map)
        {
            var min = (x: map.Min(pt => pt.x) + MonsterOffsetX,
                       y: map.Min(pt => pt.y) + MonsterOffsetY);
            var max = (x: map.Max(pt => pt.x) - MonsterWidth + MonsterOffsetY,
                       y: map.Max(pt => pt.y) - MonsterHeight + MonsterOffsetY);

            var eligiblePoints = map.Where(pt => pt.x >= min.x 
                                              && pt.x <= max.x
                                              && pt.y >= min.y
                                              && pt.y <= max.y);

            var result = ImmutableHashSet.CreateBuilder<(int, int)>();
            foreach (var pt in eligiblePoints)
            {
                var monsterPoints = monster.Select(
                    mp => (pt.x + mp.x, pt.y + mp.y));
                if (monsterPoints.All(p => map.Contains(p)))
                    result.UnionWith(monsterPoints);
            }

            return result.ToImmutableHashSet();
        }

        static ImmutableHashSet<(int x, int y)> Join(ImmutableArray<ImmutableArray<Image>> grid)
        {
            var result = ImmutableHashSet.CreateBuilder<(int, int)>();

            var fullSize = grid[0][0].Size;
            var outputSize = fullSize - 2;
            for (var row = 0; row < grid.Length; ++row)
            {
                var dy = row * outputSize;
                for (var col = 0; col < grid[row].Length; ++col)
                {
                    var image = grid[row][col];

                    var dx = col * outputSize;

                    for (var x = 1; x < fullSize - 1; ++x)
                    {
                        for (var y = 1; y < fullSize - 1; ++y)
                        {
                            if (image[(x, y)] == '#')
                                result.Add((x - 1 + dx, y - 1 + dy));
                        }
                    }
                }
            }

            return result.ToImmutable();
        }

        static IEnumerable<ImmutableHashSet<(int x, int y)>> EnumMapVariations(ImmutableHashSet<(int x, int y)> map)
        {
            yield return map;
            yield return FlipX(map);
            yield return FlipY(map);
            yield return FlipX(FlipY(map));

            var rotated = RotateMap(map);
            yield return rotated;
            yield return FlipX(rotated);
            yield return FlipY(rotated);
            yield return FlipX(FlipY(rotated));
        }

        static ImmutableHashSet<(int x, int y)> RotateMap(ImmutableHashSet<(int x, int y)> map)
            => map.Select(pt => (-pt.y, pt.x)).ToImmutableHashSet();

        static ImmutableHashSet<(int x, int y)> FlipX(ImmutableHashSet<(int x, int y)> map)
            => map.Select(pt => (-pt.x, pt.y)).ToImmutableHashSet();

        static ImmutableHashSet<(int x, int y)> FlipY(ImmutableHashSet<(int x, int y)> map)
            => map.Select(pt => (pt.x, -pt.y)).ToImmutableHashSet();

        static ImmutableArray<ImmutableArray<Image>> Arrange(IReadOnlyList<Image> images)
        {
            var lookup = (from img in images
                          from edge in img.Edges.All
                          select (img, edge)).ToLookup(x => x.edge, x => x.img);

            IEnumerable<Image> FindCandidates(Image image, params string[] edges)
                => edges.SelectMany(e => lookup[e]).Where(i => i.Id != image?.Id);

            // find a corner
            var corner = images.First(img => FindCandidates(img, img.Edges.All.ToArray()).Distinct().Count() == 2);


            // orient the corner
            if (FindCandidates(corner, corner.Edges.Left, corner.Edges.LeftFlipped).Any())
                corner = corner.Transform(flipX: true);

            if (FindCandidates(corner, corner.Edges.Top, corner.Edges.TopFlipped).Any())
                corner = corner.Transform(flipY : true);

            var size = (int)Math.Sqrt(images.Count);
            var result = Enumerable.Range(1, size)
                                   .Select(_ => new Image[size])
                                   .ToArray();

            result[0][0] = corner;
            for (var y = 0; y < size; ++y)
            {
                for (var x = 0; x < size; ++x)
                {
                    if (result[y][x] != null)
                        continue;

                    var left = x > 0 ? result[y][x - 1] : default(Image);
                    var above = y > 0 ? result[y - 1][x] : default(Image);

                    // find image to the right
                    var next = left != null
                        ? FindCandidates(left, left.Edges.Right, left.Edges.RightFlipped).First()
                        : FindCandidates(above, above.Edges.BottomFlipped, above.Edges.BottomFlipped).First();

                    result[y][x] = next.AlignWith(left?.Edges.Right, above?.Edges.Bottom);
                }
            }

            return result.Select(row => row.ToImmutableArray()).ToImmutableArray();
        }

        static IEnumerable<Image> ParseInput(IEnumerable<string> input)
            => from image in SplitImages(input)
               select new Image(image.id, image.lines.Count, string.Concat(image.lines));

        static IEnumerable<(int id, List<string> lines)> SplitImages(IEnumerable<string> input)
        {
            var id = 0;
            List<string> lines = null;
            foreach (var line in input)
            {
                if (string.IsNullOrEmpty(line))
                {
                    if (lines != null)
                        yield return (id, lines);

                    lines = null;
                }
                else if (line.StartsWith("Tile "))
                {
                    id = int.Parse(line.Substring(5, line.Length - 6));
                    lines = new List<string>();
                }
                else if (lines != null)
                {
                    lines.Add(line);
                }
            }

            if (lines != null)
                yield return (id, lines);
        }

        record Image(int Id, int Size, string Pixels)
        {
            public char this[(int x, int y) pt] 
                => Pixels[(pt.y * Size) + pt.x];

            public Edges Edges { get; } = Edges.Create(Pixels, Size);

            public Image Transform(bool flipX = false, bool flipY = false, bool rotate = false)
            {
                var result = new StringBuilder(Pixels.Length);

                for (var y = 0; y < Size; ++y)
                {
                    for (var x = 0; x < Size; ++x)
                    {
                        var inY = flipY ? Size - y - 1 : y;
                        var inX = flipX ? Size - x - 1 : x;
                        if (rotate)
                            (inX, inY) = (inY, Size - inX - 1);

                        result.Append(this[(inX, inY)]);
                    }
                }


                return new Image(Id, Size, result.ToString());
            }

            public Image AlignWith(string left, string top)
                => EnumVariations().First(v => (left == null || v.Edges.Left == left)
                                            && (top == null || v.Edges.Top == top));

            private IEnumerable<Image> EnumVariations()
            {
                yield return this;
                yield return Transform(flipX: true);
                yield return Transform(flipY: true);
                yield return Transform(flipX: true, flipY: true);                
                yield return Transform(rotate: true);
                yield return Transform(rotate: true, flipX: true);
                yield return Transform(rotate: true, flipY: true);
                yield return Transform(rotate: true, flipX: true, flipY: true);
            }

            public void Dump()
            {
                for (var y = 0; y < Size; ++y)
                    Console.WriteLine(Pixels.Substring(y * Size, Size));

                Console.WriteLine();
            }
        }

        record Edges(string Left, string Top, string Right, string Bottom)
        {
            public string LeftFlipped { get; } = Reverse(Left);
            public string TopFlipped { get; } = Reverse(Top);
            public string RightFlipped { get; } = Reverse(Right);
            public string BottomFlipped { get; } = Reverse(Bottom);

            public IEnumerable<string> All
            {
                get
                {
                    yield return Left;
                    yield return LeftFlipped;
                    yield return Top;
                    yield return TopFlipped;
                    yield return Right;
                    yield return RightFlipped;
                    yield return Bottom;
                    yield return BottomFlipped;
                }
            }

            public static Edges Create(string pixels, int size)
            {
                return new Edges(
                    Left: GetColumn(pixels, size, 0),
                    Top: pixels[0..size],
                    Right: GetColumn(pixels, size, size - 1),
                    Bottom: pixels[^size..^0]);
            }

            private static string GetColumn(string pixels, int size, int x)
            {
                var result = new StringBuilder(size);
                for (var i = x; i < pixels.Length; i += size)
                    result.Append(pixels[i]);

                return result.ToString();
            }

            private static string Reverse(string value)
            {
                var result = new StringBuilder(value.Length);
                for (var i = value.Length - 1; i >= 0; --i)
                    result.Append(value[i]);

                return result.ToString();
            }
        }
    }
}
