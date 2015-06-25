using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace FftWrap.Codegen
{
    public static class AccessModifiers
    {
        public static SyntaxToken Public = SyntaxFactory.Token(SyntaxKind.PublicKeyword);
        public static SyntaxToken Internal = SyntaxFactory.Token(SyntaxKind.InternalKeyword);
        public static SyntaxToken Private = SyntaxFactory.Token(SyntaxKind.PrivateKeyword);
        public static SyntaxToken Protected = SyntaxFactory.Token(SyntaxKind.ProtectedKeyword);
    }
}
