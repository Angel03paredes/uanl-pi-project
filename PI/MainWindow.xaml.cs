using AForge.Video;
using AForge.Video.DirectShow;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace PI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //private IVideoSource _videoSource;
        BitmapImage play = new BitmapImage(new Uri("./image/play.png", UriKind.Relative));
        BitmapImage pause = new BitmapImage(new Uri("./image/stop.png", UriKind.Relative));

        Bitmap bitmap;

        VideoCapture capture;
        double TotalFrame;
        double Fps;
        int FrameNo;
        Boolean isVideo = false;
        Boolean isVideoPause = false;
        Boolean saveVideo = false;
        System.Drawing.Size vp;
        public MainWindow()
        {
            InitializeComponent();
            HandlerControlers(Visibility.Hidden);
            //ShowHistograma();
            WindowsFormsHost host = new WindowsFormsHost();
            System.Windows.Forms.Button btn = new System.Windows.Forms.Button();
            btn.Text = "mytnsi";
            host.Child = btn;
            grid1.Children.Add(host);
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void ButtonMin_Click(object sender, RoutedEventArgs e)
        {
           System.Windows.Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        private void ButtonMax_Click(object sender, RoutedEventArgs e)
        {
            if (System.Windows.Application.Current.MainWindow.WindowState == WindowState.Normal)
                System.Windows.Application.Current.MainWindow.WindowState = WindowState.Maximized;
            else
                System.Windows.Application.Current.MainWindow.WindowState = WindowState.Normal;
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void BtnManual_Click(object sender,RoutedEventArgs e)
        {
            Manual window = new Manual();
            window.ShowDialog();
        }
        private void BtnCamara_Click(object sender, RoutedEventArgs e)
        {
            Camara window = new Camara();
            window.ShowDialog();
        }

        private void Importar_Click(object sender, RoutedEventArgs e)
        {   
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.InitialDirectory = "c:\\";
            dlg.Filter = "Files (*.jpg;*.PNG;*.mp4)|*.jpg;*.PNG;*.mp4|All Files (*.*)|*.*";
            dlg.RestoreDirectory = true;
            bool? success = dlg.ShowDialog();
            
            if (success == true)
            {
                string selectedFileName = dlg.FileName;
                string ext = System.IO.Path.GetExtension(dlg.FileName); 
                if(ext == ".mp4" || ext == ".mpeg")
                {
                    try
                    {
                        isVideo = true;
                        saveVideo = true;
                        capture = new VideoCapture(selectedFileName);
                        var vpSet = capture.QueryFrame();
                        vp = new System.Drawing.Size(vpSet.Width, vpSet.Height);
                        // Mat m = new Mat();
                        //capture.Read(m);

                        //Bitmap bmpi = m.ToImage<Bgr, Byte>().ToBitmap();
                        //ImageEdit.Source = Helpers.Convert(m.ToBitmap());

                        TotalFrame = capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameCount);
                        Fps = capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Fps);
                        HandlerControlers(Visibility.Visible);
                        ReadAllFrames();
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show(ex.Message);
                    }

                }
                else
                {
                    isVideo = false;
                    saveVideo = false;
                    bitmap = new Bitmap(selectedFileName);
                    HandlerControlers(Visibility.Hidden);
                    ImageEdit.Source = new BitmapImage(new Uri(selectedFileName));
                }
                
            } 
        }

        private async void ReadAllFrames()
        {

            Mat m = new Mat();
            int mult = Convert.ToInt32(TotalFrame) / Convert.ToInt32(Fps);
            while (isVideo == true && FrameNo < TotalFrame)
            {
                FrameNo += mult; //Convert.ToInt16(numericUpDown1.Value);
               
                capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.PosFrames, FrameNo);
                
                capture.Read(m);
                if(m.GetData() != null)
                {
                    Bitmap bmpi = m.ToImage<Bgr, Byte>().ToBitmap();
                    ImageEdit.Source = Helpers.Convert(bmpi);
                    await Task.Delay(1000 / Convert.ToInt16(Fps));
                }
                else
                {
                    isVideo = false;
                    
                }
            }
        }

        

        private async void SaveClick(object sender,RoutedEventArgs e)
        {
            try
            {
                await Task.Run(() =>
                {

                    Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                    dlg.Filter = "Files (*.jpg;*.PNG;*.mp4)|*.jpg;*.PNG;*.mp4|All Files (*.*)|*.*";

                    dlg.RestoreDirectory = false;
                    bool? success = dlg.ShowDialog();
                    if (success == true)
                    {
                        if (!saveVideo)
                            bitmap.Save(dlg.FileName, ImageFormat.Png);
                        else
                        {

                            VideoWriter VideoW = new VideoWriter(dlg.FileName,
                                           VideoWriter.Fourcc('M', 'P', '4', 'V'),
                                            Convert.ToInt32(Fps),
                                          new System.Drawing.Size(vp.Width, vp.Height),
                                           true);
                            Mat m = new Mat();
                            var si = 0;
                            int mult = Convert.ToInt32(TotalFrame) / Convert.ToInt32(Fps);
                            while (si < TotalFrame)
                            {
                                si += 1;

                                capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.PosFrames, si);
                                capture.Read(m);
                                if (m.GetData() != null)
                                {
                                    VideoW.Write(m);
                                }

                            }

                        }

                    }

                });
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error al guardar imagen " + ex.ToString());
            }
        }

       

       private void Stop_Click(object sender, RoutedEventArgs e)
        {
            if (isVideoPause)
            {
                BtnStopPlay.Source = pause;
                isVideo = true;
                isVideoPause = false;
                ReadAllFrames();
            }
            else
            {
                BtnStopPlay.Source = play;
                isVideo = false;
                isVideoPause = true;
            }
          
        }

    


        private void Reload_Click(object sender, RoutedEventArgs e)
        {
            BtnStopPlay.Source = pause;
            isVideoPause = false;
            isVideo = true;
            FrameNo = 0;
            ReadAllFrames();
        }

        private void HandlerControlers( Visibility visibility)
        {
            BtnPlay.Visibility = visibility;
            BtnReplay.Visibility = visibility;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem typeItem = (ComboBoxItem)AddFilters.SelectedItem;
            var value = typeItem.Content;
            Filtros filtros = new Filtros();
            switch (value)
            {
                case "Sepia":
                    
                    Bitmap bmpSepia =  filtros.Sepia(bitmap);
                    bitmap = bmpSepia;
                    ImageEdit.Source = Helpers.Convert(bmpSepia);
                   
                    break;

                case "Glitch":
                    filtros.Glitch();
                    break;

                case "Escala de grises":
                    Bitmap bmpEdG = filtros.EscalaDeGrises(bitmap);
                    bitmap = bmpEdG;
                    ImageEdit.Source = Helpers.Convert(bmpEdG);
                    break;

                case "Sobel":
                    filtros.Sobel();
                    break;

                case "Laplaciano":
                    filtros.Laplaciano();
                    break;
            }
        }


        private void ShowHistograma()
        {
            float[] BlueHist;
            float[] GreenHist;
            float[] RedHist;

            Image<Bgr, Byte> imgHist = new Image<Bgr, byte>("C:/Users/angel/Desktop/conejitobonito.jpg");

            DenseHistogram Histo = new DenseHistogram(255, new RangeF(0, 255));

            Image<Gray, Byte> img2Blue = imgHist[0];
            Image<Gray, Byte> img2Green = imgHist[1];
            Image<Gray, Byte> img2Red = imgHist[2];
        
            // Create the MaskedTextBox control.


            // Assign the MaskedTextBox control as the host control's child.

            // Add the interop host control to the Grid
            // control's collection of child controls.

            //  HistogramBox1.ClearHistogram();
            //  HistogramBox1.GenerateHistograms(img2Red, 256);
            //  HistogramBox1.Refresh();

            // Histo.Calculate(new Image<Gray, Byte>[] { img2Blue}, true, null);
            //The data is here
            //Histo.MatND.ManagedArray
            //BlueHist = new float[256];
            // Histo.Calculate<Byte>(new Image<Gray, byte>[] { img2Blue }, true, null); 
            // Histo.MatND.ManagedArray.CopyTo(BlueHist, 0);

            // Histo.Clear();

        }


    }
}
