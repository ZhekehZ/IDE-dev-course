using System;
using System.Collections.Generic;
using System.Linq;

namespace ArithmeticExpressionParser.Tests
{
    public class RandomExpressionGenerator
    {
        private readonly Random _random ;
        private string[] _operators;
        private (string, int?)[] _functions;

        public RandomExpressionGenerator(int seed)
        {
            _random = new Random(seed);
        }

        public void SetOperators(params string[] operators)
        {
            _operators = operators;
        }
        
        public void SetFunctions(params string[] functions)
        {
            _functions = functions.Select(name => (name, (int?)null)).ToArray();
        }

        
        public void SetFunctionsWithFixedArity(params (string, int)[] functions)
        {
            _functions = functions.Select(nameArity => (nameArity.Item1, (int?)nameArity.Item2)).ToArray();
        }
        
        public IExpression GetRandom(int iterations, string foldingOperator, Action<IExpression> additionalCallback = null)
        {
            var expressions = new Stack<IExpression>();
            
            for (var i = 0; i < iterations; i++)
            {
                switch (_random.Next(5))
                {
                    case 0: // Literal
                        expressions.Push(new Literal(_random.Next(100000).ToString()));
                        break;
                    
                    case 1: // Variable
                        const string chars = "ASDFGH";
                        var name = new string(Enumerable.Repeat(chars, 1 + _random.Next(10))
                            .Select(s => s[_random.Next(s.Length)]).ToArray());
                        expressions.Push(new Variable(name));
                        break;
                    
                    case 2: // Binary operator
                        if (expressions.Count > 1 && _operators.Length > 0)
                        {
                            var op = _operators[_random.Next(_operators.Length)];
                            expressions.Push(new BinaryExpression(
                                op,
                                expressions.Pop(), 
                                expressions.Pop()));
                        }
                        break;
                    
                    case 3:
                        if (_functions.Length == 0)
                        {
                            break;;
                        }
                        var (fun, arity) = _functions[_random.Next(_functions.Length)];

                        arity ??= _random.Next(expressions.Count);

                        if (expressions.Count < arity)
                        {
                            break;
                        }

                        var arguments = expressions.Take((int) arity).ToList();
                        for (var j = 0; j < arity; j++)
                        {
                            expressions.Pop();
                        }
                        expressions.Push(new FunctionCallExpression(fun, arguments));
                        break;
                    
                    case 4: // Additional check for the head expression
                        if (expressions.Count > 0)
                        {
                            additionalCallback?.Invoke(expressions.Peek());
                        }

                        break;
                }                
            }

            while (expressions.Count > 1)
            {
                expressions.Push(new BinaryExpression(foldingOperator, 
                    expressions.Pop(), 
                    expressions.Pop()));
            }

            return expressions.Pop();
        }
    }
}