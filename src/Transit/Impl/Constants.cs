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
    public static class Constants
    {
        public const char Esc = '~';
        public const string EscStr = "~";
        public const char Tag = '#';
        public const string TagStr = "#";
        public const char Sub = '^';
        public const string SubStr = "^";
        public const char Reserved = '`';
        public const string EscTag = EscStr + TagStr;
        public const string QuoteTag = EscTag + "'";
        public const string DirectoryAsList = "^ ";
    }
}