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
using AForge.Video;
using AForge.Video.DirectShow;

namespace PI
{
    /// <summary>
    /// Interaction logic for Camara.xaml
    /// </summary>
    public partial class Camara : Window
    {

        FilterInfoCollection filterInfoCollection;
        VideoCaptureDevice videoCaptureDevice;

        public Camara()
        {
            InitializeComponent();
           Camera();
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
            videoCaptureDevice = new VideoCaptureDevice(filterInfoCollection[CmbFilter.SelectedIndex].MonikerString);
            videoCaptureDevice.NewFrame += VideoCaptureDevice_NewFrame;
            videoCaptureDevice.Start();
        }

        private void VideoCaptureDevice_NewFrame(object sender,NewFrameEventArgs eventArgs)
        {
            //Application.Current.Dispatcher.Invoke(new Action(() =>
            //{
            //    ImageCam.Source = eventArgs.Frame.Clone() as BitmapImage;
            //}));

            try
            {
                BitmapImage bi;
                using (var bitmap = (Bitmap)eventArgs.Frame.Clone())
                {
                    bi = bitmap.ToBitmapImage();
                }
                bi.Freeze(); // avoid cross thread operations and prevents leaks
                Dispatcher.BeginInvoke(new ThreadStart(delegate { ImageCam.Source = bi; }));
            }
            catch (Exception exc)
            {
                MessageBox.Show("Error on _videoSource_NewFrame:\n" + exc.Message, "Error");
        
              StopCamera();
            }
        }

        private void Camera()
        {
            filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo filterInfo in filterInfoCollection)
                CmbFilter.Items.Add(filterInfo.Name);
            CmbFilter.SelectedIndex = 0;
            videoCaptureDevice = new VideoCaptureDevice();
        }
        private void StopCamera()
        {
            if (videoCaptureDevice != null && videoCaptureDevice.IsRunning)
            {
                videoCaptureDevice.SignalToStop();
                videoCaptureDevice.NewFrame -= new NewFrameEventHandler(VideoCaptureDevice_NewFrame);
            }
        }
    }
}
