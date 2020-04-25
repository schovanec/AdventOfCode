using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace day21
{
    class Program
    {
        static void Main(string[] args)
        {
            var program = File.ReadLines("input.txt").First();
            var code = IntcodeMachine.ParseCode(program);

            var instructions1 = new[]
            {
                // ~(A^B^C)^D
                "NOT T T",
                "AND A T",
                "AND B T",
                "AND C T",
                "NOT T J",
                "AND D J",
                "WALK"
            };

            Console.WriteLine("Part 1...");
            var result1 = RunDroid(code, instructions1);
            Console.WriteLine($"Result #1 = {result1}");

            var instructions2 = new[]
            {
                // ~(A^B^C)^D^(E|H)
                "NOT T T",
                "AND T T",
                "AND A T",
                "AND B T",
                "AND C T",
                "NOT T J",
                "AND D J",
                "NOT J T",
                "OR E T",
                "OR H T",
                "AND T J",
                "RUN"
            };

            Console.WriteLine("Part 2...");
            var result2 = RunDroid(code, instructions2);
            Console.WriteLine($"Result #2 = {result2}");
        }

        private static long? RunDroid(long[] code, string[] instructions)
        {
            var input = new Queue<long>();
            foreach (var line in instructions)
            {
                foreach (var ch in line)
                    input.Enqueue(ch);

                input.Enqueue('\n');
            }

            long? result = default;
            IntcodeMachine.RunProgram(
                program: code,
                ch =>
                {
                    if (ch < 255)
                        Console.Write((char)ch);
                    else
                        result = ch;
                },
                input
            );

            return result;
        }
    }
}
