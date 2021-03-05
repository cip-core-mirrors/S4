//   Copyright 2021 Absa Group
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace ABSA.RD.S4.S3Proxy.Misc
{
    public static class HttpRequestExtensions
    {
        public static string GetValue(this IQueryCollection query, string key) => query.TryGetValue(key, out var values) ? values.Single() : default;

        public static string GetValue(this IHeaderDictionary headers, string key) => headers.TryGetValue(key, out var values) ? values.Single() : default;
    }
}
