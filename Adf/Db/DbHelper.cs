using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace Adf.Db
{
    /// <summary>
    /// DB助手
    /// </summary>
    public class DbHelper
    {
        /// <summary>
        /// 将读取器转换为DataTable
        /// </summary>
        /// <param name="reader">数据读取器</param>
        public static DataTable IDataReaderToDataTable(IDataReader reader)
        {
            var dt = new DataTable();
            bool init = false;
            var vals = new object[0];
            dt.BeginLoadData();
            while (reader.Read())
            {
                if (!init)
                {
                    init = true;
                    int fieldCount = reader.FieldCount;
                    for (int i = 0; i < fieldCount; ++i)
                        dt.Columns.Add(reader.GetName(i), reader.GetFieldType(i));
                    vals = new object[fieldCount];
                }
                reader.GetValues(vals);
                dt.LoadDataRow(vals, true);
            }
            reader.Close();
            dt.EndLoadData();
            return dt;
        }

        /// <summary>
        /// 进行数据的安全串过滤,防止常规SQL注入
        /// </summary>
        /// <param name="input">要进行过滤的数据串</param>
        public static string Replace(string input)
        {
            if (null == input)
                return string.Empty;

            return input.Replace("'", "''");
        }

        /// <summary>
        /// 克隆一组参数
        /// </summary>
        /// <param name="parameters">参数对象要求已实现ICloneable接口</param>
        /// <returns></returns>
        public static IDbDataParameter[] CloneParameters(IDbDataParameter[] parameters)
        {
            var length = parameters.Length;
            var parameters2 = new IDbDataParameter[ length ];
            for (int i = 0; i < length; i++)
            {
                parameters2[i] = (IDbDataParameter)((ICloneable)parameters[i]).Clone();
            }
            return parameters2;
        }
    }
}
