using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using DevExpress.Mvvm.POCO;
using FoxLife.ViewModels;

namespace FoxLife.View
{ 
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();
        }

        //dragging window
        private void WindowMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        //for no storing back frames
        void DeleteLastFrame(object sender, NavigationEventArgs e)
        {
            var temp = sender as Frame;
            while (temp.NavigationService.CanGoBack)
            {
                temp.NavigationService.RemoveBackEntry();
            }
        }

        private void EventSetter_OnHandler(object sender, RoutedEventArgs e)
        {
            return;
            //throw new NotImplementedException();
        }
    }
}
