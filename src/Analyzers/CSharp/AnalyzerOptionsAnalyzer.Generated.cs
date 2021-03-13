﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

// <auto-generated>

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class AnalyzerOptionsAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(AnalyzerOptionDiagnosticDescriptors.UseImplicitlyTypedArrayWhenTypeIsObvious, AnalyzerOptionDiagnosticDescriptors.UseImplicitlyTypedArray, AnalyzerOptionDiagnosticDescriptors.ConvertExpressionBodyToBlockBodyWhenExpressionIsMultiLine, AnalyzerOptionDiagnosticDescriptors.ConvertExpressionBodyToBlockBodyWhenDeclarationIsMultiLine, AnalyzerOptionDiagnosticDescriptors.ConvertExpressionBodyToBlockBody, AnalyzerOptionDiagnosticDescriptors.RemoveAccessibilityModifiers, AnalyzerOptionDiagnosticDescriptors.RemoveEmptyLineBetweenClosingBraceAndSwitchSection, AnalyzerOptionDiagnosticDescriptors.DoNotRenamePrivateStaticReadOnlyFieldToCamelCaseWithUnderscore, AnalyzerOptionDiagnosticDescriptors.RemoveArgumentListFromObjectCreation, AnalyzerOptionDiagnosticDescriptors.RemoveParenthesesFromConditionOfConditionalExpressionWhenExpressionIsSingleToken, AnalyzerOptionDiagnosticDescriptors.UseStringEmptyInsteadOfEmptyStringLiteral, AnalyzerOptionDiagnosticDescriptors.RemoveCallToConfigureAwait, AnalyzerOptionDiagnosticDescriptors.ConvertBitwiseOperationToHasFlagCall, AnalyzerOptionDiagnosticDescriptors.SimplifyConditionalExpressionWhenItIncludesNegationOfCondition, AnalyzerOptionDiagnosticDescriptors.ConvertMethodGroupToAnonymousFunction, AnalyzerOptionDiagnosticDescriptors.DoNotUseElementAccessWhenExpressionIsInvocation, AnalyzerOptionDiagnosticDescriptors.UseIsNullPatternInsteadOfInequalityOperator, AnalyzerOptionDiagnosticDescriptors.UseComparisonInsteadOfIsNullPattern);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
        }
    }
}