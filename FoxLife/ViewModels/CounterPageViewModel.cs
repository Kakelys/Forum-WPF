using System.Windows.Media;
using DevExpress.Mvvm;
using FoxLife.ViewModels.ForumPage;
using Microsoft.Toolkit.Mvvm.Input;

namespace FoxLife.ViewModels
{
    internal class CounterPageViewModel:ViewModelBase
    {
        public int PageNumb { get; set; }
        public Brush? Background { get; set; }

        public CounterPageViewModel(int pageNumb, int currentPage)
        {
            PageNumb = pageNumb;
            if (PageNumb == currentPage)
            {
                Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#404040");
                Background.Freeze();
            }
            else
            {
                Background = (SolidColorBrush) new BrushConverter().ConvertFromString("#323232");
                Background.Freeze();
            }
        }
    }
}
