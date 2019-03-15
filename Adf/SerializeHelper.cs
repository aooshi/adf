using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Xml.Serialization;

namespace Adf
{
    /// <summary>
    /// 序列化数据处理
    /// </summary>
    public class SerializeHelper
    {


        /// <summary>
        /// 读取一个序列数据，文件不存在则返回为null
        /// </summary>
        /// <param name="path">路径</param>
        public static object GetSerialize(string path)
        {
            if (!File.Exists(path)) return null;
            IFormatter formatter = new BinaryFormatter();
            using (FileStream fs_d = File.OpenRead(path))
            {
                return formatter.Deserialize(fs_d);
            }
        }

        /// <summary>
        /// 设置一个序列数据
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="data">数据</param>
        /// <returns>是否成功</returns>
        public static void SetSerialize(string path, object data)
        {
            IFormatter formatter = new BinaryFormatter();
            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                formatter.Serialize(fs, data);
                fs.Close();
            }
        }

        /// <summary>
        /// 读取一个序列数据，文件不存在则返回为null
        /// </summary>
        /// <typeparam name="T">要序列化的类型</typeparam>
        /// <param name="path">路径</param>
        public static T GetXmlSerialize<T>(string path)
        {
            if (!File.Exists(path)) return default(T);
            XmlSerializer xs = new XmlSerializer(typeof(T));
            using (FileStream fs_d = File.OpenRead(path))
            {
                return (T)xs.Deserialize(fs_d);
            }
        }

        /// <summary>
        /// 设置一个序列数据
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="data">数据</param>
        /// <returns>是否成功</returns>
        public static void SetXmlSerialize(string path, object data)
        {
            if (data == null) throw new ArgumentNullException("data");
            XmlSerializer xs = new XmlSerializer(data.GetType());
            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                xs.Serialize(fs, data);
                fs.Close();
            }
        }
    }
}
