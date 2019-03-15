using System;
using System.Collections.Generic;
using System.Text;

namespace Adf
{
    /// <summary>
    /// ҳ����
    /// </summary>
    public class Pager
    {
        /// <summary>
        /// ��ȡ����
        /// </summary>
        public int TotalCount
        {
            get;
            protected set;
        }

        /// <summary>
        /// ��ȡ������ҳ��С
        /// </summary>
        public int PageSize
        {
            get;
            set;
        }

        /// <summary>
        /// ��ȡ����������
        /// </summary>
        public int PageIndex
        {
            get;
            set;
        }

        /// <summary>
        /// ��ȡ��ҳ��
        /// </summary>
        public int LastIndex
        {
            get;
           private set;
        }

        /// <summary>
        /// ��ȡ��һҳ����
        /// </summary>
        public int NextIndex
        {
            get;
            private set;
        }

        /// <summary>
        /// ��ȡ��һҳ����
        /// </summary>
        public int PrevIndex
        {
            get;
            private set;
        }

        /// <summary>
        /// ��ȡ������ʼ��¼��
        /// </summary>
        public long RecordIndex
        {
            get;
            private set;
        }
        
        /// <summary>
        /// ��ʼ����ʵ��
        ///    --�÷�:
        ///    var pager = new Pager();
        ///    --���ݴ���
        ///    pager.Compute('����');
        ///    --��ȡֵ
        /// </summary>
        public Pager() : this(10) { }
        /// <summary>
        /// ��ʼ����ʵ��
        /// </summary>
        /// <param name="pagesize">ҳ��С</param>
        public Pager(int pagesize) 
        {
            this.PageSize = pagesize;
        }

        /// <summary>
        /// ����ҳ��
        /// </summary>
        /// <param name="totalcount">��ҳ��</param>
        public void Compute(int totalcount)
        {
            this.LastIndex = Convert.ToInt32(Math.Ceiling((double)totalcount / this.PageSize));

            if (this.PageIndex < 1) this.PageIndex = 1;
            if (this.PageIndex > this.LastIndex) this.PageIndex = this.LastIndex;

            this.NextIndex = this.PageIndex + 1;
            this.PrevIndex = this.PageIndex - 1;
            if (this.NextIndex > this.LastIndex) this.NextIndex = LastIndex;
            if (this.PrevIndex < 1) this.PrevIndex = 1;


            this.TotalCount = totalcount;

            this.RecordIndex = (this.PageIndex - 1) * this.PageSize;
        }
    }
}