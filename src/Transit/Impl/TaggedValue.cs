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
    public class TaggedValue : ITaggedValue 
    {
        private readonly string tag;
        private readonly object representation;

        public TaggedValue(string tag, object representation)
        {
            this.tag = tag;
            this.representation = representation;
        }

        public string GetTag() 
        {
            return tag;
        }

        public object GetRepresentation() 
        {
            return representation;
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }

            if (!(obj is TaggedValue))
            {
                return false;
            }

            var other = (TaggedValue)obj;
            return (this.tag.Equals(other.GetTag()) && 
                this.representation.Equals(other.GetRepresentation()));
        }

        public override int GetHashCode()
        {
            int result = 17;
            result = 31 * result * tag.GetHashCode();
            result = 31 * result * representation.GetHashCode();

            return result;
        }
    }
}