using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace Day17
{
    class Program
    {
        static void Main(string[] args)
        {
            var file = args.DefaultIfEmpty("input.txt").First();
            var input = ParseInput(File.ReadLines(file));

            var result1 = RunSimulation(input, 3);
            Console.WriteLine($"Part 1 Result = {result1}");

            var result2 = RunSimulation(input, 4);
            Console.WriteLine($"Part 2 Result = {result2}");
        }

        private static int RunSimulation(ImmutableHashSet<(int x, int y, int z, int w)> input, int dimensions, int steps = 6)
        {
            var space = new PocketDimension(input, dimensions);
            for (var i = 0; i < steps; ++i)
                space = Step(space);

            return space.ActiveCubes.Count;
        }

        static PocketDimension Step(PocketDimension initial)
        {
            var result = initial.ActiveCubes.ToBuilder();

            foreach (var activeCube in initial.ActiveCubes)
            {
                var activeNeighbourCount = initial.GetNeighbours(activeCube).Count(pt => initial.ActiveCubes.Contains(pt));
                if (activeNeighbourCount < 2 || activeNeighbourCount > 3)
                    result.Remove(activeCube);
            }

            foreach (var inactiveCube in initial.GetAllInactiveNeighbours())
            {
                var activeNeighbourCount = initial.GetNeighbours(inactiveCube).Count(pt => initial.ActiveCubes.Contains(pt));
                if (activeNeighbourCount == 3)
                    result.Add(inactiveCube);
            }

            return initial with { ActiveCubes = result.ToImmutable() };
        }

        static ImmutableHashSet<(int x, int y, int z, int w)> ParseInput(IEnumerable<string> input)            
        {
            var result = ImmutableHashSet.CreateBuilder<(int x, int y, int z, int w)>();
            var y = 0;
            foreach (var line in input)
            {
                for (var x = 0; x < line.Length; ++x)
                {
                    if (line[x] == '#')
                        result.Add((x, y, 0, 0));
                }

                ++y;
            }

            return result.ToImmutable();
        }

        record PocketDimension(ImmutableHashSet<(int x, int y, int z, int w)> ActiveCubes, int Dimensions)
        {
            private static readonly ImmutableArray<(int x, int y, int z, int w)> neighbourOffsets
                = ImmutableArray.CreateRange(from x in Enumerable.Range(-1, 3)
                                             from y in Enumerable.Range(-1, 3)
                                             from z in Enumerable.Range(-1, 3)
                                             from w in Enumerable.Range(-1, 3)
                                             let p = (x, y, z, w)
                                             where p != (0, 0, 0, 0)
                                             select p);

            public ImmutableHashSet<(int x, int y, int z, int w)> GetAllInactiveNeighbours()
                => ActiveCubes.SelectMany(GetNeighbours)
                              .Where(pt => !ActiveCubes.Contains(pt))
                              .ToImmutableHashSet();

            public IEnumerable<(int x, int y, int z, int w)> GetNeighbours((int x, int y, int z, int w) pt)
                => neighbourOffsets.Where(o => o.w == 0 || Dimensions == 4)
                                   .Select(o => (pt.x + o.x, pt.y + o.y, pt.z + o.z, pt.w + o.w));
        }
    }
}
