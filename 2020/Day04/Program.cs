using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Day04
{
    class Program
    {
        static void Main(string[] args)
        {
            var file = args.DefaultIfEmpty("input.txt").First();
            var passports = ReadPassports(file).ToList();

            var allFields = ImmutableHashSet.Create("byr", "iyr", "eyr", "hgt", "hcl", "ecl", "pid", "cid");
            var requiredFields = allFields.Remove("cid");

            var withRequriedFields = passports.Where(pp => requiredFields.Except(pp.Select(x => x.Key)).IsEmpty);
            Console.WriteLine($"Part 1 Result: {withRequriedFields.Count()}");

            var withValieValues = withRequriedFields.Where(pp => pp.All(IsFieldValid));
            Console.WriteLine($"Part 2 Result: {withValieValues.Count()}");
        }
        
        private static bool IsFieldValid(KeyValuePair<string, string> field) => IsFieldValid(field.Key, field.Value);

        private static bool IsFieldValid(string key, string value)
            => key switch
            {
                "byr" => IsValidNumber(value, 1920, 2002),
                "iyr" => IsValidNumber(value, 2010, 2020),
                "eyr" => IsValidNumber(value, 2020, 2030),
                "hgt" => IsValidHeight(value),
                "hcl" => IsValidHexColor(value),
                "ecl" => IsValidEyeColor(value),
                "pid" => IsValidPassportNumber(value),
                "cid" => true,
                _ => false
            };

        private static bool IsValidNumber(ReadOnlySpan<char> value, int min, int max)
            => int.TryParse(value, out var num) && num >= min && num <= max;

        private static bool IsValidHeight(ReadOnlySpan<char> value)
            => (value.EndsWith("cm") && IsValidNumber(value[0..^2], 150, 193))
            || (value.EndsWith("in") && IsValidNumber(value[0..^2], 59, 76));

        private static bool IsValidHexColor(string value)
            => Regex.IsMatch(value, "^#[0-9a-f]{6}$");

        private static bool IsValidEyeColor(string value)
            => value switch
            {
                "amb" or "blu" or "brn" or "gry" or "grn" or "hzl" or "oth" => true,
                _ => false
            };

        private static bool IsValidPassportNumber(string value)
            => Regex.IsMatch(value, @"^\d{9}$");

        private static IEnumerable<ImmutableArray<KeyValuePair<string, string>>> ReadPassports(string file)
            => ReadPassports(File.ReadLines(file));

        private static IEnumerable<ImmutableArray<KeyValuePair<string, string>>> ReadPassports(IEnumerable<string> lines)
        {
            var builder = ImmutableArray.CreateBuilder<KeyValuePair<string, string>>();
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) && builder.Count > 0)
                {
                    yield return builder.ToImmutable();
                    builder.Clear();
                }

                builder.AddRange(from item in line.Split(' ')
                                 where item.Length > 0
                                 let split = item.Split(':')
                                 where split.Length == 2
                                 select KeyValuePair.Create(split[0], split[1]));
            }

            if (builder.Count > 0)
                yield return builder.ToImmutable();
        }
    }
}
