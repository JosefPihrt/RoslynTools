﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Roslynator.Text;

namespace Roslynator.Testing.Text
{
    public static class TextProcessor
    {
        public static TextAndSpans FindSpansAndRemove(string text, IComparer<LinePositionSpanInfo> comparer = null)
        {
            StringBuilder sb = StringBuilderCache.GetInstance(text.Length);

            var startPending = false;
            LinePositionInfo start = default;
            Stack<LinePositionInfo> stack = null;
            List<LinePositionSpanInfo> spans = null;

            int lastPos = 0;

            int line = 0;
            int column = 0;

            int length = text.Length;

            int i = 0;
            while (i < length)
            {
                switch (text[i])
                {
                    case '\r':
                        {
                            if (PeekNextChar() == '\n')
                            {
                                i++;
                            }

                            line++;
                            column = 0;
                            i++;
                            continue;
                        }
                    case '\n':
                        {
                            line++;
                            column = 0;
                            i++;
                            continue;
                        }
                    case '[':
                        {
                            char nextChar = PeekNextChar();
                            if (nextChar == '|')
                            {
                                sb.Append(text, lastPos, i - lastPos);

                                var start2 = new LinePositionInfo(sb.Length, line, column);

                                if (stack != null)
                                {
                                    stack.Push(start2);
                                }
                                else if (!startPending)
                                {
                                    start = start2;
                                    startPending = true;
                                }
                                else
                                {
                                    stack = new Stack<LinePositionInfo>();
                                    stack.Push(start);
                                    stack.Push(start2);
                                    startPending = false;
                                }

                                i += 2;
                                lastPos = i;
                                continue;
                            }
                            else if (nextChar == '['
                                && PeekChar(2) == '|'
                                && PeekChar(3) == ']')
                            {
                                i++;
                                column++;
                                CloseSpan();
                                i += 3;
                                lastPos = i;
                                continue;
                            }

                            break;
                        }
                    case '|':
                        {
                            if (PeekNextChar() == ']')
                            {
                                CloseSpan();
                                i += 2;
                                lastPos = i;
                                continue;
                            }

                            break;
                        }
                }

                column++;
                i++;
            }

            if (startPending
                || stack?.Count > 0)
            {
                throw new InvalidOperationException("Text span is invalid.");
            }

            sb.Append(text, lastPos, text.Length - lastPos);

            spans?.Sort(comparer ?? LinePositionSpanInfoComparer.Index);

            return new TextAndSpans(
                StringBuilderCache.GetStringAndFree(sb),
                spans?.ToImmutableArray() ?? ImmutableArray<LinePositionSpanInfo>.Empty);

            char PeekNextChar()
            {
                return PeekChar(1);
            }

            char PeekChar(int offset)
            {
                return (i + offset >= text.Length) ? '\0' : text[i + offset];
            }

            void CloseSpan()
            {
                if (stack != null)
                {
                    start = stack.Pop();
                }
                else if (startPending)
                {
                    startPending = false;
                }
                else
                {
                    throw new InvalidOperationException("Text span is invalid.");
                }

                var end = new LinePositionInfo(sb.Length + i - lastPos, line, column);

                var span = new LinePositionSpanInfo(start, end);

                (spans ??= new List<LinePositionSpanInfo>()).Add(span);

                sb.Append(text, lastPos, i - lastPos);
            }
        }

        public static TextAndSpans FindSpansAndReplace(
            string source,
            string sourceData,
            IComparer<LinePositionSpanInfo> comparer = null)
        {
            return FindSpansAndReplace(source, sourceData, expectedData: null, comparer);
        }

        public static TextAndSpans FindSpansAndReplace(
            string source,
            string sourceData,
            string expectedData,
            IComparer<LinePositionSpanInfo> comparer = null)
        {
            TextAndSpans result = FindSpansAndRemove(source);

            if (result.Spans.Length == 0)
                throw new InvalidOperationException("Text contains no span.");

            if (result.Spans.Length > 1)
                throw new InvalidOperationException("Text contains more than one span.");

            string expected2 = (expectedData != null)
                ? result.Text.Remove(result.Spans[0].Start.Index) + expectedData + result.Text.Substring(result.Spans[0].End.Index)
                : null;

            string source2 = sourceData;

            TextAndSpans result2 = FindSpansAndRemove(sourceData);

            if (result2.Spans.Length == 0)
                source2 = "[|" + sourceData + "|]";

            source2 = result.Text.Remove(result.Spans[0].Start.Index) + source2 + result.Text.Substring(result.Spans[0].End.Index);

            result = FindSpansAndRemove(source2, comparer);

            return new TextAndSpans(result.Text, expected2, result.Spans);
        }
    }
}
