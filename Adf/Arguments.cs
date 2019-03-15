using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Adf
{
    /// <summary>
    /// 参数
    /// </summary>
    public class Arguments
    {
        private StringBuilder build = new StringBuilder();

        /// <summary>
        /// 添加一个选项
        /// </summary>
        /// <param name="key"></param>
        public void AddOption(string key)
        {
            if (this.build.Length == 0)
            {
                this.build.AppendFormat("--{0}", key);
            }
            else
            {
                this.build.AppendFormat(" --{0}", key);
            }
        }

        /// <summary>
        /// 添加一个参数
        /// </summary>
        /// <param name="key"></param>
        public void Add(string key)
        {
            if (this.build.Length == 0)
            {
                this.build.AppendFormat("/{0}", key);
            }
            else
            {
                this.build.AppendFormat(" /{0}", key);
            }
        }

        /// <summary>
        /// 添加一个参数
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(string key, string value)
        {
            if (this.build.Length == 0)
            {
                this.build.AppendFormat("/{0}={1}", key, value);
            }
            else
            {
                this.build.AppendFormat(" /{0}={1}", key, value);
            }
        }
        
        /// <summary>
        /// 添加一个带符号包含的参数
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="quoteChar"></param>
        public void AddWithQuote(string key, string value, char quoteChar)
        {
            if (this.build.Length == 0)
            {
                this.build.AppendFormat("/{0}={1}{2}{1}", key, quoteChar, value);
            }
            else
            {
                this.build.AppendFormat(" /{0}={1}{2}{1}", key, quoteChar, value);
            }
        }

        /// <summary>
        /// Get Hash Code
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.build.GetHashCode();
        }

        /// <summary>
        /// 参数转换
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static Dictionary<string, string> Parse(string[] args)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            Regex spliterRegex = new Regex("^-{1,2}|^/|=|:", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Regex removerRegex = new Regex("^['\"]?(.*?)['\"]?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            string parameter = null;
            foreach (string text in args)
            {
                string[] parts = spliterRegex.Split(text, 3);
                switch (parts.Length)
                {
                    case 1:
                        if (parameter != null)
                        {
                            if (!dict.ContainsKey(parameter))
                            {
                                parts[0] = removerRegex.Replace(parts[0], "$1");
                                dict.Add(parameter, parts[0]);
                            }
                            parameter = null;
                        }
                        break;

                    case 2:
                        if ((parameter != null) && !dict.ContainsKey(parameter))
                        {
                            dict.Add(parameter, "true");
                        }
                        parameter = parts[1];
                        break;

                    case 3:
                        if ((parameter != null) && !dict.ContainsKey(parameter))
                        {
                            dict.Add(parameter, "true");
                        }
                        parameter = parts[1];
                        if (!dict.ContainsKey(parameter))
                        {
                            parts[2] = removerRegex.Replace(parts[2], "$1");
                            dict.Add(parameter, parts[2]);
                        }
                        parameter = null;
                        break;
                }
            }
            if ((parameter != null) && !dict.ContainsKey(parameter))
            {
                dict.Add(parameter, "true");
            }
            return dict;
        }

        /// <summary>
        /// 获取实例字符串形式
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.build.ToString();
        }
    }
}
