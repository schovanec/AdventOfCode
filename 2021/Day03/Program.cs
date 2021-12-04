using System.Collections.Immutable;

var input = File.ReadLines(args.FirstOrDefault() ?? "input.txt").ToImmutableList();

var indexes = Enumerable.Range(0, input.First().Length);

var gamma = ParseBits(indexes.Select(i => FindMostCommon(input, i)));
var epsilon = ParseBits(indexes.Select(i => FindLeastCommon(input, i)));

Console.WriteLine($"Part 1 Result = {gamma * epsilon}");

var oxygenRatingString = FilterList(input, FindMostCommon).Single();
var scrubberRatingString = FilterList(input, FindLeastCommon).Single();

var oxygenRating = ParseBitString(oxygenRatingString);
var scrubberRating = ParseBitString(scrubberRatingString);

Console.WriteLine($"Part 2 Result = {oxygenRating * scrubberRating}");

char FindMostCommon(ICollection<string> input, int index)
{
  var count1 = input.Count(x => x[index] == '1');
  var count0 = input.Count - count1;
  return count1 >= count0 ? '1' : '0';
}

char FindLeastCommon(ICollection<string> input, int index) => InvertBitChar(FindMostCommon(input, index));

char InvertBitChar(char bit) => bit switch
{
  '1' => '0',
  '0' => '1',
  _ => throw new ArgumentException($"Invalid bit '{bit}'", nameof(bit))
};

int ParseBits(IEnumerable<char> bits)
  => ParseBitString(string.Concat(bits));

int ParseBitString(string bits)
  => Convert.ToInt32(bits, 2);

ImmutableList<string> FilterList(ImmutableList<string> input, Func<ImmutableList<string>, int, char> selector)
{
  var size = input.FirstOrDefault()?.Length ?? 0;
  var current = input;
  for (var i = 0; i < size; ++i)
  {
    if (current.Count <= 1)
      break;

    var target = selector(current, i);
    current = current.RemoveAll(x => x[i] != target);
  }

  return current;
}