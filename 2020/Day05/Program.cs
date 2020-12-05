    using System;
    using System.IO;
    using System.Linq;

    namespace Day05
    {
        class Program
        {
            static void Main(string[] args)
            {
                var file = args.DefaultIfEmpty("input.txt").First();

                var seats = File.ReadLines(file)
                                .Select(x => x.Aggregate(0, (v, c) => (v << 1) + c switch { 'B' or 'R' => 1, _ => 0 }))
                                .OrderByDescending(id => id)
                                .ToList();

                var max = seats.First();
                Console.WriteLine($"Part 1 Result: {max}");

                var missing = seats.Zip(seats.Skip(1), (a, b) => (a, b))
                                   .Where(x => x.a - x.b > 1)
                                   .Select(x => x.b + 1)
                                   .First();
                Console.WriteLine($"Part 2 Result: {missing}");
            }
        }
    }
