using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Adf
{
    /// <summary>
    /// 字符串扩展
    /// </summary>
    public static class StringHelper
    {
        /// <summary>
        /// 换行符常量
        /// </summary>
        public const string CRLF = "\r\n";

        /// <summary>
        /// 文本分页
        /// </summary>
        /// <param name="text"></param>
        /// <param name="pageToken"></param>
        /// <returns></returns>
        public static List<string> Page(string text, string pageToken)
        {
            int foundPos = text.IndexOf(pageToken);
            if (foundPos >= 0)
            {
                var pages = new List<string>(10);
                while (foundPos >= 0)
                {
                    string pageString = text.Substring(0, foundPos);
                    if (pageString.Trim().Length != 0)
                    {
                        pages.Add(pageString);
                    }
                    text = text.Remove(0, pageString.Length + pageToken.Length);
                    foundPos = text.IndexOf(pageToken);
                }
                if (text.Length > 0)
                {
                    pages.Add(text);
                }
                return pages;
            }
            return null;
        }

        /// <summary>
        /// 以分隔符拆分字符串
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="itemMaxLength">单字符串最大长度</param>
        /// <param name="splitChar">拆分符</param>
        /// <returns></returns>
        public static string[] Compart(string ids, int itemMaxLength, char splitChar)
        {
            var idArray = new List<string>();
            var item = string.Empty;
            var index = 0;
            if (string.IsNullOrEmpty(ids))
            {
                return idArray.ToArray();
            }
            while (ids.Length > itemMaxLength)
            {
                if (ids[0] == splitChar)
                {
                    ids = ids.Substring(1);
                    continue;
                }
                if (ids[itemMaxLength] == splitChar)
                {
                    item = ids.Substring(0, itemMaxLength);
                    ids = ids.Substring(itemMaxLength + 1);
                }
                else
                {
                    item = ids.Substring(0, itemMaxLength);
                    index = item.LastIndexOf(splitChar);
                    if (index < 0)
                    {
                        throw new ArgumentOutOfRangeException("itemMaxLength", "值过小");
                    }
                    item = item.Substring(0, index);
                    ids = ids.Substring(index);
                }
                idArray.Add(item);
            }
            if (!string.IsNullOrEmpty(ids) && ids[0] == splitChar)
            {
                ids = ids.Substring(1);
            }
            if (!string.IsNullOrEmpty(ids))
            {
                idArray.Add(ids);
            }
            return idArray.ToArray();
        }

        /// <summary>
        /// 取得字符串多字节长度
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static int GetMultiByteLength(string input)
        {
            if (string.IsNullOrEmpty(input))
                return 0;

            int len = 0;

            foreach (char chr in input)
                len += (int)chr > 127 ? 2 : 1;

            return len;
        }

        /// <summary>
        /// 取得传入字符串中指定多字节长度的字符串
        /// </summary>
        /// <param name="input">要进行截取的字符串</param>
        /// <param name="len">字符长度,多字节字符按2个字符计算</param>
        /// <param name="append">须在其尾增加的字符串，当为Null或Empty时则不进行添加</param>
        /// <returns>返回截取后的字符串</returns>
        /// <example>一个中文按两个单位计算</example>
        public static string Sub(string input, int len, string append = null)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            int i = 0, j = 0;
            int al = string.IsNullOrEmpty(append) ? 0 : GetMultiByteLength(append);
            int cutlen = len - al;
            if (cutlen < 1)
                cutlen = len;

            foreach (char chr in input)
            {
                if ((int)chr > 127)
                    i += 2;
                else
                    i++;
                if (i > cutlen)
                {
                    input = input.Substring(0, j);
                    if (al > 0)
                        input += append;
                    break;
                }
                j++;
            }

            return input;
        }

        /// <summary>
        /// 串连
        /// </summary>
        /// <param name="splitChar"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string Join(string splitChar, params object[] args)
        {
            if (args.Length == 0)
                return string.Empty;

            var build = new StringBuilder();
            var first = true;
            foreach (var item in args)
            {
                if (first)
                    first = false;
                else
                    build.Append(splitChar);
                build.Append(item);
            }
            return build.ToString();
        }

        /// <summary>
        /// 判断字符串compare 在 input字符串中出现的次数
        /// </summary>
        /// <param name="input">源字符串</param>
        /// <param name="compare">用于比较的字符串</param>
        /// <returns>字符串compare 在 input字符串中出现的次数</returns>
        public static int Count(string input, string compare)
        {
            int index = input.IndexOf(compare);
            if (index != -1)
            {
                return 1 + Count(input.Substring(index + compare.Length), compare);
            }
            return 0;
        }

        static Regex REMOVE_HTMLTAG_REGEX = new Regex("<.+?>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        /// <summary>
        /// 过滤掉html数据标记
        /// </summary>
        /// <param name="input">过滤信息</param>
        public static string RemoveHtmlTag(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            input = REMOVE_HTMLTAG_REGEX.Replace(input, string.Empty);

            //input = input.Replace("<","");
            //input = input.Replace(">", "");
            return input;
        }

        static Regex REMOVE_CRLF_REGEX = new Regex(@"(\r\n)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        /// <summary>
        /// 清除给定字符串中的回车及换行符
        /// </summary>
        /// <param name="input">要清除的字符串</param>
        /// <returns>清除后返回的字符串</returns>
        public static string RemoveCrLf(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            Match m = null;

            for (m = REMOVE_CRLF_REGEX.Match(input); m.Success; m = m.NextMatch())
            {
                input = input.Replace(m.Groups[0].ToString(), string.Empty);
            }


            return input;
        }

        /// <summary>
        /// 将字符串进行Html简单编码，空格、大小于、换行、双引号、单引号、正反括号
        /// </summary>
        /// <param name="input">要进行编码的字符串</param>
        /// <returns>返回编码后的字符串</returns>
        public static string EncodeHtmlTag(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            input = input.Replace(" ", "&nbsp;");
            input = input.Replace("<", "&lt;");
            input = input.Replace(">", "&gt;");
            input = input.Replace("\n", "<br />");
            input = input.Replace("\"", "&quot;");  //双引号
            input = input.Replace("'", "&#39;");    //单引号
            input = input.Replace("(", "&#40;");
            input = input.Replace(")", "&#41;");

            return input;
        }

        /// <summary>
        /// 将字符串进行Html解码，是<see cref="EncodeHtmlTag"/>的反编辑
        /// </summary>
        /// <param name="input">要进行解码的字符串</param>
        /// <returns>返回解码后的字符串</returns>
        public static string DecodeHtmlTag(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            input = input.Replace("&nbsp;", " ");
            input = input.Replace("&lt;", "<");
            input = input.Replace("&gt;", ">");
            input = input.Replace("<br />", "\n");
            input = input.Replace("&quot;", "\""); //双引号
            input = input.Replace("&#39;", "'"); //单引号
            input = input.Replace("&#40;", "(");
            input = input.Replace("&#41;", ")");

            return input;
        }

        /// <summary>
        /// 对字符串中的大于号及小于号进行Html解码
        /// </summary>
        /// <param name="input">要进行解辑的字符串</param>
        /// <returns>返回解码后的字符串</returns>
        public static string DecodeHtmlTagChar(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            input = input.Replace("&gt;", ">");
            input = input.Replace("&lt;", "<");
            return input;
        }


        /// <summary>
        /// 对字符串中的大于号及小于号进行Html编码
        /// </summary>
        /// <param name="input">要进行编辑的字符串</param>
        /// <returns>返回加码后的字符串</returns>
        public static string EncodeHtmlTagChar(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            input = input.Replace(">", "&gt;");
            input = input.Replace("<", "&lt;");

            return input;
        }

        /// <summary>
        /// 以指定的字典数据格式化字符串，未匹配的内容原文输出
        /// </summary>
        /// <param name="input">要格式化的字符串，示例： my name is {name}, and my birthday {birthday} </param>
        /// <param name="dictionary">格式化内容字典， 名称是否区分大小写由输入字典决定</param>
        /// <exception cref="ArgumentNullException">dictionary is null</exception>
        /// <exception cref="FormatException">throwError  is true, no match replace item</exception>
        /// <returns></returns>
        public static string Format(string input, System.Collections.IDictionary dictionary)
        {
            return Format(input, dictionary, false);
        }

        /// <summary>
        /// 以指定的字典数据格式化字符串
        /// </summary>
        /// <param name="input">要格式化的字符串，示例： my name is {name}, and my birthday {birthday} </param>
        /// <param name="dictionary">格式化内容字典， 名称是否区分大小写由输入字典决定</param>
        /// <param name="throwError">未找到匹配时，是否抛出异常</param>
        /// <exception cref="ArgumentNullException">dictionary is null</exception>
        /// <exception cref="FormatException">throwError  is true, no match replace item</exception>
        /// <returns></returns>
        public static string Format(string input, System.Collections.IDictionary dictionary, bool throwError)
        {
            if (input == null)
                return input;

            if (input == "")
                return input;

            if (dictionary == null)
                throw new ArgumentNullException("dictionary");

            var chars = input.ToCharArray();
            var length = chars.Length;

            var build = new StringBuilder();
            var index = 0;
            char chr = char.MinValue;
            string name = null;
            object value = null;

            while (true)
            {
                chr = chars[index];
                index++;

                //in start
                if (chr == '{' && index < length)
                {
                    if (chars[index] == '{')
                    {
                        build.Append('{');
                        index++;
                    }
                    else
                    {
                        name = null;
                        value = null;
                        if (FormatParseName(chars, ref index, out name, throwError))
                        {
                            try
                            {
                                value = dictionary[name];
                            }
                            catch (Exception)
                            {
                                if (throwError)
                                {
                                    throw new FormatException("format " + name + " error");
                                }
                                else
                                {
                                    value = '{' + name + '}';
                                }
                            }
                        }
                        build.Append(value);
                    }
                }
                else
                {
                    build.Append(chr);
                }

                if (index == length)
                {
                    break;
                }
            }

            return build.ToString();
        }

        private static bool FormatParseName(char[] chars, ref int index, out string value, bool throwError)
        {
            var build = new StringBuilder();
            int length = chars.Length;
            char chr = char.MinValue;
            bool matched = false;
            while (true)
            {
                chr = chars[index];
                index++;

                //in start
                if (chr == '}')
                {
                    if (index == length)
                    {
                        //is end
                        matched = true;
                        break;
                    }
                    else if (index < length && chars[index] == '}')
                    {
                        build.Append('}');
                        index++;
                    }
                    else
                    {
                        matched = true;
                        break;
                    }
                }
                else
                {
                    build.Append(chr);
                }

                if (index == length)
                {
                    break;
                }
            }

            value = build.ToString();
            return matched;
        }
    }
}