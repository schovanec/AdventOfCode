using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace day07
{
    public sealed class IntcodeMachine
    {
        private readonly int[] memory;
        private readonly ChannelReader<int> input;
        private readonly ChannelWriter<int> output;
        private int programCounter = 0;

        private const int FlagStart = 100;

        private IntcodeMachine(ReadOnlySpan<int> program, ChannelReader<int> input, ChannelWriter<int> output)
        {
            this.memory = program?.ToArray() ?? throw new ArgumentNullException(nameof(program));
            this.input = input ?? throw new ArgumentNullException(nameof(input));
            this.output = output ?? throw new ArgumentNullException(nameof(output));
        }

        public static async Task RunProgramAsync(
            int[] program, ChannelReader<int> input, ChannelWriter<int> output,
            CancellationToken cancellationToken = default, bool complete = true)
        {
            try
            {
                var vm = new IntcodeMachine(program, input, output);
                await vm.StepUntilHalted(cancellationToken);
            }
            catch (Exception ex)
            {
                if (complete)
                    output.TryComplete(ex);
            }
            finally
            {
                if (complete)
                    output.TryComplete();
            }
        }

        public static async Task<List<int>> RunProgramAsync(
            int[] program, IEnumerable<int> input, CancellationToken cancellationToken = default)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            // set up the input
            var inputChannel = Channel.CreateUnbounded<int>();
            foreach (var value in input)
                await inputChannel.Writer.WriteAsync(value);

            // run the program and capture output
            var outputChannel = Channel.CreateUnbounded<int>();
            await RunProgramAsync(program, inputChannel.Reader, outputChannel.Writer, cancellationToken);

            // read all of the output
            outputChannel.Writer.TryComplete();
            return await outputChannel.Reader.ReadAllAsync().ToListAsync();
        }

        public static List<int> RunProgram(int[] program, IEnumerable<int> input, CancellationToken cancellationToken = default)
            => RunProgramAsync(program, input, cancellationToken).Result;

        public bool IsHalted => CurrentInstruction == Instruction.Halt;

        private async ValueTask StepUntilHalted(CancellationToken cancellationToken = default)
        {
            while (!IsHalted)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await Step(cancellationToken);
            }
        }

        public ValueTask Step(CancellationToken cancellationToken = default)
        {
            switch (CurrentInstruction)
            {
                case Instruction.Add:
                    return DoBinaryInstruction((x, y) => x + y);

                case Instruction.Multiply:
                    return DoBinaryInstruction((x, y) => x * y);

                case Instruction.ReadInput:
                    return ReadInput(cancellationToken);

                case Instruction.WriteOutput:
                    return WriteOutput(cancellationToken);

                case Instruction.JumpIfTrue:
                    return DoConditionalJump(x => x != 0);

                case Instruction.JumpIfFalse:
                    return DoConditionalJump(x => x == 0);

                case Instruction.CompareLess:
                    return DoBinaryInstruction((x, y) => x < y ? 1 : 0);

                case Instruction.CompareEqual:
                    return DoBinaryInstruction((x, y) => x == y ? 1 : 0);

                case Instruction.Halt:
                    return default;

                default:
                    throw new InvalidOperationException("Unknown instruction");
            }
        }

        private ValueTask DoBinaryInstruction(Func<int, int, int> func)
        {
            var a = GetParameterValue(1);
            var b = GetParameterValue(2);

            var result = func(a, b);

            var address = GetParameterRaw(3);
            memory[address] = result;

            programCounter += 4;

            return default;
        }

        private ValueTask DoConditionalJump(Predicate<int> condition)
        {
            var a = GetParameterValue(1);

            if (condition(a))
                programCounter = GetParameterValue(2);
            else
                programCounter += 3;

            return default;
        }

        private async ValueTask ReadInput(CancellationToken cancellationToken)
        {
            if (input == null)
                throw new InvalidOperationException("No input available");

            var inputValue = await input.ReadAsync(cancellationToken);

            var address = GetParameterRaw(1);
            memory[address] = inputValue;

            programCounter += 2;
        }

        private async ValueTask WriteOutput(CancellationToken cancellationToken)
        {
            var outputValue = GetParameterValue(1);
            await output.WriteAsync(outputValue, cancellationToken);

            programCounter += 2;
        }

        private Instruction CurrentInstruction
            => (Instruction)(memory[programCounter] % FlagStart);

        private int GetParameterValue(int index)
            => GetParameterMode(index) switch
            {
                Mode.Immediate => GetParameterRaw(index),
                Mode.Relative => memory[GetParameterRaw(index)],
                _ => throw new InvalidOperationException("Invalid perameter mode")
            };

        private int GetParameterRaw(int index)
            => memory[programCounter + index];

        private Mode GetParameterMode(int index)
            => GetParameterFlags(index) == 1
                ? Mode.Immediate
                : Mode.Relative;

        private int GetParameterFlags(int index)
            => index switch
            {
                1 => (memory[programCounter] / (FlagStart * 1)) % 10,
                2 => (memory[programCounter] / (FlagStart * 10)) % 10,
                3 => (memory[programCounter] / (FlagStart * 100)) % 10,
                _ => throw new ArgumentOutOfRangeException(nameof(index), index, "Invalid parameter index")
            };

        private enum Instruction
        {
            Add = 1,
            Multiply = 2,
            ReadInput = 3,
            WriteOutput = 4,
            JumpIfTrue = 5,
            JumpIfFalse = 6,
            CompareLess = 7,
            CompareEqual = 8,
            Halt = 99
        }

        private enum Mode
        {
            Immediate,
            Relative
        }
    }
}