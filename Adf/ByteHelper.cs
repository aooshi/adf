using System;
using System.Collections.Generic;

using System.Text;

namespace Adf
{
    /// <summary>
    /// 字节助手
    /// </summary>
    public class ByteHelper
    {
        /// <summary>
        /// 格式化字节数字符串为K/M/G
        /// </summary>
        /// <param name="bytes">字节数</param>
        public static string FormatBytes(int bytes)
        {
            if (bytes > 1073741824)
            {
                return ((double)(bytes / 1073741824)).ToString("0") + "G";
            }
            if (bytes > 1048576)
            {
                return ((double)(bytes / 1048576)).ToString("0") + "M";
            }
            if (bytes > 1024)
            {
                return ((double)(bytes / 1024)).ToString("0") + "K";
            }
            return bytes.ToString() + "Bytes";
        }
    }
}
