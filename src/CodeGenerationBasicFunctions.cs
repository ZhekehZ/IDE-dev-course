using System;
using System.Reflection.Emit;

namespace ArithmeticExpressionParser
{
    public delegate void FunctionGenerator(Action a, ILGenerator g);

    public delegate void BiFunctionGenerator(Action a, Action b, ILGenerator g);

    public delegate void TriFunctionGenerator(Action a, Action b, Action c, ILGenerator g);
    
    static class BasicFunctions
    {
        
        public static readonly BiFunctionGenerator Plus = (l, r, gen) =>
        {
            l();
            r();
            gen.Emit(OpCodes.Add);
        };

        public static readonly BiFunctionGenerator Mul = (l, r, gen) =>
        {
            l(); 
            r();
            gen.Emit(OpCodes.Mul);
        };
        
        public static readonly FunctionGenerator Inc = (a, gen) =>
        {
            a();
            gen.Emit(OpCodes.Ldc_I4_1);
            gen.Emit(OpCodes.Add);
        };
        
        public static readonly TriFunctionGenerator IfThenElse = (a, b, c, gen) =>
        {
            var lnZ = gen.DefineLabel();
            var lFin = gen.DefineLabel();

            var t = gen.DeclareLocal(typeof(int));
            var e = gen.DeclareLocal(typeof(int));
            b();
            gen.Emit(OpCodes.Stloc, t);
            c();
            gen.Emit(OpCodes.Stloc, e);
            a();
            
            gen.Emit(OpCodes.Ldc_I4_0);
            gen.Emit(OpCodes.Beq_S, lnZ);
            gen.Emit(OpCodes.Ldloc, t);
            gen.Emit(OpCodes.Br_S, lFin);
            gen.MarkLabel(lnZ);
            gen.Emit(OpCodes.Ldloc, e);
            gen.MarkLabel(lFin);
        };
        
        public static readonly BiFunctionGenerator Gt = (a, b, gen) =>
        {
            var lGt = gen.DefineLabel();
            var lFin = gen.DefineLabel();
            b();
            a();
            gen.Emit(OpCodes.Blt_S, lGt);
            gen.Emit(OpCodes.Ldc_I4_0);
            gen.Emit(OpCodes.Br_S, lFin);
            gen.MarkLabel(lGt);
            gen.Emit(OpCodes.Ldc_I4_1);
            gen.MarkLabel(lFin);
        };

        public static readonly BiFunctionGenerator Lt = (a, b, gen) => Gt(b, a, gen);
        
        public static readonly FunctionGenerator Sqr = (a, gen) =>
        {
            a();
            gen.Emit(OpCodes.Dup);
            gen.Emit(OpCodes.Mul);
        };

        public static readonly BiFunctionGenerator Pow = (a, b, gen) => // TODO make it O(log n) 
        {
            var i = gen.DeclareLocal(typeof(int));
            var x = gen.DeclareLocal(typeof(int));
            var r = gen.DeclareLocal(typeof(int));

            a();
            gen.Emit(OpCodes.Stloc, x);
            
            b();
            gen.Emit(OpCodes.Stloc, i);
            
            gen.Emit(OpCodes.Ldc_I4_1);
            gen.Emit(OpCodes.Stloc, r);

            var lWhile = gen.DefineLabel();
            var lFin = gen.DefineLabel();
            gen.MarkLabel(lWhile);
            
            gen.Emit(OpCodes.Ldloc, i);
            gen.Emit(OpCodes.Ldc_I4_0);
            gen.Emit(OpCodes.Beq_S, lFin);
            
            gen.Emit(OpCodes.Ldloc, r);
            gen.Emit(OpCodes.Ldloc, x);
            gen.Emit(OpCodes.Mul);
            gen.Emit(OpCodes.Stloc, r);

            gen.Emit(OpCodes.Ldloc, i);
            gen.Emit(OpCodes.Ldc_I4_1);
            gen.Emit(OpCodes.Sub);
            gen.Emit(OpCodes.Stloc, i);

            gen.Emit(OpCodes.Br_S, lWhile);
            gen.MarkLabel(lFin);
            gen.Emit(OpCodes.Ldloc, r);
        };

    }
}