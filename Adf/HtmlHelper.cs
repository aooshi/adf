using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;

namespace Adf
{
    /// <summary>
    /// Html助手
    /// </summary>
    public static class HtmlHelper
    {
        static readonly Regex NOARCHORREGEX = new Regex("<a.+?>|</a>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        /// <summary>
        /// 将字符串中href属性都替换为空
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string GetNoArchorBody(string html)
        {
            return string.IsNullOrEmpty(html) ? html : NOARCHORREGEX.Replace(html, string.Empty);
        }

        /// <summary>
        /// 数组转换为表格
        /// </summary>
        /// <param name="array">数组</param>
        /// <param name="id">表格ID</param>
        /// <param name="width">表宽</param>
        /// <param name="border">边框</param>
        /// <param name="cellpadding">填充</param>
        /// <param name="cellspacing">表间距</param>
        /// <returns></returns>
        public static string ArrayToTable(Array array, string id, string width, int border = 1, int cellpadding = 0, int cellspacing = 0)
        {
            if (array == null || array.Length == 0)
                return string.Empty;

            var build = new StringBuilder();
            build.AppendFormat("<table border=\"{0}\" cellpadding=\"{1}\" cellspacing=\"{2}\" id=\"{3}\" width=\"{4}\">", border, cellpadding, cellspacing, id, width);
            build.AppendLine();

            var type = array.GetValue(0).GetType();
            var propertys = type.GetProperties();
            var propertysLength = propertys.Length;

            //header
            build.AppendLine("<tr>");
            foreach (var p in propertys)
            {
                build.AppendFormat("<th>{0}</th>", p.Name);
                build.AppendLine();
            }
            build.AppendLine("</tr>");

            //body
            foreach (var item in array)
            {
                build.AppendLine("<tr>");
                for (var i = 0; i < propertysLength; i++)
                {
                    build.AppendFormat("<td>{0}</td>", propertys[i].GetValue(item, null));
                    build.AppendLine();
                }
                build.AppendLine("</tr>");
            }

            build.AppendLine("</table>");

            return build.ToString();
        }

        /// <summary>
        /// 数据表转换为表格
        /// </summary>
        /// <param name="dt">数据表</param>
        /// <param name="id">表格ID</param>
        /// <param name="width">表宽</param>
        /// <param name="border">边框</param>
        /// <param name="cellpadding">填充</param>
        /// <param name="cellspacing">表间距</param>
        /// <returns></returns>
        public static string DataTableToTable(DataTable dt, string id, string width, int border = 1, int cellpadding = 0, int cellspacing = 0)
        {
            if (dt == null || dt.Rows.Count == 0)
                return string.Empty;

            var build = new StringBuilder();
            build.AppendFormat("<table border=\"{0}\" cellpadding=\"{1}\" cellspacing=\"{2}\" id=\"{3}\" width=\"{4}\">", border, cellpadding, cellspacing, id, width);
            build.AppendLine();

            var columnCount = dt.Columns.Count;
                        
            //header
            build.AppendLine("<tr>");
            foreach (DataColumn c in dt.Columns)
            {
                build.AppendFormat("<th>{0}</th>", c.ColumnName);
                build.AppendLine();
            }
            build.AppendLine("</tr>");

            //body
            foreach (DataRow row in dt.Rows)
            {
                build.AppendLine("<tr>");
                for (var i = 0; i < columnCount; i++)
                {
                    build.AppendFormat("<td>{0}</td>",  row[i]);
                    build.AppendLine();
                }
                build.AppendLine("</tr>");
            }

            build.AppendLine("</table>");

            return build.ToString();
        }
    }
}