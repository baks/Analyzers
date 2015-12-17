using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class TestFixtureAnalyzer : DiagnosticAnalyzer
    {
        public static readonly string DiagnosticId = "CreateSutSetup";

        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof (Resources));

        private static readonly LocalizableString MessageFormat =
            new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager,
                typeof (Resources));

        private static readonly LocalizableString Description =
            new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager,
                typeof (Resources));

        private const string Category = "Refactoring";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category,
            DiagnosticSeverity.Info, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;
            if (namedTypeSymbol.TypeKind != TypeKind.Class)
            {
                return;
            }

            if (HasTestFixtureAttribute(namedTypeSymbol))
            {
                var evaluator = new OrthogonalEvaluator(DoesNotHavePerTestSetUpAttribute, IsSutField);

                if (namedTypeSymbol.GetMembers().Any(evaluator.Evaluate))
                {
                    var diagnostic = Diagnostic.Create(Rule, namedTypeSymbol.Locations[0],
                        ((IFieldSymbol) evaluator.SecondConditionMatch).Type.Name);

                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private static bool IsSutField(ISymbol symbol)
            => string.Equals("sut", symbol.Name, StringComparison.Ordinal) && FieldTypeIsClass((IFieldSymbol) symbol);

        private static bool FieldTypeIsClass(IFieldSymbol fieldSymbol) => fieldSymbol?.Type.TypeKind == TypeKind.Class;

        private static bool HasTestFixtureAttribute(INamedTypeSymbol symbol)
            => symbol.GetAttributes().Any(IsAttributeTestFixtureAttribute);

        private static bool DoesNotHavePerTestSetUpAttribute(ISymbol symbol) => !symbol.GetAttributes().Any(IsAttributeSetUpAttribute);

        private static bool IsAttributeTestFixtureAttribute(AttributeData attrData)
            => string.Equals(attrData.AttributeClass.Name, "TestFixtureAttribute", StringComparison.Ordinal);

        private static bool IsAttributeSetUpAttribute(AttributeData attrData)
            => string.Equals(attrData.AttributeClass.Name, "SetUpAttribute", StringComparison.Ordinal);
    }
}
