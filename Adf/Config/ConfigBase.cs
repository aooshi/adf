using System;
using System.Text;
using System.Web;
using System.IO;
using System.Xml;
using System.Configuration;
using System.Collections.Generic;
using System.Threading;

namespace Adf.Config
{
    /// <summary>
    /// 配置基类
    /// </summary>
    /// <remarks>
    /// /Config/file1.config
    /// /Config/file2.config
    /// /Config/file...N.config
    /// </remarks>
    public abstract class ConfigBase<T> : IConfig
    {
        /// <summary>
        /// 获取配置项字典
        /// </summary>
        protected Dictionary<string, T> items;

        /// <summary>
        /// 获取配置根属性字典
        /// </summary>
        protected Dictionary<string, string> attrs;

        /// <summary>
        /// current load version
        /// </summary>
        protected string version = "";

        /// <summary>
        /// 当配置文件发生变化时
        /// </summary>
        public event EventHandler Changed;

        string fileName;
        /// <summary>
        /// 配置文件名称
        /// </summary>
        public virtual string FileName
        {
            get { return fileName; }
        }

        string _filePath;
        /// <summary>
        /// 本地配置文件路径
        /// </summary>
        public string FilePath
        {
            get { return this._filePath; }
        }

        bool _fileExist;
        /// <summary>
        /// 本地文件是否存在
        /// </summary>
        public bool FileExist
        {
            get { return this._fileExist; }
        }

        /// <summary>
        /// 获取配置项总数
        /// </summary>
        public int Count
        {
            get { return this.items.Count; }
        }

        bool watcherEnable = false;
        /// <summary>
        /// 是否已启用变更监控
        /// </summary>
        public bool IsWatcher
        {
            get { return this.watcherEnable; }
        }

        /// <summary>
        /// 初始化新实例
        /// </summary>
        /// <param name="filename"></param>
        public ConfigBase(string filename)
        {
            if (filename == null)
            {
                throw new ArgumentNullException("filename");
            }

            this.fileName = filename;
            this._filePath = this.GetFilePath(filename);
            this._fileExist = File.Exists(this._filePath);
            //
            this.Init();
            //
            if (this.items == null)
            {
                this.items = new Dictionary<string, T>(5);
            }
            if (this.attrs == null)
            {
                this.attrs = new Dictionary<string, string>(5);
            }
        }

        /// <summary>
        /// get file a path
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        protected virtual string GetFilePath(string filename)
        {
            return ConfigHelper.PATH_ROOT + fileName;
        }

        /// <summary>
        /// 初始化配置导入,可重载此方法在之前与之后进行实现其它业务
        /// </summary>
        protected virtual void Init()
        {
            var items = new Dictionary<string, T>(5);
            var attrs = new Dictionary<string, string>(5);

            //
            if (this._fileExist)
            {
                this.version = this.GetFileVersion(this._filePath);
                this.LoadConfig(this._filePath, items, attrs);
            }

            //set
            this.items = items;
            this.attrs = attrs;
        }

        /// <summary>
        /// 添加到监控列表
        /// </summary>
        protected void AddWatcher()
        {
            if (this.watcherEnable == false)
            {
                ConfigWatcher.Watcher.AddConfig(this.fileName, this);
                this.watcherEnable = true;
            }
        }

        /// <summary>
        /// 重新初始化配置导入,可重载此方法在之前与之后进行实现其它业务
        /// </summary>
        public virtual void Reload()
        {
            var items = new Dictionary<string, T>(5);
            var attrs = new Dictionary<string, string>(5);
            try
            {
                if (File.Exists(this._filePath))
                {
                    this._fileExist = true;
                    this.version = this.GetFileVersion(this._filePath);
                    this.LoadConfig(this._filePath, items, attrs);
                }
                else
                {
                    this._fileExist = false;
                    this.version = "";
                }

                //reset items
                this.items = items;
                this.attrs = attrs;

                this.OnChanged();
            }
            catch (Exception exception)
            {
                this.ReloadErrorHandle(exception);
            }
        }

        /// <summary>
        /// check config file is modifyed
        /// </summary>
        /// <returns></returns>
        public bool CheckModifyed()
        {
            var version = this.GetFileVersion(this._filePath);
            return version != this.version;
        }

        private void LoadConfig(string path, Dictionary<string, T> items, Dictionary<string, string> attrs)
        {
            var doc = new XmlDocument();
            doc.Load(path);

            var name = string.Empty;

            this.FileLoad(doc);

            try
            {
                foreach (XmlAttribute attribute in doc.DocumentElement.Attributes)
                {
                    attrs[attribute.Name] = attribute.Value;
                }
                //
                foreach (XmlNode node in doc.DocumentElement.SelectNodes("item"))
                {
                    name = node.Attributes["name"].InnerText;
                    items[name] = this.NewItem(node);
                }
            }
            catch (Exception)
            {
                throw new ConfigException("analyse config failure, parse last name '" + name + "'");
            }
        }

        /// <summary>
        /// get file version
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        protected virtual string GetFileVersion(string filepath)
        {
            var version = "";
            var fileinfo = new FileInfo(filepath);
            if (fileinfo.Exists)
            {
                try
                {
                    version += fileinfo.LastWriteTimeUtc.Ticks;
                    version += ".";
                    version += fileinfo.Length.ToString();
                }
                catch
                {
                    version = "";
                }
            }
            return version;
        }

