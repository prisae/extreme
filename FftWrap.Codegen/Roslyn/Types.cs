using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace FftWrap.Codegen
{
    public static class Types
    {
        public static SyntaxToken String = SyntaxFactory.Token(SyntaxKind.StringKeyword);
    }
}
