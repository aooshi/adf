using System;
using System.Net;

namespace Adf
{
    /// <summary>
    /// http server handler
    /// </summary>
    public interface IHttpServerHandler
    {
        /// <summary>
        /// process
        /// </summary>
        /// <param name="httpContext"></param>
        void Process(HttpServerContext httpContext);
    }
}
