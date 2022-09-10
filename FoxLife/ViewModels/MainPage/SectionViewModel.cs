using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using FoxLife.Models.DBInfo.SectionInfo;
using Microsoft.Toolkit.Mvvm.Input;
using DevExpress.Mvvm;
using FoxLife.Models.DBInfo.Forum;

namespace FoxLife.ViewModels.MainPage
{
    internal class SectionViewModel : ViewModelBase
    {
        public SectionDb SectionDb { get; }
        public ObservableCollection<ForumViewModel> Topics { get; set; }

        public int Id => SectionDb.Id;
        public string Name => SectionDb.Name;

        public SectionViewModel(SectionDb sectionDb)
        {
            SectionDb = sectionDb;
            Topics = new ObservableCollection<ForumViewModel>(sectionDb.Forums.Select(t => new ForumViewModel(t)));
        }

        public RelayCommand AddForum => new(() =>
        {
            MainViewModel.ChangePage(MainViewModel.PagesEnum.ForumCreate,Id);
        });
    }
}
