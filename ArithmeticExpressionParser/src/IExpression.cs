namespace ArithmeticExpressionParser
{
    public interface IExpression
    {
        void Accept(IExpressionVisitor visitor);
    }
    
    public interface IExpressionVisitor
    {
        void Visit(Literal expression);
        void Visit(Variable expression);
        void Visit(BinaryExpression expression);
        void Visit(FunctionCallExpression expression);
    }
}