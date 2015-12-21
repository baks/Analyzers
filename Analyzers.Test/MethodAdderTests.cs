using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
            sut = new MethodAdder(SyntaxTokenList.Create(SyntaxFactory.Token(SyntaxKind.PublicKeyword)), "void",
                "TestMethod");
        }

        [Test]
        public void AddsMethodToClass()
        {
            var source = @"
using System;

public class TestClass
{
}";

            var expected = @"
using System;

public class TestClass
{
public void TestMethod(){}}";

            Assert.That(WithAddedMethod(source), Is.EqualTo(expected));
        }

        private string WithAddedMethod(string source)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default);
            var compilationUnit = syntaxTree.GetCompilationUnitRoot();
            var newCompilationUnit = compilationUnit.Accept(sut);

            return newCompilationUnit.GetText().ToString();
        }
    }
}