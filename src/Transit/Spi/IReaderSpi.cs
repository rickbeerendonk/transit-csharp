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

namespace Beerendonk.Transit.Spi
{
    /// <summary>
    ///  Interface for providing custom <see cref="IDictionaryReader"/> and 
    ///  <see cref="IListReader"/> implementations for a Reader to use when parsing 
    ///  native JSON or MessagePack composite structures. This entry point exists to enable 
    ///  Transit libraries for other .NET languages to layer on top of the C# Transit library,
    ///  but still get language-appropriate dictionaries and lists returned from a parse, 
    ///  while ensuring that parsing and decoding work correctly. This interface should never 
    ///  be used by applications that using this library.
    /// </summary>
    public interface IReaderSpi
    {
        /// <summary>
        /// Specifies a custom <see cref="IDictionaryReader"/> and
        /// <see cref="IListReader"/> to use when parsing native dictionary and lists
        /// in JSON or MessagePack. Implementations must accept any type of input and must return
        /// dictionaries or lists of any type of content. This function must be called before
        /// Reader.Read is called.
        /// </summary>
        /// <param name="dictionaryBuilder">
        /// A custom <see cref="IDictionaryReader"/> that produces a dictionary of 
        /// objects to objects
        /// </param>
        /// <param name="listBuilder">
        /// A custom <see cref="IListReader"/> that yields a list of objects.
        /// </param>
        /// <returns>A reader</returns>
        IReader SetBuilders(IDictionaryReader dictionaryBuilder, IListReader listBuilder);
    }
}