using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.Formatting;
using NUnit.Framework;

namespace Analyzers.Test
{
    [TestFixture]
    public class MethodAdderTests
    {
        private MethodAdder sut;

        [SetUp]
        public void SutSetup()
        {
            var workspace = new AdhocWorkspace();
            var options = workspace.Options;
            options = options.WithChangedOption(CSharpFormattingOptions.IndentBraces, true);
            options = options.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInMethods, true);
            options = options.WithChangedOption(CSharpFormattingOptions.IndentBlock, true);
            options = options.WithChangedOption(CSharpFormattingOptions.IndentBlock, true);
            options = options.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInTypes, true);
            workspace.Options = options;
            sut = new MethodAdder(SyntaxTokenList.Create(SyntaxFactory.Token(SyntaxKind.PublicKeyword)), "void",
                "TestMethod", workspace);
        }

        [Test]
        public void AddsMethodToClass()
        {
            var source = @"
using System;

public class TestClass
{
}";

            var syntaxTree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default);
            var compilationUnit = syntaxTree.GetCompilationUnitRoot();
            var root = syntaxTree.GetRoot();

            var newCompilationUnit = compilationUnit.Accept(sut);

            var changedSource = newCompilationUnit.GetText().ToString();

            var expected = @"
using System;

public class TestClass
{
    public void TestMethod()
    {
    }
}";
            Assert.That(changedSource, Is.EqualTo(expected));
        }
    }
}
