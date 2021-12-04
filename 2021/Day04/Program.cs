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
  var winners = boards.Where(b => b.IsWinning(called));
  var anyWinner = winners.FirstOrDefault();

  if (anyWinner != null)
  {
    if (!firstWinnerScore.HasValue)
      firstWinnerScore = anyWinner.CalculateScore(called, number);

    lastWinnerScore = anyWinner.CalculateScore(called, number);

    boards = boards.RemoveRange(winners);
  }
}

Console.WriteLine($"Part 1 Result = {firstWinnerScore}");
Console.WriteLine($"Part 2 Result = {lastWinnerScore}");

(ImmutableList<int> numbers, ImmutableList<Board> boards) ParseInput(IList<string> input)
{
  var numbers = ParseAllNumbers(input.Take(1), ',').ToImmutableList();

  var boards = ImmutableList.CreateBuilder<Board>();
  var buffer = new List<string>();
  foreach (var line in input.Skip(1))
  {
    if (string.IsNullOrWhiteSpace(line))
      AddBoard();
    else
      buffer.Add(line);
  }

  AddBoard();

  return (
    numbers,
    boards.ToImmutable()
  );

  void AddBoard()
  {
    if (buffer.Count > 0)
      boards.Add(new Board(ParseAllNumbers(buffer, ' ').ToImmutableArray()));

    buffer.Clear();
  }
}

IEnumerable<int> ParseAllNumbers(IEnumerable<string> lines, char separator)
  => lines.SelectMany(x => x.Split(separator, StringSplitOptions.RemoveEmptyEntries))
          .Select(int.Parse);

record Board(ImmutableArray<int> Numbers)
{
  public const int Size = 5;

  public int At(int col, int row)
    => Numbers[(row * 5) + col];

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
