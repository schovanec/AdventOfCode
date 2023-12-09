using System.Collections.Immutable;

var input = File.ReadLines(args.FirstOrDefault() ?? "input.txt")
                .Select(x => x.Split(' ').Select(long.Parse))
                .Select(x => x.ToImmutableArray());

var result1 = input.Sum(FindNextValue);
Console.WriteLine($"Part 1 Result = {result1}");

var result2 = input.Select(x => x.Reverse().ToImmutableArray())
                   .Sum(FindNextValue);
Console.WriteLine($"Part 2 Result = {result2}");

long FindNextValue(ImmutableArray<long> seq)
  => seq.All(x => x == 0)
    ? 0L
    : seq.Last() + FindNextValue(FindDifferences(seq));

ImmutableArray<long> FindDifferences(ImmutableArray<long> seq)
  => seq.Zip(seq.Skip(1), (a, b) => b - a)
        .ToImmutableArray();