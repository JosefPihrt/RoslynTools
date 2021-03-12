// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;

namespace Roslynator
{
    internal static class CommonWorkspaceExtensions
    {
        public static bool IsEnabled(
            this AnalyzerOptionDescriptor analyzerOption,
            Document document,
            SyntaxNode node)
        {
            return IsEnabled(analyzerOption, document, node.SyntaxTree);
        }

        public static bool IsEnabled(
            this AnalyzerOptionDescriptor analyzerOption,
            Document document,
            SyntaxToken token)
        {
            return IsEnabled(analyzerOption, document, token.SyntaxTree);
        }

        public static bool IsEnabled(
            this AnalyzerOptionDescriptor analyzerOption,
            Document document,
            SyntaxTree syntaxTree)
        {
            return analyzerOption.Parent.IsEffective(syntaxTree, document.Project.CompilationOptions)
                && analyzerOption.IsEnabled(
                    syntaxTree,
                    document.Project.CompilationOptions,
                    document.Project.AnalyzerOptions);
        }
    }
}
