// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Roslynator
{
    //TODO: del
    internal static class CompilationOptionsExtensions
    {
        //public static CompilationOptions EnsureEnabled(this CompilationOptions compilationOptions, DiagnosticDescriptor descriptor)
        //{
        //    return SetSeverity(compilationOptions, descriptor, descriptor.DefaultSeverity.ToReportDiagnostic());
        //}

        //public static CompilationOptions EnsureSuppressed(this CompilationOptions compilationOptions, DiagnosticDescriptor descriptor)
        //{
        //    return SetSeverity(compilationOptions, descriptor, ReportDiagnostic.Suppress);
        //}

        //private static CompilationOptions SetSeverity(this CompilationOptions compilationOptions, DiagnosticDescriptor descriptor, ReportDiagnostic reportDiagnostic)
        //{
        //    ImmutableDictionary<string, ReportDiagnostic> specificDiagnosticOptions = compilationOptions.SpecificDiagnosticOptions;

        //    specificDiagnosticOptions = specificDiagnosticOptions.SetItem(
        //        descriptor.Id,
        //        reportDiagnostic);

        //    return compilationOptions.WithSpecificDiagnosticOptions(specificDiagnosticOptions);
        //}
    }
}
