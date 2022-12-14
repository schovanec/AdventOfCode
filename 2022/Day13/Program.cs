using System.Collections.Immutable;

var input = ParseInput(File.ReadLines(args.FirstOrDefault() ?? "input.txt")).ToList();

var result1 = from i in Enumerable.Range(0, input.Count)
              let pair = input[i]
              where pair[0].CompareTo(pair[1]) <= 0
              select i + 1;

Console.WriteLine($"Part 1 Result = {result1.Sum()}");

var dividers = new[] { ParseList("[[2]]", out _), ParseList("[[6]]", out _) };
var result2 = input.SelectMany(x => x)
                   .Concat(dividers)
                   .OrderBy(x => x)
                   .Select((x, i) => (item: x, pos: i + 1))
                   .Where(x => dividers.Contains(x.item))
                   .Select(x => x.pos)
                   .Aggregate((a, b) => a * b);

Console.WriteLine($"Part 2 Result = {result2}");

static IEnumerable<ImmutableArray<ListItem>> ParseInput(IEnumerable<string> input)
  => input.Where(x => !string.IsNullOrEmpty(x))
          .Select(line => ParseList(line, out _))
          .Chunk(2)
          .Select(p => p.ToImmutableArray());

static ListItem ParseList(ReadOnlySpan<char> input, out ReadOnlySpan<char> tail)
{
  input = input[1..];
  var result = ImmutableList.CreateBuilder<Item>();
  while (input.Length > 0)
  {
    var ch = input[0];
    if (ch == ']')
      break;
    else if (ch == ',')
      input = input[1..];
    else
      result.Add(ParseItem(input, out input));
  }
  tail = input[1..];

  return new ListItem(result.ToImmutable());
}

static NumberItem ParseNumber(ReadOnlySpan<char> input, out ReadOnlySpan<char> tail)
{
  var length = input.IndexOfAny(',', ']');
  tail = input[length..];
  return new NumberItem(int.Parse(input[..length]));
}

static Item ParseItem(ReadOnlySpan<char> input, out ReadOnlySpan<char> tail)
  => input[0] switch
  {
    '[' => ParseList(input, out tail),
    _ => ParseNumber(input, out tail)
  };

abstract record Item : IComparable<Item>
{
  public int CompareTo(Item? other)
    => (this, other) switch
    {
      (NumberItem num1, NumberItem num2) => num1.Value.CompareTo(num2.Value),
      (ListItem list1, ListItem list2) => list1.CompareTo(list2),
      (NumberItem num, ListItem list) => num.ToListItem().CompareTo(list),
      (ListItem list, NumberItem num) => list.CompareTo(num.ToListItem()),
      _ => throw new InvalidOperationException()
    };
}

record NumberItem(int Value) : Item
{
  public ListItem ToListItem() => new(ImmutableList.Create(this as Item));
}

record ListItem(ImmutableList<Item> Items) : Item, IComparable<ListItem>
{
  public int CompareTo(ListItem? other)
  {
    other ??= new(ImmutableList<Item>.Empty);

    var i = 0;
    while (i < Items.Count && i < other.Items.Count)
    {
      var cmp = Items[i].CompareTo(other.Items[i]);
      if (cmp != 0)
        return cmp;

      ++i;
    }

    return Items.Count.CompareTo(other.Items.Count);
  }
}