using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace Day08
{
    class Program
    {
        static void Main(string[] args)
        {
            var file = args.DefaultIfEmpty("input.txt").First();
            var program = File.ReadLines(file)
                              .Select(Instruction.Parse)
                              .ToImmutableList();

            var result1 = RunProgram(program);
            Console.WriteLine($"Part 1 Result = {result1.acc}");

            var result2 = EnumProgramRepairs(program).Select(RunProgram)
                                                     .First(x => x.finished);
            Console.WriteLine($"Part 2 Result = {result2.acc}");
        }

        static IEnumerable<ImmutableList<Instruction>> EnumProgramRepairs(ImmutableList<Instruction> program)
        {
            for (var i = 0; i < program.Count; ++i)
            {
                var repaired = RepairInstruction(program[i]);
                if (repaired != null)
                    yield return program.SetItem(i, repaired);
            }
        }

        static Instruction RepairInstruction(Instruction instruction)
            => instruction.Operation switch
            {
                Instruction.Noop => instruction with { Operation = Instruction.Jump },
                Instruction.Jump => instruction with { Operation = Instruction.Noop },
                _ => null
            };

        static (int acc, bool finished) RunProgram(IReadOnlyList<Instruction> program)
        {
            var ip = 0;
            var acc = 0;
            var seen = new bool[program.Count];
            while (ip < program.Count && !seen[ip])
            {
                seen[ip] = true;

                var op = program[ip];
                switch (op.Operation)
                {
                    case Instruction.Accumulate:
                        acc += op.Argument;
                        ++ip;
                        break;

                    case Instruction.Jump:
                        ip += op.Argument;
                        break;

                    default:
                        ++ip;
                        break;
                }
            }

            return (acc, ip >= program.Count);
        }

        record Instruction(string Operation, int Argument)
        {
            public const string Accumulate = "acc";
            public const string Jump = "jmp";
            public const string Noop = "nop";

            public static Instruction Parse(string text)
            {
                var parts = text.Split(' ', 2);
                return new Instruction(parts[0], int.Parse(parts[1]));
            }
        }
    }
}
