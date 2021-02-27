// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
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

        /// <summary>
        /// Gets a test assertions.
        /// </summary>
        internal IAssert Assert { get; }

        internal void VerifyCompilerDiagnostics(
            ImmutableArray<Diagnostic> diagnostics,
            TestOptions options)
        {
            DiagnosticSeverity maxAllowedSeverity = options.AllowedCompilerDiagnosticSeverity;

            ImmutableArray<string> allowedDiagnosticIds = options.AllowedCompilerDiagnosticIds;

            if (IsAny())
            {
                IEnumerable<Diagnostic> notAllowed = diagnostics
                    .Where(f => f.Severity > maxAllowedSeverity && !allowedDiagnosticIds.Any(id => id == f.Id));

                Assert.True(false, $"No compiler diagnostics with severity higher than '{maxAllowedSeverity}' expected{notAllowed.ToDebugString()}");
            }

            bool IsAny()
            {
                foreach (Diagnostic diagnostic in diagnostics)
                {
                    if (diagnostic.Severity > maxAllowedSeverity
                        && !IsAllowed(diagnostic))
                    {
                        return true;
                    }
                }

                return false;
            }

            bool IsAllowed(Diagnostic diagnostic)
            {
                foreach (string diagnosticId in allowedDiagnosticIds)
                {
                    if (diagnostic.Id == diagnosticId)
                        return true;
                }

                return false;
            }
        }

        internal void VerifyNoNewCompilerDiagnostics(
            ImmutableArray<Diagnostic> diagnostics,
            ImmutableArray<Diagnostic> newDiagnostics,
            TestOptions options)
        {
            ImmutableArray<string> allowedDiagnosticIds = options.AllowedCompilerDiagnosticIds;

            if (allowedDiagnosticIds.IsDefault)
                allowedDiagnosticIds = ImmutableArray<string>.Empty;

            if (IsAnyNewCompilerDiagnostic())
            {
                IEnumerable<Diagnostic> diff = newDiagnostics
                    .Where(diagnostic => !allowedDiagnosticIds.Any(id => id == diagnostic.Id))
                    .Except(diagnostics, DiagnosticDeepEqualityComparer.Instance);

                Assert.True(false, $"Code fix introduced new compiler diagnostic(s).{diff.ToDebugString()}");
            }

            bool IsAnyNewCompilerDiagnostic()
            {
                foreach (Diagnostic newDiagnostic in newDiagnostics)
                {
                    if (!IsAllowed(newDiagnostic)
                        && !EqualsAny(newDiagnostic))
                    {
                        return true;
                    }
                }

                return false;
            }

            bool IsAllowed(Diagnostic diagnostic)
            {
                foreach (string diagnosticId in allowedDiagnosticIds)
                {
                    if (diagnostic.Id == diagnosticId)
                        return true;
                }

                return false;
            }

            bool EqualsAny(Diagnostic newDiagnostic)
            {
                foreach (Diagnostic diagnostic in diagnostics)
                {
                    if (DiagnosticDeepEqualityComparer.Instance.Equals(diagnostic, newDiagnostic))
                        return true;
                }

                return false;
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
            {
                VerifySupportedDiagnostics(analyzer, diagnostic);
            }
        }

        internal void VerifySupportedDiagnostics(DiagnosticAnalyzer analyzer, Diagnostic diagnostic)
        {
            if (analyzer.SupportedDiagnostics.IndexOf(diagnostic.Descriptor, DiagnosticDescriptorComparer.Id) == -1)
                Assert.True(false, $"Diagnostic \"{diagnostic.Id}\" is not supported by analyzer '{analyzer.GetType().Name}'.");
        }
    }
}
