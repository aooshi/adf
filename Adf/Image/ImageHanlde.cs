using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace Adf.Image
{
    /// <summary>
    /// ͼƬ����
    /// </summary>
    public class ImageHanlde : IDisposable
    {
        MemoryStream stream;
        ImageFormat _imageformat;

        /// <summary>
        /// ��ʼ��ͼƬ
        /// </summary>
        /// <param name="path">ͼƬ��ַ</param>
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
        /// ��ʼ��ͼƬ
        /// </summary>
        /// <param name="stream">ͼƬ��</param>
        public ImageHanlde(Stream stream)
        {
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            this.stream = new MemoryStream(buffer) ;

        }

        /// <summary>
        /// ��ȡ�����õ�ǰͼƬ�ĸ�ʽ
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
        /// ��ȡ���ڴ���ͼƬ��·��
        /// </summary>
        public string Path
        {
            get;
            private set;
        }

        /// <summary>
        /// ����һ��ˮӡͼ
        /// </summary>
        /// <param name="mark">ˮӡ����</param>
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
        /// ����һ��ˮӡͼ
        /// </summary>
        /// <param name="mark">ˮӡ����</param>
        /// <param name="savestream">Ҫ���������ļ���</param>
        /// <exception cref="ApplicationException">Create Error</exception>
        public void CreateMark(ImageMark mark,Stream savestream)
        {
            if (!mark.Create(this.stream, savestream)) 
                throw new ApplicationException(string.Concat("Create Imagemark Error " , mark.ErrorMessage));
        }


        /// <summary>
        /// ����һ��ˮӡͼ
        /// </summary>
        /// <param name="mark">ˮӡ����</param>
        /// <param name="savepath">Ҫ���������ļ�·��</param>
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
        /// ���浱ǰͼƬ,ʹ�ô˷������豣֤��ʹ�õ�<see cref="ImageHanlde"/> ���ط���
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
        /// ��ͼƬ������ָ��·��
        /// </summary>
        /// <param name="path">·��</param>
        public void Save(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                fs.Write(this.stream.ToArray(), 0, (int)this.stream.Length);
            }
        }

        /// <summary>
        /// ��ͼƬ������ָ��·��
        /// </summary>
        /// <param name="outputStream">�����</param>
        public void Save(Stream outputStream)
        {
            if (!outputStream.CanWrite) 
                throw new IOException("the stream can not be written.");
            outputStream.Write(this.stream.ToArray(), 0, (int)this.stream.Length);
        }

        /// <summary>
        /// ��ͼƬ������ָ��·��
        /// </summary>
        /// <param name="path">·��</param>
        /// <param name="if">Ҫ������ļ���ʽ</param>
        public void Save(string path,ImageFormat @if)
        {
            System.Drawing.Image img = System.Drawing.Image.FromStream(this.stream);
            img.Save(path, @if);
            img.Dispose();
        }

        #region ͼƬת��


        ///// <summary>
        ///// ת������
        ///// </summary>
        //public enum ConvertType
        //{
        //    /// <summary>
        //    /// ת��ΪJpeg��ʽ
        //    /// </summary>
        //    jpg,
        //    /// <summary>
        //    /// ת��Ϊpng��ʽ
        //    /// </summary>
        //    png,
        //    /// <summary>
        //    /// ת��Ϊgif��ʽ
        //    /// </summary>
        //    gif,
        //    /// <summary>
        //    /// ת��Ϊbmp��ʽ
        //    /// </summary>
        //    bmp
        //}


        ///// <summary>
        ///// �õ���Ҫ�ĸ�ʽ
        ///// </summary>
        ///// <param name="type">����</param>
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
        /// ��ʽת��,�����ļ���
        /// </summary>
        /// <param name="filepath">·��</param>
        /// <param name="type">����</param>
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

        #region ͼƬѹ��


        static ImageCodecInfo _JpegCodeinfo = null;
        /// <summary>
        /// Ĭ�ϵ�Jpeg�༭��
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
        /// ���õ�ǰͼƬ����
        /// </summary>
        /// <param name="quality">�����ȼ����� 1-100�����ֱ�ʾ������������60����</param>
        public void SetQuality(int quality)
        {
            System.Drawing.Image image = System.Drawing.Image.FromStream(this.stream);
            Guid FormatID = image.RawFormat.Guid;

            Bitmap bmp = new Bitmap(image.Width,image.Height);
            Graphics gr = Graphics.FromImage(bmp);
            gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            gr.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;


            //����ѹ������
            EncoderParameters encoderParams = new EncoderParameters();
            long[] qualitys = new long[1];
            qualitys[0] = quality;

            EncoderParameter encoderParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, qualitys);
            encoderParams.Param[0] = encoderParam;

            //��ȡ���뷽ʽ
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
        /// ͼƬ����Զ�ѹ������
        /// </summary>
        /// <param name="width">ͼƬ����ȣ������ڸÿ��ʱͼƬ���Զ�����ѹ������</param>
        public void CompressImage(int width)
        {
            this.CompressImage(width,int.MaxValue);
        }

        /// <summary>
        /// ͼƬ����Զ�ѹ������
        /// </summary>
        /// <param name="width">ͼƬ����ȣ������ڸÿ��ʱͼƬ���Զ�����ѹ������,��ֵС��ͼƬ��ͼʱ�Զ��Ը߶����ŵȱȼ���</param>
        /// <param name="height">ͼƬ���߶ȣ���ֵС��ͼƬ�߶�ʱ�Զ��Կ�ͼ���ŵȱȼ���</param>
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

            //����
            if (ww > width)
            {
                double _w = (double)width / (double)ww;
                hh = (int)(_w * hh);
                ww = width;
            }

            //����
            if (hh > height)
            {
                double _h = (double)height / (double)hh;
                ww = (int)(_h * ww);
                hh = height;
            }

            Bitmap bmp = new Bitmap(ww, hh);
            //��Bitmap����һ��System.Drawing.Graphics�����������Ƹ���������Сͼ��
            Graphics gr = Graphics.FromImage(bmp);
            //���� System.Drawing.Graphics�����SmoothingMode����ΪHighQuality
            gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            //�������Ҳ��ɸ�����
            gr.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            //����������High
            gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
            //��ԭʼͼ����Ƴ����������ÿ�ߵ���Сͼ
            System.Drawing.Rectangle rectDestination = new System.Drawing.Rectangle(0, 0, ww, hh);
            gr.DrawImage(image, rectDestination, 0, 0, w, h, GraphicsUnit.Pixel);
            //����ͼ�񣬴󹦸�ɣ�
            MemoryStream ms2 = new MemoryStream();
            bmp.Save(ms2,@if);
            bmp.Dispose();
            image.Dispose();

            this.stream.Dispose();
            this.stream = ms2;
        }

        #endregion

        #region IDisposable ��Ա

        /// <summary>
        /// �ͷ���ռ�õ���Դ
        /// </summary>
        public void Dispose()
        {
            stream.Dispose();
        }

        #endregion
    }
}
