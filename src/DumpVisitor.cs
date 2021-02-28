using System.Text;

namespace ArithmeticExpressionParser
{
    public class DumpVisitor : IExpressionVisitor
    {
        private readonly StringBuilder _textDump;

        /// <summary>
        /// A Visitor which converts an expression to its string representation
        /// </summary>
        public DumpVisitor()
        {
            _textDump = new StringBuilder();
        }

        public void Visit(Literal expression)
        {
            _textDump.Append("Lit(").Append(expression.Value).Append(')');
        }

        public void Visit(Variable expression)
        {
            _textDump.Append("Var(").Append(expression.Value).Append(')');
        }

        public void Visit(BinaryExpression expression)
        {
            _textDump.Append("Bin(");
            expression.LeftOperand.Accept(this);
            _textDump.Append(' ').Append(expression.Operator).Append(' ');
            expression.RightOperand.Accept(this);
            _textDump.Append(')');
        }

        public void Visit(FunctionCallExpression expression)
        {
            _textDump.Append("Fun<").Append(expression.FunctionName).Append(">(");
            if (expression.Arguments.Count > 0)
            {
                expression.Arguments[0].Accept(this);
                for (var i = 1; i < expression.Arguments.Count; i++)
                {
                    _textDump.Append(", ");
                    expression.Arguments[i].Accept(this);
                }
            }
            _textDump.Append(')');
        }

        public override string ToString()
        {
            var result = _textDump.ToString();
            _textDump.Clear();
            return result;
        }
    }
}