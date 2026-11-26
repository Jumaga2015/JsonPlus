// <copyright file="JsonValue.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace JsonPlus;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Json value kinds.
/// </summary>
public enum JsonValueKind
{
    JsonNull,
    JsonBoolean,
    JsonNumber,
    JsonString,
    JsonArray,
    JsonObject,
}

/// <summary>
/// Kinds of trivia.
/// </summary>
public enum JsonTriviaKind
{
    Whitespace,
    NewLine,
    SingleLineComment,
    MultiLineComment,
    ArrayStart,
    ArrayEnd,
    ObjectStart,
    ObjectEnd,
    Colon,
    Comma,
}

/// <summary>
/// A piece of trivia (whitespace, comment, punctuation).
/// </summary>
/// <param name="Kind">Kind of trivia.</param>
/// <param name="Value">Contents of trivia.</param>
public record struct JsonTrivia(JsonTriviaKind Kind, string Value);

/// <summary>
/// A collection of trivia elements.
/// </summary>
public class JsonTriviaCollection : List<JsonTrivia>
{
    public JsonTriviaCollection() : base(4)
    {
    }

    public JsonTriviaCollection(IEnumerable<JsonTrivia> collection) : base(collection)
    {
    }
}

public abstract class JsonValue
{
    public JsonTriviaCollection LeadingTrivia { get; set; } = [];

    public JsonTriviaCollection TrailingTrivia { get; set; } = [];

    public virtual JsonValueKind Kind { get; }

    public virtual bool IsPrimitive { get; }

    public virtual string GetRawValue() => throw new InvalidOperationException($"Value is not a string (actual type: {Kind})");

    public virtual string GetString() => throw new InvalidOperationException($"Value is not a string (actual type: {Kind})");

    public virtual double GetDouble() => throw new InvalidOperationException($"Value is not a number (actual type: {Kind})");

    public virtual bool GetBoolean() => throw new InvalidOperationException($"Value is not a boolean (actual type: {Kind})");

    public virtual JsonArray GetArrayValue() => throw new InvalidOperationException($"Value is not an array (actual type: {Kind})");

    public virtual JsonObject GetObjectValue() => throw new InvalidOperationException($"Value is not an object (actual type: {Kind})");

    public virtual JsonString GetStringValue() => throw new InvalidOperationException($"Value is not a string (actual type: {Kind})");

    public virtual JsonNumber GetNumberValue() => throw new InvalidOperationException($"Value is not a number (actual type: {Kind})");

    public virtual JsonBoolean GetBooleanValue() => throw new InvalidOperationException($"Value is not a boolean (actual type: {Kind})");

    public virtual JsonNull GetNullValue() => throw new InvalidOperationException($"Value is not null (actual type: {Kind})");

    public virtual JsonProperty GetProperty(string key) => throw new InvalidOperationException($"Value is not an object (actual type: {Kind})");

    public virtual JsonValue GetPropertyValue(string key) => throw new InvalidOperationException($"Value is not an object (actual type: {Kind})");

    public virtual JsonValue SetProperty(string key, JsonValue value) => throw new InvalidOperationException($"Value is not an object (actual type: {Kind})");

    public virtual JsonValue AddProperty(string key, JsonValue value) => throw new InvalidOperationException($"Value is not an object (actual type: {Kind})");

    public virtual JsonValue InsertProperty(int index, JsonProperty prop) => throw new InvalidOperationException($"Value is not an object (actual type: {Kind})");

    public virtual JsonValue RemoveProperty(string key) => throw new InvalidOperationException($"Value is not an object (actual type: {Kind})");

    public virtual JsonValue GetItem(int index) => throw new InvalidOperationException($"Value is not an array (actual type: {Kind})");

    public virtual JsonValue SetItem(int index, JsonValue newValue) => throw new InvalidOperationException($"Value is not an array (actual type: {Kind})");

