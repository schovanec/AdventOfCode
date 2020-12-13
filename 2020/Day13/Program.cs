using System;
using System.IO;
using System.Linq;
using System.Numerics;

namespace Day13
{
    class Program
    {
        static void Main(string[] args)
        {
            var file = args.DefaultIfEmpty("input.txt").First();
            var lines = File.ReadAllLines(file);

            Part1(lines);
            Part2(lines[1]);
        }

        private static void Part1(string[] lines)
        {
            var earliestTime = int.Parse(lines[0]);
            var times = lines[1].Split(',').Where(id => id.All(char.IsDigit))
                                           .Select(int.Parse)
                                           .ToArray();

            var firstArrival = (from t in times
                                let arrival = ((earliestTime / t) + 1) * t
                                orderby arrival
                                select (id: t, arrival)).First();
            var waitingTime = firstArrival.arrival - earliestTime;
            Console.WriteLine($"First Bus = {firstArrival.id}");
            Console.WriteLine($"Waiting Time = {waitingTime}");
            Console.WriteLine($"Part 1 Result = {firstArrival.id * waitingTime}");
        }

        private static void Part2(string input)
        {
            var inputs = input.Split(',')
                              .Select((x, i) => (value: x, offset: i))
                              .Where(x => x.value.All(char.IsDigit))
                              .Select(x => (id: int.Parse(x.value), x.offset))
                              .ToArray();

            // use the chinese remainder theorm to calculate
            // https://www.freecodecamp.org/news/how-to-implement-the-chinese-remainder-theorem-in-java-db88a3f1ffe0/

            var number = (from x in inputs
                          select (BigInteger)x.id).ToArray();

            // we need to set the remainders such that the bus is /behind/ where
            // it needs to be to line up at our time offset.
            var rem = (from x in inputs
                       select (BigInteger)(((x.id - x.offset) + x.id) % x.id)).ToArray();


            // calculate the product of all of the bus numbers
            var product = number.Aggregate(BigInteger.One, (p, n) => p * n);

            // calculate the partial product of all of the bus numbers
            var partialProduct = number.Select(n => product / n)
                                       .ToArray();

            // calculate the multiplicative invers of each bus number modulo the partial product
            var inverse = number.Zip(partialProduct, (n, p) => ComputeInverse(p, n))
                                .ToArray();

            // calculate the final sum
            var sum = partialProduct.Zip(inverse, (p, i) => p * i)
                                    .Zip(rem, (x, r) => x * r)
                                    .Aggregate(BigInteger.Zero, (s, n) => s + n);

            // divide the sum by the product
            var time = sum % product;

            Console.WriteLine($"Part 2 Result = {time}");
        }

        private static BigInteger ComputeInverse(BigInteger a, BigInteger b)
        {
            BigInteger m = b, t, q;
            BigInteger x = 0, y = 1;
            if (b == BigInteger.One)
                return 0;
            
            // Apply extended Euclid Algorithm
            while (a > 1)
            {
                // q is quotient
                q = a / b;
                t = b;
                // now proceed same as Euclid's algorithm
                b = a % b;
                a = t;
                t = x;
                x = y - q * x;
                y = t;
            }
            
            // Make x1 positive
            if (y < 0)
                y += m;
                
            return y;
        }
    }
}
