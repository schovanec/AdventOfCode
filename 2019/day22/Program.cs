using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;

namespace day22
{
    //
    // Based on: https://codeforces.com/blog/entry/72593
    //
    static class Program
    {
        private const string InputFile = "input.txt";

        static void Main(string[] args)
        {
            var instructions = ParseInstructions(File.ReadLines(InputFile)).ToArray();

            DoPart1(instructions);
            DoPart2(instructions);
        }

        private static void DoPart1(IEnumerable<(BigInteger a, BigInteger b)> instructions)
        {
            const long DeckSize = 10007;
            const long TargetPosition = 2019;

            var shuffle = instructions.Aggregate((f, g) => f.Compose(g, DeckSize));

            var result = shuffle.Apply(TargetPosition, DeckSize);
            Console.WriteLine($"Part 1 Result = {result}");
        }

        private static void DoPart2(IEnumerable<(BigInteger a, BigInteger b)> instructions)
        {
            const long DeckSize = 119315717514047;
            const long ShuffleCount = 101741582076661;
            const long TargetCardValue = 2020;

            var shuffle = instructions.Aggregate((f, g) => Compose(f, g, DeckSize))
                                      .ComposePow(ShuffleCount, DeckSize);

            var result = shuffle.ApplyInverse(TargetCardValue, DeckSize);
            Console.WriteLine($"Part 2 Result = {result}");
        }

        private static BigInteger Apply(this (BigInteger a, BigInteger b) f, BigInteger x, BigInteger m)
            => (f.a * x + f.b).MathMod(m);

        private static BigInteger ApplyInverse(this (BigInteger a, BigInteger b) f, BigInteger x, BigInteger m)
            => ((x - f.b) * BigInteger.ModPow(f.a, m - 2, m)).MathMod(m);

        private static (BigInteger a, BigInteger b) Compose(this (BigInteger a, BigInteger b) f, (BigInteger a, BigInteger b) g, BigInteger m)
            => ((f.a * g.a).MathMod(m), (f.b * g.a + g.b).MathMod(m));

        private static (BigInteger a, BigInteger b) ComposePow(this (BigInteger a, BigInteger b) f, long k, BigInteger m)
        {
            var g = (BigInteger.One, BigInteger.Zero);

            while (k > 0)
            {
                if (k % 2 == 1)
                    g = Compose(g, f, m);

                k /= 2;

                f = Compose(f, f, m);
            }

            return g;
        }

        private static BigInteger MathMod(this BigInteger n, BigInteger m)
            => (BigInteger.Abs(n * m) + n) % m;

        private static IEnumerable<(BigInteger a, BigInteger b)> ParseInstructions(IEnumerable<string> instructions)
        {
            foreach (var instruction in instructions.Where(x => !string.IsNullOrEmpty(x)))
            {
                if (string.Equals(instruction, "deal into new stack", StringComparison.OrdinalIgnoreCase))
                {
                    yield return DealIntoNewStack;
                }
                else if (instruction.StartsWith("deal with increment ", true, CultureInfo.InvariantCulture))
                {
                    var increment = long.Parse(instruction.Substring(20));
                    yield return DealWithIncrement(increment);
                }
                else if (instruction.StartsWith("cut ", true, CultureInfo.InvariantCulture))
                {
                    var position = long.Parse(instruction.Substring(4));
                    yield return Cut(position);
                }
            }
        }

        private static (BigInteger a, BigInteger b) DealIntoNewStack => (-1, -1);

        private static (BigInteger a, BigInteger b) Cut(long n) => (1, -n);

        private static (BigInteger a, BigInteger b) DealWithIncrement(long n) => (n, 0);
    }
}
