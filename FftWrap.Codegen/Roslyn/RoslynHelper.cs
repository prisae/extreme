using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FftWrap.Codegen
{
    public static class RoslynHelper
    {
        public static IdentifierNameSyntax ToIdentifierName(this string str)
        {
            return SyntaxFactory.IdentifierName(str);
        }

        public static NamespaceDeclarationSyntax ToNamespaceDeclaration(this string str)
        {
            return SyntaxFactory.NamespaceDeclaration(str.ToIdentifierName());
        }

        public static VariableDeclaratorSyntax ToVariableDeclarator(this string str)
        {
            return SyntaxFactory.VariableDeclarator(str);
        }

        public static FieldDeclarationSyntax AddVariable(this FieldDeclarationSyntax syntax, string name)
        {
            return syntax.AddDeclarationVariables(name.ToVariableDeclarator());
        }

        public static FieldDeclarationSyntax AddVariable(this FieldDeclarationSyntax syntax, string name, string value)
        {
            var st = SyntaxFactory.ParseExpression(value);
            
            return syntax.AddDeclarationVariables(name.ToVariableDeclarator()
                .WithInitializer(SyntaxFactory.EqualsValueClause(st)));
        }

        public static UsingDirectiveSyntax ToUsingDirective(this string str)
        {
            return SyntaxFactory.UsingDirective(str.ToIdentifierName());
        }
    }
}
