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

namespace Roslynator.Testing.CSharp
{
    public abstract class AbstractCSharpDiagnosticVerifier<TAnalyzer, TFixProvider> : XunitDiagnosticVerifier<TAnalyzer, TFixProvider>
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TFixProvider : CodeFixProvider, new()
    {
        /// <summary>
        /// Gets a <see cref="DiagnosticDescriptor"/> that describes diagnostic that should be verified.
        /// </summary>
        public abstract DiagnosticDescriptor Descriptor { get; }

        public override CSharpTestOptions Options => DefaultCSharpTestOptions.Value;

        /// <summary>
        /// Verifies that specified source will produce diagnostic described with see <see cref="Descriptor"/>
        /// </summary>
        /// <param name="source">A source code that should be tested. Tokens <c>[|</c> and <c>|]</c> represents start and end of selection respectively.</param>
        /// <param name="additionalFiles"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
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
                null,
                Descriptor,
                code.Spans,
                AdditionalFile.CreateRange(additionalFiles),
                null,
                null,
                null,
                null);

            await VerifyDiagnosticAsync(
                state,
                options: options,
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Verifies that specified source will produce diagnostic described with see <see cref="Descriptor"/>
        /// </summary>
        /// <param name="source">Source text that contains placeholder <c>[||]</c> to be replaced with <paramref name="sourceData"/>.</param>
        /// <param name="sourceData"></param>
        /// <param name="additionalFiles"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
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
                null,
                Descriptor,
                code.Spans,
                AdditionalFile.CreateRange(additionalFiles),
                null,
                null,
                null,
                null);

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
                null,
                Descriptor,
                ImmutableArray.Create(span),
                AdditionalFile.CreateRange(additionalFiles),
                null,
                null,
                null,
                null);

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
                null,
                Descriptor,
                spans,
                AdditionalFile.CreateRange(additionalFiles),
                null,
                null,
                null,
                null);

            await VerifyDiagnosticAsync(
                state,
                options: options,
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Verifies that specified source will not produce diagnostic described with see <see cref="Descriptor"/>
        /// </summary>
        /// <param name="source">Source text that contains placeholder <c>[||]</c> to be replaced with <paramref name="sourceData"/>.</param>
        /// <param name="sourceData"></param>
        /// <param name="additionalFiles"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
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
                AdditionalFile.CreateRange(additionalFiles),
                null,
                null,
                null,
                null);

            await VerifyNoDiagnosticAsync(
                state,
                options: options,
                cancellationToken);
        }

        /// <summary>
        /// Verifies that specified source will not produce diagnostic described with see <see cref="Descriptor"/>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="additionalFiles"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyNoDiagnosticAsync(
            string source,
            IEnumerable<string> additionalFiles = null,
            TestOptions options = null,
            CancellationToken cancellationToken = default)
        {
            var state = new DiagnosticTestState(
                source,
                null,
                Descriptor,
                null,
                AdditionalFile.CreateRange(additionalFiles),
                null,
                null,
                null,
                null);

            await VerifyNoDiagnosticAsync(
                state,
                options: options,
                cancellationToken);
        }

        /// <summary>
        /// Verifies that specified source will produce diagnostic and that the diagnostic will be fixed with the <see cref="FixProvider"/>.
        /// </summary>
        /// <param name="source">A source code that should be tested. Tokens <c>[|</c> and <c>|]</c> represents start and end of selection respectively.</param>
        /// <param name="expected"></param>
        /// <param name="additionalFiles"></param>
        /// <param name="equivalenceKey">Code action's equivalence key.</param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
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

            var state = new DiagnosticTestState(
                code.Value,
                expected,
                Descriptor,
                code.Spans,
                AdditionalFile.CreateRange(additionalFiles),
                null,
                null,
                null,
                equivalenceKey: equivalenceKey);

            await VerifyDiagnosticAndFixAsync(state, options, cancellationToken);
        }

        /// <summary>
        /// Verifies that specified source will produce diagnostic and that the diagnostic will not be fixed with the <see cref="FixProvider"/>.
        /// </summary>
        /// <param name="source">A source code that should be tested. Tokens <c>[|</c> and <c>|]</c> represents start and end of selection respectively.</param>
        /// <param name="additionalFiles"></param>
        /// <param name="equivalenceKey">Code action's equivalence key.</param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
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
                null,
                Descriptor,
                code.Spans,
                AdditionalFile.CreateRange(additionalFiles),
                null,
                null,
                null,
                equivalenceKey: equivalenceKey);

            await VerifyDiagnosticAndNoFixAsync(state, options, cancellationToken);
        }

        /// <summary>
        /// Verifies that specified source will produce diagnostic and that the diagnostic will be fixed with the <see cref="FixProvider"/>.
        /// </summary>
        /// <param name="source">Source text that contains placeholder <c>[||]</c> to be replaced with <paramref name="sourceData"/> and <paramref name="expectedData"/>.</param>
        /// <param name="sourceData"></param>
        /// <param name="expectedData"></param>
        /// <param name="additionalFiles"></param>
        /// <param name="equivalenceKey">Code action's equivalence key.</param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
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

            var state = new DiagnosticTestState(
                code.Value,
                code.ExpectedValue,
                Descriptor,
                code.Spans,
                AdditionalFile.CreateRange(additionalFiles),
                null,
                null,
                null,
                equivalenceKey: equivalenceKey);

            await VerifyDiagnosticAndFixAsync(state, options, cancellationToken);
        }

        //TODO: del
        //internal async Task VerifyFixAsync(
        //    string source,
        //    string sourceData,
        //    string expectedData,
        //    IEnumerable<(string source, string expectedSource)> additionalFiles = null,
        //    string equivalenceKey = null,
        //    TestOptions options = null,
        //    CancellationToken cancellationToken = default)
        //{
        //    var code = TestCode.Parse(source, sourceData, expectedData);

        //    Debug.Assert(code.Spans.Length > 0);

        //    var state = new DiagnosticTestState(
        //        code.Value,
        //        code.ExpectedValue,
        //        Descriptor,
        //        code.Spans,
        //        AdditionalFile.CreateRange(additionalFiles),
        //        null,
        //        null,
        //        null,
        //        equivalenceKey);

        //    await VerifyFixAsync(
        //        state,
        //        options: options,
        //        cancellationToken: cancellationToken);
        //}

        //TODO: del
        //internal async Task VerifyFixAsync(
        //    string source,
        //    string expected,
        //    IEnumerable<(string source, string expectedSource)> additionalFiles = null,
        //    string equivalenceKey = null,
        //    TestOptions options = null,
        //    CancellationToken cancellationToken = default)
        //{
        //    var code = TestCode.Parse(source);

        //    Debug.Assert(code.Spans.Length > 0);

        //    var state = new DiagnosticTestState(
        //        code.Value,
        //        expected,
        //        Descriptor,
        //        code.Spans,
        //        AdditionalFile.CreateRange(additionalFiles),
        //        null,
        //        null,
        //        null,
        //        equivalenceKey);

        //    await VerifyFixAsync(
        //        state: state,
        //        options: options,
        //        cancellationToken: cancellationToken);
        //}

        //TODO: 
        //public async Task VerifyNoFixAsync(
        //    string source,
        //    IEnumerable<string> additionalFiles = null,
        //    string equivalenceKey = null,
        //    TestOptions options = null,
        //    CancellationToken cancellationToken = default)
        //{
        //    var code = TestCode.Parse(source);

        //    Debug.Assert(code.Spans.Length > 0);

        //    var state = new DiagnosticTestState(
        //        code.Value,
        //        code.ExpectedValue,
        //        Descriptor,
        //        code.Spans,
        //        AdditionalFile.CreateRange(additionalFiles),
        //        null,
        //        null,
        //        null,
        //        equivalenceKey);

        //    await VerifyNoFixAsync(
        //        state,
        //        options: options,
        //        cancellationToken: cancellationToken);
        //}
    }
}
