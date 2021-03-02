using System;
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

        public VarargFunc<int, int> Compile(string s)
        {
            var type = _cogen.Assembly(_parser.Parse(s), "DAsm", "Evaluator", "eval");
            var method = type.GetMethod("eval")!;
            return args =>
            {
                var objects = args.Select(x => (object)x).ToArray();
                if (method is not null) return (int) method.Invoke(type, objects)!;
                throw new Exception();
            };
        }    
    }
}