namespace GuidFromStringAnalysis
{
    using System;
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class GuidFromStringAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GuidFromString";

        private const string Category = "AnalyzerPerformance";

        private static readonly LocalizableString Title =
            new LocalizableResourceString(
                nameof(Resources.AnalyzerTitle),
                Resources.ResourceManager,
                typeof(Resources));

        private static readonly LocalizableString MessageFormat =
            new LocalizableResourceString(
                nameof(Resources.AnalyzerMessageFormat),
                Resources.ResourceManager,
                typeof(Resources));

        private static readonly LocalizableString Description =
            new LocalizableResourceString(
                nameof(Resources.AnalyzerDescription),
                Resources.ResourceManager,
                typeof(Resources));

        private static readonly DiagnosticDescriptor Rule =
            new DiagnosticDescriptor(
                DiagnosticId, Title, MessageFormat, Category,
                DiagnosticSeverity.Info, isEnabledByDefault: true,
                description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeObjectCreation, SyntaxKind.ObjectCreationExpression);
        }

        private static void AnalyzeObjectCreation(SyntaxNodeAnalysisContext context)
        {
            var creationExpr = (ObjectCreationExpressionSyntax)context.Node;

            var symbolInfo = context.SemanticModel.GetSymbolInfo(creationExpr.Type);
            var typeSymbol = symbolInfo.Symbol as INamedTypeSymbol;
            if (typeSymbol == null || typeSymbol.ToString() != typeof(Guid).FullName)
                return;

            var arguments = creationExpr.ArgumentList.Arguments;
            if (arguments.Count == 1 && arguments[0].Expression.Kind() == SyntaxKind.StringLiteralExpression) {
                var diagnostic = Diagnostic.Create(Rule, arguments[0].GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
