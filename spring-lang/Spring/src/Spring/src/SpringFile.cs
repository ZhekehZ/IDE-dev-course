using JetBrains.ReSharper.Plugins.Spring.Resolve;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Tree;

namespace JetBrains.ReSharper.Plugins.Spring
{
    public class SpringFile : FileElementBase
    {
        public override NodeType NodeType => SpringFileNodeType.Instance;

        public override PsiLanguageType Language => SpringLanguage.Instance;
    }

    public class SpringBlock : CompositeElement
    {
        public override NodeType NodeType => SpringCompositeNodeType.BLOCK;

        public override PsiLanguageType Language => SpringLanguage.Instance;
    }

    public class SpringType : CompositeElement
    {
        public override NodeType NodeType => SpringCompositeNodeType.TYPE_VARIABLE;

        public override PsiLanguageType Language => SpringLanguage.Instance;
    }

    public class SpringReference : CompositeElement
    {
        public override NodeType NodeType => SpringCompositeNodeType.REFERENCE;

        public override PsiLanguageType Language => SpringLanguage.Instance;

        public override ReferenceCollection GetFirstClassReferences() =>
            ReferenceCollection.Empty.Append(new PascalVariableReference(this));
    }

    public class SpringProcedure : CompositeElement
    {
        public override NodeType NodeType => SpringCompositeNodeType.PROCEDURE_DECLARATION;

        public override PsiLanguageType Language => SpringLanguage.Instance;
    }
}