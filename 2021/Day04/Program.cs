using System.Collections.Immutable;

var (numbers, boards) = ParseInput(File.ReadAllLines(args.FirstOrDefault() ?? "input.txt"));

var called = new HashSet<int>();
var firstWinnerScore = default(int?);
var lastWinnerScore = default(int?);
foreach (var number in numbers)
{
  if (boards.IsEmpty)
    break;

  called.Add(number);
  var allWinners = boards.Where(b => b.IsWinning(called));
  var anyWinner = allWinners.FirstOrDefault();

  if (anyWinner != null)
  {
    if (!firstWinnerScore.HasValue)
      firstWinnerScore = anyWinner.CalculateScore(called, number);

    lastWinnerScore = anyWinner.CalculateScore(called, number);

    boards = boards.RemoveRange(allWinners);
  }
}

Console.WriteLine($"Part 1 Result = {firstWinnerScore}");
Console.WriteLine($"Part 2 Result = {lastWinnerScore}");

(ImmutableList<int> numbers, ImmutableList<Board> boards) ParseInput(IList<string> input)
{
  var numbers = ParseAllNumbers(input.Take(1), ',').ToImmutableList();

  var boards = Split(input.Skip(1), string.IsNullOrWhiteSpace)
    .Select(g => new Board(ParseAllNumbers(g, ' ').ToImmutableArray()))
    .ToImmutableList();

  return (numbers, boards);
}

IEnumerable<IEnumerable<T>> Split<T>(IEnumerable<T> array, Predicate<T> predicate)
{
  var buffer = ImmutableArray.CreateBuilder<T>();
  foreach (var item in array)
  {
    if (predicate(item))
    {
      if (buffer.Count > 0)
      {
        yield return buffer.ToImmutable();
        buffer.Clear();
      }
    }
    else
    {
      buffer.Add(item);
    }
  }

  if (buffer.Count > 0)
    yield return buffer.ToImmutable();
}

IEnumerable<int> ParseAllNumbers(IEnumerable<string> lines, char separator)
  => lines.SelectMany(x => x.Split(separator, StringSplitOptions.RemoveEmptyEntries))
          .Select(int.Parse);

record Board(ImmutableArray<int> Numbers)
{
  public const int Size = 5;

  public int At(int col, int row)
    => Numbers[(row * Size) + col];

  public bool HasMarkedRow(IReadOnlySet<int> called)
    => Enumerable.Range(0, Size)
                 .Any(i => IsMarkedRow(i, called));

  public bool IsMarkedRow(int row, IReadOnlySet<int> called)
    => Enumerable.Range(0, Size)
                 .All(i => called.Contains(At(i, row)));

  public bool HasMarkedColumn(IReadOnlySet<int> called)
    => Enumerable.Range(0, Size)
                 .Any(i => IsMarkedColumn(i, called));

  public bool IsMarkedColumn(int column, IReadOnlySet<int> called)
    => Enumerable.Range(0, Size)
                 .All(i => called.Contains(At(column, i)));

  public bool IsWinning(IReadOnlySet<int> called)
    => HasMarkedRow(called)
    || HasMarkedColumn(called);

  public int CalculateScore(IReadOnlySet<int> called, int lastCalled)
    => Numbers.Except(called).Sum() * lastCalled;
}
