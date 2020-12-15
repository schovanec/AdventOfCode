using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace Day14
{
    class Program
    {
        static void Main(string[] args)
        {
            var file = args.DefaultIfEmpty("input.txt").First();

            var instructions = File.ReadLines(file)
                                   .Select(x => ParseInstruction(x));

            Part1(instructions);
            Part2(instructions);
        }

        static void Part1(IEnumerable<Instruction> instructions)
        {
            var currentMask = new MaskInstruction(0, 0);
            var memory = new Dictionary<ulong, ulong>();
            foreach (var op in instructions)
            {
                switch (op)
                {
                    case MaskInstruction mask:
                        currentMask = mask;
                        break;

                    case StoreInstruction { Address: var address, Value: var value }:
                        var valueToStore = (value & currentMask.Mask) | currentMask.Image;
                        memory[address] = valueToStore;
                        break;
                }
            }

            var sum = memory.Values.Aggregate((a, b) => a + b);
            Console.WriteLine($"Part 1 Result = {sum}");
        }

        static void Part2(IEnumerable<Instruction> instructions)
        {
            var currentMask = new MaskInstruction(0, 0);
            var memory = new Dictionary<ulong, ulong>();
            foreach (var op in instructions)
            {
                switch (op)
                {
                    case MaskInstruction mask:
                        currentMask = mask;
                        break;

                    case StoreInstruction { Address: var address, Value: var value }:
                        foreach (var addrToWrite in GenerateAddresses(address, currentMask.Image, currentMask.Mask))
                            memory[addrToWrite] = value;
                        break;
                }
            }

            var sum = memory.Values.Aggregate((a, b) => a + b);
            Console.WriteLine($"Part 2 Result = {sum}");
        }

        static ImmutableArray<ulong> GenerateAddresses(ulong address, ulong image, ulong mask)
        {
            var result = ImmutableArray.CreateBuilder<ulong>();
            result.Add(0);
            for (var i = 0; i < 36; ++i)
            {
                var bit = (ulong)1 << i;
                if ((image & bit) == bit)
                {
                    for (var j = 0; j < result.Count; ++j)
                        result[j] |= bit;
                }
                else if ((mask & bit) == bit)
                {
                    int k = result.Count;
                    for (var j = 0; j < k; ++j)
                        result.Add(result[j] | bit);
                }
                else
                {
                    for (var j = 0; j < result.Count; ++j)
                        result[j] |= (address & bit);
                }
            }

            return result.ToImmutable();
        }

        static Instruction ParseInstruction(ReadOnlySpan<char> instruction)
        {
            if (instruction.StartsWith("mask = "))
                return ParseMask(instruction.Slice(7));
            else if (instruction.StartsWith("mem["))
                return ParseStoreInstruction(instruction);

            return null;
        }

        static MaskInstruction ParseMask(ReadOnlySpan<char> maskText)
        {
            ulong mask = 0;
            ulong image = 0;
            for (var i = 0; i < 36; ++i)
            {
                mask <<= 1;
                image <<= 1;

                switch (maskText[i])
                {
                    case '1':
                        image |= 1;
                        break;

                    case '0':
                        break;

                    case 'X':
                        mask |= 1;
                        break;
                }
            }

            return new MaskInstruction(image, mask);
        }

        static StoreInstruction ParseStoreInstruction(ReadOnlySpan<char> instruction)
        {
            var start = instruction.IndexOf('[');
            if (start < 0)
                return null;

            var end = instruction.Slice(start + 1).IndexOf(']');
            if (end < 0)
                return null;

            var addressText = instruction.Slice(start + 1, end);
            ulong address = ulong.Parse(addressText);

            var equalsPos = instruction.IndexOf('=');
            if (equalsPos < 0)
                return null;

            var valueText = instruction.Slice(equalsPos + 1).Trim();
            ulong value = ulong.Parse(valueText);

            return new StoreInstruction(address, value);
        }

        record Instruction;

        record MaskInstruction(ulong Image, ulong Mask) : Instruction;

        record StoreInstruction(ulong Address, ulong Value) : Instruction;
    }
}