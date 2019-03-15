using System;
using System.Collections.Generic;

namespace Adf
{
    /// <summary>
    /// 日志代理器
    /// </summary>
    public class LogAgent
    {
        /// <summary>
        /// default instance
        /// </summary>
        public static readonly LogAgent Default = new LogAgent();

        /// <summary>
        /// 新的日志事件
        /// </summary>
        public event EventHandler<LogEventArgs> Writing = null;

        Dictionary<string, ILogWriter> writerDictionary = new Dictionary<string, ILogWriter>(5);

        /// <summary>
        /// get writer count
        /// </summary>
        public int WriterCount
        {
            get { return this.writerDictionary.Count; }
        }

        ILogWriter message;
        /// <summary>
        /// get message writer
        /// </summary>
        public ILogWriter Message
        {
            get { return this.message; }
        }

        ILogWriter debug;
        /// <summary>
        /// get debug writer
        /// </summary>
        public ILogWriter Debug
        {
            get { return this.debug; }
        }

        ILogWriter warning;
        /// <summary>
        /// get warning writer
        /// </summary>
        public ILogWriter Warning
        {
            get { return this.warning; }
        }

        ILogWriter error;
        /// <summary>
        /// get error writer
        /// </summary>
        public ILogWriter Error
        {
            get { return this.error; }
        }

        int levelFlags = 0;
        /// <summary>
        /// get level flags
        /// </summary>
        public int LevelFlags
        {
            get { return this.levelFlags; }
        }

        /// <summary>
        /// 初始化新实例
        /// </summary>
        public LogAgent()
        {
            this.error = this.GetWriter(LogLevel.Error, "error");
            this.debug = this.GetWriter(LogLevel.Debug, "debug");
            this.message = this.GetWriter(LogLevel.Message, "message");
            this.warning = this.GetWriter(LogLevel.Warning, "warning");
        }

        private void OnWriting(object sender, LogEventArgs e)
        {
            var func = this.Writing;
            if (func != null)
            {
                func(sender, e);
            }
        }

        private void OnDisposed(object sender, EventArgs e)
        {
            ILogWriter writer = (ILogWriter)sender;
            lock (this.writerDictionary)
            {
                this.writerDictionary.Remove(writer.Name);
            }
        }

        /// <summary>
        /// 遍历当前代理的所有书写器的
        /// </summary>
        /// <param name="action"></param>
        public void ForEach(Action<ILogWriter> action)
        {
            ILogWriter[] writers = null;
            lock (this.writerDictionary)
            {
                writers = new ILogWriter[this.writerDictionary.Count];
                this.writerDictionary.Values.CopyTo(writers, 0);
            }

            for (int i = 0; i < writers.Length; i++)
            {
                var w = writers[i];
                action(w);
            }
        }

        /// <summary>
        /// get a writer
        /// </summary>
        /// <param name="level"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual ILogWriter GetWriter(LogLevel level, string name)
        {
            ILogWriter writer = null;
            lock (this.writerDictionary)
            {
                if (this.writerDictionary.TryGetValue(name, out writer) == false)
                {
                    writer = new LogAgentWriter(level, name);
                    writer.Writing += this.OnWriting;
                    writer.Disposed += this.OnDisposed;
                    //
                    if (this.levelFlags > 0)
                    {
                        writer.Enable = (this.levelFlags & (int)level) == (int)level;
                    }
                    //
                    this.writerDictionary.Add(name, writer);
                }
            }
            //
            return writer;
        }

        /// <summary>
        /// set level flags
        /// </summary>
        /// <param name="levelFlags"></param>
        public void SetLevelFlags(int levelFlags)
        {
            if (levelFlags < 0)
            {
                throw new ArgumentOutOfRangeException("flags");
            }
            //
            this.levelFlags = levelFlags;
            //
            if (levelFlags == 0)
            {
                this.ForEach(w => { w.Enable = true; });
            }
            //
            if (levelFlags > 0)
            {
                this.ForEach(w =>
                {
                    w.Enable = (levelFlags & (int)w.Level) == (int)w.Level;
                });
            }
        }
    }
}
