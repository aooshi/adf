using System;
using System.IO;
using System.Collections.Generic;

namespace Adf.Config
{
    /// <summary>
    /// 配置文件监控
    /// </summary>
    public sealed class ConfigWatcher : IDisposable
    {
        /// <summary>
        /// check interval, default 30000(30s)
        /// </summary>
        //public static int INTERVAL_MILLISECONDS = 3000;
        public static int INTERVAL_MILLISECONDS = 30 * 1000;

        /// <summary>
        /// get watcher instance
        /// </summary>
        public readonly static ConfigWatcher Watcher = new ConfigWatcher();
        //
        private System.Threading.Thread thread;
        private System.Threading.EventWaitHandle waitEventHandle;
        private Dictionary<string, IConfig> configDictionary;
        //
        private bool disposed = false;
        private object reloadLockObject = new object();


        private ConfigWatcher()
        {
            this.configDictionary = new Dictionary<string, IConfig>();
            //
            this.waitEventHandle = new System.Threading.EventWaitHandle(false, System.Threading.EventResetMode.AutoReset);
            //
            this.thread = new System.Threading.Thread(this.Processor);
            this.thread.IsBackground = true;
            this.thread.Start();
        }

        private void Processor()
        {
            while (this.disposed == false)
            {
                this.waitEventHandle.WaitOne(INTERVAL_MILLISECONDS);
                //
                if (this.disposed == false)
                {
                    IConfig[] configs = null;
                    lock (this.configDictionary)
                    {
                        configs = new IConfig[this.configDictionary.Count];
                        this.configDictionary.Values.CopyTo(configs, 0);
                    }
                    //
                    foreach (var config in configs)
                    {
                        this.Check(config);
                    }
                }
                //
            }

            this.waitEventHandle.Close();
        }

        private void Check(IConfig config)
        {
            try
            {
                var modifyed = config.CheckModifyed();
                if (modifyed == true)
                {
                    config.Reload();
                }
            }
            catch
            {
                //background check, ignore error
            }
        }

        /// <summary>
        /// 添加一个配置文件监控，注意： 若同名则会使用新对象覆盖旧有
        /// </summary>
        /// <param name="filename">文件名不区分大小写</param>
        /// <param name="config"></param>
        public void AddConfig(string filename, IConfig config)
        {
            if (filename == null)
                throw new ArgumentNullException("filename");

            if (config == null)
                throw new ArgumentNullException("config");

            filename = filename.ToLower();
            lock (this.configDictionary)
            {
                this.configDictionary[filename] = config;
            }
        }

        /// <summary>
        /// 指定的文件是否已被配置
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public bool Exists(string filename)
        {
            if (filename == null)
                throw new ArgumentNullException("filename");

            IConfig config = null;
            return this.configDictionary.TryGetValue(filename, out config);
        }

        /// <summary>
        /// 返回已配置文件清单
        /// </summary>
        /// <returns></returns>
        public string[] GetFileNames()
        {
            string[] filenames = null;
            lock (this.configDictionary)
            {
                filenames = new string[this.configDictionary.Count];
                this.configDictionary.Keys.CopyTo(filenames, 0);
            }
            return filenames;
        }

        /// <summary>
        /// 资源释放
        /// </summary>
        public void Dispose()
        {
            this.disposed = true;
            this.waitEventHandle.Set();
        }
    }
}