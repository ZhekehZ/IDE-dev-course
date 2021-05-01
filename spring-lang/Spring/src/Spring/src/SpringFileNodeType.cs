using System;
using JetBrains.ReSharper.Plugins.Spring.Resolve;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;

namespace JetBrains.ReSharper.Plugins.Spring
{
    internal class SpringFileNodeType : CompositeNodeType
    {
        public SpringFileNodeType(string s, int index) : base(s, index)
        {
        }

        public static readonly SpringFileNodeType Instance = new("Spring_FILE", 0);

        public override CompositeElement Create() => new SpringFile();
    }

    internal class SpringCompositeNodeType : CompositeNodeType
    {
        public SpringCompositeNodeType(string s, int index) : base(s, index)
        {
        }

        public static readonly SpringCompositeNodeType BLOCK = new("Spring_BLOCK", 0);
        public static readonly SpringCompositeNodeType TYPE_VARIABLE = new("Spring_BLOCK", 1);
        public static readonly SpringCompositeNodeType PROCEDURE_ARGUMENT_DECLARATION = new("Spring_PROC", 2);
        public static readonly SpringCompositeNodeType VARIABLE_DECLARATION = new("Spring_VAR", 3);
        public static readonly SpringCompositeNodeType REFERENCE = new("Spring_REFERENCE", 4);
        public static readonly SpringCompositeNodeType PROCEDURE_DECLARATION = new("Spring_PROCEDURE", 5);
        public static readonly SpringCompositeNodeType OTHER = new("Spring_OTHER", 6);

        public override CompositeElement Create()
        {
            if (this == BLOCK) return new SpringBlock();
            if (this == TYPE_VARIABLE) return new SpringType();
            if (this == VARIABLE_DECLARATION) return new LocalVariableDeclaration();
            if (this == PROCEDURE_ARGUMENT_DECLARATION) return new ProcedureArgumentDeclaration();
            if (this == REFERENCE) return new SpringReference();
            if (this == PROCEDURE_DECLARATION) return new SpringProcedure();
            throw new InvalidOperationException();
        }
    }
}