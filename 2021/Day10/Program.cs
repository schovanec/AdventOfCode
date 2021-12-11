var input = File.ReadLines(args.FirstOrDefault() ?? "input.txt").ToList();

int totalInvalidScore = 0;
var incompleteScoreList = new List<long>();
foreach (var line in input)
{
  var (result, tail) = Parse(new ParseInput(line));
  if (result.State == ParseState.Invalid && !tail.IsEmpty)
  {
    totalInvalidScore += GetInvalidPoints(tail.Head);
  }
  else if (result.State == ParseState.Incomplete)
  {
    incompleteScoreList.Add(
      result.Completion.Aggregate(0L, (prev, ch) => (prev * 5) + GetIncompletePoints(ch)));
  }
};

Console.WriteLine($"Part 1 Result = {totalInvalidScore}");

incompleteScoreList.Sort();
var incompleteWinner = incompleteScoreList[incompleteScoreList.Count / 2];

Console.WriteLine($"Part 2 Result = {incompleteWinner}");

(ParseResult result, ParseInput tail) Parse(ParseInput text, char? expectedClosingChar = null)
{
  while (!text.IsEmpty)
  {
    var ch = text.Head;
    if (ch == expectedClosingChar)
    {
      return (ParseResult.Success, text.Tail);
    }
    else if (GetClosingChar(ch) is char closingChar)
    {
      (var result, text) = Parse(text.Tail, closingChar);
      if (result.State == ParseState.Invalid)
        return (result, text);
      else if (result.State == ParseState.Incomplete)
        return (result.AppendToCompletion(expectedClosingChar), default);
    }
    else
    {
      return (ParseResult.Invalid, text);
    }
  }

  var finalResult = expectedClosingChar.HasValue
    ? ParseResult.Incomplete(expectedClosingChar.Value)
    : ParseResult.Success;

  return (finalResult, default);
}

char? GetClosingChar(char ch) => ch switch { '(' => ')', '[' => ']', '{' => '}', '<' => '>', _ => null };

int GetInvalidPoints(char ch) => ch switch { ')' => 3, ']' => 57, '}' => 1197, '>' => 25137, _ => 0 };

int GetIncompletePoints(char ch) => ch switch { ')' => 1, ']' => 2, '}' => 3, '>' => 4, _ => 0 };

record struct ParseInput(string Text, int Offset = 0)
{
  public bool IsEmpty => Text == null || Offset >= Text.Length;

  public char Head => Text[Offset];

  public ParseInput Tail => IsEmpty ? this : this with { Offset = Offset + 1 };

  public override string ToString()
    => IsEmpty ? "" : Text.Substring(Offset);
}

enum ParseState { Success, Incomplete, Invalid };

record struct ParseResult(ParseState State, string Completion = "")
{
  public static ParseResult Success => new ParseResult(ParseState.Success);

  public static ParseResult Invalid => new ParseResult(ParseState.Invalid);

  public static ParseResult Incomplete(char expected, string? existingCompletion = null)
    => new ParseResult(ParseState.Incomplete, (existingCompletion ?? "") + expected);

  public ParseResult AppendToCompletion(char? expected)
  {
    if (!expected.HasValue)
      return this;

    var newCompletion = (Completion ?? "") + expected.Value;
    return this with { Completion = newCompletion };
  }
}
