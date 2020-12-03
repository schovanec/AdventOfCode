using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Day02
{
    class Program
    {
        static void Main(string[] args)
        {
            var file = args.DefaultIfEmpty("input.txt").First();

            var passwords = from line in File.ReadLines(file)
                            let m = Regex.Match(line, @"^(?<min>\d+)-(?<max>\d+)\s+(?<char>.): (?<password>.*)$")
                            where m.Success
                            select new
                            {
                                PolicyValue1 = int.Parse(m.Groups["min"].Value),
                                PolicyValue2 = int.Parse(m.Groups["max"].Value),
                                Letter = m.Groups["char"].Value[0],
                                Password = m.Groups["password"].Value
                            };

            var validPasswordCountMethod1 = (from pw in passwords
                                             let count = pw.Password.Count(x => x == pw.Letter)
                                             where count >= pw.PolicyValue1 && count <= pw.PolicyValue2
                                             select pw).Count();

            Console.WriteLine($"Valid Passwords With Method 1: {validPasswordCountMethod1}");

            var validPasswordCountMethod2 = (from pw in passwords
                                             let char1 = SafeCharAt(pw.Password, pw.PolicyValue1 - 1)
                                             let char2 = SafeCharAt(pw.Password, pw.PolicyValue2 - 1)
                                             where char1 != char2
                                                && (char1 == pw.Letter || char2 == pw.Letter)
                                             select pw).Count();

            Console.WriteLine($"Valid Passwords With Method 2: {validPasswordCountMethod2}");
        }

        static char? SafeCharAt(string text, int pos)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            return pos >= 0 && pos < text.Length
                ? text[pos]
                : default(char?);
        }
    }
}
