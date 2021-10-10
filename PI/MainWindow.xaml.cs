
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
       // VideoWriter videoW;
        HistogramBox histogramR = new HistogramBox();
        HistogramBox histogramG = new HistogramBox();
        HistogramBox histogramB = new HistogramBox();
        Bitmap bitmap;
        Bitmap bitmapClean;
        List<Bitmap> listBitamp = new List<Bitmap>();
        
        EnumFiltros FiltroVideo = EnumFiltros.None;

        VideoCapture capture;
        double TotalFrame;
        double Fps;
        int FrameNo;
        Boolean isVideo = false;
        Boolean isVideoPause = false;
        Boolean saveVideo = false;
        Guid nameTempVideo;
        System.Drawing.Size vp;
        Process progressBar;
        public MainWindow()
        {
            InitializeComponent();
            cmbSpeed.SelectedIndex = 0;
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

        private async void Importar_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                await Task.Run(() => {
                    Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                    dlg.InitialDirectory = "c:\\";
                    dlg.Filter = "Files |*.jpg;*.png;*.mp4|JPG (*.jpg)|*.jpg|PNG(*.png)|*.png|MP4(*.mp4)|*.mp4|All Files (*.*)|*.*";
                    dlg.RestoreDirectory = true;
                    bool? success = dlg.ShowDialog();

                    if (success == true)
                    {


                        Dispatcher.Invoke(() =>
                        {
                            listFilters.Items.Clear();
                        });

                        string selectedFileName = dlg.FileName;
                        string ext = System.IO.Path.GetExtension(dlg.FileName);
                        if (ext == ".mp4" || ext == ".mpeg")
                        {

                            if (capture != null && !isVideoPause)
                            {
                                System.Windows.MessageBox.Show("Pausa el video antes de importar","Aviso",MessageBoxButton.OK,MessageBoxImage.Warning);
                                goto Finish;
                            }
                            VideoCapture captureTemp = new VideoCapture(selectedFileName);
                            var vpSet = captureTemp.QuerySmallFrame();
                            vp = new System.Drawing.Size(vpSet.Width, vpSet.Height);
                            TotalFrame = captureTemp.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameCount);
                            Fps = captureTemp.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Fps);

                            if ((vpSet.Width * 2) < 650)
                            {
                                listBitamp.Clear();
                                listBitamp = new List<Bitmap>();
                                Dispatcher.Invoke(() => {
                                    BtnReplay.Visibility = Visibility.Hidden;
                                    listFilters.Items.Clear();
                                    BtnStopPlay.Source = pause;
                                });

                                isVideoPause = false;
                                FrameNo = 0;
                                bitmap = null;
                                isVideo = true;
                                saveVideo = true;
                                capture = captureTemp;
                                Dispatcher.Invoke(() =>
                                {
                                    HandlerControlers(Visibility.Visible);
                                });
                                ConvertToList();
                              //  ReadAllFrames();
                            }
                            else
                            {
                                System.Windows.MessageBox.Show("El video debe ser una resolución menor, menor a 640 x 480","aviso",MessageBoxButton.OK,MessageBoxImage.Warning);
                            }



                        }
                        else
                        {
                            isVideo = false;
                            saveVideo = false;
                            bitmap = new Bitmap(selectedFileName);
                            bitmapClean = (Bitmap)bitmap.Clone();

                            HandlerControlers(Visibility.Hidden);
                            Dispatcher.Invoke(() => { ImageEdit.Source = Helpers.Convert(bitmap); });
                            ShowHistograma();
                       
                        }
                    Finish:
                        System.Diagnostics.Debug.WriteLine("Salta el proceso de importar video");
                    }

                });
            }
            catch(Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }
        }

        private async void ConvertToList()
        {
            
            await Task.Run(() => {
                var factor = 100 / TotalFrame;
                
                Dispatcher.Invoke(() => {
                     progressBar = new Process();
                    progressBar.Show();
                });

                Mat m = new Mat();
                // int mult = Convert.ToInt32(TotalFrame) / Convert.ToInt32(Fps);
                int i = 0;
                while (isVideo == true && i < TotalFrame)
                {
                    Dispatcher.Invoke(() =>
                    {
                        i++;
                        progressBar.ChangueValue(factor * i);
                        // Int16.Parse(cmbSpeed.Text); //Convert.ToInt16(numericUpDown1.Value);
                    });

                    capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.PosFrames, i);
                    m = capture.QuerySmallFrame();
                    if (m != null)
                    {
                        Bitmap bmpi = m.ToImage<Bgr, Byte>().ToBitmap();
                        listBitamp.Add(bmpi);
                    }
                
                }
                progressBar.CloseModal();
            });

        }



        //Useless
        private async void videoTemp()
        {
            await Task.Run(() =>
            {
                try
                {
                    VideoCapture videoTemp = capture;
                    nameTempVideo = Guid.NewGuid();
                    VideoWriter VideoW = new VideoWriter("C:/Users/angel/Desktop/PI/PI/PI/videos/"+nameTempVideo.ToString()+ ".mp4",
                                   VideoWriter.Fourcc('M', 'P', '4', 'V'),
                                    Convert.ToInt32(Fps),
                                  new System.Drawing.Size(vp.Width, vp.Height),
                                   true);
                    Mat m ;
                    var frameNoVideoW = 0;
                    while (frameNoVideoW < TotalFrame)
                    {
                        frameNoVideoW += 1;

                        videoTemp.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.PosFrames, frameNoVideoW);
                        m = videoTemp.QuerySmallFrame();
                        if (m != null)
                        {
                            VideoW.Write(m);
                        }

                    }
                    videoTemp = null;
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.ToString());
                }
            });
        }

        private  async void ReadAllFrames()
        {
            await Task.Run( () =>
            {

            try
            {
                 Mat m = new Mat();
                    // int mult = Convert.ToInt32(TotalFrame) / Convert.ToInt32(Fps);
                    
                    while (isVideo == true && FrameNo < TotalFrame)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            FrameNo += Int16.Parse( cmbSpeed.Text); //Convert.ToInt16(numericUpDown1.Value);
                        });
                       


                        capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.PosFrames, FrameNo);
                        m = capture.QuerySmallFrame();
                        if (m != null)
                        {


                            Bitmap bmpi = m.ToImage<Bgr, Byte>().ToBitmap();
                            Bitmap newBitmap = new Bitmap(100,100);
                            // Bitmap filBmp =  Filtros.Sobel(bmpi);
                            switch(FiltroVideo){
                                case EnumFiltros.Sepia:
                                    newBitmap = Filtros.Sepia(bmpi);
                                    break;
                                case EnumFiltros.EscalaDeGrises:
                                    newBitmap = Filtros.EscalaDeGrises(bmpi);
                                    break;
                                case EnumFiltros.Glitch:
                                    newBitmap = Filtros.Glitch(bmpi);
                                    break;
                                case EnumFiltros.Sobel:
                                    newBitmap = Filtros.Sobel(bmpi);
                                    break;
                                case EnumFiltros.Laplaciano:
                                    newBitmap = Filtros.Laplaciano(bmpi);
                                    break;
                                default:
                                   newBitmap = (Bitmap)bmpi.Clone();
                                    break;
                            }
                            
                                listBitamp.Add(newBitmap);
                            
                            ShowHistograma((Bitmap)newBitmap.Clone());
                            this.Dispatcher.Invoke(() =>
                            {
                                if(newBitmap != null)
                                    ImageEdit.Source = Helpers.Convert(newBitmap);
                            });

                         //  await Task.Delay(1000 / Convert.ToInt16(Fps ));
                        }
                        else
                        {
                            isVideo = false;

                        }

                        if(FrameNo + 1 >= TotalFrame)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                BtnReplay.Visibility = Visibility.Visible;
                            });
                            }
                    }

              
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }
            });
        }

        

        private  void SaveClick(object sender,RoutedEventArgs e)
        {
            isVideoPause = false;
            Stop_Click(sender,e);
            Thread proccessSave = new Thread(Proccessvideo);
            proccessSave.Start();


          
        }

       private  void Proccessvideo()
        {
            if (bitmap != null || saveVideo)
            {
                try
                {

                    if (!saveVideo)
                    {
                        Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                        dlg.Filter = "JPG (*.jpg)|*.jpg| PNG (*.png)|*.png";

                        dlg.RestoreDirectory = false;
                        bool? success = dlg.ShowDialog();
                        if (success == true)
                        {
                            bitmap.Save(dlg.FileName, ImageFormat.Png);
                        }
                    }
                    else
                    {

                        Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                        dlg.Filter = "MP4 (*.mp4)|*.mp4";

                        dlg.RestoreDirectory = false;
                        bool? success = dlg.ShowDialog();
                        if (success == true)
                        {

                            VideoCapture writeCapture = capture;

                            VideoWriter VideoW = new VideoWriter(dlg.FileName,
                                           VideoWriter.Fourcc('P', 'I', 'M', '3'),
                                            Convert.ToInt32(Fps),
                                          new System.Drawing.Size(vp.Width, vp.Height),
                                           true);
                            Mat m = new Mat();
                            foreach(var bmp in listBitamp)
                            {
                              
                                //writeCapture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.PosFrames, frameNoVideoW);
                                m = bmp.ToMat();  //writeCapture.QuerySmallFrame();
                                if (m != null)
                                {
                                  
                                        VideoW.Write(m);
                                  
                                }
                             

                            }
                            
                            if (listBitamp.Count < TotalFrame )
                            {
                                var tempIterator = listBitamp.Count;
                                while(tempIterator < TotalFrame)
                                {

                                    writeCapture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.PosFrames, tempIterator);
                                    m = writeCapture.QuerySmallFrame();
                                    if (m != null)
                                    {
                                        VideoW.Write(m);
                                    }
                                    tempIterator++;
                                }    
                            }

                        }
                    }


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
                Dispatcher.Invoke(() => {
                    BtnStopPlay.Source = play;
                });
                isVideo = false;
                isVideoPause = true;
            }
          
        }

    


        private  void Reload_Click(object sender, RoutedEventArgs e)
        {
           
                BtnStopPlay.Source = pause;
                isVideoPause = false;
                isVideo = true;
                FrameNo = 0;
            listBitamp = new List<Bitmap>();
            BtnReplay.Visibility = Visibility.Hidden;
            ReadAllFrames();
          
        }

        private void HandlerControlers( Visibility visibility)
        {
            Dispatcher.Invoke(() => {
                BtnPlay.Visibility = visibility;
                cmbSpeed.Visibility = visibility;
                txtSpeed.Visibility = visibility;
            });
           // BtnReplay.Visibility = visibility;
        }

        private async void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           

             ComboBoxItem typeItem = (ComboBoxItem)AddFilters.SelectedItem;
            var value = typeItem.Content;
            await Task.Run(() => {
                if (bitmap != null)
            {
                switch (value)
                {
                    case "Sepia":

                        Bitmap bmpSepia = Filtros.Sepia(bitmap);
                        bitmap = bmpSepia;
                        this.Dispatcher.Invoke(() =>
                        {
                            ImageEdit.Source = Helpers.Convert(bmpSepia);
                        });
                        ShowHistograma();
                        AddItemList("Sepia");
                        break;

                    case "Glitch":
                        Bitmap bmpGlitch = Filtros.Glitch(bitmap);
                        bitmap = bmpGlitch;
                        this.Dispatcher.Invoke(() =>
                        {
                            ImageEdit.Source = Helpers.Convert(bmpGlitch);
                        });
                        ShowHistograma();
                        AddItemList("Glitch");
                        break;

                    case "Escala de grises":
                        Bitmap bmpEdG = Filtros.EscalaDeGrises(bitmap);
                        bitmap = bmpEdG;
                        this.Dispatcher.Invoke(() =>
                        {
                            ImageEdit.Source = Helpers.Convert(bmpEdG);
                        });
                        ShowHistograma();
                        AddItemList("Escala de grises");
                        break;

                    case "Sobel":
                        Bitmap bmpSobel = Filtros.Sobel(bitmap);
                        bitmap = bmpSobel;
                        this.Dispatcher.Invoke(() =>
                        {
                            ImageEdit.Source = Helpers.Convert(bmpSobel);
                        });
                        ShowHistograma();
                        AddItemList("Sobel");
                        break;

                    case "Laplaciano":
                        Bitmap bmpLaplace = Filtros.Laplaciano(bitmap);
                        bitmap = bmpLaplace;
                        this.Dispatcher.Invoke(() =>
                        {
                            ImageEdit.Source = Helpers.Convert(bmpLaplace);
                        });
                        ShowHistograma();
                        AddItemList("Laplaciano");
                        break;
                }
            }
            else
            {

                switch (value)
                {
                    case "Sepia":

                        AddItemList("Sepia");
                            FiltroVideo = EnumFiltros.Sepia;
                        break;

                    case "Glitch":
                      
                        AddItemList("Glitch");
                            FiltroVideo = EnumFiltros.Glitch;
                            break;

                    case "Escala de grises":
                            FiltroVideo = EnumFiltros.EscalaDeGrises;
                            AddItemList("Escala de grises");
                        break;

                    case "Sobel":
                            FiltroVideo = EnumFiltros.Sobel;
                            AddItemList("Sobel");
                        break;

                    case "Laplaciano":
                            FiltroVideo = EnumFiltros.Laplaciano;
                            AddItemList("Laplaciano");
                        break;
                }

            }
            });
            AddFilters.SelectedIndex = 0;
        
        }


        private  async void ShowHistograma(Bitmap BitVideo = null)
        {
            await Task.Run(() => {
                Mat imga;
                if (bitmap != null)
                    imga = bitmap.ToMat();
                else
                    imga = BitVideo.ToMat();

                Image<Bgr, Byte> imgHist = imga.ToImage<Bgr, Byte>(); // imga.ToImage<Bgr, Byte>().ToBitmap();

                Image<Gray, Byte> imgBlue = imgHist[0];
                Image<Gray, Byte> imgGreen = imgHist[1];
                Image<Gray, Byte> imgRed = imgHist[2];


                DenseHistogram histR = new DenseHistogram(256, new RangeF(0, 255));
                histR.Calculate(new Image<Gray, byte>[] { imgRed }, false, null);
                Mat mR = new Mat();
                histR.CopyTo(mR);
                histogramR.ClearHistogram();
                Mat mHistR = histogramR.GenerateHistogram("RED", System.Drawing.Color.Red, mR, 256, new float[] { 0, 256 });
                Dispatcher.Invoke(() =>
                {
                    imageHistR.Source = Helpers.Convert(mHistR.ToImage<Bgr, Byte>().ToBitmap());
                });

                DenseHistogram histG = new DenseHistogram(256, new RangeF(0, 255));
                histG.Calculate(new Image<Gray, byte>[] { imgGreen }, false, null);
                Mat mG = new Mat();
                histG.CopyTo(mG);
                histogramG.ClearHistogram();
                Mat mHistG = histogramR.GenerateHistogram("GREEN", System.Drawing.Color.Green, mG, 256, new float[] { 0, 256 });
                Dispatcher.Invoke(() =>
                {
                    imageHistG.Source = Helpers.Convert(mHistG.ToImage<Bgr, Byte>().ToBitmap());
                });

                DenseHistogram histB = new DenseHistogram(256, new RangeF(0, 255));
                histB.Calculate(new Image<Gray, byte>[] { imgBlue }, false, null);
                Mat mB = new Mat();
                histB.CopyTo(mB);
                histogramB.ClearHistogram();
                Mat mHistB = histogramR.GenerateHistogram("BLUE", System.Drawing.Color.Blue, mB, 256, new float[] { 0, 256 });
                Dispatcher.Invoke(() =>
                {
                    imageHistB.Source = Helpers.Convert(mHistB.ToImage<Bgr, Byte>().ToBitmap());
                });
            });
        }

        private  void Clean_Click(object sender, RoutedEventArgs e)
        {
            if (bitmap != null)
            {
                bitmap = (Bitmap)bitmapClean.Clone();
                ImageEdit.Source = Helpers.Convert(bitmap);
                ShowHistograma();
                listFilters.Items.Clear();
            }
            else
            {
                listBitamp = new List<Bitmap>();
                FiltroVideo = EnumFiltros.None;
                listFilters.Items.Clear();
            }
        }

        private void AddItemList(String filter)
        {
            Dispatcher.Invoke(() =>
            {
                System.Windows.Controls.ListViewItem item = new System.Windows.Controls.ListViewItem { Background = (System.Windows.Media.Brush)Resources["Primary"] };
                StackPanel stp = new StackPanel { Orientation = System.Windows.Controls.Orientation.Horizontal };
                System.Windows.Controls.Image img = new System.Windows.Controls.Image { Source = Helpers.Convert(bitmap), Width = 40, Height = 40 };
                TextBlock txt = new TextBlock { Text = filter, Foreground = (System.Windows.Media.Brush)Resources["Font2"], Margin = new Thickness { Top = 0, Bottom = 0, Right = 5, Left = 5 }, VerticalAlignment = VerticalAlignment.Center };
                stp.Children.Add(img);
                stp.Children.Add(txt);
                item.Content = stp;
                listFilters.Items.Add(item);
            });
        }

     
     
        

    }
}
