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
    /// ���л����ݴ���
    /// </summary>
    public class SerializeHelper
    {


        /// <summary>
        /// ��ȡһ���������ݣ��ļ��������򷵻�Ϊnull
        /// </summary>
        /// <param name="path">·��</param>
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
        /// ����һ����������
        /// </summary>
        /// <param name="path">·��</param>
        /// <param name="data">����</param>
        /// <returns>�Ƿ�ɹ�</returns>
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
        /// ��ȡһ���������ݣ��ļ��������򷵻�Ϊnull
        /// </summary>
        /// <typeparam name="T">Ҫ���л�������</typeparam>
        /// <param name="path">·��</param>
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
        /// ����һ����������
        /// </summary>
        /// <param name="path">·��</param>
        /// <param name="data">����</param>
        /// <returns>�Ƿ�ɹ�</returns>
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
