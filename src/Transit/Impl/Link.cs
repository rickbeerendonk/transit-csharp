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
using System.Collections.Immutable;

namespace NForza.Transit.Impl
{
    /// <summary>
    /// Represents a link.
    /// </summary>
    internal class Link : ILink
    {
        private const string LINK = "link";
        private const string IMAGE = "image";

        private const string HREF = "href";
        private const string REL = "rel";
        private const string PROMPT = "prompt";
        private const string NAME = "name";
        private const string RENDER = "render";

        private IImmutableDictionary<string, object> dictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="Link"/> class.
        /// </summary>
        /// <param name="href">The href.</param>
        /// <param name="rel">The rel.</param>
        /// <param name="name">The name.</param>
        /// <param name="render">The render.</param>
        /// <param name="prompt">The prompt.</param>
        /// <exception cref="System.ArgumentNullException">
        /// href
        /// or
        /// rel
        /// </exception>
        /// <exception cref="System.ArgumentException">Value of render must be "link" or "image".</exception>
        public Link(Uri href, string rel, string name, string render, string prompt)
        {
            var dictionary = ImmutableDictionary.Create<string, object>();

            if (href == null)
            {
                throw new ArgumentNullException("href");
            }
            dictionary = dictionary.Add(HREF, href);

            if (rel == null)
            {
                throw new ArgumentNullException("rel");
            }
            dictionary = dictionary.Add(REL, rel);
            
            if (name != null)
            {
                dictionary = dictionary.Add(NAME, name);
            }

            if (prompt != null)
            {
                dictionary = dictionary.Add(PROMPT, prompt);
            }

            if (render != null)
            {
                if ((render == LINK) || (render == IMAGE))
                {
                    dictionary = dictionary.Add(RENDER, render);
                }
                else
                {
                    throw new ArgumentException("Value of render must be \"link\" or \"image\".");
                }
            }

            this.dictionary = dictionary;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Link"/> class.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        public Link(IImmutableDictionary<string, object> dictionary)
        {
            this.dictionary = dictionary;
        }

        /// <summary>
        /// Converts the link to a dictionary.
        /// </summary>
        /// <returns>The dictionary.</returns>
        public IImmutableDictionary<string, object> ToDictionary()
        {
            return dictionary;
        }

        /// <summary>
        /// Gets the href.
        /// </summary>
        /// <value>
        /// The href.
        /// </value>
        public Uri Href
        {
            get { return (Uri)dictionary[HREF]; }
        }

        /// <summary>
        /// Gets the rel.
        /// </summary>
        /// <value>
        /// The rel.
        /// </value>
        public string Rel
        {
            get { return (string)dictionary[REL]; }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name
        {
            get { return (string)dictionary[NAME]; }
        }

        /// <summary>
        /// Gets the prompt.
        /// </summary>
        /// <value>
        /// The prompt.
        /// </value>
        public string Prompt
        {
            get { return (string)dictionary[PROMPT]; }
        }

        /// <summary>
        /// Gets the render semantic
        /// </summary>
        /// <value>
        /// The render.
        /// </value>
        public string Render
        {
            get { return (string)dictionary[RENDER]; }
        }
    }
}