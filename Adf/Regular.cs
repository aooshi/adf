using System;
using System.Text.RegularExpressions;
using Adf.Config;

namespace Adf
{
    /// <summary>
    /// 常规正常表达式
    /// </summary>
    public static class Regular
    {
        /// <summary>
        /// is email
        /// </summary>
        public static readonly Regex IS_EMAIL = new Regex(RegularConfig.Instance.GetItem("IS_EMAIL", @"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$"), RegexOptions.IgnoreCase | RegexOptions.Compiled);
        /// <summary>
        /// has email
        /// </summary>
        public static readonly Regex HAS_EMAIL = new Regex(RegularConfig.Instance.GetItem("HAS_EMAIL", @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"), RegexOptions.IgnoreCase | RegexOptions.Compiled);
        /// <summary>
        /// is ipv4
        /// </summary>
        public static readonly Regex IS_IPV4 = new Regex(RegularConfig.Instance.GetItem("IS_IPV4", @"^(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9])\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[0-9])$"), RegexOptions.Compiled);
        /// <summary>
        /// has ipv4
        /// </summary>
        public static readonly Regex HAS_IPV4 = new Regex(RegularConfig.Instance.GetItem("HAS_IPV4", @"(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9])\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[0-9])"), RegexOptions.Compiled);
        /// <summary>
        /// is ipv6
        /// </summary>
        public static readonly Regex IS_IPV6 = new Regex(RegularConfig.Instance.GetItem("IS_IPV6", @"^\s*((([0-9A-Fa-f]{1,4}:){7}(([0-9A-Fa-f]{1,4})|:))|(([0-9A-Fa-f]{1,4}:){6}(:|((25[0-5]|2[0-4]\d|[01]?\d{1,2})(\.(25[0-5]|2[0-4]\d|[01]?\d{1,2})){3})|(:[0-9A-Fa-f]{1,4})))|(([0-9A-Fa-f]{1,4}:){5}((:((25[0-5]|2[0-4]\d|[01]?\d{1,2})(\.(25[0-5]|2[0-4]\d|[01]?\d{1,2})){3})?)|((:[0-9A-Fa-f]{1,4}){1,2})))|(([0-9A-Fa-f]{1,4}:){4}(:[0-9A-Fa-f]{1,4}){0,1}((:((25[0-5]|2[0-4]\d|[01]?\d{1,2})(\.(25[0-5]|2[0-4]\d|[01]?\d{1,2})){3})?)|((:[0-9A-Fa-f]{1,4}){1,2})))|(([0-9A-Fa-f]{1,4}:){3}(:[0-9A-Fa-f]{1,4}){0,2}((:((25[0-5]|2[0-4]\d|[01]?\d{1,2})(\.(25[0-5]|2[0-4]\d|[01]?\d{1,2})){3})?)|((:[0-9A-Fa-f]{1,4}){1,2})))|(([0-9A-Fa-f]{1,4}:){2}(:[0-9A-Fa-f]{1,4}){0,3}((:((25[0-5]|2[0-4]\d|[01]?\d{1,2})(\.(25[0-5]|2[0-4]\d|[01]?\d{1,2})){3})?)|((:[0-9A-Fa-f]{1,4}){1,2})))|(([0-9A-Fa-f]{1,4}:)(:[0-9A-Fa-f]{1,4}){0,4}((:((25[0-5]|2[0-4]\d|[01]?\d{1,2})(\.(25[0-5]|2[0-4]\d|[01]?\d{1,2})){3})?)|((:[0-9A-Fa-f]{1,4}){1,2})))|(:(:[0-9A-Fa-f]{1,4}){0,5}((:((25[0-5]|2[0-4]\d|[01]?\d{1,2})(\.(25[0-5]|2[0-4]\d|[01]?\d{1,2})){3})?)|((:[0-9A-Fa-f]{1,4}){1,2})))|(((25[0-5]|2[0-4]\d|[01]?\d{1,2})(\.(25[0-5]|2[0-4]\d|[01]?\d{1,2})){3})))(%.+)?\s*$"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
        /// <summary>
        /// has ipv6
        /// </summary>
        public static readonly Regex HAS_IPV6 = new Regex(RegularConfig.Instance.GetItem("HAS_IPV6", @"\s*((([0-9A-Fa-f]{1,4}:){7}(([0-9A-Fa-f]{1,4})|:))|(([0-9A-Fa-f]{1,4}:){6}(:|((25[0-5]|2[0-4]\d|[01]?\d{1,2})(\.(25[0-5]|2[0-4]\d|[01]?\d{1,2})){3})|(:[0-9A-Fa-f]{1,4})))|(([0-9A-Fa-f]{1,4}:){5}((:((25[0-5]|2[0-4]\d|[01]?\d{1,2})(\.(25[0-5]|2[0-4]\d|[01]?\d{1,2})){3})?)|((:[0-9A-Fa-f]{1,4}){1,2})))|(([0-9A-Fa-f]{1,4}:){4}(:[0-9A-Fa-f]{1,4}){0,1}((:((25[0-5]|2[0-4]\d|[01]?\d{1,2})(\.(25[0-5]|2[0-4]\d|[01]?\d{1,2})){3})?)|((:[0-9A-Fa-f]{1,4}){1,2})))|(([0-9A-Fa-f]{1,4}:){3}(:[0-9A-Fa-f]{1,4}){0,2}((:((25[0-5]|2[0-4]\d|[01]?\d{1,2})(\.(25[0-5]|2[0-4]\d|[01]?\d{1,2})){3})?)|((:[0-9A-Fa-f]{1,4}){1,2})))|(([0-9A-Fa-f]{1,4}:){2}(:[0-9A-Fa-f]{1,4}){0,3}((:((25[0-5]|2[0-4]\d|[01]?\d{1,2})(\.(25[0-5]|2[0-4]\d|[01]?\d{1,2})){3})?)|((:[0-9A-Fa-f]{1,4}){1,2})))|(([0-9A-Fa-f]{1,4}:)(:[0-9A-Fa-f]{1,4}){0,4}((:((25[0-5]|2[0-4]\d|[01]?\d{1,2})(\.(25[0-5]|2[0-4]\d|[01]?\d{1,2})){3})?)|((:[0-9A-Fa-f]{1,4}){1,2})))|(:(:[0-9A-Fa-f]{1,4}){0,5}((:((25[0-5]|2[0-4]\d|[01]?\d{1,2})(\.(25[0-5]|2[0-4]\d|[01]?\d{1,2})){3})?)|((:[0-9A-Fa-f]{1,4}){1,2})))|(((25[0-5]|2[0-4]\d|[01]?\d{1,2})(\.(25[0-5]|2[0-4]\d|[01]?\d{1,2})){3})))(%.+)?\s*"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
        /// <summary>
        /// is mobile of china
        /// </summary>
        public static readonly Regex IS_MOBILEOFCHINA = new Regex(RegularConfig.Instance.GetItem("IS_MOBILEOFCHINA", @"^1[3458]\d{9}$"), RegexOptions.IgnoreCase | RegexOptions.Compiled);
        /// <summary>
        /// has mobile of china
        /// </summary>
        public static readonly Regex IS_CHINESESTRING = new Regex(RegularConfig.Instance.GetItem("IS_CHINESESTRING", @"^[\u4e00-\u9fa5]+$"), RegexOptions.IgnoreCase | RegexOptions.Compiled);
        /// <summary>
        /// has chinese language
        /// </summary>
        public static readonly Regex HAS_CHINESESTRING = new Regex(RegularConfig.Instance.GetItem("HAS_CHINESESTRING", @"[\u4e00-\u9fa5]+"), RegexOptions.IgnoreCase | RegexOptions.Compiled);
        /// <summary>
        /// is post code of china
        /// </summary>
        public static readonly Regex IS_POSTCODEOFCHINA = new Regex(RegularConfig.Instance.GetItem("IS_POSTCODEOFCHINA", @"^\d{6}$"), RegexOptions.IgnoreCase | RegexOptions.Compiled);
        /// <summary>
        /// is normal chr
        /// </summary>
        public static readonly Regex IS_NORMAL = new Regex(RegularConfig.Instance.GetItem("IS_NORMAL", @"^[\w\d_]+$"), RegexOptions.IgnoreCase | RegexOptions.Compiled);
        /// <summary>
        /// is phone of china
        /// </summary>
        public static readonly Regex IS_PHONEOFCHINA = new Regex(RegularConfig.Instance.GetItem("IS_PHONEOFCHINA", @"^(\d{3,4}\-)?\d{6,8}$"), RegexOptions.Compiled);
        /// <summary>
        /// is all number
        /// </summary>
        public static readonly Regex IS_NUMBER = new Regex(RegularConfig.Instance.GetItem("IS_NUMBER", @"^-?\d+$"), RegexOptions.Compiled);
        /// <summary>
        /// is number or float number
        /// </summary>
        public static readonly Regex IS_NUMBERORFLOAT = new Regex(RegularConfig.Instance.GetItem("IS_NUMBERORFLOAT", @"^-?\d+$|^(-?\d+)(\.\d+)?$"), RegexOptions.Compiled);


        ///// <summary>
        ///// 表示是否为日期时间的正则表达式 格式为: 2000-01-01 00:00:00
        ///// </summary>
        //public const string DATETIME = @"^[0-9]{4}\-[0-9]{1,2}\-[0-9]{1,2} [0-9]{2}\:[0-9]{2}\:[0-9]{2}$";
        ///// <summary>
        ///// 是否为 DATETIME 所表示的值
        ///// </summary>
        //public static readonly Regex DataTime = new Regex(DATETIME, RegexOptions.Compiled);

        ///// <summary>
        ///// 表示是否为正确的日期格式正则表达式 格式为: 2000-01-01
        ///// </summary>
        //public const string DATE = @"^[0-9]{4}\-[0-9]{1,2}\-[0-9]{1,2}$";
        ///// <summary>
        ///// 是否为 DATE 所表示的值
        ///// </summary>
        //public static readonly Regex Data = new Regex(DATE, RegexOptions.Compiled);


        ///// <summary>
        ///// 表示是否为正确的时间格式正则表达式 格式为: 00:00:00
        ///// </summary>
        //public const string TIME = @"^[0-9]{2}\:[0-9]{2}\:[0-9]{2}$";
        ///// <summary>
        ///// 是否为 TIME 所表示的值
        ///// </summary>
        //public static readonly Regex Time = new Regex(TIME, RegexOptions.Compiled);

        ///// <summary>
        ///// 表示是否为邮件的正则表达式
        ///// </summary>
        //public const string EMAIL = @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
        ///// <summary>
        ///// 是否为 EMAIL 所表示的值
        ///// </summary>
        //public static readonly Regex Email = new Regex(EMAIL, RegexOptions.Compiled);

        ///// <summary>
        ///// 表示是否为金额的正则表达式,不带小数位数
        ///// </summary>
        //public const string MONEY = @"^[123456789]{1}\d{0,9}$";
        ///// <summary>
        ///// 是否为 MONEY 所表示的值
        ///// </summary>
        //public static readonly Regex Money = new Regex(MONEY, RegexOptions.Compiled);

        ///// <summary>
        ///// 表示是否为不可为负数,最多带两数小数的正金额值
        ///// </summary>
        //public const string JUSTMONEY = @"^\d+(\.\d{1,2})?$";
        ///// <summary>
        ///// 是否为 JUSTMONEY 所表示的值
        ///// </summary>
        //public static readonly Regex Justmoney = new Regex(JUSTMONEY, RegexOptions.Compiled);

        ///// <summary>
        ///// 表示是否为最多两位小数的只负金额值
        ///// </summary>
        //public const string MINUSMONEY = @"^(\-){1}\d+{1}(\.\d{1,2})?$";
        ///// <summary>
        ///// 是否为 MINUSMONEY 所表示的值
        ///// </summary>
        //public static readonly Regex MinUsmoney = new Regex(MINUSMONEY, RegexOptions.Compiled);

        ///// <summary>
        ///// 表示是否为可正可负且可带两位小数的金额数值
        ///// </summary>
        //public const string JMMONEY = @"^(\-)?\d+{1}(\.\d{1,2})?$";
        ///// <summary>
        ///// 是否为 JMMONEY 所表示的值
        ///// </summary>
        //public static readonly Regex Jmmoney = new Regex(JMMONEY, RegexOptions.Compiled);

        ///// <summary>
        ///// 判断输入字符串是否是数字与字母的组合
        ///// </summary>
        //public const string LETTERANDNUMBER = "^[A-Za-z0-9]+$";
        ///// <summary>
        ///// 是否为 LETTERANDNUMBER 所表示的值
        ///// </summary>
        //public static readonly Regex LetterandNumber = new Regex(LETTERANDNUMBER, RegexOptions.Compiled);

        ///// <summary>
        ///// 判断输入字符串是否为中文
        ///// </summary>
        //public const string CHINESE = @"[\u4e00-\u9fa5]+";
        ///// <summary>
        ///// 是否为 CHINESE 所表示的值
        ///// </summary>
        //public static readonly Regex Chinese = new Regex(CHINESE, RegexOptions.Compiled);
        
        ///// <summary>
        ///// 验证是否为正确的32位MD5加密串
        ///// </summary>
        //public const string MD5 = "^[0-9a-zA-Z]{32}$";
        ///// <summary>
        ///// 是否为 MD5 所表示的值
        ///// </summary>
        //public static readonly Regex Md5 = new Regex(MD5, RegexOptions.Compiled | RegexOptions.IgnoreCase);

    }
}
