using System;

namespace Adf
{
    /// <summary>
    /// 日志代理书写器
    /// </summary>
    public class LogAgentWriter : ILogWriter
    {
        /// <summary>
        /// 日志写入事件
        /// </summary>
        public event EventHandler<LogEventArgs> Writing = null;
        /// <summary>
        /// 资源释放完成事件
        /// </summary>
        public event EventHandler Disposed = null;

        LogLevel level = LogLevel.None;
        /// <summary>
        /// 获取当前日志书写器级别
        /// </summary>
        public LogLevel Level
        {
            get { return this.level; }
        }

        string name = "";
        /// <summary>
        /// 获取日志名
        /// </summary>
        public string Name
        {
            get { return this.name; }
        }

        bool enable = true;
        /// <summary>
        /// 获取或设置当前日志书写器的启用状态
        /// </summary>
        public bool Enable
        {
            get { return this.enable; }
            set { this.enable = value; }
        }

        /// <summary>
        /// 初始化新实例
        /// </summary>
        /// <param name="name"></param>
        /// <param name="level"></param>
        public LogAgentWriter(LogLevel level, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            this.level = level;
            this.name = name;
        }

        /// <summary>
        /// 初始化新实例
        /// </summary>
        /// <param name="level"></param>
        public LogAgentWriter(LogLevel level)
        {
            this.level = level;
            this.name = this.GetType().Name;
        }

        public void Write(string content)
        {
            if (this.enable == false) return;

            var func = this.Writing;
            if (func != null)
            {
                func(this, new LogEventArgs(content, LogLevel.Debug));
            }
        }

        public void Write(string content, params object[] args)
        {
            if (this.enable == false) return;

            if (content != null && args.Length > 0)
            {
                content = string.Format(content, args);
            }

            this.Write(content);
        }

        public void WriteTimeLine(string content, params object[] args)
        {
            if (this.enable == false) return;
            
            //2017-01-01 11:11:00 ......
            //2017-01-01 11:11:01 ......
            this.Write(string.Concat(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), ' ', content, Environment.NewLine), args);
        }

        public void WriteTime(string content, params object[] args)
        {
            if (this.enable == false) return;

            //2017-01-01 11:11:00 ......2017-01-01 11:11:01 ......
            this.Write(string.Concat(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), ' ', content), args);
        }

        public void WriteLine(string content, params object[] args)
        {
            if (this.enable == false) return;

            this.Write(string.Concat(content, Environment.NewLine), args);
        }

        public void WriteLine()
        {
            if (this.enable == false) return;

            this.Write(Environment.NewLine);
        }

        private void OnDisposed()
        {
            var action = this.Disposed;
            if (action != null)
            {
                action(this, EventArgs.Empty);
            }
        }

        public void Dispose()
        {
            this.OnDisposed();
            //
            this.Writing = null;
            this.Disposed = null;
        }
    }
}
