using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace day13
{
    class Program
    {
        static void Main(string[] args)
        {
            var code = File.ReadLines(args.FirstOrDefault() ?? "input.txt").FirstOrDefault();
            var program = IntcodeMachine.ParseCode(code);

            // Part 1
            RunPart1(program);

            // Part 2
            RunPart2(program);
        }

        private static void RunPart1(long[] program)
        {
            Console.WriteLine("Part 1:");

            var tiles = new Dictionary<(int x, int y), TileId>();
            RunGame(
                program: program,
                read: () => 0,
                write: (pos, value) => tiles[pos] = (TileId)value);

            var totalBlocks = tiles.Values.Count(x => x == TileId.Block);
            Console.WriteLine($"Total Blocks = {totalBlocks}");
            Console.WriteLine();
        }

        private static void RunPart2(long[] program)
        {
            Console.WriteLine("Part 2:");

            // insert querters
            program[0] = 2;

            // run the game
            var tiles = new Dictionary<(int x, int y), TileId>();
            var lastX = new Dictionary<TileId, int>();
            long score = 0;
            RunGame(
                program: program,
                read: () =>
                {
                    if (lastX.TryGetValue(TileId.Ball, out var ball) &&
                        lastX.TryGetValue(TileId.HorizontalPaddle, out var paddle))
                    {
                        if (ball < paddle)
                            return -1;
                        else if (ball > paddle)
                            return 1;
                    }

                    return 0;
                },
                write: (pos, value) =>
                {
                    if (pos.x < 0)
                    {
                        score = value;
                    }
                    else
                    {
                        var tile = (TileId)value;
                        tiles[pos] = tile;
                        lastX[tile] = pos.x;
                    }
                }
            );

            var blockCount = tiles.Count(t => t.Value == TileId.Block);
            Console.WriteLine($"You {(blockCount > 0 ? "Lose" : "Win")}!");
            Console.WriteLine($"Final Score: {score}");
            Console.WriteLine();
        }

        static void RunGame(long[] program, Func<long> read, Action<(int x, int y), long> write)
        {
            var buffer = new long[3];
            long pos = 0;
            IntcodeMachine.RunProgram(
                program,
                read,
                x =>
                {
                    buffer[pos++] = x;
                    if (pos == 3)
                    {
                        write(((int)buffer[0], (int)buffer[1]), buffer[2]);
                        pos = 0;
                    }
                });
        }

        enum TileId
        {
            Empty = 0,
            Wall = 1,
            Block = 2,
            HorizontalPaddle = 3,
            Ball = 4
        }
    }
}
