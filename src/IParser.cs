namespace ArithmeticExpressionParser
{
    public interface IParser<out T>
    {
        T Parse(string text);
    }
}