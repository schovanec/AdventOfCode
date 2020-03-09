using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace day11
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var code = File.ReadLines(args.FirstOrDefault() ?? "input.txt").First();
            var program = IntcodeMachine.ParseCode(code);

            var location = (x: 0L, y: 0L);
            var painted = new Dictionary<(long x, long y), long>();
            var heading = (x: 0L, y: 1L);

            var input = Channel.CreateUnbounded<long>();
            var output = Channel.CreateUnbounded<long>();
            var robot = IntcodeMachine.RunProgramAsync(program, input.Reader.ReadAsync, output.Writer.WriteAsync);

            async Task<bool> WaitForOutput()
                => await Task.WhenAny(robot, output.Reader.WaitToReadAsync().AsTask()) != robot;

            while (!robot.IsCompleted)
            {
                // send the current colour as input
                await input.Writer.WriteAsync(painted.TryGetValue(location, out var color) ? color : 0);

                // Wait for output
                if (!await WaitForOutput())
                    break;

                // Paint the current square
                painted[location] = await output.Reader.ReadAsync();
                Console.WriteLine($"Painted {location} {(painted[location] == 1 ? "White" : "Black")}");

                if (!await WaitForOutput())
                    break;

                // move to the next square
                var direction = await output.Reader.ReadAsync();
                if (direction == 0)
                {
                    Console.Write($"Turning Left: {heading} ==> ");
                    heading = (-heading.y, heading.x);
                    Console.WriteLine(heading);
                }
                else
                {
                    Console.Write($"Turning Right: {heading} ==> ");
                    heading = (heading.y, -heading.x);
                    Console.WriteLine(heading);
                }

                location = (location.x + heading.x, location.y + heading.y);
            }

            var totalPaintedSquares = painted.Count;
            Console.WriteLine($"Part 1 Result = {totalPaintedSquares}");
        }
    }
}
