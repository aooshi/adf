using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace Adf
{
    /// <summary>
    /// HTTP 状态描述
    /// </summary>
    public static class HttpStatusHelper
    {
        static string[][] descriptionArray;

        static HttpStatusHelper()
        {
            string[][] strArray = new string[6][];
            strArray[1] = new string[] { "Continue", "Switching Protocols", "Processing" };
            strArray[2] = new string[] { "OK", "Created", "Accepted", "Non-Authoritative Information", "No Content", "Reset Content", "Partial Content", "Multi-Status" };
            strArray[3] = new string[] { "Multiple Choices", "Moved Permanently", "Found", "See Other", "Not Modified", "Use Proxy", string.Empty, "Temporary Redirect" };
            strArray[4] = new string[] { 
        "Bad Request", "Unauthorized", "Payment Required", "Forbidden", "Not Found", "Method Not Allowed", "Not Acceptable", "Proxy Authentication Required", "Request Timeout", "Conflict", "Gone", "Length Required", "Precondition Failed", "Request Entity Too Large", "Request-Uri Too Long", "Unsupported Media Type", 
        "Requested Range Not Satisfiable", "Expectation Failed", string.Empty, string.Empty, string.Empty, string.Empty, "Unprocessable Entity", "Locked", "Failed Dependency"
     };
            strArray[5] = new string[] { "Internal Server Error", "Not Implemented", "Bad Gateway", "Service Unavailable", "Gateway Timeout", "Http Version Not Supported", string.Empty, "Insufficient Storage" };

            descriptionArray = strArray;
        }
        
        /// <summary>
        /// 获取指定状态描述
        /// </summary>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        public static string GetStatusDescription(HttpStatusCode statusCode)
        {
            return GetStatusDescription((int)statusCode);
        }

        /// <summary>
        /// 获取指定状态描述
        /// </summary>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        private static string GetStatusDescription(int statusCode)
        {
            if ((statusCode >= 100) && (statusCode < 600))
            {
                int index = statusCode / 100;
                int num2 = statusCode % 100;
                if (num2 < descriptionArray[index].Length)
                {
                    return descriptionArray[index][num2];
                }
            }
            return string.Empty;
        }
    }
}