using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace ArithmeticExpressionParser
{
    
    public class CodeGenerationVisitor : IExpressionVisitor
    {
        private MethodBuilder _methodBuilder;
        
        private readonly Dictionary<string, FunctionGenerator> _functions;
        private readonly Dictionary<string, BiFunctionGenerator> _biFunctions;
        private readonly Dictionary<string, TriFunctionGenerator> _triFunctions;
        private readonly Dictionary<string, BiFunctionGenerator> _operators;
        private readonly List<string> _arguments = new();

        public CodeGenerationVisitor(
            Dictionary<string, FunctionGenerator> functions = null, 
            Dictionary<string, BiFunctionGenerator> biFunctions = null, 
            Dictionary<string, TriFunctionGenerator> triFunctions = null, 
            Dictionary<string, BiFunctionGenerator> operators = null)
        {
            _functions = functions;
            _biFunctions = biFunctions;
            _triFunctions = triFunctions;
            _operators = operators;
        }

        public Type Assembly(IExpression expression, string name, string className, string methodName)
        {
            _arguments.Clear();
            var aName = new AssemblyName(name);
            var ab = AssemblyBuilder.DefineDynamicAssembly(aName, AssemblyBuilderAccess.RunAndCollect);
            var mb = ab.DefineDynamicModule(aName.Name!);
            var tb = mb.DefineType(className, TypeAttributes.Public);
            _methodBuilder = tb.DefineMethod(methodName, MethodAttributes.Public | MethodAttributes.Static);
            
            expression.Accept(this);
            _methodBuilder.GetILGenerator().Emit(OpCodes.Ret);
            
            var arguments = new Type[_arguments.Count];
            for (var i = 0; i < arguments.Length; i++)
            {
                arguments[i] = typeof(int);
            }

            _methodBuilder.SetReturnType(typeof(int));
            _methodBuilder.SetParameters(arguments);

            return tb.CreateType();
        }
        
        public void Visit(Literal expression)
        {
            var gen = _methodBuilder.GetILGenerator();
            gen.Emit(OpCodes.Ldc_I4, int.Parse(expression.Value));
        }

        public void Visit(Variable expression)
        {
            var gen = _methodBuilder.GetILGenerator();
            if (!_arguments.Contains(expression.Value))
            {
                _arguments.Add(expression.Value);    
            }
            gen.Emit(OpCodes.Ldarg, _arguments.IndexOf(expression.Value));
        }

        public void Visit(BinaryExpression expression)
        {
            _operators[expression.Operator](
                () => expression.LeftOperand.Accept(this), 
                () => expression.RightOperand.Accept(this), 
                _methodBuilder.GetILGenerator());
        }

        public void Visit(FunctionCallExpression expression)
        {
            switch (expression.Arguments.Count)
            {
                case 1:
                    var fn = _functions.First(f => f.Key == expression.FunctionName).Value;
                    fn(() => expression.Arguments[0].Accept(this), _methodBuilder.GetILGenerator());
                    break;
                case 2:
                    var biFn = _biFunctions.First(f => f.Key == expression.FunctionName).Value;
                    biFn(() => expression.Arguments[0].Accept(this),
                         () => expression.Arguments[1].Accept(this),
                         _methodBuilder.GetILGenerator());
                    break;   
                case 3:
                    var triFn = _triFunctions.First(f => f.Key == expression.FunctionName).Value;
                    triFn(() => expression.Arguments[0].Accept(this),
                          () => expression.Arguments[1].Accept(this),
                          () => expression.Arguments[2].Accept(this),
                          _methodBuilder.GetILGenerator());
                    break;
                default:
                    throw new CogenInvalidFunctionArity(expression.Arguments.Count);
            }
        }
    }
}