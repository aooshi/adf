using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Adf
{
    /// <summary>
    /// HTTP 文件参数
    /// </summary>
    public class HttpServerFileParameter
    {
        /// <summary>
        /// 字段名称
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName
        {
            get;
            private set;
        }

        /// <summary>
        /// 文件类型
        /// </summary>
        public string ContentType
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取当前文件流对象
        /// </summary>
        public Stream Stream
        {
            get;
            private set;
        }

        /// <summary>
        /// 妆始化新实例
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fileName"></param>
        /// <param name="contentType"></param>
        public HttpServerFileParameter(string name, string fileName, string contentType)
        {
            this.Name = name;
            this.FileName = fileName;
            this.ContentType = contentType;
            this.Stream = this.CreateStream();
        }

        /// <summary>
        /// 创建文件接收流
        /// </summary>
        /// <returns></returns>
        protected virtual System.IO.Stream CreateStream()
        {
            return new MemoryStream();
        }

        /// <summary>
        /// 将数据存储至文件
        /// </summary>
        /// <param name="savePath"></param>
        public void Save(string savePath)
        {
            var stream = this.Stream;
            stream.Position = 0;
            using (var fs = new FileStream(savePath, FileMode.Create))
            {
                var read = 0;
                var readall = 0;
                byte[] buffer = new byte[4096];
                while (true)
                {
                    read = stream.Read(buffer, 0, 4096);
                    if (read == 0)
                    {
                        break;
                    }
                    readall += read;
                    fs.Write(buffer, 0, read);
                }
            }
            stream.Position = 0;
        }

        /// <summary>
        /// class destroy
        /// </summary>
        ~HttpServerFileParameter()
        {
            StreamHelper.TryClose(this.Stream);
        }
    }
}