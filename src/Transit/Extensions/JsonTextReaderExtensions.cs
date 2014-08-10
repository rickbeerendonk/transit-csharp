// Copyright © 2014 NForza. All Rights Reserved.
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

namespace NForza.Transit.Extensions
{
    internal static class JsonTextReaderExtensions
    {
        /// <summary>
        /// Tries to read the next token and returns the token type.
        /// </summary>
        /// <param name="jsonTextReader">A JsonTextReader</param>
        /// <returns>The token type</returns>
        public static JsonToken NextToken(this JsonTextReader jsonTextReader)
        {
            if (jsonTextReader.Read())
            {
                return jsonTextReader.TokenType;
            }

            return default(JsonToken);
        }
    }
}
