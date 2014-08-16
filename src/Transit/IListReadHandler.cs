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

namespace NForza.Transit
{
    /// <summary>
    /// Provides an <see cref="IListReader"/> to Transit 
    /// to use in incrementally parsing a list representation of 
    /// a value.
    /// </summary>
    public interface IListReadHandler : IReadHandler
    {
        /// <summary>
        /// Provides an <see cref="IListReader"/> that 
        /// a parser can use to convert a list representation to 
        /// an instance of a type incrementally.
        /// </summary>
        /// <returns>A ListReader.</returns>
        IListReader ListReader();
    }
}