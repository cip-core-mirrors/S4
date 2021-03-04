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
using ABSA.RD.S4.S3Proxy.Proxy;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace ABSA.RD.S4.S3ProxyHost
{
    class ProxyWorker
    {
        private readonly IProxy _proxy;

        public ProxyWorker(IProxy proxy)
        {
            _proxy = proxy;
        }

        public async Task Run(HttpContext context)
        {
            var response = await _proxy.MakeRequest(context.Request);

            context.Response.ContentType = response.ContentType;

            if (response.Headers != null)
                foreach (var header in response.Headers)
                    context.Response.Headers[header.Key] = header.Value;

            if (response.Body != null)
                await context.Response.BodyWriter.WriteAsync(response.Body);
        }
    }
}