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
    }
}
