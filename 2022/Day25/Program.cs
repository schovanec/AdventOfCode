var input = File.ReadAllLines(args.FirstOrDefault() ?? "input.txt");

var sum = input.Select(Decode).Sum();
var result = Encode(sum);
Console.WriteLine($"Part 1 Result = {result}");

static long Decode(string number)
{
  if (string.IsNullOrEmpty(number))
    return 0;

  return GetDeciamlValue(number[^1]) + (5 * Decode(number[..^1]));
}

static string Encode(long number)
{
  var digit = number % 5;
  var rest = number / 5;

  char encodedDigit;
  if (digit >= 3)
  {
    encodedDigit = GetEncodedDigit(digit - 5);
    ++rest;
  }
  else
  {
    encodedDigit = GetEncodedDigit(digit);
  }

  if (rest != 0)
    return Encode(rest) + encodedDigit;
  else
    return encodedDigit.ToString();
}

static int GetDeciamlValue(char ch)
  => ch switch
  {
    '2' => 2,
    '1' => 1,
    '0' => 0,
    '-' => -1,
    '=' => -2,
    _ => throw new ArgumentOutOfRangeException(nameof(ch))
  };

static char GetEncodedDigit(long value)
  => value switch
  {
    2 => '2',
    1 => '1',
    0 => '0',
    -1 => '-',
    -2 => '=',
    _ => throw new ArgumentOutOfRangeException(nameof(value))
  };