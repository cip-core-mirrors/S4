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