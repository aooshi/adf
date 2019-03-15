using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.IO;
using System.Timers;
using System.Collections;

namespace Adf
{
    /// <summary>
    /// 日志管理器,可通过Log:Disabled配置存储级别，以分号分隔，默认全开，当指定某项时则禁止, 若值为all则禁止所有日志
    /// </summary>
    public class LogManager : IDisposable
    {
        QueueTask<AsyncItem> asyncQueue = null;

        Dictionary<string, LogWriter> writers = new Dictionary<string, LogWriter>(10);
        bool toConsole = false;
        bool isDispose = false;

        IntervalLoop intervalLoop = new IntervalLoop(60);

        /// <summary>
        /// 新写入事件
        /// </summary>
        public event EventHandler<LogEventArgs> Writing = null;

        /// <summary>
        /// 新异常
        /// </summary>
        public event EventHandler<LogExceptionEventArgs> NewException = null;

        /// <summary>
        /// 异步存储异常时
        /// </summary>
        public event EventHandler<LogExceptionEventArgs> AsyncError = null;

        /// <summary>
        /// 获取日志组名称
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取消息日志记录器, 受全局配置参数 Log:Disabled:Message 影响
        /// </summary>
        public LogWriter Message
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取警告日志记录器, 受全局配置参数 Log:Disabled:Warning 影响
        /// </summary>
        public LogWriter Warning
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取错误日志记录器, 受全局配置参数 Log:Disabled:Error 影响
        /// </summary>
        public LogWriter Error
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取调试日志记录器, 受全局配置参数 Log:Disabled:Debug 影响
        /// </summary>
        public LogWriter Debug
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取新异常时是否发送至邮件（若邮件已配置）,默认开启
        /// </summary>
        public bool NewExceptionToMail
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取或设置是否输出至Console
        /// </summary>
        public bool ToConsole
        {
            get
            {
                return this.toConsole;
            }
            set
            {
                this.ForEach(w => { w.ToConsole = value; });
                this.toConsole = value;
            }
        }

        string path;
        /// <summary>
        /// 获取或设置存储路径
        /// </summary>
        public string Path
        {
            get
            {
                return this.path;
            }
            set
            {
                this.path = value.TrimEnd('\\', '/');
                this.ForEach(w => { w.Path = this.path; });
            }
        }

        bool throwFlushError = true;
        /// <summary>
        /// 获取或设置刷新数据至硬盘时是否抛出异常，默认启用
        /// </summary>
        public bool ThrowFlushError
        {
            get
            {
                return this.throwFlushError;
            }
            set
            {
                this.ForEach(w => w.ThrowFlushError = value);
                this.throwFlushError = value;
            }
        }

        /// <summary>
        /// 获取是否具有日志组，当初始name为空时此值为false,否则为true
        /// </summary>
        public bool IsOwnner
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取是否已配置全部禁用，当配置Log:Disabled为all时此值为true
        /// </summary>
        public bool AllDisabled
        {
            get;
            private set;
        }

        int flushInterval = 0;

        /// <summary>
        /// 获取刷新间隔时间,单位：秒，配置项 Log:FlushInterval
        /// </summary>
        public int FlushInterval
        {
            get { return this.flushInterval; }
        }

        /// <summary>
        /// 初始化新实例
        /// </summary>
        public LogManager()
            : this(string.Empty, string.Empty)
        {
        }

        /// <summary>
        /// 初始化新实例并指定名称
        /// </summary>
        /// <param name="name">日志组名称</param>
        public LogManager(string name)
            : this(name, string.Empty)
        {
        }

