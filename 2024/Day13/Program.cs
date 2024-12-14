var games = Game.ParseMany(File.ReadAllLines(args.FirstOrDefault() ?? "input.txt")) //@"..\..\..\sample1.txt"))
                .ToList();

var result1 = games.Sum(g => FindBest(g, CalculateCost));
Console.WriteLine($"Part 1 Result = {result1}");

var harder = games.Select(g => g.MovePrize(10000000000000, 10000000000000)).ToList();
var result2 = harder.Sum(g => FindBest(g, CalculateCost));
Console.WriteLine($"Part 2 Result = {result2}");

static long CalculateCost(long aPresses, long bPresses)
  => (aPresses * 3) + (bPresses * 1);

static long FindBest(Game game, Func<long, long, long> cost)
{
  var detT = Determinant(game.ButtonA, game.ButtonB);
  var detA = Determinant(game.Prize, game.ButtonB);
  var detB = Determinant(game.ButtonA, game.Prize);

  var aPresses = detA / detT;
  var bPresses = detB / detT;

  if (game.GetClawPosition(aPresses, bPresses) == game.Prize)
    return cost(aPresses, bPresses);
  else
    return 0;
}

static long Determinant(Vector a, Vector b) => (a.X * b.Y) - (b.X * a.Y);

record struct Vector(long X, long Y)
{
  public static Vector Parse(string input)
  {
    var coords = input.Split(',', 2, StringSplitOptions.TrimEntries);
    var x = int.Parse(coords[0].Split(['+', '='])[1]);
    var y = int.Parse(coords[1].Split(['+', '='])[1]);
    return new Vector(x, y);
  }
}

record Game(Vector ButtonA, Vector ButtonB, Vector Prize)
{
  public Vector GetClawPosition(long aPresses, long bPresses)
    => new Vector(
      (ButtonA.X * aPresses) + (ButtonB.X * bPresses),
      (ButtonA.Y * aPresses) + (ButtonB.Y * bPresses));

  public Game MovePrize(long offsetX, long offsetY)
    => this with { Prize = new Vector(Prize.X + offsetX, Prize.Y + offsetY) };

  public static IEnumerable<Game> ParseMany(IReadOnlyList<string> input)
  {
    using var e = input.GetEnumerator();
    while (e.MoveNext())
    {
      if (!string.IsNullOrEmpty(e.Current))
      {
        var buttonA = Vector.Parse(e.Current.Split(':', 2, StringSplitOptions.TrimEntries)[1]);

        e.MoveNext();
        var buttonB = Vector.Parse(e.Current.Split(':', 2, StringSplitOptions.TrimEntries)[1]);

        e.MoveNext();
        var prize = Vector.Parse(e.Current.Split(':', 2, StringSplitOptions.TrimEntries)[1]);

        yield return new Game(buttonA, buttonB, prize);
      }
    }
  }
}