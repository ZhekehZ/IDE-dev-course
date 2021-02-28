using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArithmeticExpressionParser
{
    public class ArithmeticExpressionParser : IParser<IExpression>
    {
        private const string ValidOperatorSymbols = ":!@#$%^&*-+_/<>=?";
        private const char FunctionArgumentsSeparator = ',';

        private readonly string _bracketsOpen;
        private readonly string _bracketsClose;
        private readonly List<string> _functionNames;
        private readonly Dictionary<string, ushort> _operators;

        /// <summary>
        /// A parser for arithmetic expressions consisting of natural numbers, variables, functions,
        /// brackets and binary operators.
        ///
        /// - The names of variables and functions must match the following regular expression:
        /// <code>[A-Za-z]([A-Za-z0-9]*)</code>
        ///
        /// - Function call must have the following form: <code>FName Op Arg1 [, Arg2 [, Arg3 ...]] Cl</code> where
        /// FName is the name of the function, Op is the open parenthesis, ArgN is the argument,
        /// and Cl is the closing parenthesis (paired with Op)
        ///
        /// - All binary operators must consists only of <see cref="ValidOperatorSymbols"/> characters  
        /// </summary>
        /// 
        /// <param name="operators">Mapping from operator name to priority</param>
        /// <param name="brackets">
        /// Supported brackets in the following format: <code>Op1 Cl1[Op2 Cl2]</code> <example>"()[]{}"</example>
        /// </param>
        /// <param name="functionNames">List of supported function names</param>
        /// <exception cref="ArgumentException"></exception>
        public ArithmeticExpressionParser(
            Dictionary<string, ushort> operators = null, 
            string brackets = "()", 
            List<string> functionNames = null)
        {
            operators ??= new Dictionary<string, ushort>();
            functionNames ??= new List<string>();

            if (!operators.Keys.All(IsValidOperatorName))
            {
                throw new ArgumentException("Invalid character in the operator. " +
                                            "The operator must consist of +" + ValidOperatorSymbols + " characters.");
            }

            _operators = operators;

            if (!functionNames.All(IsValidFunctionName))
            {
                throw new ArgumentException("Invalid function name. ");
            }
            _functionNames = functionNames;

            if (brackets.Length % 2 > 0 || brackets.Length == 0)
            {
                throw new ArgumentException("Invalid brackets format. " +
                                            "The number of characters must be even and greater than 0.");
            }

            var sbOpenBrackets = new StringBuilder();
            var sbCloseBrackets = new StringBuilder();

            for (var i = 0; i < brackets.Length; i++)
            {
                if (i % 2 == 0)
                {
                    sbOpenBrackets.Append(brackets[i]);
                }
                else
                {
                    sbCloseBrackets.Append(brackets[i]);
                }
            }

            _bracketsOpen = sbOpenBrackets.ToString();
            _bracketsClose = sbCloseBrackets.ToString();
        }

        private static bool IsValidOperatorName(string name)
        {
            return name.All(ValidOperatorSymbols.Contains);
        }

        private static bool IsValidFunctionName(string name)
        {
            return name.All(char.IsLetterOrDigit) && char.IsLetter(name[0]);
        }
        
        private enum SymbolKind
        {
            Operator, Literal, VariableOrFunction, BracketOpen, BracketClose, Blank
        }
        
        private SymbolKind GetSymbolKind(char symbol)
        {
            if (ValidOperatorSymbols.Contains(symbol))
            {
                return SymbolKind.Operator;
            }

            if (_bracketsOpen.Contains(symbol))
            {
                return SymbolKind.BracketOpen;
            }
         
            if (_bracketsClose.Contains(symbol))
            {
                return SymbolKind.BracketClose;
            }
            
            if (char.IsDigit(symbol))
            {
                return SymbolKind.Literal;
            }

            if (char.IsLetter(symbol))
            {
                return SymbolKind.VariableOrFunction;
            }

            if (char.IsWhiteSpace(symbol))
            {
                return SymbolKind.Blank;
            }
            
            throw new ParserInvalidCharacter(symbol);
        }
        
        private static string ReadPredAndMovePosition(string text, Predicate<char> predicate, ref int position)
        {
            var endPosition = position;
            while (endPosition < text.Length && predicate.Invoke(text[endPosition]))
            {
                endPosition++;
            }

            var variable = text.Substring(position, endPosition - position);
            position = endPosition;
            return variable;
        }

        private static string ReadIdentAndMovePosition(string text, ref int position)
        {
            return ReadPredAndMovePosition(text, char.IsLetter, ref position);
        }
        
        private static string ReadNatAndMovePosition(string text, ref int position)
        {
            return ReadPredAndMovePosition(text, char.IsDigit, ref position);
        }
        
        private static string ReadOperatorAndMovePosition(string text, ref int position)
        {
            return ReadPredAndMovePosition(text, ValidOperatorSymbols.Contains, ref position);
        }
        
        private List<IExpression> ParseFunctionArguments(string text, ref int position)
        {
            while (position < text.Length && char.IsWhiteSpace(text[position]))
            {
                position++;
            }

            if (position == text.Length || !_bracketsOpen.Contains(text[position]))
            {
                throw new ParserInvalidFunctionCall();
            }

            var separators = new List<int>{position};
            var openBrackets = 1;
            for (position++; position < text.Length && openBrackets > 0; position++)
            {
                if (_bracketsOpen.Contains(text[position]))
                {
                    openBrackets++;
                } 
                else if (_bracketsClose.Contains(text[position]))
                {
                    openBrackets--;
                }
                else if (text[position] == FunctionArgumentsSeparator && openBrackets == 1)
                {
                    separators.Add(position);
                }
            }

            separators.Add(position - 1);
            if (!_bracketsClose.Contains(text[position - 1]) ||
                _bracketsClose.IndexOf(text[position - 1]) != _bracketsOpen.IndexOf(text[separators[0]]))
            {
                throw new ParserInvalidBrackets();
            }

            var arguments = new List<IExpression>();
            for (var i = 0; i < separators.Count - 1; i++)
            {
                var len = separators[i + 1] - separators[i] - 1;
                if (len > 0)
                {
                    arguments.Add(Parse(text.Substring(separators[i] + 1, len)));
                }
            }

            return arguments;
        }
        
        private static void ApplyBinaryOperatorToOutputStack(string operatorName, Stack<IExpression> stack)
        {
            if (stack.Count < 2)
            {
               throw new ParserInvalidExpression("Not enough arguments for a binary operation (" + 
                                                 operatorName + ").");
            }

            var rightOperand = stack.Pop();
            var leftOperand = stack.Pop();
            stack.Push(new BinaryExpression(operatorName, leftOperand, rightOperand));
        } 
        
        public IExpression Parse(string text)
        {
            // Bracket symbol, Operator name, Operator priority
            var operatorStack = new Stack<Tuple<char, string, int>>(); 
            var expressionStack = new Stack<IExpression>();
            
            for (var i = 0; i < text.Length;)
            {
                switch (GetSymbolKind(text[i]))
                {
                    case SymbolKind.Operator:
                        var operatorName = ReadOperatorAndMovePosition(text, ref i);

                        if (!_operators.ContainsKey(operatorName))
                        {
                            throw new ParserInvalidOperatorName(operatorName);
                        }
                        
                        var priority = _operators[operatorName];
                        while (operatorStack.Count > 0 && operatorStack.Peek().Item3 > priority)
                        {
                            ApplyBinaryOperatorToOutputStack(operatorStack.Pop().Item2, expressionStack);
                        }
                        operatorStack.Push(new Tuple<char, string, int>('\0', operatorName, priority));
                        break;
                    
                    case SymbolKind.Literal:
                        var literal = ReadNatAndMovePosition(text, ref i);
                        expressionStack.Push(new Literal(literal));
                        break;
                    
                    case SymbolKind.VariableOrFunction:
                        var name = ReadIdentAndMovePosition(text, ref i);
                        if (_functionNames.Contains(name))
                        {
                            expressionStack.Push(new FunctionCallExpression(name, ParseFunctionArguments(text, ref i)));
                        }
                        else
                        {
                            expressionStack.Push(new Variable(name));
                        }
                        break;
                    
                    case SymbolKind.BracketOpen:
                        operatorStack.Push(new Tuple<char, string, int>(text[i], null, -1));
                        i++;
                        break;
                    
                    case SymbolKind.BracketClose:
                        while (operatorStack.Count > 0 && operatorStack.Peek().Item3 != -1)
                        {
                            ApplyBinaryOperatorToOutputStack(operatorStack.Pop().Item2, expressionStack);
                        }

                        if (operatorStack.Count == 0 ||
                            _bracketsOpen.IndexOf(operatorStack.Pop().Item1) != _bracketsClose.IndexOf(text[i]))
                        {
                            throw new ParserInvalidBrackets();
                        }
                        i++;
                        break;
                    
                    case SymbolKind.Blank:
                        i++;
                        break;
                    
                    default:
                        throw new ParserFatalError();
                }
            }
            
            while (operatorStack.Count > 0)
            {
                var (_, name, priority) = operatorStack.Pop();
                if (priority == -1)
                {
                    throw new ParserInvalidBrackets();
                }
                else
                {
                    ApplyBinaryOperatorToOutputStack(name, expressionStack);
                }
            }
            
            if (expressionStack.Count != 1)
            {
                throw new ParserInvalidExpression("Unfinished expression");
            }

            return expressionStack.Pop();
        }
    }
}