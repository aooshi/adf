using System;
using System.Collections.Generic;
using System.Text;

namespace Adf
{
    /// <summary>
    /// 时间ID生成器
    /// </summary>
    public static class UniqueID
    {
        /*
        process+seconds+guid+rand
        名称	长度	构成
        process	4	abs(crc32( process + hostname + rand())) % 1500625
        seconds	7	(now()-2000-1-1).utc.sencods 
        guid	5	abs(guid) % 52521875
        rand	4	rand(), 最大值为 1500625
        每段数字以35进制表示，不足指定长度的以大写字母Z填充， 共同组成20字符长度标识
        */

        static object lockObject = new object();
        static long basetime = new DateTime(1900, 1, 1,0,0,0, DateTimeKind.Utc).Ticks;
        static string hostname = System.Net.Dns.GetHostName();
        static string processid = System.Diagnostics.Process.GetCurrentProcess().Id.ToString();
        static Random random = new Random();

        const int MAX_HOST = 1500625;
        const int MAX_RAND = 1500625;
        const int MAX_GUID = 52521875;

        /// <summary>
        /// 编码器
        /// </summary>
        public readonly static NumberBaseEncode Encoder = new NumberBaseEncode("0123456789abcdefghijklmnopqrstuvwxy");
        /// <summary>
        /// 填充字符
        /// </summary>
        public const char PAD = 'z';
        //Z 预留为填充值

                
        /// <summary>
        /// 生成一个标识，长度为22字符
        /// </summary>
        public static string Generate()
        {
            var host = Math.Abs(Adf.CRC32Helper.Encode(hostname + processid + random.NextDouble())) % MAX_HOST;
            var time = (DateTime.Now.ToUniversalTime().Ticks - basetime) / 10000000;
            var guid = Math.Abs(Adf.CRC32Helper.Encode(Guid.NewGuid().ToString())) % MAX_GUID;
            var rand = random.Next(MAX_RAND);

            var v1 = Encoder.Encode(host);
            var v2 = Encoder.Encode(time);
            var v3 = Encoder.Encode(guid);
            var v4 = Encoder.Encode(rand);

            if (v1.Length < 4) v1 = v1.PadRight(4, PAD);
            //因大于70年间隔的1000年内，一定是7字符长度，因此此处不进行判断
            //if (v2.Length < 7) v2 = v2.PadRight(7, PAD);
            if (v3.Length < 5) v3 = v3.PadRight(5, PAD);
            if (v4.Length < 4) v4 = v4.PadRight(4, PAD);

            var build = new StringBuilder();
            build.Append(v1);
            build.Append(v2);
            build.Append(v3);
            build.Append(v4);

            return build.ToString();
        }

        /// <summary>
        /// 生成新标识，并以加密型式追加指定字符，若要进行追加不加密，请使用不带参数方法生成码后再进行字符串拼接
        /// 不带前后缀的标准长度为22字符
        /// </summary>
        /// <param name="prefix">加密前缀</param>
        /// <param name="append">加密的追加的信息</param>
        public static string Generate(string prefix, string append)
        {
            var is_pre = string.IsNullOrEmpty(prefix) == false;
            var is_app = string.IsNullOrEmpty(append) == false;

            if (!is_pre && !is_app)
                return Generate();


            var build = new StringBuilder();
                       

            if (is_pre)
            {
                var prefix_crc32 = Math.Abs(Adf.CRC32Helper.Encode(prefix));
                build.Append( Encoder.Encode(prefix_crc32) );
            }

            build.Append(Generate());

            if (is_app)
            {
                var append_crc32 = Math.Abs(Adf.CRC32Helper.Encode(append));
                build.Append(Encoder.Encode(append_crc32));
            }

            return build.ToString();

            //var unixstamp = Adf.UnixTimestampHelper.ToTimestamp();
            //var microsecond = int.Parse(DateTime.Now.ToString("fffffff"));
            //var hostname = Math.Abs(Adf.CRC32Helper.Encode(UniqueID.process));
            //var rand = random.Next();
            ////
            //var emicrosecond = Encoder.Encode(microsecond);
            //var eunixstamp = Encoder.Encode(unixstamp);
            //var ehostname = Encoder.Encode(hostname);
            //var erand = Encoder.Encode(rand);
            //var eidentification = Encoder.Encode(GetIdentification());
            //var eappend = string.Empty;
            //if (!string.IsNullOrEmpty(append))
            //{
            //    var append_crc32 = Math.Abs(Adf.CRC32Helper.Encode(append));
            //    eappend = Encoder.Encode(append_crc32);
            //}
            //var eprefix = string.Empty;
            //if (!string.IsNullOrEmpty(prefix))
            //{
            //    var prefix_crc32 = Math.Abs(Adf.CRC32Helper.Encode(prefix));
            //    eprefix = Encoder.Encode(prefix_crc32);
            //}
            ////
            //var build = new StringBuilder();
            //build.Append(eunixstamp);
            //build.Append(emicrosecond);
            //build.Append(ehostname);
            //build.Append(eidentification);
            //build.Append(erand);
            ////int.MaxValue = 2147483647 = ZKM0ZL = 6 LEGNTH
            ////ID LEGNTH = 5 * 6 = 30
            //if (build.Length < 30)
            //{
            //    var padlen = 30 - build.Length;
            //    build.Append(Adf.RandomHelper.UpperLetterAndNumber(padlen));
            //}
            ////
            //return string.Concat(eprefix, build.ToString(), eappend);
        }
    }
}