using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace day05
{
    public sealed class Machine
    {
        private readonly int[] memory;

        public Machine(IEnumerable<int> memory)
        {
            this.memory = memory.ToArray();
        }

        public int InstructionCounter { get; set; }

        public int this[int address] => memory[address];

        public ReadOnlyMemory<int> Memory => memory.AsMemory();

        public void Execute(Func<int> read = null, Action<int> write = null)
        {
#if true
            var state = new State(memory, write, read);

            while (true)
            {
                var op = new Instruction(state, InstructionCounter);
                if (!operationMap.TryGetValue(op.OpCode, out var handler) || handler == null)
                    throw new InvalidOperationException("Invalid instruction");

                var nextInstructionOffset = handler(op);
                if (nextInstructionOffset < 0)
                    break;

                InstructionCounter += nextInstructionOffset + 1;
            }
#else
            int ip = 0;
            while (true)
            {
                var op = memory[ip];
                if (op == OperationHalt)
                    break;

                var param1 = memory[memory[ip + OffsetParam1]];
                var param2 = memory[memory[ip + OffsetParam2]];

                int result;
                switch (op)
                {
                    case OperationAdd:
                        result = param1 + param2;
                        break;

                    case OperationMultiply:
                        result = param1 * param2;
                        break;

                    default:
                        throw new InvalidOperationException("Invalid Operation!");
                }

                memory[memory[ip + OffsetOutput]] = result;

                ip += InstructionSize;
            }
#endif
        }

        private static readonly ImmutableDictionary<int, Func<Instruction, int>> operationMap = new Dictionary<int, Func<Instruction, int>>()
        {
            { 1, DoAdd },
            { 2, DoMultiply },
            { 3, DoInput },
            { 4, DoOutput },
            { 99, _ => -1 }
        }.ToImmutableDictionary();

        private static int DoAdd(Instruction op)
        {
            var param1 = op.ReadArgument(0);
            var param2 = op.ReadArgument(1);

            var result = param1 + param2;

            op.WriteResult(2, result);

            return 3;
        }

        private static int DoMultiply(Instruction op)
        {
            var param1 = op.ReadArgument(0);
            var param2 = op.ReadArgument(1);

            var result = param1 * param2;

            op.WriteResult(2, result);

            return 3;
        }

        private static int DoInput(Instruction op)
        {
            var value = op.ReadInput();
            op.WriteResult(0, value);

            return 1;
        }

        private static int DoOutput(Instruction op)
        {
            var value = op.ReadArgument(0);
            op.WriteOutput(value);

            return 1;
        }

        private sealed class State
        {
            public State(int[] memory, Action<int> write = null, Func<int> read = null)
            {
                Memory = memory;
                Write = write ?? (_ => { });
                Read = read ?? (() => throw new InvalidOperationException("No input available"));
            }

            public int[] Memory { get; }

            public Action<int> Write { get; }

            public Func<int> Read { get; }
        }

        readonly struct Instruction
        {
            private const int OpcodeLimit = 100;

            private readonly State state;

            private readonly int ip;

            public Instruction(State state, int ip)
            {
                this.state = state;
                this.ip = ip;
            }

            private int[] Memory => state.Memory;

            public int OpCode => Memory[ip] % OpcodeLimit;

            public int ReadArgument(int argumentOffset)
            {
                if (argumentOffset < 0)
                    throw new ArgumentOutOfRangeException(nameof(argumentOffset));

                var value = Memory[ip + argumentOffset + 1];
                return IsImmediate(argumentOffset)
                    ? value
                    : Memory[value];
            }

            private bool IsImmediate(int argumentOffset)
            {
                var digit = argumentOffset switch
                {
                    0 => OpcodeLimit,
                    1 => OpcodeLimit * 10,
                    2 => OpcodeLimit * 100,
                    3 => OpcodeLimit * 1000,
                    _ => throw new ArgumentException("Invalid Argument", nameof(argumentOffset))
                };

                return ((Memory[ip] / digit) % 10) == 1;
            }

            public void WriteResult(int argumentOffset, int value)
            {
                if (argumentOffset < 0)
                    throw new ArgumentOutOfRangeException(nameof(argumentOffset));

                var address = Memory[ip + argumentOffset + 1];
                Memory[address] = value;
            }

            public void WriteOutput(int output) => state.Write(output);

            public int ReadInput() => state.Read();
        }
    }
}