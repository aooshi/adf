using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Adf
{
    /// <summary>
    /// HTTP Server 文件接收器
    /// </summary>
    public interface IHttpServerFileHandler
    {
        /// <summary>
        /// 创建内容接收参数
        /// </summary>
        /// <param name="name">上传窗值设置的值，一般为 input.name 值</param>
        /// <param name="fileName">文件在客户端上的名称</param>
        /// <param name="httpContext">请求上下文</param>
        /// <param name="contentType">文件类型</param>
        /// <returns></returns>
        HttpServerFileParameter Create(string name, string fileName,string contentType, HttpServerContext httpContext);
    }
}