        /// <summary>
        /// 初始化新实例并指定名称
        /// </summary>
        /// <param name="name">日志组名称</param>
        /// <param name="path">当为Null时，若配置(AppSetting)Log:Path则为配置值，否则以当前应用程序目录下Log为目录</param>
        public LogManager(string name, string path)
        {
            this.Name = name ?? string.Empty;
            this.IsOwnner = !string.Empty.Equals(this.Name);
            //
            if (string.IsNullOrEmpty(path))
            {
                path = GetConfig("Log:Path");
                if (string.Empty.Equals(path))
                {
                    path = System.IO.Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Log");
                }
            }
            this.path = path;
            //
            this.asyncQueue = new QueueTask<AsyncItem>(this.AsyncLogFlush);
            //
            var logDisabled = GetConfig("Log:Disabled") ?? "";
            this.AllDisabled = "all".Equals(logDisabled, StringComparison.OrdinalIgnoreCase);
            //
            if (this.IsOwnner)
            {
                this.Message = new ManageLogWriter(name, path);
                this.Warning = new ManageLogWriter(string.Concat(name, "-warning"), path);
                this.Error = new ManageLogWriter(string.Concat(name, "-error"), path);
                this.Debug = new ManageLogWriter(string.Concat(name, "-debug"), path);
            }
            else
            {
                this.Message = new ManageLogWriter("message", path);
                this.Warning = new ManageLogWriter("warning", path);
                this.Error = new ManageLogWriter("error", path);
                this.Debug = new ManageLogWriter("debug", path);
            }

            //set manage
            this.Message.Manager = this;
            this.Warning.Manager = this;
            this.Error.Manager = this;
            this.Debug.Manager = this;
            //set level
            this.Message.Level = LogLevel.Message;
            this.Warning.Level = LogLevel.Warning;
            this.Error.Level = LogLevel.Error;
            this.Debug.Level = LogLevel.Debug;
            //enable
            this.Message.Enable = this.GetDisable(this.Message, "Message");
            this.Warning.Enable = this.GetDisable(this.Warning, "Warning");
            this.Error.Enable = this.GetDisable(this.Error, "Error");
            this.Debug.Enable = this.GetDisable(this.Debug, "Debug");
            //writing
            this.Message.Writing += this.WritingEvent;
            this.Warning.Writing += this.WritingEvent;
            this.Error.Writing += this.WritingEvent;
            //console
            this.Warning.ConsoleColor = ConsoleColor.Yellow;
            this.Error.ConsoleColor = ConsoleColor.Red;
            //
            this.writers.Add(this.Message.Name, this.Message);
            this.writers.Add(this.Warning.Name, this.Warning);
            this.writers.Add(this.Error.Name, this.Error);
            //
            this.NewExceptionToMail = ExceptionMail.Instance.Available;
            //
            this.intervalLoop.Arrived += this.Arrived;
            //
            var interval = GetConfigAsInt("Log:FlushInterval", 0);
            this.SetFlushInterval(interval);
            //
            Adf.Config.LogConfig.Instance.Changed += this.ConfigChanged;
        }

        private void Arrived(object sender, EventArgs e)
        {
            this.ForEach(log =>
            {
                try
                {
                    log.Flush();
                }
                catch
                {
                    //ignore error from background flush
                }
            });
        }

