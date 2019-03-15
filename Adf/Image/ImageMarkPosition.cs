using System;
using System.Collections.Generic;
using System.Text;

namespace Adf.Image
{
    /// <summary>
    /// 水印图片位置说明
    /// </summary>
    public enum ImageMarkPosition
    {
        /// <summary>
        /// 右下角，可设置margin属性以设置边距
        /// </summary>
        Bottom_Right,

        /// <summary>
        /// 左下角，可设置margin属性以设置边距
        /// </summary>
        Bottom_Left,

        /// <summary>
        /// 右上角，可设置margin属性以设置边距
        /// </summary>
        Top_Right,

        /// <summary>
        /// 左上角，可设置margin属性以设置边距
        /// </summary>
        Top_Left,

        /// <summary>
        /// 正中间
        /// </summary>
        Center,

        /// <summary>
        /// 定制，以Top与Left设置为准
        /// </summary>
        Custom
    }
}
