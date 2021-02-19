using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace ABSA.RD.S4.S3Proxy.Proxy
{
    public interface IProxy
    {
        Task<ProxyResponse> MakeRequest(HttpRequest original);
    }
}