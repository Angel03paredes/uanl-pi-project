using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PI
{
    class Filtros
    {
        public static  Bitmap Sepia(Bitmap bitmap)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            Color p;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    p = bitmap.GetPixel(x, y);

                    int a = p.A;
                    int r = p.R;
                    int g = p.G;
                    int b = p.B;

                    int tr = (int)(0.393 * r + 0.769 * g + 0.189 * b);
                    int tg = (int)(0.349 * r + 0.686 * g + 0.168 * b);
                    int tb = (int)(0.272 * r + 0.534 * g + 0.131 * b);

                    if (tr > 255)
                    {
                        r = 255;
                    }
                    else
                    {
                        r = tr;
                    }

                    if (tg > 255)
                    {
                        g = 255;
                    }
                    else
                    {
                        g = tg;
                    }

                    if (tb > 255)
                    {
                        b = 255;
                    }
                    else
                    {
                        b = tb;
                    }

                    bitmap.SetPixel(x, y, Color.FromArgb(a, r, g, b));
                }
            }
            return bitmap;
        }
        public static Bitmap Glitch(Bitmap bitmap)
        {
            Random random = new Random();
            Color p;
            Color p2;
            Color p3;
             int width = bitmap.Width;
            int height = bitmap.Height;
            int factWidth = width / 4;
            int factHeight = height / 12;
            float mixPercentage;
            for (int y = 0; y < height; y++)
            {
                mixPercentage = (float)(.5 + random.Next(0, 50) / 100);
                for (int x = 0; x < width; x++)
                {
                    //randomColor = System.Drawing.Color.FromArgb(255, random.Next(0, 255), random.Next(0, 255), random.Next(0, 255));
                  
                    
                    
                    if (x + 30 < width && x - 30 > 0)
                        p2 = bitmap.GetPixel(x + 30, y);
                    
                    else
                        p2 = bitmap.GetPixel(x, y);
                    
                    p = bitmap.GetPixel(x, y);

                    if (x > factWidth && x < factWidth * 3 && y > factHeight && y < factHeight * 2)
                    {
                        p = bitmap.GetPixel(x + factWidth / 2, y);
                        p2 = bitmap.GetPixel(x + (factWidth + 30) / 2, y);
                    }

                    if (x > factWidth * 2 && x < factWidth * 3 && y > factHeight *4 && y < factHeight * 5)
                    {
                        p = bitmap.GetPixel(x + factWidth / 2, y);
                        p2 = bitmap.GetPixel(x + factWidth   / 2, y);
                    }

                    if ( y > factHeight * 7 && y < (factHeight * 7) + factHeight)
                    {
                        p = bitmap.GetPixel(x, y + factHeight *2);
                        p2 = bitmap.GetPixel(x, y +factHeight * 2);
                    }

                    Color newPixel = Helpers.Lerp(Color.FromArgb(255, p.R, p.G, p.B), Color.FromArgb(255,p.R,p2.G,p2.B), mixPercentage);
                    bitmap.SetPixel(x, y, newPixel);
                }
            }
            return bitmap;
        }
        public static Bitmap EscalaDeGrises(Bitmap bitmap)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            Color p;
            for (int y = 0; y < height; y++)
            {
                
                    for (int x = 0; x < width; x++)
                    {
                        Color c = bitmap.GetPixel(x, y);

                        int r = c.R;
                        int g = c.G;
                        int b = c.B;
                        int avg = (r + g + b) / 3;
                        bitmap.SetPixel(x, y, Color.FromArgb(avg, avg, avg));
                    }
               
            }
            return bitmap;
        }

        public static Bitmap Negativo(Bitmap bitmap)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            Color p;
            for (int y = 0; y < height; y++)
            {

                for (int x = 0; x < width; x++)
                {
                    Color c = bitmap.GetPixel(x, y);

                    int r = c.R;
                    int g = c.G;
                    int b = c.B;
                  
                    bitmap.SetPixel(x, y, Color.FromArgb(255 - r, 255 - g,  255 - b));
                }

            }
            return bitmap;
        }
        public  static Bitmap Sobel(Bitmap bitmap)
        {
            int[,] sobelMat = new int[,]
            {
                { 1,2,1},
                { 0,0,0 },
                { -1,-2,-1}
            };

            Bitmap gray = EscalaDeGrises((Bitmap)bitmap.Clone());
            Bitmap sobel = Helpers.BorderBitmap(sobelMat, gray, 0, 255);
            return sobel;

        }
        public static Bitmap Laplaciano(Bitmap bitmap)
        {
            int[,] sobelMat = new int[,]
           {
                { 1,1,1},
                { 1,-8,1},
                { 1,1,1}
           };

            Bitmap gray = EscalaDeGrises((Bitmap)bitmap.Clone());
            Bitmap sobel = Helpers.BorderBitmap(sobelMat, gray, 32, 64);
            return sobel;
        }
    }
}
