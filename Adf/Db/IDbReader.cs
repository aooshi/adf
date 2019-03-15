using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace Adf.Db
{
    /// <summary>
    /// Data Object Reader Interface
    /// </summary>
    public interface IDbReader
    {
        /// <summary>
        /// name index
        /// </summary>
        Dictionary<string, int> NameIndex
        {
            get;
        }

        /// <summary>
        /// get reader object
        /// </summary>
        IDataReader Reader
        {
            get;
        }

        /// <summary>
        /// get value
        /// </summary>
        /// <typeparam name="T">value type</typeparam>
        /// <param name="name">field name</param>
        /// <returns>is T type value</returns>
        T Get<T>(string name);

        /// <summary>
        /// get value
        /// </summary>
        /// <param name="name">field name</param>
        /// <returns>is T type value</returns>
        object Get(string name);

        /// <summary>
        /// Get To Int16
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Int16 GetAsInt16(string name);

        /// <summary>
        /// Get To Int32
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Int32 GetAsInt32(string name);

        /// <summary>
        /// Get To Int64
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Int64 GetAsInt64(string name);

        /// <summary>
        /// Get To Uint16
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        UInt16 GetAsUInt16(string name);

        /// <summary>
        /// Get To Uint32
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        UInt32 GetAsUInt32(string name);

        /// <summary>
        /// Get To Uint64
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        UInt64 GetAsUInt64(string name);

        /// <summary>
        /// Get To DateTime
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        DateTime GetAsDateTime(string name);

        /// <summary>
        /// Get To Boolean
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Boolean GetAsBoolean(string name);

        /// <summary>
        /// Get To Byte
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Byte GetAsByte(string name);

        /// <summary>
        /// Get To Decimal
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Decimal GetAsDecimail(string name);

        /// <summary>
        /// Get To Double
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Double GetAsDouble(string name);

        /// <summary>
        /// Get To SByte
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        SByte GetAsSByte(string name);

        /// <summary>
        /// Get To String
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        String GetAsString(string name);
    }
}
