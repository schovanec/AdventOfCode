var plays = File.ReadLines(args.FirstOrDefault() ?? "input.txt")
                .Select(x => (player: ParseMove(x[2]), opponent: ParseMove(x[0])));

var totalScore = plays.Sum(x => CalculateScore(x.player, x.opponent));

Console.WriteLine($"Part 1 Result = {totalScore}");

static int CalculateScore(Move player, Move opponent)
  => GetMoveScore(player) + GetOutcomeScore(GetOutcome(player, opponent));

static int GetMoveScore(Move move) => (int)move;

static int GetOutcomeScore(Outcome outcome) => (int)outcome;

static Outcome GetOutcome(Move player, Move opponent)
  => (player, opponent) switch
  {
    (Move.Rock, Move.Scissors) => Outcome.Win,
    (Move.Scissors, Move.Paper) => Outcome.Win,
    (Move.Paper, Move.Rock) => Outcome.Win,
    (Move x, Move y) when x == y => Outcome.Draw,
    _ => Outcome.Lose
  };

static Move ParseMove(char input)
  => input switch
  {
    'A' or 'X' => Move.Rock,
    'B' or 'Y' => Move.Paper,
    'C' or 'Z' => Move.Scissors,
    _ => throw new ArgumentException("Invalid move", nameof(input))
  };

enum Move { Rock = 1, Paper = 2, Scissors = 3 }

enum Outcome { Lose = 0, Draw = 3, Win = 6 }
