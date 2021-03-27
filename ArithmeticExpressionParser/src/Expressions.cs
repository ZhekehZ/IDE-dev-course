using System.Collections.Generic;

namespace ArithmeticExpressionParser
{
    public class Literal : IExpression
    {
        public readonly string Value;

        public Literal(string value)
        {
            Value = value;
        }

        public void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class Variable : IExpression
    {
        public readonly string Value;

        public Variable(string value)
        {
            Value = value;
        }

        public void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class BinaryExpression : IExpression
    {
        public readonly IExpression LeftOperand;
        public readonly IExpression RightOperand;
        public readonly string Operator;

        public BinaryExpression(string @operator, IExpression leftOperand, IExpression rightOperand)
        {
            LeftOperand = leftOperand;
            RightOperand = rightOperand;
            Operator = @operator;
        }

        public void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class FunctionCallExpression : IExpression
    {
        public readonly string FunctionName;
        public readonly List<IExpression> Arguments;

        public FunctionCallExpression(string functionName, List<IExpression> arguments)
        {
            FunctionName = functionName;
            Arguments = arguments;
        }

        public void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}