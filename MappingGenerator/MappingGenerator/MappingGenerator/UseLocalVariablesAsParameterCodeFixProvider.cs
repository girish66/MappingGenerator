using System;
using System.Composition;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MappingGenerator
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseLocalVariablesAsParameterCodeFixProvider)), Shared]
    public class UseLocalVariablesAsParameterCodeFixProvider : CodeFixProvider
    {
        private const string title = "Use local variables as parameters";
        
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create("CS1501", "CS7036");

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics.First();
            var token = root.FindToken(diagnostic.Location.SourceSpan.Start);
            
            var statement = token.Parent.FindContainer<InvocationExpressionSyntax>();
            if (statement == null || statement.ArgumentList.Arguments.Count != 0)
            {
                return;
            }

            context.RegisterCodeFix(CodeAction.Create(title: title, createChangedDocument: c => UseLocalVariablesAsParameters(context.Document, statement, c), equivalenceKey: title), diagnostic);
        }

        private async Task<Document> UseLocalVariablesAsParameters(Document document, InvocationExpressionSyntax invocationExpression, CancellationToken cancellationToken)
        {
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);

            var methodSymbol = semanticModel.GetSymbolInfo(invocationExpression, cancellationToken).CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault();
            if (methodSymbol != null)
            {
                var mappingSourceFinder = new LocalScopeMappingSourceFinder(semanticModel, invocationExpression);
                var argumentList = MethodHelper.FindBestArgumentsMatch(methodSymbol, semanticModel, mappingSourceFinder);
                if (argumentList != null)
                {
                    var root = await document.GetSyntaxRootAsync(cancellationToken);
                    var newRoot = root.ReplaceNode(invocationExpression, invocationExpression.WithArgumentList(argumentList));
                    return document.WithSyntaxRoot(newRoot);
                }
            }
            return document;
        }
    }
}