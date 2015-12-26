using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace InitializeFrom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class InitializeFromAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "InitializeFrom";

        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof (Resources));

        private static readonly LocalizableString MessageFormat =
            new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager,
                typeof (Resources));

        private static readonly LocalizableString Description =
            new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager,
                typeof (Resources));

        private const string Category = "Convenience";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category,
            DiagnosticSeverity.Info, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeSymbol, SyntaxKind.ObjectInitializerExpression);
        }

        private static void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
        {
            var type = context.SemanticModel.GetTypeInfo(context.Node.Parent);
            var members = type.Type.GetTypedMembers().ToDictionary(x => x.Symbol.Name);

            var location = context.Node.GetLocation().SourceSpan.Start;
            var scope = context.SemanticModel.LookupSymbols(location);
            var parameters = scope.OfType<IParameterSymbol>().Select(x => new ReferenceProperties
            {
                Reference = x,
                Type = x.Type,
                Properties = x.Type.GetTypedMembers().Where(y => MemberMatches(y, members, context.SemanticModel, location))
            });

            var locals = scope.OfType<ILocalSymbol>().Select(x => new ReferenceProperties
            {
                Reference = x,
                Type = x.Type,
                Properties = x.Type.GetTypedMembers().Where(y => MemberMatches(y, members, context.SemanticModel, location))
            });

            var fields = scope.OfType<IFieldSymbol>().Select(x => new ReferenceProperties
            {
                Reference = x,
                Type = x.Type,
                Properties = x.Type.GetTypedMembers().Where(y => MemberMatches(y, members, context.SemanticModel, location))
            });

            var properties = scope.OfType<IPropertySymbol>().Select(x => new ReferenceProperties
            {
                Reference = x,
                Type = x.Type,
                Properties = x.Type.GetTypedMembers().Where(y => MemberMatches(y, members, context.SemanticModel, location))
            });

            var names =
                parameters.Union(locals)
                    .Union(fields)
                    .Union(properties)
                    .Where(x => x.Properties.Count() > 1 && x.Type.Name != type.Type.Name);

            foreach (var name in names)
            {
                var assignment = new Dictionary<string, string>();
                foreach (var prop in name.Properties)
                {
                    assignment.Add(prop.Symbol.Name, name.Reference.Name + "." + prop.Symbol.Name);
                }
                var diagnostic = Diagnostic.Create(Rule, context.Node.GetLocation(), properties: assignment.ToImmutableDictionary());
                context.ReportDiagnostic(diagnostic);
            }

        }

        private static bool MemberMatches(TypedSymbol symbol, Dictionary<string, TypedSymbol> members, SemanticModel model,
            int location)
        {
            return members.ContainsKey(symbol.Symbol.Name) &&
                   members[symbol.Symbol.Name].Type.ToMinimalDisplayString(model, location) ==
                   symbol.Type.ToMinimalDisplayString(model, location);
        }
    }
}
