using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace Analyzers.Test
{
    [TestFixture]
    public class TestFixtureAnalyzerTests : DiagnosticVerifier
    {
        [Test]
        public void NoDiagnosticWhenEmptyTestCode()
        {
            var sourceCode = @"";

            VerifyCSharpDiagnostic(sourceCode);
        }

        [Test]
        public void RaisesInfoDiagnosticWhenClassWithTestFixtureAttribute()
        {
            var sourceCode = @"
    using NUnit.Framework;

    namespace ConsoleApplication1
    {
        [TestFixture]
        class TypeName
        {   
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "CreateSutSetup",
                Message = "Create SUT setup method",
                Severity = DiagnosticSeverity.Info,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 7, 15)
                        }
            };

            VerifyCSharpDiagnostic(sourceCode, expected);
        }

        [Test]
        public void DoesNotRaiseDiagnosticWhenTestFixtureContainsSetUpMethod()
        {
            var sourceCode = @"
    using NUnit.Framework;

    namespace ConsoleApplication1
    {
        [TestFixture]
        class TestFixture
        {   
            [SetUp]
            public void PerTestSetUp()
            {
            }
        }
    }";

            VerifyCSharpDiagnostic(sourceCode);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new TestFixtureAnalyzer();
        }
    }
}