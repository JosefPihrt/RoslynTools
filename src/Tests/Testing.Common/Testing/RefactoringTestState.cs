// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Text;

#pragma warning disable RCS1223

namespace Roslynator.Testing
{
    public sealed class RefactoringTestState : TestState
    {
        public RefactoringTestState(
            string source,
            string expectedSource,
            IEnumerable<TextSpan> spans,
            IEnumerable<AdditionalFile> additionalFiles = null)
            : this(source, expectedSource, spans, additionalFiles, null, null, null)
        {
        }

        public RefactoringTestState(
            string source,
            string expectedSource,
            IEnumerable<TextSpan> spans,
            IEnumerable<AdditionalFile> additionalFiles,
            IEnumerable<KeyValuePair<string, ImmutableArray<TextSpan>>> expectedSpans,
            string codeActionTitle,
            string equivalenceKey) : base(source, expectedSource, additionalFiles, expectedSpans, codeActionTitle, equivalenceKey)
        {
            Spans = spans?.ToImmutableArray() ?? ImmutableArray<TextSpan>.Empty;
        }

        public ImmutableArray<TextSpan> Spans { get; private set; }

        private RefactoringTestState(RefactoringTestState other)
            : this(
                source: other.Source,
                expectedSource: other.ExpectedSource,
                spans: other.Spans,
                additionalFiles: other.AdditionalFiles,
                expectedSpans: other.ExpectedSpans,
                codeActionTitle: other.CodeActionTitle,
                equivalenceKey: other.EquivalenceKey)
        {
        }

        public RefactoringTestState Update(
            string source,
            string expectedSource,
            IEnumerable<TextSpan> spans,
            IEnumerable<AdditionalFile> additionalFiles,
            IEnumerable<KeyValuePair<string, ImmutableArray<TextSpan>>> expectedSpans,
            string codeActionTitle,
            string equivalenceKey)
        {
            return new RefactoringTestState(
                source: source,
                expectedSource: expectedSource,
                spans: spans,
                additionalFiles: additionalFiles,
                expectedSpans: expectedSpans,
                codeActionTitle: codeActionTitle,
                equivalenceKey: equivalenceKey);
        }

        internal RefactoringTestState MaybeUpdate(
            string source = null,
            string expectedSource = null,
            IEnumerable<TextSpan> spans = null,
            IEnumerable<AdditionalFile> additionalFiles = null,
            IEnumerable<KeyValuePair<string, ImmutableArray<TextSpan>>> expectedSpans = null,
            string codeActionTitle = null,
            string equivalenceKey = null)
        {
            return new RefactoringTestState(
                source: source ?? Source,
                expectedSource: expectedSource ?? ExpectedSource,
                spans: spans ?? Spans,
                additionalFiles: additionalFiles ?? AdditionalFiles,
                expectedSpans: expectedSpans ?? ExpectedSpans,
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

        new public RefactoringTestState WithSource(string source)
        {
            return new RefactoringTestState(this) { Source = source };
        }

        new public RefactoringTestState WithExpectedSource(string expectedSource)
        {
            return new RefactoringTestState(this) { ExpectedSource = expectedSource };
        }

        public RefactoringTestState WithSpans(IEnumerable<TextSpan> spans)
        {
            return new RefactoringTestState(this) { Spans = spans?.ToImmutableArray() ?? ImmutableArray<TextSpan>.Empty };
        }

        new public RefactoringTestState WithAdditionalFiles(IEnumerable<AdditionalFile> additionalFiles)
        {
            return new RefactoringTestState(this) { AdditionalFiles = additionalFiles?.ToImmutableArray() ?? ImmutableArray<AdditionalFile>.Empty };
        }

        new public RefactoringTestState WithCodeActionTitle(string codeActionTitle)
        {
            return new RefactoringTestState(this) { CodeActionTitle = codeActionTitle };
        }

        new public RefactoringTestState WithEquivalenceKey(string equivalenceKey)
        {
            return new RefactoringTestState(this) { EquivalenceKey = equivalenceKey };
        }
    }
}
