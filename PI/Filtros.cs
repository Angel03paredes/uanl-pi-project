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
        public static Bitmap Sepia(Bitmap bitmap)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            Color p;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    //get pixel value
                    p = bitmap.GetPixel(x, y);

                    //extract pixel component ARGB
                    int a = p.A;
                    int r = p.R;
                    int g = p.G;
                    int b = p.B;

                    //calculate temp value
                    int tr = (int)(0.393 * r + 0.769 * g + 0.189 * b);
                    int tg = (int)(0.349 * r + 0.686 * g + 0.168 * b);
                    int tb = (int)(0.272 * r + 0.534 * g + 0.131 * b);

                    //set new RGB value
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

                    //set the new RGB value in image pixel
                    bitmap.SetPixel(x, y, Color.FromArgb(a, r, g, b));
                }
            }
            return bitmap;
        }
        public static void Glitch()
        {
            //TODO glitch Filter
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
        public static void Sobel()
        {
            //TODO Sobel filter
        }
        public static void Laplaciano()
        {
            //TODO Laplaciano Filter
        }
    }
}
