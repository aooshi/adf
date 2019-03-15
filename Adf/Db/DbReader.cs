using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace Adf.Db
{
    /// <summary>
    /// Public Reader
    /// </summary>
    public class DbReader : IDbReader
    {
        /// <summary>
        /// data
        /// </summary>
        public Dictionary<string, int> NameIndex
        {
            get;
            private set;
        }

        /// <summary>
        /// get reader object
        /// </summary>
        public IDataReader Reader
        {
            get;
            private set;
        }

        /// <summary>
        /// initialize new instance
        /// </summary>
        /// <param name="reader"></param>
        public DbReader(IDataReader reader)
        {
            this.Reader = reader;
            this.NameIndex = new Dictionary<string, int>(reader.FieldCount, StringComparer.InvariantCultureIgnoreCase);
            for (var i = 0; i < reader.FieldCount; i++)
            {
                this.NameIndex[reader.GetName(i)] = i;
            }
        }

        /// <summary>
        /// get value
        /// </summary>
        /// <typeparam name="T">value type</typeparam>
        /// <param name="name">field name</param>
        /// <returns>is T type value</returns>
        public virtual T Get<T>(string name)
        {
            var value = this.Get(name);

            if (value == null)
                return default(T);

            if (value is T)
                return (T)value;

            return (T)Convert.ChangeType(value,typeof(T));

            //return value == null ? default(T) : (T)value;
        }

        /// <summary>
        /// get value
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public object Get(string name)
        {
            var index = 0;
            if (this.NameIndex.TryGetValue(name, out index) && !this.Reader.IsDBNull(index))
            {
                return this.Reader.GetValue(index);
            }
            return null;
        }

        /// <summary>
        /// Get To Int16
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Int16 GetAsInt16(string name)
        {
            return Convert.ToInt16(this.Get(name));
        }

        /// <summary>
        /// Get To Int32
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Int32 GetAsInt32(string name)
        {
            return Convert.ToInt32(this.Get(name));
        }

        /// <summary>
        /// Get To Int64
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Int64 GetAsInt64(string name)
        {
            return Convert.ToInt64(this.Get(name));
        }

        /// <summary>
        /// Get To Uint16
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public UInt16 GetAsUInt16(string name)
        {
            return Convert.ToUInt16(this.Get(name));
        }
        
        /// <summary>
        /// Get To Uint32
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public UInt32 GetAsUInt32(string name)
        {
            return Convert.ToUInt32(this.Get(name));
        }
        
        /// <summary>
        /// Get To Uint64
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public UInt64 GetAsUInt64(string name)
        {
            return Convert.ToUInt64(this.Get(name));
        }

        /// <summary>
        /// Get To DateTime
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public DateTime GetAsDateTime(string name)
        {
            return Convert.ToDateTime(this.Get(name));
        }

        /// <summary>
        /// Get To Boolean
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Boolean GetAsBoolean(string name)
        {
            return Convert.ToBoolean(this.Get(name));
        }

        /// <summary>
        /// Get To Byte
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Byte GetAsByte(string name)
        {
            return Convert.ToByte(this.Get(name));
        }

        /// <summary>
        /// Get To Decimal
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Decimal GetAsDecimail(string name)
        {
            return Convert.ToDecimal(this.Get(name));
        }

        /// <summary>
        /// Get To Double
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Double GetAsDouble(string name)
        {
            return Convert.ToDouble(this.Get(name));
        }

        /// <summary>
        /// Get To SByte
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public SByte GetAsSByte(string name)
        {
            return Convert.ToSByte(this.Get(name));
        }

        /// <summary>
        /// Get To String
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public String GetAsString(string name)
        {
            return Convert.ToString(this.Get(name));
        }
    }
}