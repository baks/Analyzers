using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Analyzers
{
    public class SutSetupBodyCreator : CSharpSyntaxWalker
    {
        private const string NSubstituteType = "Substitute";
        private const string SubstituteCreationMethod = "For";

        private readonly string fieldName;
        private readonly SemanticModel semanticModel;

        public SutSetupBodyCreator(Document document, string fieldName)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }
            if (fieldName == null)
            {
                throw new ArgumentNullException(nameof(fieldName));
            }
            semanticModel = document.GetSemanticModelAsync().Result;
            this.fieldName = fieldName;
        }

        public BlockSyntax Result { get; private set; }

        public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            if (FoundVariableNamed(node, fieldName))
            {
                var fieldType = TypeSymbolFor(node.Declaration.Type);
                var constructor = GetConstructorFor(fieldType);

                var objectCreation = CreateSutInstanceCreationExpr(fieldType, constructor);

                var collaborators = CreateSubstitutesForCollaborators(constructor.Parameters);

                var sutAssignment = Assignment(Identifier(fieldName), objectCreation);

                Result =
                    SyntaxFactory.Block(
                        collaborators.Select(SyntaxFactory.ExpressionStatement)
                            .Union(new[] {SyntaxFactory.ExpressionStatement(sutAssignment)}));
            }
            base.VisitFieldDeclaration(node);
        }

        private INamedTypeSymbol TypeSymbolFor(TypeSyntax type)
        {
            var info = semanticModel.GetSymbolInfo(type);
            return info.Symbol as INamedTypeSymbol;
        }

        private static IMethodSymbol GetConstructorFor(INamedTypeSymbol typeSymbol)
            => typeSymbol.Constructors.SingleOrDefault(m => !m.IsStatic);

        private static bool FoundVariableNamed(FieldDeclarationSyntax node, string fieldName)
            => node.Declaration.Variables.Any(
                vd => string.Equals(vd.Identifier.Text, fieldName, StringComparison.OrdinalIgnoreCase));

        private static ObjectCreationExpressionSyntax CreateSutInstanceCreationExpr(INamedTypeSymbol typeSymbol,
            IMethodSymbol constructor)
        {
            return SyntaxFactory.ObjectCreationExpression(
                SyntaxFactory.IdentifierName(typeSymbol.Name),
                ArgumentList(constructor.Parameters),
                null
                );
        }

        private static ArgumentListSyntax ArgumentList(ImmutableArray<IParameterSymbol> parameters)
        {
            return parameters.Any()
                ? SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(parameters.Select(Argument)))
                : SyntaxFactory.ArgumentList();
        }

        private static ArgumentSyntax Argument(IParameterSymbol p) => SyntaxFactory.Argument(Identifier(p.Name));

        private static IEnumerable<AssignmentExpressionSyntax> CreateSubstitutesForCollaborators(
            ImmutableArray<IParameterSymbol> parameters)
            => parameters.Select(p => Assignment(Identifier(p.Name), SubstituteCreation(p)));

        private static AssignmentExpressionSyntax Assignment(ExpressionSyntax left, ExpressionSyntax right)
        {
            return SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                left,
                right);
        }

        private static InvocationExpressionSyntax SubstituteCreation(IParameterSymbol p)
        {
            return SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    Identifier(NSubstituteType), ExpressionWithGenericTypes(p)));
        }

        private static GenericNameSyntax ExpressionWithGenericTypes(IParameterSymbol p)
        {
            return SyntaxFactory.GenericName(Identifier(SubstituteCreationMethod).Identifier, GenericTypeArgumentList(p));
        }

        private static TypeArgumentListSyntax GenericTypeArgumentList(IParameterSymbol p)
        {
            return SyntaxFactory.TypeArgumentList(LessThanToken(),
                SyntaxFactory.SeparatedList<TypeSyntax>(new[] {Identifier(p.Type.Name)}),
                GreaterThanToken());
        }

        private static IdentifierNameSyntax Identifier(string name) => SyntaxFactory.IdentifierName(name);

        private static SyntaxToken GreaterThanToken() => SyntaxFactory.Token(SyntaxKind.GreaterThanToken);

        private static SyntaxToken LessThanToken() => SyntaxFactory.Token(SyntaxKind.LessThanToken);
    }
}