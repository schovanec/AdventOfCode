var counts = File.ReadAllLines(args.FirstOrDefault() ?? "input.txt")
                 .Select(CountWinningNumbers)
                 .ToArray();

var result1 = counts.Where(x => x > 0).Sum(x => 1 << (x - 1));
Console.WriteLine($"Part 1 Result = {result1}");

var result2 = PlayGame(counts);
Console.WriteLine($"Part 2 Result = {result2}");

int PlayGame(IList<int> counts)
{
  var queue = new Queue<int>(Enumerable.Range(0, counts.Count));
  var totalCards = 0;
  while (queue.Count > 0)
  {
    var cardIndex = queue.Dequeue();
    ++totalCards;

    for (var i = 1; i <= counts[cardIndex]; ++i)
      queue.Enqueue(cardIndex + i);
  }

  return totalCards;
}

int CountWinningNumbers(string card)
{
  var numbers = card.Split(':', 2)[1];
  return numbers.Split('|', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                     .Select(ParseNumberList)
                     .Aggregate((a, b) => a.Intersect(b))
                     .Count();
}

IEnumerable<int> ParseNumberList(string list)
  => list.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(int.Parse);