// <copyright file="JsonTriviaEditing.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace JsonPlus;

using System.Collections.Generic;

/// <summary>
/// Keeps commas and layout trivia consistent when an element is added to, inserted into or removed
/// from an array or an object.
/// </summary>
/// <remarks>
/// The parser stores the separator between two elements in the trailing trivia of the first one,
/// after its closing bracket, its comma and its comments (", " or ",\n"). The indentation of an
/// element, and the comments on the lines above it, are in its leading trivia. The separator before
/// the first element is in the leading trivia of the container, after the opening bracket.
/// </remarks>
internal static class JsonTriviaEditing
{
    private static bool IsComment(JsonTriviaKind kind) =>
        kind is JsonTriviaKind.SingleLineComment or JsonTriviaKind.MultiLineComment;

    private static bool IsLayout(JsonTriviaKind kind) =>
        kind is JsonTriviaKind.Whitespace or JsonTriviaKind.NewLine || IsComment(kind);

    private static bool IsClosing(JsonTriviaKind kind) =>
        kind is JsonTriviaKind.ArrayEnd or JsonTriviaKind.ObjectEnd;

    /// <summary>
    /// Copies the indentation of <paramref name="source"/> to <paramref name="target"/>, without the
    /// comments, if <paramref name="target"/> has no layout trivia of its own.
    /// </summary>
    public static void CopyIndentation(JsonTriviaCollection source, JsonTriviaCollection target)
    {
        if (target.Count > 0 && IsLayout(target[0].Kind))
        {
            return;
        }

        var end = source.FindIndex(t => !IsLayout(t.Kind));
        if (end < 0)
        {
            end = source.Count;
        }

        var start = end;
        while (start > 0 && source[start - 1].Kind == JsonTriviaKind.Whitespace)
        {
            start--;
        }

        target.InsertRange(0, source.GetRange(start, end - start));
    }

    /// <summary>
    /// Copies the trivia between a key and its value (the colon and the whitespace), without comments.
    /// </summary>
    public static void CopyKeySeparator(JsonTriviaCollection source, JsonTriviaCollection target)
    {
        if (target.Count == 0)
        {
            target.AddRange(source.FindAll(t => !IsComment(t.Kind)));
        }
    }

    /// <summary>
    /// Prepares the trivia of a new element that is appended after the last element.
    /// </summary>
    /// <param name="separatorSource">Trailing trivia of the element before the last one, or leading trivia of the container if the last element is the only one.</param>
    /// <param name="lastLeading">Leading trivia of the last element.</param>
    /// <param name="lastTrailing">Trailing trivia of the last element.</param>
    /// <param name="newLeading">Leading trivia of the new element.</param>
    /// <param name="newTrailing">Trailing trivia of the new element.</param>
    public static void PrepareAppend(
        JsonTriviaCollection separatorSource,
        JsonTriviaCollection lastLeading,
        JsonTriviaCollection lastTrailing,
        JsonTriviaCollection newLeading,
        JsonTriviaCollection newTrailing)
    {
        CopyIndentation(lastLeading, newLeading);

        // the trivia before the closing bracket of the container moves to the new last element
        var tail = RemoveSeparator(lastTrailing);
        AddComma(lastTrailing);
        lastTrailing.AddRange(GetSeparator(separatorSource));
        newTrailing.AddRange(tail);
    }

    /// <summary>
    /// Prepares the trivia of a new element that is inserted before an existing element.
    /// </summary>
    /// <param name="separatorSource">Trailing trivia of the element before the insertion point, or leading trivia of the container at index 0.</param>
    /// <param name="nextLeading">Leading trivia of the element after the insertion point.</param>
    /// <param name="newLeading">Leading trivia of the new element.</param>
    /// <param name="newTrailing">Trailing trivia of the new element.</param>
    public static void PrepareInsert(
        JsonTriviaCollection separatorSource,
        JsonTriviaCollection nextLeading,
        JsonTriviaCollection newLeading,
        JsonTriviaCollection newTrailing)
    {
        CopyIndentation(nextLeading, newLeading);
        AddComma(newTrailing);
        if (GetSeparator(newTrailing).Count == 0)
        {
            newTrailing.AddRange(GetSeparator(separatorSource));
        }
    }

    /// <summary>
    /// Updates the trivia of the element before the last one, when the last element is removed.
    /// </summary>
    /// <param name="previousTrailing">Trailing trivia of the element that becomes the last one.</param>
    /// <param name="removedTrailing">Trailing trivia of the removed element.</param>
    public static void PrepareRemoveLast(JsonTriviaCollection previousTrailing, JsonTriviaCollection removedTrailing)
    {
        // keep a trailing comma only if the document already used one
        if (!removedTrailing.Exists(t => t.Kind == JsonTriviaKind.Comma))
        {
            var commaIndex = previousTrailing.FindIndex(t => t.Kind == JsonTriviaKind.Comma);
            if (commaIndex >= 0)
            {
                previousTrailing.RemoveAt(commaIndex);
            }
        }

        RemoveSeparator(previousTrailing);
        previousTrailing.AddRange(GetSeparator(removedTrailing));
    }

    private static void AddComma(JsonTriviaCollection trailing)
    {
        if (!trailing.Exists(t => t.Kind == JsonTriviaKind.Comma))
        {
            // after the closing bracket of an array or object value, before any comment
            trailing.Insert(trailing.FindLastIndex(t => IsClosing(t.Kind)) + 1, new JsonTrivia(JsonTriviaKind.Comma, ","));
        }
    }

    private static int GetSeparatorStart(JsonTriviaCollection trivia)
    {
        var start = 0;
        for (var i = 0; i < trivia.Count; i++)
        {
            if (!IsLayout(trivia[i].Kind) || IsComment(trivia[i].Kind))
            {
                start = i + 1;
            }
        }

        return start;
    }

    private static List<JsonTrivia> GetSeparator(JsonTriviaCollection trivia)
    {
        var start = GetSeparatorStart(trivia);
        return trivia.GetRange(start, trivia.Count - start);
    }

    private static List<JsonTrivia> RemoveSeparator(JsonTriviaCollection trivia)
    {
        var start = GetSeparatorStart(trivia);
        var separator = trivia.GetRange(start, trivia.Count - start);
        trivia.RemoveRange(start, trivia.Count - start);
        return separator;
    }
}
