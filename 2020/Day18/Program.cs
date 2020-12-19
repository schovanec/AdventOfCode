using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Day18
{
    class Program
    {
        static void Main(string[] args)
        {
            var file = args.DefaultIfEmpty("input.txt").First();

            var input = File.ReadAllLines(file);

            var result1 = input.Select(x => Expression.Parse(x))
                               .Select(x => x.Evaluate())
                               .Sum();
            Console.WriteLine($"Part 1 Result = {result1}");

            var result2 = input.Select(x => Expression.Parse(x, true))
                               .Select(x => x.Evaluate())
                               .Sum();
            Console.WriteLine($"Part 1 Result = {result2}");
        }

        abstract record Expression()
        {
            public abstract long Evaluate();

            public static Expression Parse(ReadOnlySpan<char> input, bool timesSplitsTerms = false)
            {
                var stack = new Stack<(Expression expression, char? op, bool paren)>();
                var lastExpression = default(Expression);
                var lastOperator = default(char?);

                void CombineWithLast(Expression current)
                {
                    lastExpression = lastExpression == null
                        ? current
                        : new OperatorExpression(
                            lastOperator ?? throw new FormatException(),
                            lastExpression,
                            current);
                }

                void PopAll()
                {
                    bool paren = false;
                    while (!paren && stack.Count > 0)
                    {
                        var current = lastExpression;
                        (lastExpression, lastOperator, paren) = stack.Pop();

                        CombineWithLast(current);
                    }
                }

                while (input.Length > 0)
                {
                    var ch = input[0];
                    switch (ch)
                    {
                        case '+':
                            input = input.Slice(1);
                            lastOperator = ch;
                            break;

                        case '*':
                            input = input.Slice(1);
                            if (timesSplitsTerms)
                            {
                                stack.Push((lastExpression, ch, false));
                                lastExpression = null;
                            }
                            else
                            {
                                lastOperator = ch;
                            }
                            break;

                        case '(':
                            input = input.Slice(1);
                            stack.Push((lastExpression, lastOperator, true));
                            lastExpression = null;
                            lastOperator = null;
                            break;

                        case ')':
                            input = input.Slice(1);
                            PopAll();
                            lastOperator = null;
                            break;

                        case ' ':
                            input = input.Slice(1);
                            break;

                        case char when char.IsDigit(ch):
                            CombineWithLast(new UnitExpression(ParseNumber(input, out input)));
                            break;

                        default:
                            throw new FormatException();
                    }
                }

                PopAll();

                if (stack.Count > 0)
                    throw new FormatException();

                return lastExpression;
            }

            private static long ParseNumber(ReadOnlySpan<char> input, out ReadOnlySpan<char> rest)
            {
                int len = 0;
                while (len < input.Length && char.IsDigit(input[len]))
                    ++len;

                if (len == 0)
                    throw new FormatException();

                rest = input.Slice(len);
                return long.Parse(input.Slice(0, len));
            }
        }

        record UnitExpression(long Value) : Expression
        {
            public override long Evaluate() => Value;

            public override string ToString()
                => Value.ToString();
        }

        record OperatorExpression(char Operator, Expression First, Expression Second) : Expression
        {
            public override long Evaluate()
                => Operator switch
                {
                    '+' => First.Evaluate() + Second.Evaluate(),
                    '*' => First.Evaluate() * Second.Evaluate(),
                    _   => throw new InvalidOperationException()
                };

            public override string ToString()
                => $"({First.ToString()} {Operator} {Second.ToString()})";
        }
    }
}
