﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace day15
{
    class Program
    {
        static void Main(string[] args)
        {
            var code = File.ReadLines("input.txt").FirstOrDefault();
            var program = IntcodeMachine.ParseCode(code);

            var walls = new HashSet<(long x, long y)>();

            var position = (x: 0L, y: 0L);
            var visited = new HashSet<(long x, long y)>() { position };

            var moves = new Stack<long>();

            var machine = new IntcodeMachine(program);
            var found = false;

            while (!found)
            {
                var next = (from cmd in Enumerable.Range(1, 4)
                            let newPosition = CalculatePosition(position, cmd)
                            where !walls.Contains(newPosition)
                               && !visited.Contains(newPosition)
                            select new { Command = cmd, NewPosition = newPosition }).FirstOrDefault();

                if (next != null)
                {
                    // try to move
                    var result = machine.StepMany(next.Command);
                    if (result.Output == 0)
                    {
                        walls.Add(next.NewPosition);
                    }
                    else
                    {
                        visited.Add(next.NewPosition);
                        position = next.NewPosition;
                        moves.Push(next.Command);
                        if (result.Output == 2)
                            found = true;
                        else if (result.Output != 1)
                            throw new InvalidOperationException("Unexpected output");
                    }
                }
                else if (moves.Count > 0)
                {
                    // backtrack
                    var cmd = InverseMove(moves.Pop());
                    var result = machine.StepMany(cmd);
                    if (result.Output != 1)
                        throw new InvalidOperationException("Unexpected output");

                    position = CalculatePosition(position, cmd);
                }
            }

            Console.WriteLine($"Fewest Moves = {moves.Count}");

            // IntcodeMachine.RunProgram(
            //     program,
            //     read: () =>
            //     {
            //         var position = path.Peek();
            //         var moves = from cmd in Enumerable.Range(1, 4)
            //                     let newPosition = CalculatePosition(position, cmd)
            //                     where !walls.Contains(newPosition)
            //                        && !visited.Contains(newPosition)
            //                     select new { Command = cmd, NewPosition = newPosition };

            //         var next = moves.FirstOrDefault();
            //         if (next != null)
            //         {
            //             path.Push(next.NewPosition);
            //             return next.Command;
            //         }
            //         else
            //         {
            //             path.Pop();
            //             var backtrackPosition = path.Peek();
            //             return GetMoveCommand(position, backtrackPosition);
            //         }
            //     },
            //     write: value =>
            //     {
            //         var position = path.Peek();
            //         if (value == 0)
            //         {
            //             walls.Add(position);
            //             path.Pop();
            //         }
            //         else if ()
            // )
        }

        private static (long x, long y) CalculatePosition((long x, long y) position, long command)
            => command switch
            {
                1 => (position.x, position.y + 1),
                2 => (position.x, position.y - 1),
                3 => (position.x - 1, position.y),
                4 => (position.x + 1, position.y),
                _ => position
            };

        private static long InverseMove(long move)
            => move switch
            {
                1 => 2,
                2 => 1,
                3 => 4,
                4 => 3,
                _ => throw new ArgumentOutOfRangeException(nameof(move))
            };

        private static long GetMoveCommand((long x, long y) current, (long x, long y) next)
        {
            if (next.y == current.y)
            {
                if (next.x < current.x)
                    return 3;
                else if (next.x > current.x)
                    return 4;
            }
            else if (next.x == current.x)
            {
                if (next.y < current.y)
                    return 2;
                else if (next.y > current.y)
                    return 1;
            }

            throw new InvalidOperationException("Cannot move");
        }
    }
}
