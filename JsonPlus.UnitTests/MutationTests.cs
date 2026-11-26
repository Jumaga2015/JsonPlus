// <copyright file="MutationTests.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace JsonPlus.UnitTests;

using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class MutationTests
{
    [TestMethod]
    public void ModifyPropertyValuePreservesTrivia()
    {
        var originalJson = """
        {
            "name": "John", // User's first name
            "age": /* age property */ 20, // should be over 21
            "city": "New York"
        }
        """;

        // parse and modify JSON
        var value = JsonCodec.Decode(originalJson).SetProperty("age", new JsonNumber(22));

        var expectedJson = """
        {
            "name": "John", // User's first name
            "age": /* age property */ 22, // should be over 21
            "city": "New York"
        }
        """;

        Assert.AreEqual(expectedJson, JsonCodec.Encode(value), "Modified JSON does not match expected output.");
    }

    [TestMethod]
    public void AddPropertyPreservesTrivia()
    {
        var originalJson = """
        {
            "firstName": "Alice",
            "lastName": "Smith",
            "email": "alice@wonderland.com" // email your fantasy trips
        }
        """;

        // parse and modify JSON
        // the methods automatically copy indentation from the previous property
        var jsonObject = JsonCodec.Decode(originalJson)
            .AddProperty("phone", new JsonString("+1-555-0123"))
            .InsertProperty(2, new JsonProperty("address", new JsonString("123 Fantasy Rd")));

        var expectedJson = """
        {
            "firstName": "Alice",
            "lastName": "Smith",
            "address": "123 Fantasy Rd",
            "email": "alice@wonderland.com", // email your fantasy trips
            "phone": "+1-555-0123"
        }
        """;

        Assert.AreEqual(expectedJson, JsonCodec.Encode(jsonObject), "Modified JSON does not match expected output.");
    }

    [TestMethod]
    public void AddArrayItemPreservesTrivia()
    {
        var originalJson = """
        [
            "apple", // First fruit
            "banana", // Second fruit
            "cherry" // Third fruit
        ]
        """;

        // parse and modify
        // the Add/AddItem methods automatically handle trivia preservation and comma insertion
        var value = JsonCodec.Decode(originalJson).AddItem(new JsonString("date"));

        var expectedJson = """
        [
            "apple", // First fruit
            "banana", // Second fruit
            "cherry", // Third fruit
            "date"
        ]
        """;

        Assert.AreEqual(expectedJson, JsonCodec.Encode(value), "Modified JSON does not match expected output.");
    }

    [TestMethod]
    public void ModifyTriviaPreservesFormatting()
    {
        var originalJson = """
        {
            "status": "don't panic", // important status
            "answer": 42,
        }
        """;

        // decode
        var value = JsonCodec.Decode(originalJson);

        // add a comment, just after the comma
        value.GetPropertyValue("answer").TrailingTrivia
            .Insert(1, new JsonTrivia(JsonTriviaKind.SingleLineComment, " // the answer to life, universe, and everything"));

        var expectedJson = """
        {
            "status": "don't panic", // important status
            "answer": 42, // the answer to life, universe, and everything
        }
        """;

        Assert.AreEqual(expectedJson, JsonCodec.Encode(value), "Modified JSON does not match expected output.");
    }

    private const string ArrayOfObjectsJson = """
        [
            // first
            { "name": "prod" }, // after prod
            // second
            { "name": "dev" } // after dev
        ]
        """;

    private const string ObjectJson = """
        {
            // comment a
            "a": 1, // after a
            // comment b
            "b": 2 // after b
        }
        """;

    private static JsonValue NewObject(string name) => JsonCodec.Decode($$"""{ "name": "{{name}}" }""");

    private static void AssertEncodesTo(string expectedJson, JsonValue value)
    {
        var actualJson = JsonCodec.Encode(value);
        Assert.AreEqual(expectedJson, actualJson, "Modified JSON does not match expected output.");
        JsonCodec.Decode(actualJson, JsonCodec.RelaxedDecodingOptions with { AllowTrailingCommas = false });
    }

    [TestMethod]
    public void AddObjectItemPutsCommaAfterClosingBrace()
    {
        var value = JsonCodec.Decode(ArrayOfObjectsJson, JsonCodec.RelaxedDecodingOptions).AddItem(NewObject("test"));

        AssertEncodesTo("""
            [
                // first
                { "name": "prod" }, // after prod
                // second
                { "name": "dev" }, // after dev
                { "name": "test" }
            ]
            """, value);
    }

    [TestMethod]
    public void InsertObjectItemAtStartAddsCommaAndKeepsComments()
    {
        var value = JsonCodec.Decode(ArrayOfObjectsJson, JsonCodec.RelaxedDecodingOptions).InsertItem(0, NewObject("test"));

        AssertEncodesTo("""
            [
                { "name": "test" },
                // first
                { "name": "prod" }, // after prod
                // second
                { "name": "dev" } // after dev
            ]
            """, value);
    }

    [TestMethod]
    public void InsertObjectItemInMiddleAddsCommaAndKeepsComments()
    {
        var value = JsonCodec.Decode(ArrayOfObjectsJson, JsonCodec.RelaxedDecodingOptions).InsertItem(1, NewObject("test"));

        AssertEncodesTo("""
            [
                // first
                { "name": "prod" }, // after prod
                { "name": "test" },
                // second
                { "name": "dev" } // after dev
            ]
            """, value);
    }

    [TestMethod]
    public void InsertScalarItemInSingleLineArrayAddsComma()
    {
        var value = JsonCodec.Decode("[1, 2]").InsertItem(1, new JsonNumber(9));

        AssertEncodesTo("[1, 9, 2]", value);
    }

    [TestMethod]
    public void RemoveLastItemRemovesCommaOfNewLastItem()
    {
        var value = JsonCodec.Decode(ArrayOfObjectsJson, JsonCodec.RelaxedDecodingOptions).RemoveItemAt(1);

        AssertEncodesTo("""
            [
                // first
                { "name": "prod" } // after prod
            ]
            """, value);
    }

    [TestMethod]
    public void RemoveLastItemKeepsExistingTrailingComma()
    {
        var value = JsonCodec.Decode("[1, 2,]", JsonCodec.RelaxedDecodingOptions).RemoveItemAt(1);

        Assert.AreEqual("[1,]", JsonCodec.Encode(value));
    }

    [TestMethod]
    public void AddPropertyDoesNotCopyCommentOfPreviousProperty()
    {
        var value = JsonCodec.Decode(ObjectJson, JsonCodec.RelaxedDecodingOptions).AddProperty("c", new JsonNumber(3));

        AssertEncodesTo("""
            {
                // comment a
                "a": 1, // after a
                // comment b
                "b": 2, // after b
                "c": 3
            }
            """, value);
    }

    [TestMethod]
    public void InsertPropertyAtStartDoesNotCopyCommentOfNextProperty()
    {
        var value = JsonCodec.Decode(ObjectJson, JsonCodec.RelaxedDecodingOptions).InsertProperty(0, new JsonProperty("z", new JsonNumber(0)));

        AssertEncodesTo("""
            {
                "z": 0,
                // comment a
                "a": 1, // after a
                // comment b
                "b": 2 // after b
            }
            """, value);
    }

    [TestMethod]
    public void InsertPropertyAtStartOfSinglePropertyObjectAddsNoTrailingComma()
    {
        var value = JsonCodec.Decode("""{ "a": 1 }""").InsertProperty(0, new JsonProperty("z", new JsonNumber(0)));

        AssertEncodesTo("""{ "z": 0, "a": 1 }""", value);
    }

    [TestMethod]
    public void RemoveLastPropertyRemovesCommaOfNewLastProperty()
    {
        var value = JsonCodec.Decode(ObjectJson, JsonCodec.RelaxedDecodingOptions).RemoveProperty("b");

        AssertEncodesTo("""
            {
                // comment a
                "a": 1 // after a
            }
            """, value);
    }

    [TestMethod]
    public void AddObjectPropertyValueAfterObjectValueDoesNotCopyOpeningBrace()
    {
        var value = JsonCodec.Decode("""{ "a": { "x": 1 } }""").AddProperty("b", new JsonNumber(2));

        AssertEncodesTo("""{ "a": { "x": 1 }, "b": 2 }""", value);
    }
}
