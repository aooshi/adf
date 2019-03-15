using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;

namespace Adf
{
    /// <summary>
    /// 日志书写器
    /// </summary>
    /// <remarks>
    /// 配置清单
    /// Log:Path 存储路径, 默认当前应用根目录下 Log 文件夹
    /// Log:Disabled:{Name} 禁止某一写书器, 默认值：false, 单个设置的disabled会覆盖全局设置
    /// </remarks>
    public class LogWriter : IDisposable, ILogWriter
    {
        bool disposed = false;
        MemoryStream cacheStream;

        bool flushing = false;
        object flushLockObject = new object();

        LogLevel level = LogLevel.None;

        /// <summary>
        /// 获取或指定当前日志写书器的日志级别，默认不指定。
        /// </summary>
        public LogLevel Level
        {
            get { return this.level; }
            set { this.level = value; }
        }

        /// <summary>
        /// 新写入事件
        /// </summary>
        public event EventHandler<LogEventArgs> Writing = null;

        /// <summary>
        /// 日志存储刷新完成事件
        /// </summary>
        public event EventHandler Flushed = null;

        /// <summary>
        /// 资源释放完成
        /// </summary>
        public event EventHandler Disposed = null;

        string name = null;
        /// <summary>
        /// 获取当前日志名称
        /// </summary>
        public string Name
        {
            get { return this.name; }
        }

        string path = null;
        /// <summary>
        /// 获取或设置日志存储路径
        /// </summary>
        public string Path
        {
            get
            {
                return this.path;
            }
            set
            {
                if (value == null)
                    this.path = null;
                else
                    this.path = value.TrimEnd('\\', '/');
            }
        }

        string extension = ".log";
        /// <summary>
        /// 获取或设置日志文件扩展名（带点）
        /// </summary>
        public string Extension
        {
            get { return this.extension; }
            set { this.extension = value; }
        }

        Encoding encoding = null;
        /// <summary>
        /// 获取或设置字符编码, 默认UTF8
        /// </summary>
        public Encoding Encoding
        {
            get { return this.encoding; }
            set { this.encoding = value; }
        }

        bool enable = true;
        /// <summary>
        /// 获取或设置是否启用
        /// </summary>
        public bool Enable
        {
            get { return this.enable; }
            set { this.enable = value; }
        }

        bool toConsole = false;
        /// <summary>
        /// 获取或设置是否输出至 Console
        /// </summary>
        public bool ToConsole
        {
            get { return this.toConsole; }
            set { this.toConsole = value; }
        }

        ConsoleColor consoleColor;
        /// <summary>
        /// 获取或设置输出至Console的颜色
        /// </summary>
        public ConsoleColor ConsoleColor
        {
            get { return this.consoleColor; }
            set { this.consoleColor = value; }
        }

        int bufferSize = 0;
        /// <summary>
        /// 获取或设置缓冲区大小，零则不缓冲实际刷新，默认：零
        /// </summary>
        public int BufferSize
        {
            get { return this.bufferSize; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value cannot be less than zero");
                }
                this.bufferSize = value;
            }
        }

        /// <summary>
        /// get current cache count
        /// </summary>
        public long BufferCount
        {
            get { return this.cacheStream.Length; }
        }

        /// <summary>
        /// get is disposed
        /// </summary>
        public bool IsDisposed
        {
            get { return this.disposed; }
        }


        bool throwFlushError = true;
        /// <summary>
        /// is throw exception on flush error, default enable
        /// </summary>
        public bool ThrowFlushError
        {
            get
            {
                return this.throwFlushError;
            }
            set
            {
                this.throwFlushError = value;
            }
        }

        LogManager manager = null;
        /// <summary>
        /// get this writer parent manager
        /// </summary>
        public LogManager Manager
        {
            get { return this.manager; }
            set { this.manager = value; }
        }

        /// <summary>
        /// 初始化新实例
        /// </summary>
        /// <param name="name"></param>
        public LogWriter(string name)
        {
            this.name = name;
            this.Path = null;
            //
            this.Initialize();
        }

        /// <summary>
        /// 初始化新实例
        /// </summary>
        /// <param name="path">当为Null时，若配置(Log.config)Log:Path:name或Log:Path则为配置值，否则以当前应用程序目录下Log为目录</param>
        /// <param name="name"></param>
        public LogWriter(string name, string path)
        {
            this.name = name;
            this.Path = path;
            //
            this.Initialize();
        }

        private void Initialize()
        {
            this.encoding = Encoding.UTF8;
            this.consoleColor = System.ConsoleColor.White;
            this.cacheStream = new MemoryStream(64);
            //
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }
            if (string.IsNullOrEmpty(this.path))
            {
                this.Path = this.LoadPath(this.name);
            }

