// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.Testing
{
    /// <summary>
    /// Represents base type for verifying a diagnostic, a code fix and a refactoring.
    /// </summary>
    public abstract class CodeVerifier
    {
        internal CodeVerifier(IAssert assert)
        {
            Assert = assert;
        }

        /// <summary>
        /// Gets a common code verification options.
        /// </summary>
        protected abstract TestOptions CommonOptions { get; }

        /// <summary>
        /// Gets a code verification options.
        /// </summary>
        public TestOptions Options => CommonOptions;

        internal IAssert Assert { get; }

        internal void VerifyCompilerDiagnostics(
            ImmutableArray<Diagnostic> diagnostics,
            TestOptions options)
        {
            foreach (Diagnostic diagnostic in diagnostics)
            {
                if (!options.IsAllowedCompilerDiagnostic(diagnostic))
                {
                    Assert.True(false, $"No compiler diagnostics with severity higher than '{options.AllowedCompilerDiagnosticSeverity}' expected"
                        + diagnostics.Where(d => !options.IsAllowedCompilerDiagnostic(d)).ToDebugString());
                }
            }
        }

        internal void VerifyNoNewCompilerDiagnostics(
            ImmutableArray<Diagnostic> diagnostics,
            ImmutableArray<Diagnostic> newDiagnostics,
            TestOptions options)
        {
            foreach (Diagnostic newDiagnostic in newDiagnostics)
            {
                if (!options.IsAllowedCompilerDiagnostic(newDiagnostic)
                    && IsNewCompilerDiagnostic(newDiagnostic))
                {
                    IEnumerable<Diagnostic> diff = newDiagnostics
                        .Where(diagnostic => !options.IsAllowedCompilerDiagnostic(diagnostic))
                        .Except(diagnostics, DiagnosticDeepEqualityComparer.Instance);

                    var message = "Code fix introduced new compiler diagnostic";

                    if (diff.Count() > 1)
                        message += "s";

                    message += ".";

                    Assert.True(false, message + diff.ToDebugString());
                }
            }

            bool IsNewCompilerDiagnostic(Diagnostic newDiagnostic)
            {
                foreach (Diagnostic diagnostic in diagnostics)
                {
                    if (DiagnosticDeepEqualityComparer.Instance.Equals(diagnostic, newDiagnostic))
                        return false;
                }

                return true;
            }
        }

        internal async Task VerifyAdditionalDocumentsAsync(
            Project project,
            ImmutableArray<ExpectedDocument> expectedDocuments,
            CancellationToken cancellationToken = default)
        {
            foreach (ExpectedDocument expectedDocument in expectedDocuments)
            {
                Document document = project.GetDocument(expectedDocument.Id);

                string actual = await document.ToFullStringAsync(simplify: true, format: true, cancellationToken);

                Assert.Equal(expectedDocument.Text, actual);
            }
        }

        internal async Task<Document> VerifyAndApplyCodeActionAsync(
            Document document,
            CodeAction codeAction,
            string title)
        {
            if (title != null)
                Assert.Equal(title, codeAction.Title);

            ImmutableArray<CodeActionOperation> operations = await codeAction.GetOperationsAsync(CancellationToken.None);

            return operations
                .OfType<ApplyChangesOperation>()
                .Single()
                .ChangedSolution
                .GetDocument(document.Id);
        }

        internal void VerifySupportedDiagnostics(
            DiagnosticAnalyzer analyzer,
            ImmutableArray<Diagnostic> diagnostics)
        {
            foreach (Diagnostic diagnostic in diagnostics)
                VerifySupportedDiagnostics(analyzer, diagnostic);
        }

        internal void VerifySupportedDiagnostics(DiagnosticAnalyzer analyzer, Diagnostic diagnostic)
        {
            if (analyzer.SupportedDiagnostics.IndexOf(diagnostic.Descriptor, DiagnosticDescriptorComparer.Id) == -1)
                Assert.True(false, $"Diagnostic \"{diagnostic.Id}\" is not supported by '{analyzer.GetType().Name}'.");
        }

        internal void VerifyFixableDiagnostics(CodeFixProvider fixProvider, string diagnosticId)
        {
            ImmutableArray<string> fixableDiagnosticIds = fixProvider.FixableDiagnosticIds;

            if (!fixableDiagnosticIds.Contains(diagnosticId))
                Assert.True(false, $"Diagnostic '{diagnosticId}' is not fixable by '{fixProvider.GetType().Name}'.");
        }
    }
}
