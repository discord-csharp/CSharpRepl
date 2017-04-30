using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CSDiscordService
{
    public sealed class ConsoleRewriter : CSharpSyntaxRewriter
    {
        private static readonly IdentifierNameSyntax ConsoleIdentifier = IdentifierName("__Console");
        private readonly SemanticModel model;

        public ConsoleRewriter(SemanticModel semanticModel)
        {
            model = semanticModel;
        }

        public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            var symbolInfo = model.GetSymbolInfo(node);
            var methodSymbol = (IMethodSymbol)symbolInfo.Symbol;

            var containingTypeName = methodSymbol.ContainingType?.Name;
            var containingNamespaceName = methodSymbol.ContainingNamespace?.Name;
            var containingAssembly = methodSymbol.ContainingAssembly?.Name;

            if (string.Equals("Console", containingTypeName, StringComparison.Ordinal) &&
                string.Equals("System", containingNamespaceName, StringComparison.Ordinal) &&
                string.Equals("System.Console", containingAssembly, StringComparison.Ordinal))
            {
                var old = (MemberAccessExpressionSyntax)node.Expression;
                return node.ReplaceNode(old, old.Update(ConsoleIdentifier, old.OperatorToken, old.Name));
            }

            return node;
        }
    }
}
