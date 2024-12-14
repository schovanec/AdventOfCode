var games = Game.ParseMany(File.ReadAllLines(args.FirstOrDefault() ?? "input.txt"))
                .ToList();

var result1 = games.Sum(g => FindBestCost(g, CalculateCost));
Console.WriteLine($"Part 1 Result = {result1}");

static int FindBestCost(Game game, Func<int, int, int> costFunction)
  => game.EnumValidMoves()
         .Select(x => costFunction(x.a, x.b))
         .DefaultIfEmpty(0)
         .Min();

static int CalculateCost(int aPresses, int bPresses)
  => (aPresses * 3) + (bPresses * 1);

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
  public IEnumerable<(int a, int b)> EnumValidMoves()
    => from i in Enumerable.Range(0, MaxButtonA)
       from j in Enumerable.Range(0, MaxButtonB)
       let endX = (ButtonA.X * i) + (ButtonB.X * j)
       let endY = (ButtonA.Y * i) + (ButtonB.Y * j)
       where Prize == new Vector(endX, endY)
       select (i, j);

  public int MaxButtonA => (int)Math.Min(Prize.X / ButtonA.X + 1, Prize.Y / ButtonA.Y + 1);

  public int MaxButtonB => (int)Math.Min(Prize.X / ButtonB.X + 1, Prize.Y / ButtonB.Y + 1);

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