    public virtual JsonValue AddItem(JsonValue item) => throw new InvalidOperationException($"Value is not an array (actual type: {Kind})");

    public virtual JsonValue InsertItem(int index, JsonValue newValue) => throw new InvalidOperationException($"Value is not an array (actual type: {Kind})");

    public virtual JsonValue RemoveItem(JsonValue item) => throw new InvalidOperationException($"Value is not an array (actual type: {Kind})");

    public virtual JsonValue RemoveItemAt(int index) => throw new InvalidOperationException($"Value is not an array (actual type: {Kind})");
}

public sealed class JsonProperty
{
    public JsonString Key { get; set; }

    public JsonValue Value { get; set; }

    public JsonProperty(JsonString key, JsonValue value) => (Key, Value) = (key, value);

    public JsonProperty(string key, JsonValue value) => (Key, Value) = (new JsonString(key), value);

    public JsonValue GetValue() => Value;

    public JsonProperty SetValue(JsonValue newValue)
    {
        Value = newValue;
        return this;
    }
}

public sealed class JsonString : JsonValue, IEquatable<string>, IEquatable<JsonString>
{
    public string Value { get; private set; }

    public string RawValue { get; private set; }

    public JsonString(string value, string rawValue) => (Value, RawValue) = (value, rawValue);

    public JsonString(string value) : this(value, JsonCodec.EncodeString(value))
    {
    }

    public override JsonValueKind Kind => JsonValueKind.JsonString;

    public override bool IsPrimitive => true;

    public override string GetString() => Value;

    public override string GetRawValue() => RawValue;

    public override JsonString GetStringValue() => this;

    public override string ToString() => Value;

    public override int GetHashCode() => Value.GetHashCode();

    public override bool Equals(object? obj) => Equals(obj as JsonString);

    public bool Equals(string? other) => Value.Equals(other, StringComparison.Ordinal);

    public bool Equals(JsonString? other) => other is not null && Value.Equals(other.Value, StringComparison.Ordinal);
}

public sealed class JsonNumber : JsonValue, IEquatable<double>, IEquatable<JsonNumber>
{
    private double? value;

    private readonly string rawValue;

    public JsonNumber(string rawValue) => this.rawValue = rawValue;

    public JsonNumber(double val) => (value, rawValue) = (val, val.ToString(System.Globalization.CultureInfo.InvariantCulture));

    public override JsonValueKind Kind => JsonValueKind.JsonNumber;

    public override bool IsPrimitive => true;

    public override string GetRawValue() => rawValue;

    public override double GetDouble() => value ??= double.Parse(rawValue, System.Globalization.CultureInfo.InvariantCulture);

    public override JsonNumber GetNumberValue() => this;

    public override int GetHashCode() => value.GetHashCode();

    public override bool Equals(object? obj) => Equals(obj as JsonString);

    public bool Equals(double other) => value.Equals(other);

    public bool Equals(JsonNumber? other) => other is not null && value.Equals(other.value);
}

public sealed class JsonBoolean : JsonValue, IEquatable<bool>, IEquatable<JsonBoolean>
{
    public JsonBoolean(bool value) => Value = value;

    public bool Value { get; }

    public override JsonValueKind Kind => JsonValueKind.JsonBoolean;

    public override bool IsPrimitive => true;

    public override bool GetBoolean() => Value;

    public override JsonBoolean GetBooleanValue() => this;

    public override int GetHashCode() => Value.GetHashCode();

    public override bool Equals(object? obj) => Equals(obj as JsonString);

    public bool Equals(bool other) => Value.Equals(other);

    public bool Equals(JsonBoolean? other) => other is not null && Value.Equals(other.Value);
}

public sealed class JsonNull : JsonValue, IEquatable<JsonNull>
{
    internal static readonly JsonNull Instance = new();

    public JsonNull()
    {
    }

    public override JsonValueKind Kind => JsonValueKind.JsonNull;

    public override bool IsPrimitive => true;

