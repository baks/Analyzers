using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Analyzers
{
    public sealed class MethodAdder : CSharpSyntaxRewriter
    {
        private readonly SyntaxTokenList modifiers;
        private readonly string returnType;
        private readonly string name;

        public MethodAdder(SyntaxTokenList modifiers, string returnType, string name)
        {
            if (modifiers == null)
            {
                throw new ArgumentNullException(nameof(modifiers));
            }
            if (returnType == null)
            {
                throw new ArgumentNullException(nameof(returnType));
            }
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            this.modifiers = modifiers;
            this.returnType = returnType;
            this.name = name;
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var newMethod = SyntaxFactory.MethodDeclaration(
                SyntaxFactory.List<AttributeListSyntax>(),
                modifiers,
                SyntaxFactory.IdentifierName(returnType).WithLeadingTrivia(SyntaxFactory.Space),
                default(ExplicitInterfaceSpecifierSyntax),
                SyntaxFactory.Identifier(name).WithLeadingTrivia(SyntaxFactory.Space),
                null,
                SyntaxFactory.ParameterList(),
                default(SyntaxList<TypeParameterConstraintClauseSyntax>),
                SyntaxFactory.Block(),
                SyntaxFactory.Token(SyntaxKind.SemicolonToken)
                ).WithLeadingTrivia(Enumerable.Repeat(SyntaxFactory.Space, 4));

            return base.VisitClassDeclaration(node.AddMembers(newMethod));
        }
    }
}
