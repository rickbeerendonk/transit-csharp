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
    /// Converts a transit representation to an instance of a type; if type 
    /// implements <see cref="IListReadHandler"/> or 
    /// <see cref="IDictionaryReadHandler"/> to support 
    /// incremental parsing of representation, that interface will be used instead.
    /// </summary>
    public interface IReadHandler
    {
        /// <summary>
        /// Converts a transit value to an instance of a type.
        /// </summary>
        /// <param name="representation">The transit value</param>
        /// <returns>The converted object</returns>
        object FromRepresentation(object representation);
    }
}