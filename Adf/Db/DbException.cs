using System;
using System.Collections.Generic;
using System.Text;

namespace Adf.Db
{
    /// <summary>
    /// �쳣������
    /// </summary>
    public class DbException:Exception
    {
        /// <summary>
        /// ��ʼ���µ��쳣ʵ��
        /// </summary>
        /// <param name="message">��Ϣ</param>
        public DbException(string message) : base(message) { }
    }
}
