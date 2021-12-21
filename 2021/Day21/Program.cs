using System.Collections.Immutable;

var input = File.ReadLines(args.FirstOrDefault() ?? "input.txt")
                .Select(x => x.Split(": "))
                .Select(x => int.Parse(x[1]))
                .ToList();

Part1();

Part2();

void Part1()
{
  int lastRoll = 0;
  int rollCount = 0;

  var game = new Game(new Player(input[0]), new Player(input[1]));

  var player = 0;
  while (!game.GetWinner(1000).HasValue)
  {
    var rolled = Roll() + Roll() + Roll();
    game = game.StepPractice(player, rolled);

    player = (player + 1) % 2;
  }

  var losingScore = Math.Min(game.Player1.Score, game.Player2.Score);
  Console.WriteLine($"Part 1 Result = {rollCount * losingScore}");

  int Roll()
  {
    if (lastRoll >= 100)
      lastRoll = 0;

    ++rollCount;
    return ++lastRoll;
  }
}

void Part2()
{
  var wins = new long[input.Count];
  var games = ImmutableHashSet.Create<(Game game, long count)>(
    (new Game(new Player(input[0]), new Player(input[1])), 1)
  );

  var player = 0;
  while (games.Count > 0)
  {
    var next = (from a in games
                from b in a.game.StepQuantum(player)
                group (game: b.game, count: a.count * b.count) by b.game into g
                select (game: g.Key, count: g.Sum(x => x.count))).ToImmutableHashSet();

    var newWins = (from a in next
                   let winner = a.game.GetWinner(21)
                   where winner.HasValue
                   select (player: winner.Value, games: a)).ToList();

    if (newWins.Count > 0)
    {
      foreach (var w in newWins)
        wins[w.player] += w.games.count;

      next = next.Except(newWins.Select(w => w.games));
    }

    games = next;
    player = (player + 1) % input.Count;
  }

  var winnerScore = wins.Max();
  Console.WriteLine($"Part 2 Result = {winnerScore}");
}

record Player(int Position, int Score = 0)
{
  public Player Move(int positions)
  {
    var newPosition = ((Position + positions - 1) % 10) + 1;
    var newScore = Score + newPosition;
    return new Player(newPosition, newScore);
  }

  public bool IsWin(int max) => Score >= max;
}

record Game(Player Player1, Player Player2)
{
  private static ImmutableArray<(int move, long count)> quantumMoves
    = (from i in Enumerable.Range(1, 3)
       from j in Enumerable.Range(1, 3)
       from k in Enumerable.Range(1, 3)
       let sum = i + j + k
       group sum by sum into g
       select (g.Key, g.LongCount())).ToImmutableArray();

  public int? GetWinner(int max)
  {
    if (Player1.IsWin(max))
      return 0;
    else if (Player2.IsWin(max))
      return 1;
    else
      return default;
  }

  public Game StepPractice(int player, int rollTotal)
    => player switch
    {
      0 => this with { Player1 = Player1.Move(rollTotal) },
      1 => this with { Player2 = Player2.Move(rollTotal) },
      _ => throw new InvalidOperationException()
    };

  public IEnumerable<(Game game, long count)> StepQuantum(int player)
    => player switch
    {
      0 => from m in quantumMoves
           select (this with { Player1 = Player1.Move(m.move) }, m.count),
      1 => from m in quantumMoves
           select (this with { Player2 = Player2.Move(m.move) }, m.count),
      _ => Enumerable.Empty<(Game, long)>()
    };
}
