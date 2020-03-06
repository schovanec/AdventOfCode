using System;
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
                }
            };

            var count = 0;
            foreach (var test in tests)
            {
                var machine = new Machine(test.ProgramSequence);

                var output = new List<int>();
                using var input = test.InputSequence.GetEnumerator();

                machine.Execute(
                    read: () => input.MoveNext() ? input.Current : 0,
                    write: output.Add
                );

                var finalState = machine.Memory.ToArray();

                var expectedFinalState = test.ExpectedFinalStateSequence;
                var expectedOutput = test.ExpectedOutputSequence;

                Console.WriteLine($"Test #{++count}:");
                Console.WriteLine($"               Input: [{test.Input}]");
                Console.WriteLine($"             Program: [{test.Program}]");
                Console.WriteLine($"Expected Final State: [{test.ExpectedFinalState}]");
                Console.WriteLine($"  Actual Final State: [{string.Join(",", finalState)}] {(finalState.SequenceEqual(expectedFinalState) ? "Ok" : "Fail")}");
                Console.WriteLine($"     Expected Output: [{test.ExpectedOutput}] {(output.SequenceEqual(expectedOutput) ? "Ok" : "Fail")}");
                Console.WriteLine($"       Actual Output: [{string.Join(",", output)}]");
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
