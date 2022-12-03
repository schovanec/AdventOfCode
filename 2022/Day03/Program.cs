var bags = File.ReadAllLines(args.FirstOrDefault() ?? "input.txt")
               .Select(line => new Bag(line))
               .ToList();

var priorityTotal = bags.Select(x => x.ItemsInBoth.First())
                        .Sum(GetItemPriority);

Console.WriteLine($"Part 1 Result = {priorityTotal}");

var badgePriorityTotal = bags.Select(x => x.AllItems)
                             .Chunk(3)
                             .Select(x => x.Aggregate((a, b) => a.Intersect(b)).First())
                             .Sum(GetItemPriority);

Console.WriteLine($"Part 2 Result = {badgePriorityTotal}");

static int GetItemPriority(char item)
  => item switch
  {
    >= 'a' and <= 'z' => item - 'a' + 1,
    >= 'A' and <= 'Z' => item - 'A' + 27,
    _ => 0
  };

record Bag(IEnumerable<char> AllItems)
{
  public int Size = AllItems.Count() / 2;

  IEnumerable<char> First => AllItems.Take(Size);

  IEnumerable<char> Second => AllItems.Skip(Size);

  public IEnumerable<char> ItemsInBoth => First.Intersect(Second);
}