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
                           let num = long.Parse(line)
                           orderby num
                           select num).ToArray();

            const long target = 2020;

            var resultOf2 = FindSumOfTwo(target, numbers) ?? (0, 0);
            Console.WriteLine($"For 2 numbers:");
            Console.WriteLine($"Found: {resultOf2}");
            Console.WriteLine($"Product: {resultOf2.a * resultOf2.b}");

            var resultOf3 = FindSumOfThree(target, numbers) ?? (0, 0, 0);
            Console.WriteLine($"For 3 numbers:");
            Console.WriteLine($"Found: {resultOf3}");
            Console.WriteLine($"Product: {resultOf3.a * resultOf3.b * resultOf3.c}");
        }

        static (long a, long b)? FindSumOfTwo(long target, long[] numbers)
            => FindSumOfTwo(target, numbers, 0, numbers.Length);

        static (long a, long b)? FindSumOfTwo(long target, long[] numbers, int startIndex, int count)
        {
            for (int i = 0; i < count; ++i)
            {
                var current = numbers[i + startIndex];
                var diff = target - current;
                
                var pos = Array.BinarySearch(numbers, i + startIndex + 1, count - i - 1, diff);
                if (pos >= 0)
                    return (current, diff);
            }

            return default;
        }

        static (long a, long b, long c)? FindSumOfThree(long target, long[] numbers)
            => FindSumOfThree(target, numbers, 0, numbers.Length);

        static (long a, long b, long c)? FindSumOfThree(long target, long[] numbers, int startIndex, int count)
        {
            for (int i = 0; i < count; ++i)
            {
                var current = numbers[i + startIndex];
                var diff = target - current;

                var pos = Array.BinarySearch(numbers, i + startIndex + 1, count - i - 1, diff);
                if (pos < 0)
                    pos = ~pos;
                else
                    ++pos;
                
                var pair = FindSumOfTwo(diff, numbers, i + 1, pos - i - 1);
                if (pair.HasValue)
                    return (pair.Value.a, pair.Value.b, current);
            }

            return default;
        }
    }
}
