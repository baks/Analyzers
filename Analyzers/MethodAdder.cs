using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace Analyzers
{
    public sealed class MethodAdder : CSharpSyntaxRewriter
    {
        private readonly SyntaxTokenList accessibilityModifiers;
        private readonly string returnType;
        private readonly string name;

        public MethodAdder(SyntaxTokenList accessibilityModifiers, string returnType, string name)
        {
            if (accessibilityModifiers == null)
            {
                throw new ArgumentNullException(nameof(accessibilityModifiers));
            }
            if (returnType == null)
            {
                throw new ArgumentNullException(nameof(returnType));
            }
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            this.accessibilityModifiers = accessibilityModifiers;
            this.returnType = returnType;
            this.name = name;
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var newMethod = SyntaxFactory.MethodDeclaration(
                WithoutAttributes(),
                accessibilityModifiers,
                MethodReturnType(returnType),
                WithoutExplicitInterface(),
                MethodName(name),
                WithoutGenericTypeParameters(),
                WithoutParameters(),
                WithoutTypeParametersConstraint(),
                EmptyBlock(),
                NoneToken());

            return base.VisitClassDeclaration(node.AddMembers(newMethod));
        }

        private static BlockSyntax EmptyBlock()
        {
            return SyntaxFactory.Block();
        }

        private static SyntaxToken NoneToken()
        {
            return SyntaxFactory.Token(SyntaxKind.None);
        }

        private static SyntaxList<TypeParameterConstraintClauseSyntax> WithoutTypeParametersConstraint()
        {
            return default(SyntaxList<TypeParameterConstraintClauseSyntax>);
        }

        private static ParameterListSyntax WithoutParameters()
        {
            return SyntaxFactory.ParameterList();
        }

        private static TypeParameterListSyntax WithoutGenericTypeParameters()
        {
            return null;
        }

        private static SyntaxToken MethodName(string name)
        {
            return SyntaxFactory.Identifier(name).WithLeadingTrivia(SyntaxFactory.Space);
        }

        private static ExplicitInterfaceSpecifierSyntax WithoutExplicitInterface()
        {
            return default(ExplicitInterfaceSpecifierSyntax);
        }

        private static IdentifierNameSyntax MethodReturnType(string returnType)
        {
            return SyntaxFactory.IdentifierName(returnType).WithLeadingTrivia(SyntaxFactory.Space);
        }

        private static SyntaxList<AttributeListSyntax> WithoutAttributes()
        {
            return SyntaxFactory.List<AttributeListSyntax>();
        }
    }
}
