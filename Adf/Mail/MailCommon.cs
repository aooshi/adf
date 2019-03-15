using System;
using System.Text;
using System.Collections.Generic;
using System.Collections;

namespace Adf.Mail
{
    /// <summary>
    /// 邮件助手
    /// </summary>
    public static class MailCommon
    {
        /// <summary>
        /// 表示一行
        /// </summary>
        public static readonly string NewLine = "\r\n";

        /// <summary>
        /// MIME
        /// </summary>
        public static readonly string MimeVersion = "MIME-Version: 1.0";

        /// <summary>
        /// Mailer
        /// </summary>
        public static readonly string Mailer = "X-Mailer: http://www.aooshi.org/adf/";

        /// <summary>
        /// 进行BASE64头的编码
        /// </summary>
        /// <param name="input">要进行编码的串</param>
        /// <param name="encoding">编码类型</param>
        /// <returns>返回编码后的串</returns>
        public static string Base64EncodHead(string input, Encoding encoding)
        {
            //return "=?" + charset + "?B?" + MailCommon.Base64Encode(input, encoding) + "?=";

            input = Convert.ToBase64String(encoding.GetBytes(input));

            return "=?" + encoding.HeaderName + "?B?" + input + "?=";
        }

        /// <summary>
        /// 将指定的字符串进行76字符行短行
        /// </summary>
        /// <param name="input">要进行处理的串</param>
        /// <returns>返回字符串形式的Base64编码后串</returns>
        public static string Line76Break(string input)
        {
            //满足邮件要求，进行每行最大 76 字符的处理
            if (input.Length < 77)
            {
                return input;
            }

            StringBuilder build = new StringBuilder();

            long maxFor = input.Length - (input.Length % 76);  //取得余数

            //须要循环的数
            int pos = 0;
            for (; pos < maxFor; pos += 76)
            {
                build.Append(input.Substring(pos, 76)); //取得76个字符
                build.Append(MailCommon.NewLine);//增加换行符
            }

            //增加最后数据
            if (pos < input.Length)
            {
                build.Append(input.Substring(pos));
            }

            return build.ToString();
        }

        /// <summary>
        /// 将指定的字符串进行76字符行短行
        /// </summary>
        /// <param name="input">要进行处理的串</param>
        /// <param name="lineStart">每行开始字符</param>
        /// <returns>返回字符串形式的Base64编码后串</returns>
        public static string Line76Break(string input, string lineStart)
        {
            //满足邮件要求，进行每行最大 76 字符的处理
            if (input.Length < 77)
            {
                return input;
            }
            if (lineStart.Length > 76)
            {
                throw new ArgumentOutOfRangeException("lineStart", "length limit less than 76.");
            }

            var lineStartLength = lineStart.Length;
            var inputLength = input.Length;
            var lineMaxLength = 76 - lineStartLength;
            var readAll = 0;
            var read = 0;
            var buffer = new Char[76];
            lineStart.CopyTo(0, buffer, 0, lineStartLength);
            //
            StringBuilder build = new StringBuilder();
            using (var reader = new System.IO.StringReader(input))
            {
                while (readAll < inputLength)
                {
                    read = reader.ReadBlock(buffer, lineStartLength, lineMaxLength);
                    readAll += read;

                    build.Append(buffer, 0, lineStartLength + read);
                    build.Append(MailCommon.NewLine);
                }

                //remove last newlist
                build.Length -= MailCommon.NewLine.Length;
            }

            //long maxFor = input.Length - (input.Length % 76);  //取得余数

            ////须要循环的数
            //int pos = 0;
            //for (; pos < maxFor; pos += 76)
            //{
            //    build.Append(input.Substring(pos, 76)); //取得76个字符
            //    build.Append(MailCommon.NewLine);//增加换行符
            //}

            ////增加最后数据
            //if (pos < input.Length)
            //{
            //    build.Append(input.Substring(pos));
            //}

            return build.ToString();
        }

        ///// <summary>
        ///// 将一个数组合并成符合邮件的每行最大76字符字符串，结果追加至字符构建对象
        ///// </summary>
        ///// <param name="list"></param>
        ///// <param name="builder"></param>
        ///// <param name="lineStart">每行初始字符</param>
        //public static void JoinLine76Break(StringBuilder builder, IList<string> list, string lineStart)
        //{
        //    var lineLength = builder.Length;
        //    var lineStartLength = lineStart.Length;

        //    if (lineStartLength > 76)
        //    {
        //        throw new ArgumentOutOfRangeException("lineStart", "length limit less than 76.");
        //    }

        //    for (int i = 0, l = list.Count; i < l; i++)
        //    {
        //        var itemLength = list[i].Length;
        //        if (lineLength + itemLength > 76)
        //        {
        //            builder.Append(MailCommon.NewLine);
        //            builder.Append(lineStart);

        //            lineLength = itemLength + lineStartLength;
        //        }
        //        else
        //        {
        //            lineLength += itemLength;
        //        }
        //        builder.Append(list[i]);
        //    }
        //}

        /// <summary>
        /// 编码邮件地址头
        /// </summary>
        /// <param name="name"></param>
        /// <param name="address"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string Base64EncodeAddress(string name, string address, Encoding encoding)
        {
            //判断姓名是否为空
            if (string.IsNullOrEmpty(name))
            {
                name = address.Split('@')[0];
            }

            //判断姓名是否为键盘可打印，如果是，则不进行编码，如果不是，则进行编码
            if (MailCommon.IsAscii(name) == false)
            {
                name = MailCommon.Base64EncodHead(name, encoding);
            }

            return "\"" + name + "\" <" + address + ">";
        }

        ///// <summary>
        ///// 构建邮件分隔符
        ///// </summary>
        ///// <param name="dt"></param>
        ///// <param name="messageId"></param>
        ///// <returns></returns>
        //public static string BuildBoundary(DateTime dt, string messageId)
        //{
        //    string id = "";
        //    id += dt.ToUniversalTime().Ticks.ToString("x");
        //    id += ".";
        //    id += Guid.NewGuid().ToString("N");
        //    return id;
        //}

        /// <summary>
        /// 判断是否为键盘字符
        /// </summary>
        /// <param name="input">要进行判断的字符串</param>
        /// <returns>返回Bool值表示是否为键盘字符</returns>
        public static bool IsAscii(string input)
        {
            char[] cs = input.ToCharArray();

            foreach (char c in cs)
                if (c > 126 || c < 32)
                    return false;

            return true;
        }

        /// <summary>
        /// 创建用于邮件体的邮件地址集合的数组
        /// </summary>
        /// <param name="addressList">须要处理的集合</param>
        /// <param name="encoding"></param>
        /// <returns>返回字符串型式数组</returns>
        public static string JoinAddressList(List<MailAddress> addressList, Encoding encoding)
        {
            var array = new string[addressList.Count];

            for (int i = 0, l = addressList.Count; i < l; i++)
            {
                var address = addressList[i];
                array[i] = MailCommon.Base64EncodeAddress(address.Name, address.Address, encoding);
            }

            var result = string.Join(",", array);
            return result;
        }

        /// <summary>
        /// 构键消息标识
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string BuildMessageID(DateTime dt)
        {
            string time = dt.ToUniversalTime().Ticks.ToString("x");
            string uuid = Guid.NewGuid().ToString("N");

            return string.Concat(time, 'z', uuid);

            //return Guid.NewGuid().ToString("N");
        }
    }
}