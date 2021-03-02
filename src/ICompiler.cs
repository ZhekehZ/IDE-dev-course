using System;

namespace ArithmeticExpressionParser
{
    
    public delegate TOut VarargFunc<in TIn, out TOut>(params TIn[] args);

    public interface ICompiler<T, TR>
    {
        public (VarargFunc<T, TR>, string[]) Compile(string text);
    }
}