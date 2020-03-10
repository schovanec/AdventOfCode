using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace day12
{
    class Program
    {
        static void Main(string[] args)
        {
#if true
            // puzzle input
            var input = new (long, long, long)[]
            {
                (-5,  6, -11),
                (-8, -4,  -2),
                ( 1, 16,   4),
                (11, 11,  -4)
            };

            const int steps = 1000;
#elif true
            // example 2
            var input = new (long, long, long)[]
            {
                (-8, -10, 0),
                (5, 5, 10),
                (2, -7, 3),
                (9, -8, -3)
            };

            const int steps = 100;
#else
            // example 1
            var input = new (long, long, long)[]
            {
                (-1, 0, 2),
                (2, -10, -7),
                (4, -8, 8),
                (3, 5, -1)
            };

            const int steps = 10;
#endif

            RunPart1(input, steps);

            RunPart2(input);
        }

        private static void RunPart1((long, long, long)[] input, int steps)
        {
            var moons = input.Select(p => new Moon(p)).ToArray();
            var pairs = moons.SelectMany(m => moons, (m1, m2) => (first: m1, second: m2)).Where(p => p.first != p.second).ToArray();

            for (int step = 0; step < steps; ++step)
            {
                foreach (var pair in pairs)
                    pair.first.ApplyGravity(pair.second);

                foreach (var moon in moons)
                    moon.StepLocation();
            }

            var totalEnergy = moons.Sum(m => m.TotalEnergy);
            Console.WriteLine($"Total Energy: {totalEnergy}");
        }

        private static void RunPart2((long x, long y, long z)[] moons)
        {
            var axisStepCounts = new[]
            {
                CountStepsToRepeatOnAxis(moons.Select(m => m.x)),
                CountStepsToRepeatOnAxis(moons.Select(m => m.y)),
                CountStepsToRepeatOnAxis(moons.Select(m => m.z))
            };

            Console.WriteLine($"Steps: X={axisStepCounts[0]}, Y={axisStepCounts[1]}, Z={axisStepCounts[2]}");

            var totalStepsToStart = axisStepCounts.Aggregate(LeastCommonMultipler);
            Console.WriteLine($"Part 2 Result = {totalStepsToStart}");
        }

        private static long LeastCommonMultipler(IEnumerable<long> values)
            => values.Aggregate(LeastCommonMultipler);

        private static long LeastCommonMultipler(long a, long b)
            => (a * b) / GreatestCommonDivisor(a, b);

        private static long GreatestCommonDivisor(long a, long b)
            => a % b == 0 ? b : GreatestCommonDivisor(b, a % b);

        private static long CountStepsToRepeatOnAxis(IEnumerable<long> initial)
        {
            var pos = initial.ToArray();
            var count = pos.Length;
            var velocity = new long[count];
            long steps = 0;

            do
            {
                for (int i = 0; i < count; ++i)
                {
                    for (int j = i + 1; j < count; ++j)
                    {
                        velocity[i] += Math.Sign(pos[j] - pos[i]);
                        velocity[j] += Math.Sign(pos[i] - pos[j]);
                    }
                }

                for (int i = 0; i < count; ++i)
                    pos[i] += velocity[i];

                ++steps;

            } while (!pos.SequenceEqual(initial) || !velocity.All(v => v.Equals(default)));

            return steps;
        }
    }

    sealed class Moon
    {
        public Moon(Vector position)
        {
            Position = position;
        }

        public Vector Position { get; private set; }

        public Vector Velocity { get; private set; }

        public long PotentialEnergy => Position.EnergyValue;

        public long KineticEnergy => Velocity.EnergyValue;

        public long TotalEnergy => PotentialEnergy * KineticEnergy;

        public void ApplyGravity(Moon other)
        {
            Velocity = Velocity.Add(
                dx: Math.Sign(other.Position.X - Position.X),
                dy: Math.Sign(other.Position.Y - Position.Y),
                dz: Math.Sign(other.Position.Z - Position.Z)
            );
        }

        public void StepLocation()
        {
            Position = Position.Add(Velocity);
        }
    }

    readonly struct Vector : IEquatable<Vector>
    {
        public Vector(long x, long y, long z) => (X, Y, Z) = (x, y, z);

        public long X { get; }

        public long Y { get; }

        public long Z { get; }

        public Vector Add(long dx, long dy, long dz) => new Vector(X + dx, Y + dy, Z + dz);

        public Vector Add(Vector other) => Add(other.X, other.Y, other.Z);

        public long EnergyValue => Math.Abs(X) + Math.Abs(Y) + Math.Abs(Z);

        public bool Equals(Vector other) => (X, Y, Z) == (other.X, other.Y, other.Z);

        public override bool Equals(object obj) => obj is Vector other && Equals(other);

        public override int GetHashCode() => (X, Y, Z).GetHashCode();

        public override string ToString() => $"({X}, {Y}, {Z})";

        public void Deconstruct(out long x, out long y, out long z) => (x, y, z) = (X, Y, Z);

        public static implicit operator Vector(in (long x, long y, long z) p)
            => new Vector(p.x, p.y, p.z);
    }
}
