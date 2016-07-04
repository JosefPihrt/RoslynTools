﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pihrtsoft.CodeAnalysis.CSharp;
using Pihrtsoft.CodeAnalysis.Metadata;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Pihrtsoft.CodeAnalysis.CSharp.CSharpFactory;

namespace CodeGenerator
{
    public class OptionsPagePropertiesGenerator : Generator
    {
        public OptionsPagePropertiesGenerator()
        {
            DefaultNamespace = "Pihrtsoft.CodeAnalysis.VisualStudio";
        }

        public CompilationUnitSyntax Generate(IEnumerable<RefactoringInfo> refactorings)
        {
            return CompilationUnit()
                .WithUsings(
                    UsingDirective(ParseName("System.ComponentModel")),
                    UsingDirective(ParseName("Pihrtsoft.CodeAnalysis.CSharp.Refactoring")),
                    UsingDirective(ParseName("Pihrtsoft.CodeAnalysis.VisualStudio.TypeConverters"))
                    )
                .WithMember(
                    NamespaceDeclaration(DefaultNamespace)
                        .WithMember(
                            ClassDeclaration("RefactoringsOptionsPage")
                                .WithModifiers(
                                        SyntaxKind.PublicKeyword,
                                        SyntaxKind.PartialKeyword)
                                .WithMembers(
                                    CreateMembers(refactorings))));
        }

        private IEnumerable<MemberDeclarationSyntax> CreateMembers(IEnumerable<RefactoringInfo> refactorings)
        {
            yield return ConstructorDeclaration("RefactoringsOptionsPage")
                .WithModifiers(SyntaxKind.PublicKeyword)
                .WithBody(refactorings.Select(refactoring =>
                    {
                        return ExpressionStatement(
                            SimpleAssignmentExpression(
                                IdentifierName(refactoring.Identifier),
                                (refactoring.IsEnabledByDefault) ? TrueLiteralExpression() : FalseLiteralExpression()));
                    }));

            yield return MethodDeclaration(VoidType(), "Apply")
                .WithModifiers(SyntaxKind.PublicKeyword)
                .WithStatements(refactorings.Select(refactoring =>
                    {
                        return ExpressionStatement(
                            InvocationExpression("SetIsEnabled")
                                .WithArguments(
                                    Argument(SimpleMemberAccessExpression("RefactoringIdentifiers", refactoring.Identifier)),
                                    Argument(refactoring.Identifier)));
                    }));

            foreach (RefactoringInfo info in refactorings)
                yield return CreateRefactoringProperty(info);
        }

        private PropertyDeclarationSyntax CreateRefactoringProperty(RefactoringInfo refactoring)
        {
            return PropertyDeclaration(BoolType(), refactoring.Identifier)
                .WithAttributeLists(
                    AttributeList(Attribute("Category").WithArgument(IdentifierName("RefactoringCategory"))),
                    AttributeList(Attribute("DisplayName").WithArgument(StringLiteralExpression(refactoring.Title))),
                    AttributeList(Attribute("Description").WithArgument(StringLiteralExpression(CreateDescription(refactoring)))),
                    AttributeList(Attribute("TypeConverter").WithArgument(TypeOfExpression(IdentifierName("EnabledDisabledConverter"))))
                    )
                .WithModifiers(SyntaxKind.PublicKeyword)
                .WithAccessorList(
                    AutoGetter(),
                    AutoSetter());
        }

        private static string CreateDescription(RefactoringInfo refactoring)
        {
            string s = "";

            if (refactoring.Syntaxes.Count > 0)
                s = "Syntax: " + string.Join(", ", refactoring.Syntaxes.Select(f => f.Name));

            if (!string.IsNullOrEmpty(refactoring.Scope))
            {
                if (!string.IsNullOrEmpty(s))
                    s += "\r\n";

                s += "Scope: " + refactoring.Scope;
            }

            return s;
        }
    }
}
