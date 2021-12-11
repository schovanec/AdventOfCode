using System.Text;

var input = File.ReadLines(args.FirstOrDefault() ?? "input.txt").ToList();

var invalidScore = input
  .Select(Parse)
  .Where(x => x.result == Result.Invalid)
  .Select(x => x.completion[0])
  .Sum(ch => ch switch { ')' => 3, ']' => 57, '}' => 1197, '>' => 25137, _ => 0 });

Console.WriteLine($"Part 1 Result = {invalidScore}");

var incompleteScores = input
  .Select(Parse)
  .Where(x => x.result == Result.Ok)
  .Select(x => x.completion)
  .Select(x => x.Aggregate(0L, (prev, ch) => (prev * 5) + ch switch { ')' => 1, ']' => 2, '}' => 3, '>' => 4, _ => 0 }))
  .OrderBy(n => n)
  .ToList();

var incompleteScore = incompleteScores[incompleteScores.Count / 2];

Console.WriteLine($"Part 1 Result = {incompleteScore}");

(Result result, string completion) Parse(string line)
{
  var stack = new Stack<char>();

  foreach (var ch in line)
  {
    if (stack.Count > 0 && ch == stack.Peek())
    {
      stack.Pop();
    }
    else
    {
      var closing = ch switch { '(' => ')', '[' => ']', '{' => '}', '<' => '>', _ => default(char?) };
      if (closing.HasValue)
        stack.Push(closing.Value);
      else
        return (Result.Invalid, ch.ToString());
    }
  }

  var completion = new StringBuilder();
  while (stack.Count > 0)
    completion.Append(stack.Pop());

  return (Result.Ok, completion.ToString());
}

enum Result { Invalid, Ok };