    public override JsonNull GetNullValue() => this;

    public override int GetHashCode() => 0;

    public override bool Equals(object? obj) => Equals(obj as JsonString);

    public bool Equals(JsonNull? other) => other is not null;
}

public class JsonArray : JsonValue, IList<JsonValue>, IEquatable<JsonArray>
{
    private List<JsonValue> Items { get; } = new List<JsonValue>(8);

    public override JsonValueKind Kind => JsonValueKind.JsonArray;

    public override bool IsPrimitive => false;

    public override JsonArray GetArrayValue() => this;

    public int Count => Items.Count;

    public bool IsReadOnly => false;

    public JsonValue this[int index] { get => Items[index]; set => Items[index] = value; }

    public int IndexOf(JsonValue item) => Items.IndexOf(item);

    public void Insert(int index, JsonValue item)
    {
        if (index == Items.Count)
        {
            Add(item);
            return;
        }
        var separatorSource = index > 0 ? Items[index - 1].TrailingTrivia
            : Items.Count > 1 ? Items[0].TrailingTrivia
            : LeadingTrivia;
        JsonTriviaEditing.PrepareInsert(separatorSource, Items[index].LeadingTrivia, item.LeadingTrivia, item.TrailingTrivia);
        Items.Insert(index, item);
    }

    public void RemoveAt(int index)
    {
        if (index > 0 && index == Items.Count - 1)
        {
            JsonTriviaEditing.PrepareRemoveLast(Items[index - 1].TrailingTrivia, Items[index].TrailingTrivia);
        }
        Items.RemoveAt(index);
    }

    public void Clear() => Items.Clear();

    public bool Contains(JsonValue item) => Items.Contains(item);

    public void CopyTo(JsonValue[] array, int arrayIndex) => Items.CopyTo(array, arrayIndex);

    public bool Remove(JsonValue item)
    {
        var index = Items.IndexOf(item);
        if (index < 0)
        {
            return false;
        }
        RemoveAt(index);
        return true;
    }

    public IEnumerator<JsonValue> GetEnumerator() => Items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();

    public override JsonValue GetItem(int index) => Items[index];

    public override JsonValue SetItem(int index, JsonValue newValue)
    {
        Items[index] = newValue;
        return this;
    }

    public override JsonValue InsertItem(int index, JsonValue newValue)
    {
        Insert(index, newValue);
        return this;
    }

    public override JsonValue RemoveItemAt(int index)
    {
        RemoveAt(index);
        return this;
    }

    public override JsonValue RemoveItem(JsonValue item)
    {
        Remove(item);
        return this;
    }

    public override JsonValue AddItem(JsonValue item)
    {
        Add(item);
        return this;
    }

    public void Add(JsonValue item)
    {
        if (Items.Count > 0)
        {
            var lastItem = Items[Items.Count - 1];
            var separatorSource = Items.Count > 1 ? Items[Items.Count - 2].TrailingTrivia : LeadingTrivia;
            JsonTriviaEditing.PrepareAppend(separatorSource, lastItem.LeadingTrivia, lastItem.TrailingTrivia, item.LeadingTrivia, item.TrailingTrivia);
        }
        Items.Add(item);
    }

    public override int GetHashCode()
    {
        return Items.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as JsonArray);
    }

    public bool Equals(JsonArray? other)
    {
        if (other is null || other.Count != Count)
        {
            return false;
        }
        for (int i = 0; i < Count; i++)
        {
            if (!Items[i].Equals(other.Items[i]))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Used during parsing to add items without modifying trivia.
    /// </summary>
    /// <param name="item">The item to add.</param>
    internal void AddParsedValue(JsonValue item)
    {
        Items.Add(item);
    }
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "not using 'Collection' suffix for consistency with other types")]
public class JsonObject : JsonValue, IDictionary<string, JsonValue>, IList<JsonProperty>, IEquatable<JsonObject>
{
    private readonly List<JsonProperty> sequence = new(8);

