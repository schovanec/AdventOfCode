using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;

namespace day14
{
    class Program
    {
        static void Main(string[] args)
        {
            var reactions = File.ReadLines(args.FirstOrDefault() ?? "sample3.txt").Select(Reaction.Parse).ToList();

            var reactionsByProduct = reactions.ToDictionary(r => r.Output.Chemical);

            var totalRequired = reactionsByProduct.Keys.ToDictionary(x => x, _ => 0L);
            totalRequired["FUEL"] = 1;
            totalRequired["ORE"] = 0;

            var totalProduced = reactionsByProduct.Keys.ToDictionary(x => x, _ => 0L);

            var needed = totalRequired.Where(x => x.Key != "ORE" && x.Value > totalProduced[x.Key]).Select(x => x.Key);
            while (true)
            {
                var target = needed.FirstOrDefault();
                if (target == null)
                    break;

                if (target != "ORE")
                {
                    var reaction = reactionsByProduct[target];

                    var required = totalRequired[target] - totalProduced[target];
                    var multiple = required / reaction.Output.Quantity;
                    if (required % reaction.Output.Quantity > 0)
                        ++multiple;

                    foreach (var input in reaction.Inputs)
                        totalRequired[input.Chemical] += input.Quantity * multiple;

                    totalProduced[target] += reaction.Output.Quantity * multiple;
                }
            }

            var totalOreRequired = totalRequired["ORE"];
            Console.WriteLine($"Total ORE = {totalOreRequired}");

            var availableOre = 1000000000000L;
            var initialTotal = availableOre / totalOreRequired;
            var totalExcess = from r in totalRequired
                              join p in totalProduced on r.Key equals p.Key
                              let excess = p.Value - r.Value
                              select (p.Key, amount: excess * initialTotal);

            foreach (var item in totalExcess)
                Console.WriteLine(item);

            var additionalTotal = totalExcess.Min(x => x.amount / totalRequired[x.Key]);

            Console.WriteLine($"Initial Total: {initialTotal}");
            Console.WriteLine($"Additional: {additionalTotal}");
            Console.WriteLine($"Sum: {initialTotal + additionalTotal}");
        }
    }

    sealed class Reaction
    {
        public Reaction(ImmutableList<ReactionPart> inputs, ReactionPart output)
        {
            Inputs = inputs ?? ImmutableList<ReactionPart>.Empty;
            Output = output;
        }

        public ImmutableList<ReactionPart> Inputs { get; }

        public ReactionPart Output { get; }

        public override string ToString()
        {
            var result = new StringBuilder();
            foreach (var item in Inputs)
            {
                if (result.Length > 0)
                    result.Append(',').Append(' ');

                result.Append(item.Quantity).Append(' ').Append(item.Chemical);
            }

            result.Append(" => ");

            result.Append(Output.Quantity).Append(' ').Append(Output.Chemical);

            return result.ToString();
        }

        public static Reaction Parse(string input)
        {
            var parts = input.Split(" => ");
            var inputs = parts[0].Split(',');
            return new Reaction(
                inputs.Select(ReactionPart.Parse).ToImmutableList(),
                ReactionPart.Parse(parts[1]));
        }
    }

    struct ReactionPart
    {
        public ReactionPart(string chemical, long quantity)
        {
            Chemical = chemical?.ToUpperInvariant() ?? throw new ArgumentNullException(nameof(chemical));
            Quantity = quantity;
        }

        public string Chemical { get; }

        public long Quantity { get; }

        public override string ToString() => $"{Quantity} {Chemical}";

        public static ReactionPart Parse(string input)
        {
            var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return new ReactionPart(parts[1], long.Parse(parts[0]));
        }
    }
}
