using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;

namespace Day22
{
    class Program
    {
        static void Main(string[] args)
        {
            var file = args.DefaultIfEmpty("input.txt").First();
            var decks = ParseInput(File.ReadLines(file)).ToArray();

            Part1(decks);
            Part2(decks);
        }

        private static void Part1(List<int>[] decks)
        {
            var player1 = new Queue<int>();
            foreach (var item in decks[0])
                player1.Enqueue(item);

            var player2 = new Queue<int>();
            foreach (var item in decks[1])
                player2.Enqueue(item);

            while (player1.Count > 0 && player2.Count > 0)
            {
                var card1 = player1.Dequeue();
                var card2 = player2.Dequeue();

                if (card1 > card2)
                {
                    player1.Enqueue(card1);
                    player1.Enqueue(card2);
                }
                else if (card2 > card1)
                {
                    player2.Enqueue(card2);
                    player2.Enqueue(card1);
                }
            }

            var winner = player1.Count > 0 ? player1 : player2;
            var score = 0L;
            while (winner.Count > 0)
            {
                var count = winner.Count;
                score += count * winner.Dequeue();
            }

            Console.WriteLine($"Part 1 Result = {score}");
        }

        private static void Part2(List<int>[] decks)
        {
            var (_, score) = RunGame(decks[0].ToImmutableList(), decks[1].ToImmutableList());
            Console.WriteLine($"Part 2 Result = {score}");
        }

        private static (int player, long score) RunGame(ImmutableList<int> deck1, ImmutableList<int> deck2)
        {
            var player1 = deck1.ToBuilder();
            var player2 = deck2.ToBuilder();
            var seen = new HashSet<string>();

            while (player1.Count > 0 && player2.Count > 0)
            {
                var key = GetKey(player1, player2);
                if (seen.Contains(key))
                    return (1, Score(player1));

                seen.Add(key);

                var card1 = Draw(player1);
                var card2 = Draw(player2);

                int winner = 0;
                if (card1 <= player1.Count && card2 <= player2.Count)
                    (winner, _) = RunGame(player1.Take(card1).ToImmutableList(), player2.Take(card2).ToImmutableList());
                else if (card1 > card2)
                    winner = 1;
                else if (card2 > card1)
                    winner = 2;

                if (winner == 1)
                {
                    player1.Add(card1);
                    player1.Add(card2);
                }
                else if (winner == 2)
                {
                    player2.Add(card2);
                    player2.Add(card1);
                }
            }

            return player1.Count > 0
                ? (1, Score(player1))
                : (2, Score(player2));
        }

        private static string GetKey(IReadOnlyList<int> deck1, IReadOnlyList<int> deck2)
        {
            var result = new StringBuilder();
            result.AppendJoin(',', deck1);
            result.Append('|');
            result.AppendJoin(',', deck2);
            return result.ToString();
        }

        private static long Score(IReadOnlyList<int> deck)
        {
            var count = deck.Count;
            return deck.Select((x, i) => x * (count - i))
                       .Sum();
        }

        private static int Draw(ImmutableList<int>.Builder deck)
        {
            var card = deck[0];
            deck.RemoveAt(0);
            return card;
        }

        static IEnumerable<List<int>> ParseInput(IEnumerable<string> lines)
        {
            List<int> current = null;
            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line))
                {
                    if (current != null)
                        yield return current;

                    current = null;
                }
                else if (current == null)
                {
                    current = new List<int>();
                }
                else
                {
                    current.Add(int.Parse(line));
                }
            }

            if (current != null)
                yield return current;
        }
    }
}
