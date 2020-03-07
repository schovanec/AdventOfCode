using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace day07
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Any())
            {
                var program = File.ReadLines(args.First()).First().Split(',').Select(int.Parse).ToArray();

                var maxThrustValue = EnumAllPermutations(Enumerable.Range(0, 5))
                    .Select(x => CalculateOutputSignals(x, program))
                    .Max(x => x.Last());

                Console.WriteLine($"Part 1 Result: {maxThrustValue}");

                var maxFeedbackThrustValue = default(int?);
                foreach (var perm in EnumAllPermutations(Enumerable.Range(5, 5)))
                {
                    var thrustValue = await CalculateOutputSignalWithFeedback(perm, program);
                    if (!maxFeedbackThrustValue.HasValue || thrustValue > maxFeedbackThrustValue)
                        maxFeedbackThrustValue = thrustValue;
                }

                Console.WriteLine($"Part 2 Result: {maxFeedbackThrustValue}");
            }
            else
            {
                await RunNormalTests();

                await RunFeedbackTests();
            }
        }

        private static Task RunNormalTests()
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
                Console.WriteLine();
            }

            return Task.CompletedTask;
        }

        private static async Task RunFeedbackTests()
        {
            var tests = new[]
            {
                    new
                    {
                        Inputs = new[] { 9,8,7,6,5 },
                        Program = new [] { 3,26,1001,26,-4,26,3,27,1002,27,2,27,1,27,26,27,4,27,1001,28,-1,28,1005,28,6,99,0,0,5 },
                        ExpectedResult = 139629729
                    },
                    new
                    {
                        Inputs = new[] { 9,7,8,5,6 },
                        Program = new [] { 3,52,1001,52,-5,52,3,53,1,52,56,54,1007,54,5,55,1005,55,26,1001,54,-5,54,1105,1,12,1,53,54,53,1008,54,0,55,1001,55,1,55,2,53,55,53,4,53,1001,56,-1,56,1005,56,6,99,0,0,0,0,10 },
                        ExpectedResult = 18216
                    }
                };

            var counter = 0;
            foreach (var test in tests)
            {
                Console.WriteLine($"Test #{++counter}");

                var result = await CalculateOutputSignalWithFeedback(test.Inputs, test.Program);
                Console.WriteLine($"Result: {result}");
                Console.WriteLine($"Expected: {test.ExpectedResult} [{(result == test.ExpectedResult ? "Ok" : "Fail")}]");
                Console.WriteLine();
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

        private static async Task<int> CalculateOutputSignalWithFeedback(IEnumerable<int> phaseSettingInputs, IEnumerable<int> program, bool debug = false)
        {
            var phaseSettings = phaseSettingInputs.ToArray();
            var channels = phaseSettings.Select(_ => Channel.CreateUnbounded<int>()).ToArray();

            var programs = new List<Task>();
            var count = phaseSettings.Length;
            for (int i = 0; i < count; ++i)
            {
                var phase = phaseSettings[i];
                var input = channels[i];
                var output = channels[(i + 1) % count];

                // write initial input
                await input.Writer.WriteAsync(phase);

                // start program
                var id = i;
                programs.Add(ExecuteProgramAsync(
                    program,
                    input.Reader,
                    output.Writer,
                    debug ? x => Console.WriteLine($"[{id}] <== {x}") : default(Action<int>),
                    debug ? x => Console.WriteLine($"[{id}] ==> {x}") : default(Action<int>)));
            }

            // Write initial input to fist program
            await channels[0].Writer.WriteAsync(0);

            // Wait for all of the programs to halt
            await Task.WhenAll(programs);

            // Get the last output, which will be written to the first channel
            return await channels.First().Reader.ReadAsync();
        }

        private static async Task ExecuteProgramAsync(
            IEnumerable<int> program,
            ChannelReader<int> input, ChannelWriter<int> output,
            Action<int> inputWatcher = null, Action<int> outputWatcher = null)
        {
            try
            {
                Func<Task<int>> reader = async () =>
                {
                    var result = await input.ReadAsync();
                    if (inputWatcher != null)
                        inputWatcher(result);

                    return result;
                };

                Func<int, Task> writer = async x =>
                {
                    if (outputWatcher != null)
                        outputWatcher(x);

                    await output.WriteAsync(x);
                };

                var machine = new Machine(program);
                await machine.ExecuteAsync(reader, writer);

                output.Complete();
            }
            catch (Exception ex)
            {
                output.Complete(ex);
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
