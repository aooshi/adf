using System;
using System.Collections.Generic;
using System.Text;

namespace Adf
{
    /// <summary>
    /// �����
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
        /// ����һ���������������ݸ��ݴ���ֵ�볤�Ⱦ������
        /// </summary>
        /// <param name="chars">��������ַ�����</param>
        /// <param name="size">����ַ�������</param>
        /// <returns>�������ɵ������</returns>
        internal static string Make(char[] chars, int size)
        {
            int cl = chars.Length - 1;  //��һ��ֹ���ݳ���

            var seed2 = System.Threading.Interlocked.Increment(ref seed);

            var random = new Random(seed2);
            var build = new StringBuilder();
            
            for (int i = 0; i < size; i++)
                build.Append( chars[random.Next() % cl] );

            return build.ToString();
        }

        /// <summary>
        /// ����ָ��λ�������0-9�����ַ���
        /// </summary>
        /// <param name="size">����ַ�������</param>
        /// <returns>�������ɵ��ַ���</returns>
        public static string Number(int size)
        {
            return Make(NUMBER, size);
        }

        /// <summary>
        /// ����һ����ĸ����������ַ���
        /// </summary>
        /// <param name="size">����ַ�������</param>
        /// <returns>�������ɵ��ַ���</returns>
        public static string LetterAndNumber(int size)
        {
            return Make(LETTERANDNUMBER_CHARS, size);
        }

        /// <summary>
        /// ����һ��Сд��ĸ����������ַ���
        /// </summary>
        /// <param name="size">����ַ�������</param>
        /// <returns>�������ɵ��ַ���</returns>
        public static string LowerLetterAndNumber(int size)
        {
            return Make(LOWER_LETTERANDNUMBER_CHARS, size);
        }
        /// <summary>
        /// ����һ����д��ĸ����������ַ���
        /// </summary>
        /// <param name="size">����ַ�������</param>
        /// <returns>�������ɵ��ַ���</returns>
        public static string UpperLetterAndNumber(int size)
        {
            return Make(UPPER_LETTERANDNUMBER_CHARS, size);
        }

        /// <summary>
        /// ����һ��Сд��ĸ��ɵ�����ַ���
        /// </summary>
        /// <param name="size">����ַ�������</param>
        /// <returns>�������ɵ��ַ���</returns>
        public static string LowerLetter(int size)
        {
            return Make(LOWERLETTERS, size);
        }

        /// <summary>
        /// ������ĸ��ɵ�����ַ���
        /// </summary>
        /// <param name="size">����ַ�������</param>
        /// <returns>�������ɵ��ַ���</returns>
        public static string Letter(int size)
        {
            return Make(LETTERS, size);
        }
    }
}


