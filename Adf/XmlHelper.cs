using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace Adf
{
    /// <summary>
    /// XML Helper
    /// </summary>
    public static class XmlHelper
    {
        /// <summary>
        /// Object From XML File
        /// </summary>
        /// <param name="xmlfilepath"></param>
        /// <returns></returns>
        public static T FromFile<T>(string xmlfilepath)
        {
            using (var fs = new FileStream(xmlfilepath, FileMode.Open, FileAccess.Read))
            {
                return FromStream<T>(fs);
            }
        }

        /// <summary>
        /// Object From XML Stream
        /// </summary>
        /// <param name="xmlStream"></param>
        /// <returns></returns>
        private static T FromStream<T>(Stream xmlStream)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            return (T)serializer.Deserialize(xmlStream);
        }

        /// <summary>
        /// Object To XML Stream
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="xmlStream"></param>
        private static void ToStream(object obj, Stream xmlStream)
        {
            if (obj == null) return;

            XmlSerializer serializer = new XmlSerializer(obj.GetType());

            // Create an XmlSerializerNamespaces object.
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();

            // Add two namespaces with prefixes.
            ns.Add("xsd", "http://www.w3.org/2001/XMLSchema");
            ns.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");

            XmlWriter writer = new XmlTextWriter(xmlStream, new UTF8Encoding());
            // Serialize using the XmlTextWriter.
            serializer.Serialize(writer, obj, ns);
            xmlStream.Position = 0;
        }

        /// <summary>
        /// Object To File
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="xmlFilePath"></param>
        public static void ToFile(object obj, string xmlFilePath)
        {
            // Create an XmlTextWriter using a FileStream.
            using (Stream fs = new FileStream(xmlFilePath, FileMode.Create))
            {
                ToStream(obj, fs);
            }
        }

        /// <summary>
        /// get xmlnode attribute
        /// </summary>
        /// <param name="node"></param>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string GetAttribute(XmlNode node, string name, string defaultValue = "")
        {
            if (node == null)
                return defaultValue;

            var attribute = node.Attributes[name];

            return attribute == null ? defaultValue : attribute.InnerText;
        }
    }
}