    private readonly Dictionary<string, JsonProperty> items = new(8);

    public override JsonValueKind Kind => JsonValueKind.JsonObject;

    public override bool IsPrimitive => false;

    public override JsonObject GetObjectValue() => this;

    public ICollection<string> Keys => items.Keys;

    public ICollection<JsonValue> Values => [.. items.Values.Select(it => it.Value)];

    public int Count => items.Count;

    public bool IsReadOnly => false;

    public JsonProperty this[int index]
    {
        get => sequence[index];
        set => sequence[index] = value;
    }

    public JsonValue this[string key]
    {
        get => items[key].Value;
        set
        {
            if (items.TryGetValue(key, out var prop))
            {
                var oldValue = prop.Value;
                prop.Value = value;
                if (prop.Value.LeadingTrivia.Count == 0)
                {
                    prop.Value.LeadingTrivia.AddRange(oldValue.LeadingTrivia);
                }
                if (prop.Value.TrailingTrivia.Count == 0)
                {
                    prop.Value.TrailingTrivia.AddRange(oldValue.TrailingTrivia);
                }
            }
            else
            {
                Add(key, value);
            }
        }
    }

    public override JsonProperty GetProperty(string key)
    {
        if (items.TryGetValue(key, out var prop))
        {
            return prop;
        }
        throw new KeyNotFoundException($"The given key '{key}' was not present in the JsonObject.");
    }

    public override JsonValue GetPropertyValue(string key)
    {
        if (items.TryGetValue(key, out var prop))
        {
            return prop.Value;
        }
        throw new KeyNotFoundException($"The given key '{key}' was not present in the JsonObject.");
    }

    public override JsonValue SetProperty(string key, JsonValue value)
    {
        this[key] = value;
        return this;
    }

    public override JsonValue AddProperty(string key, JsonValue value)
    {
        Add(key, value);
        return this;
    }

    public override JsonValue InsertProperty(int index, JsonProperty prop)
    {
        Insert(index, prop);
        return this;
    }

    public override JsonValue RemoveProperty(string key)
    {
        Remove(key);
        return this;
    }

    /// <summary>
    /// Used  during parsing to add items without modifying trivia.
    /// </summary>
    /// <param name="prop">The property to add.</param>
    internal void AddParsedProperty(JsonProperty prop)
    {
        items[prop.Key.Value] = prop;
        sequence.Add(prop);
    }

    public void Add(KeyValuePair<string, JsonValue> item)
    {
        Add(item.Key, item.Value);
    }

    public void Add(string key, JsonValue value)
    {
        if (items.ContainsKey(key))
        {
            throw new ArgumentException($"An item with the same key has already been added. Key: {key}");
        }
        Add(new JsonProperty(key, value));
    }

    public void Add(JsonProperty prop)
    {
        if (sequence.Count > 0)
        {
            var last = sequence[sequence.Count - 1];
            var separatorSource = sequence.Count > 1 ? sequence[sequence.Count - 2].Value.TrailingTrivia : LeadingTrivia;
            CopyKeyValueSeparator(last, prop);
            JsonTriviaEditing.PrepareAppend(separatorSource, last.Key.LeadingTrivia, last.Value.TrailingTrivia, prop.Key.LeadingTrivia, prop.Value.TrailingTrivia);
        }
        items[prop.Key.Value] = prop;
        sequence.Add(prop);
    }

    public void Insert(int index, JsonProperty prop)
    {
        if (index == sequence.Count)
        {
            Add(prop);
            return;
        }
        var next = sequence[index];
        var separatorSource = index > 0 ? sequence[index - 1].Value.TrailingTrivia
            : sequence.Count > 1 ? sequence[0].Value.TrailingTrivia
            : LeadingTrivia;
        CopyKeyValueSeparator(next, prop);
        JsonTriviaEditing.PrepareInsert(separatorSource, next.Key.LeadingTrivia, prop.Key.LeadingTrivia, prop.Value.TrailingTrivia);
        sequence.Insert(index, prop);
        items[prop.Key.Value] = prop;
    }

