using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace day07
{
    public sealed class Machine
    {
        private readonly int[] memory;

        public Machine(IEnumerable<int> memory)
        {
            this.memory = memory.ToArray();
        }

        public int ProgramCounter { get; set; }

        public int this[int address] => memory[address];

        public ReadOnlyMemory<int> Memory => memory.AsMemory();

        public void Execute(Func<int> read = null, Action<int> write = null)
        {
            var state = new State(memory, write, read);

            while (true)
            {
                var op = new Instruction(state, ProgramCounter);
                var handler = GetHandler(op);

                var result = handler(op);
                if (result.IsHalt)
                    break;

                ProgramCounter = result.GetNextAddress(ProgramCounter);
            }
        }

        private static Func<Instruction, OperationResult> GetHandler(Instruction op)
            => op.OpCode switch
            {
                1 => op => DoBinaryOperation(op, (a, b) => a + b),
                2 => op => DoBinaryOperation(op, (a, b) => a * b),
                3 => DoInput,
                4 => DoOutput,
                5 => op => DoConditionalJump(op, x => x != 0),
                6 => op => DoConditionalJump(op, x => x == 0),
                7 => op => DoBinaryOperation(op, (a, b) => a < b ? 1 : 0),
                8 => op => DoBinaryOperation(op, (a, b) => a == b ? 1 : 0),
                99 => _ => OperationResult.Halt,
                _ => throw new InvalidOperationException("Invalid instruction")
            };

        private static OperationResult DoBinaryOperation(Instruction op, Func<int, int, int> func)
        {
            var result = func(op.ReadArgument(0), op.ReadArgument(1));
            op.WriteResult(2, result);
            return OperationResult.FromParameterCount(3);
        }

#if false
        private static OperationResult DoAdd(Instruction op)
        {
            var param1 = op.ReadArgument(0);
            var param2 = op.ReadArgument(1);

            var result = param1 + param2;

            op.WriteResult(2, result);

            return OperationResult.FromParameterCount(3);
        }

        private static OperationResult DoMultiply(Instruction op)
        {
            var param1 = op.ReadArgument(0);
            var param2 = op.ReadArgument(1);

            var result = param1 * param2;

            op.WriteResult(2, result);

            return OperationResult.FromParameterCount(3);
        }
#endif

        private static OperationResult DoInput(Instruction op)
        {
            var value = op.ReadInput();
            op.WriteResult(0, value);

            return OperationResult.FromParameterCount(1);
        }

        private static OperationResult DoOutput(Instruction op)
        {
            var value = op.ReadArgument(0);
            op.WriteOutput(value);

            return OperationResult.FromParameterCount(1);
        }

        private static OperationResult DoConditionalJump(Instruction op, Predicate<int> condition)
        {
            var value = op.ReadArgument(0);
            return condition(value)
                ? OperationResult.JumpTo(op.ReadArgument(1))
                : OperationResult.FromParameterCount(2);
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

        private readonly struct OperationResult
        {
            public static readonly OperationResult Halt = new OperationResult(-1, true);

            public OperationResult(int address, bool isAbsolute = false)
            {
                Address = address;
                IsAbsolute = isAbsolute;
            }

            public int Address { get; }

            public bool IsAbsolute { get; }

            public bool IsHalt => IsAbsolute && Address < 0;

            public int GetNextAddress(int currentAddress)
                => IsAbsolute ? Address : (currentAddress + Address);

            public static OperationResult FromParameterCount(int count)
                => count >= 0
                    ? new OperationResult(1 + count)
                    : throw new ArgumentOutOfRangeException(nameof(count));

            public static OperationResult JumpTo(int address)
                => address >= 0
                    ? new OperationResult(address, true)
                    : throw new ArgumentOutOfRangeException(nameof(address));
        }
    }
}