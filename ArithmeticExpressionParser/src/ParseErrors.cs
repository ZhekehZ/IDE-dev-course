using System;

namespace ArithmeticExpressionParser
{
    class ParserInvalidCharacter : Exception
    {
        public readonly char Character;
        public ParserInvalidCharacter(char character) : base("Invalid character (" + character + ")")
        {
            Character = character;
        }
    }
    
    class ParserInvalidOperatorName : Exception
    {
        public readonly string OperatorName;
        public ParserInvalidOperatorName(string operatorName) 
            : base("Invalid operator name (" + operatorName + ")")
        {
            OperatorName = operatorName;
        }
    }
    
    class ParserInvalidExpression : Exception
    {
        public ParserInvalidExpression(string message) : base(message)
        {
        }
    }
    
    class ParserInvalidBrackets : Exception
    {
    }

    class ParserInvalidFunctionCall : Exception
    {
    }
    
    class ParserFatalError : Exception
    {
    }
}