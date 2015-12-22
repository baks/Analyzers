using System;
using Microsoft.CodeAnalysis;

namespace Analyzers
{
    public class FieldInfo
    {
        public readonly SyntaxToken AccessibilityModifier;
        public readonly string Type;
        public readonly string FieldName;

        public FieldInfo(SyntaxToken accessibilityModifier, string type, string fieldName)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            if (fieldName == null)
            {
                throw new ArgumentNullException(nameof(fieldName));
            }
            AccessibilityModifier = accessibilityModifier;
            Type = type;
            FieldName = fieldName;
        }
    }
}