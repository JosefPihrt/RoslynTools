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
using Microsoft.CodeAnalysis.Text;

namespace Roslynator.Testing
{
    /// <summary>
    /// Represents verifier for a diagnostic that is produced by <see cref="DiagnosticAnalyzer"/>.
    /// </summary>
    public abstract class DiagnosticVerifier<TAnalyzer, TFixProvider> : CodeVerifier
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TFixProvider : CodeFixProvider, new()
    {
        internal DiagnosticVerifier(IAssert assert) : base(assert)
        {
        }

        public async Task VerifyDiagnosticAsync(
            DiagnosticTestState state,
            TestOptions options = null,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            options ??= Options;

            TAnalyzer analyzer = Activator.CreateInstance<TAnalyzer>();
            ImmutableArray<DiagnosticDescriptor> supportedDiagnostics = analyzer.SupportedDiagnostics;

            using (Workspace workspace = new AdhocWorkspace())
            {
                (Document document, ImmutableArray<ExpectedDocument> expectedDocuments) = ProjectHelpers.CreateDocument(workspace.CurrentSolution, state, options);

                SyntaxTree tree = await document.GetSyntaxTreeAsync();

                ImmutableArray<Diagnostic> expectedDiagnostics = state.GetDiagnostics(tree);

                VerifySupportedDiagnostics(analyzer, expectedDiagnostics);

                Compilation compilation = await document.Project.GetCompilationAsync(cancellationToken);

                ImmutableArray<Diagnostic> compilerDiagnostics = compilation.GetDiagnostics(cancellationToken);

                VerifyCompilerDiagnostics(compilerDiagnostics, options);

                compilation = UpdateCompilation(compilation, expectedDiagnostics);

                ImmutableArray<Diagnostic> diagnostics = await compilation.GetAnalyzerDiagnosticsAsync(analyzer, DiagnosticComparer.SpanStart, cancellationToken);

                if (diagnostics.Length > 0
                    && supportedDiagnostics.Length > 1)
                {
                    VerifyDiagnostics(state, analyzer, expectedDiagnostics, FilterDiagnostics(diagnostics, expectedDiagnostics), cancellationToken);
                }
                else
                {
                    VerifyDiagnostics(state, analyzer, expectedDiagnostics, diagnostics, cancellationToken);
                }
            }

            static IEnumerable<Diagnostic> FilterDiagnostics(
                ImmutableArray<Diagnostic> diagnostics,
                ImmutableArray<Diagnostic> expectedDiagnostics)
            {
                foreach (Diagnostic diagnostic in diagnostics)
                {
                    var success = false;
                    foreach (Diagnostic expectedDiagnostic in expectedDiagnostics)
                    {
                        if (DiagnosticComparer.Id.Equals(diagnostic, expectedDiagnostic))
                        {
                            success = true;
                            break;
                        }
                    }

                    if (success)
                        yield return diagnostic;
                }
            }
        }

        /// <summary>
        /// Verifies that specified source will not produce diagnostic described with see <see cref="Descriptor"/>
        /// </summary>
        /// <param name="state"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyNoDiagnosticAsync(
            DiagnosticTestState state,
            TestOptions options = null,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            options ??= Options;

            TAnalyzer analyzer = Activator.CreateInstance<TAnalyzer>();

            using (Workspace workspace = new AdhocWorkspace())
            {
                (Document document, ImmutableArray<ExpectedDocument> expectedDocuments) = ProjectHelpers.CreateDocument(workspace.CurrentSolution, state, options);

                SyntaxTree tree = await document.GetSyntaxTreeAsync();

                ImmutableArray<Diagnostic> expectedDiagnostics = state.GetDiagnostics(tree);

                VerifySupportedDiagnostics(analyzer, expectedDiagnostics);

                Compilation compilation = await document.Project.GetCompilationAsync(cancellationToken);

                ImmutableArray<Diagnostic> compilerDiagnostics = compilation.GetDiagnostics(cancellationToken);

                VerifyCompilerDiagnostics(compilerDiagnostics, options);

                compilation = UpdateCompilation(compilation, expectedDiagnostics);

                ImmutableArray<Diagnostic> analyzerDiagnostics = await compilation.GetAnalyzerDiagnosticsAsync(analyzer, DiagnosticComparer.SpanStart, cancellationToken);

                ImmutableArray<Diagnostic> actualDiagnostics = analyzerDiagnostics.Intersect(
                    expectedDiagnostics,
                    DiagnosticComparer.Id)
                    .ToImmutableArray();

                if (!actualDiagnostics.IsEmpty)
                    Assert.True(false, $"No diagnostic expected{actualDiagnostics.ToDebugString()}");
            }
        }

