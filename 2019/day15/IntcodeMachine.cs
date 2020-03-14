using System;
using System.Collections.Generic;
using System.Linq;

namespace day15
{
    public sealed class IntcodeMachine
    {
        private readonly long[] memory;
        private long programCounter = 0;
        private long relativeBaseAddress = 0;

        public const int DefaultMemorySize = 1 << 12;

        private const long FlagStart = 100;

        public IntcodeMachine(
            ReadOnlySpan<long> program,
            int memorySize = DefaultMemorySize)
        {
            this.memory = new long[Math.Max(program.Length, memorySize)];
            program.CopyTo(memory);
        }

        public static long[] ParseCode(string code)
            => (code ?? throw new ArgumentNullException(nameof(code))).Split(',').Select(long.Parse).ToArray();

        public bool IsHalted => CurrentInstruction == Instruction.Halt;

        public ExecuteResult StepMany(Queue<long> input)
        {
            while (true)
            {
                var result = Step(input);
                if (result.State != ExecuteState.Running)
                    return result;
            }
        }

        public ExecuteResult StepMany(long input)
        {
            var inputQueue = new Queue<long>();
            inputQueue.Enqueue(input);
            return StepMany(inputQueue);
        }

        public ExecuteResult Step(Queue<long> input)
        {
            switch (CurrentInstruction)
            {
                case Instruction.Add:
                    return DoBinaryInstruction((x, y) => x + y);

                case Instruction.Multiply:
                    return DoBinaryInstruction((x, y) => x * y);

                case Instruction.ReadInput:
                    return ReadInput(input);

                case Instruction.WriteOutput:
                    return WriteOutput();

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
                    return ExecuteResult.Halted;

                default:
                    throw new InvalidOperationException("Unknown instruction");
            }
        }

        private ExecuteResult DoBinaryInstruction(Func<long, long, long> func)
        {
            var a = GetParameter(1);
            var b = GetParameter(2);

            var result = func(a, b);

            ref var target = ref GetParameter(3);
            target = result;

            programCounter += 4;

            return ExecuteResult.Running;
        }

        private ExecuteResult DoConditionalJump(Predicate<long> condition)
        {
            var a = GetParameter(1);

            if (condition(a))
                programCounter = GetParameter(2);
            else
                programCounter += 3;

            return ExecuteResult.Running;
        }

        private ExecuteResult DoOffsetRelativeBase()
        {
            relativeBaseAddress += GetParameter(1);
            programCounter += 2;
            return ExecuteResult.Running;
        }

        private ExecuteResult ReadInput(Queue<long> input)
        {
            if (input.Count == 0)
                return ExecuteResult.NeedInput;

            var inputValue = input.Dequeue();

            ref var target = ref GetParameter(1);
            target = inputValue;

            programCounter += 2;

            return ExecuteResult.Running;
        }

        private ExecuteResult WriteOutput()
        {
            var outputValue = GetParameter(1);
            programCounter += 2;

            return ExecuteResult.HaveOutput(outputValue);
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

    public enum ExecuteState
    {
        Running,
        Halted,
        NeedInput,
        HaveOutput
    }

    public struct ExecuteResult
    {
        public static readonly ExecuteResult Running = new ExecuteResult(ExecuteState.Running);
        public static readonly ExecuteResult Halted = new ExecuteResult(ExecuteState.Halted);
        public static readonly ExecuteResult NeedInput = new ExecuteResult(ExecuteState.NeedInput);

        public ExecuteResult(ExecuteState state, long? output = default)
        {
            State = state;
            Output = output;
        }

        public ExecuteState State { get; }

        public long? Output { get; }

        public static ExecuteResult HaveOutput(long value)
            => new ExecuteResult(ExecuteState.HaveOutput, value);
    }
}