using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PI
{
    class Helpers
    {
        public static BitmapImage Convert(Bitmap src)
        {
          if(src != null)
            {
                MemoryStream ms = new MemoryStream();
                ((System.Drawing.Bitmap)src).Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                ms.Seek(0, SeekOrigin.Begin);
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
            else
            {
                return new BitmapImage();
            }
            
        }

        public static Color Lerp( Color s, Color t, float k)
        {
            var bk = (1 - k);
            var a = s.A * bk + t.A * k;
            var r = s.R * bk + t.R * k;
            var g = s.G * bk + t.G * k;
            var b = s.B * bk + t.B * k;
            return Color.FromArgb((int)a, (int)r, (int)g, (int)b);
        }

        public static Bitmap BorderBitmap(int[,] mat , Bitmap grayscale,int pInf , int pSup) 
        {
            Color color;

            
            int width = grayscale.Width ;
            int height = grayscale.Height ;
            Bitmap newBitmap = new Bitmap(width,height);

            int suma = 0;
            for(int x = 1; x < width -1; x++)
            {
                for (int y = 1; y < height -1; y++)
                {
                    suma = 0;
                    for (int i = -1; i < 2; i++)
                    {
                        for(int j = -1; j < 2; j++)
                        {
                            color = grayscale.GetPixel(x + i,y + j);
                            suma = suma + (color.R * mat[i + 1, j + 1]);
                        }
                    }
                    if (suma < pInf)
                        suma = 0;
                    else if (suma > pSup)
                        suma = 255;

                    newBitmap.SetPixel(x, y, Color.FromArgb(suma, suma, suma));
                }
            }



            return newBitmap;
        }

       

    }

   
}
