using System;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Analyzers.Test
{
    public class UsingsAdderTests
    {
        private UsingsAdder sut;

        [Test]
        public void AddsUsingIfDoesNotExist()
        {
            var source = @"public class TestClass{}";

            var expected = @"usingUsingName;public class TestClass{}";
            Assert.That(WithAddedUsings(source, "UsingName"), Is.EqualTo(expected));
        }

        [Test]
        public void DoesNotAddUsingIfItIsThere()
        {
            var source = @"using UsingName;public class TestClass{}";

            var expected = @"using UsingName;public class TestClass{}";
            Assert.That(WithAddedUsings(source, "UsingName"), Is.EqualTo(expected));
        }

        private string WithAddedUsings(string source, string directive)
        {
            sut = new UsingsAdder(directive);
            var syntaxTree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default);
            var compilationUnit = syntaxTree.GetCompilationUnitRoot();
            var newCompilationUnit = compilationUnit.Accept(sut);

            return newCompilationUnit.GetText().ToString();
        }
    }
}
