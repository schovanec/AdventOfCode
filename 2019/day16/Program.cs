using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;

namespace day16
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Any())
            {
                var input = File.ReadLines(args.First()).First();
                var result = FlawedFrequencyTransmission(input);
                Console.WriteLine($"Part 1 Result = {result.Substring(0, 8)}");

                string message = ComputeMessage(input);
                Console.WriteLine($"Part 2 Result = {message}");
            }
            else
            {
                foreach (var test in partOneTestCases)
                {
                    var result = FlawedFrequencyTransmission(test.input).Substring(0, 8);
                    var passedPartOne = result == test.result;
                    Console.WriteLine($"Part 1: {test.input} => {result} [{(passedPartOne ? "Ok" : "Fail")}]");
                }

                foreach (var test in partTwoTestCases)
                {
                    var result = ComputeMessage(test.input);
                    var passed = result == test.result;
                    Console.WriteLine($"Part 2: {test.input} => {result} [{(passed ? "Ok" : "Fail")}]");
                }
            }
        }

        private static string ComputeMessage(string input)
        {
            var totalLength = input.Length * 10000;
            var offset = int.Parse(input.Substring(0, 7));
            if (offset < totalLength / 2)
                throw new InvalidOperationException("Unexpected offset");

            var bufferSize = totalLength - offset;
            var values = input.Select(ch => ch - '0').ToArray();
            var buffer = new int[bufferSize];
            for (var i = 0; i < buffer.Length; ++i)
                buffer[i] = values[(offset + i) % values.Length];

            for (var i = 0; i < 100; ++i)
            {
                var sum = 0;
                for (var j = bufferSize - 1; j >= 0; --j)
                {
                    sum += buffer[j];
                    buffer[j] = Math.Abs(sum) % 10;
                }
            }

            var output = new StringBuilder(8);
            for (var i = 0; i < 8; ++i)
                output.Append((char)(buffer[i] + '0'));

            return output.ToString();
        }

        private static readonly ImmutableArray<int> basePattern = ImmutableArray.Create(0, 1, 0, -1);

        private static string FlawedFrequencyTransmission(string transmission)
        {
            var input = transmission.Select(x => x - '0').ToArray();
            var output = new int[input.Length];
            for (var i = 0; i < 100; ++i)
            {
                for (var j = 0; j < input.Length; ++j)
                {
                    int value = 0;
                    for (var k = 0; k < input.Length; ++k)
                        value += input[k] * GetPatternDigit(j + 1, k);

                    output[j] = Math.Abs(value) % 10;
                }

                (input, output) = (output, input);
            }

            var result = new StringBuilder(input.Length);
            foreach (var i in input)
                result.Append((char)(i + '0'));

            return result.ToString();
        }

        static int GetPatternDigit(int size, int offset)
        {
            var index = ((offset + 1) / size) % basePattern.Length;
            return basePattern[index];
        }

        public static IEnumerable<int> RepeatInfinite(ImmutableArray<int> source)
        {
            var count = source.Length;
            while (true)
            {
                for (int i = 0; i < count; ++i)
                    yield return source[i];
            }
        }

        public static IEnumerable<int> Repeat(ImmutableArray<int> source, int n)
        {
            var count = source.Length;
            while (n-- > 0)
            {
                for (int i = 0; i < count; ++i)
                    yield return source[i];
            }
        }

        private static readonly ImmutableArray<(string input, string result)> partOneTestCases = ImmutableArray.Create(
            ("80871224585914546619083218645595", "24176176"),
            ("19617804207202209144916044189917", "73745418"),
            ("69317163492948606335995924319873", "52432133")
        );

        private static readonly ImmutableArray<(string input, string result)> partTwoTestCases = ImmutableArray.Create(
            ("03036732577212944063491565474664", "84462026"),
            ("02935109699940807407585447034323", "78725270"),
            ("03081770884921959731165446850517", "53553731")
        );
    }
}
