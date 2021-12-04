using System.Collections.Immutable;

var (numbers, boards) = ParseInput(File.ReadLines(args.FirstOrDefault() ?? "input.txt"));

var called = new HashSet<int>();
var firstWinnerScore = default(int?);
var lastWinnerScore = default(int?);
foreach (var number in numbers)
{
  if (boards.IsEmpty)
    break;

  called.Add(number);
  var winners = FindWinningBoards(boards, called);
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

IEnumerable<Board> FindWinningBoards(IEnumerable<Board> boards, IReadOnlySet<int> called)
  => boards.Where(b => b.IsWinning(called));

(ImmutableList<int> numbers, ImmutableList<Board> boards) ParseInput(IEnumerable<string> input)
{
  var numbers = ImmutableList<int>.Empty;
  var boards = ImmutableList.CreateBuilder<Board>();

  var buffer = new List<string>();
  foreach (var line in input)
  {
    if (string.IsNullOrWhiteSpace(line))
      ProcessBuffer();
    else
      buffer.Add(line);
  }

  ProcessBuffer();

  return (
    numbers,
    boards.ToImmutable()
  );

  void ProcessBuffer()
  {
    if (numbers.IsEmpty)
      numbers = ParseAllNumbers(buffer, ',').ToImmutableList();
    else if (buffer.Count > 0)
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
