using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Analyzers
{
    public class UsingsAdder : CSharpSyntaxRewriter
    {
        private readonly string usingDirective;

        public UsingsAdder(string usingDirective)
        {
            if (usingDirective == null)
            {
                throw new ArgumentNullException(nameof(usingDirective));
            }
            this.usingDirective = usingDirective;
        }

        public override SyntaxNode VisitCompilationUnit(CompilationUnitSyntax node)
        {
            var directive = SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(usingDirective));
            return base.VisitCompilationUnit(node.Usings.Any(u => AreEqual(u, directive)) ? node : node.AddUsings(directive));
        }

        private bool AreEqual(UsingDirectiveSyntax a, UsingDirectiveSyntax b)
        {
            return a.Name.ToString() == b.Name.ToString();
        }
    }
}
