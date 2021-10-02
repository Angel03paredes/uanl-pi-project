using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;


namespace PI
{
    /// <summary>
    /// Interaction logic for Camara.xaml
    /// </summary>
    public partial class Camara : Window
    {

        Emgu.CV.VideoCapture capture;
        System.Windows.Forms.PictureBox pb = new System.Windows.Forms.PictureBox();
        static readonly CascadeClassifier cascade = new CascadeClassifier("./data/haarcascade_frontalface_default.xml");
        private static readonly Random random = new Random();
        // IBackgroundSubtractor backgroundSubstractor;
        public Camara()
        {
            InitializeComponent();
            host.Child = pb;

        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void ButtonMin_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void ButtonMax_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
                this.WindowState = WindowState.Maximized;
            else
                this.WindowState = WindowState.Normal;
        }
        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            StopCamera();
            this.Close();
        }

        private void InitCam_Click(object sender, RoutedEventArgs e)
        {
            capture = null;
            Camera();
        }




        private void Camera()
        {
            if (capture == null)
            {
                capture = new VideoCapture(0);
            } 
           // backgroundSubstractor = new Emgu.CV.BgSegm.BackgroundSubtractorMOG();//BackgroundSubtractorKNN(200, 20, false);
            capture.ImageGrabbed += Capture_ImageGrabbed;
            capture.Start();


        }

        private void Capture_ImageGrabbed(object sender, EventArgs e)
        {
            if (capture != null)
            {
                try
                {
                    Mat m = new Mat();
                    capture.Retrieve(m);
                    Bitmap bit = m.ToBitmap();
                    Image<Gray, byte> grayImage = bit.ToImage<Gray, byte>();
                    System.Drawing.Rectangle[] rectangles = cascade.DetectMultiScale(grayImage,1.4,0);

                    
                    foreach(System.Drawing.Rectangle rectangulo in rectangles)
                    {
                        using(Graphics graphics = Graphics.FromImage(bit))
                        {
                            using (System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)), 1))
                            {
                                graphics.DrawRectangle(pen, rectangulo);
                                
                            }
                        }
                    }
                    pb.Image = bit;

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al grabar frames " + ex.ToString());
                }
            }
        }

        private void StopCamera()
        {
            if (capture != null)
            {
                capture.Stop();
            }
        }

       
    }
}
