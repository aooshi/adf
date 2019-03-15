using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Adf.Image
{
    /// <summary>
    /// 为图片处理水印
    /// </summary>
    public class ImageMark
    {
        string _txtMark;
        string _imgMark;
        string _ErrorMessage;

        Font _txtFont;
        int _Left ;
        int _Top ;


        /// <summary>
        /// 获取文字水印文本
        /// </summary>
        public string TxtMark
        {
            get { return _txtMark; }
            protected internal set { _txtMark = value; }
        }

        /// <summary>
        /// 获取水印图片对象路径(注:如果已设置TxtMark则此设置将无效)
        /// </summary>
        public string ImgMark
        {
            get { return _imgMark; }
            protected internal set
            {
                _imgMark = value;
                if (!File.Exists(value)) 
                    throw new FileNotFoundException(string.Format("Not Find File '{0}';", value));
            }
        }

        /// <summary>
        /// 获取创建时出现的异常文本
        /// </summary>
        public string ErrorMessage
        {
            get { return _ErrorMessage; }
            protected internal set { _ErrorMessage = value; } //继承时使用
        }


        /// <summary>
        /// 获取或设置水印文字的大小及字体
        /// </summary>
        public Font TxtFont
        {
            get { return _txtFont; }
            set { _txtFont = value; }
        }


        Color _txtcolor = Color.Black;
        /// <summary>
        /// 获取或设置文字水印色
        /// </summary>
        public Color TxtColor
        {
            get { return _txtcolor; }
            set { _txtcolor = value; }
        }

        int _txtAlpha = 255;
        /// <summary>
        /// 获取或设置文字水印透明度,值由0-255，值越小越透明
        /// </summary>
        public int TxtAlpha
        {
            get { return _txtAlpha; }
            set { _txtAlpha = value; }
        }

        //Color _txtbgcolor = Color.Empty;
        ///// <summary>
        ///// 获取或设置文字水印背景色
        ///// </summary>
        //public Color TxtBgColor
        //{
        //    get { return _txtbgcolor; }
        //    set { _txtbgcolor = value; }
        //}

        //int _txtbgAlpha = 100;
        ///// <summary>
        ///// 获取或设置文字水印背景透明度,值由0-255，值越小越透明
        ///// </summary>
        //public int TxtBgAlpha
        //{
        //    get { return _txtbgAlpha; }
        //    set { _txtbgAlpha = value; }
        //}

        /// <summary>
        /// 获取或设置水印距左边宽度,默认为10
        /// </summary>
        public int Left
        {
            get { return _Left; }
            set { _Left = value; }
        }

        /// <summary>
        /// 获取或设置水印距顶高度,默认为10
        /// </summary>
        public int Top
        {
            get { return _Top; }
            set { _Top = value; }
        }


        ImageMarkPosition _position;
        /// <summary>
        /// 获取或设置水印图生成位置
        /// </summary>
        public ImageMarkPosition Position
        {
            get { return this._position; }
            set { _position = value; }
        }

        int _margin;
        /// <summary>
        /// 获取或设置当<see cref="Position"/> 不等于<see cref="ImageMarkPosition.Custom"/> 时水印与图片的边距
        /// </summary>
        public int Margin
        {
            get { return _margin; }
            set { _margin = value; }
        }


        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="imgMark">水印图片</param>
        public ImageMark(string imgMark)
            : this( null, imgMark, 16)
        {
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="txtMark">水印文字</param>
        /// <param name="FontSize">文字大小</param>
        public ImageMark(string txtMark, int FontSize)
            : this(txtMark, null, FontSize)
        {
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="txtMark">水印文字</param>
        /// <param name="imgMark">水印图片</param>
        /// <param name="FontSize">文字大小</param>
        private ImageMark(string txtMark, string imgMark, int FontSize)
        {
            _txtMark = txtMark;
            _imgMark = imgMark;

            _txtFont = new Font("Arial", FontSize);

            _position = ImageMarkPosition.Center;

            _Left = 10;
            _Top = 10;
        }

        /// <summary>
        /// 计算水印位置
        /// </summary>
        /// <param name="imgwidth">图片宽度</param>
        /// <param name="imgheight">图片高度</param>
        /// <param name="markwidth">水印宽度</param>
        /// <param name="markheight">水印高度</param>
        /// <returns>int[x,y]</returns>
        protected virtual int[] GetPosition(int imgwidth, int imgheight, int markwidth, int markheight)
        {
            int x, y;
            switch (this.Position)
            {
                case ImageMarkPosition.Bottom_Left:
                    x = Margin;
                    y = imgheight - markheight - Margin;
                    break;
                case ImageMarkPosition.Bottom_Right:
                    x = imgwidth - markwidth - Margin;
                    y = imgheight - markheight - Margin;
                    break;
                case ImageMarkPosition.Center:
                    x = (imgwidth/2) -(markwidth / 2);
                    y = (imgheight/2) - (markheight / 2);
                    break;
                case ImageMarkPosition.Top_Left:
                    x = y = Margin;
                    break;
                case ImageMarkPosition.Top_Right:
                    x = imgwidth - markwidth - Margin;
                    y = Margin;
                    break;
                default:
                    x = _Left;
                    y = _Top;
                    break;
            }

            return new int[] { x, y };
        }

        /// <summary>
        /// 创建一个水印
        /// </summary>
        /// <param name="stream">要创建水印的图片流</param>
        /// <param name="savestream">要保存至的流</param>
        public virtual bool Create(Stream stream,Stream savestream)
        {
            if (!savestream.CanWrite) 
                throw new IOException("the stream can not be written.");

            int mwidth, mheight;
            int[] pos; //x,y

            try
            {
                using (System.Drawing.Image image = System.Drawing.Image.FromStream(stream))
                {
                    System.Drawing.Imaging.ImageFormat imgformat = image.RawFormat;
                    Graphics g = Graphics.FromImage(image);
                    try
                    {
                        g.SmoothingMode = SmoothingMode.AntiAlias;
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.CompositingQuality = CompositingQuality.HighQuality;

                        //文字水印
                        if (!string.IsNullOrEmpty(_txtMark))
                        {
                            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias; //文字抗锯齿


                            mwidth = (int)(_txtFont.Size * _txtMark.Length);
                            mheight = (int)(_txtFont.Size);
                            pos = GetPosition(image.Width, image.Height, mwidth, mheight);

                            //g.RotateTransform(20);

                            //if (this._txtbgcolor != Color.Empty)
                            //{
                            //    SolidBrush b = new SolidBrush(Color.FromArgb(this._txtbgAlpha,this._txtbgcolor));
                            //    Rectangle r = new Rectangle(pos[0]-1, pos[1]-1, mwidth+1, mheight+1);
                            //    g.DrawRectangle(new Pen(b),r);
                            //    g.FillRectangle(b, r);
                            //}

                            g.DrawImage(image, 0, 0, image.Width, image.Height);

                            using (SolidBrush b2 = new SolidBrush(Color.FromArgb(this._txtAlpha, this._txtcolor)))
                                g.DrawString(_txtMark, _txtFont, b2, pos[0], pos[1]);

                        }

                        //图片水印
                        else if (!string.IsNullOrEmpty(_imgMark))
                        {
                            if (!File.Exists(_imgMark)) throw new FileNotFoundException(string.Format("Not Find File '{0}';", _imgMark));
                            //加图片水印 
                            using (System.Drawing.Image copyImage = System.Drawing.Image.FromFile(_imgMark))
                            {
                                mwidth = copyImage.Width;
                                mheight = copyImage.Height;
                                pos = GetPosition(image.Width, image.Height, mwidth, mheight);

                                g.DrawImage(copyImage, new Rectangle(pos[0], pos[1], copyImage.Width, copyImage.Height), 0, 0, copyImage.Width, copyImage.Height, GraphicsUnit.Pixel);
                            }
                        }
                        else
                        {
                            throw new ApplicationException("not set imgMark or txtMark");
                        }

                    }
                    finally
                    {
                        g.Dispose();

                        image.Save(savestream, imgformat);
                        image.Dispose(); //释放资源占用
                    }                    
                }

            }
            catch (Exception e)
            {
                _ErrorMessage = e.Message;
                return false;
            }
            _ErrorMessage = "";
            return true;
        }

        
        /// <summary>
        /// 开始创建,并返回创建是否成功,如果不成功其异常信息将写入Result属性
        /// </summary>
        /// <param name="filepath">要添加水印的图片路径</param>
        /// <param name="savefilepath">水印添加后的保存路径</param>
        public virtual bool Create(string filepath, string savefilepath)
        {
            if (string.IsNullOrEmpty(filepath)) throw new ArgumentNullException("filepath");
            if (!File.Exists(filepath)) throw new FileNotFoundException(string.Format("Not Find File '{0}';", filepath));

            using (FileStream fs = new FileStream(filepath, FileMode.Open))
            {
                try
                {
                    using (FileStream fs2 = new FileStream(savefilepath, FileMode.Create))
                    {
                        if (!this.Create(fs, fs2)) return false;
                    }
                }
                catch (Exception e)
                {
                    _ErrorMessage = e.Message;
                    return false;
                }
            }

            return true;
        }


        /// <summary>
        /// 开始创建,并返回创建是否成功,如果不成功其异常信息将写入Result属性
        /// </summary>
        /// <param name="filepath">要添加水印的图片路径</param>
        public virtual bool Create(string filepath)
        {
            if (string.IsNullOrEmpty(filepath)) throw new ArgumentNullException("filepath");
            if (!File.Exists(filepath)) throw new FileNotFoundException(string.Format("Not Find File '{0}';", filepath));

            using (MemoryStream ms = new MemoryStream())
            {

                using (FileStream fs = new FileStream(filepath, FileMode.Open))
                {
                    if (!this.Create(fs, ms)) return false;
                }

                try
                {
                    File.Delete(filepath);

                    using (FileStream fs = new FileStream(filepath, FileMode.Create))
                    {
                        fs.Write(ms.ToArray(), 0, (int)ms.Length);
                        fs.Close();
                    }

                }
                catch (Exception err)
                {
                    _ErrorMessage = err.Message;
                    return false;
                }
            }
            return true;
        }

    }
}
