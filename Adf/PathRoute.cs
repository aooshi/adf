using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Adf
{
    /// <summary>
    /// path route handler
    /// </summary>
    public class PathRoute<T>
    {
        Dictionary<string, T> typeDictionary = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);

        string typeNamespace;
        /// <summary>
        /// get type namespace
        /// </summary>
        public string TypeNamespace
        {
            get { return this.typeNamespace; }
        }

        Assembly assembly;
        /// <summary>
        /// get assembly
        /// </summary>
        public Assembly Assembly
        {
            get { return this.assembly; }
        }

        /// <summary>
        /// initialize new type namespace instance
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="typeNamespace"></param>
        public PathRoute(Assembly assembly, string typeNamespace)
        {
            this.assembly = assembly;
            this.typeNamespace = typeNamespace;
        }

        /// <summary>
        /// get map type of path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Type GetType(string path)
        {
            var obj = this.GetInstance(path);
            if (obj == null)
                return null;
            return obj.GetType();
        }

        /// <summary>
        /// get path instance
        /// </summary>
        /// <param name="path">ex: /path,   /part1/part2,   /part1/part2/.../partN</param>
        /// <exception cref="ArgumentNullException">path is null</exception>
        /// <exception cref="ArgumentException">path only allow contain a-z0-9</exception>
        /// <returns></returns>
        public T GetInstance(string path)
        {
            path = this.BuildTypeName(path);

            T obj;
            if (this.typeDictionary.TryGetValue(path, out obj) == false)
            {
                obj = (T)this.assembly.CreateInstance(path, true);
                lock (this.typeDictionary)
                {
                    if (typeDictionary.ContainsKey(path) == false)
                    {
                        typeDictionary.Add(path, obj);
                    }
                    else
                    {
                        obj = typeDictionary[path];
                    }
                }
            }

            return obj;
        }

        /// <summary>
        /// build type name
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        protected virtual string BuildTypeName(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            char[] chars = new char[path.Length];
            for (int i = 0, l = path.Length; i < l; i++)
            {
                if (path[i] == '\'')
                {
                    chars[i] = '.';
                }
                else if (path[i] == '/')
                {
                    chars[i] = '.';
                }
                else if (path[i] > 47 && path[i] < 58)
                {
                    //0-9
                    chars[i] = path[i];
                }
                else if (path[i] > 64 && path[i] < 91)
                {
                    //A-Z
                    chars[i] = path[i];
                }
                else if (path[i] > 96 && path[i] < 123)
                {
                    //a-z
                    chars[i] = path[i];
                }
                else
                {
                    throw new ArgumentException("path only allow contain a-z0-9", "path");
                }
            }

            if (chars[0] == '.')
            {
                path = this.typeNamespace + new String(chars);
            }
            else
            {
                path = this.typeNamespace + '.' + new String(chars);
            }

            return path;
        }
    }
}