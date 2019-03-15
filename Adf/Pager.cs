using System;
using System.Collections.Generic;
using System.Text;

namespace Adf
{
    /// <summary>
    /// 页序处理
    /// </summary>
    public class Pager
    {
        /// <summary>
        /// 获取总数
        /// </summary>
        public int TotalCount
        {
            get;
            protected set;
        }

        /// <summary>
        /// 获取或设置页大小
        /// </summary>
        public int PageSize
        {
            get;
            set;
        }

        /// <summary>
        /// 获取或设置索引
        /// </summary>
        public int PageIndex
        {
            get;
            set;
        }

        /// <summary>
        /// 获取总页数
        /// </summary>
        public int LastIndex
        {
            get;
           private set;
        }

        /// <summary>
        /// 获取下一页索引
        /// </summary>
        public int NextIndex
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取上一页索引
        /// </summary>
        public int PrevIndex
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取索引开始记录数
        /// </summary>
        public long RecordIndex
        {
            get;
            private set;
        }
        
        /// <summary>
        /// 初始化新实例
        ///    --用法:
        ///    var pager = new Pager();
        ///    --数据处理
        ///    pager.Compute('总数');
        ///    --获取值
        /// </summary>
        public Pager() : this(10) { }
        /// <summary>
        /// 初始化新实例
        /// </summary>
        /// <param name="pagesize">页大小</param>
        public Pager(int pagesize) 
        {
            this.PageSize = pagesize;
        }

        /// <summary>
        /// 计算页序
        /// </summary>
        /// <param name="totalcount">总页数</param>
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