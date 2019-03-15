using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Adf.Image
{
    /// <summary>
    /// ΪͼƬ����ˮӡ
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
        /// ��ȡ����ˮӡ�ı�
        /// </summary>
        public string TxtMark
        {
            get { return _txtMark; }
            protected internal set { _txtMark = value; }
        }

        /// <summary>
        /// ��ȡˮӡͼƬ����·��(ע:���������TxtMark������ý���Ч)
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
        /// ��ȡ����ʱ���ֵ��쳣�ı�
        /// </summary>
        public string ErrorMessage
        {
            get { return _ErrorMessage; }
            protected internal set { _ErrorMessage = value; } //�̳�ʱʹ��
        }


        /// <summary>
        /// ��ȡ������ˮӡ���ֵĴ�С������
        /// </summary>
        public Font TxtFont
        {
            get { return _txtFont; }
            set { _txtFont = value; }
        }


        Color _txtcolor = Color.Black;
        /// <summary>
        /// ��ȡ����������ˮӡɫ
        /// </summary>
        public Color TxtColor
        {
            get { return _txtcolor; }
            set { _txtcolor = value; }
        }

        int _txtAlpha = 255;
        /// <summary>
        /// ��ȡ����������ˮӡ͸����,ֵ��0-255��ֵԽСԽ͸��
        /// </summary>
        public int TxtAlpha
        {
            get { return _txtAlpha; }
            set { _txtAlpha = value; }
        }

        //Color _txtbgcolor = Color.Empty;
        ///// <summary>
        ///// ��ȡ����������ˮӡ����ɫ
        ///// </summary>
        //public Color TxtBgColor
        //{
        //    get { return _txtbgcolor; }
        //    set { _txtbgcolor = value; }
        //}

        //int _txtbgAlpha = 100;
        ///// <summary>
        ///// ��ȡ����������ˮӡ����͸����,ֵ��0-255��ֵԽСԽ͸��
        ///// </summary>
        //public int TxtBgAlpha
        //{
        //    get { return _txtbgAlpha; }
        //    set { _txtbgAlpha = value; }
        //}

        /// <summary>
        /// ��ȡ������ˮӡ����߿��,Ĭ��Ϊ10
        /// </summary>
        public int Left
        {
            get { return _Left; }
            set { _Left = value; }
        }

        /// <summary>
        /// ��ȡ������ˮӡ�ඥ�߶�,Ĭ��Ϊ10
        /// </summary>
        public int Top
        {
            get { return _Top; }
            set { _Top = value; }
        }


        ImageMarkPosition _position;
        /// <summary>
        /// ��ȡ������ˮӡͼ����λ��
        /// </summary>
        public ImageMarkPosition Position
        {
            get { return this._position; }
            set { _position = value; }
        }

        int _margin;
        /// <summary>
        /// ��ȡ�����õ�<see cref="Position"/> ������<see cref="ImageMarkPosition.Custom"/> ʱˮӡ��ͼƬ�ı߾�
        /// </summary>
        public int Margin
        {
            get { return _margin; }
            set { _margin = value; }
        }


        /// <summary>
        /// ��ʼ��
        /// </summary>
        /// <param name="imgMark">ˮӡͼƬ</param>
        public ImageMark(string imgMark)
            : this( null, imgMark, 16)
        {
        }

        /// <summary>
        /// ��ʼ��
        /// </summary>
        /// <param name="txtMark">ˮӡ����</param>
        /// <param name="FontSize">���ִ�С</param>
        public ImageMark(string txtMark, int FontSize)
            : this(txtMark, null, FontSize)
        {
        }

        /// <summary>
        /// ��ʼ��
        /// </summary>
        /// <param name="txtMark">ˮӡ����</param>
        /// <param name="imgMark">ˮӡͼƬ</param>
        /// <param name="FontSize">���ִ�С</param>
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
        /// ����ˮӡλ��
        /// </summary>
        /// <param name="imgwidth">ͼƬ���</param>
        /// <param name="imgheight">ͼƬ�߶�</param>
        /// <param name="markwidth">ˮӡ���</param>
        /// <param name="markheight">ˮӡ�߶�</param>
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
        /// ����һ��ˮӡ
        /// </summary>
        /// <param name="stream">Ҫ����ˮӡ��ͼƬ��</param>
        /// <param name="savestream">Ҫ����������</param>
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

                        //����ˮӡ
                        if (!string.IsNullOrEmpty(_txtMark))
                        {
                            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias; //���ֿ����


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

                        //ͼƬˮӡ
                        else if (!string.IsNullOrEmpty(_imgMark))
                        {
                            if (!File.Exists(_imgMark)) throw new FileNotFoundException(string.Format("Not Find File '{0}';", _imgMark));
                            //��ͼƬˮӡ 
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
                        image.Dispose(); //�ͷ���Դռ��
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
        /// ��ʼ����,�����ش����Ƿ�ɹ�,������ɹ����쳣��Ϣ��д��Result����
        /// </summary>
        /// <param name="filepath">Ҫ���ˮӡ��ͼƬ·��</param>
        /// <param name="savefilepath">ˮӡ��Ӻ�ı���·��</param>
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
        /// ��ʼ����,�����ش����Ƿ�ɹ�,������ɹ����쳣��Ϣ��д��Result����
        /// </summary>
        /// <param name="filepath">Ҫ���ˮӡ��ͼƬ·��</param>
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
