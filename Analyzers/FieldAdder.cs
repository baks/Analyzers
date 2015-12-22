using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Analyzers
{
    public class FieldAdder : CSharpSyntaxRewriter
    {
        private readonly FieldInfo[] fields;

        public FieldAdder(params FieldInfo[] fields)
        {
            if (fields == null)
            {
                throw new ArgumentNullException(nameof(fields));
            }
            this.fields = fields;
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
            => base.VisitClassDeclaration(node.AddMembers(fields.Select(NewField).ToArray()));

        private static MemberDeclarationSyntax NewField(FieldInfo fieldInfo)
        {
            return SyntaxFactory.FieldDeclaration(SyntaxFactory.List<AttributeListSyntax>(),
                SyntaxFactory.TokenList(fieldInfo.AccessibilityModifier),
                SyntaxFactory.VariableDeclaration(FieldType(fieldInfo.Type),
                    VariableName(fieldInfo.FieldName)));
        }

        private static IdentifierNameSyntax FieldType(string type) => SyntaxFactory.IdentifierName(type);

        private static SeparatedSyntaxList<VariableDeclaratorSyntax> VariableName(string fieldName)
            => SyntaxFactory.SeparatedList(new[] {SyntaxFactory.VariableDeclarator(fieldName)});
    }
}
