using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace day07
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Any())
            {
                var program = File.ReadLines(args.First()).First().Split(',').Select(int.Parse).ToArray();
                var result = EnumAllPermutations(Enumerable.Range(0, 5))
                    .Select(x => CalculateOutputSignals(x, program))
                    .Max(x => x.Last());

                Console.WriteLine($"Result: {result}");
            }
            else
            {
                var tests = new[]
                {
                new
                {
                    Inputs = new[] { 4,3,2,1,0 },
                    Program = new [] { 3,15,3,16,1002,16,10,16,1,16,15,15,4,15,99,0,0 },
                    ExpectedResult = 43210
                },
                new
                {
                    Inputs = new[] { 0,1,2,3,4 },
                    Program = new [] { 3,23,3,24,1002,24,10,24,1002,23,-1,23,101,5,23,23,1,24,23,23,4,23,99,0,0 },
                    ExpectedResult = 54321
                },
                new
                {
                    Inputs = new [] { 1,0,4,3,2 },
                    Program = new [] { 3,31,3,32,1002,32,10,32,1001,31,-2,31,1007,31,0,33,1002,33,7,33,1,33,31,31,1,32,31,31,4,31,99,0,0,0 },
                    ExpectedResult = 65210
                }
            };

                var counter = 0;
                foreach (var test in tests)
                {
                    Console.WriteLine($"Test #{++counter}");

                    var outputs = CalculateOutputSignals(test.Inputs, test.Program).ToArray();
                    Console.WriteLine($"Results: {string.Join(", ", outputs)}");

                    var result = outputs.Last();
                    Console.WriteLine($"Expected: {test.ExpectedResult} [{(result == test.ExpectedResult ? "Ok" : "Fail")}]");
                }
            }
        }

        private static IEnumerable<int> CalculateOutputSignals(IEnumerable<int> phaseSettingInputs, IEnumerable<int> program)
        {
            var lastSignalValue = 0;
            foreach (var phaseValue in phaseSettingInputs)
            {
                var outputs = new List<int>();

                var inputs = new Queue<int>();
                inputs.Enqueue(phaseValue);
                inputs.Enqueue(lastSignalValue);

                var machine = new Machine(program);
                machine.Execute(
                    read: () => inputs.Dequeue(),
                    write: outputs.Add);

                lastSignalValue = outputs.Single();
                yield return lastSignalValue;
            }
        }

        private static IEnumerable<ImmutableArray<int>> EnumAllPermutations(IEnumerable<int> source)
        {
            var buffer = source.ToArray();
            do
            {
                yield return buffer.ToImmutableArray();

            } while (NextPermutation(buffer));
        }

        private static bool NextPermutation(Span<int> numbers)
        {
            if (numbers.Length <= 1)
                return false;

            // find first decreasing element
            var i = numbers.Length - 2;
            while ((i >= 0) && (numbers[i] >= numbers[i + 1]))
                --i;

            if (i >= 0)
            {
                var j = numbers.Length - 1;
                while ((j >= i) && (numbers[j] <= numbers[i]))
                    --j;

                (numbers[i], numbers[j]) = (numbers[j], numbers[i]);
            }

            Reverse(numbers.Slice(i + 1));
            return (i >= 0);
        }

        private static void Reverse(Span<int> numbers)
        {
            if (numbers.Length > 1)
            {
                var (i, j) = (0, numbers.Length - 1);
                while (i < j)
                {
                    (numbers[i], numbers[j]) = (numbers[j], numbers[i]);
                    ++i;
                    --j;
                }
            }
        }
    }
}
