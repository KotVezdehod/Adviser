using System;
using System.Collections.Generic;
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

namespace Adviser
{
    /// <summary>
    /// Логика взаимодействия для PopUp.xaml
    /// </summary>
    public partial class PopUp : Window
    {
        bool active = false;

        public PopUp()
        {
            InitializeComponent();

        }

        private void Window_Activated(object sender, EventArgs e)
        {
            if (active) return;

            Task.Factory.StartNew(() =>
            {
                Double op = 1;

                active = true;
                while (op > 0)
                {
                    Application.Current.Dispatcher.Invoke(() => { Opacity = op; });

                    op = op - 0.01;
                    Thread.Sleep(20);
                }

                Application.Current.Dispatcher.Invoke(() => { Hide(); });

                active = false;
            });
            
        }
    }

}
