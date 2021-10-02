
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
using Emgu.CV.UI;
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
      
        HistogramBox histogramR = new HistogramBox();
        HistogramBox histogramG = new HistogramBox();
        HistogramBox histogramB = new HistogramBox();
        Bitmap bitmap;
        Bitmap bitmapClean;

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
                        bitmap = null;
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
                    bitmapClean = (Bitmap)bitmap.Clone();
                    HandlerControlers(Visibility.Hidden);
                    ImageEdit.Source =Helpers.Convert(bitmap);
                    ShowHistograma();
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

                    if (m.GetData() != null)
                    {
                        Bitmap bmpi = m.ToImage<Bgr, Byte>().ToBitmap();
                       // Bitmap filBmp = Filtros.Sepia(bmpi);
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
          if(bitmap != null || saveVideo)
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
            else
            {
                System.Windows.MessageBox.Show("No hay recurso ");
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
            if (bitmap != null )
            {
           
                
                switch (value)
                {
                    case "Sepia":

                        Bitmap bmpSepia = Filtros.Sepia(bitmap);
                        bitmap = bmpSepia;
                        ImageEdit.Source = Helpers.Convert(bmpSepia);
                        ShowHistograma();
                        AddItemList("Sepia");
                        break;

                    case "Glitch":
                        Filtros.Glitch();
                        break;

                    case "Escala de grises":
                        Bitmap bmpEdG = Filtros.EscalaDeGrises(bitmap);
                        bitmap = bmpEdG;
                        ImageEdit.Source = Helpers.Convert(bmpEdG);
                        ShowHistograma();
                        AddItemList("Escala de grises");
                        break;

                    case "Sobel":
                        Filtros.Sobel();
                        AddItemList("Sobel");
                        break;

                    case "Laplaciano":
                        Filtros.Laplaciano();
                        AddItemList("Laplaciano");
                        break;
                }
            }

            AddFilters.SelectedIndex = 0;
        }


        private void ShowHistograma()
        {
            Mat imga = bitmap.ToMat();
            Image<Bgr, Byte> imgHist = imga.ToImage<Bgr, Byte>(); // imga.ToImage<Bgr, Byte>().ToBitmap();

            Image<Gray, Byte> imgBlue = imgHist[0];
            Image<Gray, Byte> imgGreen = imgHist[1];
            Image<Gray, Byte> imgRed = imgHist[2];


            DenseHistogram histR = new DenseHistogram(256,new RangeF(0,255));
            histR.Calculate(new Image<Gray, byte>[] { imgRed }, false, null);
            Mat mR = new Mat();
            histR.CopyTo(mR);
            histogramR.ClearHistogram();
            Mat mHistR = histogramR.GenerateHistogram("RED",System.Drawing.Color.Red,mR,256,new float[] { 0,256});
            imageHistR.Source =Helpers.Convert(mHistR.ToImage<Bgr, Byte>().ToBitmap());

            DenseHistogram histG = new DenseHistogram(256, new RangeF(0, 255));
            histG.Calculate(new Image<Gray, byte>[] { imgGreen }, false, null);
            Mat mG = new Mat();
            histG.CopyTo(mG);
            histogramG.ClearHistogram();
            Mat mHistG = histogramR.GenerateHistogram("GREEN", System.Drawing.Color.Green, mG, 256, new float[] { 0, 256 });
            imageHistG.Source = Helpers.Convert(mHistG.ToImage<Bgr, Byte>().ToBitmap());

            DenseHistogram histB = new DenseHistogram(256, new RangeF(0, 255));
            histB.Calculate(new Image<Gray, byte>[] { imgBlue }, false, null);
            Mat mB = new Mat();
            histB.CopyTo(mB);
            histogramB.ClearHistogram();
            Mat mHistB = histogramR.GenerateHistogram("BLUE", System.Drawing.Color.Blue, mB, 256, new float[] { 0, 256 });
            imageHistB.Source = Helpers.Convert(mHistB.ToImage<Bgr, Byte>().ToBitmap());


        }

        private  void Clean_Click(object sender, RoutedEventArgs e)
        {

            bitmap = (Bitmap)bitmapClean.Clone();
            ImageEdit.Source = Helpers.Convert(bitmap);
            ShowHistograma();
            listFilters.Items.Clear();

        }

        private void AddItemList(String filter)
        {
            System.Windows.Controls.ListViewItem item = new System.Windows.Controls.ListViewItem { Background=(System.Windows.Media.Brush)Resources["Primary"]};
            StackPanel stp = new StackPanel { Orientation = System.Windows.Controls.Orientation.Horizontal };
            System.Windows.Controls.Image img = new System.Windows.Controls.Image { Source = Helpers.Convert(bitmap), Width = 40, Height = 40 };
            TextBlock txt = new TextBlock { Text = filter, Foreground=(System.Windows.Media.Brush)Resources["Font2"], Margin=new Thickness { Top = 0, Bottom = 0, Right = 5, Left = 5 }, VerticalAlignment =VerticalAlignment.Center };
            stp.Children.Add(img);
            stp.Children.Add(txt);
            item.Content = stp;
            listFilters.Items.Add(item);
        }

     
      
        

    }
}
