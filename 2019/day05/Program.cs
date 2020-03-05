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
#if false
            var fileName = args.FirstOrDefault() ?? "input.txt";

            // read the program
            var input = File.ReadLines(fileName).First();
            var program = input.Split(',').Select(x => int.Parse(x)).ToImmutableArray();

            const int target = 19690720;
            for (int noun = 0; noun <= 99; ++noun)
            {
                for (int verb = 0; verb <= 99; ++verb)
                {
                    var memory = program.ToList();
                    memory[1] = noun;
                    memory[2] = verb;
                    ExecuteIntcode(memory);

                    var result = memory[0];
                    if (result == target)
                    {
                        Console.WriteLine($"Found! noun={noun}, verb={verb}, 100*noun + verb = {100 * noun + verb}");
                    }
                }
            }

            // Solution to part 1: 9581917
#else
            var tests = new (int[] program, int[] result)[]
            {
                (new [] { 1,0,0,0,99 }, new [] { 2,0,0,0,99 }),
                (new [] { 2,3,0,3,99 }, new [] { 2,3,0,6,99 }),
                (new [] { 2,4,4,5,99,0 }, new [] { 2,4,4,5,99,9801 }),
                (new [] { 1,1,1,4,99,5,6,0,99 }, new [] { 30,1,1,4,2,5,6,0,99 })
            };

            foreach (var test in tests)
            {
                var machine = new Machine(test.program);
                machine.Execute();
                var finalState = machine.Memory.ToArray();
                Console.WriteLine($"{string.Join(",", test.program)} => {string.Join(",", finalState)} - Pass?: {finalState.SequenceEqual(test.result)}");
            }
#endif
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
