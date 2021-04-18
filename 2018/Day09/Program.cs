using System;
using System.Linq;

namespace Day09
{
    class Program
    {
        static void Main(string[] args)
        {
            var players = args.Length >= 1 ? int.Parse(args[0]) : 405;
            var highest = args.Length >= 2 ? int.Parse(args[1]) : 70953;

            Console.WriteLine($"Part 1 Result = {RunGame(players, highest)}");
            Console.WriteLine($"Part 2 Result = {RunGame(players, highest * 100)}");
        }

        private static long RunGame(int players, long highest)
        {
            var current = new Node(0);
            var scores = new long[players];
            
            var nextValue = 1;
            while (nextValue <= highest)
            {
                if (nextValue % 23 == 0)
                {
                    int player = (int)((nextValue - 1) % players);
                    scores[player] += nextValue;

                    for (var i = 0; i < 7; ++i)
                        current = current.Previous;

                    scores[player] += current.Value;
                    current = current.Remove();
                }
                else
                {
                    current = current.Next;
    
                    var newNode = new Node(nextValue);
                    newNode.InsertAfter(current);
                    current = newNode;
                }

                nextValue++;
            }

            return scores.Max();
        }

        class Node
        {
            public Node(long value)
            {
                Value = value;
                Previous = this;
                Next = this;
            }
            
            public long Value { get; }

            public Node Previous { get; private set; }

            public Node Next { get; private set; }

            public void InsertAfter(Node other)
            {
                if (other == null)
                    throw new ArgumentNullException(nameof(other));

                if (Next != this || Previous != this)
                    throw new InvalidOperationException();

                var next = other.Next;

                other.Next = this;
                this.Next = next;

                this.Previous = other;
                next.Previous = this;
            }

            public Node Remove()
            {
                if (Next == this || Previous == this)
                    throw new InvalidOperationException();

                Next.Previous = Previous;
                Previous.Next = Next;

                var result = Next;

                Next = this;
                Previous = this;

                return result;
            }
        }
    }
}
