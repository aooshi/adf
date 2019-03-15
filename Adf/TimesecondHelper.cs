using System;
using System.Collections.Generic;

using System.Text;

namespace Adf
{
    /// <summary>
    /// 时间秒值,描述自 0001-01-01 00:00:00 UTC 起至今秒值
    /// </summary>
    public static class TimesecondHelper
    {
        const int REMOVE_TICK = 10000000;

        /// <summary>
        /// 返回当前时间的时间秒值
        /// </summary>
        public static long ToTimesecond()
        {
            return DateTime.UtcNow.Ticks / REMOVE_TICK;
        }
        /// <summary>
        /// 返回指定时间的时间秒值
        /// </summary>
        /// <param name="time">要返回的基础时间</param>
        public static long ToTimesecond(DateTime time)
        {
            return time.ToUniversalTime().Ticks / REMOVE_TICK;
        }

        /// <summary>
        /// 根据.net时间戳转为时间秒值
        /// </summary>
        /// <param name="ticks"></param>
        /// <returns></returns>
        public static long ToTimesecond(long ticks)
        {
            return ticks / REMOVE_TICK;
        }

        /// <summary>
        /// 将时间秒值转换为时间对象
        /// </summary>
        /// <param name="timesecond">时间秒值</param>
        public static DateTime ToDateTime(long timesecond)
        {
            return new DateTime(timesecond * REMOVE_TICK, DateTimeKind.Utc).ToLocalTime();
        }
    }
}