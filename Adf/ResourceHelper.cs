using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Drawing;

namespace Adf
{
    /// <summary>
    /// 资源助手
    /// </summary>
    public static class ResourceHelper
    {
        /// <summary>
        /// 获取当前程序集文本资源
        /// </summary>
        /// <param name="name">项目名称.文件名/项目名称.目录名.文件名</param>
        /// <returns></returns>
        public static string GetText(string name)
        {
            return GetText(name, Assembly.GetCallingAssembly());
        }

        /// <summary>
        /// 获取指定程序集文本资源
        /// </summary>
        /// <param name="name">项目名称.文件名/项目名称.目录名.文件名</param>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static string GetText(string name,Assembly assembly)
        {
            using (System.IO.Stream stream = assembly.GetManifestResourceStream(name))
            {
                if (stream == null)
                    throw new KeyNotFoundException("not found resouce," + name);

                using (StreamReader sr = new StreamReader(stream))
                {
                    return sr.ReadToEnd();
                }
            }
        }
        
        /// <summary>
        /// 获取当前程序集资源字节数组
        /// </summary>
        /// <param name="name">项目名称.文件名/项目名称.目录名.文件名</param>
        /// <returns></returns>
        public static byte[] GetBytes(string name)
        {
            return GetBytes(name, Assembly.GetCallingAssembly());
        }

        /// <summary>
        /// 获取当前程序集资源字节数组
        /// </summary>
        /// <param name="name">项目名称.文件名/项目名称.目录名.文件名</param>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static byte[] GetBytes(string name, Assembly assembly)
        {
            using (System.IO.Stream stream = assembly.GetManifestResourceStream(name))
            {
                if (stream == null)
                    throw new KeyNotFoundException("not found resouce," + name);

                using(var br = new BinaryReader(stream))
                {
                    return br.ReadBytes((int)stream.Length);
                }
            }
        }

    }
}