    public bool ContainsKey(string key)
    {
        return items.ContainsKey(key);
    }

    public bool Remove(string key)
    {
        if (items.ContainsKey(key))
        {
            var index = sequence.FindIndex(p => p.Key.Value == key);
            if (index < 0)
            {
                throw new InvalidOperationException("Inconsistent state: key exists in dictionary but not in sequence");
            }
            RemoveAt(index);
            return true;
        }
        return false;
    }

    public bool TryGetValue(string key, out JsonValue value)
    {
        if (items.TryGetValue(key, out var prop))
        {
            value = prop.Value;
            return true;
        }
        value = JsonNull.Instance;
        return false;
    }

    public void Clear()
    {
        items.Clear();
        sequence.Clear();
    }

    public bool Contains(KeyValuePair<string, JsonValue> item)
    {
        if (items.TryGetValue(item.Key, out var prop))
        {
            return EqualityComparer<JsonValue>.Default.Equals(prop.Value, item.Value);
        }
        return false;
    }

    public void CopyTo(KeyValuePair<string, JsonValue>[] array, int arrayIndex)
    {
        ((ICollection<KeyValuePair<string, JsonValue>>)items).CopyTo(array, arrayIndex);
    }

    public bool Remove(KeyValuePair<string, JsonValue> item)
    {
        if (Contains(item))
        {
            return Remove(item.Key);
        }
        return false;
    }

    public IEnumerator<KeyValuePair<string, JsonValue>> GetEnumerator()
    {
        return items.Select(kv => new KeyValuePair<string, JsonValue>(kv.Key, kv.Value.Value)).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int IndexOf(JsonProperty item)
    {
        return sequence.IndexOf(item);
    }

    public void RemoveAt(int index)
    {
        var removed = sequence[index];
        if (index > 0 && index == sequence.Count - 1)
        {
            JsonTriviaEditing.PrepareRemoveLast(sequence[index - 1].Value.TrailingTrivia, removed.Value.TrailingTrivia);
        }
        items.Remove(removed.Key.Value);
        sequence.RemoveAt(index);
    }

    public bool Contains(JsonProperty prop)
    {
        if (!items.TryGetValue(prop.Key.Value, out var item))
        {
            return false;
        }
        if (!EqualityComparer<JsonValue>.Default.Equals(item.Value, prop.Value))
        {
            return false;
        }
        return true;
    }

    public void CopyTo(JsonProperty[] array, int arrayIndex)
    {
        sequence.CopyTo(array, arrayIndex);
    }

    public bool Remove(JsonProperty item)
    {
        var index = sequence.IndexOf(item);
        if (index < 0)
        {
            return false;
        }
        RemoveAt(index);
        return true;
    }

    IEnumerator<JsonProperty> IEnumerable<JsonProperty>.GetEnumerator()
    {
        return sequence.GetEnumerator();
    }

    public override int GetHashCode()
    {
        return sequence.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as JsonObject);
    }

    public bool Equals(JsonObject? other)
    {
        if (other is null || other.Count != Count)
        {
            return false;
        }
        for (int i = 0; i < sequence.Count; i++)
        {
            if (!sequence[i].Key.Equals(other.sequence[i].Key) || !sequence[i].Value.Equals(other.sequence[i].Value))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Copies the colon and the whitespace around it from <paramref name="src"/> to <paramref name="dst"/>.
    /// </summary>
    private static void CopyKeyValueSeparator(JsonProperty src, JsonProperty dst)
    {
        JsonTriviaEditing.CopyKeySeparator(src.Key.TrailingTrivia, dst.Key.TrailingTrivia);
        JsonTriviaEditing.CopyIndentation(src.Value.LeadingTrivia, dst.Value.LeadingTrivia);
    }
}
