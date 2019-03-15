using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Adf.Config
{
    /// <summary>
    /// config loader
    /// </summary>
    public class ConfigLoader<T>
    {
        string searchPattern;
        /// <summary>
        /// get search pattern
        /// </summary>
        public string SearchPattern
        {
            get { return this.searchPattern; }
        }

        Dictionary<string, T> dictionary;

        string[] names;
        /// <summary>
        /// get all config names
        /// </summary>
        public string[] Names
        {
            get { return this.names; }
        }

        /// <summary>
        /// get config count
        /// </summary>
        public int Count
        {
            get { return this.dictionary.Count; }
        }

        /// <summary>
        /// initialize new search instance
        /// </summary>
        /// <param name="searchPattern">ex: *.config / *.ex.config</param>
        public ConfigLoader(string searchPattern)
        {
            this.searchPattern = searchPattern;
            var dictionary = this.Load();
            //
            var names = new string[dictionary.Count];
            dictionary.Keys.CopyTo(names, 0);
            //
            this.dictionary = dictionary;
            this.names = names;
        }

        /// <summary>
        /// load config
        /// </summary>
        /// <returns></returns>
        protected virtual Dictionary<string, T> Load()
        {
            Dictionary<string, T> configDictionary = new Dictionary<string, T>();
            var filepaths = Directory.GetFiles(Adf.ConfigHelper.PATH_ROOT, this.searchPattern);
            var filename = "";
            foreach (var filepath in filepaths)
            {
                filename = System.IO.Path.GetFileName(filepath);
                configDictionary.Add(filename.Split('.')[0], (T)Activator.CreateInstance(typeof(T), filename));
            }
            return configDictionary;
        }

        /// <summary>
        /// reload config
        /// </summary>
        public void Reload()
        {
            var dictionary = this.Load();
            //
            var names = new string[dictionary.Count];
            dictionary.Keys.CopyTo(names, 0);
            //
            this.dictionary = dictionary;
            this.names = names;
        }

        /// <summary>
        /// get config instance
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public T this[string name]
        {
            get
            {
                T value = default(T);
                this.dictionary.TryGetValue(name, out value);
                return value;
            }
        }
    }
}