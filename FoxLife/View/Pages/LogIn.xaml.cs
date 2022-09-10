using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FoxLife.ViewModels;

namespace FoxLife.View.Pages
{
    public partial class LogIn : Page
    {
        public LogIn()
        {
            InitializeComponent();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null)
            { ((dynamic)this.DataContext).Password = ((PasswordBox)sender).SecurePassword; }
        }
    }
}
