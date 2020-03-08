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
            var program = File.ReadLines(args.FirstOrDefault() ?? "input.txt")
                .First()
                .Split(',')
                .Select(int.Parse)
                .ToArray();

            // Part 1
            var maxThrustValue = EnumAllPermutations(Enumerable.Range(0, 5))
                .Max(x => CalculateOutputSignal(x, program));

            Console.WriteLine($"Part 1 Result: {maxThrustValue}");

            // Part 2
            var maxFeedbackThrustValue = await EnumAllPermutations(Enumerable.Range(5, 5))
                .ToAsyncEnumerable()
                .MaxAwaitAsync(x => CalculateOutputSignalWithFeedbackAsync(x, program));

            Console.WriteLine($"Part 2 Result: {maxFeedbackThrustValue}");
        }

        private static int CalculateOutputSignal(IEnumerable<int> phaseSettingInputs, int[] program)
            => phaseSettingInputs.Aggregate(0, (prev, phase) => IntcodeMachine.RunProgram(program, new[] { phase, prev }).Single());

        private static async ValueTask<int> CalculateOutputSignalWithFeedbackAsync(IEnumerable<int> phaseSettingInputs, int[] program)
        {
            var channel = await Task.WhenAll(phaseSettingInputs.Select(CreateAmplifierInputChannel));
            var count = channel.Length;
            var amp = new Task[count];
            for (int i = 0; i < count; ++i)
            {
                // get this amplifier's input
                var input = channel[i];

                // connect the output to the next amplifier's input
                var output = channel[(i + 1) % channel.Length];

                // run the program
                amp[i] = IntcodeMachine.RunProgramAsync(program, input.Reader, output.Writer);
            }

            // send initial input to the first amp
            await channel.First().Writer.WriteAsync(0);

            // wait for all of the programs to finish
            await Task.WhenAll(amp);

            // retrieve the final output from the input of the first amp
            return await channel.First().Reader.ReadAsync();
        }

        private static async Task<Channel<int>> CreateAmplifierInputChannel(int phase)
        {
            var result = Channel.CreateUnbounded<int>();
            await result.Writer.WriteAsync(phase);
            return result;
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
