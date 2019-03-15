using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace Adf.Image
{
    /// <summary>
    /// 图片处理
    /// </summary>
    public class ImageHanlde : IDisposable
    {
        MemoryStream stream;
        ImageFormat _imageformat;

        /// <summary>
        /// 初始化图片
        /// </summary>
        /// <param name="path">图片地址</param>
        public ImageHanlde(string path)
        {
            using(FileStream fs = new FileStream(path,  FileMode.Open))
            {
                byte[] buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);
                stream = new MemoryStream(buffer);
            }
            this.Path = path;
        }

        /// <summary>
        /// 初始化图片
        /// </summary>
        /// <param name="stream">图片流</param>
        public ImageHanlde(Stream stream)
        {
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            this.stream = new MemoryStream(buffer) ;

        }

        /// <summary>
        /// 获取或设置当前图片的格式
        /// </summary>
        public ImageFormat ImageFormat
        {
            get
            {
                return _imageformat;
            }
            set
            {
                MemoryStream ms = new MemoryStream();
                using (System.Drawing.Image image = System.Drawing.Image.FromStream(this.stream))
                {
                    image.Save(ms, value);
                    image.Dispose();
                }
                this.stream.Dispose();
                this.stream = ms;
                this._imageformat = value;
            }
        }

        /// <summary>
        /// 获取正在处理图片的路径
        /// </summary>
        public string Path
        {
            get;
            private set;
        }

        /// <summary>
        /// 创建一个水印图
        /// </summary>
        /// <param name="mark">水印对象</param>
        /// <exception cref="ApplicationException">Create Error</exception>
        public void CreateMark(ImageMark mark)
        {
            MemoryStream ms = new MemoryStream();
            if (!mark.Create(this.stream, ms)) 
                throw new ApplicationException(string.Concat("Create Imagemark Error " , mark.ErrorMessage));
            this.stream.Dispose();
            this.stream = ms;
        }

        /// <summary>
        /// 创建一个水印图
        /// </summary>
        /// <param name="mark">水印对象</param>
        /// <param name="savestream">要保存至的文件流</param>
        /// <exception cref="ApplicationException">Create Error</exception>
        public void CreateMark(ImageMark mark,Stream savestream)
        {
            if (!mark.Create(this.stream, savestream)) 
                throw new ApplicationException(string.Concat("Create Imagemark Error " , mark.ErrorMessage));
        }


        /// <summary>
        /// 创建一个水印图
        /// </summary>
        /// <param name="mark">水印对象</param>
        /// <param name="savepath">要保存至的文件路径</param>
        /// <exception cref="ApplicationException">Create Error</exception>
        public void CreateMark(ImageMark mark, string savepath)
        {
            using (FileStream fs = new FileStream(savepath, FileMode.Create))
            {
                if (!mark.Create(this.stream, fs))
                    throw new ApplicationException(string.Concat("Create Imagemark Error ", mark.ErrorMessage));
            }
        }


        /// <summary>
        /// 保存当前图片,使用此方法必需保证是使用的<see cref="ImageHanlde"/> 重载方法
        /// </summary>
        public void Save()
        {
            if (string.IsNullOrEmpty(this.Path))
                throw new ApplicationException("Instead of using DisposeImage(string path) initialized");

            using (FileStream fs = new FileStream(this.Path, FileMode.Create))
            {
                fs.Write(this.stream.ToArray(), 0, (int)this.stream.Length);
            }
        }

        /// <summary>
        /// 将图片保存至指定路径
        /// </summary>
        /// <param name="path">路径</param>
        public void Save(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                fs.Write(this.stream.ToArray(), 0, (int)this.stream.Length);
            }
        }

        /// <summary>
        /// 将图片保存至指定路径
        /// </summary>
        /// <param name="outputStream">输出流</param>
        public void Save(Stream outputStream)
        {
            if (!outputStream.CanWrite) 
                throw new IOException("the stream can not be written.");
            outputStream.Write(this.stream.ToArray(), 0, (int)this.stream.Length);
        }

        /// <summary>
        /// 将图片保存至指定路径
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="if">要保存的文件格式</param>
        public void Save(string path,ImageFormat @if)
        {
            System.Drawing.Image img = System.Drawing.Image.FromStream(this.stream);
            img.Save(path, @if);
            img.Dispose();
        }

        #region 图片转换


        ///// <summary>
        ///// 转换类型
        ///// </summary>
        //public enum ConvertType
        //{
        //    /// <summary>
        //    /// 转换为Jpeg格式
        //    /// </summary>
        //    jpg,
        //    /// <summary>
        //    /// 转换为png格式
        //    /// </summary>
        //    png,
        //    /// <summary>
        //    /// 转换为gif格式
        //    /// </summary>
        //    gif,
        //    /// <summary>
        //    /// 转换为bmp格式
        //    /// </summary>
        //    bmp
        //}


        ///// <summary>
        ///// 得到须要的格式
        ///// </summary>
        ///// <param name="type">类型</param>
        //private static ImageFormat getFormat(ConvertType type)
        //{
        //    switch (type)
        //    {
        //        case ConvertType.bmp:
        //            return System.Drawing.Imaging.ImageFormat.Bmp;
        //        case ConvertType.gif:
        //            return System.Drawing.Imaging.ImageFormat.Gif;
        //        case ConvertType.jpg:
        //            return System.Drawing.Imaging.ImageFormat.Jpeg;
        //        default:
        //            return System.Drawing.Imaging.ImageFormat.Png;
        //    }
        //}


        /// <summary>
        /// 格式转换,返回文件名
        /// </summary>
        /// <param name="filepath">路径</param>
        /// <param name="type">类型</param>
        public static string ConvertFormat(string filepath, ImageFormat type)
        {
            string tempfile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid() + "." + type.ToString());
            string result = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(filepath), System.IO.Path.GetFileNameWithoutExtension(filepath)) + "." + type.ToString();
            using (System.Drawing.Image image = System.Drawing.Image.FromFile(filepath))
            {
                image.Save(tempfile, type);
            }
            //remove old
            File.Delete(filepath);
            //path
            File.Move(tempfile, result);
            //
            return result;
        }


        #endregion

        #region 图片压缩


        static ImageCodecInfo _JpegCodeinfo = null;
        /// <summary>
        /// 默认的Jpeg编辑器
        /// </summary>
        public static ImageCodecInfo JpegCodeinfo
        {
            get
            {
                if (_JpegCodeinfo == null)
                {
                    foreach (ImageCodecInfo i in ImageCodecInfo.GetImageEncoders())
                    {
                        if (i.FormatDescription.ToUpper() == "JPEG")
                        {
                            _JpegCodeinfo = i;
                            return i;
                        }
                    }

                    if (_JpegCodeinfo == null) 
                        throw new ApplicationException("Not Find JPEG ImageCodecInfo");
                }

                return _JpegCodeinfo;
            }
        }

        /// <summary>
        /// 设置当前图片质量
        /// </summary>
        /// <param name="quality">质量等级，由 1-100的数字表示，建议设置在60以上</param>
        public void SetQuality(int quality)
        {
            System.Drawing.Image image = System.Drawing.Image.FromStream(this.stream);
            Guid FormatID = image.RawFormat.Guid;

            Bitmap bmp = new Bitmap(image.Width,image.Height);
            Graphics gr = Graphics.FromImage(bmp);
            gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            gr.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;


            //设置压缩质量
            EncoderParameters encoderParams = new EncoderParameters();
            long[] qualitys = new long[1];
            qualitys[0] = quality;

            EncoderParameter encoderParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, qualitys);
            encoderParams.Param[0] = encoderParam;

            //获取编码方式
            ImageCodecInfo ici = null;
            foreach (ImageCodecInfo i in ImageCodecInfo.GetImageEncoders())
            {
                if (i.FormatID == FormatID)
                {
                    ici = i;
                    break;
                }
            }

            if (ici == null)
            {
                ici = JpegCodeinfo;
            }

            gr.DrawImage(image, 0, 0, image.Width,image.Height);
            MemoryStream ms2 = new MemoryStream();
            bmp.Save(ms2,ici ,encoderParams);
            bmp.Dispose();
            image.Dispose();

            this.stream.Dispose();
            this.stream = ms2;
        }

        /// <summary>
        /// 图片宽高自动压缩处理
        /// </summary>
        /// <param name="width">图片最大宽度，当大于该宽度时图片将自动进行压缩处理</param>
        public void CompressImage(int width)
        {
            this.CompressImage(width,int.MaxValue);
        }

        /// <summary>
        /// 图片宽高自动压缩处理
        /// </summary>
        /// <param name="width">图片最大宽度，当大于该宽度时图片将自动进行压缩处理,当值小于图片宽图时自动以高度缩放等比计算</param>
        /// <param name="height">图片最大高度，当值小于图片高度时自动以宽图缩放等比计算</param>
        public void CompressImage(int width, int height)
        {
            int ww, hh,w,h;
            ImageFormat @if;
            string ifs;

            System.Drawing.Image image = System.Drawing.Image.FromStream(this.stream);
            ww = w = image.Width;
            hh = h = image.Height;
            @if = image.RawFormat;
            ifs = @if.ToString().ToUpper();

            if (ww <= width && hh <= height)
            {
                image.Dispose();
                return;
            }

            //超宽
            if (ww > width)
            {
                double _w = (double)width / (double)ww;
                hh = (int)(_w * hh);
                ww = width;
            }

            //超高
            if (hh > height)
            {
                double _h = (double)height / (double)hh;
                ww = (int)(_h * ww);
                hh = height;
            }

            Bitmap bmp = new Bitmap(ww, hh);
            //从Bitmap创建一个System.Drawing.Graphics对象，用来绘制高质量的缩小图。
            Graphics gr = Graphics.FromImage(bmp);
            //设置 System.Drawing.Graphics对象的SmoothingMode属性为HighQuality
            gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            //下面这个也设成高质量
            gr.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            //下面这个设成High
            gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
            //把原始图像绘制成上面所设置宽高的缩小图
            System.Drawing.Rectangle rectDestination = new System.Drawing.Rectangle(0, 0, ww, hh);
            gr.DrawImage(image, rectDestination, 0, 0, w, h, GraphicsUnit.Pixel);
            //保存图像，大功告成！
            MemoryStream ms2 = new MemoryStream();
            bmp.Save(ms2,@if);
            bmp.Dispose();
            image.Dispose();

            this.stream.Dispose();
            this.stream = ms2;
        }

        #endregion

        #region IDisposable 成员

        /// <summary>
        /// 释放所占用的资源
        /// </summary>
        public void Dispose()
        {
            stream.Dispose();
        }

        #endregion
    }
}
