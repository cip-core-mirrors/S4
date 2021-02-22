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
