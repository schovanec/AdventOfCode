using System;
using System.Collections.Generic;
using System.Linq;

namespace Day11
{
    class Program
    {
        const int PuzzleInput = 5791;
        const int GridSize = 300;

        static void Main(string[] args)
        {
            var input = args.Select(int.Parse)
                            .DefaultIfEmpty(PuzzleInput)
                            .First();

            var grid = CalculatePower(input);

            var highest1 = FindHighestPowerSquare(grid, 3);
            Console.WriteLine($"Part 1 Result: {highest1}");

            var highest2 = FindHighestPowerSquareAndSize(grid);
            Console.WriteLine($"Part 2 Result: {highest2}");
        }

        private static int[][] CalculatePower(int serial)
        {
            var result = new int[GridSize][];

            for (var x = 1; x <= GridSize; ++x)
            {
                var rackId = x + 10;
                result[x - 1] = new int[GridSize];
                
                for (var y = 1; y <= GridSize; ++y)
                {
                    var power = ((rackId * y) + serial) * rackId;
                    var value = (power / 100) % 10;
                    result[x - 1][y - 1] = value - 5;
                }
            }

            return result;
        }

        private static (int x, int y, int size, int power) FindHighestPowerSquareAndSize(int[][] grid)
        {
            var cache = new Dictionary<(int x, int y, int size), int>();
            return (from size in Enumerable.Range(1, GridSize)
                    let r = FindHighestPowerSquare(grid, size, cache)
                    orderby r.power descending
                    select (r.x, r.y, size, r.power)).First();
        }

        private static (int x, int y, int power) FindHighestPowerSquare(int[][] grid, int size, Dictionary<(int x, int y, int size), int> cache = null)
        {
            cache ??= new Dictionary<(int x, int y, int size), int>();

            return (from x in Enumerable.Range(0, GridSize - size + 1)
                    from y in Enumerable.Range(0, GridSize - size + 1)
                    let p = GetTotalPower(grid, x, y, size, cache)
                    orderby p descending
                    select (x + 1, y + 1, p)).First();
        }
        
        private static int GetTotalPower(int[][] grid, int x, int y, int size, Dictionary<(int x, int y, int size), int> cache)
        {
            if (size == 1)
                return grid[x][y];
            
            if (!cache.TryGetValue((x, y, size), out var result))
            {
                result = GetTotalPower(grid, x, y, size - 1, cache);
                for (var i = 0; i < size; ++i)
                {
                    result += grid[x + size - 1][y + i];

                    if (i < (size - 1))
                        result += grid[x + i][y + size - 1];
                }

                cache.Add((x, y, size), result);
            }

            return result;
        }
    }
}
