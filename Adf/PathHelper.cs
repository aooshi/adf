using System;
using System.Collections.Generic;

using System.Text;

namespace Adf
{
    /// <summary>
    /// 路径处理
    /// </summary>
    public static class PathHelper
    {
        const string SLASH_STRING = "\\";
        const string BACKSLASH_STRING = "/";
        
        static Dictionary<char, byte> invalidNameDictionary = CreateInvalidNameChars();
        static Dictionary<char, byte> invalidPathDictionary = CreateInvalidPathChars();
                
        private static Dictionary<char, byte> CreateInvalidPathChars()
        {
            var chars = System.IO.Path.GetInvalidPathChars();
            var dictionary = new Dictionary<char, byte>(chars.Length);
            foreach (var chr in chars)
            {
                dictionary.Add(chr, 0);
            }
            return dictionary;
        }

        private static Dictionary<char, byte> CreateInvalidNameChars()
        {
            var chars = System.IO.Path.GetInvalidFileNameChars();
            var dictionary = new Dictionary<char, byte>(chars.Length);
            foreach (var chr in chars)
            {
                dictionary.Add(chr, 0);
            }
            return dictionary;
        }

        /// <summary>
        /// 获取应用程序内文件或目录路径
        /// </summary>
        /// <param name="nameOrPath">文件名,目录或一个绝对路径, 若传入的是一个绝对路径，则将原样返回</param>
        /// <returns></returns>
        public static string GetApplicationFile(string nameOrPath)
        {
            if (string.IsNullOrEmpty(nameOrPath))
                throw new ArgumentNullException("nameOrPath");

            if (System.IO.Path.IsPathRooted(nameOrPath))
                return nameOrPath;

            //
            var path = ConfigHelper.PATH_APP_ROOT;
            var filepath = System.IO.Path.Combine(path, nameOrPath);
            //
            return filepath;
        }

        /// <summary>
        /// 是否为不允许的名称字符
        /// </summary>
        /// <param name="chr"></param>
        /// <returns></returns>
        public static bool IsInvalidNameChar(Char chr)
        {
            return invalidNameDictionary.ContainsKey(chr);
        }

        /// <summary>
        /// 是否为不允许的路径字符
        /// </summary>
        /// <param name="chr"></param>
        /// <returns></returns>
        public static bool IsInvalidPathChar(Char chr)
        {
            return invalidPathDictionary.ContainsKey(chr);
        }


        /// <summary>
        /// 检查文件名是否合法
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool CheckFileName(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            foreach (var chr in name.ToCharArray())
            {
                if (invalidNameDictionary.ContainsKey(chr))
                {
                    //throw new ArgumentOutOfRangeException("name", "name no allow contains char '" + chr + "'");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 检查路径是否合法
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool CheckFilePath(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            foreach (var chr in path.ToCharArray())
            {
                if (invalidPathDictionary.ContainsKey(chr))
                {
                    //throw new ArgumentOutOfRangeException("path", "path no allow contains char '" + chr + "'");
                    return false;
                }
            }

            return true;
        }


        /// <summary>
        /// 将指定的路径进行判断，如果最后未有斜杠\，如果未有则自动增加一个
        /// </summary>
        /// <param name="path">路径串</param>
        public static string EndSlash(string path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;
            return path.EndsWith(SLASH_STRING) ? path : string.Concat(path,SLASH_STRING);
        }

        /// <summary>
        /// 验证指定的路径最后一位是否为反斜扛 / ,如果不是则自动增加一个
        /// </summary>
        /// <param name="path">路径串</param>
        public static string EndBackSlash(string path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;
            return path.EndsWith(BACKSLASH_STRING) ? path : string.Concat(path , BACKSLASH_STRING);
        }

        ///// <summary>
        ///// 根据指定的数字创建一组数字目录，以每目录文件数量等级数为标准
        ///// </summary>
        ///// <param name="number">要进行处理的数字</param>
        //public static string NumberPath(long number)
        //{
        //    return NumberPath(number, 1000, true);
        //}

        ///// <summary>
        ///// 根据指定的数字创建一组数字目录，以每目录文件数量等级数为标准
        ///// </summary>
        ///// <param name="number">要进行处理的数字</param>
        ///// <param name="level">目录文件数等级,建议为1000</param>
        //public static string NumberPath(long number, int level)
        //{
        //    return NumberPath(number, level, true);
        //}

        ///// <summary>
        ///// 根据指定的数字创建一组数字目录，以每目录文件数量等级数为标准
        ///// </summary>
        ///// <param name="number">要进行处理的数字</param>
        ///// <param name="level">目录文件数等级,建议为1000</param>
        ///// <param name="containsbase">是否包含基数</param>
        //public static string NumberPath(long number, int level, bool containsbase)
        //{
        //    string result = "";
        //    if (containsbase) result += number.ToString();

        //    while (number > level)
        //    {
        //        number /= level;
        //        result = number.ToString() + "/" + result;
        //    }

        //    return result;
        //}
        /// <summary>
        /// 数字转路径，路径包含当前目录下文件名称
        /// </summary>
        /// <param name="number"></param>
        /// <param name="separateChar">路径分隔符</param>
        /// <param name="value">单目录最大值,此值不得小于100</param>
        /// <returns></returns>
        public static string NumberPathFull(long number, string separateChar = "/", int value = 1000)
        {
            int name;
            string path = NumberPath(number, out name, separateChar, value);

            return string.Empty.Equals(path) ? name.ToString() : string.Concat(path, separateChar, name);
        }

        /// <summary>
        /// 数字转路径，路径不包含当前目录下文件名
        /// </summary>
        /// <param name="number"></param>
        /// <param name="separateChar">路径分隔符</param>
        /// <param name="value">单目录最大值,此值不得小于100</param>
        /// <returns></returns>
        public static string NumberPath(long number, string separateChar = "/", int value = 1000)
        {
            int name;
            return NumberPath(number, out name, separateChar, value);
        }

        /// <summary>
        /// 数字转路径,并返回当前目录下文件名
        /// </summary>
        /// <param name="number"></param>
        /// <param name="separateChar">路径分隔符</param>
        /// <param name="value">单目录最大值,此值不得小于100</param>
        /// <param name="name">文件名</param>
        /// <returns></returns>
        public static string NumberPath(long number, out int name, string separateChar = "/", int value=1000)
        {
            if (number <= value)
            {
                name = (int)number;
                return string.Empty;
            }

            const int max = 9;
            var segment = new string[max];
            var i = max;

            name = (int)(number % value);

            while (number > value)
            {
                number /= value;
                segment[--i] = (number % value).ToString();
            }

            return string.Join(separateChar, segment, i, max - i);
        }
    }
}
