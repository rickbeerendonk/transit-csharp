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

namespace NForza.Transit.Impl.WriteHandlers
{
    internal class DoubleWriteHandler : AbstractWriteHandler
    {
        public override bool CanWrite(object obj)
        {
            return obj is Double;
        }

        public override string Tag(object obj) {

            Double d = (Double)obj;

            if (Double.IsNaN(d) || Double.IsInfinity(d))
            {
                return "z";
            }
            else
            {
                return "d";
            }
        }

        public override object Representation(object obj) {

            Double d = System.Convert.ToDouble(obj);

            if (Double.IsNaN(d))
            {
                return "NaN";
            } else if (Double.IsPositiveInfinity(d)) {
                return "INF";
            } else if (Double.IsNegativeInfinity(d)) {
                return "-INF";
            } else {
                return d;
            }
        }

        public override string StringRepresentation(object obj) {
            return this.Representation(obj).ToString();
        }
    }
}
