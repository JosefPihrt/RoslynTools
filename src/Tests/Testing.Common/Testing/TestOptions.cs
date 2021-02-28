// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Roslynator.Testing
{
    public abstract class TestOptions
    {
        internal TestOptions(
            IEnumerable<MetadataReference> metadataReferences = null,
            IEnumerable<string> allowedCompilerDiagnosticIds = null,
            DiagnosticSeverity allowedCompilerDiagnosticSeverity = DiagnosticSeverity.Info)
        {
            AllowedCompilerDiagnosticSeverity = allowedCompilerDiagnosticSeverity;
            AllowedCompilerDiagnosticIds = allowedCompilerDiagnosticIds?.ToImmutableArray() ?? ImmutableArray<string>.Empty;
            MetadataReferences = metadataReferences?.ToImmutableArray() ?? ImmutableArray<MetadataReference>.Empty;
        }

        public abstract string Language { get; }

        public abstract string DocumentName { get; }

        /// <summary>
        /// Gets a common parse options.
        /// </summary>
        protected abstract ParseOptions CommonParseOptions { get; }

        /// <summary>
        /// Gets a common compilation options.
        /// </summary>
        protected abstract CompilationOptions CommonCompilationOptions { get; }

        /// <summary>
        /// Gets a parse options that should be used to parse tested source code.
        /// </summary>
        public ParseOptions ParseOptions => CommonParseOptions;

        /// <summary>
        /// Gets a compilation options that should be used to compile test project.
        /// </summary>
        public CompilationOptions CompilationOptions => CommonCompilationOptions;

        public DiagnosticSeverity AllowedCompilerDiagnosticSeverity { get; protected set; }

        public ImmutableArray<string> AllowedCompilerDiagnosticIds { get; protected set; }

        public ImmutableArray<MetadataReference> MetadataReferences { get; protected set; }

        protected abstract TestOptions CommonWithAllowedCompilerDiagnosticSeverity(DiagnosticSeverity value);

        protected abstract TestOptions CommonWithAllowedCompilerDiagnosticIds(IEnumerable<string> values);

        protected abstract TestOptions CommonWithMetadataReferences(IEnumerable<MetadataReference> values);

        public TestOptions WithAllowedCompilerDiagnosticSeverity(DiagnosticSeverity value)
        {
            return CommonWithAllowedCompilerDiagnosticSeverity(value);
        }

        public TestOptions WithAllowedCompilerDiagnosticIds(IEnumerable<string> values)
        {
            return CommonWithAllowedCompilerDiagnosticIds(values);
        }

        public TestOptions WithMetadataReferences(IEnumerable<MetadataReference> values)
        {
            return CommonWithMetadataReferences(values);
        }

        internal bool IsAllowedCompilerDiagnostic(Diagnostic diagnostic)
        {
            if (diagnostic.Severity <= AllowedCompilerDiagnosticSeverity)
                return true;

            foreach (string diagnosticId in AllowedCompilerDiagnosticIds)
            {
                if (diagnostic.Id == diagnosticId)
                    return true;
            }

            return false;
        }
    }
}