            if (PathHelper.CheckFileName(this.name) == false)
            {
                throw new ArgumentOutOfRangeException("name", "name contains invalid char");
            }

            if (PathHelper.CheckFilePath(this.path) == false)
            {
                throw new ArgumentOutOfRangeException("path", "path contains invalid char");
            }
            //
            this.Disable();
        }

        private string LoadPath(string name)
        {
            //use log:path:name
            var path = GetConfig("Log:Path:" + name);
            if (path != "" && path != null)
            {
                return path;
            }
            //use log:path
            path = GetConfig("Log:Path");
            if (path != "" && path != null)
            {
                return path;
            }
            //use application
            path = this.CreateDefaultPath();
            if (path == "" || path == null)
            {
                throw new LogException("CreateDefaultPath() method return empty.");
            }
            //
            return path;
        }

        /// <summary>
        /// create default path
        /// </summary>
        /// <returns></returns>
        protected virtual string CreateDefaultPath()
        {
            return System.IO.Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Log");
        }

        private void Disable()
        {
            var disabled = "";
            //
            disabled = GetConfig("Log:Disabled:" + this.name);
            if (disabled != null && disabled != "" && (disabled == "1" || "true".Equals(disabled, StringComparison.OrdinalIgnoreCase)))
            {
                this.enable = false;
                return;
            }
            //
            disabled = GetConfig("Log:Disabled");
            if (disabled != null && disabled != "" && "all".Equals(disabled, StringComparison.OrdinalIgnoreCase))
            {
                this.enable = false;
                return;
            }
        }

        /// <summary>
        /// 获取日志路径
        /// </summary>
        /// <returns></returns>
        protected virtual string BuildPath()
        {
            return this.BuildPath(this.name);
        }

        /// <summary>
        /// 获取日志路径
        /// </summary>
        /// <returns></returns>
        /// <param name="name"></param>
        protected virtual string BuildPath(string name)
        {
            var date = DateTime.Now;
            // path/date/filename.extension

            var build = new StringBuilder();
            build.Append(this.path);
            build.Append(System.IO.Path.DirectorySeparatorChar);
            build.Append(date.ToString("yyyyMMdd"));
            build.Append(System.IO.Path.DirectorySeparatorChar);
            build.Append(name);
            build.Append(this.extension);

            var filepath = build.ToString();
            return filepath;

            //return string.Format("{0}/{1:yyyyMMdd}/{2}{3}", this.path, date, name, this.extension);
        }

        /// <summary>
        /// 获取当前日志文件路径
        /// </summary>
        /// <returns></returns>
        public string GetFilePath()
        {
            return this.BuildPath();
        }
        

        /// <summary>
        /// 写入内容
        /// </summary>
        /// <param name="content"></param>
        /// <param name="args">格式化数据</param>
        /// <exception cref="ObjectDisposedException"></exception>
        public virtual void Write(string content, params object[] args)
        {
            if (null == content)
            {
                return;
            }

            if (this.enable == false)
            {
                return;
            }
            
            if (args.Length > 0)
            {
                content = string.Format(content, args);
            }

            this.Write(content);
        }

        /// <summary>
        /// 写入内容
        /// </summary>
        /// <param name="content"></param>
        /// <exception cref="ObjectDisposedException"></exception>
        public virtual void Write(string content)
        {
            if (null == content)
            {
                return;
            }

            if (this.enable == false)
            {
                return;
            }

            if (this.disposed)
            {
                throw new ObjectDisposedException(this.name);
            }

            if (this.toConsole == true)
            {
                this.WriteToConsole(content);
            }

            this.OnWriting(content);

            var buffer = this.encoding.GetBytes(content);
            if (this.bufferSize == 0)
            {
                this.Flush(buffer);
            }
            else
            {
                lock (this.cacheStream)
                {
                    //
                    this.cacheStream.Write(buffer, 0, buffer.Length);
                    //
                    if (this.flushing == false && this.cacheStream.Position > this.bufferSize)
                    {
                        this.flushing = true;

                        System.Threading.ThreadPool.QueueUserWorkItem(us =>
                        {
                            try
                            {
                                this.Flush();
                            }
                            catch (Exception)
                            {
                                this.flushing = false;
                                //ignore error
                            }
                        });
                    }
                }
            }
        }

        /// <summary>
        /// 触发写入事件
        /// </summary>
        /// <param name="content"></param>
        protected virtual void OnWriting(string content)
        {
            var action = this.Writing;
            if (action != null)
            {
                action(this, new LogEventArgs(content, this.level));
            }
        }

        /// <summary>
        /// 记录一个打包的格式化日志记录
        /// </summary>
        /// <param name="content"></param>
        /// <param name="args">内容格式化串</param>
        public virtual void WritePack(string content, params object[] args)
        {
            /*
             BEGIN 2017-01-01 11:11:00
                ...
             END
             */
            this.Write(string.Concat("BEGIN ", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), Environment.NewLine
                    , content, Environment.NewLine
                    , "END", Environment.NewLine)
                , args);
        }

        /// <summary>
        /// 以时间起始的行
        /// </summary>
        /// <param name="content"></param>
        /// <param name="args"></param>
        public virtual void WriteTimeLine(string content, params object[] args)
        {
            //2017-01-01 11:11:00 ......
            //2017-01-01 11:11:01 ......
            this.Write(string.Concat(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), ' ', content, Environment.NewLine), args);
        }

        /// <summary>
        /// 以时间起始的内容
        /// </summary>
        /// <param name="content"></param>
        /// <param name="args"></param>
        public virtual void WriteTime(string content, params object[] args)
        {
            //2017-01-01 11:11:00 ......2017-01-01 11:11:01 ......
            this.Write(string.Concat(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), ' ', content), args);
        }

        /// <summary>
        /// 按行写入记录
        /// </summary>
        /// <param name="content"></param>
        /// <param name="args"></param>
        public virtual void WriteLine(string content, params object[] args)
        {
            this.Write(string.Concat(content, Environment.NewLine), args);
        }

        /// <summary>
        /// 写入空行
        /// </summary>
        public void WriteLine()
        {
            this.Write(Environment.NewLine);
        }

        /// <summary>
        /// 刷新缓冲区
        /// </summary>
        public void Flush()
        {
            byte[] buffer = null;
            int size = 0;
            //
            lock (this.cacheStream)
            {
                size = (int)this.cacheStream.Position;
                if (size > 0)
                {
                    //reset position to read
                    this.cacheStream.Position = 0;
                    //read data
                    buffer = StreamHelper.Receive(this.cacheStream, (int)size);
                    //reset position to next write
                    this.cacheStream.Position = 0;
                }

                //reset flush
                this.flushing = false;
            }

            //
            if (size > 0)
            {
                this.Flush(buffer);
            }
        }

        /// <summary>
        /// 将内容刷新至硬盘
        /// </summary>
        /// <param name="buffer"></param>
        protected virtual void Flush(byte[] buffer)
        {
            var path = this.BuildPath();

            lock (this.flushLockObject)
            {
                this.Flush(path, buffer, false);
            }

            //trigger event
            this.OnFlushCompleted();
        }

        private void Flush(string path, byte[] buffer, bool retry)
        {
            try
            {
                using (var fs = new FileStream(path, FileMode.Append, FileAccess.Write))
                {
                    fs.Write(buffer, 0, buffer.Length);
                }
            }
            catch (DirectoryNotFoundException)
            {
                //已为重试
                if (retry)
                {
                    if (this.throwFlushError == true)
                    {
                        throw;
                    }
                    else
                    {
                        //丢掉日志
                        return;
                    }
                }

                //igore runing of remove rootpath
                try
                {
                    var dir = System.IO.Path.GetDirectoryName(path);
                    if (Directory.Exists(dir) == false)
                    {
                        Directory.CreateDirectory(dir);
                    }
                }
                catch { }

                this.Flush(path, buffer, true);
            }
            catch (Exception)
            {
                if (this.throwFlushError == true)
                    throw;


                //丢掉日志
            }
        }

        /// <summary>
        /// flush completed
        /// </summary>
        protected void OnFlushCompleted()
        {
            var action = this.Flushed;
            if (action != null)
            {
                action(this, EventArgs.Empty);
            }
        }


        /// <summary>
        /// console logger
        /// </summary>
        /// <param name="content"></param>
        protected virtual void WriteToConsole(string content)
        {
            Console.ForegroundColor = this.ConsoleColor;
            Console.Write(content);
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

        /// <summary>
        /// config change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LogConfigChanged(object sender, EventArgs e)
        {
            if (Adf.Config.LogConfig.Instance.FileExist)
            {
                //change path
                var path = this.LoadPath(this.name);
                if (PathHelper.CheckFilePath(path) == true)
                {
                    this.Path = path;
                }
                //
                this.Disable();
            }
        }

        /// <summary>
        /// 引发Disposed事件
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnDisposed(EventArgs args)
        {
            var action = this.Disposed;
            if (action != null)
            {
                action(this, args);
            }
        }

        /// <summary>
        /// 资源释放
        /// </summary>
        public virtual void Dispose()
        {
            this.Flush();

            if (this.disposed == false)
            {
                this.disposed = true;
                //
                this.cacheStream.Close();
                //
                this.OnDisposed(EventArgs.Empty);

                this.Writing = null;
                this.Disposed = null;
                this.Flushed = null;
            }
        }
    }
}