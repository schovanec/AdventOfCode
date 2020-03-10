using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace day13
{
    public sealed class IntcodeMachine
    {
        private readonly long[] memory;
        private readonly Func<CancellationToken, ValueTask<long>> readInput;
        private readonly Func<long, CancellationToken, ValueTask> writeOutput;
        private long programCounter = 0;
        private long relativeBaseAddress = 0;

        public const int DefaultMemorySize = 1 << 12;

        private const string ErrorNoInput = "Input stream ended";

        private const long FlagStart = 100;

        private IntcodeMachine(
            ReadOnlySpan<long> program,
            Func<CancellationToken, ValueTask<long>> readInput = null,
            Func<long, CancellationToken, ValueTask> writeOutput = null,
            int memorySize = DefaultMemorySize)
        {
            this.memory = new long[Math.Max(program.Length, memorySize)];
            program.CopyTo(memory);

            this.readInput = readInput ?? (t => throw new InvalidOperationException(ErrorNoInput));
            this.writeOutput = writeOutput ?? ((v, t) => default(ValueTask));
        }

        public static long[] ParseCode(string code)
            => (code ?? throw new ArgumentNullException(nameof(code))).Split(',').Select(long.Parse).ToArray();

        public static async Task RunProgramAsync(
            long[] program,
            Func<CancellationToken, ValueTask<long>> read,
            Func<long, CancellationToken, ValueTask> write,
            CancellationToken cancellationToken = default)
        {
            var vm = new IntcodeMachine(program, read, write);
            await vm.StepUntilHalted(cancellationToken);
        }

        public static Task RunProgramAsync(
            long[] program,
            Func<long> read,
            Action<long> write,
            CancellationToken cancellationToken = default)
        {
            return RunProgramAsync(
                program,
                _ => new ValueTask<long>(read()),
                (v, _) => { write(v); return default; },
                cancellationToken);
        }

        public static void RunProgram(long[] program, Func<long> read, Action<long> write)
            => RunProgramAsync(program, read, write).Wait();

        public static async Task<List<long>> RunProgramAsync(
            long[] program, IEnumerable<long> input, CancellationToken cancellationToken = default)
        {
            using var stream = input?.GetEnumerator() ?? throw new ArgumentNullException(nameof(input));
            var output = new List<long>();
            await RunProgramAsync(
                program,
                () => stream.MoveNext() ? stream.Current : throw new InvalidOperationException(),
                output.Add,
                cancellationToken
            );

            return output;
        }

        public static List<long> RunProgram(long[] program, IEnumerable<long> input, CancellationToken cancellationToken = default)
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

                case Instruction.OffsetRelativeBase:
                    return DoOffsetRelativeBase();

                case Instruction.Halt:
                    return default;

                default:
                    throw new InvalidOperationException("Unknown instruction");
            }
        }

        private ValueTask DoBinaryInstruction(Func<long, long, long> func)
        {
            var a = GetParameter(1);
            var b = GetParameter(2);

            var result = func(a, b);

            ref var target = ref GetParameter(3);
            target = result;

            programCounter += 4;

            return default;
        }

        private ValueTask DoConditionalJump(Predicate<long> condition)
        {
            var a = GetParameter(1);

            if (condition(a))
                programCounter = GetParameter(2);
            else
                programCounter += 3;

            return default;
        }

        private ValueTask DoOffsetRelativeBase()
        {
            relativeBaseAddress += GetParameter(1);
            programCounter += 2;
            return default;
        }

        private async ValueTask ReadInput(CancellationToken cancellationToken)
        {
            var inputValue = await readInput(cancellationToken);

            void WriteOutput(long value)
            {
                ref var target = ref GetParameter(1);
                target = inputValue;
            }

            WriteOutput(inputValue);
            programCounter += 2;
        }

        private async ValueTask WriteOutput(CancellationToken cancellationToken)
        {
            var outputValue = GetParameter(1);
            await writeOutput(outputValue, cancellationToken);

            programCounter += 2;
        }

        private Instruction CurrentInstruction
            => (Instruction)(memory[programCounter] % FlagStart);

        private ref long GetParameter(long index)
        {
            ref var parameter = ref memory[programCounter + index];
            switch (GetParameterMode(index))
            {
                case Mode.Immediate:
                    return ref parameter;

                case Mode.Indirect:
                    return ref memory[parameter];

                case Mode.Relative:
                    return ref memory[relativeBaseAddress + parameter];

                default:
                    throw new InvalidOperationException("Invalid parameter mode");
            }
        }

        private Mode GetParameterMode(long index)
            => GetParameterFlags(index) switch
            {
                0 => Mode.Indirect,
                1 => Mode.Immediate,
                2 => Mode.Relative,
                _ => throw new InvalidOperationException("Invalid paramter flags")
            };

        private long GetParameterFlags(long index)
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
            OffsetRelativeBase = 9,
            Halt = 99
        }

        private enum Mode
        {
            Immediate,
            Indirect,
            Relative
        }
    }
}