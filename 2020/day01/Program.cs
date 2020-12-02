using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace day01
{
    class Program
    {
        static void Main(string[] args)
        {
            var numbers = (from line in File.ReadLines(args.DefaultIfEmpty("input.txt").First())
                           where !string.IsNullOrWhiteSpace(line)
                           select long.Parse(line)).ToArray();

            const long target = 2020;

            //var result = FindSumPair(numbers, target);

            for (var count = 2; count <= 3; ++count)
            {
                var result = FindSum(numbers, target, count);

                Console.WriteLine($"For {count} numbers:");
                Console.WriteLine($"Found: {string.Join(", ", result)}");
                Console.WriteLine($"Product: {result.Aggregate((a, b) => a * b)}");
                Console.WriteLine();
            }
        }

        static (long a, long b) FindSumPair(long[] numbers, long target)
        {
            for (int i = 0; i < numbers.Length; ++i)
            {
                for (int j = i+1; j < numbers.Length; ++j)
                {
                    if (numbers[i] + numbers[j] == target)
                        return (numbers[i], numbers[j]);
                }
            }

            return default;
        }

        static ImmutableArray<long> FindSum(long[] numbers, long target, int count, int startIndex = 0)
        {
            if (count <= 0)
            {
                return ImmutableArray<long>.Empty;
            }

            if (count == 1)
            {
                return numbers.Skip(startIndex).Contains(target)
                    ? ImmutableArray.Create(target)
                    : ImmutableArray<long>.Empty;
            }

            for (int i = startIndex; i < numbers.Length; ++i)
            {
                var current = numbers[i];
                var result = FindSum(numbers, target - current, count - 1, i + 1);
                if (!result.IsEmpty)
                    return result.Add(current);
            }

            return ImmutableArray<long>.Empty;
        }
    }
}
