// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Roslynator.Testing.Text;

namespace Roslynator.Testing
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public sealed class ExpectedTestState
    {
        public ExpectedTestState(
            string source,
            IEnumerable<KeyValuePair<string, ImmutableArray<TextSpan>>> spans = null,
            string title = null,
            IEnumerable<string> alwaysVerifyAnnotations = null)
        {
            Source = source;
            Spans = spans?.ToImmutableDictionary(f => f.Key, f => f.Value) ?? ImmutableDictionary<string, ImmutableArray<TextSpan>>.Empty;
            Title = title;
            AlwaysVerifyAnnotations = alwaysVerifyAnnotations?.ToImmutableArray() ?? ImmutableArray<string>.Empty;
        }

        public string Source { get; }

        public ImmutableDictionary<string, ImmutableArray<TextSpan>> Spans { get; }

        public string Title { get; }

        public ImmutableArray<string> AlwaysVerifyAnnotations { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => Source;

        internal static ExpectedTestState Parse(string text)
        {
            (string source, ImmutableDictionary<string, ImmutableArray<TextSpan>> spans) = TextProcessor.FindAnnotatedSpansAndRemove(text);

            return new ExpectedTestState(source, spans);
        }
    }
}