        public async Task VerifyFixAsync(
            DiagnosticTestState state,
            TestOptions options = null,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            options ??= Options;

            TAnalyzer analyzer = Activator.CreateInstance<TAnalyzer>();
            TFixProvider fixProvider = Activator.CreateInstance<TFixProvider>();

            ImmutableArray<DiagnosticDescriptor> supportedDiagnostics = analyzer.SupportedDiagnostics;
            ImmutableArray<string> fixableDiagnosticIds = fixProvider.FixableDiagnosticIds;

            using (Workspace workspace = new AdhocWorkspace())
            {
                (Document document, ImmutableArray<ExpectedDocument> expectedDocuments) = ProjectHelpers.CreateDocument(workspace.CurrentSolution, state, options);

                Project project = document.Project;

                document = project.GetDocument(document.Id);

                SyntaxTree tree = await document.GetSyntaxTreeAsync();

                ImmutableArray<Diagnostic> expectedDiagnostics = state.GetDiagnostics(tree);

                foreach (Diagnostic diagnostic in expectedDiagnostics)
                {
                    if (!fixableDiagnosticIds.Contains(diagnostic.Id))
                        Assert.True(false, $"Diagnostic '{diagnostic.Id}' is not fixable by code fix provider '{fixProvider.GetType().Name}'.");
                }

                VerifySupportedDiagnostics(analyzer, expectedDiagnostics);

                Compilation compilation = await project.GetCompilationAsync(cancellationToken);

                ImmutableArray<Diagnostic> compilerDiagnostics = compilation.GetDiagnostics(cancellationToken);

                VerifyCompilerDiagnostics(compilerDiagnostics, options);

                compilation = UpdateCompilation(compilation, expectedDiagnostics);

                ImmutableArray<Diagnostic> previousDiagnostics = ImmutableArray<Diagnostic>.Empty;

                var fixRegistered = false;

                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    ImmutableArray<Diagnostic> diagnostics = await compilation.GetAnalyzerDiagnosticsAsync(analyzer, DiagnosticComparer.SpanStart, cancellationToken);

                    int length = diagnostics.Length;

                    if (length == 0)
                        break;

                    if (length == previousDiagnostics.Length
                        && diagnostics.Intersect(previousDiagnostics, DiagnosticDeepEqualityComparer.Instance).Count() == length)
                    {
                        Assert.True(false, "Same diagnostics returned before and after the fix was applied.");
                    }

                    Diagnostic diagnostic = null;
                    foreach (Diagnostic d in diagnostics)
                    {
                        foreach (Diagnostic d2 in expectedDiagnostics)
                        {
                            if (d.Id == d2.Id)
                            {
                                diagnostic = d;
                                break;
                            }
                        }

                        if (diagnostic != null)
                            break;
                    }

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

                    compilation = await document.Project.GetCompilationAsync(cancellationToken);

                    ImmutableArray<Diagnostic> newCompilerDiagnostics = compilation.GetDiagnostics(cancellationToken);

                    VerifyCompilerDiagnostics(newCompilerDiagnostics, options);

                    VerifyNoNewCompilerDiagnostics(compilerDiagnostics, newCompilerDiagnostics, options);

                    compilation = UpdateCompilation(compilation, expectedDiagnostics);

                    previousDiagnostics = diagnostics;
                }

                Assert.True(fixRegistered, "No code fix has been registered.");

                string actual = await document.ToFullStringAsync(simplify: true, format: true, cancellationToken);

                Assert.Equal(state.ExpectedSource, actual);

                if (expectedDocuments.Any())
                    await VerifyAdditionalDocumentsAsync(document.Project, expectedDocuments, cancellationToken);
            }
        }

        /// <summary>
        /// Verifies that specified source does not contains diagnostic that can be fixed with the <see cref="FixProvider"/>.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyNoFixAsync(
            DiagnosticTestState state,
            TestOptions options = null,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            options ??= Options;

            TAnalyzer analyzer = Activator.CreateInstance<TAnalyzer>();
            TFixProvider fixProvider = Activator.CreateInstance<TFixProvider>();

            ImmutableArray<DiagnosticDescriptor> supportedDiagnostics = analyzer.SupportedDiagnostics;
            ImmutableArray<string> fixableDiagnosticIds = fixProvider.FixableDiagnosticIds;

            using (Workspace workspace = new AdhocWorkspace())
            {
                (Document document, ImmutableArray<ExpectedDocument> expectedDocuments) = ProjectHelpers.CreateDocument(workspace.CurrentSolution, state, options);

                Compilation compilation = await document.Project.GetCompilationAsync(cancellationToken);

                SyntaxTree tree = await document.GetSyntaxTreeAsync();

                ImmutableArray<Diagnostic> expectedDiagnostics = state.GetDiagnostics(tree);

                VerifySupportedDiagnostics(analyzer, expectedDiagnostics);

                ImmutableArray<Diagnostic> compilerDiagnostics = compilation.GetDiagnostics(cancellationToken);

                VerifyCompilerDiagnostics(compilerDiagnostics, options);

                compilation = UpdateCompilation(compilation, expectedDiagnostics);

                ImmutableArray<Diagnostic> diagnostics = await compilation.GetAnalyzerDiagnosticsAsync(analyzer, DiagnosticComparer.SpanStart, cancellationToken);

                foreach (Diagnostic diagnostic in diagnostics)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (expectedDiagnostics.IndexOf(diagnostic, DiagnosticComparer.Id) == -1)
                        continue;

                    if (!fixableDiagnosticIds.Contains(diagnostic.Id))
                        continue;

                    var context = new CodeFixContext(
                        document,
                        diagnostic,
                        (a, d) =>
                        {
                            if ((state.EquivalenceKey == null || string.Equals(a.EquivalenceKey, state.EquivalenceKey, StringComparison.Ordinal))
                                && d.Contains(diagnostic))
                            {
                                Assert.True(false, "No code fix expected.");
                            }
                        },
                        CancellationToken.None);

                    await fixProvider.RegisterCodeFixesAsync(context);
                }
            }
        }

        private void VerifyDiagnostics(
            DiagnosticTestState state,
            TAnalyzer analyzer,
            IEnumerable<Diagnostic> expectedDiagnostics,
            IEnumerable<Diagnostic> actualDiagnostics,
            CancellationToken cancellationToken = default)
        {
            VerifyDiagnostics(
                state,
                analyzer,
                expectedDiagnostics,
                actualDiagnostics,
                checkAdditionalLocations: false,
                cancellationToken: cancellationToken);
        }

        private void VerifyDiagnostics(
            DiagnosticTestState state,
            TAnalyzer analyzer,
            IEnumerable<Diagnostic> expectedDiagnostics,
            IEnumerable<Diagnostic> actualDiagnostics,
            bool checkAdditionalLocations,
            CancellationToken cancellationToken = default)
        {
            int expectedCount = 0;
            int actualCount = 0;

            using (IEnumerator<Diagnostic> expectedEnumerator = expectedDiagnostics.OrderBy(f => f, DiagnosticComparer.SpanStart).GetEnumerator())
            using (IEnumerator<Diagnostic> actualEnumerator = actualDiagnostics.OrderBy(f => f, DiagnosticComparer.SpanStart).GetEnumerator())
            {
                if (!expectedEnumerator.MoveNext())
                    Assert.True(false, "Diagnostic's location not found in a source text.");

                do
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    expectedCount++;

                    Diagnostic expectedDiagnostic = expectedEnumerator.Current;

                    VerifySupportedDiagnostics(analyzer, expectedDiagnostic);

                    if (actualEnumerator.MoveNext())
                    {
                        actualCount++;

                        VerifyDiagnostic(
                            actualEnumerator.Current,
                            expectedDiagnostic,
                            state.DiagnosticMessage,
                            state.FormatProvider,
                            checkAdditionalLocations: checkAdditionalLocations);
                    }
                    else
                    {
                        while (expectedEnumerator.MoveNext())
                            expectedCount++;

                        ReportMismatch(actualDiagnostics, actualCount, expectedCount);
                    }

                } while (expectedEnumerator.MoveNext());

                if (actualEnumerator.MoveNext())
                {
                    actualCount++;

                    while (actualEnumerator.MoveNext())
                        actualCount++;

                    ReportMismatch(actualDiagnostics, actualCount, expectedCount);
                }
            }

            void ReportMismatch(IEnumerable<Diagnostic> actualDiagnostics, int actualCount, int expectedCount)
            {
                if (actualCount == 0)
                {
                    Assert.True(false, $"No diagnostic found, expected: {expectedCount}.");
                }
                else
                {
                    Assert.True(false, $"Mismatch between number of diagnostics, expected: {expectedCount} actual: {actualCount}{actualDiagnostics.ToDebugString()}");
                }
            }
        }

        private void VerifyDiagnostic(
            Diagnostic actualDiagnostic,
            Diagnostic expectedDiagnostic,
            string message,
            IFormatProvider formatProvider,
            bool checkAdditionalLocations = false)
        {
            if (actualDiagnostic.Id != expectedDiagnostic.Id)
                Assert.True(false, $"Diagnostic's ID expected to be \"{expectedDiagnostic.Id}\", actual: \"{actualDiagnostic.Id}\"{GetMessage()}");

            VerifyLocation(actualDiagnostic.Location, expectedDiagnostic.Location);

            if (checkAdditionalLocations)
                VerifyAdditionalLocations(actualDiagnostic.AdditionalLocations, expectedDiagnostic.AdditionalLocations);

            if (message != null)
            {
                Assert.Equal(message, actualDiagnostic.GetMessage(formatProvider));
            }

            void VerifyLocation(
                Location actualLocation,
                Location expectedLocation)
            {
                VerifyFileLinePositionSpan(actualLocation.GetLineSpan(), expectedLocation.GetLineSpan());
            }

            void VerifyAdditionalLocations(
                IReadOnlyList<Location> actual,
                IReadOnlyList<Location> expected)
            {
                int actualCount = actual.Count;
                int expectedCount = expected.Count;

                if (actualCount != expectedCount)
                    Assert.True(false, $"{expectedCount} additional location(s) expected, actual: {actualCount}{GetMessage()}");

                for (int j = 0; j < actualCount; j++)
                    VerifyLocation(actual[j], expected[j]);
            }

            void VerifyFileLinePositionSpan(
                FileLinePositionSpan actual,
                FileLinePositionSpan expected)
            {
                if (actual.Path != expected.Path)
                    Assert.True(false, $"Diagnostic expected to be in file \"{expected.Path}\", actual: \"{actual.Path}\"{GetMessage()}");

                VerifyLinePosition(actual.StartLinePosition, expected.StartLinePosition, "start");

                VerifyLinePosition(actual.EndLinePosition, expected.EndLinePosition, "end");
            }

            void VerifyLinePosition(
                LinePosition actual,
                LinePosition expected,
                string startOrEnd)
            {
                int actualLine = actual.Line;
                int expectedLine = expected.Line;

                if (actualLine != expectedLine)
                    Assert.True(false, $"Diagnostic expected to {startOrEnd} on line {expectedLine + 1}, actual: {actualLine + 1}{GetMessage()}");

                int actualCharacter = actual.Character;
                int expectedCharacter = expected.Character;

                if (actualCharacter != expectedCharacter)
                    Assert.True(false, $"Diagnostic expected to {startOrEnd} at column {expectedCharacter + 1}, actual: {actualCharacter + 1}{GetMessage()}");
            }

            string GetMessage()
            {
                return $"\r\n\r\nExpected diagnostic:\r\n{expectedDiagnostic}\r\n\r\nActual diagnostic:\r\n{actualDiagnostic}\r\n";
            }
        }

        private Compilation UpdateCompilation(
            Compilation compilation,
            ImmutableArray<Diagnostic> diagnostics)
        {
            foreach (Diagnostic diagnostic in diagnostics)
            {
                if (!diagnostic.Descriptor.IsEnabledByDefault)
                    compilation = compilation.EnsureEnabled(diagnostic.Descriptor);
            }

            return compilation;
        }
    }
}
