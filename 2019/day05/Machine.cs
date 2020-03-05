using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace day05
{
    public class Machine
    {
        private readonly int[] memory;

        public Machine(ReadOnlySpan<int> memory)
        {
            this.memory = memory.ToArray();
        }

        public int InstructionCounter { get; set; }

        public int this[int address] => memory[address];

        public ReadOnlyMemory<int> Memory => memory.AsMemory();

        protected virtual int ReadInput()
        {
            var line = Console.ReadLine();
            return int.Parse(line);
        }

        protected virtual void WriteOutput(int value)
        {
            Console.WriteLine(value);
        }

        public void Execute()
        {
#if true
            while (true)
            {
                var op = new Instruction(memory, InstructionCounter);
                if (!operationMap.TryGetValue(op.OpCode, out var handler) || handler == null)
                    throw new InvalidOperationException("Invalid instruction");

                var nextInstructionOffset = handler(op);
                if (nextInstructionOffset <= 0)
                    break;

                InstructionCounter += nextInstructionOffset;
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
            { 99, _ => 0 }
        }.ToImmutableDictionary();

        private static int DoAdd(Instruction op)
        {
            var param1 = op.ReadArgument(0);
            var param2 = op.ReadArgument(1);

            var result = param1 + param2;

            op.WriteResult(2, result);

            return 4;
        }

        private static int DoMultiply(Instruction op)
        {
            var param1 = op.ReadArgument(0);
            var param2 = op.ReadArgument(1);

            var result = param1 * param2;

            op.WriteResult(2, result);

            return 4;
        }

        readonly struct Instruction
        {
            private const int OpcodeLimit = 100;

            private readonly int[] memory;
            private readonly int position;

            public Instruction(int[] memory, int position)
            {
                this.memory = memory;
                this.position = position;
            }

            public int OpCode => memory[position] % OpcodeLimit;

            public int ReadArgument(int argumentOffset)
            {
                if (argumentOffset < 0)
                    throw new ArgumentOutOfRangeException(nameof(argumentOffset));

                var value = memory[position + argumentOffset + 1];
                //var mode = (memory[position] / (OpcodeLimit * argumentOffset)) % 10;
                return IsImmediate(argumentOffset)
                    ? value
                    : memory[value];
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

                return ((memory[position] / digit) % 10) == 1;
            }

            public void WriteResult(int argumentOffset, int value)
            {
                if (argumentOffset < 0)
                    throw new ArgumentOutOfRangeException(nameof(argumentOffset));

                var address = memory[position + argumentOffset + 1];
                memory[address] = value;
            }
        }
    }
}