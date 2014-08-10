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

using System;

namespace NForza.Transit
{
    /// <summary>
    /// Represents a hypermedia link, as per http://amundsen.com/media-types/collection/format/#arrays-links
    /// </summary>
    public interface ILink
    {
        /// <summary>
        /// Get the link's href
        /// </summary>
        /// <returns>href</returns>
        Uri GetHref();

        /// <summary>
        /// Get the link's rel
        /// </summary>
        /// <returns>rel</returns>
        string GetRel();

        /// <summary>
        /// Get the link's name
        /// </summary>
        /// <returns>name</returns>
        string GetName();

        /// <summary>
        /// Get the link's prompt
        /// </summary>
        /// <returns>prompt</returns>
        string GetPrompt();

        /// <summary>
        /// Get the link's render semantic
        /// </summary>
        /// <returns>render</returns>
        string GetRender();
    }
}
