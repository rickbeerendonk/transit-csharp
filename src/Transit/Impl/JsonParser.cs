// Copyright © 2014 Rick Beerendonk. All Rights Reserved.
//
// This code is a C# port of the Java version created and maintained by Cognitect, therefore
//
// Copyright © 2014 Cognitect. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS-IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Newtonsoft.Json;
using Beerendonk.Transit.Extensions;
using System;
using System.Collections.Immutable;

namespace Beerendonk.Transit.Impl;

/// <summary>
/// Represents a JSON parser.
/// </summary>
internal class JsonParser(
    JsonTextReader jsonTextReader,
    IImmutableDictionary<string, IReadHandler> handlers,
    IDefaultReadHandler<object> defaultHandler,
    IDictionaryReader dictionaryBuilder,
    IListReader listBuilder)
    : AbstractParser(handlers, defaultHandler, dictionaryBuilder, listBuilder)
{
    private readonly JsonTextReader jp = jsonTextReader;

    private object? ParseLong()
    {
        // TODO Use BigInteger just in case (write test, see for example TestReadBooleanDictionary where short numbers are used):
        object? val;
        //try {
            val = jp.Value;
        //} catch(IOException e) {
        //    val = new BigInteger(jp.getText());
        //}

        return val;
    }

    /// <summary>
    /// Parses the specified cache.
    /// </summary>
    /// <param name="cache">The cache.</param>
    /// <returns></returns>
    public override object? Parse(ReadCache cache)
    {
        if (jp.Read())
        {
            return ParseVal(false, cache);
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Parses the value.
    /// </summary>
    /// <param name="asDictionaryKey">If set to <c>true</c> [as dictionary key].</param>
    /// <param name="cache">The cache.</param>
    /// <returns>
    /// The parsed value.
    /// </returns>
    public override object? ParseVal(bool asDictionaryKey, ReadCache cache) =>
        jp.TokenType switch
        {
            JsonToken.Integer => ParseLong(),
            JsonToken.StartArray => ParseList(asDictionaryKey, cache, null),
            JsonToken.StartObject => ParseDictionary(asDictionaryKey, cache, null),
            JsonToken.PropertyName or JsonToken.String => cache.CacheRead(jp.Value as string, asDictionaryKey, this),
            JsonToken.Float or JsonToken.Boolean => jp.Value,
            _ => null
        };

    /// <summary>
    /// Parses the dictionary.
    /// </summary>
    /// <param name="ignored">if set to <c>true</c> [ignored].</param>
    /// <param name="cache">The cache.</param>
    /// <param name="handler">The handler.</param>
    /// <returns></returns>
    public override object? ParseDictionary(bool ignored, ReadCache cache, IDictionaryReadHandler? handler)
    {
        return ParseDictionary(ignored, cache, handler, JsonToken.EndObject);
    }

    /// <summary>
    /// Parses the dictionary.
    /// </summary>
    /// <param name="ignored">if set to <c>true</c> [ignored].</param>
    /// <param name="cache">The cache.</param>
    /// <param name="handler">The handler.</param>
    /// <param name="endToken">The end token.</param>
    /// <returns></returns>
    public object? ParseDictionary(bool ignored, ReadCache cache, IDictionaryReadHandler? handler, JsonToken endToken)
    {
        IDictionaryReader dr = (handler != null) ? handler.DictionaryReader() : dictionaryBuilder;

        object d = dr.Init();

        while (jp.NextToken() != endToken)
        {
            object? key = ParseVal(true, cache);
            if (key is Tag taggedKey)
            {
                object? val;
                jp.Read(); // advance to read value
                string tag = taggedKey.GetValue();
                if (TryGetHandler(tag, out IReadHandler? valHandler) && valHandler is not null)
                {
                    if (jp.TokenType == JsonToken.StartObject && valHandler is IDictionaryReadHandler mapHandler) {
                        // use map reader to decode value
                        val = ParseDictionary(false, cache, mapHandler);
                    } else if (jp.TokenType == JsonToken.StartArray && valHandler is IListReadHandler listHandler) {
                        // use array reader to decode value
                        val = ParseList(false, cache, listHandler);
                    } else {
                        // read value and decode normally
                        val = valHandler.FromRepresentation(ParseVal(false, cache)!);
                    }
                } else {
                    // default decode
                    val = this.Decode(tag, ParseVal(false, cache));
                }
                jp.Read(); // advance to read end of object or array
                return val;
            }
            else
            {
                jp.Read(); // advance to read value
                d = dr.Add(d, key, ParseVal(false, cache));
            }
        }

        return dr.Complete(d);
    }

    /// <summary>
    /// Parses the list.
    /// </summary>
    /// <param name="asDictionaryKey">If set to <c>true</c> [as dictionary key].</param>
    /// <param name="cache">The cache.</param>
    /// <param name="handler">The handler.</param>
    /// <returns>
    /// The parsed list.
    /// </returns>
    public override object? ParseList(bool asDictionaryKey, ReadCache cache, IListReadHandler? handler)
    {
        // if nextToken == JsonToken.EndArray
        if (jp.NextToken() != JsonToken.EndArray)
        {
            bool firstIsDirectoryAsList = jp.TokenType == JsonToken.String
                && (jp.Value as string) == Constants.DirectoryAsList;
            object? firstVal = ParseVal(false, cache);
            if (firstVal != null)
            {
                if (firstIsDirectoryAsList)
                {
                    // if the same, build a map w/ rest of array contents
                    return ParseDictionary(false, cache, null, JsonToken.EndArray);
                }
                else
                    if (firstVal is Tag)
                    {
                        if (firstVal is Tag)
                        {
                            object? val;
                            jp.Read(); // advance to value
                            string tag = ((Tag)firstVal).GetValue();
                            if (TryGetHandler(tag, out IReadHandler? valHandler) && valHandler is not null)
                            {
                                if (jp.TokenType == JsonToken.StartObject && valHandler is IDictionaryReadHandler mapHandler)
                                {
                                    // use map reader to decode value
                                    val = ParseDictionary(false, cache, mapHandler);
                                }
                                else
                                    if (jp.TokenType == JsonToken.StartArray && valHandler is IListReadHandler listHandler)
                                    {
                                        // use array reader to decode value
                                        val = ParseList(false, cache, listHandler);
                                    }
                                    else
                                    {
                                        // read value and decode normally
                                        val = valHandler.FromRepresentation(ParseVal(false, cache)!);
                                    }
                            }
                            else
                            {
                                // default decode
                                val = this.Decode(tag, ParseVal(false, cache));
                            }
                            jp.Read(); // advance past end of object or array
                            return val;
                        }
                    }
            }

            // Process list w/o special decoding or interpretation
            IListReader lr = (handler != null) ? handler.ListReader() : listBuilder;
            object l = lr.Init();
            l = lr.Add(l, firstVal);
            while (jp.NextToken() != JsonToken.EndArray) {
                l = lr.Add(l, ParseVal(false, cache));
            }
            return lr.Complete(l);
        }

        // Make an empty collection, honoring handler's ListReader, if present
        IListReader lr2 = (handler != null) ? handler.ListReader() : listBuilder;
        return lr2.Complete(lr2.Init());
    }
}
