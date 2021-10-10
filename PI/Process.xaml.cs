using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PI
{
    /// <summary>
    /// Interaction logic for Process.xaml
    /// </summary>
    public partial class Process : Window
    {
        public Process()
        {
            InitializeComponent();
        }
        public void ChangueValue(Double val)
        {
            Dispatcher.Invoke(() => {
                progressBar.Value = val;
            });
        }

        public void CloseModal()
        {
            Dispatcher.Invoke(() =>
            {
                this.Close();
            });
        }
    }
}
