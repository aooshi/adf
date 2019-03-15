using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Adf
{
    /// <summary>
    /// 基本类型
    /// </summary>
    public class AuthcodeHelper
    {
        static Random random = new Random();

        /// <summary>
        /// 创建一个图片,图片类型Gif
        /// </summary>
        /// <param name="outputStream"></param>
        /// <param name="code"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="fontSize"></param>
        public static void CreateImage(Stream outputStream, string code, int width, int height, int fontSize)
        {
            using (Bitmap img = new Bitmap(width, height))
            using (Graphics g = Graphics.FromImage(img))
            {
                g.FillRectangle(new SolidBrush(Color.Bisque), new Rectangle(0, 0, width, height));

                using (Font ft = new Font("Arial", fontSize, (FontStyle.Bold)))
                    g.DrawString(code, ft, new SolidBrush(Color.Black), 2, 1);

                img.Save(outputStream, ImageFormat.Gif);
            }
        }


        /// <summary>
        /// 创建一个图片,图片类型Gif
        /// </summary>
        /// <param name="outputStream"></param>
        /// <param name="code"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="fontSize"></param>
        /// <param name="noiseCount"></param>
        public static void CreateImage(Stream outputStream, string code, int width, int height, int fontSize, int noiseCount=25)
        {
            int x1, x2, y1, y2;

            using(System.Drawing.Bitmap image = new System.Drawing.Bitmap(width, height))
            using(Graphics g = Graphics.FromImage(image))
            {
                //清空图片背景色
                g.Clear(Color.White);

                //画图片的背景噪音线
                for (int i = 0; i < noiseCount; i++)
                {
                    x1 = random.Next(image.Width);
                    x2 = random.Next(image.Width);
                    y1 = random.Next(image.Height);
                    y2 = random.Next(image.Height);

                    g.DrawLine(new Pen(Color.Silver), x1, y1, x2, y2);
                }

                using (Font font = new System.Drawing.Font("Arial", fontSize, (System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic)))
                {
                    using (System.Drawing.Drawing2D.LinearGradientBrush brush = new System.Drawing.Drawing2D.LinearGradientBrush(new Rectangle(0, 0, image.Width, image.Height), Color.Blue, Color.DarkRed, 1.2f, true))
                        g.DrawString(code, font, brush, 2, 2);
                }

                //画图片的前景噪音点
                for (int i = 0; i < noiseCount; i++)
                {
                    int x = random.Next(image.Width);
                    int y = random.Next(image.Height);

                    image.SetPixel(x, y, Color.FromArgb(random.Next()));
                }

                //画图片的边框线
                g.DrawRectangle(new Pen(Color.Silver), 0, 0, image.Width - 1, image.Height - 1);

                image.Save(outputStream, System.Drawing.Imaging.ImageFormat.Gif);
            }
        }


        /// <summary>
        /// 画笔
        /// </summary>
        static Brush[] BrushItems = new Brush[] {     Brushes.OliveDrab,
                                                      Brushes.ForestGreen,
                                                      Brushes.DarkCyan,
                                                      Brushes.LightSlateGray,
                                                      Brushes.RoyalBlue,
                                                      Brushes.SlateBlue,
                                                      Brushes.DarkViolet,
                                                      Brushes.MediumVioletRed,
                                                      Brushes.IndianRed,
                                                      Brushes.Firebrick,
                                                      Brushes.Chocolate,
                                                      Brushes.Peru,
                                                      Brushes.Goldenrod
        };
        static int BurshItemLength = BrushItems.Length;
        /// <summary>
        /// 创建一个图片,图片类型Gif
        /// </summary>
        /// <param name="outputStream"></param>
        /// <param name="code"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="fontSize"></param>
        /// <param name="noiseCount"></param>
        public static void CreateImage2(Stream outputStream, string code, int width, int height, int fontSize, int noiseCount = 25)
        {
            using (var objBitmap = new Bitmap(width, height))
            using (var g = Graphics.FromImage(objBitmap))
            {
                //绘画背景颜色
                g.Clear(Color.White);

                //绘画文字
                g.DrawString(code, new Font("Arial", fontSize, FontStyle.Bold), BrushItems[random.Next(0, BurshItemLength)], 3, 1);  
                
                // 绘画文字噪音点
                for (int n = 0; n < noiseCount; n++)
                {
                    int x = random.Next(width);
                    int y = random.Next(height);
                    objBitmap.SetPixel(x, y, Color.Black);
                }

                // 绘画边框
                g.DrawRectangle(Pens.DarkGray, 0, 0, width - 1, height - 1);
                //
                objBitmap.Save(outputStream, ImageFormat.Gif);
            }
        }
    }
}
