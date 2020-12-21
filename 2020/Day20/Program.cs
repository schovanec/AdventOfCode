using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Day20
{
    class Program
    {
        static void Main(string[] args)
        {
            var file = args.DefaultIfEmpty("input.txt").First();
            var images = ParseInput(File.ReadLines(file)).ToImmutableList();

            var lookup = (from img in images
                          from edge in img.AllEdges
                          select (img, edge)).ToLookup(x => x.edge, x => x.img);

            var corners = (from img in images
                           from edge in img.AllEdges
                           from m in lookup[edge]
                           where m != img
                           group m by img into g
                           where g.Distinct().Count() == 2
                           select g.Key);

            var result1 = corners.Aggregate(1L, (p, x) => p * x.Id);
            Console.WriteLine($"Part 1 Result = {result1}");

            

            // foreach (var img in images)
            // {
            //     Console.WriteLine($"Image {img.Id}");
                
            //     var matches = img.AllEdges
            //                      .SelectMany(x => lookup[x])
            //                      .Where(x => x != img)
            //                      .Select(x => x.Id)
            //                      .Distinct();

            //     foreach (var m in matches)
            //         Console.WriteLine($" - {m}");
            // }

            //var matches = from i in images
            //              from e in i.AllEdges
            //              group i by e into g
            //              where g.Select(i => i.Id).Distinct().Count() > 1
            //              select g;

            // foreach (var g in matches)
            // {
            //     Console.WriteLine("Group:");
            //     foreach (var img in g)
            //         Console.WriteLine($" - {img.Id}");
            // }

            // var size = (int)Math.Sqrt(images.Select(x => x.Id).Distinct().Count());
            // var arranged = Arrange(images, size);

            // var result = (long)arranged[0].Id
            //            * (long)arranged[size - 1].Id
            //            * (long)arranged[^1].Id
            //            * (long)arranged[^size].Id;
            // Console.WriteLine($"Part 1 Result = {result}");
        }

        // static ImmutableList<ImageEdges> Arrange(ImmutableList<ImageEdges> images, int size)
        // {
        // }

        static IEnumerable<ImageEdges> ParseInput(IEnumerable<string> input)
            => from image in SplitImages(input)
               select ImageEdges.Parse(image.id, image.lines);

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

        enum Side { Left, Top, Right, Bottom };

        record ImageEdges(int Id, int Size, int Left, int Top, int Right, int Bottom)
        {
            public IEnumerable<int> AllEdges
            {
                get
                {
                    yield return Left;
                    yield return FlipBits(Left);
                    yield return Top;
                    yield return FlipBits(Top);
                    yield return Right;
                    yield return FlipBits(Right);
                    yield return Bottom;
                    yield return FlipBits(Bottom);
                }
            }

#if false
            public ImageEdges FlipX()
                => this with 
                {
                    Left = Right,
                    Right = Left,
                    Top = FlipBits(Top),
                    Bottom = FlipBits(Bottom)
                };

            public ImageEdges FlipY()
                => this with
                {
                    Left = FlipBits(Left),
                    Right = FlipBits(Right),
                    Top = Bottom,
                    Bottom = Top
                };

            public ImageEdges RotateLeft()
                => this with 
                {
                    Left = FlipBits(Top),
                    Top = Right,
                    Right = FlipBits(Bottom),
                    Bottom = Left
                };

            public IEnumerable<ImageEdges> AllVariations()
            {
                var flips = new ImageEdges[]
                {
                    this,
                    this.FlipX(),
                    this.FlipY(),
                    this.FlipX().FlipY()
                };

                for (var i = 0; i < flips.Length; ++i)
                {
                    var r = flips[i];
                    yield return r;
                    for (var j = 0; j < 3; ++j)
                    {
                        r = r.RotateLeft();
                        yield return r;
                    }
                }
            }
#endif

            public static ImageEdges Parse(int id, List<string> lines)
            {
                var size = Math.Min(lines.Count, lines.Min(x => x.Length));
                int top = 0;
                int bottom = 0;
                int left = 0;
                int right = 0;
                for (var i = 0; i < size; ++i)
                {
                    var mask = 1 << (size - i - 1);

                    top |= lines[0][i] == '#' ? mask : 0;
                    bottom |= lines[size - 1][i] == '#' ? mask : 0;

                    left |= lines[i][0] == '#' ? mask : 0;
                    right |= lines[i][size - 1] == '#' ? mask : 0;
                }

                return new ImageEdges(id, size, left, top, right, bottom);
            }

            private int FlipBits(int edge)
            {
                int result = 0;
                for (var i = 0; i < Size; ++i)
                {
                    result |= (edge & (1 << (Size - i - 1))) != 0 ? 1 << i : 0;
                }

                return result;
            }
        }
    }
}
