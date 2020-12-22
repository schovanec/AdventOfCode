using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Day21
{
    class Program
    {
        static void Main(string[] args)
        {
            var file = args.DefaultIfEmpty("input.txt").First();
            var foods = ParseInput(File.ReadLines(file));

            var candidatesByAllergen = (from food in foods
                                        from allergen in food.Allergens
                                        group food.Ingredients by allergen into g
                                        select new {
                                            Allergen = g.Key,
                                            Candidates = g.Aggregate((a, b) => a.Intersect(b))
                                        }).ToImmutableList();

            var allCandidates = candidatesByAllergen.Select(x => x.Candidates)
                                                    .Aggregate((a, b) => a.Union(b));

            var result1 = (from food in foods
                           from ingredient in food.Ingredients
                           where !allCandidates.Contains(ingredient)
                           select ingredient).Count();

            Console.WriteLine($"Part 1 Result = {result1}");

            var canonicalList = new List<(string ingredient, string allergen)>();
            var remaining = candidatesByAllergen;
            while (remaining.Count > 0)
            {
                var singles = remaining.Where(x => x.Candidates.Count == 1)
                                       .Select(x => (ingredient: x.Candidates.Single(), allergen: x.Allergen))
                                       .ToImmutableHashSet();

                Debug.Assert(singles.Count > 0);

                canonicalList.AddRange(singles);
                
                var ingredients = singles.Select(x => x.ingredient).ToImmutableHashSet();
                remaining = remaining.Select(x => new { Allergen = x.Allergen, Candidates = x.Candidates.Except(ingredients) })
                                     .Where(x => x.Candidates.Count > 0)
                                     .ToImmutableList();
            }

            var dangerounsIngredientList = canonicalList.OrderBy(x => x.allergen)
                                                        .Select(x => x.ingredient)
                                                        .ToImmutableList();
            Console.WriteLine($"Part 2 Result = {string.Join(',', dangerounsIngredientList)}");
        }

        static IEnumerable<Food> ParseInput(IEnumerable<string> input)
        {
            foreach (var line in input)
            {
                var splits = line.Split(" (contains ", 2);
                var ingredients = splits[0].Split(' ').ToImmutableHashSet();
                var allergens = splits[1].TrimEnd(')').Split(", ").ToImmutableHashSet();

                yield return new Food(ingredients, allergens);
            }
        }

        record Food(ImmutableHashSet<string> Ingredients, ImmutableHashSet<string> Allergens);
    }
}
