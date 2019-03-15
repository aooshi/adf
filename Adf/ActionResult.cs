using System;

namespace Adf
{
    /// <summary>
    /// 常规请求结果对象
    /// </summary>
    public class ActionResult
    {
        string message = null;
        /// <summary>
        /// <para>获取或设置消息</para>
        /// <para>get or set action message</para>
        /// </summary>
        public string Message
        {
            get { return message; }
            set { message = value; }
        }


        int code = 0;
        /// <summary>
        /// <para>获取或设置请求结果编号</para>
        /// <para>get or set action code</para>
        /// </summary>
        public int Code
        {
            get { return code; }
            set { code = value; }
        }

        object data = null;
        /// <summary>
        /// <para>获取或设置请求结果数据</para>
        /// <para>get or set action data</para>
        /// </summary>
        public object Data
        {
            get { return data; }
            set { data = value; }
        }

        /// <summary>
        /// <para>初始化新实例</para>
        /// <para>initialize a new instance</para>
        /// </summary>
        public ActionResult()
        {

        }

        /// <summary>
        /// <para>初始化新实例</para>
        /// <para>initialize a new instance</para>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// </summary>
        public ActionResult(int code, string message)
        {
            this.code = code;
            this.message = message;
        }

        /// <summary>
        /// <para>初始化新实例</para>
        /// <para>initialize a new instance</para>
        /// <param name="code"></param>
        /// <param name="data"></param>
        /// </summary>
        public ActionResult(int code, object data)
        {
            this.code = code;
            this.data = data;
        }

    }


    /// <summary>
    /// 常规请求结果对象
    /// </summary>
    public class ActionResult<T>
    {
        string message = null;
        /// <summary>
        /// <para>获取或设置消息</para>
        /// <para>get or set action message</para>
        /// </summary>
        public string Message
        {
            get { return message; }
            set { message = value; }
        }


        int code = 0;
        /// <summary>
        /// <para>获取或设置请求结果编号</para>
        /// <para>get or set action code</para>
        /// </summary>
        public int Code
        {
            get { return code; }
            set { code = value; }
        }

        T data = default(T);
        /// <summary>
        /// <para>获取或设置请求结果数据</para>
        /// <para>get or set action data</para>
        /// </summary>
        public T Data
        {
            get { return data; }
            set { data = value; }
        }

        /// <summary>
        /// <para>初始化新实例</para>
        /// <para>initialize a new instance</para>
        /// </summary>
        public ActionResult()
        {

        }

        /// <summary>
        /// <para>初始化新实例</para>
        /// <para>initialize a new instance</para>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// </summary>
        public ActionResult(int code, string message)
        {
            this.code = code;
            this.message = message;
        }

        /// <summary>
        /// <para>初始化新实例</para>
        /// <para>initialize a new instance</para>
        /// <param name="code"></param>
        /// <param name="data"></param>
        /// </summary>
        public ActionResult(int code, T data)
        {
            this.code = code;
            this.data = data;
        }

    }
}
