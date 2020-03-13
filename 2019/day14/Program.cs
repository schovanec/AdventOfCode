using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace day14
{
    class Program
    {
        static void Main(string[] args)
        {
            var input = File.ReadLines(args.FirstOrDefault() ?? "sample3.txt").Select(Reaction.Parse).ToList();
            var reactions = input.ToDictionary(r => r.Output.Chemical);

            var clock = new Stopwatch();

            // Part 1
            clock.Start();
            long totalOreRequired = CalculateRquiredOre(1, reactions);
            clock.Stop();
            Console.WriteLine($"Total ORE = {totalOreRequired}");
            Console.WriteLine($"Time = {clock.ElapsedMilliseconds:#,##0}ms");
            Console.WriteLine();

            // Part 2
            clock.Restart();
            var targetAmount = 1000000000000;
            var initialLowerBound = targetAmount / totalOreRequired;
            long bestFuelAmount = FindMaximumFuel(targetAmount, reactions, initialLowerBound);
            clock.Stop();
            Console.WriteLine($"Maximum Fuel Amount = {bestFuelAmount}");
            Console.WriteLine($"Time = {clock.ElapsedMilliseconds:#,##0}ms");
            Console.WriteLine();
        }

        private static long FindMaximumFuel(
            long targetOreAmount, Dictionary<string, Reaction> reactions, long initialLowerBound)
        {
            // find the point where we go over
            var upperBound = initialLowerBound;
            while (CalculateRquiredOre(upperBound, reactions) < targetOreAmount)
                upperBound *= 2;

            // do a binary search between best and high
            var lowerBound = initialLowerBound;
            while (upperBound - lowerBound > 1)
            {
                var fuelAmount = (lowerBound + upperBound) / 2;
                var ore = CalculateRquiredOre(fuelAmount, reactions);
                if (ore < targetOreAmount)
                    lowerBound = fuelAmount;
                else if (ore > targetOreAmount)
                    upperBound = fuelAmount;
            }

            return lowerBound;
        }

        private static long CalculateRquiredOre(long fuelAmount, Dictionary<string, Reaction> reactions)
        {
            var totalRequired = reactions.Keys.ToDictionary(x => x, _ => 0L);
            totalRequired["FUEL"] = fuelAmount;
            totalRequired["ORE"] = 0;

            var totalProduced = reactions.Keys.ToDictionary(x => x, _ => 0L);

            var needed = totalRequired.Where(x => x.Key != "ORE" && x.Value > totalProduced[x.Key]).Select(x => x.Key);
            while (true)
            {
                var target = needed.FirstOrDefault();
                if (target == null)
                    break;

                if (target != "ORE")
                {
                    var reaction = reactions[target];

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
            return totalOreRequired;
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
