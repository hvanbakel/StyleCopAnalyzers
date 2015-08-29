﻿namespace StyleCop.Analyzers.DocumentationRules
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using StyleCop.Analyzers.Helpers;

    /// <summary>
    /// Implements a code fix for SA1633.
    /// </summary>
    /// <remarks>
    /// <para>To fix a violation of this rule, remove the <c>&lt;returns&gt;</c> tag from the element.</para>
    /// </remarks>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SA1633CodeFixProvider))]
    [Shared]
    public class SA1633CodeFixProvider : CodeFixProvider
    {
        private const string CompanyName = "FooCorp"; // Should come from settings.

        private static readonly ImmutableArray<string> FixableDiagnostics =
            ImmutableArray.Create(FileHeaderAnalyzers.SA1633Identifier);

        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds => FixableDiagnostics;

        /// <inheritdoc/>
        public override FixAllProvider GetFixAllProvider()
        {
            return CustomFixAllProviders.BatchFixer;
        }

        /// <inheritdoc/>
        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (Diagnostic diagnostic in context.Diagnostics.Where(d => FixableDiagnostics.Contains(d.Id)))
            {
                context.RegisterCodeFix(CodeAction.Create(DocumentationResources.SA1633CodeFix, token => GetTransformedDocumentAsync(context.Document, token), equivalenceKey: nameof(SA1633CodeFixProvider)), diagnostic);
            }

            return SpecializedTasks.CompletedTask;
        }

        private static async Task<Document> GetTransformedDocumentAsync(Document document, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            var fileHeader = FileHeaderHelpers.ParseFileHeader(root);
            SyntaxNode newSyntaxRoot;
            if (fileHeader.IsMissing)
            {
                newSyntaxRoot = AddHeader(root, document.Name);
            }
            else
            {
                newSyntaxRoot = ReplaceHeader(document, root);
            }

            return document.WithSyntaxRoot(newSyntaxRoot);
        }

        private static SyntaxNode ReplaceHeader(Document document, SyntaxNode root)
        {
            var existingTrivia = root.GetLeadingTrivia().Where(x => !x.IsKind(SyntaxKind.EndOfLineTrivia) && !x.IsKind(SyntaxKind.SingleLineCommentTrivia));
            return root.WithLeadingTrivia(CreateNewHeader(document.Name).AddRange(existingTrivia).Add(SyntaxFactory.CarriageReturnLineFeed));
        }

        private static SyntaxNode AddHeader(SyntaxNode root, string name)
        {
            var newTrivia = CreateNewHeader(name).AddRange(root.GetLeadingTrivia());
            if (!newTrivia.Last().IsKind(SyntaxKind.EndOfLineTrivia))
            {
                newTrivia = newTrivia.Add(SyntaxFactory.CarriageReturnLineFeed);
            }

            return root.WithLeadingTrivia(newTrivia);
        }

        private static SyntaxTriviaList CreateNewHeader(string name)
        {
            return SyntaxFactory.ParseLeadingTrivia($@"// <copyright file=""{name}"" company=""{CompanyName}"">
//   Copyright (c) FooCorp. All rights reserved.
// </copyright>
");
        }
    }
}