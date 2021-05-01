using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Tree;

namespace JetBrains.ReSharper.Plugins.Spring.Resolve
{
    public class ProcedureArgumentDeclaration : DeclarationBase
    {
        public ProcedureArgumentDeclaration()
        {
            DeclaredElement = new ProcedureArgumentDeclared(this);
        }

        public override NodeType NodeType => SpringCompositeNodeType.PROCEDURE_ARGUMENT_DECLARATION;
        public override IDeclaredElement DeclaredElement { get; }
    }

    public class ProcedureArgumentDeclared : SpringDeclared
    {
        public ProcedureArgumentDeclared(IDeclaration declaration) : base(declaration)
        {
        }
    }

    public class LocalVariableDeclaration : DeclarationBase
    {
        public LocalVariableDeclaration()
        {
            DeclaredElement = new LocalVariableDeclared(this);
        }

        public override NodeType NodeType => SpringCompositeNodeType.VARIABLE_DECLARATION;
        public override IDeclaredElement DeclaredElement { get; }
    }

    public class LocalVariableDeclared : SpringDeclared
    {
        public LocalVariableDeclared(IDeclaration declaration) : base(declaration)
        {
        }
    }
}