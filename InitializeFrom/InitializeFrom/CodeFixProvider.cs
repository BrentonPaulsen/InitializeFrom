using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace InitializeFrom
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(InitializeFromCodeFixProvider)), Shared]
    public class InitializeFromCodeFixProvider : CodeFixProvider
    {
        private const string Title = "Initialize from";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(InitializeFromAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<InitializerExpressionSyntax>().First();

            var assignments = 
                diagnostic.Properties.OrderBy(x => x.Key).Select(
                    x => SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                        SyntaxFactory.IdentifierName(x.Key),
                        SyntaxFactory.IdentifierName(x.Value)
                        )).ToArray();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: c => MakeAssignmentsAsync(context.Document, declaration, assignments, c),
                    equivalenceKey: Title),
                diagnostic);
        }

        private async Task<Document> MakeAssignmentsAsync(Document document, InitializerExpressionSyntax initializer, ExpressionSyntax[] assignments, CancellationToken cancellationToken)
        {
            // Compute new uppercase name.
            var newInit = initializer.AddExpressions(assignments);

            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = root.ReplaceNode(initializer, newInit);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}