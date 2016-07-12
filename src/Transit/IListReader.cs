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

namespace Beerendonk.Transit
{
    /// <summary>
    /// Identifies a list reader.
    /// </summary>
    public interface IListReader
    {
        /// <summary>
        /// Initializes a new gestational list.
        /// </summary>
        /// <returns>A new gestational list.</returns>
        object Init();

        /// <summary>
        /// Adds an item to the list, returning a new list; 
        /// new list must be used for any further invocations.
        /// </summary>
        /// <param name="list">A gestational list.</param>
        /// <param name="item">An item.</param>
        /// <returns>A new gestational list.</returns>
        object Add(object list, object item);

        /// <summary>
        /// Completes building of a list from a gestational list.
        /// </summary>
        /// <param name="list">The gestational list.</param>
        /// <returns>The completed list.</returns>
        object Complete(object list);
    }
}