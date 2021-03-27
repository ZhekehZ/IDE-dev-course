using System.Collections.Generic;
using System.Linq;

namespace ArithmeticExpressionParser
{
    public class ArithmeticExpressionCompiler : ICompiler<int, int>
    {
        private readonly ArithmeticExpressionParser _parser;
        private readonly CodeGenerationVisitor _cogen;
        
        public ArithmeticExpressionCompiler(ArithmeticExpressionParser parser, CodeGenerationVisitor cogen)
        {
            _parser = parser;
            _cogen = cogen;
        }

        public (VarargFunc<int, int>, string[]) Compile(IExpression e)
        {
            var (type, argNames) = _cogen.Assembly(e, "DAsm", "Evaluator", "eval");

            var sl = new SortedList<string, int>();
            for (var i = 0; i < argNames.Length; i++)
            {
                sl.Add(argNames[i], i);
            }

            var mapping = sl.Values.ToList();
            var invMapping = mapping.Select((_, i) => mapping.IndexOf(i)).ToArray();
            
            var method = type.GetMethod("eval")!;
            if (method == null)
            {
                throw new CogenAssemblyException();
            }
            
            var compiled = new VarargFunc<int, int>(args =>
            {
                var callArgs = invMapping.Select(idx => (object)args[idx]).ToArray();
                return (int) method.Invoke(type, callArgs)!;
            });
            return (compiled, sl.Keys.ToArray());
        }

        public (VarargFunc<int, int>, string[]) Compile(string s)
        {
            return Compile(_parser.Parse(s));
        }

    }
}