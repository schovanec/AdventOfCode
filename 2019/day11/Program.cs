using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

            // Part 1
            var painted = await RunRobotAsync(program);
            var totalPaintedSquares = painted.Count;
            Console.WriteLine($"Part 1 Result = {totalPaintedSquares}");
            Console.WriteLine();

            // Part 2
            var pixels = await RunRobotAsync(program, initialColor: 1);
            var image = GenerateImage(pixels);
            Console.WriteLine("Part 2 Image:");
            foreach (var row in image)
                Console.WriteLine(row);

            Console.WriteLine();
        }

        private static string[] GenerateImage(Dictionary<(long x, long y), long> image)
        {
            var minX = image.Keys.Min(p => p.x);
            var maxX = image.Keys.Max(p => p.x);
            var minY = image.Keys.Min(p => p.y);
            var maxY = image.Keys.Max(p => p.y);

            var buffer = new StringBuilder((int)(maxX - minX + 1));
            var result = new string[(int)(maxY - minY + 1)];

            for (var y = maxY; y >= minY; --y)
            {
                buffer.Clear();
                for (var x = minX; x <= maxX; ++x)
                {
                    if ((image.TryGetValue((x, y), out var color) ? color : 0) == 1)
                        buffer.Append('#');
                    else
                        buffer.Append('.');
                }

                result[maxY - y] = buffer.ToString();
            }

            return result;
        }

        private static async Task<Dictionary<(long x, long y), long>> RunRobotAsync(long[] program, long initialColor = 0)
        {
            var location = (x: 0L, y: 0L);
            var painted = new Dictionary<(long x, long y), long>() { [location] = initialColor };
            var heading = (x: 0L, y: 1L);

            // run the robot
            var input = Channel.CreateUnbounded<long>();
            var output = Channel.CreateUnbounded<long>();
            var robot = IntcodeMachine.RunProgramAsync(program, input.Reader.ReadAsync, output.Writer.WriteAsync)
                                      .ContinueWith(_ => output.Writer.TryComplete());

            while (!robot.IsCompleted)
            {
                // send the current colour as input
                await input.Writer.WriteAsync(painted.TryGetValue(location, out var color) ? color : 0);

                // Wait for output
                if (!await output.Reader.WaitToReadAsync())
                    break;

                // Paint the current square
                painted[location] = await output.Reader.ReadAsync();

                if (!await output.Reader.WaitToReadAsync())
                    break;

                // Update our heading
                heading = await output.Reader.ReadAsync() == 0
                    ? (-heading.y, heading.x)   // turn left
                    : (heading.y, -heading.x);  // turn right

                // Move to the next location
                location = (location.x + heading.x, location.y + heading.y);
            }

            return painted;
        }
    }
}
