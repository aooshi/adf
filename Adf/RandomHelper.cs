using System;
using System.Collections.Generic;
using System.Text;

namespace Adf
{
    /// <summary>
    /// 随机数
    /// </summary>
    public static class RandomHelper
    {
        static char[] NUMBER = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

        static char[] LOWER_LETTERANDNUMBER_CHARS = new char[]{'0','1', '2','3','4','5','6','7','8','9'
										   ,'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q'
										   ,'r','s','t','u','v','w','x','y','z'};

        static char[] UPPER_LETTERANDNUMBER_CHARS = new char[]{'0','1', '2','3','4','5','6','7','8','9'
										   ,'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q'
										   ,'R','S','T','U','V','W','X','Y','Z'};

        static char[] LETTERANDNUMBER_CHARS = new char[]{'0','1', '2','3','4','5','6','7','8','9'
										   ,'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q'
										   ,'r','s','t','u','v','w','x','y','z','A','B','C','D','E','F','G','H'
										   ,'I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z'};

        static char[] LOWERLETTERS = new char[]{'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q'
										   ,'r','s','t','u','v','w','x','y','z'};

        static char[] LETTERS = new char[]{'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q'
										   ,'r','s','t','u','v','w','x','y','z','A','B','C','D','E','F','G','H'
										   ,'I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z'};

        static int seed = 0;
        
        /// <summary>
        /// 创建一个随机数，随机数据根据传入值与长度决定结果
        /// </summary>
        /// <param name="chars">随机数的字符数组</param>
        /// <param name="size">随机字符串长度</param>
        /// <returns>返回生成的随机数</returns>
        internal static string Make(char[] chars, int size)
        {
            int cl = chars.Length - 1;  //减一防止数据超限

            var seed2 = System.Threading.Interlocked.Increment(ref seed);

            var random = new Random(seed2);
            var build = new StringBuilder();
            
            for (int i = 0; i < size; i++)
                build.Append( chars[random.Next() % cl] );

            return build.ToString();
        }

        /// <summary>
        /// 生成指定位数的随机0-9数字字符串
        /// </summary>
        /// <param name="size">随机字符串长度</param>
        /// <returns>返回生成的字符串</returns>
        public static string Number(int size)
        {
            return Make(NUMBER, size);
        }

        /// <summary>
        /// 生成一个字母与数字随机字符串
        /// </summary>
        /// <param name="size">随机字符串长度</param>
        /// <returns>返回生成的字符串</returns>
        public static string LetterAndNumber(int size)
        {
            return Make(LETTERANDNUMBER_CHARS, size);
        }

        /// <summary>
        /// 生成一个小写字母与数字随机字符串
        /// </summary>
        /// <param name="size">随机字符串长度</param>
        /// <returns>返回生成的字符串</returns>
        public static string LowerLetterAndNumber(int size)
        {
            return Make(LOWER_LETTERANDNUMBER_CHARS, size);
        }
        /// <summary>
        /// 生成一个大写字母与数字随机字符串
        /// </summary>
        /// <param name="size">随机字符串长度</param>
        /// <returns>返回生成的字符串</returns>
        public static string UpperLetterAndNumber(int size)
        {
            return Make(UPPER_LETTERANDNUMBER_CHARS, size);
        }

        /// <summary>
        /// 生成一个小写字母组成的随机字符串
        /// </summary>
        /// <param name="size">随机字符串长度</param>
        /// <returns>返回生成的字符串</returns>
        public static string LowerLetter(int size)
        {
            return Make(LOWERLETTERS, size);
        }

        /// <summary>
        /// 生成字母组成的随机字符串
        /// </summary>
        /// <param name="size">随机字符串长度</param>
        /// <returns>返回生成的字符串</returns>
        public static string Letter(int size)
        {
            return Make(LETTERS, size);
        }
    }
}


