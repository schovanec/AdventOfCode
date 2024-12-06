using System.Collections.Immutable;

var (rules, updates) = ParseInput(File.ReadAllLines(args.FirstOrDefault() ?? "input.txt"));

var sumOfMiddleItems = updates.Where(u => IsValidOrder(u, rules))
                              .Sum(u => u[u.Length / 2]);
Console.WriteLine($"Part 1 Result = {sumOfMiddleItems}");

var sortMap = rules.Select(r => (key: r, value: -1))
                   .Concat(rules.Select(r => (key: (r.b, r.a), value: 1)))
                   .ToDictionary(x => x.key, x => x.value);

var sumOfFixedMiddleItems = updates.Where(u => !IsValidOrder(u, rules))
                                   .Select(u => FixUpdateList(u, sortMap))
                                   .Sum(u => u[u.Length / 2]);
Console.WriteLine($"Part 2 Result = {sumOfFixedMiddleItems}");

static bool IsValidOrder(ImmutableArray<int> pages, ImmutableList<(int a, int b)> rules)
{
  var failed = from r in rules
               let i = pages.IndexOf(r.a)
               let j = pages.IndexOf(r.b)
               where i >= 0 && j >= 0 && i > j
               select r;

  return !failed.Any();
}

static ImmutableArray<int> FixUpdateList(ImmutableArray<int> updates, IDictionary<(int a, int b), int> sortMap)
{
  return updates.Sort((a, b) => (a, b) switch
  {
    (int x, int y) when x == y => 0,
    _ => sortMap[(a, b)]
  });
}

static (ImmutableList<(int a, int b)> rules, ImmutableList<ImmutableArray<int>> updates) ParseInput(string[] lines)
{
  var rules = ImmutableList.CreateBuilder<(int a, int b)>();
  var endOfRules = 0;
  for (var i = 0; i < lines.Length; ++i)
  {
    if (string.IsNullOrEmpty(lines[i]))
    {
      endOfRules = i;
      break;
    }

    var parts = lines[i].Split('|', 2)
                        .Select(int.Parse)
                        .ToArray();

    rules.Add((parts[0], parts[1]));
  }

  var updates = ImmutableList.CreateBuilder<ImmutableArray<int>>();
  for (var i = endOfRules + 1; i < lines.Length; ++i)
    updates.Add(lines[i].Split(',').Select(int.Parse).ToImmutableArray());

  return (rules.ToImmutable(), updates.ToImmutable());
}