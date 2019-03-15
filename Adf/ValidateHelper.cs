using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Adf
{
    /// <summary>
    /// 校验助手
    /// </summary>
    public static class ValidateHelper
    {
        /// <summary>
        /// 判断对象是否为空
        /// </summary>
        /// <param name="obj">object类型</param>
        /// <returns>bool类型</returns>
        public static bool IsNull(object obj)
        {
            return obj == null;
        }

        /// <summary>
        /// 判断对象是否为数据空对象
        /// </summary>
        /// <param name="obj">object类型</param>
        /// <returns>bool类型</returns>
        public static bool IsDBNull(object obj)
        {
            return DBNull.Value.Equals(obj);
        }

        /// <summary>
        /// 判断是否为数据空对象或空对象
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsNullOrDBNull(object obj)
        {
            return obj == null || DBNull.Value.Equals(obj);
        }

        /// <summary>
        /// 是否为邮箱
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsEmail(string source)
        {
            return null == source ? false : Regular.IS_EMAIL.IsMatch(source);
        }

        /// <summary>
        /// 是否包含邮箱
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool HasEmail(string source)
        {
            return null == source ? false : Regular.HAS_EMAIL.IsMatch(source);
        }

        /// <summary>
        /// 是否为Int32字符
        /// </summary>
        /// <param name="source">字符串</param>
        /// <returns></returns>
        public static bool IsInt32(string source)
        {
            if (source == null)
                return false;

            int i;
            return int.TryParse(source, out i);
        }


        /// <summary>
        /// 是否为Int64字符
        /// </summary>
        /// <param name="source">字符串</param>
        /// <returns></returns>
        public static bool IsInt64(string source)
        {
            Int64 i;
            return Int64.TryParse(source, out i);
        }

        /// <summary>
        /// 是否为时间
        /// </summary>
        /// <param name="source">字符串</param>
        /// <returns></returns>
        public static bool IsDateTime(string source)
        {
            DateTime i;
            return DateTime.TryParse(source, out i);
        }

        /// <summary>
        /// 是否为中国手机号
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsMobileOfChina(string source)
        {
            return null == source ? false : Regular.IS_MOBILEOFCHINA.IsMatch(source);
        }

        /// <summary>
        /// 是否为IPV4 IP
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsIPv4(string source)
        {
            return null == source ? false : Regular.IS_IPV4.IsMatch(source);
        }

        /// <summary>
        /// 是否含 有Ipv4 IP
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool HasIPv4(string source)
        {
            return null == source ? false : Regular.HAS_IPV4.IsMatch(source);
        }

        /// <summary>
        /// 是否为IPV6 IP
        /// </summary>
        /// <param name="source"></param>
        /// <param name="hasIPv4"></param>
        /// <returns></returns>
        public static bool IsIPv6(string source, bool hasIPv4 = false)
        {
            if (hasIPv4)
                return null == source ? false : Regular.IS_IPV6.IsMatch(source);
            else
                return null == source ? false : !Regular.IS_IPV4.IsMatch(source) && Regular.IS_IPV6.IsMatch(source);
        }

        /// <summary>
        /// 是否含 有Ipv6 IP
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool HasIPv6(string source)
        {
            return null == source ? false : Regular.HAS_IPV6.IsMatch(source);
        }
        /// <summary>
        /// 是否为IPV4 或 IPv6 IP
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsIP(string source)
        {
            return null == source ? false : (Regular.IS_IPV4.IsMatch(source) || Regular.IS_IPV6.IsMatch(source));
        }

        /// <summary>
        /// 是否含 有Ipv4 或 IPv6 IP
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool HasIP(string source)
        {
            return null == source ? false : (Regular.HAS_IPV4.IsMatch(source) || Regular.HAS_IPV6.IsMatch(source));
        }

        //#region 验证网址
        ///// <summary>
        ///// 验证网址
        ///// </summary>
        ///// <param name="source"></param>
        ///// <returns></returns>
        //public static bool IsUrl(string source)
        //{
        //    return Regex.IsMatch(source, @"^(((file|gopher|news|nntp|telnet|http|ftp|https|ftps|sftp)://)|(www\.))+(([a-zA-Z0-9\._-]+\.[a-zA-Z]{2,6})|([0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}))(/[a-zA-Z0-9\&amp;%_\./-~-]*)?$", RegexOptions.IgnoreCase);
        //}
        //public static bool HasUrl(string source)
        //{
        //    return Regex.IsMatch(source, @"(((file|gopher|news|nntp|telnet|http|ftp|https|ftps|sftp)://)|(www\.))+(([a-zA-Z0-9\._-]+\.[a-zA-Z]{2,6})|([0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}))(/[a-zA-Z0-9\&amp;%_\./-~-]*)?", RegexOptions.IgnoreCase);
        //}
        //#endregion


        //#region 验证身份证是否有效
        ///// <summary>
        ///// 验证身份证是否有效
        ///// </summary>
        ///// <param name="Id"></param>
        ///// <returns></returns>
        //public static bool IsIDCard(string Id)
        //{
        //    if (Id.Length == 18)
        //    {
        //        bool check = IsIDCard18(Id);
        //        return check;
        //    }
        //    else if (Id.Length == 15)
        //    {
        //        bool check = IsIDCard15(Id);
        //        return check;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        //public static bool IsIDCard18(string Id)
        //{
        //    long n = 0;
        //    if (long.TryParse(Id.Remove(17), out n) == false || n < Math.Pow(10, 16) || long.TryParse(Id.Replace('x', '0').Replace('X', '0'), out n) == false)
        //    {
        //        return false;//数字验证
        //    }
        //    string address = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";
        //    if (address.IndexOf(Id.Remove(2)) == -1)
        //    {
        //        return false;//省份验证
        //    }
        //    string birth = Id.Substring(6, 8).Insert(6, "-").Insert(4, "-");
        //    DateTime time = new DateTime();
        //    if (DateTime.TryParse(birth, out time) == false)
        //    {
        //        return false;//生日验证
        //    }
        //    string[] arrVarifyCode = ("1,0,x,9,8,7,6,5,4,3,2").Split(',');
        //    string[] Wi = ("7,9,10,5,8,4,2,1,6,3,7,9,10,5,8,4,2").Split(',');
        //    char[] Ai = Id.Remove(17).ToCharArray();
        //    int sum = 0;
        //    for (int i = 0; i < 17; i++)
        //    {
        //        sum += int.Parse(Wi[i]) * int.Parse(Ai[i].ToString());
        //    }
        //    int y = -1;
        //    Math.DivRem(sum, 11, out y);
        //    if (arrVarifyCode[y] != Id.Substring(17, 1).ToLower())
        //    {
        //        return false;//校验码验证
        //    }
        //    return true;//符合GB11643-1999标准
        //}

        //public static bool IsIDCard15(string Id)
        //{
        //    long n = 0;
        //    if (long.TryParse(Id, out n) == false || n < Math.Pow(10, 14))
        //    {
        //        return false;//数字验证
        //    }
        //    string address = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";
        //    if (address.IndexOf(Id.Remove(2)) == -1)
        //    {
        //        return false;//省份验证
        //    }
        //    string birth = Id.Substring(6, 6).Insert(4, "-").Insert(2, "-");
        //    DateTime time = new DateTime();
        //    if (DateTime.TryParse(birth, out time) == false)
        //    {
        //        return false;//生日验证
        //    }
        //    return true;//符合15位身份证标准
        //}
        //#endregion


        /// <summary>
        /// 是不是中国电话，格式010-88888888
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsPhoneOfChina(string source)
        {
            return null == source ? false : Regular.IS_PHONEOFCHINA.IsMatch(source);
        }

        /// <summary>
        /// 是否为中国邮政编码 6个数字
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsPostcodeOfChina(string source)
        {
            return null == source ? false : Regular.IS_POSTCODEOFCHINA.IsMatch(source);
        }

        /// <summary>
        /// 是否为中文
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsChinese(string source)
        {
            return null == source ? false : Regular.IS_CHINESESTRING.IsMatch(source);
        }

        /// <summary>
        /// 是否包含中文
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool HasChinese(string source)
        {
            return null == source ? false : Regular.HAS_CHINESESTRING.IsMatch(source);
        }

        /// <summary>
        /// 验证是不是否为常规字符 字母，数字，下划线的组合
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsNormal(string source)
        {
            return null == source ? false : Regular.IS_NORMAL.IsMatch(source);
        }

        /// <summary> 
        /// 是否为非负纯数字
        /// </summary> 
        /// <param name="source"></param> 
        /// <param name="isNegative">是否允许为负数</param>
        /// <returns></returns> 
        public static bool IsNumber(string source, bool isNegative = false)
        {
            var result = null == source ? false : Regular.IS_NUMBER.IsMatch(source);
            return result && !isNegative ? source[0] != '-' : result;
        }

        /// <summary> 
        /// 是否为整数或浮点数 
        /// </summary> 
        /// <param name="source"></param> 
        /// <param name="isNegative">是否允许为负数</param>
        /// <returns></returns> 
        public static bool IsNumberOrFloat(string source, bool isNegative = false)
        {
            var result = null == source ? false : Regular.IS_NUMBERORFLOAT.IsMatch(source);
            return result && !isNegative ? source[0] != '-' : result;
        }

        /// <summary>
        /// 判断是否为键盘字符
        /// </summary>
        /// <param name="input">要进行判断的字符串</param>
        /// <returns>返回Bool值表示是否为键盘字符</returns>
        public static bool IsKeyboard(string input)
        {
            char[] cs = input.ToCharArray();

            foreach (char c in cs)
                if (c > 126 || c < 32)
                    return false;

            return true;
        }

        /// <summary>
        /// 是否为不区分大小写的字母和数字
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsLetterAndNumber(string input)
        {
            return IsLetterAndNumber(input, LetterCase.SupperAndLower);
        }

        /// <summary>
        /// 是否为字母和数字
        /// </summary>
        /// <param name="input"></param>
        /// <param name="letterCase">指定大小写规则</param>
        /// <returns></returns>
        public static bool IsLetterAndNumber(string input, LetterCase letterCase)
        {
            char[] cs = input.ToCharArray();

            if (letterCase == LetterCase.SupperAndLower)
            {
                foreach (char c in cs)
                {
                    //lower
                    if (c > 96)
                    {
                        if (c < 123)
                        {
                        }
                        else
                        {
                            return false;
                        }
                    }
                    //upper
                    else if (c > 64)
                    {
                        if (c < 91)
                        {
                        }
                        else
                        {
                            return false;
                        }
                    }
                    //number
                    else if (c > 47)
                    {
                        if (c < 58)
                        {

                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else if (letterCase == LetterCase.Supper)
            {
                foreach (char c in cs)
                {
                    //upper
                    if (c > 64)
                    {
                        if (c < 91)
                        {
                        }
                        else
                        {
                            return false;
                        }
                    }
                    //number
                    else if (c > 47)
                    {
                        if (c < 58)
                        {

                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else if (letterCase == LetterCase.Lower)
            {
                foreach (char c in cs)
                {
                    //lower
                    if (c > 96)
                    {
                        if (c < 123)
                        {
                        }
                        else
                        {
                            return false;
                        }
                    }
                    //number
                    else if (c > 47)
                    {
                        if (c < 58)
                        {

                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 是否为不区分大小写的字母
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsLetter(string input)
        {
            return IsLetter(input, LetterCase.SupperAndLower);
        }

        /// <summary>
        /// 是否为指定规则的字母
        /// </summary>
        /// <param name="input"></param>
        /// <param name="letterCase">指定大小写规则</param>
        /// <returns></returns>
        public static bool IsLetter(string input, LetterCase letterCase)
        {
            char[] cs = input.ToCharArray();

            if (letterCase == LetterCase.SupperAndLower)
            {
                foreach (char c in cs)
                {
                    //lower
                    if (c > 96)
                    {
                        if (c < 123)
                        {
                        }
                        else
                        {
                            return false;
                        }
                    }
                    //upper
                    else if (c > 64)
                    {
                        if (c < 91)
                        {
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else if (letterCase == LetterCase.Supper)
            {
                foreach (char c in cs)
                {
                    //upper
                    if (c > 64 && c < 91)
                    {
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else if (letterCase == LetterCase.Lower)
            {
                foreach (char c in cs)
                {
                    //lower
                    if (c > 96 && c < 123)
                    {
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }

    /// <summary>
    /// 字母大小写属性
    /// </summary>
    public enum LetterCase
    {
        /// <summary>
        /// 字母大写和小写
        /// </summary>
        SupperAndLower = 0,
        /// <summary>
        /// 仅大写字母
        /// </summary>
        Supper = 1,
        /// <summary>
        /// 仅小写字母
        /// </summary>
        Lower = 2
    }
}