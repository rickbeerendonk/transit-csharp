# transit-csharp

[![Build status](http://img.shields.io/appveyor/ci/rickbeerendonk/transit-csharp.svg)](https://ci.appveyor.com/project/rickbeerendonk/transit-csharp/branch/master)
[![NuGet version](http://img.shields.io/nuget/v/Transit.svg)](https://www.nuget.org/packages/Transit)

Transit is a data format and a set of libraries for conveying values between applications written in different languages. This library provides support for marshalling Transit data to/from C#.

* [Rationale](http://blog.cognitect.com/blog/2014/7/22/transit)
* [API docs](http://rickbeerendonk.github.io/transit-csharp/)
* [Specification](http://github.com/cognitect/transit-format)

This implementation's major.minor version number corresponds to the version of the Transit specification it supports.

JSON and JSON-Verbose are implemented, but more tests need to be written.
MessagePack is **not** implemented yet. 

_NOTE: Transit is a work in progress and may evolve based on feedback. As a result, while Transit is a great option for transferring data between applications, it should not yet be used for storing data durably over time. This recommendation will change when the specification is complete._

## Releases and Dependency Information

* Latest release: [![NuGet version](http://img.shields.io/nuget/v/Transit.svg)](https://www.nuget.org/packages/Transit)

This is a Portable Class Library with the following targets:

* .NET Framework 4.5
* Windows 8
* Windows Phone 8.1

## Usage

## Default Type Mapping

|Transit type|Write accepts|Read returns|
|------------|-------------|------------|
|null|null|null|
|string|System.String|System.String|
|boolean|System.Boolean|System.Boolean|
|integer|System.Byte, System.Int16, System.Int32, System.Int64|System.Int64|
|decimal|System.Single, System.Double|System.Double|
|keyword|Beerendonk.Transit.IKeyword|Beerendonk.Transit.IKeyword|
|symbol|Beerendonk.Transit.ISymbol|Beerendonk.Transit.ISymbol|
|big decimal|_not implemented_|Beerendonk.Transit.Numerics.BigRational|
|big integer|System.Numerics.BigInteger|System.Numerics.BigInteger|
|time|System.DateTime|System.DateTime|
|uri|System.Uri|System.Uri|
|uuid|System.Guid|System.Guid|
|char|System.Char|System.Char|
|array|T[], System.Collections.Generic.IList<>|System.Collections.Generic.IList<object>|
|list|System.Collections.Generic.IEnumerable<>|System.Collections.Generic.IEnumerable<object>|
|set|System.Collections.Generic.ISet<>|System.Collections.Generic.ISet<object>|
|map|System.Collections.Generic.IDictionary<,>|System.Collections.Generic.IDictionary<object, object>|
|link|Beerendonk.Transit.ILink|Beerendonk.Transit.ILink|
|ratio +|Beerendonk.Transit.IRatio|Beerendonk.Transit.IRatio|

\+ Extension type

## Layered Implementations

## Copyright and License
Copyright © 2014 Rick Beerendonk.

This library is a C# port of the Java version created and maintained by Cognitect, therefore

Copyright © 2014 Cognitect

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
http://www.apache.org/licenses/LICENSE-2.0
Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
