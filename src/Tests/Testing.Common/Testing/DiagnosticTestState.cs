// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

#pragma warning disable RCS1223

namespace Roslynator.Testing
{
    //TODO: Diagnostic.Properties, AdditionalLocations
    public sealed class DiagnosticTestState : TestState
    {
        public DiagnosticTestState(
            string source,
            string expectedSource,
            DiagnosticDescriptor descriptor,
            IEnumerable<TextSpan> spans,
            IEnumerable<AdditionalFile> additionalFiles = null)
            : this(source, expectedSource, descriptor, spans, additionalFiles, null, null, null, null)
        {
        }

        public DiagnosticTestState(
            string source,
            string expectedSource,
            DiagnosticDescriptor descriptor,
            IEnumerable<TextSpan> spans,
            IEnumerable<AdditionalFile> additionalFiles,
            string diagnosticMessage,
            IFormatProvider formatProvider,
            string codeActionTitle,
            string equivalenceKey) : base(source, expectedSource, additionalFiles, codeActionTitle, equivalenceKey)
        {
            Descriptor = descriptor;
            Spans = spans?.ToImmutableArray() ?? ImmutableArray<TextSpan>.Empty;
            DiagnosticMessage = diagnosticMessage;
            FormatProvider = formatProvider;
        }

        private DiagnosticTestState(DiagnosticTestState other)
            : this(
                source: other.Source,
                expectedSource: other.ExpectedSource,
                descriptor: other.Descriptor,
                spans: other.Spans,
                additionalFiles: other.AdditionalFiles,
                diagnosticMessage: other.DiagnosticMessage,
                formatProvider: other.FormatProvider,
                codeActionTitle: other.CodeActionTitle,
                equivalenceKey: other.EquivalenceKey)
        {
        }

        public DiagnosticDescriptor Descriptor { get; private set; }

        public ImmutableArray<TextSpan> Spans { get; private set; }

        public string DiagnosticMessage { get; private set; }

        public IFormatProvider FormatProvider { get; private set; }

        internal ImmutableArray<Diagnostic> GetDiagnostics(SyntaxTree tree)
        {
            return ImmutableArray.CreateRange(
                Spans,
                span => Diagnostic.Create(
                    Descriptor,
                    Location.Create(tree, span)));
        }

        public DiagnosticTestState Update(
            string source,
            string expectedSource,
            DiagnosticDescriptor descriptor,
            IEnumerable<TextSpan> spans,
            IEnumerable<AdditionalFile> additionalFiles,
            string diagnosticMessage,
            IFormatProvider formatProvider,
            string codeActionTitle,
            string equivalenceKey)
        {
            return new DiagnosticTestState(
                source: source,
                expectedSource: expectedSource,
                descriptor: descriptor,
                spans: spans,
                additionalFiles: additionalFiles,
                diagnosticMessage: diagnosticMessage,
                formatProvider: formatProvider,
                codeActionTitle: codeActionTitle,
                equivalenceKey: equivalenceKey);
        }

        internal DiagnosticTestState MaybeUpdate(
            string source = null,
            string expectedSource = null,
            DiagnosticDescriptor descriptor = null,
            IEnumerable<TextSpan> spans = null,
            IEnumerable<AdditionalFile> additionalFiles = null,
            string diagnosticMessage = null,
            IFormatProvider formatProvider = null,
            string codeActionTitle = null,
            string equivalenceKey = null)
        {
            return new DiagnosticTestState(
                source: source ?? Source,
                expectedSource: expectedSource ?? ExpectedSource,
                descriptor: descriptor ?? Descriptor,
                spans: spans ?? Spans,
                additionalFiles: additionalFiles ?? AdditionalFiles,
                diagnosticMessage: diagnosticMessage ?? DiagnosticMessage,
                formatProvider: formatProvider ?? FormatProvider,
                codeActionTitle: codeActionTitle ?? CodeActionTitle,
                equivalenceKey: equivalenceKey ?? EquivalenceKey);
        }

        protected override TestState CommonWithSource(string source)
        {
            return WithSource(source);
        }

        protected override TestState CommonWithExpectedSource(string expectedSource)
        {
            return WithExpectedSource(expectedSource);
        }

        protected override TestState CommonWithAdditionalFiles(IEnumerable<AdditionalFile> additionalFiles)
        {
            return WithAdditionalFiles(additionalFiles);
        }

        protected override TestState CommonWithCodeActionTitle(string codeActionTitle)
        {
            return WithCodeActionTitle(codeActionTitle);
        }

        protected override TestState CommonWithEquivalenceKey(string equivalenceKey)
        {
            return WithEquivalenceKey(equivalenceKey);
        }

        new public DiagnosticTestState WithSource(string source)
        {
            return new DiagnosticTestState(this) { Source = source };
        }

        new public DiagnosticTestState WithExpectedSource(string expectedSource)
        {
            return new DiagnosticTestState(this) { ExpectedSource = expectedSource };
        }

        public DiagnosticTestState WithDescriptor(DiagnosticDescriptor descriptor)
        {
            return new DiagnosticTestState(this) { Descriptor = descriptor };
        }

        public DiagnosticTestState WithSpans(IEnumerable<TextSpan> spans)
        {
            return new DiagnosticTestState(this) { Spans = spans?.ToImmutableArray() ?? ImmutableArray<TextSpan>.Empty };
        }

        new public DiagnosticTestState WithAdditionalFiles(IEnumerable<AdditionalFile> additionalFiles)
        {
            return new DiagnosticTestState(this) { AdditionalFiles = additionalFiles?.ToImmutableArray() ?? ImmutableArray<AdditionalFile>.Empty };
        }

        public DiagnosticTestState WithDiagnosticMessage(string diagnosticMessage)
        {
            return new DiagnosticTestState(this) { DiagnosticMessage = diagnosticMessage };
        }

        public DiagnosticTestState WithFormatProvider(IFormatProvider formatProvider)
        {
            return new DiagnosticTestState(this) { FormatProvider = formatProvider };
        }

        new public DiagnosticTestState WithCodeActionTitle(string codeActionTitle)
        {
            return new DiagnosticTestState(this) { CodeActionTitle = codeActionTitle };
        }

        new public DiagnosticTestState WithEquivalenceKey(string equivalenceKey)
        {
            return new DiagnosticTestState(this) { EquivalenceKey = equivalenceKey };
        }
    }
}
