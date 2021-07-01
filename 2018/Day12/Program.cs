using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace Day12
{
    class Program
    {
        static void Main(string[] args)
        {
#if false
            var (initialState, notes) = ParseInput(File.ReadAllLines("example1.txt"));
#else
            var (initialState, notes) = ParseInput(File.ReadAllLines(args.DefaultIfEmpty("input.txt").First()));
#endif

            var result1 = initialState;
            for (var i = 0; i < 20; ++i)
                result1 = Simulate(result1, notes);

            Console.WriteLine($"Part 1 Result = {result1.Sum()}");

            var result2 = initialState;
            for (var i = 0L; i < 50000000000L; ++i)
                result2 = Simulate(result2, notes);

            Console.WriteLine($"Part 1 Result = {result2.Sum()}");
        }

        public static ImmutableSortedSet<long> Simulate(ImmutableSortedSet<long> currentState, ImmutableHashSet<long> notes)
        {
            var min = currentState.Min();
            var max = currentState.Max() + 4;

            var value = 0L;
            var result = currentState.ToBuilder();
            for (var i = min; i <= max; ++i)
            {
                value = ((value << 1) & 0b11111) | (currentState.Contains(i) ? 1L : 0L);

                if (notes.Contains(value))
                    result.Add(i - 2L);
                else
                    result.Remove(i - 2L);
            }

            return result.ToImmutable();
        }

        public static (ImmutableSortedSet<long> state, ImmutableHashSet<long> notes) ParseInput(string[] input)
        {
            var initialState = input.First()
                                    .Skip(15)
                                    .Select((ch, i) => (value: ch, index: i))
                                    .Where(x => x.value == '#')
                                    .Select(x => (long)x.index)
                                    .ToImmutableSortedSet();

            var notes = ImmutableHashSet.CreateBuilder<long>();
            foreach (var note in input.Skip(2))
            {
                var split = note.Split(" => ", 2);
                if (split[1] == "#")
                {
                    var id = split[0].Reverse()
                                     .Select((ch, i) => ch switch { '#' => 1 << i, _ => 0 })
                                     .Aggregate((a, b) => a | b);
                    
                    notes.Add(id);
                }
            }

            return (initialState, notes.ToImmutable());
        }
    }
}
