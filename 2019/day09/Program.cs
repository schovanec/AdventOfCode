using System;
using System.IO;
using System.Linq;

namespace day09
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Any())
            {
                var program = IntcodeMachine.ParseCode(File.ReadLines(args.First()).First());

                // Part 1
                Console.WriteLine("Part 1:");
                IntcodeMachine.RunProgram(
                    program,
                    () => 1,
                    WriteToConsole
                );

                Console.WriteLine();

                // Part 2
                Console.WriteLine("Part 2:");
                IntcodeMachine.RunProgram(
                    program,
                    () => 2,
                    WriteToConsole
                );
            }
            else
            {
                RunTests();
            }
        }

        private static void RunTests()
        {
            var testPrograms = new[] {
                    "109,1,204,-1,1001,100,1,100,1008,100,16,101,1006,101,0,99",
                    "1102,34915192,34915192,7,4,7,99,0",
                    "104,1125899906842624,99"
                };

            var count = 0;
            foreach (var code in testPrograms)
            {
                try
                {
                    Console.WriteLine($"Test #{++count}");

                    var program = IntcodeMachine.ParseCode(code);
                    IntcodeMachine.RunProgram(
                        program,
                        ReadConsoleInput,
                        WriteToConsole
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                finally
                {
                    Console.WriteLine();
                }
            }
        }

        private static long ReadConsoleInput()
        {
            while (true)
            {
                Console.Write("Input a value: ");
                var text = Console.ReadLine();
                if (long.TryParse(text, out var result))
                    return result;

                Console.WriteLine("Invalid number!");
            }
        }

        private static void WriteToConsole(long value)
        {
            Console.WriteLine($"Output: {value}");
        }
    }
}
