// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Roslynator.Testing.CSharp.Xunit;
using Roslynator.Testing.Text;

namespace Roslynator.Testing.CSharp
{
    public abstract class AbstractCSharpDiagnosticVerifier<TAnalyzer, TFixProvider> : XunitDiagnosticVerifier<TAnalyzer, TFixProvider>
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TFixProvider : CodeFixProvider, new()
    {
        public abstract DiagnosticDescriptor Descriptor { get; }

        public override CSharpTestOptions Options => DefaultCSharpTestOptions.Value;

        public async Task VerifyDiagnosticAsync(
            string source,
            IEnumerable<string> additionalFiles = null,
            TestOptions options = null,
            CancellationToken cancellationToken = default)
        {
            var code = TestCode.Parse(source);

            Debug.Assert(code.Spans.Length > 0);

            var state = new DiagnosticTestState(
                code.Value,
                expectedSource: null,
                Descriptor,
                code.Spans,
                code.AdditionalSpans,
                additionalFiles: AdditionalFile.CreateRange(additionalFiles));

            await VerifyDiagnosticAsync(
                state,
                options: options,
                cancellationToken: cancellationToken);
        }

        public async Task VerifyDiagnosticAsync(
            string source,
            string sourceData,
            IEnumerable<string> additionalFiles = null,
            TestOptions options = null,
            CancellationToken cancellationToken = default)
        {
            var code = TestCode.Parse(source, sourceData);

            Debug.Assert(code.Spans.Length > 0);

            var state = new DiagnosticTestState(
                source,
                expectedSource: null,
                Descriptor,
                code.Spans,
                code.AdditionalSpans,
                additionalFiles: AdditionalFile.CreateRange(additionalFiles));

            await VerifyDiagnosticAsync(
                state,
                options: options,
                cancellationToken: cancellationToken);
        }

        internal async Task VerifyDiagnosticAsync(
            string source,
            TextSpan span,
            IEnumerable<string> additionalFiles = null,
            TestOptions options = null,
            CancellationToken cancellationToken = default)
        {
            var state = new DiagnosticTestState(
                source,
                expectedSource: null,
                Descriptor,
                ImmutableArray.Create(span),
                additionalFiles: AdditionalFile.CreateRange(additionalFiles));

            await VerifyDiagnosticAsync(
                state,
                options: options,
                cancellationToken: cancellationToken);
        }

        internal async Task VerifyDiagnosticAsync(
            string source,
            IEnumerable<TextSpan> spans,
            IEnumerable<string> additionalFiles = null,
            TestOptions options = null,
            CancellationToken cancellationToken = default)
        {
            var state = new DiagnosticTestState(
                source,
                expectedSource: null,
                Descriptor,
                spans,
                additionalFiles: AdditionalFile.CreateRange(additionalFiles));

            await VerifyDiagnosticAsync(
                state,
                options: options,
                cancellationToken: cancellationToken);
        }

        public async Task VerifyNoDiagnosticAsync(
            string source,
            string sourceData,
            IEnumerable<string> additionalFiles = null,
            TestOptions options = null,
            CancellationToken cancellationToken = default)
        {
            var code = TestCode.Parse(source, sourceData);

            Debug.Assert(code.Spans.Length == 0);

            var state = new DiagnosticTestState(
                code.Value,
                code.ExpectedValue,
                Descriptor,
                code.Spans,
                code.AdditionalSpans,
                AdditionalFile.CreateRange(additionalFiles));

            await VerifyNoDiagnosticAsync(
                state,
                options: options,
                cancellationToken);
        }

        public async Task VerifyNoDiagnosticAsync(
            string source,
            IEnumerable<string> additionalFiles = null,
            TestOptions options = null,
            CancellationToken cancellationToken = default)
        {
            var state = new DiagnosticTestState(
                source,
                expectedSource: null,
                Descriptor,
                spans: null,
                additionalFiles: AdditionalFile.CreateRange(additionalFiles));

            await VerifyNoDiagnosticAsync(
                state,
                options: options,
                cancellationToken);
        }

        public async Task VerifyDiagnosticAndFixAsync(
            string source,
            string expected,
            IEnumerable<(string source, string expectedSource)> additionalFiles = null,
            string equivalenceKey = null,
            TestOptions options = null,
            CancellationToken cancellationToken = default)
        {
            var code = TestCode.Parse(source);

            Debug.Assert(code.Spans.Length > 0);

            (string expectedValue, ImmutableDictionary<string, ImmutableArray<TextSpan>> expectedSpans) = TextProcessor.FindAnnotatedSpansAndRemove(expected);

            var state = new DiagnosticTestState(
                code.Value,
                expectedValue,
                Descriptor,
                code.Spans,
                additionalSpans: code.AdditionalSpans,
                additionalFiles: AdditionalFile.CreateRange(additionalFiles),
                expectedSpans: expectedSpans,
                equivalenceKey: equivalenceKey);

            await VerifyDiagnosticAndFixAsync(state, options, cancellationToken);
        }

        public async Task VerifyDiagnosticAndNoFixAsync(
            string source,
            IEnumerable<(string source, string expectedSource)> additionalFiles = null,
            string equivalenceKey = null,
            TestOptions options = null,
            CancellationToken cancellationToken = default)
        {
            var code = TestCode.Parse(source);

            Debug.Assert(code.Spans.Length > 0);

            var state = new DiagnosticTestState(
                code.Value,
                expectedSource: null,
                Descriptor,
                code.Spans,
                additionalSpans: code.AdditionalSpans,
                additionalFiles: AdditionalFile.CreateRange(additionalFiles),
                equivalenceKey: equivalenceKey);

            await VerifyDiagnosticAndNoFixAsync(state, options, cancellationToken);
        }

        public async Task VerifyDiagnosticAndFixAsync(
            string source,
            string sourceData,
            string expectedData,
            IEnumerable<(string source, string expectedSource)> additionalFiles = null,
            string equivalenceKey = null,
            TestOptions options = null,
            CancellationToken cancellationToken = default)
        {
            var code = TestCode.Parse(source, sourceData, expectedData);

            Debug.Assert(code.Spans.Length > 0);

            (string expectedValue, ImmutableDictionary<string, ImmutableArray<TextSpan>> expectedSpans) = TextProcessor.FindAnnotatedSpansAndRemove(code.ExpectedValue);

            var state = new DiagnosticTestState(
                code.Value,
                expectedValue,
                Descriptor,
                code.Spans,
                code.AdditionalSpans,
                AdditionalFile.CreateRange(additionalFiles),
                null,
                null,
                expectedSpans: expectedSpans,
                null,
                equivalenceKey: equivalenceKey);

            await VerifyDiagnosticAndFixAsync(state, options, cancellationToken);
        }
    }
}
