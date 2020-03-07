using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

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
            Func<Task<int>> reader = null;
            if (read != null)
                reader = () => Task.FromResult(read());

            Func<int, Task> writer = null;
            if (write != null)
                writer = x => { write(x); return Task.CompletedTask; };

            ExecuteAsync(reader, writer)
                .AsTask()
                .Wait();
        }

        public async ValueTask ExecuteAsync(Func<Task<int>> read = null, Func<int, Task> write = null)
        {
            var state = new State(memory, write, read);

            while (true)
            {
                var op = new Instruction(state, ProgramCounter);
                var handler = GetHandler(op);

                var result = await handler(op);
                if (result.IsHalt)
                    break;

                ProgramCounter = result.GetNextAddress(ProgramCounter);
            }
        }

        private static Func<Instruction, ValueTask<OperationResult>> GetHandler(Instruction op)
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
                99 => _ => new ValueTask<OperationResult>(OperationResult.Halt),
                _ => throw new InvalidOperationException("Invalid instruction")
            };

        private static ValueTask<OperationResult> DoBinaryOperation(Instruction op, Func<int, int, int> func)
        {
            var result = func(op.ReadArgument(0), op.ReadArgument(1));
            op.WriteResult(2, result);
            return new ValueTask<OperationResult>(OperationResult.FromParameterCount(3));
        }

        private static async ValueTask<OperationResult> DoInput(Instruction op)
        {
            var value = await op.ReadInput();
            op.WriteResult(0, value);

            return OperationResult.FromParameterCount(1);
        }

        private static async ValueTask<OperationResult> DoOutput(Instruction op)
        {
            var value = op.ReadArgument(0);

            await op.WriteOutput(value);

            return OperationResult.FromParameterCount(1);
        }

        private static ValueTask<OperationResult> DoConditionalJump(Instruction op, Predicate<int> condition)
        {
            var value = op.ReadArgument(0);
            var result = condition(value)
                ? OperationResult.JumpTo(op.ReadArgument(1))
                : OperationResult.FromParameterCount(2);

            return new ValueTask<OperationResult>(result);
        }

        private sealed class State
        {
            public State(int[] memory, Func<int, Task> write = null, Func<Task<int>> read = null)
            {
                Memory = memory;
                Write = write ?? (_ => Task.CompletedTask);
                Read = read ?? (() => throw new InvalidOperationException("No input available"));
            }

            public int[] Memory { get; }

            public Func<int, Task> Write { get; }

            public Func<Task<int>> Read { get; }
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

            public Task WriteOutput(int output) => state.Write(output);

            public Task<int> ReadInput() => state.Read();
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