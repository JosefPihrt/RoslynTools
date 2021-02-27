// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Roslynator.Testing.Text;

namespace Roslynator.Testing
{
    /// <summary>
    /// Represents a source code to be tested.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public readonly struct TestCode
    {
        internal TestCode(string value, ImmutableArray<TextSpan> spans)
            : this(value, null, spans)
        {
        }

        internal TestCode(string value, string expectedValue, ImmutableArray<TextSpan> spans)
        {
            Value = value;
            ExpectedValue = expectedValue;
            Spans = spans;
        }

        /// <summary>
        /// Gets a source code that should be tested.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Gets a source code after a code fix or a refactoring was applied.
        /// </summary>
        public string ExpectedValue { get; }

        /// <summary>
        /// Gets a collections of spans that represent selected text.
        /// </summary>
        public ImmutableArray<TextSpan> Spans { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => Value;

        /// <summary>
        /// Finds and removes spans that are marked with <c>[|</c> and <c>|]</c> tokens.
        /// </summary>
        /// <param name="value"></param>
        public static TestCode Parse(string value)
        {
            return TextProcessor.FindSpansAndRemove(value);
        }

        /// <summary>
        /// Finds and replace span that is marked with <c>[||]</c> token.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="replacement1"></param>
        /// <param name="replacement2"></param>
        public static TestCode Parse(
            string value,
            string replacement1,
            string replacement2 = null)
        {
            return TextProcessor.FindSpansAndReplace(value, replacement1, replacement2);
        }
    }
}