        /// <summary>
        /// load config
        /// </summary>
        /// <param name="document"></param>
        protected virtual void FileLoad(XmlDocument document)
        {
        }

        /// <summary>
        /// reload error handle, default ignore error
        /// </summary>
        /// <param name="exception"></param>
        protected virtual void ReloadErrorHandle(Exception exception)
        {
        }

        /// <summary>
        /// trigger change event
        /// </summary>
        protected virtual void OnChanged()
        {
            if (this.Changed != null)
            {
                this.Changed(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 新项
        /// </summary>
        /// <param name="node"></param>
        protected abstract T NewItem(XmlNode node);

        /// <summary>
        /// 获取指定名称配置
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public T this[string name]
        {
            get
            {
                T value;
                return this.items.TryGetValue(name, out value) ? value : default(T);
            }
        }

        /// <summary>
        /// 获取根属性节点
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetAttr(string name)
        {
            string value = null;
            this.attrs.TryGetValue(name, out value);
            return value;
        }

        /// <summary>
        /// 获取根属性节值
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public string GetAttr(string name, string defaultValue)
        {
            string value = null;
            if (this.attrs.TryGetValue(name, out value) == false)
            {
                return defaultValue;
            }
            return value;
        }
        
        /// <summary>
        /// 获取一个根属性节点整型值配置
        /// </summary>
        /// <param name="name"></param>
        /// <param name="default"></param>
        /// <returns></returns>
        public Int16 GetAttrAsInt16(string name, short @default = 0)
        {
            string v = null;
            this.attrs.TryGetValue(name, out v);
            return v == null ? @default : Int16.Parse(v);
        }

        /// <summary>
        /// 获取一个根属性节点整型值配置
        /// </summary>
        /// <param name="name"></param>
        /// <param name="default"></param>
        /// <returns></returns>
        public Int32 GetAttrAsInt32(string name, int @default = 0)
        {
            string v = null;
            this.attrs.TryGetValue(name, out v);
            return v == null ? @default : int.Parse(v);
        }

        /// <summary>
        /// 获取一个根属性节点长整型值配置
        /// </summary>
        /// <param name="name"></param>
        /// <param name="default"></param>
        /// <returns></returns>
        public Int64 GetAttrAsInt64(string name, long @default = 0)
        {
            string v = null;
            this.attrs.TryGetValue(name, out v);
            return v == null ? @default : long.Parse(v);
        }



        /// <summary>
        /// 获取一个根属性节点整型值配置
        /// </summary>
        /// <param name="name"></param>
        /// <param name="default"></param>
        /// <returns></returns>
        public UInt16 GetAttrAsUInt16(string name, ushort @default = 0)
        {
            string v = null;
            this.attrs.TryGetValue(name, out v);
            return v == null ? @default : UInt16.Parse(v);
        }

        /// <summary>
        /// 获取一个根属性节点整型值配置
        /// </summary>
        /// <param name="name"></param>
        /// <param name="default"></param>
        /// <returns></returns>
        public UInt32 GetAttrAsUInt32(string name, uint @default = 0)
        {
            string v = null;
            this.attrs.TryGetValue(name, out v);
            return v == null ? @default : uint.Parse(v);
        }

        /// <summary>
        /// 获取一个根属性节点长整型值配置
        /// </summary>
        /// <param name="name"></param>
        /// <param name="default"></param>
        /// <returns></returns>
        public UInt64 GetAttrAsUInt64(string name, ulong @default = 0)
        {
            string v = null;
            this.attrs.TryGetValue(name, out v);
            return v == null ? @default : ulong.Parse(v);
        }


        /// <summary>
        /// 获取一个根属性节点布尔值配置
        /// </summary>
        /// <param name="name"></param>
        /// <param name="default"></param>
        /// <returns></returns>
        public bool GetAttrAsBoolean(string name, Boolean @default = false)
        {
            string v = null;
            this.attrs.TryGetValue(name, out v);

            if (v == null)
                return @default;

            if (v == "1")
                return true;

            if (v == "0")
                return false;

            return bool.Parse(v);
        }

        






        /// <summary>
        /// get config
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T GetItem(string name, T defaultValue)
        {
            T value;
            return this.items.TryGetValue(name, out value) ? value : defaultValue;
        }

        /// <summary>
        /// 尝试获取
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGet(string name, out T value)
        {
            return this.items.TryGetValue(name, out value);
        }

        /// <summary>
        /// 是否存在某一配置
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Exists(string name)
        {
            T value;
            return this.items.TryGetValue(name, out value);
        }

        /// <summary>
        /// 获取所有键列表
        /// </summary>
        /// <returns></returns>
        public string[] GetKeys()
        {
            var items = this.items;

            string[] keys = new string[items.Count];
            items.Keys.CopyTo(keys, 0);
            return keys;
        }

        /// <summary>
        /// 获取所有值列表
        /// </summary>
        /// <returns></returns>
        public T[] GetItems()
        {
            var items = this.items;

            T[] values = new T[items.Count];
            items.Values.CopyTo(values, 0);
            return values;
        }

        object IConfig.this[string name]
        {
            get
            {
                var value2 = default(T);
                if (this.TryGet(name, out value2) == false)
                {
                    return null;
                }
                return value2;
            }
        }

        bool IConfig.TryGet(string name, out object value)
        {
            var value2 = default(T);
            var result = true;
            if (this.TryGet(name, out value2) == false)
            {
                value = value2;
            }
            else
            {
                value = null;
                result = false;
            }
            return result;
        }
    }
}