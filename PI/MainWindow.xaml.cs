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

namespace PI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        BitmapImage play = new BitmapImage(new Uri("./image/play.png", UriKind.Relative));
        BitmapImage pause = new BitmapImage(new Uri("./image/stop.png", UriKind.Relative));

        Bitmap bitmap;

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
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        private void ButtonMax_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow.WindowState == WindowState.Normal)
                Application.Current.MainWindow.WindowState = WindowState.Maximized;
            else
                Application.Current.MainWindow.WindowState = WindowState.Normal;
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
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
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = "c:\\";
            dlg.Filter = "Files (*.jpg;*.PNG;*.mp4)|*.jpg;*.PNG;*.mp4|All Files (*.*)|*.*";
            dlg.RestoreDirectory = true;
            bool? success = dlg.ShowDialog();
            
            if (success == true)
            {
                string selectedFileName = dlg.FileName;
                string ext = System.IO.Path.GetExtension(dlg.FileName); 
                if(ext == ".mp4")
                {
                    HandlerControlers(Visibility.Visible);
                    MediaEdit.Source = new Uri(selectedFileName);
                    MediaEdit.Play();
                }
                else
                {
                    bitmap = new Bitmap(selectedFileName);
                     BitmapImage bitmapi = new BitmapImage();
                    bitmapi.BeginInit();
                    bitmapi.UriSource = new Uri(selectedFileName);
                    bitmapi.EndInit();
                    ImageEdit.Visibility = Visibility.Visible;
                    ImageEdit.Source = bitmapi;
                }
                
            } 
        }

        private void SaveClick(object sender,RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "Files (*.jpg;*.PNG;*.mp4)|*.jpg;*.PNG;*.mp4|All Files (*.*)|*.*";
            
            dlg.RestoreDirectory = false;
            bool? success = dlg.ShowDialog();
            if (success == true)
            { 
                //bitmap.Save(dlg.FileName, ImageFormat.Png);

            }

        }

        private void SeekToMediaPosition(object sender, RoutedPropertyChangedEventArgs<double> args)
        {
            int SliderValue = (int)TimeLine.Value;
            TimeSpan ts = new TimeSpan(0, 0, 0, 0, SliderValue);
            MediaEdit.Position = ts;
           
        }

       private void Stop_Click(object sender, RoutedEventArgs e)
        {
            var state = GetMediaState(MediaEdit);
            if ( state == MediaState.Pause )
            {
                MediaEdit.Play();
                BtnStopPlay.Source = pause;
            }
            else
            {
                MediaEdit.Pause();
                BtnStopPlay.Source = play;
            }
        }

        private MediaState GetMediaState(MediaElement myMedia)
        {
            FieldInfo hlp = typeof(MediaElement).GetField("_helper", BindingFlags.NonPublic | BindingFlags.Instance);
            object helperObject = hlp.GetValue(myMedia);
            FieldInfo stateField = helperObject.GetType().GetField("_currentState", BindingFlags.NonPublic | BindingFlags.Instance);
            MediaState state = (MediaState)stateField.GetValue(helperObject);
            return state;
        }


        private void Reload_Click(object sender, RoutedEventArgs e)
        {
            BtnStopPlay.Source = pause;
            MediaEdit.Stop();
            MediaEdit.Play();
        }

        private void HandlerControlers( Visibility visibility)
        {
            TimeLine.Visibility = visibility;
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

                    ImageEdit.Source = Helpers.Convert(bmpSepia);
                   
                    break;

                case "Glitch":
                    filtros.Glitch();
                    break;

                case "Escala de grises":
                    filtros.EscalaDeGrises();
                    break;

                case "Sobel":
                    filtros.Sobel();
                    break;

                case "Laplaciano":
                    filtros.Laplaciano();
                    break;
            }
        }

      


    }
}
