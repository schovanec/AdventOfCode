var input = File.ReadLines(args.FirstOrDefault() ?? "input.txt")
                .SelectMany(x => x.Split(','))
                .Select(int.Parse)
                .ToList();

var min = input.Min();
var max = input.Max();
var range = Enumerable.Range(min, max - min + 1);

var best1 = range.Min(i => input.Sum(n => Distance(n, i)));
Console.WriteLine($"Part 1 Result = {best1}");

var best2 = range.Min(i => input.Sum(n => SumTo(Distance(n, i))));
Console.WriteLine($"Part 2 Result = {best2}");

int Distance(int a, int b) => Math.Abs(a - b);

int SumTo(int n) => (n * (n + 1)) / 2;