using System;
using System.Reflection;
using System.Net;

namespace Adf
{
    /// <summary>
    /// http server route handler
    /// </summary>
    public class HttpServerRouteHandler : IHttpServerHandler
    {
        PathRoute<IHttpServerHandler> pathRoute;

        /// <summary>
        /// initialize new instance
        /// </summary>
        public HttpServerRouteHandler()
        {
            var thisType = this.GetType();
            this.pathRoute = new PathRoute<IHttpServerHandler>(thisType.Assembly, thisType.Namespace + ".HttpHandlers");
        }

        /// <summary>
        /// initialize action instance
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="handlersNamespace"></param>
        public HttpServerRouteHandler(Assembly assembly, string handlersNamespace)
        {
            this.pathRoute = new PathRoute<IHttpServerHandler>(assembly, handlersNamespace);
        }

        /// <summary>
        /// not found action
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        protected virtual HttpStatusCode NotFoundAction(HttpServerContext httpContext)
        {
            httpContext.Content = "Not Found";
            return HttpStatusCode.NotFound;
        }

        /// <summary>
        /// default action
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        protected virtual HttpStatusCode DefaultAction(HttpServerContext httpContext)
        {
            httpContext.Content = "Not Found";
            return HttpStatusCode.NotFound;
        }

        /// <summary>
        /// process
        /// </summary>
        /// <param name="httpContext"></param>
        /// <exception cref="ArgumentNullException">path is null</exception>
        /// <exception cref="ArgumentException">path only allow contain a-z0-9</exception>
        /// <returns></returns>
        public virtual void Process(HttpServerContext httpContext)
        {
            if (httpContext.Path == "/")
            {
                this.DefaultAction(httpContext);
            }
            else
            {
                var handler = this.pathRoute.GetInstance(httpContext.Path);
                if (handler == null)
                {
                    this.NotFoundAction(httpContext);
                }
                else
                {
                    handler.Process(httpContext);
                }
            }
        }
    }
}