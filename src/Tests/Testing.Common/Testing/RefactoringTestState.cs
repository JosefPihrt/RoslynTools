// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Roslynator.Testing
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public sealed class RefactoringTestState
    {
        public RefactoringTestState(
            string source,
            IEnumerable<TextSpan> spans,
            IEnumerable<AdditionalFile> additionalFiles = null,
            string equivalenceKey = null)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Spans = spans?.ToImmutableArray() ?? ImmutableArray<TextSpan>.Empty;
            AdditionalFiles = additionalFiles?.ToImmutableArray() ?? ImmutableArray<AdditionalFile>.Empty;
            EquivalenceKey = equivalenceKey;
        }

        public string Source { get; }

        public ImmutableArray<TextSpan> Spans { get; private set; }

        public ImmutableArray<AdditionalFile> AdditionalFiles { get; }

        public string EquivalenceKey { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => Source;

        internal RefactoringTestState(RefactoringTestState other)
            : this(
                source: other.Source,
                spans: other.Spans,
                additionalFiles: other.AdditionalFiles,
                equivalenceKey: other.EquivalenceKey)
        {
        }

        public RefactoringTestState Update(
            string source,
            IEnumerable<TextSpan> spans,
            IEnumerable<AdditionalFile> additionalFiles,
            string equivalenceKey)
        {
            return new RefactoringTestState(
                source: source,
                spans: spans,
                additionalFiles: additionalFiles,
                equivalenceKey: equivalenceKey);
        }

        //protected override TestState CommonWithSource(string source)
        //{
        //    return WithSource(source);
        //}

        //protected override TestState CommonWithExpectedSource(string expectedSource)
        //{
        //    return WithExpectedSource(expectedSource);
        //}

        //protected override TestState CommonWithAdditionalFiles(IEnumerable<AdditionalFile> additionalFiles)
        //{
        //    return WithAdditionalFiles(additionalFiles);
        //}

        //protected override TestState CommonWithCodeActionTitle(string codeActionTitle)
        //{
        //    return WithCodeActionTitle(codeActionTitle);
        //}

        //protected override TestState CommonWithEquivalenceKey(string equivalenceKey)
        //{
        //    return WithEquivalenceKey(equivalenceKey);
        //}

        //new public RefactoringTestState WithSource(string source)
        //{
        //    return new RefactoringTestState(this) { Source = source ?? throw new ArgumentNullException(nameof(source)) };
        //}

        //new public RefactoringTestState WithExpectedSource(string expectedSource)
        //{
        //    return new RefactoringTestState(this) { ExpectedSource = expectedSource };
        //}

        //public RefactoringTestState WithSpans(IEnumerable<TextSpan> spans)
        //{
        //    return new RefactoringTestState(this) { Spans = spans?.ToImmutableArray() ?? ImmutableArray<TextSpan>.Empty };
        //}

        //new public RefactoringTestState WithAdditionalFiles(IEnumerable<AdditionalFile> additionalFiles)
        //{
        //    return new RefactoringTestState(this) { AdditionalFiles = additionalFiles?.ToImmutableArray() ?? ImmutableArray<AdditionalFile>.Empty };
        //}

        //new public RefactoringTestState WithCodeActionTitle(string codeActionTitle)
        //{
        //    return new RefactoringTestState(this) { CodeActionTitle = codeActionTitle };
        //}

        //new public RefactoringTestState WithEquivalenceKey(string equivalenceKey)
        //{
        //    return new RefactoringTestState(this) { EquivalenceKey = equivalenceKey };
        //}
    }
}
