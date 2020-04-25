using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;

namespace day22
{
    class Program
    {
        private const int DefaultCardCount = 10007;
        private const string DefaultInputFile = "input.txt";
        private const int DefaultCardToFind = 2019;

        static void Main(string[] args)
        {
            var (cardCount, inputFile, cardToFind) = ParseArguments(args);

            var deck = ImmutableList.CreateRange(Enumerable.Range(0, cardCount));
            var instructions = ParseInstructions(File.ReadLines(inputFile));
            var result = instructions.Aggregate(deck, ApplyShuffleOperation);

            if (!cardToFind.HasValue)
                Console.WriteLine(string.Join(" ", result));
            else
                Console.WriteLine($"Part 1 Result = {result.IndexOf(cardToFind.Value)}");
        }

        private static ImmutableList<int> ApplyShuffleOperation(ImmutableList<int> deck, (ShuffleOperation type, int argument) operation)
            => operation switch
            {
                (ShuffleOperation.DeailIntoNewStack, _) => DealIntoNewStack(deck),
                (ShuffleOperation.DealWithIncrement, var increment) => DealWithIncrement(increment, deck),
                (ShuffleOperation.CutStack, var position) => CutStack(position, deck),
                _ => deck
            };

        public static ImmutableList<int> DealIntoNewStack(ImmutableList<int> deck)
            => deck.Reverse();

        public static ImmutableList<int> CutStack(int position, ImmutableList<int> deck)
        {
            if (Math.Abs(position) >= deck.Count)
                throw new ArgumentOutOfRangeException(nameof(position));

            var index = (deck.Count + position) % deck.Count;
            var head = deck.GetRange(0, index);
            var tail = deck.GetRange(index, deck.Count - index);
            return tail.AddRange(head);
        }

        public static ImmutableList<int> DealWithIncrement(int n, ImmutableList<int> deck)
        {
            var position = 0;
            var result = deck.ToBuilder();
            foreach (var card in deck)
            {
                result[position] = card;
                position += n;
                position %= deck.Count;
            }

            return result.ToImmutable();
        }

        private static (int number, string file, int? cardToFind) ParseArguments(ReadOnlySpan<string> args)
        {
            var number = DefaultCardCount;
            var file = DefaultInputFile;
            int? cardToFind = DefaultCardToFind;

            if (args.Length > 0)
            {
                file = args[0];
                args = args.Slice(1);

                cardToFind = default(int?);
            }

            if (args.Length > 0 && int.TryParse(args[0], out var givenNumber))
            {
                number = givenNumber;
                args = args.Slice(1);
            }

            if (args.Length > 0 && int.TryParse(args[0], out var givenCardToFind))
            {
                cardToFind = givenCardToFind;
                args = args.Slice(1);
            }

            return (number, file, cardToFind);
        }

        private static IEnumerable<(ShuffleOperation operation, int parameter)> ParseInstructions(IEnumerable<string> instructions)
        {
            foreach (var instruction in instructions.Where(x => !string.IsNullOrEmpty(x)))
            {
                if (string.Equals(instruction, "deal into new stack", StringComparison.OrdinalIgnoreCase))
                {
                    yield return (ShuffleOperation.DeailIntoNewStack, 0);
                }
                else if (instruction.StartsWith("deal with increment ", true, CultureInfo.InvariantCulture))
                {
                    var increment = int.Parse(instruction.Substring(20));
                    yield return (ShuffleOperation.DealWithIncrement, increment);
                }
                else if (instruction.StartsWith("cut ", true, CultureInfo.InvariantCulture))
                {
                    var position = int.Parse(instruction.Substring(4));
                    yield return (ShuffleOperation.CutStack, position);
                }
            }
        }

        enum ShuffleOperation
        {
            DeailIntoNewStack,
            DealWithIncrement,
            CutStack
        }
    }
}