        private bool GetDisable(LogWriter writer, string name)
        {
            var disabled = "";
            //
            disabled = GetConfig("Log:Disabled:" + writer.Name);
            if (disabled != null && disabled != "" && (disabled == "1" || "true".Equals(disabled, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }
            //
            disabled = GetConfig("Log:Disabled:" + name);
            if (disabled != null && disabled != "" && (disabled == "1" || "true".Equals(disabled, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }
            //
            if (this.AllDisabled)
            {
                return false;
            }

            return true;
        }

        private bool GetDisable(LogWriter writer)
        {
            var disabled = "";
            //
            disabled = GetConfig("Log:Disabled:" + writer.Name);
            if (disabled != null && disabled != "" && (disabled == "1" || "true".Equals(disabled, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }
            //
            if (this.AllDisabled)
            {
                return false;
            }

            return true;
        }

        private void ConfigChanged(object sender, EventArgs e)
        {
            //reset log path
            var path = GetConfig("Log:Path");
            if (string.IsNullOrEmpty(path) == false && Adf.PathHelper.CheckFilePath(path) == true)
            {
                this.Path = path;
            }

            //reset flush interval
            var interval = GetConfigAsInt("Log:FlushInterval", 0);
            this.SetFlushInterval(interval);

            //reset disabled
            var logDisabled = GetConfig("Log:Disabled") ?? "";
            this.AllDisabled = "all".Equals(logDisabled, StringComparison.OrdinalIgnoreCase);

            //
            this.Message.Enable = this.GetDisable(this.Message, "Message");
            this.Warning.Enable = this.GetDisable(this.Warning, "Warning");
            this.Error.Enable = this.GetDisable(this.Error, "Error");

            //
            this.ForEach(log =>
            {
                var w = log as ManageLogWriter;
                if (w != null && w.isException)
                {
                    w.Enable = this.GetDisable(w, "Exception");
                }
                else
                {
                    log.Enable = this.GetDisable(log);
                }
            });
        }

        /// <summary>
        /// 循环调用当前被托管的每个日志写入器
        /// </summary>
        /// <param name="action"></param>
        public void Each(Action<LogWriter> action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            this.ForEach(action);
        }

        /// <summary>
        /// 循环当前每个日志写入器
        /// </summary>
        /// <param name="action"></param>
        protected void ForEach(Action<LogWriter> action)
        {
            LogWriter[] writers = null;
            lock (this.writers)
            {
                writers = new LogWriter[this.writers.Count];
                this.writers.Values.CopyTo(writers, 0);
            }
            foreach (var writer in writers)
                action(writer);
        }


        /// <summary>
        /// <para>创建一个非托管的异步日志书写器，书写器使用完成应调用LogWriter.Dispose方法释放资源</para>
        /// <para>此方法适用于日志名称与路径变化频繁，或要求非实时写入，日志数量巨量的场景</para>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public LogWriter CreateAsyncWriter(string name)
        {
            return this.CreateAsyncWriter(name, this.path);
        }

        /// <summary>
        /// <para>创建一个非托管的异步日志书写器，书写器使用完成应调用LogWriter.Dispose方法释放资源</para>
        /// <para>此方法适用于日志名称与路径变化频繁，或要求非实时写入，日志数量巨量的场景</para>
        /// <para>可通过配置Log:AsynBuffer来指定初始缓冲区大小,默认4096</para>
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual LogWriter CreateAsyncWriter(string name, string path)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            var writer = new AsyncLogWriter(name, path, this.asyncQueue);
            writer.Writing += this.WritingEvent;
            writer.ToConsole = this.toConsole;
            writer.ThrowFlushError = this.throwFlushError;
            writer.BufferSize = GetConfigAsInt("Log:AsynBuffer", 4096);
            //
            return writer;
        }

        /// <summary>
        /// get exception logmanager, 受全局配置参数 Log:Disabled:Exception 影响
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public virtual LogWriter GetExceptionWriter(Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException("exception");

            var writer = this.GetWriter(exception.GetType().Name) as ManageLogWriter;
            if (writer == null)
            {
                throw new NotSupportedException(this.GetWriter(exception.GetType().Name).GetType().Name + " type not support");
            }
            //writer.Writing += this.WritingEvent;
            writer.ConsoleColor = this.Error.ConsoleColor;
            //writer.ToConsole = this.toConsole;
            //writer.ThrowFlushError = this.throwFlushError;
            writer.isException = true;
            writer.Enable = this.GetDisable(writer, "Exception");
            //
            return writer;
        }

        /// <summary>
        /// <para>获取一个拖管日志写书器， 写书器由本管理器托管进行FlushInterval间隔刷新， LogWriter.Dispose方法被管理器托管，无需独立调用</para>
        /// <para>此方法适用于日志名称相对确定的托管日志数量小于1万场景</para>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual LogWriter GetWriter(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            name = this.IsOwnner ? this.Name + "-" + name : name;
            //
            LogWriter writer = null;
            lock (this.writers)
            {
                if (this.writers.TryGetValue(name, out writer) == false)
                {
                    writer = new ManageLogWriter(name, this.Path);
                    writer.Writing += this.WritingEvent;
                    writer.ToConsole = this.toConsole;
                    writer.ThrowFlushError = this.throwFlushError;
                    writer.BufferSize = this.flushInterval == 0 ? 0 : int.MaxValue;
                    this.writers.Add(name, writer);
                }
            }
            //
            return writer;
        }

        /// <summary>
        /// 异常记录
        /// </summary>
        /// <param name="exception"></param>
        public virtual void Exception(Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException("exception");

            GetExceptionWriter(exception).WriteTimeLine(exception.ToString());

            //event
            if (this.NewException != null)
                this.NewException(this, new LogExceptionEventArgs(exception));

            //mail
            if (this.NewExceptionToMail)
            {
                try
                {
                    this.SendExceptionMail(exception);
                }
                catch (Exception e)
                {
                    GetExceptionWriter(exception).WriteTimeLine(e.ToString());
                }
            }
        }

        /// <summary>
        /// 发送异常邮件
        /// </summary>
        /// <param name="exception"></param>
        public virtual void SendExceptionMail(Exception exception)
        {
            ExceptionMail.Instance.SendMail(exception);
        }

        /// <summary>
        /// 设置刷新间隔时间
        /// </summary>
        /// <param name="interval">以秒为单位的数值</param>
        public void SetFlushInterval(int interval)
        {
            if (interval < 0)
                throw new ArgumentOutOfRangeException("interval", "interval must than zero or equal zero");

            //
            this.flushInterval = interval;

            //reset buffer size
            var bufferSize = interval == 0 ? 0 : int.MaxValue;
            this.ForEach(log =>
            {
                log.BufferSize = bufferSize;
            });

            //
            this.intervalLoop.Interval = interval;
        }

        /// <summary>
        /// 设置是否开启日志
        /// </summary>
        public virtual void Enable(bool value)
        {
            this.ForEach(log => log.Enable = value);
        }

        /// <summary>
        /// 刷新并存储所有缓冲区内容，该方法一般在应用结束或需要批量将数据存储时使用
        /// </summary>
        public virtual void Flush()
        {
            this.ForEach(log => log.Flush());
        }

        /// <summary>
        /// 资源释放
        /// </summary>
        public virtual void Dispose()
        {
            if (this.isDispose == false)
            {
                this.isDispose = true;

                this.intervalLoop.Dispose();

                this.ForEach(logWriter =>
                {
                    ((ManageLogWriter)logWriter).ManageDispose();
                });

                lock (this.writers)
                {
                    this.writers.Clear();
                }

                this.asyncQueue.Dispose();

                this.Writing = null;
                this.NewException = null;
                this.writers = null;
                this.Message = null;
                this.Warning = null;
                this.Error = null;
            }
        }

        /// <summary>
        /// 中转输出事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WritingEvent(object sender, LogEventArgs e)
        {
            var action = this.Writing;
            if (action != null)
            {
                action(sender, e);
            }
        }

        /*
         GetConfig & GetConfigAsInt is compatible function,   new version must use log config
         */

        /// <summary>
        /// get config item from logconfig or appconfig, first logconfig
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static string GetConfig(string name)
        {
            var logConfig = Adf.Config.LogConfig.Instance.FileExist;
            var value = "";
            if (logConfig == true)
            {
                value = Adf.Config.LogConfig.Instance.GetString(name, "");
            }
            else
            {
                value = Adf.ConfigHelper.GetSetting(name, "");
            }
            return value;
        }

        /// <summary>
        /// get type int config item
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private static int GetConfigAsInt(string name, int defaultValue)
        {
            var stringValue = GetConfig(name);
            if (stringValue == "")
            {
                return defaultValue;
            }
            var intValue = 0;
            if (int.TryParse(stringValue, out intValue) == false)
            {
                intValue = defaultValue;
            }
            return intValue;
        }

        private class ManageLogWriter : LogWriter
        {
            public bool isException = false;

            public ManageLogWriter(string name, string path)
                : base(name, path)
            {
            }

            public void ManageDispose()
            {
                base.Dispose();
            }

            public override void Dispose()
            {
                //重载， 不允许被手动dispose
            }
        }

        private class AsyncLogWriter : LogWriter
        {
            QueueTask<AsyncItem> asyncQueue = null;

            public AsyncLogWriter(string name, string path, QueueTask<AsyncItem> asyncQueue)
                : base(name, path)
            {
                this.asyncQueue = asyncQueue;
            }

            //重载将数据进行异常托管
            protected override void Flush(byte[] buffer)
            {
                var asyncItem = new AsyncItem();
                asyncItem.buffer = buffer;
                asyncItem.filepath = this.BuildPath();
                asyncItem.writer = this;

                this.asyncQueue.Add(asyncItem);
            }
        }

        private class AsyncItem
        {
            public string filepath;
            public byte[] buffer;
            public LogWriter writer;
        }

        private void AsyncLogFlush(AsyncItem asyncItem)
        {
            this.AsyncLogFlush(asyncItem, false);
        }

        private void AsyncLogFlush(AsyncItem asyncItem, bool retry)
        {
            try
            {
                using (var fs = new FileStream(asyncItem.filepath, FileMode.Append, FileAccess.Write))
                {
                    fs.Write(asyncItem.buffer, 0, asyncItem.buffer.Length);
                }
            }
            catch (DirectoryNotFoundException)
            {
                //已为重试
                if (retry)
                {
                    //backgroup save ignore error
                    return;
                }

                //igore runing of remove rootpath
                try
                {
                    var dir = System.IO.Path.GetDirectoryName(asyncItem.filepath);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                }
                catch (Exception exception)
                {
                    this.TriggerError(asyncItem, exception);
                }

                this.AsyncLogFlush(asyncItem, true);
            }
            catch (Exception exception)
            {
                //backgroup save ignore error
                this.TriggerError(asyncItem, exception);
            }
        }

        private void TriggerError(AsyncItem asyncItem, Exception exception)
        {
            var action = this.AsyncError;
            if (action != null)
            {
                action(asyncItem.writer, new LogExceptionEventArgs(exception));
            }
        }

        ///// <summary>
        ///// use to log agent newlog event
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //public void LogAgentNewLog(object sender, LogEventArgs e)
        //{
        //    if (e.Level == LogLevel.None)
        //    {
        //    }
        //    else if (e.Level == LogLevel.Message)
        //    {
        //        this.Message.Write(e.Content);
        //    }
        //    else if (e.Level == LogLevel.Warning)
        //    {
        //        this.Warning.Write(e.Content);
        //    }
        //    else if (e.Level == LogLevel.Debug)
        //    {
        //        this.Debug.Write(e.Content);
        //    }
        //    else if (e.Level == LogLevel.Error)
        //    {
        //        this.Error.Write(e.Content);
        //    }
        //}
    }
}