using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
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
        BitmapImage play = new BitmapImage(new Uri("./image/play.png", UriKind.Relative));
        BitmapImage pause = new BitmapImage(new Uri("./image/stop.png", UriKind.Relative));
        Emgu.CV.VideoCapture capture;
        System.Windows.Forms.PictureBox pb = new System.Windows.Forms.PictureBox();
        static readonly CascadeClassifier cascade = new CascadeClassifier("./data/haarcascade_frontalface_default.xml");
        
        Boolean captureFrame = false;
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
            ShowControls(Visibility.Visible);


        }

        private void Capture_ImageGrabbed(object sender, EventArgs e)
        {
            if (capture != null &&  !captureFrame)
            {
                try
                {
                    Mat m = new Mat();
                    capture.Retrieve(m);
                    Bitmap bit = m.ToBitmap();
                    Image<Gray, byte> grayImage = bit.ToImage<Gray, byte>();
                    System.Drawing.Rectangle[] rectangles = cascade.DetectMultiScale(grayImage,1.1,3);
                    var si = 0;
                    
                    foreach(System.Drawing.Rectangle rectangulo in rectangles)
                    {
                        si++;
                        //System.Drawing.Color.FromArgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255))
                        using (Graphics graphics = Graphics.FromImage(bit))
                        {
                            using (System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.Red, 3))
                            {
                                String persona = "Persona" + si.ToString();
                                graphics.DrawRectangle(pen, rectangulo);
                                graphics.DrawString(persona,new Font("Segoe UI",12), new System.Drawing.SolidBrush(System.Drawing.Color.Red), new PointF(rectangulo.X,rectangulo.Y));
                            }
                        }
                        
                   }
                    // Para mostrar cuantas personas se estan leyendo
                    using (Graphics graphics = Graphics.FromImage(bit))
                    {
                            String persona = "Conteo de personas:  " + si.ToString();
                            
                            graphics.DrawString(persona, new Font("Segoe UI", 22), new System.Drawing.SolidBrush(System.Drawing.Color.Blue), new PointF(0,0));
                       
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
                capture.Dispose();
            }
        }

        private void StopCam_Click(object sender, RoutedEventArgs e)
        {
            if (captureFrame)
                BtnStopPlay.Source = pause;
            else
                BtnStopPlay.Source = play;
            
            captureFrame = !captureFrame;
        }

        private void SaveCam_Click(object sender, RoutedEventArgs e)
        {

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.Filter = "Files (*.jpg;*.PNG)|*.jpg;*.PNG|All Files (*.*)|*.*";

            dlg.RestoreDirectory = false;
            bool? success = dlg.ShowDialog();
            if (success == true)
            {

                Bitmap bit = new Bitmap(pb.Image);
                bit.Save(dlg.FileName, ImageFormat.Png);


            }

        }

        private void ShowControls(Visibility visibility)
        {
            SaveCam.Visibility = visibility;
            StopCam.Visibility = visibility;
            
            
        }
    }
}
