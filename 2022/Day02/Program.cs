var input = File.ReadLines(args.FirstOrDefault() ?? "input.txt")
                .Select(x => (first: x[0], second: x[2]))
                .ToList();

var scoresPart1 = from item in input
                  let opponent = ParseMove(item.first)
                  let player = ParseMove(item.second)
                  let outcome = GetOutcome(player, opponent)
                  select CalculateScore(player, outcome);

Console.WriteLine($"Part 1 Result = {scoresPart1.Sum()}");

var scoresPart2 = from item in input
                  let opponent = ParseMove(item.first)
                  let outcome = ParseOutcome(item.second)
                  let player = GetMoveForDesiredOutcome(opponent, outcome)
                  select CalculateScore(player, outcome);

Console.WriteLine($"Part 2 Result = {scoresPart2.Sum()}");

static int CalculateScore(Move playerMove, Outcome outcome)
  => GetMoveScore(playerMove) + GetOutcomeScore(outcome);

static int GetMoveScore(Move move) => (int)move + 1;

static int GetOutcomeScore(Outcome outcome) => (int)outcome * 3;

static Outcome GetOutcome(Move player, Move opponent)
  => (player, opponent) switch
  {
    (Move.Rock, Move.Scissors) => Outcome.Win,
    (Move.Scissors, Move.Paper) => Outcome.Win,
    (Move.Paper, Move.Rock) => Outcome.Win,
    (Move x, Move y) when x == y => Outcome.Draw,
    _ => Outcome.Lose
  };

static Move GetMoveForDesiredOutcome(Move opponent, Outcome desiredOutcome)
  => desiredOutcome switch
  {
    Outcome.Win => (Move)(((int)opponent + 1) % 3),
    Outcome.Lose => (Move)(((int)opponent + 2) % 3),
    _ => opponent,
  };

static Move ParseMove(char input)
  => input switch
  {
    'A' or 'X' => Move.Rock,
    'B' or 'Y' => Move.Paper,
    'C' or 'Z' => Move.Scissors,
    _ => throw new ArgumentException("Invalid move", nameof(input))
  };

static Outcome ParseOutcome(char input)
  => input switch
  {
    'X' => Outcome.Lose,
    'Y' => Outcome.Draw,
    'Z' => Outcome.Win,
    _ => throw new ArgumentException("Invalid outcome", nameof(input))
  };

enum Move { Rock, Paper, Scissors }

enum Outcome { Lose, Draw, Win }
