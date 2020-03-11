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
            var reactions = File.ReadLines(args.FirstOrDefault() ?? "sample2.txt").Select(Reaction.Parse).ToList();

            var reactionsByProduct = reactions.ToDictionary(r => r.Output.Chemical);

            var targets = new Queue<ReactionPart>();
            var totalOreRequired = 0L;
            targets.Enqueue(new ReactionPart("FUEL", 1));
            var excess = reactionsByProduct.Keys.ToDictionary(x => x, _ => 0L);
            while (targets.Count > 0)
            {
                var target = targets.Dequeue();
                if (target.Chemical == "ORE")
                {
                    totalOreRequired += target.Quantity;
                }
                else
                {
                    var reaction = reactionsByProduct[target.Chemical];

                    var required = target.Quantity;
                    var available = excess[target.Chemical];
                    if (available > 0)
                    {
                        var amountToUse = Math.Min(available, required);
                        required -= amountToUse;
                        available -= amountToUse;
                        excess[target.Chemical] = available;
                    }

                    if (required > 0)
                    {
                        var multiple = required / reaction.Output.Quantity;
                        if (required % reaction.Output.Quantity > 0)
                            ++multiple;

                        foreach (var input in reaction.Inputs)
                            targets.Enqueue(new ReactionPart(input.Chemical, input.Quantity * multiple));

                        var amountProduced = multiple * reaction.Output.Quantity;
                        available += Math.Max(0, amountProduced - required);
                        excess[target.Chemical] = available;
                    }
                }
            }

            Console.WriteLine($"Total ORE = {totalOreRequired}");

            foreach (var item in excess.Where(x => x.Value > 0))
                Console.WriteLine($"Excess {item.Key} = {item.Value}");

            //var availableOreQuantity = 1000000000000;
            //var maximumFuelProduced = 
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
