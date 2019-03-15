using System;
using System.Collections.Generic;

using System.Text;

namespace Adf
{
    /// <summary>
    /// UNIX 时间戳
    /// </summary>
    public static class UnixTimestampHelper
    {
        /// <summary>
        /// UNIX TIMESTAMP
        /// </summary>
        public static readonly DateTime UNIXTIMESTAMP_BASE = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        /// <summary>
        /// UNIX TIMESTAMP TICK
        /// </summary>
        public static readonly long UNIXTIMESTAMP_BASETICK = UNIXTIMESTAMP_BASE.Ticks;
        /// <summary>
        /// 1970, 1, 1, 0, 0, 0 seconds
        /// </summary>
        public static readonly int UNIXTIMESTAMP_BASESECONDS = (int)(UNIXTIMESTAMP_BASE.Ticks / 10000000);
        
        /// <summary>
        /// 返回当前时间的Unix Timestamp
        /// </summary>
        public static int ToTimestamp()
        {
            return (int)((DateTime.UtcNow.Ticks - UNIXTIMESTAMP_BASETICK) / 10000000);
        }
        /// <summary>
        /// 返回指定时间的Unix Timestamp
        /// </summary>
        /// <param name="time">要返回的基础时间</param>
        public static int ToTimestamp(DateTime time)
        {
            return (int)((time.ToUniversalTime().Ticks - UNIXTIMESTAMP_BASETICK) / 10000000);
        }

        /// <summary>
        /// 将.net时间戳转为Unix Timestamp
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static int ToTimestamp(long timestamp)
        {
            return (int)((timestamp - UNIXTIMESTAMP_BASETICK) / 10000000);
        }

        /// <summary>
        /// 将Unix Timestamp转换为时间对象
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        public static DateTime ToDateTime(int timestamp)
        {
            var r = timestamp == 0 ? UNIXTIMESTAMP_BASE : UNIXTIMESTAMP_BASE.AddSeconds(timestamp);
            return r.ToLocalTime();
        }

        /// <summary>
        /// 返回当前时间的Unix Timestamp
        /// </summary>
        public static long ToInt64Timestamp()
        {
            return (long)((DateTime.UtcNow.Ticks - UNIXTIMESTAMP_BASETICK) / 10000000);
        }
        /// <summary>
        /// 返回指定时间的Unix Timestamp
        /// </summary>
        /// <param name="time">要返回的基础时间</param>
        public static long ToInt64Timestamp(DateTime time)
        {
            return (long)((time.ToUniversalTime().Ticks - UNIXTIMESTAMP_BASETICK) / 10000000);
        }

        /// <summary>
        /// 将.net时间戳转为Unix Timestamp
        /// </summary>
        /// <param name="ticks"></param>
        /// <returns></returns>
        public static long ToInt64Timestamp(long ticks)
        {
            return (long)((ticks - UNIXTIMESTAMP_BASETICK) / 10000000);
        }

        /// <summary>
        /// 将Unix Timestamp转换为时间对象
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        public static DateTime ToDateTime(long timestamp)
        {
            var r = timestamp == 0 ? UNIXTIMESTAMP_BASE : UNIXTIMESTAMP_BASE.AddSeconds(timestamp);
            return r.ToLocalTime();
        }

    }
}
