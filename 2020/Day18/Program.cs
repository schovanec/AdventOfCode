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
            var expressions = File.ReadLines(file)
                                  .Select(x => Expression.Parse(x))
                                  .ToList();

            var sum = expressions.Select(x => x.Evaluate()).Sum();
            Console.WriteLine($"Part 1 Result = {sum}");
        }

        abstract record Expression()
        {
            public abstract long Evaluate();

            public static Expression Parse(ReadOnlySpan<char> input, bool )
            {
                var stack = new Stack<(Expression expression, char? op)>();
                var lastExpression = default(Expression);
                var lastOperator = default(char?);
                while (input.Length > 0)
                {
                    var ch = input[0];
                    switch (ch)
                    {
                        case '+':
                        case '*':
                            lastOperator = ch;
                            input = input.Slice(1);
                            break;

                        case '(':
                            stack.Push((lastExpression, lastOperator));
                            lastExpression = null;
                            lastOperator = null;
                            input = input.Slice(1);
                            break;

                        case ')':
                            var (prevLastExp, prevOp) = stack.Pop();
                            if (prevLastExp != null)
                            {
                                lastExpression = new OperatorExpression(
                                    prevOp ?? throw new FormatException(),
                                    prevLastExp,
                                    lastExpression);
                            
                                lastOperator = null;
                            }
                            input = input.Slice(1);
                            break;

                        case ' ':
                            input = input.Slice(1);
                            break;

                        case char when char.IsDigit(ch):
                            var numberExpression = new UnitExpression(ParseNumber(input, out input));
                            if (lastExpression != null)
                            {
                                lastExpression = new OperatorExpression(
                                    lastOperator ?? throw new FormatException(),
                                    lastExpression,
                                    numberExpression);

                                lastOperator = null;
                            }
                            else
                            {
                                lastExpression = numberExpression;
                            }
                            break;

                        default:
                            throw new FormatException();
                    }
                }

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
        }
    }
}
