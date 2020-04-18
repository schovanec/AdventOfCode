using System;
using System.IO;
using System.Linq;

namespace day19
{
    class Program
    {
        static void Main(string[] args)
        {
            var code = File.ReadLines("input.txt").FirstOrDefault();
            var program = IntcodeMachine.ParseCode(code);

            long affectedPointCount = RunPart1(program);
            Console.WriteLine($"Part 1 Result = {affectedPointCount}");

            var firstPoint = RunPart2(program, 100);
            Console.WriteLine($"Part 2 Result = {firstPoint.x * 10000 + firstPoint.y} [{firstPoint}]");
        }

        private static long RunPart1(long[] program)
        {
            const int Size = 100;
            long affectedPointCount = 0;
            for (var y = 0; y < Size; ++y)
            {
                for (var x = 0; x < Size; ++x)
                {
                    long result = RunDroneProgram(program, x, y);

                    if (result == 1)
                    {
                        ++affectedPointCount;

                        Console.Write('#');
                    }
                    else
                    {
                        Console.Write('.');
                    }
                }

                Console.WriteLine();
            }

            return affectedPointCount;
        }

        private static (int x, int y) RunPart2(long[] program, int size)
        {
            var ymin = 0;
            var xstart = 0;
            while (true)
            {
                var startSpan = FindBeamRange(program, ymin, xstart);

                var wmin = startSpan.max - startSpan.min + 1;
                if (wmin >= size)
                {
                    var ymax = ymin + size - 1;
                    var endSpan = FindBeamRange(program, ymax, startSpan.min);
                    var wmax = endSpan.max - endSpan.min + 1;
                    if (wmax >= size && (endSpan.min + size - 1) <= startSpan.max)
                        return (endSpan.min, ymin);
                }

                xstart = Math.Max(xstart, startSpan.min);
                ++ymin;
            }
        }

        private static (int min, int max) FindBeamRange(long[] program, int y, int xstart = 0)
        {
            var xmin = xstart - 1;
            long result = 0;
            while (result == 0 && xmin < 10000)
            {
                ++xmin;
                result = RunDroneProgram(program, xmin, y);
            }

            if (xmin >= 10000)
                return (0, 0);

            var xmax = xmin;
            while (result == 1)
            {
                ++xmax;
                result = RunDroneProgram(program, xmax, y);
            }

            return (xmin, xmax - 1);
        }

        private static long RunDroneProgram(long[] program, int x, int y)
        {
            long result = 0;
            IntcodeMachine.RunProgram(
                program,
                v => { result = v; },
                x,
                y
            );
            return result;
        }
    }
}
