using System.Collections.Immutable;

var bags = File.ReadAllLines(args.FirstOrDefault() ?? "input.txt")
               .Select(Bag.Create)
               .ToImmutableList();

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

record Bag(ImmutableHashSet<char> First, ImmutableHashSet<char> Second)
{
  public ImmutableHashSet<char> AllItems => First.Union(Second);

  public IEnumerable<char> ItemsInBoth => First.Intersect(Second);

  public static Bag Create(string contents)
  {
    var size = contents.Length / 2;
    return new Bag(
      contents.Take(size).ToImmutableHashSet(),
      contents.Skip(size).ToImmutableHashSet());
  }
}