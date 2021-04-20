using System;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;

namespace JetBrains.ReSharper.Plugins.Spring
{
    internal class SpringFileNodeType : CompositeNodeType
    {
        public SpringFileNodeType(string s, int index) : base(s, index)
        {
        }

        public static readonly SpringFileNodeType Instance = new SpringFileNodeType("Spring_FILE", 0);

        public override CompositeElement Create()
        {
            return new SpringFile();
        }
    }
    internal class SpringCompositeNodeType : CompositeNodeType
    {
        public SpringCompositeNodeType(string s, int index) : base(s, index)
        {
        }
        public static readonly SpringCompositeNodeType BLOCK = new SpringCompositeNodeType("Spring_BLOCK", 0);
        public static readonly SpringCompositeNodeType OTHER = new SpringCompositeNodeType("Spring_OTHER", 1);

        public override CompositeElement Create()
        {
            if (this == BLOCK)
                return new SpringBlock();
            else 
                throw new InvalidOperationException();
        }
    }

}