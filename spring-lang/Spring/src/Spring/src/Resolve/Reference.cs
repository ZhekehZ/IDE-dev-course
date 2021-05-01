using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.DataFlow;
using JetBrains.DataStructures;
using JetBrains.Lifetimes;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Resolve;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Tree;


namespace JetBrains.ReSharper.Plugins.Spring.Resolve
{
    public class PascalVariableReference : TreeReferenceBase<ITreeNode>
    {
        private readonly ITreeNode _node;

        public PascalVariableReference(ITreeNode node) : base(node)
        {
            _node = node;
        }

        public override ResolveResultWithInfo ResolveWithoutCache()
        {
            var file = _node.GetContainingFile();
            if (file == null) return ResolveResultWithInfo.Unresolved;

            foreach (var decl in GetAllDeclarations(_node))
            {

                if (decl.DeclaredName == GetName())
                {
                    return new ResolveResultWithInfo(new SimpleResolveResult(decl.DeclaredElement),
                        ResolveErrorType.OK);
                }
            }

            return ResolveResultWithInfo.Unresolved;
        }

        public override string GetName() => _node.GetText();
        public override ISymbolTable GetReferenceSymbolTable(bool useReferenceName) => throw new NotImplementedException();
        public override TreeTextRange GetTreeTextRange() => _node.GetTreeTextRange();
        public override IReference BindTo(IDeclaredElement element) => this;
        public override IReference BindTo(IDeclaredElement element, ISubstitution substitution) => this;
        public override IAccessContext GetAccessContext() => new DefaultAccessContext(_node);
        public override bool IsValid() => myOwner.IsValid();
        
        
        private static IEnumerable<IDeclaration> GetAllDeclarations(ITreeNode curr)
        {
            while (curr != null && curr.NodeType != SpringCompositeNodeType.PROCEDURE_DECLARATION)  curr = curr.Parent;
            if (curr == null) return ImmutableArray<IDeclaration>.Empty;
            var declarations = new HashSet<IDeclaration>();
            foreach (var node in curr.FirstChild.Children()) CollectDFS(node, declarations);
            return declarations;
        }
        
        private static void CollectDFS(ITreeNode curr, HashSet<IDeclaration> declarations)
        {
            if (curr is IDeclaration)
            {
                declarations.Add(curr as IDeclaration);
                return;
            }
            
            foreach (var ch in curr.Children()) CollectDFS(ch, declarations);
        }
    }

}