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

using System;
using System.Collections.Immutable;

namespace Beerendonk.Transit.Impl;

/// <summary>
/// Represents an abstract parser.
/// </summary>
internal abstract class AbstractParser : IParser
{
    /// <summary>
    /// Formats the date time.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public static string FormatDateTime(DateTime value)
    {
        const string dateTimeFormat = "yyyy-MM-dd'T'HH:mm:ss.fff'Z'";
        return new DateTimeOffset(value).UtcDateTime.ToString(dateTimeFormat);
    }

    /// <summary>
    /// The handlers.
    /// </summary>
    protected readonly IImmutableDictionary<string, IReadHandler> handlers;

    private readonly IDefaultReadHandler<object> defaultHandler;

    /// <summary>
    /// The dictionary builder.
    /// </summary>
    protected IDictionaryReader dictionaryBuilder;

    /// <summary>
    /// The list builder.
    /// </summary>
    protected IListReader listBuilder;

    /// <summary>
    /// Initializes a new instance of the <see cref="AbstractParser"/> class.
    /// </summary>
    /// <param name="handlers">The handlers.</param>
    /// <param name="defaultHandler">The default handler.</param>
    /// <param name="dictionaryBuilder">The dictionary builder.</param>
    /// <param name="listBuilder">The list builder.</param>
    protected AbstractParser(IImmutableDictionary<string, IReadHandler> handlers,
        IDefaultReadHandler<object> defaultHandler,
        IDictionaryReader dictionaryBuilder,
        IListReader listBuilder)
    {
        this.handlers = handlers;
        this.defaultHandler = defaultHandler;
        this.dictionaryBuilder = (IDictionaryReader)dictionaryBuilder;
        this.listBuilder = (IListReader)listBuilder;
    }

    /// <summary>
    /// Tries the get the handler.
    /// </summary>
    /// <param name="tag">The tag.</param>
    /// <param name="readHandler">The read handler.</param>
    /// <returns><c>true</c> if the handler is returned; <c>false</c> otherwise.</returns>
    protected bool TryGetHandler(string tag, out IReadHandler readHandler)
    {
        return handlers.TryGetValue(tag, out readHandler);
    }

    /// <summary>
    /// Decodes the specified tag.
    /// </summary>
    /// <param name="tag">The tag.</param>
    /// <param name="representation">The representation.</param>
    /// <returns></returns>
    /// <exception cref="TransitException">Cannot FromRepresentation  + tag + :  + representation.ToString()</exception>
    protected object Decode(string tag, object representation)
    {
        IReadHandler d;
        if (TryGetHandler(tag, out d))
        {
            return d.FromRepresentation(representation);
        }
        else
        {
            if (defaultHandler != null)
            {
                return defaultHandler.FromRepresentation(tag, representation);
            }
            else
            {
                throw new TransitException("Cannot FromRepresentation " + tag + ": " + representation.ToString());
            }
        }
    }

    /// <summary>
    /// Parses the string.
    /// </summary>
    /// <param name="obj">The object.</param>
    /// <returns></returns>
    protected internal object ParseString(object obj)
    {
        if (obj is not string s || s.Length <= 1)
        {
            return obj;
        }

        return s[0] switch
        {
            Constants.Esc => s[1] switch
            {
                Constants.Esc or Constants.Sub or Constants.Reserved => s[1..],
                Constants.Tag => new Tag(s[2..]),
                _ => Decode(s[1..2], s.Length > 2 ? s[2..] : string.Empty)
            },
            Constants.Sub when s[1] == ' ' => Constants.DirectoryAsList,
            _ => obj
        };
    }

    /// <summary>
    /// Parses the specified cache.
    /// </summary>
    /// <param name="cache">The cache.</param>
    /// <returns></returns>
    public abstract object Parse(ReadCache cache);

    /// <summary>
    /// Parses the value.
    /// </summary>
    /// <param name="asDictionaryKey">If set to <c>true</c> [as dictionary key].</param>
    /// <param name="cache">The cache.</param>
    /// <returns>
    /// The parsed value.
    /// </returns>
    public abstract object ParseVal(bool asDictionaryKey, ReadCache cache);

    /// <summary>
    /// Parses the dictionary.
    /// </summary>
    /// <param name="asDictionaryKey">If set to <c>true</c> [as dictionary key].</param>
    /// <param name="cache">The cache.</param>
    /// <param name="handler">The handler.</param>
    /// <returns>
    /// The parced dictionary
    /// </returns>
    public abstract object ParseDictionary(bool asDictionaryKey, ReadCache cache, IDictionaryReadHandler handler);

    /// <summary>
    /// Parses the list.
    /// </summary>
    /// <param name="asDictionaryKey">If set to <c>true</c> [as dictionary key].</param>
    /// <param name="cache">The cache.</param>
    /// <param name="handler">The handler.</param>
    /// <returns>
    /// The parsed list.
    /// </returns>
    public abstract object ParseList(bool asDictionaryKey, ReadCache cache, IListReadHandler handler);
}
