// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;
using Roslynator.Testing.CSharp.Xunit;
using Roslynator.Testing.Text;

namespace Roslynator.Testing.CSharp
{
    public abstract class AbstractCSharpCompilerDiagnosticFixVerifier<TFixProvider> : XunitCompilerDiagnosticFixVerifier<TFixProvider>
        where TFixProvider : CodeFixProvider, new()
    {
        /// <summary>
        /// Gets an ID of a diagnostic to verify.
        /// </summary>
        public abstract string DiagnosticId { get; }

        public override CSharpTestOptions Options => DefaultCSharpTestOptions.Value;

        /// <summary>
        /// Verifies that specified source will produce compiler diagnostic with ID specified in <see cref="DiagnosticId"/>.
        /// </summary>
        /// <param name="source">Source text that contains placeholder <c>[||]</c> to be replaced with <paramref name="sourceData"/> and <paramref name="expectedData"/>.</param>
        /// <param name="sourceData"></param>
        /// <param name="expectedData"></param>
        /// <param name="additionalFiles"></param>
        /// <param name="equivalenceKey">Code action's equivalence key.</param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyFixAsync(
            string source,
            string sourceData,
            string expectedData,
            IEnumerable<(string source, string expectedSource)> additionalFiles = null,
            string equivalenceKey = null,
            TestOptions options = null,
            CancellationToken cancellationToken = default)
        {
            var code = TestCode.Parse(source, sourceData, expectedData);

            Debug.Assert(code.Spans.Length == 0);

            (string expectedValue, ImmutableDictionary<string, ImmutableArray<TextSpan>> expectedSpans) = TextProcessor.FindAnnotatedSpansAndRemove(code.ExpectedValue);

            //TODO: expectedspans
            var state = new CompilerDiagnosticFixTestState(
                DiagnosticId,
                code.Value,
                expectedValue,
                AdditionalFile.CreateRange(additionalFiles),
                expectedSpans: expectedSpans,
                codeActionTitle: null,
                equivalenceKey);

            await VerifyFixAsync(
                state,
                options: options,
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Verifies that specified source will produce compiler diagnostic with ID specified in <see cref="DiagnosticId"/>.
        /// </summary>
        /// <param name="source">A source code that should be tested. Tokens <c>[|</c> and <c>|]</c> represents start and end of selection respectively.</param>
        /// <param name="expected"></param>
        /// <param name="additionalFiles"></param>
        /// <param name="equivalenceKey">Code action's equivalence key.</param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyFixAsync(
            string source,
            string expected,
            IEnumerable<(string source, string expectedSource)> additionalFiles = null,
            string equivalenceKey = null,
            TestOptions options = null,
            CancellationToken cancellationToken = default)
        {
            (string expectedValue, ImmutableDictionary<string, ImmutableArray<TextSpan>> expectedSpans) = TextProcessor.FindAnnotatedSpansAndRemove(expected);

            var state = new CompilerDiagnosticFixTestState(
                DiagnosticId,
                source,
                expectedValue,
                AdditionalFile.CreateRange(additionalFiles),
                expectedSpans: expectedSpans,
                codeActionTitle: null,
                equivalenceKey: equivalenceKey);

            await VerifyFixAsync(
                state,
                options,
                cancellationToken);
        }

        /// <summary>
        /// Verifies that specified source will not produce compiler diagnostic with ID specified in <see cref="DiagnosticId"/>.
        /// </summary>
        /// <param name="source">A source code that should be tested. Tokens <c>[|</c> and <c>|]</c> represents start and end of selection respectively.</param>
        /// <param name="additionalFiles"></param>
        /// <param name="equivalenceKey">Code action's equivalence key.</param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyNoFixAsync(
            string source,
            IEnumerable<string> additionalFiles = null,
            string equivalenceKey = null,
            TestOptions options = null,
            CancellationToken cancellationToken = default)
        {
            var state = new CompilerDiagnosticFixTestState(
                DiagnosticId,
                source,
                expectedSource: null,
                AdditionalFile.CreateRange(additionalFiles),
                expectedSpans: null,
                codeActionTitle: null,
                equivalenceKey: equivalenceKey);

            await VerifyNoFixAsync(
                state,
                options,
                cancellationToken);
        }
    }
}
