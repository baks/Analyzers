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
        public void RaisesInfoDiagnosticWhenClassWithTestFixtureAttributeAndSutField()
        {
            var sourceCode = @"
    using NUnit.Framework;

    namespace ConsoleApplication1
    {
        class SutClass {}

        [TestFixture]
        class TypeName
        {
            private SutClass sut; 
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "CreateSutSetup",
                Message = "Create SUT setup method for SutClass",
                Severity = DiagnosticSeverity.Info,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 9, 15)
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