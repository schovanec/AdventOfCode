﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace day05
{
    class Program
    {
        static void Main(string[] args)
        {
#if true
            var fileName = args.FirstOrDefault() ?? "input.txt";

            // read the program
            var input = File.ReadLines(fileName).First();
            var program = input.Split(',').Select(int.Parse);

            var machine = new Machine(program);
            machine.Execute(
                read: ReadInputValue,
                write: WriteOutputValue
            );
#else
            var tests = new[]
            {
                new TestCase
                {
                    Program = "1,0,0,0,99",
                    ExpectedFinalState = "2,0,0,0,99"
                },
                new TestCase
                {
                    Program = "2,3,0,3,99",
                    ExpectedFinalState = "2,3,0,6,99"
                },
                new TestCase
                {
                    Program = "2,4,4,5,99,0",
                    ExpectedFinalState = "2,4,4,5,99,9801"
                },
                new TestCase
                {
                    Program = "1,1,1,4,99,5,6,0,99",
                    ExpectedFinalState = "30,1,1,4,2,5,6,0,99"
                },
                new TestCase
                {
                    Program = "3,0,4,0,99",
                    ExpectedFinalState = "1,0,4,0,99",
                    Input = "1",
                    ExpectedOutput = "1"
                },
                new TestCase
                {
                    Program = "3,0,4,0,99",
                    ExpectedFinalState = "9876,0,4,0,99",
                    Input = "9876",
                    ExpectedOutput = "9876"
                },
                new TestCase
                {
                    Program = "1002,4,3,4,33",
                    ExpectedFinalState = "1002,4,3,4,99"
                },
                new TestCase
                {
                    Program = "3,9,8,9,10,9,4,9,99,-1,8",
                    Input = "8",
                    ExpectedOutput = "1"
                },
                new TestCase
                {
                    Program = "3,9,8,9,10,9,4,9,99,-1,8",
                    Input = "99",
                    ExpectedOutput = "0"
                },
                new TestCase
                {
                    Program = "3,9,7,9,10,9,4,9,99,-1,8",
                    Input = "7",
                    ExpectedOutput = "1"
                },
                new TestCase
                {
                    Program = "3,9,7,9,10,9,4,9,99,-1,8",
                    Input = "8",
                    ExpectedOutput = "0"
                },
                new TestCase
                {
                    Program = "3,3,1108,-1,8,3,4,3,99",
                    Input = "8",
                    ExpectedOutput = "1"
                },
                new TestCase
                {
                    Program = "3,3,1108,-1,8,3,4,3,99",
                    Input = "99",
                    ExpectedOutput = "0"
                },
                new TestCase
                {
                    Program = "3,3,1107,-1,8,3,4,3,99",
                    Input = "7",
                    ExpectedOutput = "1"
                },
                new TestCase
                {
                    Program = "3,3,1107,-1,8,3,4,3,99",
                    Input = "8",
                    ExpectedOutput = "0"
                },
                new TestCase
                {
                    Program = "3,21,1008,21,8,20,1005,20,22,107,8,21,20,1006,20,31,1106,0,36,98,0,0,1002,21,125,20,4,20,1105,1,46,104,999,1105,1,46,1101,1000,1,20,4,20,1105,1,46,98,99",
                    Input = "7",
                    ExpectedOutput = "999"
                },
                new TestCase
                {
                    Program = "3,21,1008,21,8,20,1005,20,22,107,8,21,20,1006,20,31,1106,0,36,98,0,0,1002,21,125,20,4,20,1105,1,46,104,999,1105,1,46,1101,1000,1,20,4,20,1105,1,46,98,99",
                    Input = "8",
                    ExpectedOutput = "1000"
                },
                new TestCase
                {
                    Program = "3,21,1008,21,8,20,1005,20,22,107,8,21,20,1006,20,31,1106,0,36,98,0,0,1002,21,125,20,4,20,1105,1,46,104,999,1105,1,46,1101,1000,1,20,4,20,1105,1,46,98,99",
                    Input = "9",
                    ExpectedOutput = "1001"
                },
            };

            var count = 0;
            foreach (var test in tests)
            {
                Console.WriteLine($"Test #{++count}:");
                Console.WriteLine($"               Input: [{test.Input}]");
                Console.WriteLine($"             Program: [{test.Program}]");

                if (!string.IsNullOrEmpty(test.ExpectedFinalState))
                    Console.WriteLine($"Expected Final State: [{test.ExpectedFinalState}]");

                if (!string.IsNullOrEmpty(test.ExpectedOutput))
                    Console.WriteLine($"     Expected Output: [{test.ExpectedOutput}]");

                try
                {
                    var machine = new Machine(test.ProgramSequence);

                    var output = new List<int>();
                    using var input = test.InputSequence.GetEnumerator();

                    machine.Execute(
                        read: () => input.MoveNext() ? input.Current : 0,
                        write: output.Add
                    );

                    if (!string.IsNullOrEmpty(test.ExpectedFinalState))
                    {
                        var finalState = machine.Memory.ToArray();
                        var expectedFinalState = test.ExpectedFinalStateSequence;
                        Console.WriteLine($"  Actual Final State: [{string.Join(",", finalState)}] {(finalState.SequenceEqual(expectedFinalState) ? "Ok" : "Fail")}");
                    }

                    if (!string.IsNullOrEmpty(test.ExpectedOutput))
                    {
                        var expectedOutput = test.ExpectedOutputSequence;
                        Console.WriteLine($"       Actual Output: [{string.Join(",", output)}] {(output.SequenceEqual(expectedOutput) ? "Ok" : "Fail")}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"    Execution Failed: {ex.Message}");
                }

                Console.WriteLine();
            }
#endif
        }

        private static int ReadInputValue()
        {
            while (true)
            {
                Console.Write("Input a value: ");
                if (int.TryParse(Console.ReadLine(), out var result))
                    return result;

                Console.WriteLine("Error: Invalid input!");
            }
        }

        private static void WriteOutputValue(int value)
        {
            Console.WriteLine($"Output Value: {value}");
        }

        private class TestCase
        {
            public string Program { get; set; }

            public IEnumerable<int> ProgramSequence => ParseInputString(Program);

            public string Input { get; set; }

            public IEnumerable<int> InputSequence => ParseInputString(Input);

            public string ExpectedFinalState { get; set; }

            public IEnumerable<int> ExpectedFinalStateSequence => ParseInputString(ExpectedFinalState);

            public string ExpectedOutput { get; set; }

            public IEnumerable<int> ExpectedOutputSequence => ParseInputString(ExpectedOutput);

            private static IEnumerable<int> ParseInputString(string text)
                => (text ?? "").Split(',').Where(x => x.Length > 0).Select(int.Parse);
        }

#if false
        private const int OperationAdd = 1;
        private const int OperationMultiply = 2;
        private const int OperationHalt = 99;

        private const int InstructionSize = 4;

        private const int OffsetParam1 = 1;
        private const int OffsetParam2 = 2;
        private const int OffsetOutput = 3;

        private static void ExecuteIntcode(List<int> memory)
        {
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
        }
#endif
    }
}
