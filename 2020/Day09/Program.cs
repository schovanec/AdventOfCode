using System;
using System.IO;
using System.Linq;

namespace Day09
{
    class Program
    {
        static void Main(string[] args)
        {
            var file = args.DefaultIfEmpty("input.txt").First();
            var numbers = File.ReadLines(file).Select(long.Parse).ToArray();         

            var part1Result = FindFirstInvalid(numbers);
            Console.WriteLine($"Part 1 Result = {part1Result}");

            var range = FindContiguousSet(part1Result, numbers);
            var part2Result = range.Min() + range.Max();
            Console.WriteLine($"Part 2 Result = {part2Result}");
        }

        static long FindFirstInvalid(ReadOnlySpan<long> numbers, int size = 25)
        {
            for (var i = size; i < numbers.Length; ++i)
            {
                if (!HasSumOfTwo(numbers[i], numbers.Slice(i - size, size)))
                    return numbers[i];
            }

            return 0;
        }

        static bool HasSumOfTwo(long target, ReadOnlySpan<long> numbers)
        {
            for (var i = 0; i < numbers.Length; ++i)
            {
                for (var j = i + 1; j < numbers.Length; ++j)
                {
                    if (numbers[i] + numbers[j] == target)
                        return true;
                }
            }

            return false;
        }

        static long[] FindContiguousSet(long target, long[] numbers)
        {
            var i = 0; 
            var j = 0;
            var sum = numbers[0];
            while (sum != target)
            {
                if (sum < target)
                {
                    ++j;
                    sum += numbers[j];                    
                }
                else if (sum > target)
                {
                    sum -= numbers[i];
                    ++i;
                }
            }

            return numbers[i..(j+1)];
        }
    }
}
