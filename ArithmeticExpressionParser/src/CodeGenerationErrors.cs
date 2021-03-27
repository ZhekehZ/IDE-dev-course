using System;

namespace ArithmeticExpressionParser
{
    class CogenInvalidFunctionArity : Exception
    {
        public readonly int Arity;
        public CogenInvalidFunctionArity(int arity) : base("Invalid arity (" + arity + ")")
        {
            Arity = arity;
        }
    }

    class CogenAssemblyException : Exception
    {
    }
}