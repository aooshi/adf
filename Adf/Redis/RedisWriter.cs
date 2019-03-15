using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Adf
{
    /// <summary>
    /// 命令写入工具
    /// </summary>
    public class RedisWriter : MemoryStream
    {
        RedisClient client;
        /// <summary>
        /// 初始化新实例
        /// </summary>
        /// <param name="client"></param>
        /// <param name="command"></param>
        /// <param name="argumentSize">参数总数</param>
        public RedisWriter(RedisClient client, int argumentSize, string command)
            : base(256)
        {
            this.client = client;
            
            //writer command
            var buffer = client.Encoding.GetBytes(string.Format("*{0}\r\n${1}\r\n{2}\r\n",argumentSize,command.Length,command));
            //to io
            this.Write(buffer,0,buffer.Length);
        }
        /// <summary>
        /// 写入一个参数
        /// </summary>
        /// <param name="data"></param>
        public void WriteArgument(string data)
        {
            this.WriteArgument( this.client.Encoding.GetBytes(data) );
        }
        /// <summary>
        /// 写入一个参数
        /// </summary>
        /// <param name="data"></param>
        public void WriteArgument(byte[] data)
        {
            byte[] buffer;
            //len
            buffer = this.client.Encoding.GetBytes(string.Concat("$",data.Length, "\r\n"));
            base.Write(buffer, 0, buffer.Length);
            //data
            base.Write(data,0,data.Length);
            base.Write(RedisConnection.CRLF, 0, RedisConnection.CRLF.Length);
        }
    }
}
