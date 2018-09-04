namespace GuidFromStringAnalysis
{
    using System;
    using System.Collections.Immutable;
    using System.Composition;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GuidFromStringCodeFixProvider))]
    [Shared]
    public class GuidFromStringCodeFixProvider : CodeFixProvider
    {
        private const string Title = "Construct from integers";

        public sealed override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GuidFromStringAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var expr = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf()
                .OfType<ObjectCreationExpressionSyntax>().First();

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: c => ConstructGuidFromIntegersAsync(context.Document, expr, c),
                    equivalenceKey: Title),
                diagnostic);
        }

        private async Task<Document> ConstructGuidFromIntegersAsync(
            Document document, ObjectCreationExpressionSyntax creationExpr, CancellationToken cancellationToken)
        {
            var argument = creationExpr.ArgumentList.Arguments[0];
            var literal = (LiteralExpressionSyntax)argument.Expression;
            var guidStr = literal.Token.Text.Substring(1, literal.Token.Text.Length - 2);
            if (!Guid.TryParse(guidStr, out var guid))
                return document;

            var bytes = guid.ToByteArray();
            var data1 = BitConverter.ToUInt32(bytes, 0);
            var data2 = BitConverter.ToUInt16(bytes, 4);
            var data3 = BitConverter.ToUInt16(bytes, 6);

            var newArgList = SeparatedList<ArgumentSyntax>(
                new SyntaxNodeOrToken[]{
                    // Add any trivia of the original argument to preserve whitespace.
                    Argument(PaddedHexLiteral(data1)).WithLeadingTrivia(argument.GetLeadingTrivia()),
                    Token(SyntaxKind.CommaToken),
                    Argument(PaddedHexLiteral(data2)),
                    Token(SyntaxKind.CommaToken),
                    Argument(PaddedHexLiteral(data3)),
                    Token(SyntaxKind.CommaToken),
                    Argument(PaddedHexLiteral(bytes[8])),
                    Token(SyntaxKind.CommaToken),
                    Argument(PaddedHexLiteral(bytes[9])),
                    Token(SyntaxKind.CommaToken),
                    Argument(PaddedHexLiteral(bytes[10])),
                    Token(SyntaxKind.CommaToken),
                    Argument(PaddedHexLiteral(bytes[11])),
                    Token(SyntaxKind.CommaToken),
                    Argument(PaddedHexLiteral(bytes[12])),
                    Token(SyntaxKind.CommaToken),
                    Argument(PaddedHexLiteral(bytes[13])),
                    Token(SyntaxKind.CommaToken),
                    Argument(PaddedHexLiteral(bytes[14])),
                    Token(SyntaxKind.CommaToken),
                    Argument(PaddedHexLiteral(bytes[15])),
                });

            var oldRoot = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = oldRoot.ReplaceNode(argument, newArgList);

            return document.WithSyntaxRoot(newRoot);
        }

        private static LiteralExpressionSyntax PaddedHexLiteral(byte value)
        {
            return LiteralExpression(
                SyntaxKind.NumericLiteralExpression,
                Literal("0x" + value.ToString("X2"), value));
        }

        private static LiteralExpressionSyntax PaddedHexLiteral(ushort value)
        {
            return LiteralExpression(
                SyntaxKind.NumericLiteralExpression,
                Literal("0x" + value.ToString("X4"), value));
        }

        private static LiteralExpressionSyntax PaddedHexLiteral(uint value)
        {
            return LiteralExpression(
                SyntaxKind.NumericLiteralExpression,
                Literal("0x" + value.ToString("X8"), value));
        }
    }
}
