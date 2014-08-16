// Copyright © 2014 NForza. All Rights Reserved.
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

namespace NForza.Transit.Impl
{
    /// <summary>
    /// Identifies a parser.
    /// </summary>
    public interface IParser
    {
        /// <summary>
        /// Parses the specified cache.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <returns></returns>
        object Parse(ReadCache cache);

        /// <summary>
        /// Parses the value.
        /// </summary>
        /// <param name="asDictionaryKey">If set to <c>true</c> [as dictionary key].</param>
        /// <param name="cache">The cache.</param>
        /// <returns>The parsed value.</returns>
        object ParseVal(bool asDictionaryKey, ReadCache cache);

        /// <summary>
        /// Parses the dictionary.
        /// </summary>
        /// <param name="asDictionaryKey">If set to <c>true</c> [as dictionary key].</param>
        /// <param name="cache">The cache.</param>
        /// <param name="handler">The handler.</param>
        /// <returns>The parced dictionary</returns>
        object ParseDictionary(bool asDictionaryKey, ReadCache cache, IDictionaryReadHandler handler);

        /// <summary>
        /// Parses the list.
        /// </summary>
        /// <param name="asDictionaryKey">If set to <c>true</c> [as dictionary key].</param>
        /// <param name="cache">The cache.</param>
        /// <param name="handler">The handler.</param>
        /// <returns>The parsed list.</returns>
        object ParseList(bool asDictionaryKey, ReadCache cache, IListReadHandler handler);
    }
}
