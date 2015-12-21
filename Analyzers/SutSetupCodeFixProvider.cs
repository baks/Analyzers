using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SutSetupCodeFixProvider)), Shared]
    public sealed class SutSetupCodeFixProvider : CodeFixProvider
    {
        private const string ProviderTitle = "Create system under test setup method";

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(TestFixtureAnalyzer.DiagnosticId);

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().First();
            var sutField = declaration.Ancestors().OfType<FieldDeclarationSyntax>().SingleOrDefault(FieldNamedSut);
            if (sutField != null)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: ProviderTitle,
                        createChangedDocument: c => CreateSutSetupMethodAsync(context.Document, declaration, sutField, c),
                        equivalenceKey: ProviderTitle),
                    diagnostic);
            }
        }

        private async Task<Document> CreateSutSetupMethodAsync(Document document, TypeDeclarationSyntax typeDecl, FieldDeclarationSyntax sutField, CancellationToken cancellationToken)
        {
            //var newTypeDeclaration = typeDecl.Accept(new MethodRewriter());
            //var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            //var sutFieldSymbol = semanticModel.GetDeclaredSymbol(sutField, cancellationToken) as IFieldSymbol;
            //sutFieldSymbol.Type.GetMembers().OfType<IMethodSymbol>().Any(m => m.)
            var syntaxRoot = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = syntaxRoot.ReplaceNode(typeDecl, typeDecl);

            return document.WithSyntaxRoot(newRoot);
        }

        private static bool FieldNamedSut(FieldDeclarationSyntax fieldDeclaration)
            =>
                fieldDeclaration.Declaration.Variables.Any(
                    vd => string.Equals("sut", vd.Identifier.ValueText, StringComparison.OrdinalIgnoreCase));
    }
}