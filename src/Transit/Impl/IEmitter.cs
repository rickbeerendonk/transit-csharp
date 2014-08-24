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
    internal interface IEmitter
    {
        void Emit(object obj, bool asDictionaryKey, WriteCache cache);
        void EmitNull(bool asDictionaryKey, WriteCache cache);
        void EmitString(string prefix, string tag, string s, bool asDictionaryKey, WriteCache cache);
        void EmitBoolean(bool b, bool asDictionaryKey, WriteCache cache);
        void EmitInteger(object i, bool asDictionaryKey, WriteCache cache);
        void EmitInteger(long i, bool asDictionaryKey, WriteCache cache);
        void EmitDouble(object d, bool asDictionaryKey, WriteCache cache);
        void EmitDouble(float d, bool asDictionaryKey, WriteCache cache);
        void EmitDouble(double d, bool asDictionaryKey, WriteCache cache);
        void EmitBinary(object b, bool asDictionaryKey, WriteCache cache);
        void EmitListStart(long size);
        void EmitListEnd();
        void EmitDictionaryStart(long size);
        void EmitDictionaryEnd();
        bool PrefersStrings();
        void FlushWriter();
    }
}
