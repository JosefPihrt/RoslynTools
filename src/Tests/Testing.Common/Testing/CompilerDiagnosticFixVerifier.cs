// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Roslynator.Testing
{
    /// <summary>
    /// Represents a verifier for compiler diagnostic.
    /// </summary>
    public abstract class CompilerDiagnosticFixVerifier<TFixProvider> : CodeVerifier
        where TFixProvider : CodeFixProvider, new()
    {
        internal CompilerDiagnosticFixVerifier(IAssert assert) : base(assert)
        {
        }

        /// <summary>
        /// Verifies that specified source will produce compiler diagnostic with ID specified in <see cref="DiagnosticId"/>.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyFixAsync(
            CompilerDiagnosticFixTestState state,
            TestOptions options = null,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            options ??= Options;

            TFixProvider fixProvider = Activator.CreateInstance<TFixProvider>();
            ImmutableArray<string> fixableDiagnosticIds = fixProvider.FixableDiagnosticIds;

            if (!fixableDiagnosticIds.Contains(state.DiagnosticId))
                Assert.True(false, $"Code fix provider '{fixProvider.GetType().Name}' cannot fix diagnostic '{state.DiagnosticId}'.");

            using (Workspace workspace = new AdhocWorkspace())
            {
                (Document document, ImmutableArray<ExpectedDocument> expectedDocuments) = ProjectHelpers.CreateDocument(workspace.CurrentSolution, state, options);

                Project project = document.Project;

                document = project.GetDocument(document.Id);

                ImmutableArray<Diagnostic> previousDiagnostics = ImmutableArray<Diagnostic>.Empty;

                var fixRegistered = false;

                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    Compilation compilation = await document.Project.GetCompilationAsync(cancellationToken);

                    ImmutableArray<Diagnostic> diagnostics = compilation.GetDiagnostics(cancellationToken: cancellationToken);

                    int length = diagnostics.Length;

                    if (length == 0)
                        break;

                    if (previousDiagnostics.Any())
                        VerifyNoNewCompilerDiagnostics(previousDiagnostics, diagnostics, options);

                    if (length == previousDiagnostics.Length
                        && !diagnostics.Except(previousDiagnostics, DiagnosticDeepEqualityComparer.Instance).Any())
                    {
                        Assert.True(false, "Same diagnostics returned before and after the fix was applied.");
                    }

                    Diagnostic diagnostic = FindDiagnostic(diagnostics);

                    if (diagnostic == null)
                        break;

                    CodeAction action = null;

                    var context = new CodeFixContext(
                        document,
                        diagnostic,
                        (a, d) =>
                        {
                            if (action == null
                                && (state.EquivalenceKey == null
                                    || string.Equals(a.EquivalenceKey, state.EquivalenceKey, StringComparison.Ordinal))
                                && d.Contains(diagnostic))
                            {
                                if (action != null)
                                    Assert.True(false, "Multiple fixes available.");

                                action = a;
                            }
                        },
                        CancellationToken.None);

                    await fixProvider.RegisterCodeFixesAsync(context);

                    if (action == null)
                        break;

                    fixRegistered = true;

                    document = await VerifyAndApplyCodeActionAsync(document, action, state.CodeActionTitle);

                    previousDiagnostics = diagnostics;
                }

                Assert.True(fixRegistered, "No code fix has been registered.");

                string actual = await document.ToFullStringAsync(simplify: true, format: true, cancellationToken);

                Assert.Equal(state.ExpectedSource, actual);

                if (expectedDocuments.Any())
                    await VerifyAdditionalDocumentsAsync(document.Project, expectedDocuments, cancellationToken);
            }

            Diagnostic FindDiagnostic(ImmutableArray<Diagnostic> diagnostics)
            {
                Diagnostic match = null;

                foreach (Diagnostic diagnostic in diagnostics)
                {
                    if (string.Equals(diagnostic.Id, state.DiagnosticId, StringComparison.Ordinal))
                    {
                        if (match == null
                            || diagnostic.Location.SourceSpan.Start > match.Location.SourceSpan.Start)
                        {
                            match = diagnostic;
                        }
                    }
                }

                return match;
            }
        }

        /// <summary>
        /// Verifies that specified source will not produce compiler diagnostic with ID specified in <see cref="DiagnosticId"/>.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyNoFixAsync(
            CompilerDiagnosticFixTestState state,
            TestOptions options = null,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            options ??= Options;

            TFixProvider fixProvider = Activator.CreateInstance<TFixProvider>();
            ImmutableArray<string> fixableDiagnosticIds = fixProvider.FixableDiagnosticIds;

            using (Workspace workspace = new AdhocWorkspace())
            {
                (Document document, ImmutableArray<ExpectedDocument> expectedDocuments) = ProjectHelpers.CreateDocument(workspace.CurrentSolution, state, options);

                Compilation compilation = await document.Project.GetCompilationAsync(cancellationToken);

                foreach (Diagnostic diagnostic in compilation.GetDiagnostics(cancellationToken: cancellationToken))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (!fixableDiagnosticIds.Contains(diagnostic.Id))
                        continue;

                    var context = new CodeFixContext(
                        document,
                        diagnostic,
                        (a, d) =>
                        {
                            if (!d.Contains(diagnostic))
                                return;

                            if (state.EquivalenceKey != null
                                && !string.Equals(a.EquivalenceKey, state.EquivalenceKey, StringComparison.Ordinal))
                            {
                                return;
                            }

                            Assert.True(false, "No code fix expected.");
                        },
                        CancellationToken.None);

                    await fixProvider.RegisterCodeFixesAsync(context);
                }
            }
        }
    }
}
