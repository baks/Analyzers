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
        private readonly SyntaxTokenList modifiers;
        private readonly string returnType;
        private readonly string name;
        private readonly Workspace workspace;

        public MethodAdder(SyntaxTokenList modifiers, string returnType, string name, Workspace workspace)
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
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }
            this.modifiers = modifiers;
            this.returnType = returnType;
            this.name = name;
            this.workspace = workspace;
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var newMethod = SyntaxFactory.MethodDeclaration(
                WithoutAttributes(),
                modifiers,
                MethodReturnType(returnType),
                WithoutExplicitInterface(),
                MethodName(name),
                WithoutGenericTypeParameters(),
                WithoutParameters(),
                WithoutTypeParametersConstraint(),
                SyntaxFactory.Block(),
                NoneToken());

            return
                base.VisitClassDeclaration(
                    node.AddMembers(
                        MethodWithLeadingAndTrailingTrivia(
                            (MethodDeclarationSyntax) Formatter.Format(newMethod, workspace))));
        }

        private static MethodDeclarationSyntax MethodWithLeadingAndTrailingTrivia(MethodDeclarationSyntax method)
        {
            return method.WithLeadingTrivia(Enumerable.Repeat(SyntaxFactory.Space, 4))
                .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);
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
