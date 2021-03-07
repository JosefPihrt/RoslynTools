// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Roslynator.Testing
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public sealed class DiagnosticTestState
    {
        public DiagnosticTestState(
            DiagnosticDescriptor descriptor,
            string source,
            IEnumerable<TextSpan> spans,
            IEnumerable<TextSpan> additionalSpans = null,
            IEnumerable<AdditionalFile> additionalFiles = null,
            string diagnosticMessage = null,
            IFormatProvider formatProvider = null,
            string equivalenceKey = null,
            bool alwaysVerifyAdditionalLocations = false)
        {
            Descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Spans = spans?.ToImmutableArray() ?? ImmutableArray<TextSpan>.Empty;
            AdditionalSpans = additionalSpans?.ToImmutableArray() ?? ImmutableArray<TextSpan>.Empty;
            AdditionalFiles = additionalFiles?.ToImmutableArray() ?? ImmutableArray<AdditionalFile>.Empty;
            DiagnosticMessage = diagnosticMessage;
            FormatProvider = formatProvider;
            EquivalenceKey = equivalenceKey;
            AlwaysVerifyAdditionalLocations = alwaysVerifyAdditionalLocations;

            if (Spans.Length > 1
                && !AdditionalSpans.IsEmpty)
            {
                throw new ArgumentException("", nameof(additionalSpans));
            }
        }

        internal DiagnosticTestState(DiagnosticTestState other)
            : this(
                descriptor: other.Descriptor,
                source: other.Source,
                spans: other.Spans,
                additionalSpans: other.AdditionalSpans,
                additionalFiles: other.AdditionalFiles,
                diagnosticMessage: other.DiagnosticMessage,
                formatProvider: other.FormatProvider,
                equivalenceKey: other.EquivalenceKey,
                alwaysVerifyAdditionalLocations: other.AlwaysVerifyAdditionalLocations)
        {
        }

        public DiagnosticDescriptor Descriptor { get; }

        public string Source { get; }

        public ImmutableArray<TextSpan> Spans { get; }

        public ImmutableArray<TextSpan> AdditionalSpans { get; }

        public ImmutableArray<AdditionalFile> AdditionalFiles { get; }

        public string DiagnosticMessage { get; }

        public IFormatProvider FormatProvider { get; }

        public string EquivalenceKey { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"{Descriptor.Id}  {Source}";

        public bool AlwaysVerifyAdditionalLocations { get; }

        internal ImmutableArray<Diagnostic> GetDiagnostics(SyntaxTree tree)
        {
            return ImmutableArray.CreateRange(
                Spans,
                span => Diagnostic.Create(
                    Descriptor,
                    Location.Create(tree, span),
                    additionalLocations: AdditionalSpans.Select(span => Location.Create(tree, span)).ToImmutableArray()));
        }

        public DiagnosticTestState Update(
            DiagnosticDescriptor descriptor,
            string source,
            IEnumerable<TextSpan> spans,
            IEnumerable<TextSpan> additionalSpans,
            IEnumerable<AdditionalFile> additionalFiles,
            string diagnosticMessage,
            IFormatProvider formatProvider,
            string equivalenceKey,
            bool alwaysVerifyAdditionalLocations)
        {
            return new DiagnosticTestState(
                descriptor: descriptor,
                source: source,
                spans: spans,
                additionalSpans: additionalSpans,
                additionalFiles: additionalFiles,
                diagnosticMessage: diagnosticMessage,
                formatProvider: formatProvider,
                equivalenceKey: equivalenceKey,
                alwaysVerifyAdditionalLocations: alwaysVerifyAdditionalLocations);
        }
    }
}
