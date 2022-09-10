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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FoxLife.View.Pages
{
    /// <summary>
    /// Логика взаимодействия для Forum.xaml
    /// </summary>
    public partial class Topic : Page
    {
        public Topic()
        {
            InitializeComponent();
        }

        private void MoveCarriageToEnd(object sender, RoutedEventArgs e)
        {
            Task.Run(async () =>
            {
                await Task.Delay(1);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.SelectionStart = MessageBox.Text.Length;
                });
            });
            
        }

        private void PreventMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}
