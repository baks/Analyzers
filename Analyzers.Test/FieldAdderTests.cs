using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Analyzers.Test
{
    public class FieldAdderTests
    {
        private FieldAdder sut;

        [Test]
        public void AddsFieldDeclarationToClass()
        {
            var source = "public class TestClass{}";
            var fields = new FieldInfo(SyntaxFactory.Token(SyntaxKind.PrivateKeyword), "object", "field1");

            var expected = "public class TestClass{privateobjectfield1;}";
            Assert.That(WithAddedFields(source, fields), Is.EqualTo(expected));
        }

        [Test]
        public void AddsFieldsToClass()
        {
            var source = "public class TestClass{}";
            var fields = new []
            {
                new FieldInfo(SyntaxFactory.Token(SyntaxKind.PrivateKeyword), "object", "field1"),
                new FieldInfo(SyntaxFactory.Token(SyntaxKind.PublicKeyword), "string", "field2") 
            };

            var expected = "public class TestClass{privateobjectfield1;publicstringfield2;}";
            Assert.That(WithAddedFields(source, fields), Is.EqualTo(expected));
        }

        private string WithAddedFields(string source, params FieldInfo[] fields)
        {
            sut = new FieldAdder(fields);
            var syntaxTree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default);
            var compilationUnit = syntaxTree.GetCompilationUnitRoot();
            var newCompilationUnit = compilationUnit.Accept(sut);

            return newCompilationUnit.GetText().ToString();
        }
    }
}
