// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Roslynator.Testing
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public sealed class CompilerDiagnosticFixTestState
    {
        public CompilerDiagnosticFixTestState(
            string diagnosticId,
            string source,
            IEnumerable<AdditionalFile> additionalFiles = null,
            string equivalenceKey = null)
        {
            DiagnosticId = diagnosticId ?? throw new ArgumentNullException(nameof(diagnosticId));
            Source = source ?? throw new ArgumentNullException(nameof(source));
            AdditionalFiles = additionalFiles?.ToImmutableArray() ?? ImmutableArray<AdditionalFile>.Empty;
            EquivalenceKey = equivalenceKey;
        }

        internal CompilerDiagnosticFixTestState(CompilerDiagnosticFixTestState other)
            : this(
                diagnosticId: other.DiagnosticId,
                source: other.Source,
                additionalFiles: other.AdditionalFiles,
                equivalenceKey: other.EquivalenceKey)
        {
        }

        public string DiagnosticId { get; }

        public string Source { get; }

        public ImmutableArray<AdditionalFile> AdditionalFiles { get; }

        public string EquivalenceKey { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"{DiagnosticId}  {Source}";

        public CompilerDiagnosticFixTestState Update(
            string diagnosticId,
            string source,
            IEnumerable<AdditionalFile> additionalFiles,
            string equivalenceKey)
        {
            return new CompilerDiagnosticFixTestState(
                diagnosticId: diagnosticId,
                source: source,
                additionalFiles: additionalFiles,
                equivalenceKey: equivalenceKey);
        }
    }
}
