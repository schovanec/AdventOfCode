using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace day17
{
    class Program
    {
        static void Main(string[] args)
        {
            var code = File.ReadLines("input.txt").FirstOrDefault();
            var program = IntcodeMachine.ParseCode(code);

            DoPart1(program);

            DoPart2(program);
        }

        private static void DoPart1(long[] program)
        {
            var map = new List<string>();
            var line = new StringBuilder();
            IntcodeMachine.RunProgram(
                program,
                ch =>
                {
                    if (ch == 10)
                    {
                        if (line.Length > 0)
                            map.Add(line.ToString());

                        line.Clear();
                    }
                    else
                    {
                        line.Append((char)ch);
                    }
                }
            );

            foreach (var row in map)
                Console.WriteLine(row);

            var width = map.First().Length;
            var height = map.Count;
            var intersections = new List<(int x, int y)>();
            for (int x = 1; x < width - 1; ++x)
            {
                for (int y = 1; y < height - 1; ++y)
                {
                    var isIntersection =
                        map[y][x] == '#' &&
                        map[y - 1][x] == '#' &&
                        map[y + 1][x] == '#' &&
                        map[y][x - 1] == '#' &&
                        map[y][x + 1] == '#';

                    if (isIntersection)
                        intersections.Add((x, y));
                }
            }

            var alignmentParamSum = intersections.Sum(p => p.x * p.y);
            Console.WriteLine($"Part 1 Result = {alignmentParamSum}");
        }

        private static void DoPart2(long[] program)
        {
            program[0] = 2;

            long? lastOutput = default;

            IntcodeMachine.RunProgram(
                program,
                ch =>
                {
                    if (ch <= 255)
                        Console.Write((char)ch);
                    else
                        lastOutput = ch;
                },
                "C,A,C,B,C,B,A,B,B,A",  // Main
                "R,4,L,4,L,4,R,8,R,10", // A
                "R,4,L,10,R,10",        // B
                "L,4,L,4,L,10,R,4",     // C
                "n"
            );

            Console.WriteLine($"Part 2 Result = {lastOutput}");
        }
    }
}
