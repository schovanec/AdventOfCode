var input = File.ReadAllLines(args.FirstOrDefault() ?? "input.txt")
                .Select(x => x.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries))
                .Select(x => (left: int.Parse(x[0]), right: int.Parse(x[1])))
                .ToArray();

var list1 = input.Select(x => x.left).OrderBy(x => x).ToArray();
var list2 = input.Select(x => x.right).OrderBy(x => x).ToArray();

var totalDistance = list1.Zip(list2)
                         .Sum(x => Math.Abs(x.First - x.Second));

Console.WriteLine($"Part 1 Result = {totalDistance}");

var counts = list2.GroupBy(x => x, (x, n) => (key: x, count: n.Count()))
                  .ToDictionary(x => x.key, x => x.count);

var similarityScore = list1.Sum(x => x * counts.GetValueOrDefault(x));

Console.WriteLine($"Part 2 Result = {similarityScore}");