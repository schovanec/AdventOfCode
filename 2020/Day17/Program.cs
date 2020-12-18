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
            var dimension = PocketDimension.Parse(File.ReadLines(file));

            var result = dimension;
            for (var i = 0; i < 6; ++i)
                result = Step(result);

            Console.WriteLine($"Part 1 Result = {result.ActiveCubes.Count}");
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

            return new PocketDimension(result.ToImmutable());
        }

        record PocketDimension(ImmutableHashSet<(int x, int y, int z)> ActiveCubes)
        {
            public ImmutableHashSet<(int x, int y, int z)> GetAllInactiveNeighbours()
                => ActiveCubes.SelectMany(GetNeighbours)
                              .Where(pt => !ActiveCubes.Contains(pt))
                              .ToImmutableHashSet();

            public IEnumerable<(int x, int y, int z)> GetNeighbours((int x, int y, int z) pt)
            {
                for (var x = pt.x - 1; x <= pt.x + 1; ++x)
                {
                    for (var y = pt.y - 1; y <= pt.y + 1; ++y)
                    {
                        for (var z = pt.z - 1; z <= pt.z + 1; ++z)
                        {
                            if ((x, y, z) != pt)                            
                                yield return (x, y, z);
                        }
                    }
                }
            }

            public static PocketDimension Parse(IEnumerable<string> input)            
            {
                var result = ImmutableHashSet.CreateBuilder<(int x, int y, int z)>();
                var y = 0;
                foreach (var line in input)
                {
                    for (var x = 0; x < line.Length; ++x)
                    {
                        if (line[x] == '#')
                            result.Add((x, y, 0));
                    }

                    ++y;
                }

                return new PocketDimension(result.ToImmutable());
            }
        }
    }
}
