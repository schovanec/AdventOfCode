using System.Collections.Immutable;

var cards = File.ReadAllLines(args.FirstOrDefault() ?? "input.txt")
                .Select(Card.Parse)
                .ToArray();

var result1 = cards.Sum(x => x.Score);
Console.WriteLine($"Part 1 Result = {result1}");

var result2 = PlayGame(cards);
Console.WriteLine($"Part 2 Result = {result2}");

int PlayGame(IEnumerable<Card> cards)
{
  var winCounts = cards.ToDictionary(c => c.Id, c => c.OurWinningNumbers.Count());
  var cardCounts = cards.ToDictionary(c => c.Id, _ => 0);

  var queue = new Queue<int>(cards.Select(c => c.Id));

  while (queue.Count > 0)
  {
    var id = queue.Dequeue();
    cardCounts[id]++;

    for (var i = 1; i <= winCounts[id]; ++i)
      queue.Enqueue(id + i);
  }

  return cardCounts.Values.Sum();
}

record Card(int Id, ImmutableHashSet<int> Winning, ImmutableHashSet<int> Yours)
{
  public IEnumerable<int> OurWinningNumbers => Winning.Intersect(Yours);

  public int Score
    => OurWinningNumbers.Count() is int n and > 0
      ? 1 << (n - 1)
      : 0;

  public static Card Parse(string input)
  {
    var parts = input.Split(':', 2, StringSplitOptions.TrimEntries);

    var id = int.Parse(parts[0][4..]);

    var lists = parts[1].Split('|', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Select(ParseNumberList)
                        .ToArray();

    return new(id, lists[0], lists[1]);
  }

  private static ImmutableHashSet<int> ParseNumberList(string list)
    => list.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
           .Select(int.Parse)
           .ToImmutableHashSet();
}