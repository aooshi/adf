using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Net;

namespace Adf
{
    /// <summary>
    /// Http Server Action
    /// </summary>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    public delegate void HttpServerAction(HttpServerContext httpContext);

    /// <summary>
    /// http server action route
    /// </summary>
    public class HttpServerActionHandler : IHttpServerHandler
    {
        Dictionary<string, HttpServerAction> actionDictionary = new Dictionary<string, HttpServerAction>(StringComparer.OrdinalIgnoreCase);
        object actionInstance;

        /// <summary>
        /// initialize new instance
        /// </summary>
        public HttpServerActionHandler()
        {
            this.actionInstance = this;
        }

        /// <summary>
        /// initialize action instance
        /// </summary>
        /// <param name="actionInstance"></param>
        public HttpServerActionHandler(object actionInstance)
        {
            this.actionInstance = actionInstance;
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
        /// get http action
        /// </summary>
        /// <param name="action"></param>
        /// <returns>not find is null</returns>
        protected virtual HttpServerAction GetAction(string action)
        {
            for (int i = 0, l = action.Length; i < l; i++)
            {
                if (action[i] > 47 && action[i] < 58)
                {
                    //0-9
                }
                else if (action[i] > 64 && action[i] < 91)
                {
                    //A-Z
                }
                else if (action[i] > 96 && action[i] < 123)
                {
                    //a-z
                }
                else
                {
                    //throw new ArgumentException("action only allow contain a-z0-9");
                    return null;
                }
            }

            //
            HttpServerAction objAction;
            if (this.actionDictionary.TryGetValue(action, out objAction) == false)
            {
                try
                {
                    objAction = (HttpServerAction)Delegate.CreateDelegate(typeof(HttpServerAction), this.actionInstance, action, true);
                }
                catch (Exception)
                {
                    objAction = null;
                }

                lock (this.actionDictionary)
                {
                    if (actionDictionary.ContainsKey(action) == false)
                    {
                        actionDictionary.Add(action, objAction);
                    }
                    else
                    {
                        objAction = actionDictionary[action];
                    }
                }
            }
            return objAction;
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
            var action = httpContext.QueryString["action"];
            if (action == null || action == "")
            {
                this.DefaultAction(httpContext);
            }
            else
            {
                var objAction = this.GetAction(action);
                if (objAction == null)
                {
                    this.NotFoundAction(httpContext);
                }
                else
                {
                    objAction(httpContext);
                }
            }
        }
    }
}