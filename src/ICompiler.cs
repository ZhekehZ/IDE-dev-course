using System;

namespace ArithmeticExpressionParser
{
    
    public delegate TOut VarargFunc<in TIn, out TOut>(params TIn[] args);

    public interface ICompiler<in T, out TR>
    {
        public VarargFunc<T, TR> Compile(string text);
    }
}