// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;

namespace Roslynator
{
    internal static class TestExtensions
    {
        public static async Task<SyntaxNode> GetSyntaxRootAsync(
            this Document document,
            bool simplify,
            bool format,
            CancellationToken cancellationToken = default)
        {
            if (simplify)
                document = await Simplifier.ReduceAsync(document, Simplifier.Annotation, cancellationToken: cancellationToken);

            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken);

            if (format)
                root = Formatter.Format(root, Formatter.Annotation, document.Project.Solution.Workspace);

            return root;
        }
    }
}
