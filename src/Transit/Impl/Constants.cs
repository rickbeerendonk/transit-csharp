// Copyright � 2014 Rick Beerendonk. All Rights Reserved.
//
// This code is a C# port of the Java version created and maintained by Cognitect, therefore
//
// Copyright � 2014 Cognitect. All Rights Reserved.
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

namespace Beerendonk.Transit.Impl
{
    /// <summary>
    /// Contains a list of constants.
    /// </summary>
    internal static class Constants
    {
        /// <summary>
        /// The escape.
        /// </summary>
        public const char Esc = '~';

        /// <summary>
        /// The escape string,
        /// </summary>
        public const string EscStr = "~";

        /// <summary>
        /// The tag.
        /// </summary>
        public const char Tag = '#';

        /// <summary>
        /// The tag string.
        /// </summary>
        public const string TagStr = "#";

        /// <summary>
        /// The sub.
        /// </summary>
        public const char Sub = '^';

        /// <summary>
        /// The sub string.
        /// </summary>
        public const string SubStr = "^";

        /// <summary>
        /// Reserved.
        /// </summary>
        public const char Reserved = '`';

        /// <summary>
        /// The escape tag.
        /// </summary>
        public const string EscTag = EscStr + TagStr;

        /// <summary>
        /// The quote tag.
        /// </summary>
        public const string QuoteTag = EscTag + "'";

        /// <summary>
        /// The directory as list.
        /// </summary>
        public const string DirectoryAsList = "^ ";
    }
}