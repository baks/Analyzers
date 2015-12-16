using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;

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

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: ProviderTitle,
                    createChangedDocument: c => CreateSutSetupMethodAsync(context.Document, declaration, c),
                    equivalenceKey: ProviderTitle),
                diagnostic);
        }

        private async Task<Document> CreateSutSetupMethodAsync(Document document, TypeDeclarationSyntax typeDecl, CancellationToken cancellationToken)
        {
            //var newTypeDeclaration = typeDecl.Accept(new MethodRewriter());
            var syntaxRoot = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = syntaxRoot.ReplaceNode(typeDecl, typeDecl);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}