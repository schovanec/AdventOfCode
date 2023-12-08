using System.Collections.Immutable;
using Hand = System.Collections.Immutable.ImmutableArray<char>;

var hands = File.ReadLines(args.FirstOrDefault() ?? "input.txt")
                .Select(ParseHandWithBet)
                .ToArray();

var ranked1 = from h in hands
              let type = IdentifyHand(h.hand)
              let value = GetHandValue(h.hand, GetCardValue)
              orderby type, value
              select h;

var result1 = CalculateWinnings(ranked1);
Console.WriteLine($"Part 1 Result = {result1}");

var ranked2 = from h in hands
              let type = IdentifyHandWithJokers(h.hand)
              let value = GetHandValue(h.hand, GetCardValueWithJokers)
              orderby type, value
              select h;

var result2 = CalculateWinnings(ranked2);
Console.WriteLine($"Part 2 Result = {result2}");

int CalculateWinnings(IEnumerable<(Hand hand, int bet)> hands)
  => hands.Select((h, i) => h.bet * (i + 1)).Sum();

int IdentifyHand(Hand hand)
{
  var groups = hand.GroupBy(ch => ch)
                   .Select(g => g.Count())
                   .OrderByDescending(n => n)
                   .ToArray();

  return groups switch
  {
  [5] => 7,
  [4, 1] => 6,
  [3, 2] => 5,
  [3, 1, 1] => 4,
  [2, 2, 1] => 3,
  [2, 1, 1, 1] => 2,
    _ => 1
  };
}

int IdentifyHandWithJokers(Hand hand)
  => GetJokerReplacements(hand).Max(IdentifyHand);

int GetCardValue(char card)
  => "23456789TJQKA".IndexOf(card) + 1;

int GetCardValueWithJokers(char card)
  => "J23456789TQKA".IndexOf(card) + 1;

int GetHandValue(Hand hand, Func<char, int> value)
  => hand.Aggregate(0, (a, card) => (a << 4) | value(card));

IList<Hand> GetJokerReplacements(Hand hand)
{
  var result = new List<Hand>() { hand };

  var nonJokerCards = hand.Except(['J']).Distinct().ToArray();
  if (nonJokerCards.Length > 0)
    Search(hand, 0, nonJokerCards);

  return result;

  void Search(Hand cards, int pos, char[] replacements)
  {
    if (pos >= cards.Length)
    {
      result.Add(cards);
    }
    else if (cards[pos] != 'J')
    {
      Search(cards, pos + 1, replacements);
    }
    else
    {
      foreach (var ch in replacements)
        Search(cards.SetItem(pos, ch), pos + 1, replacements);
    }
  }
}

static (Hand hand, int bet) ParseHandWithBet(string input)
{
  var split = input.Split(' ', 2);
  return new(split[0].ToImmutableArray(), int.Parse(split[1]));
}